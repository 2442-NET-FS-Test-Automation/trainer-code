# Caching and Consuming Third-Party APIs

## Learning Objectives
- Explain response caching vs in-memory caching and implement both.
- Decide what is safe to cache in an inventory domain — and what never is.
- Consume an external HTTP API through a typed client with `IHttpClientFactory`.
- Know where API versioning fits, in one paragraph.

## Why This Matters
These are the cross-cutting concerns that turn a working API into an efficient neighbor. Caching cuts
repeat work — and quietly becomes a correctness bug when applied to the wrong data. The typed HTTP client
makes your service a well-behaved *consumer* of someone else's API, completing the SOA picture where your
service is one node among many: it serves clients and calls suppliers in the same breath.

## The Concept

### Two caches, two altitudes
**Response caching** operates on whole HTTP responses via standard headers — the framework (and any proxy
along the way) can replay a response without your action running:

```csharp
builder.Services.AddResponseCaching();
app.UseResponseCaching();                       // middleware, before the endpoints

[HttpGet]
[ResponseCache(Duration = 30)]                  // emits Cache-Control: max-age=30
public async Task<ActionResult<IEnumerable<InventoryDto>>> Get() => ...;
```

**In-memory caching** (`IMemoryCache`) stores *your* objects server-side, keyed, with expiration — finer
grained, works for any data, invisible to HTTP. Inject it into the controller like any other service and
wrap the hot read:

```csharp
builder.Services.AddMemoryCache();          // Program.cs

// InventoryController: IMemoryCache _cache injected via the constructor
[HttpGet]
[ResponseCache(Duration = 30)]              // layer 2: HTTP
public async Task<ActionResult<IEnumerable<InventoryDto>>> Get()
{
    var dtos = await _cache.GetOrCreateAsync("inventory:all", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
        var items = await _service.AllAsync();      // layer 1: only runs on a cache MISS
        return _mapper.Map<List<InventoryDto>>(items);
    });
    return Ok(dtos);
}
```

`GetOrCreateAsync` is the whole pattern: hit -> return cached; miss -> run the factory once, cache, return.

### What may be cached (the domain judgment)
Caching is a *correctness* decision wearing a performance costume. In an inventory domain a stale product
name is harmless, but a stale `CurrentStock` re-introduces the oversell lie the concurrency machinery
exists to prevent — a cached "5 on hand" served during a burst is wrong the moment the first order
commits. So caching live data is only legal when **every write invalidates**: the demo's `Create` and
`Delete` each run `_cache.Remove("inventory:all")`, so the next read rebuilds fresh. Expiry alone is fine
for slow-changing shapes (catalog names); write-path eviction is the price of caching anything live.
And the layers are independent — evicting `IMemoryCache` does not clear a response the HTTP layer already
stored; that one ages out with `max-age` (or a client sends `Cache-Control: no-cache`).
Checklist before caching anything: How stale is acceptable? Who invalidates it (expiry, or explicit
removal on write)? Is this data per-user (never response-cache authenticated, personalized responses)?
REST's "cacheable" principle (`../02-rest-http/rest-principles.md`) is exactly this, done with headers.

### Consuming a third-party API: IHttpClientFactory + typed client
Your service consumes a supplier's API the way clients will consume yours. The registration pairs an
interface with a preconfigured `HttpClient`:

```csharp
builder.Services.AddHttpClient<ISupplierClient, SupplierClient>(c =>
    c.BaseAddress = new Uri("https://dummyjson.com/"));
```

```csharp
public class SupplierClient : ISupplierClient
{
    private readonly HttpClient _http;                       // injected, factory-managed
    public SupplierClient(HttpClient http) => _http = http;

    public async Task<decimal?> GetListPriceAsync(string sku, CancellationToken ct = default)
    {
        var digits = new string(sku.Where(char.IsDigit).ToArray());   // "BK-001" -> "001"
        if (!int.TryParse(digits, out var id)) return null;           // no supplier match -> caller 404s
        var product = await _http.GetFromJsonAsync<SupplierProduct>($"products/{id}", ct);
        return product?.Price;                                        // unreachable supplier THROWS -> 500
    }
    private record SupplierProduct(int Id, string Title, decimal Price);
}
```

