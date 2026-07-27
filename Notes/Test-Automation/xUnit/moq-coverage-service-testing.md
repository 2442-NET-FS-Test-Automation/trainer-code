# Mocking with Moq, Service-Layer Testing, and Code Coverage

## Learning Objectives
- Explain why unit tests isolate dependencies, and what determinism and speed have to do with it.
- Place dummy, stub, fake, mock, and spy in the test-doubles taxonomy at one-line depth.
- Use Moq's core API: `Setup`/`Returns`/`ReturnsAsync`, argument matchers, `.Object`, `Verify`, and
  throwing setups.
- State why Moq needs interfaces or virtual members, and what to do about sealed/static dependencies.
- Test a service class: happy path, failure path, and interaction verification.
- Collect coverage with coverlet, read line vs branch coverage, and argue why coverage is a signal, not a
  target.

## Why This Matters
The service layer is where business rules live — may this member check out this book, what happens when
stock hits zero — and it is the layer worth unit testing hardest. But services depend on repositories,
clocks, and external APIs, and a test that touches a real database is slow, order-dependent, and fails for
reasons unrelated to the rule under test. Mocking lets you replace those dependencies with programmable
stand-ins so each test checks one rule, deterministically, in milliseconds. Moq is the most widely used
mocking library in .NET, and "mock the repository, test the service" is both the daily-work pattern and a
near-guaranteed interview exercise — as is the follow-up about what your coverage number actually proves.

## The Concept

### Why isolate dependencies
A *unit* test should test *one unit*: if `CheckoutService` calls a real database, a red test could mean a
broken rule, a down database, or leftover data from the last run. Isolation buys **determinism** (the
dependency returns exactly what you scripted, every run), **speed** (no I/O — thousands of tests in
seconds, so they run per-commit), and **defect localization** (the only real code is the service, so a
failure indicts the service). The trade-off to volunteer: an isolated test proves the service's logic
against a *scripted* dependency, not that the real wiring works — that is what a smaller number of
integration tests are for.

### The test-doubles taxonomy
"Test double" is the umbrella term (from a stunt double). The classic five, one line each:

- **Dummy** — passed to satisfy a parameter list, never used (a `null` logger).
- **Stub** — returns canned answers; state fed *into* the test ("`GetStock` returns 3").
- **Fake** — a working lightweight implementation (an in-memory repository over a `Dictionary`).
- **Mock** — pre-programmed with expectations about *calls it should receive*; the test verifies the
  interaction happened ("`Save` was called exactly once").
- **Spy** — records the calls made to it for later inspection; a mock you interrogate afterward.

The interview-grade distinction: **stubs support state verification** (assert on returned values/final
state); **mocks support behavior verification** (assert the right calls happened). Moq's `Mock<T>` plays
both roles — `Setup` makes it a stub, `Verify` makes it a mock — which is why the vocabulary blurs.

### Moq essentials
Moq builds a runtime implementation of an interface that you script per test. It is one package added to
the test project — the xUnit template ships no mocking library:

```bash
dotnet add YourProject.Tests package Moq
```

Version 4.20 is the current line. (The coverage tooling below needs nothing extra: the xUnit template
already references `coverlet.collector`.)

```csharp
using Moq;

var repo = new Mock<IInventoryRepository>();

// Stub return values; matchers constrain which arguments qualify
repo.Setup(r => r.GetStock("clean-code")).Returns(3);
repo.Setup(r => r.GetStockAsync(It.IsAny<string>())).ReturnsAsync(3);
repo.Setup(r => r.GetStock(It.Is<string>(id => id.StartsWith("ref")))).Returns(0);

// Throwing setup — script the failure path
repo.Setup(r => r.GetStock("missing-isbn")).Throws(new KeyNotFoundException());

// .Object is the actual IInventoryRepository instance you inject
var service = new CheckoutService(repo.Object);

// Behavior verification — did the service make the call, the right number of times?
repo.Verify(r => r.Save(It.IsAny<CheckoutRecord>()), Times.Once);
```

