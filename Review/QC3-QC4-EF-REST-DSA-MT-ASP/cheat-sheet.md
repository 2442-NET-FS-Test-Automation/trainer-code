# QC-3 + QC-4 — Cheat Sheet

Dense quick-reference for the combined sitting. Syntax and comparison tables per section, sourced from
the content notes, the demo answer keys (`library-fulfillment`, `algorithms-threading-demo`), and the
rubric example columns. Skim the morning of the exam.

---

## 1. REST & HTTP

**HTTP verbs** *(source: `content/02-rest-http/http-fundamentals.md`)*

| Verb | Meaning | Typical success code |
|---|---|---|
| GET | read a resource | 200 OK |
| POST | create a resource | 201 Created (+ `Location` header) |
| PUT | replace a resource | 200 / 204 |
| PATCH | partial update | 200 / 204 |
| DELETE | remove a resource | 204 No Content |

**Status code classes** *(source: `content/02-rest-http/http-fundamentals.md`)*

| Class | Meaning | Ones the demo API actually returns |
|---|---|---|
| 1xx | informational | — |
| 2xx | success | 200 OK, 201 Created, 202 Accepted, 204 No Content |
| 3xx | redirection | — |
| 4xx | client error | 400 validation fail, 401 no/bad token, 403 wrong role, 404 not found, 409 duplicate |
| 5xx | server error | 500 unhandled, 503 maintenance middleware |

**REST principles** *(source: `content/02-rest-http/rest-principles.md`)*: client-server, **stateless**,
cacheable, uniform interface, layered system, code-on-demand (optional).

**URL conventions**: plural nouns, nest for relationships — `/api/inventory`, `/api/inventory/{sku}`,
`/users/1/orders`. No verbs in URLs (the HTTP method is the verb).

**curl** *(source: `demo/walkthroughs/06b-layers-rest-middleware.md` live beats)*:

```bash
# GET
curl http://localhost:5137/api/inventory
# POST with a JSON body
curl -X POST http://localhost:5137/api/inventory \
  -H "Content-Type: application/json" \
  -d '{"sku":"BK-010","name":"Domain-Driven Design","price":54.99,"currentStock":4}'
```

**AuthN vs AuthZ** *(source: `content/08-security/authentication-jwt.md`)*: authentication = who you are
(login -> 401 when missing/invalid); authorization = what you may do (role/policy -> 403 when
insufficient). Pipeline order: `UseAuthentication()` **before** `UseAuthorization()`.

**SOA sketch** *(source: `content/02-rest-http/rest-principles.md`)*:
`Client -> API Gateway -> [Auth Service | Order Service | Inventory Service] -> each service's own DB`
— loosely coupled services over a network, each independently deployable.

**REST vs SOAP** *(source: `content/07-soap/soap-vs-rest.md`)*

| | REST | SOAP |
|---|---|---|
| Payload | JSON (usually) over HTTP | XML envelope (Envelope/Header/Body) |
| Contract | convention + docs (OpenAPI) | strict WSDL |
| Verbs | HTTP methods | always HTTP POST + SOAPAction |
| Caching/scale | HTTP-native, lightweight | heavier, stateful standards (WS-*) |
| Use | public web APIs | formal enterprise/legacy integration |

---

## 2. DSA

**Big-O ladder** *(source: `content/03-dsa/big-o-complexity.md`)*:
O(1) < O(log n) < O(n) < O(n log n) < O(n^2). Worst-case growth vs input size; drop constants and
lower-order terms.

**Structure operations** *(source: `content/03-dsa/collections-adts.md`)*

