# Unit Testing in .NET with xUnit: Facts, Theories, Lifecycle, and Fixtures

## Learning Objectives
- Name the three .NET test frameworks and explain why xUnit is the common modern default.
- Create a test project with `dotnet new xunit`, reference the system under test, and run `dotnet test`.
- Choose between `[Fact]` and `[Theory]` + `[InlineData]`, and structure tests as Arrange-Act-Assert.
- Explain the red-green-refactor cycle of test-driven development, what design pressure it applies, and
  where writing the test first does not pay for itself.
- Pick theory rows with equivalence partitioning and boundary-value analysis instead of by intuition, and
  say where coverage measurement — the white-box check, whose design counterpart is statement/branch/path
  coverage — tells you which rows are missing.
- Assert with the built-in `Assert` API and with FluentAssertions `.Should()` chains, and say why teams
  adopt the latter.
- Describe xUnit's lifecycle: constructor per test, `IDisposable.Dispose` teardown, `ITestOutputHelper`.
- Share expensive setup with `IClassFixture<T>` and `ICollectionFixture<T>`, and name the cost of sharing.

## Why This Matters
Unit tests are the base of the test pyramid, and in .NET that base is overwhelmingly written in xUnit — it
is the template the .NET CLI ships and the framework used to test .NET itself. Writing one passing test is
not the skill; the skill is knowing the machinery: when a test class is constructed, how to parameterize
twenty cases without twenty copies of a method, and how to share a genuinely expensive resource (a database
container, a seeded catalog) without letting tests contaminate each other. Those lifecycle and fixture
questions are exactly where interviews go after the softball "what is a unit test."

## The Concept

### The three frameworks, and why xUnit
All three run under `dotnet test`: **MSTest** (Microsoft's original, `[TestMethod]`, common in older
enterprise code), **NUnit** (a JUnit port, mature, `[Test]`/`[TestCase]`, one shared class instance per
test class), and **xUnit.net** (written by an original NUnit author to fix its design lessons). xUnit is
the modern default because it creates a **new class instance per test** — isolation by construction — and
replaces setup/teardown attributes with the plain constructor and `Dispose`. The concepts transfer across
all three; say that in an interview, then defend your default. One-line adjacent: **TUnit** is an emerging
source-generated framework, but xUnit remains the safe answer.

### Project setup: template, reference, run
A test project is a normal project referencing the code it tests plus the framework packages. The template
wires everything, including the `coverlet.collector` coverage package:

```bash
dotnet new xunit -o LibraryCatalog.Tests
dotnet add LibraryCatalog.Tests reference LibraryCatalog/LibraryCatalog.csproj
dotnet test
```

`dotnet test` builds both projects, discovers every `[Fact]`/`[Theory]`, runs them, and reports results —
the same command CI runs. Convention: name the project `{ProjectUnderTest}.Tests` and methods like
`Checkout_WhenBookOutOfStock_Throws`, so a failure reads as a sentence.

### `[Fact]`, `[Theory]`, and Arrange-Act-Assert
A `[Fact]` takes no parameters — a single case. A `[Theory]` is parameterized: one method, many data rows,
each reported as its own result. Structure every body as **Arrange-Act-Assert**: set up the world, do the
one thing, check the outcome — one behavior per test, so a red test names exactly what broke.

```csharp
using Xunit;

public class LateFeeCalculatorTests
{
    [Fact]
    public void Fee_ReturnedOnTime_IsZero()
    {
        var calculator = new LateFeeCalculator(dailyRate: 0.50m);  // Arrange
        decimal fee = calculator.FeeFor(daysLate: 0);              // Act
        Assert.Equal(0m, fee);                                     // Assert
    }

    [Theory]
    [InlineData(1, 0.50)]
    [InlineData(7, 3.50)]
    [InlineData(30, 15.00)]
    public void Fee_LateReturn_ChargesDailyRate(int daysLate, decimal expected)
    {
        var calculator = new LateFeeCalculator(dailyRate: 0.50m);
        Assert.Equal(expected, calculator.FeeFor(daysLate));
    }
}
```

`[InlineData]` takes compile-time constants only; the adjacent attribute interviewers ask about is
`[MemberData]`, which points at a static member yielding rows for data too complex for attributes. The
trade-off of theories: ideal for pure input/output logic like fee math; if each row needs a *different
arrange*, you have several tests wearing one method's name — split them.