Why the factory instead of `new HttpClient()`: it pools and recycles the underlying handlers, avoiding
both socket exhaustion (a new client per call leaks OS sockets) and stale-DNS (a single eternal client
never re-resolves). The **typed client** pattern adds the seams you already value: consumers depend on
`ISupplierClient`, tests fake it, and the base address/headers live in one registration. Design notes
visible in the code: the token is forwarded; "no match" is a `null` (the controller turns it into 404); an
unreachable supplier *throws*, and the global exception middleware makes that a clean 500 — deliberately
online-or-not-at-all rather than hiding failures behind a fake fallback.

### Versioning, the one paragraph
When a front end depends on your contract, breaking changes need a *road*, and that is API versioning:
publish `/api/v1/inventory`, introduce `/api/v2/...` alongside it (URL-segment versioning is the most
common; header and query-string variants exist — `Asp.Versioning.*` packages wire any of them), migrate
consumers, retire v1 on a schedule. Know the shape and the reason — DTO discipline
(`dtos-service-layer-automapper.md`) is what makes versioning *possible*, because the version is a
contract of DTOs, not of entities.

## Say It in an Interview
- *"Response caching replays whole HTTP responses via `Cache-Control` headers without running the action;
  in-memory caching stores keyed objects server-side with expiration — `GetOrCreateAsync` is the whole
  pattern."*
- *"Caching is a correctness decision: live stock may only be cached because every write evicts the key —
  a cached count without invalidation re-introduces the oversell bug. My checklist: acceptable staleness,
  invalidation owner, and never response-caching personalized data."*
- *"`new HttpClient()` per call exhausts sockets; one eternal client caches DNS forever.
  `IHttpClientFactory` pools handlers and solves both, and the typed-client pattern gives me an interface
  seam to fake in tests."*
- *"Versioning runs contracts in parallel — `/v1` beside `/v2` — and it's DTO discipline that makes that
  possible, because a version is a contract of DTOs, not entities."*

## Check Yourself
1. `[ResponseCache(Duration = 30)]` vs `IMemoryCache.GetOrCreateAsync` — what layer does each operate at,
   and who can serve the cached copy?
2. Why is caching `CurrentStock` *without invalidation* a correctness bug rather than a staleness
   inconvenience — and what makes it safe in the demo?
3. Name the two failure modes of managing `HttpClient` yourself, and what solves both.
4. In the supplier client, why does "no match" return `null` while "supplier unreachable" throws?
5. What three questions do you ask before caching any piece of data?
6. Why do DTOs make API versioning feasible?

**Answers:** (1) Response caching: the HTTP layer — the framework or any intermediary proxy can replay it;
memory cache: inside your process — only your code reads it. (2) Stock is the value concurrency control
protects; serving a cached count during concurrent writes reasserts the stale read the RowVersion
machinery eliminated — oversell returns. It's safe in the demo because every write path (`Create`,
`Delete`) evicts the key, so no read outlives the data it describes. (3) Socket exhaustion (client per call) and stale DNS (one
eternal client); `IHttpClientFactory` handler pooling. (4) No match is a *data outcome* the caller maps to
404; unreachable is a *dependency failure* — throwing lets the global handler produce an honest 500
instead of masking the outage. (5) How stale is acceptable? Who invalidates it? Is it per-user/
authenticated? (6) The version is a public contract; DTOs decouple that contract from entities, so v1 and
v2 shapes can coexist over one model.

## Summary
- Response caching = whole responses via `Cache-Control` (middleware + `[ResponseCache]`); memory caching
  = keyed server-side objects (`GetOrCreateAsync` + expiration).
- Cache by domain judgment: expiry alone suits slow-changing shapes; live data demands write-path
  eviction (`_cache.Remove` on every mutation) — staleness and invalidation are part of the design, not
  afterthoughts.
- `IHttpClientFactory` typed clients: pooled handlers, one configuration point, an interface seam;
  null for no-match, throw (-> global 500) for a dead dependency.
- Versioning = parallel contracts (`/v1`, `/v2`) enabled by DTO discipline; know why even before you build
  one.

## Resources
- [Response caching in ASP.NET Core (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/response)
- [Cache in-memory in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory)
- [Use IHttpClientFactory to implement resilient HTTP requests](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