| Structure | Lookup | Insert | Delete | C# type |
|---|---|---|---|---|
| Array / `List<T>` | O(1) by index | O(n) (shift) / amortized O(1) append | O(n) | `T[]`, `List<T>` |
| Linked list | O(n) | O(1) at known node | O(1) at known node | `LinkedList<T>` |
| Hash table | O(1) avg | O(1) avg | O(1) avg | `Dictionary<K,V>`, `HashSet<T>` |
| Stack (LIFO) | Peek O(1) | Push O(1) | Pop O(1) | `Stack<T>` |
| Queue (FIFO) | Peek O(1) | Enqueue O(1) | Dequeue O(1) | `Queue<T>` |
| Priority queue | Peek O(1) | O(log n) | O(log n) | `PriorityQueue<TElement,TPriority>` |

**Choosing**: fast lookup -> hash table; ordered FIFO processing -> queue; undo/backtracking -> stack;
serve-by-priority -> priority queue; sorted data + search -> binary search.

**Search** *(source: `demo/algorithms-threading-demo/DsaThreading/Searches.cs`)*

| | Linear | Binary |
|---|---|---|
| Cost | O(n) | O(log n) |
| Precondition | none | data **must be sorted** |
| Shape | one `for` loop, compare each | `low/high/mid`, halve the range |

```csharp
for (int i = 0; i < data.Length; i++)          // linear
    if (data[i] == target) return i;

int mid = low + (high - low) / 2;              // binary midpoint (overflow-safe)
if (sorted[mid] < target) low = mid + 1; else high = mid - 1;
```

**Sorts** *(source: `content/03-dsa/sorting.md`, `demo/algorithms-threading-demo/DsaThreading/Sorts.cs`)*

| Sort | Cost | Recognize by |
|---|---|---|
| Bubble | O(n^2) | nested loops, swap **adjacent** pairs `a[j] > a[j+1]` |
| Insertion | O(n^2) | grow sorted prefix; `while` shifting larger items right |
| Selection | O(n^2) | find **min** of unsorted region, swap into place |
| Merge (Nice) | O(n log n) | recursion: split halves, then merge two sorted arrays |

**Recursion** *(source: `content/03-dsa/recursion-memoization.md`)*: base case stops, recursive case
shrinks toward it — `int Fact(int n) => n == 0 ? 1 : n * Fact(n - 1);`. Memoization = cache results
top-down (`Dictionary<int,long>` before recursing); tabulation = build the table bottom-up iteratively.

**Trees/graphs** *(source: `content/03-dsa/trees-graphs.md`)*: tree = hierarchical, one root, no cycles;
graph = vertices + edges, may be cyclic/directed.

---

## 3. EF Core

**CLI** *(source: `content/01-efcore/code-first-data-first.md`)*

```bash
dotnet ef migrations add Init          # code-first: C# -> migration
dotnet ef database update              # apply migration -> SQL schema
dotnet ef dbcontext scaffold "ConnString" Microsoft.EntityFrameworkCore.SqlServer   # data-first
```

**Code-first vs data-first**: code-first = write classes, generate schema (greenfield); data-first =
scaffold classes from an existing DB (legacy).

**Configuration precedence** *(source: `Library.Data/LibraryDbContext.cs`)*:
**Convention < Data Annotations < Fluent API** (in `OnModelCreating`).

| Convention | Annotation | Fluent API |
|---|---|---|
| `Id` -> PK, nav props -> FKs | `[Key]`, `[Required]`, `[MaxLength(100)]`, `[Table("Customers")]` | composite keys, unique indexes, column types, relationships: `e.HasIndex(p => p.Sku).IsUnique();` |

**DbContext essentials** *(source: `content/01-efcore/efcore-orm-dbcontext.md`)*:
session with the DB; `DbSet<T>` per entity; builds queries; change tracker records entity states
(Added / Modified / Deleted / Unchanged); `SaveChanges()` emits matching INSERT/UPDATE/DELETE.
Registered in DI (`AddDbContextFactory`/`AddDbContext`) — **scoped, not thread-safe, one per unit of
work** (`content/01-efcore/efcore-concurrency.md`).

```csharp
using var db = new AppDbContext(options);
db.Books.Add(book);          // state: Added
db.SaveChanges();            // INSERT executed here
```