### Test-driven development: writing the test first
Everything above assumes the code exists and you are testing it. **Test-driven development (TDD)** inverts
the order: the test comes first, and it is a *design* practice that happens to leave a suite behind. The
cycle is three steps, deliberately small.

**Red** — write one failing test for behavior that does not exist yet. `LateFeeCalculator` has not been
written, so this does not even compile, and a compile failure counts as red:

```csharp
[Fact]
public void Fee_ReturnedOnTime_IsZero()
{
    var calculator = new LateFeeCalculator(dailyRate: 0.50m);
    Assert.Equal(0m, calculator.FeeFor(daysLate: 0));
}
```

**Green** — write the *simplest* code that passes, even if it is obviously incomplete. `return 0m;` is a
legitimate green here, and that is not cheating: it proves the test can fail and then pass, and the next
red test (`Fee_LateReturn_ChargesDailyRate`) is what forces the real arithmetic.

**Refactor** — with the test green, improve the design. Rename, extract, remove duplication. The test is
your safety net: if the refactor breaks behavior, you know within seconds, and you know it was the
refactor because nothing else changed.

What the cycle buys is mostly not the tests. It is **design pressure**: code that is hard to test first is
usually code with a hidden dependency — a `new` inside a method, a static clock, a database call in a
constructor — so writing the test first pushes you toward constructor-injected seams before the awkward
shape sets. It also guarantees every test has been seen to fail, which a test-after suite cannot claim; an
assertion that never failed may be asserting nothing.

Where it does not fit — say this unprompted, because the honest answer is what separates a practitioner
from someone reciting a slogan. Exploratory spikes where you are still learning what the API should be,
UI layout work whose "correct" is visual, and thin glue code with no logic all pay the cycle's cost without
collecting its benefit. Most teams in practice write tests *after* the code, and that is a defensible
default; what is not defensible is not knowing what the cycle is or why anyone runs it. The adjacent term
interviewers pair with it is **BDD** (behavior-driven development), which keeps the same rhythm but states
each case in stakeholder language — Given/When/Then — so a non-engineer can read the specification.

### Choosing the rows: equivalence partitioning and boundary-value analysis
A theory is only as good as its rows, and "rows I thought of" is not a technique. Exhaustive testing is
impossible — a method taking one `int` has four billion inputs — so two black-box techniques pick a small
set that stands in for all of them.

**Equivalence partitioning** splits the input space into classes whose members should all be treated the
same way, then takes **one representative per class**. If a second value in the same class found a bug the
first missed, they were not really one class. Classes come from how the **system under test** behaves, not
from how the domain talks: if the method never branches on a value, every value of it is one class, however
many names the business has for it. Take a loan-limit rule: a member's total loans may not exceed five, and
the request quantity must be between 1 and 3 books.

| Input | Classes | One representative each |
|---|---|---|
| Total loans after checkout (max 5) | valid: 0-5; invalid: negative; invalid: above 5 | `3`, `-1`, `9` |
| Requested quantity (1-3) | valid: 1-3; invalid: zero or negative; invalid: above 3 | `2`, `0`, `7` |
| Member id | valid existing; invalid nonexistent; invalid malformed/null | `"m-100"`, `"m-999"`, `null` |

Notice the classes are both valid and invalid: the valid partitions are your positive cases, the invalid
partitions your negative ones, and one representative per class is what keeps a suite small without leaving
a behavior unchecked.

**Boundary-value analysis** then adds the rows at the **edges** of each class, because off-by-one errors
(`<` where `<=` belonged, an inclusive limit implemented as exclusive) cluster disproportionately there.
For a rule stated as "at most 5", the boundaries are `4, 5, 6` — the value below the edge, the edge itself,
and the value just past it (the classic *three-value* form; the *two-value* form keeps only the edge and
its immediate outside neighbor). Partitioning alone would have tested `3`, `-1`, and `9` and never noticed
that `5` is rejected. Edges are not only numeric: empty string, maximum length, `null`, an empty
collection, and the first and last day of a range are all boundaries.

Together they make a defensible row set for one theory. `LoanPolicy` is the small domain type holding the
rule, and `IsWithinLimit` answers whether a proposed total is allowed:

```csharp
[Theory]
// Equivalence classes: comfortably inside, comfortably outside, and the invalid negative class.
[InlineData(2, true)]
[InlineData(9, false)]
[InlineData(-1, false)]
// Boundaries of the "at most 5" rule: below, on, just past.
[InlineData(4, true)]
[InlineData(5, true)]
[InlineData(6, false)]
public void IsWithinLimit_ByTotalLoans(int totalLoans, bool expected) =>
    Assert.Equal(expected, new LoanPolicy(maxLoans: 5).IsWithinLimit(totalLoans));
```