`It.IsAny<T>()` matches anything; `It.Is<T>(predicate)` matches conditionally — prefer the tightest
matcher that still expresses the rule, because an over-loose `IsAny` lets a wrong-argument bug pass.
`Times` (`Once`, `Never`, `Exactly(n)`, `AtLeastOnce`) makes interaction assertions precise. The cost to
volunteer: `Setup`/`Verify` couple the test to the *conversation* between service and dependency, so
refactoring that conversation breaks tests even when behavior is intact — verify only interactions that
*are* the requirement (a save, a notification), not every call.

### Why interfaces and virtual members — and the sealed/static problem
Moq generates, at runtime, a subclass or interface implementation whose members it can intercept — so it
works only on **interfaces** and **virtual/abstract members**. It cannot override **sealed** classes,
**static** methods, or non-virtual members: `DateTime.Now`, `File.ReadAllText`, or a sealed SDK client
cannot be mocked directly. The adjacent answer interviewers want: the **wrapper (adapter) pattern** — put
an interface you own in front of the unmockable thing (`IClock.UtcNow`, `IFileStore.Read`), depend on the
interface, mock it freely, and leave the trivial pass-through to integration tests. This is why DI-heavy
codebases inject interfaces everywhere: testability is a design property, not a tooling trick.

### The service-layer pattern, worked
The system under test — a checkout service with an injected repository:

```csharp
public interface IInventoryRepository
{
    int GetStock(string isbn);
    void Save(CheckoutRecord record);
}

public record CheckoutRecord(string Isbn, string MemberId, DateTime CheckedOutUtc);

public class OutOfStockException(string isbn) : Exception($"No copies of {isbn} available.");

public class CheckoutService(IInventoryRepository repository)
{
    public CheckoutRecord Checkout(string isbn, string memberId)
    {
        if (repository.GetStock(isbn) <= 0)
            throw new OutOfStockException(isbn);

        var record = new CheckoutRecord(isbn, memberId, DateTime.UtcNow);
        repository.Save(record);
        return record;
    }
}
```

The tests — happy path (state + interaction) and failure path (exception + *no* save):

```csharp
using FluentAssertions;
using Moq;
using Xunit;

public class CheckoutServiceTests
{
    private readonly Mock<IInventoryRepository> _repo = new();
    private readonly CheckoutService _service;

    public CheckoutServiceTests() => _service = new CheckoutService(_repo.Object);

    [Fact]
    public void Checkout_BookInStock_SavesAndReturnsRecord()
    {
        _repo.Setup(r => r.GetStock("clean-code")).Returns(3);

        var record = _service.Checkout("clean-code", "member-42");

        record.Isbn.Should().Be("clean-code");
        record.MemberId.Should().Be("member-42");
        _repo.Verify(r => r.Save(It.Is<CheckoutRecord>(c => c.Isbn == "clean-code")), Times.Once);
    }

    [Fact]
    public void Checkout_BookOutOfStock_ThrowsAndNeverSaves()
    {
        _repo.Setup(r => r.GetStock("refactoring")).Returns(0);

        var act = () => _service.Checkout("refactoring", "member-42");

        act.Should().Throw<OutOfStockException>();
        _repo.Verify(r => r.Save(It.IsAny<CheckoutRecord>()), Times.Never);
    }
}
```

Each assertion earns its place: the happy path checks returned state *and* the one interaction that is a
business requirement (the checkout was persisted); the failure path checks the exception *and* that no
phantom record was saved — the `Times.Never` verify is the assertion juniors forget and interviewers probe
for. No database, no ordering, milliseconds per test.

### Code coverage: collecting it and reading it honestly
Coverage measures which code your tests executed. The xUnit template already includes
**coverlet.collector**, so collection is one flag:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