**Seeding**: `b.Entity<Product>().HasData(new Product { Id = 1, ... });` — explicit PKs required
(runs inside the migration, before identity values exist).

**Migration safety** *(source: `content/01-efcore/code-first-data-first.md`)*: incremental migrations,
review generated SQL, never drop populated columns without a data-preserving step; a generated
migration's `Up()`/`Down()` can be edited (e.g. `migrationBuilder.Sql(...)`) before `database update`.

**Raw SQL (Nice, notes-only)** *(source: `content/01-efcore/loading-strategies-raw-sql.md`)*:
`db.Books.FromSql($"EXEC GetBooks")` (entities), `db.Database.SqlQuery<int>($"SELECT COUNT(*) ...")`
(scalars).

---

## 4. C# Multithreading

**Creation ladder** *(source: `content/04-multithreading/threads-threadpool-lifecycle.md`,
`parallelism-tpl.md`)*

| Tool | Shape | Use |
|---|---|---|
| `Thread` | `var t = new Thread(Work); t.Start(); t.Join();` | manual, long-lived, full control |
| `ThreadPool` | `ThreadPool.QueueUserWorkItem(_ => Work());` | fire-and-forget on pooled threads |
| TPL `Task` | `await Task.Run(() => Heavy());` | the default: composition, results, exceptions |
| `Parallel` | `Parallel.For(0, 100, i => Process(i));` | data-parallel CPU loops |

**Lifecycle**: Unstarted -> Running -> (WaitSleepJoin) -> Stopped; OS scheduler time-slices runnable
threads. Context switch = save/restore thread state — too many threads = thrashing, lower throughput.

**Concurrency vs parallelism**: concurrency = interleaved progress (even one core); parallelism =
literally simultaneous on multiple cores.

**Synchronization** *(source: `content/04-multithreading/synchronization.md`,
`demo/algorithms-threading-demo/DsaThreading/Bank.cs`)*

```csharp
private readonly object _gate = new();
lock (_gate) { _balance += amount; }   // serializes the read-modify-write
```

| Tool | Cost | Use |
|---|---|---|
| `Interlocked.Increment(ref x)` | cheapest | single atomic op |
| `lock` / `Monitor` | medium | multi-statement critical section (`lock` = Monitor sugar + guaranteed exit) |
| `SemaphoreSlim(n)` | medium | limit concurrent count (also async: `WaitAsync`) |
| `Mutex` | heavy | **cross-process** named lock |
| `ReaderWriterLockSlim` | medium | many readers, one writer |

**Pitfalls**: race = unsynchronized read-modify-write; deadlock = two threads each hold a lock the other
needs (fix: acquire locks in one consistent order); starvation = a thread never scheduled.

**Thread-safe collections**: `ConcurrentDictionary<K,V>` (`TryAdd`, `AddOrUpdate`),
`BlockingCollection<T>` (producer/consumer), `ConcurrentQueue<T>`.

**Cancellation** *(source: `content/04-multithreading/cancellation-exceptions.md`)*

```csharp
var cts = new CancellationTokenSource();
var task = Task.Run(() => Work(cts.Token), cts.Token);
cts.Cancel();   // cooperative: Work must observe token.IsCancellationRequested / ThrowIfCancellationRequested()
```

Cooperative (poll the token, exit cleanly) vs preemptive (`Thread.Abort` — forcibly kills, risks corrupt
state; removed in modern .NET). Task exceptions surface as `AggregateException` / on `await`.