Five rows, one method, and every one of them justifiable in a code review — which is the real point:
another engineer can ask "why these inputs?" and get an answer that is not "those felt like enough."

Two adjacent notes interviewers reach for. First, **decision tables** are the technique when the answer
depends on a *combination* of inputs (member in good standing AND stock available AND under the limit)
rather than one value at a time — partitioning per input misses interaction rules. Second, partitioning and
boundaries are **black-box** techniques: they come from the requirement, not the source. The **white-box**
counterpart is choosing rows from the code's own structure — statement, branch, and path coverage — and in
practice you use the black-box set first, then run coverage to see which branches your rows never executed.
Coverage does not tell you what to test; it tells you what you *missed*, and each uncovered branch is a
question: is there a class of input I did not think of, or is this code unreachable?

### Asserting: built-in `Assert` and FluentAssertions
The built-in API covers the essentials: `Assert.Equal(expected, actual)` (expected first — the failure
message labels them), `True/False`, `Null/NotNull`, `Contains`, `Empty`, `IsType<T>`, and
`Assert.Throws<TException>(() => ...)`. **FluentAssertions** (package `FluentAssertions`, here 8.10)
layers a `.Should()` chain on any value. Teams adopt it because assertions read like the sentence you
meant and **failure messages carry real diagnostics** — a collection mismatch names the differing element,
not just "Assert.Equal failed."

```csharp
using FluentAssertions;

var results = catalog.SearchByAuthor("Fowler");   // catalog seeded with Clean Code + Refactoring

results.Should().ContainSingle()
       .Which.Title.Should().Be("Refactoring");
```

Trade-off and adjacent note: it is one more dependency, and as of **v8 FluentAssertions requires a paid
license for commercial use** (free non-commercially) — which is why some teams pin v7 or use alternatives
such as Shouldly or the built-in asserts. Know that before naming it your default.

### Lifecycle: constructor per test, `Dispose` for teardown
xUnit has no `[SetUp]`/`[TearDown]`. It **creates a fresh instance of the test class for every test**: the
constructor is per-test setup, and implementing `IDisposable` makes `Dispose` per-test teardown. Instance
fields cannot leak between tests — isolation is structural. Contrast to state explicitly: **NUnit reuses
one instance** for all tests in a class, so mutable fields there bleed unless reset in `[SetUp]`. And
because tests run in parallel with no per-test console, injected **`ITestOutputHelper`** is the supported
way to write diagnostic output, attached to each test's result:

```csharp
using Xunit;
using Xunit.Abstractions;

public class CheckoutTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly Catalog _catalog = new();     // fresh per test — constructor runs before each test

    public CheckoutTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Checkout_AvailableBook_Succeeds()
    {
        _catalog.Add(new Book("The Pragmatic Programmer", "Hunt & Thomas"));
        var result = _catalog.Checkout("The Pragmatic Programmer");
        _output.WriteLine($"Checkout result: {result.Status}");
        Assert.True(result.Succeeded);
    }

    public void Dispose() => _catalog.Clear();     // per-test teardown, runs after each test
}
```

The cost of per-test construction: anything expensive in the constructor is paid on *every* test — the cue
for fixtures.

### Sharing expensive context: `IClassFixture` and `ICollectionFixture`
When setup is genuinely expensive — a database container, a large seeded catalog — build it once in a
**fixture** class and let xUnit inject it. **`IClassFixture<T>`**: one fixture instance for all tests in
one class, created before the first test and `Dispose`d after the last. **`ICollectionFixture<T>`** on a
`[CollectionDefinition]` class, plus `[Collection("name")]` on each participating class: one instance
shared **across classes** — which also stops those classes running in parallel with each other.

```csharp
using Xunit;

public class SeededCatalogFixture : IDisposable
{
    public Catalog Catalog { get; } = CatalogSeeder.LoadLargeCatalog(); // expensive, runs once
    public void Dispose() { /* release the resource */ }
}

public class InventoryQueryTests(SeededCatalogFixture fixture) : IClassFixture<SeededCatalogFixture>
{
    [Fact]
    public void Inventory_KnownTitle_HasStock() =>
        Assert.True(fixture.Catalog.StockOf("Clean Code") > 0);
}
```