That drops a `coverage.cobertura.xml` file (Cobertura is the interchange format CI tools parse) under
`TestResults/{guid}/`. To read it as humans: **ReportGenerator**, a dotnet global tool that turns
Cobertura XML into a browsable HTML report — one-line depth is all an interview needs.

Two numbers matter. **Line coverage**: fraction of executable lines run. **Branch coverage**: fraction of
decision outcomes taken — `if (stock <= 0)` has two branches, and a suite that only tests in-stock books
counts the line as covered while never taking the throwing branch. Branch coverage is the stricter,
more honest number.

The trap to name unprompted: **coverage is a signal, not a target.** Coverage proves code *ran*, not that
anything was *asserted* — a test that calls `Checkout` and asserts nothing yields the same 100% as the
real tests above and stays green if the business rule is deleted. Mandated numbers breed assert-free tests
written to move the metric. Use coverage to *find untested regions* — the uncovered failure branch — and
let assertions define quality. The follow-up one level beyond: **mutation testing** (e.g. Stryker.NET)
mutates your code and checks that tests fail, directly measuring assertion strength where coverage cannot.

### Common failures and what they mean
Four failures account for most of the time lost to mocking. The first two are limits of the tool wearing
the costume of a bug in your code; the last two are defects in the test itself.

**`NullReferenceException` raised inside a framework extension method you never called directly.** You
mocked an interface whose useful API is a set of **extension methods**. `IMemoryCache` is the classic
case: `Set`, `Get<T>`, and `GetOrCreateAsync` are all extension methods, while the interface itself
declares just `TryGetValue`, `CreateEntry`, `Remove`, and `GetCurrentStatistics`. Moq can only
intercept members declared *on the
interface*; an extension method is a static call the mock never sees, so it runs against a mock whose
underlying members return nothing useful. Fix: do not mock that type. Use the real lightweight
implementation the library ships (`new MemoryCache(new MemoryCacheOptions())`), and mock only behavior
that is genuinely declared on the interface.

**`NotSupportedException`, on the `Setup` line or on the constructor.** Moq subclasses at runtime, so it
can only intercept interfaces and virtual or abstract members. A non-virtual method fails at setup —
*"Non-overridable members ... may not be used in setup / verification expressions"* — and a sealed class
fails at `new Mock<T>()` — *"Type to mock must be an interface, a delegate, or a non-sealed, non-static
class."* Note that both fail loudly; a mock never silently ignores a `Setup`. Fix: introduce an interface
at that seam, or mark the member `virtual`; if you cannot change the type (a static, sealed, third-party
API), wrap it in a thin adapter interface of your own and mock the adapter.

**A test passes, and it should not have.** The usual cause is `It.IsAny<T>()` where the argument was the
requirement. `repo.Verify(r => r.Save(It.IsAny<Record>()), Times.Once)` stays green when the service
saves the *wrong* record. Tighten to `It.Is<T>(predicate)` any time the argument's value carries meaning.
The sibling case: a `[Theory]` whose assert never uses its parameters — every row passes because the rows
are decorative.

**`Setup` was configured but the mock returns a default.** A loose mock — the default — answers any
unconfigured call with `default(T)`: `0`, `null`, or a completed `Task` wrapping those. So this is always
the same root cause: **the setup expression does not match the call the code actually makes.** A
different overload, an argument the matcher does not accept, a different member than you thought. Set
`MockBehavior.Strict` on the mock temporarily and the diagnosis is immediate — it throws on any
unconfigured call and names the exact invocation that went unmatched. (`ReturnsAsync(x)` and
`Returns(Task.FromResult(x))` are equivalent, so the async form is not the culprit here; what will not
compile is `Returns(x)` on a `Task<T>` member.)

## Say It in an Interview
- *"I isolate dependencies so a unit test is deterministic, fast, and indicts exactly one unit — the mock
  returns scripted values, so a failure means the service's logic broke, not the database."*