**async/await** *(source: `content/04-multithreading/async-await.md`)*: frees the calling thread during
I/O; `await Task.Run(...)` moves CPU work off the caller. Sequential vs parallel gain is bounded by the
serial portion (Amdahl's law) plus sync overhead.

**Profiling (Nice, notes-only)** *(source: `content/04-multithreading/debugging-profiling.md`)*: VS
Threads window, Parallel Stacks, Concurrency Visualizer.

---

## 5. ASP.NET Core

**Pipeline order** *(source: `Library.ControllerApi/Program.cs` — the taught order)*

```
ExceptionHandlingMiddleware  ->  Swagger  ->  timing middleware (inline app.Use)
->  maintenance short-circuit  ->  UseResponseCaching  ->  UseCors
->  UseAuthentication  ->  UseAuthorization  ->  MapControllers
```

Request flows down the chain, response back up in reverse. A middleware that does not call `next()`
**short-circuits** (the 503 maintenance check). `[Authorize]` failures short-circuit **before** MVC —
that is why a 401 carries no `X-Elapsed-ms` header but a 200 does
(`demo/walkthroughs/07-cross-cutting.md`).

**Minimal API vs controllers** *(source: `content/02-rest-http/minimal-api-hosting.md`,
`content/06-aspnet-core/controllers-actions.md`)*

```csharp
app.MapGet("/inventory", async (IInventoryRepository repo) => await repo.GetAllAsync()); // minimal

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    [HttpGet("{sku}")]
    public async Task<ActionResult<InventoryDto>> GetBySku(string sku) { ... }
}
```

**Method annotations**: `[HttpGet]`, `[HttpGet("{sku}")]`, `[HttpPost]`, `[HttpDelete("{sku}")]` — map
action to verb + route template under the controller's `[Route]` base.

**Returning codes** *(source: `Library.ControllerApi/Controllers/InventoryController.cs`)*:
`Ok(dto)` 200; `CreatedAtAction(nameof(GetBySku), new { sku }, dto)` 201 + Location; `NoContent()` 204;
`BadRequest()` 400 (automatic via `[ApiController]` on ModelState failure); `NotFound()` 404.

**Model binding**: `[FromRoute]`, `[FromQuery]`, `[FromBody]` (complex types default to body on POST),
`[FromHeader]`.

**Validation**: `[Required]`, `[MaxLength]`, `[Range]` on the DTO; `[ApiController]` auto-returns 400.
.NET 10 gotcha: on record DTOs, plain annotations work — `[property:]`-targeted annotations on the
positional record throw `InvalidOperationException` at runtime
(`content/06-aspnet-core/model-binding-validation.md`).

**Service + DTO + AutoMapper** *(source: `content/06-aspnet-core/dtos-service-layer-automapper.md`)*:

```csharp
builder.Services.AddScoped<IInventoryService, InventoryService>();   // API service behind an interface
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
var dto = _mapper.Map<InventoryDto>(item);   // never return entities from actions
```

**Custom filter vs middleware**: middleware = pipeline-wide, sees `HttpContext`
(`app.Use(async (ctx, next) => { ... await next(ctx); ... })`); filter = MVC-only, wraps the **action**
(`o.Filters.Add<TimingFilter>()`). Middleware runs for every request; filters only when routing reached MVC.

**Caching** *(source: `content/06-aspnet-core/caching-consuming-apis.md`)*: `[ResponseCache(Duration = 30)]`
+ `app.UseResponseCaching()` = client/HTTP caching; `IMemoryCache.GetOrCreateAsync("inventory:all", ...)` =
server-side, **evict on writes** (`_cache.Remove("inventory:all")` in `Create`/`Delete`). Gotcha: HTTP
response caching can replay a stale body even after the server-side cache was evicted — the two caches
are independent.

**3rd-party consumption**: typed client —
`builder.Services.AddHttpClient<ISupplierClient, SupplierClient>(c => c.BaseAddress = new Uri("https://dummyjson.com/"));`
injected into the controller like any service.

**JWT (QC-3 REST Nice, delivered)** *(source: `content/08-security/authentication-jwt.md`,
`demo/walkthroughs/09-role-claims.md`)*: issue signed token in `ITokenService`;
`AddAuthentication().AddJwtBearer(...)` validates issuer/audience/key/lifetime; `[Authorize]` = any
authenticated user (401 otherwise), `[Authorize(Roles = "admin")]` = role claim required (403 otherwise).