The cost — say it unprompted: a fixture is **shared mutable state**. If one test checks out the last copy
of a shared book, a later test assuming stock breaks, and the failure depends on run order. Keep fixtures
effectively read-only, or give each test its own rows/keys. The follow-up one level beyond: parallelism —
xUnit parallelizes across classes/collections, never within a class, so a collection fixture protects its
shared resource by serializing member classes at the price of throughput.

### Common failures and what they mean
The failures that cost beginners the most time are not assertion failures — they are the runner not
finding, not building, or not showing you things.

**`dotnet test` reports zero tests, or the IDE's test explorer stays empty.** Two causes, in order of
likelihood. The test project is not in the solution, so a solution-level run never reaches it — add it
(`dotnet sln add path/to/Project.Tests`). Or the project is missing `Microsoft.NET.Test.Sdk` and
`xunit.runner.visualstudio`: the template adds both, hand-rolled projects forget them, and without them
there is no discovery mechanism at all. Worth knowing what is *not* the cause, since it is commonly
guessed: method visibility does not affect discovery — a non-public `[Fact]` still runs — and a
non-public test *class* is a build error (`xUnit1000: Test classes must be public`), not a silent
omission.

**`dotnet add reference` fails with "does not exist" for a path you can see on disk.** The path resolves
relative to the **current directory**, not the solution root. From inside a nested test folder, the
project you want is several `..` segments up. Count them from where your shell actually is, or pass both
paths explicitly from the root.

**`Console.WriteLine` output is nowhere.** It is not discarded — it goes to the test process's own
stdout, where two things hide it: the runner cannot attribute it to any individual test (tests run in
parallel with no per-test console), and default verbosity does not print it. Inject `ITestOutputHelper`
and use its `WriteLine`, which attaches output to the individual test result, and add
`--logger "console;verbosity=detailed"` to surface it in terminal output.

**Every row of a `[Theory]` passes, including the one that should fail.** Check that the assert actually
*uses* the parameters. A theory whose body ignores its inputs is one test wearing several names, and it
will stay green through any change to the code it claims to cover.

**A test passes alone and fails in the suite, or fails only sometimes.** This is shared state, and the
usual source is a fixture. `IClassFixture` and `ICollectionFixture` hand every test the *same* instance;
one test that mutates it changes the world a later test assumed, and parallel ordering decides whether
you notice. Keep fixtures read-only, or give each test its own rows and keys.

**`Assert.Equal` fails on values that look identical in the message.** Usually a precision difference —
`0.1 + 0.2` in `double` is `0.30000000000000004`, not `0.3`, so assert money and other exact quantities
in `decimal` (where `0.1m + 0.2m` really does equal `0.3m`) or use the overload taking a precision
argument. The other common case is a collection whose contents match but whose order does not. For
collections, assert on the property you actually care about rather than the whole object, and
prefer an assertion library whose failure output names the differing element.

## Say It in an Interview
- *"The three .NET frameworks are MSTest, NUnit, and xUnit; xUnit is the modern default — a new test-class
  instance per test gives built-in isolation, and it's what .NET itself is tested with. The concepts
  transfer across all three."*
- *"Setup is `dotnet new xunit`, add a project reference to the code under test, and `dotnet test` builds,
  discovers, and runs everything — the same command locally and in CI."*
- *"A `[Fact]` is a single case; a `[Theory]` with `[InlineData]` runs one method over many rows, each
  reported separately — `[MemberData]` covers rows too complex for attributes. Every body is
  Arrange-Act-Assert, one behavior per test."*
- *"TDD is red-green-refactor: write one failing test, write the simplest code that passes, then clean up
  with the test as a safety net. The real payoff is design pressure — code that's hard to test first
  usually has a hidden dependency — plus the guarantee that every test has been seen to fail. I don't run
  it for exploratory spikes or UI layout, where the cycle costs more than it returns."*
- *"I choose theory rows with equivalence partitioning — one representative per class of input that should
  behave alike, valid and invalid — plus boundary values at each edge, because off-by-ones live at the
  edges: for 'at most 5' I test 4, 5, and 6. Those are black-box techniques; I then run coverage as the
  white-box check on which branches my rows never reached."*
- *"Built-in `Assert` covers equality, booleans, collections, and `Assert.Throws`; FluentAssertions adds
  `.Should()` chains with much richer failure messages — though v8 went to a paid commercial license, so
  some teams stay on v7 or alternatives."*
- *"xUnit has no setup/teardown attributes: the constructor runs before every test and `Dispose` after,
  because each test gets a fresh instance — unlike NUnit's shared one. Output goes through
  `ITestOutputHelper`."*