- *"Dummy fills a parameter, stub returns canned answers, fake is a real lightweight implementation, mock
  verifies expected calls, spy records calls — stubs support state verification, mocks behavior
  verification, and Moq's `Mock<T>` does both."*
- *"With Moq I `Setup(...).Returns(...)` or `ReturnsAsync`, constrain arguments with `It.IsAny` or
  `It.Is(predicate)`, inject `.Object`, and assert interactions with `Verify(..., Times.Once)` — including
  `Times.Never` on failure paths."*
- *"Moq intercepts interfaces and virtual members, so sealed or static dependencies can't be mocked
  directly — I wrap them behind an interface I own, like `IClock`, and mock the wrapper."*
- *"For a service I test the happy path with state asserts plus a Verify that the save happened, and the
  failure path with the expected exception plus `Times.Never` on the save."*
- *"Coverage is `dotnet test --collect:\"XPlat Code Coverage\"` — coverlet emits Cobertura XML,
  ReportGenerator renders it. Branch coverage is stricter than line coverage, and either is a signal, not
  a target: 100% with no asserts proves nothing — mutation testing measures assertion strength."*

## Check Yourself
1. A test of `CheckoutService` against a real database fails only on the second run of the suite. Which
   benefits of isolation would mocking restore, and what does that test *not* prove afterward?
2. In one line each: stub vs mock — and which kind of verification does each support?
3. Write the Moq line asserting the repository's `Save` was never called with any `CheckoutRecord`.
4. Your service calls `DateTime.Now` and a sealed payment SDK client directly. Why can't Moq help as-is,
   and what is the standard fix?
5. A suite reaches 100% line coverage, yet deleting the out-of-stock check keeps everything green. Name
   two distinct gaps that allowed this.

**Answers:** (1) Determinism (scripted returns, no leftover rows) and defect localization (only the
service is real); the mocked test no longer proves the service works against the real database — that
remains integration-test territory. (2) A stub feeds canned data in and supports state verification
(assert on outputs); a mock carries call expectations and supports behavior verification (assert the
interaction happened). (3) `_repo.Verify(r => r.Save(It.IsAny<CheckoutRecord>()), Times.Never);`
(4) `DateTime.Now` is static and the client is sealed — Moq only intercepts interfaces and virtual
members; wrap each behind an interface you own (`IClock`, `IPaymentGateway`) and mock the wrapper.
(5) Weak assertions — tests executed the branch but never asserted the throw or the `Times.Never` save —
and line-only measurement that counted the `if` line without requiring both branch outcomes; mutation
testing exposes exactly this.

## Summary
- Isolating dependencies makes unit tests deterministic, fast, and precise — but only integration tests
  prove the real wiring.
- Taxonomy: dummy (filler), stub (canned answers, state verification), fake (working lightweight
  implementation), mock (call expectations, behavior verification), spy (records calls).
- Moq core: `new Mock<I>()`, `Setup(...).Returns/ReturnsAsync/Throws`, `It.IsAny<T>()` / `It.Is<T>(pred)`,
  inject `.Object`, assert with `Verify(..., Times.X)` — tightest matcher that expresses the rule.
- Moq requires interfaces or virtual members; wrap sealed/static dependencies (`IClock`) behind interfaces
  you own.
- Service pattern: happy path = state asserts + `Verify(..., Times.Once)` on the required interaction;
  failure path = expected exception + `Verify(..., Times.Never)` on the side effect.
- Coverage: `dotnet test --collect:"XPlat Code Coverage"` (coverlet, in the template) -> Cobertura XML ->
  ReportGenerator HTML; branch coverage is stricter than line coverage.
- Coverage is a signal, not a target — full coverage with empty asserts proves nothing; mutation testing
  measures what coverage cannot.

## Resources
- [Moq (github.com/devlooped/moq)](https://github.com/devlooped/moq)
- [Use code coverage for unit testing (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage)
- [Mocks Aren't Stubs (martinfowler.com)](https://martinfowler.com/articles/mocksArentStubs.html)
