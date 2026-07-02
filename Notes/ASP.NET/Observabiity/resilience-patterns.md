# Resilience Patterns: Custom Exceptions, Repository, and Factory

## Learning Objectives
- Design a custom exception that carries data, and catch specific-before-base.
- Put persistence behind a repository interface and say what the seam buys you.
- Use a factory to centralize object construction with validation.
- Defend each pattern choice in one sentence — or recognize it as decoration.

## Why This Matters
These are foundational patterns, met where they earn their keep. A bare `KeyNotFoundException` tells an
operator nothing; `UnknownSkuException` carrying the SKU becomes a clean 400 with the offending value in
the body. The repository seam is what integration tests fake. And the factory is the one place
order-construction rules live, so "reject unknown kinds" is enforced once, not per endpoint. Pattern
questions ("why an interface? why a factory?") are also code-review and interview staples — this note is
your answer sheet.

## The Concept

### Custom exceptions that carry data
An exception type is an API: its *type* says what went wrong, its *properties* say about what:

```csharp
public sealed class UnknownSkuException : Exception
{
    public string Sku { get; }
    public UnknownSkuException(string sku) : base($"Unknown SKU '{sku}'") => Sku = sku;
}

// thrown at the point of knowledge:
public int ResolveProductId(string sku)
{
    try { return _skuToProductId[sku]; }
    catch (KeyNotFoundException) { throw new UnknownSkuException(sku); }   // translate, add data
}
```

The translation matters: `KeyNotFoundException` is an *implementation detail* (there happens to be a
dictionary); `UnknownSkuException` is a *domain fact* (this SKU does not exist). Callers catch meaning,
not mechanism. And the catch site orders clauses **specific before base** — the compiler enforces the
direction, but the design point is that the specific clause produces the better response:

```csharp
try { var order = factory.Create(req.Kind, req.CustomerId, ...); ... return Results.Created(...); } // 201
catch (UnknownSkuException ex)                        // SPECIFIC first -> a 400 with the data
{
    Log.Warning("Rejected order: unknown SKU {Sku}", ex.Sku);
    return Results.BadRequest(new { error = ex.Message, sku = ex.Sku });
}
// anything else escapes to the global handler -> 500 (base case, handled ONCE in middleware)
```

Guidelines: derive from `Exception`; end the name in `Exception`; add properties for every fact the
handler will want; throw where the knowledge is, catch where the *decision* is (see the middleware in
`../06-aspnet-core/aspnet-pipeline-middleware.md` for the base case).

### Repository behind an interface
The repository pattern puts data access behind a seam the rest of the app depends on:

```csharp
public interface IInventoryRepository
{
    Task<IReadOnlyList<InventoryItem>> AllAsync(CancellationToken ct = default);
    Task<InventoryItem?> BySkuAsync(string sku, CancellationToken ct = default);
    Task<InventoryItem> AddAsync(string sku, string name, decimal price, int quantity, CancellationToken ct = default);
    Task<bool> RemoveAsync(string sku, CancellationToken ct = default);
}

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();   // consumers see only the interface
```

The implementation creates one context per operation from the `IDbContextFactory` (the unit-of-work rule
again) and hides every EF detail — `Include`s, cascade behavior on delete — behind method names that speak
the domain. What the seam buys: **substitutability** (tests hand the interface a fake and exercise the
service without a database), **one place for query logic**, and **a smaller surface** for callers to
misuse.

Honest caveat: EF's `DbContext` already *is* a repository/unit-of-work implementation, so wrapping it is
partly ceremonial in small apps. The pattern earns its place when consumers should not know about EF at
all — service layers and test suites are exactly that position.

### Factory: construction rules in one place
When building an object involves decisions — validate the kind, resolve SKUs, set defaults — that logic
belongs in one constructor-owning place, not scattered across call sites:

```csharp
public class OrderFactory
{
    public Order Create(string kind, int customerId, IEnumerable<(string sku, int qty)> lines) => kind switch
    {
        "normal"    => Build(Priority.Normal, customerId, lines),
        "expedited" => Build(Priority.Expedited, customerId, lines),
        _ => throw new ArgumentException($"Unknown order kind '{kind}'")   // reject in the default arm
    };

    private Order Build(Priority priority, int customerId, IEnumerable<(string sku, int qty)> lines) => new()
    {
        CustomerId = customerId, Priority = priority, Status = Status.Pending,
        Lines = lines.Select(l => new OrderLine
        {
            ProductId = _fulfillment.ResolveProductId(l.sku),   // unknown SKU throws HERE, with data
            Quantity  = l.qty
        }).ToList()
    };
}
```

Every order in the system is born valid, `Pending`, with resolved product ids — or not born at all. Note
the composition: the factory *uses* the custom exception (via `ResolveProductId`) and *feeds* the
201/400 endpoint split. The three patterns are one pipeline: factory validates -> exception carries the
failure -> handler maps it to a status code -> the logger records it at Warning.

### Defending the choices
Whether in a README, a design review, or an interview, be ready to answer in one sentence each: what your
custom exception carries and who catches it; what your repository interface hides and what will fake it in
tests; what invariants your factory enforces. If a sentence comes out empty, the pattern is decoration —
cut it or give it a job.

## Say It in an Interview
- *"A custom exception translates mechanism into meaning: `KeyNotFoundException` says 'a dictionary
  missed'; `UnknownSkuException` says 'this SKU does not exist' and carries the SKU so the handler can
  return a useful 400. Throw where the knowledge is; catch where the decision is."*
- *"Catch clauses go specific before base — the specific clause produces the better response, and anything
  unknown escapes to one global handler that maps it to a 500."*
- *"A repository is a domain-named seam over persistence: it buys substitutability for tests, one home for
  query logic, and a narrow surface. I'll also say honestly that `DbContext` is already a repository — the
  wrap earns its keep when consumers shouldn't know EF exists."*
- *"A factory centralizes construction rules — validation, resolution, defaults — so invalid objects are
  never constructed anywhere in the system."*

## Check Yourself
1. Why translate `KeyNotFoundException` into `UnknownSkuException` instead of letting it propagate?
2. What property design rule makes a custom exception useful to its catcher?
3. Name the three things a repository interface buys, and the honest caveat about EF.
4. Where should "reject unknown order kinds" be enforced, and why not in each endpoint?
5. Trace the pipeline from a bad SKU in a request body to the HTTP response and the log line.

**Answers:** (1) The original leaks an implementation detail (a dictionary exists); the custom type states
a domain fact and carries the SKU — callers catch meaning, not mechanism. (2) Carry every fact the handler
will need as properties (the SKU, the id, the limit) — the message alone is prose. (3) Substitutability
(fakes in tests), centralized query logic, smaller misuse surface; caveat: `DbContext` is already a
repository/unit-of-work, so the wrap is for consumers who shouldn't see EF. (4) In the factory's default
switch arm — one place; per-endpoint enforcement drifts and gets skipped. (5) Factory calls
`ResolveProductId` -> `UnknownSkuException(sku)` thrown at the point of knowledge -> endpoint's specific
catch returns `400 BadRequest` with the SKU in the body -> `Log.Warning("Rejected order: unknown SKU
{Sku}", ...)` records it.

## Summary
- Custom exceptions translate mechanism into meaning and carry the data the handler needs; catch
  specific-before-base, decide at the catch site, let the unknown escape to the global 500.
- Repository = domain-named persistence seam: substitutable (tests), centralized (queries), narrow
  (misuse-resistant); acknowledge that EF is one already — the seam is for your consumers.
- Factory = every construction rule in one place; invalid objects are never constructed.
- The three compose into one 201/400 pipeline — and you should be able to say why each exists in one
  sentence.

## Resources
- [Creating and throwing exceptions (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/creating-and-throwing-exceptions)
- [Repository pattern (Microsoft architecture docs)](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Factory Method pattern (Refactoring Guru)](https://refactoring.guru/design-patterns/factory-method)