- *"`IClassFixture` builds expensive context once per class and `ICollectionFixture` shares it across
  classes; the cost is shared mutable state and reduced parallelism, so I keep fixtures read-only."*

## Check Yourself
1. Ten tests live in one xUnit class with an expensive operation in the constructor. How many times does
   it run, and what are your two options for running it once?
2. When do you reach for `[Theory]` over separate `[Fact]`s, and when is a theory the wrong call?
3. Why does `Console.WriteLine` fail you in xUnit, and what do you use instead?
4. A colleague proposes FluentAssertions for a commercial product. What benefit and what caveat do you raise?
5. Two classes share an `ICollectionFixture` holding a seeded catalog. One test mutates it and an unrelated
   test now fails intermittently. What happened, and how do you prevent it?
6. A discount applies to orders of 10 items or more. Give the `[InlineData]` rows you would write and name
   the technique behind each one.
7. In TDD, a colleague objects that returning a hard-coded `0m` to make the first test pass is "cheating."
   What is your answer, and what does the cycle actually buy that a test-after suite cannot claim?

**Answers:** (1) Ten times — xUnit constructs a fresh instance per test; run it once via `IClassFixture<T>`
(per class) or `ICollectionFixture<T>` + `[Collection]` (across classes). (2) A theory fits many
input/output rows over the *same* arrange and logic, like fee math; it is wrong when rows need different
setups or assert different behaviors — split into facts. (3) Tests run in parallel with no per-test
console; inject `ITestOutputHelper`, whose `WriteLine` attaches output to the individual test result.
(4) Benefit: readable chains and diagnostic-rich failures; caveat: v8+ requires a paid license for
commercial use, so evaluate v7, Shouldly, or built-in asserts. (5) Shared-mutable-fixture contamination —
the mutation changed state a later test assumed, and ordering made it intermittent; keep the fixture
read-only, give tests their own data, or reset state per test. (6) Equivalence partitioning gives one
representative per class — say `3` (no discount) and `25` (discount); boundary-value analysis adds `9`,
`10`, and `11` around the edge, since "10 or more" is exactly where an off-by-one would hide.
(7) It is not cheating: green means "simplest thing that passes," and the hard-coded value proves the test
can fail and then pass. The next red test forces the real logic. What TDD claims uniquely is that every
test has been *observed* to fail, so none of them is silently asserting nothing — plus the design pressure
of having to construct the type under test before it exists, which surfaces hidden dependencies early.

## Summary
- MSTest, NUnit, xUnit all run under `dotnet test`; xUnit is the modern default with per-test class
  instances for structural isolation (NUnit shares one instance).
- Setup: `dotnet new xunit`, add a project reference, `dotnet test` — the template already includes
  `coverlet.collector`.
- `[Fact]` = single case; `[Theory]` + `[InlineData]` = one method, many reported cases; `[MemberData]`
  for complex rows; every test reads Arrange-Act-Assert.
- TDD = red (failing test first) / green (simplest passing code) / refactor (clean up under the net). Buys
  design pressure toward injectable seams and proof that every test has been seen to fail; skip it for
  spikes, UI layout, and logic-free glue. BDD is the same rhythm stated as Given/When/Then.
- Choose rows deliberately: equivalence partitioning (one representative per class of same-behaving input,
  valid and invalid) plus boundary-value analysis (below / on / just past each edge); decision tables when
  inputs interact. Those are black-box; coverage is the white-box check on what the rows missed.
- Built-in `Assert`: `Equal` (expected first), `True/False`, `Null`, `Contains`, `IsType`, `Throws<T>`.
- FluentAssertions `.Should()` chains buy readability and rich failure output; v8 is paid for commercial
  use — know the licensing before adopting.
- Lifecycle: constructor = per-test setup, `Dispose` = per-test teardown, `ITestOutputHelper` = per-test
  output.
- `IClassFixture<T>` shares expensive context within a class, `ICollectionFixture<T>` across classes — at
  the cost of shared mutable state and cross-class parallelism.

## Resources
- [Getting started with xUnit.net v2 (xunit.net)](https://xunit.net/docs/getting-started/v2/getting-started)
- [Unit testing C# with dotnet test and xUnit (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit)
- [FluentAssertions — Introduction (fluentassertions.com)](https://fluentassertions.com/introduction)
- [Test-Driven Development (martinfowler.com)](https://martinfowler.com/bliki/TestDrivenDevelopment.html)
