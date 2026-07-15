# QC-3 + QC-4 — Study Guide

Organized by topic cluster across the five exam sections (REST, DSA, EF Core, C# Multithreading, ASP.NET
Core). Each cluster lists the objectives it covers (verbatim, tagged with tier), a concept recap
synthesized from the content notes with source pointers, the key points and pitfalls interviewers probe,
and one annotated worked example from the Library demo threads.

Source roots:
- Content notes: `weeklytechrepo/EFCore-REST-SOAP/content/`
- Demo scripts: `weeklytechrepo/EFCore-REST-SOAP/demo/walkthroughs/`
- End-state code (answer keys): `weeklytechrepo/EFCore-REST-SOAP/demo/library-fulfillment/` and
  `weeklytechrepo/EFCore-REST-SOAP/demo/algorithms-threading-demo/DsaThreading/`
- Trainer notes mirror: `trainer-code/Notes/{EFCore,DSA,Multithreading,ASP.NET,SOAP}/`

---

## 1. HTTP & REST Design

**Objectives covered**
- *(Must)* Describe the purpose of REST and RESTful design.
- *(Must)* Describe the purpose of HTTP messaging.
- *(Must)* Describe the HTTP request/response lifecycle.
- *(Must)* Describe HTTP request methods (verbs).
- *(Must)* Describe HTTP response code classes (1xx,2xx,3xx,etc.)
- *(Must)* Describe the REST principles.
- *(Should)* Describe URL conventions when using REST.

**Concept recap** *(sources: `content/02-rest-http/http-fundamentals.md`,
`content/02-rest-http/rest-principles.md`)*
HTTP is the request/response protocol of the web: the client sends a text-based **request** — start line
(method + URI + version), headers, optional body — and the server returns a **response** — status line,
headers, optional body. The verbs carry intent: GET reads, POST creates, PUT replaces, PATCH partially
updates, DELETE removes. Status codes group into classes: 1xx informational, 2xx success, 3xx
redirection, 4xx the client got it wrong, 5xx the server did. **REST** rides on top of this: an
architectural style where **resources** (nouns) are identified by URIs and manipulated through the
uniform verb set, with the six principles — client-server, **stateless** (every request self-contained,
no server-side session), cacheable, uniform interface, layered system, optional code-on-demand. URL
conventions follow: plural nouns (`/users`), identity by segment (`/users/1`), nesting for relationships
(`/users/1/orders`), never verbs in paths.

**Key points / pitfalls**
- "Stateless" trips people: it means no *session* state on the server between requests — the database is
  not "state" in this sense. The token, not a server session, carries identity.
- Know one concrete code per class, and the exam-favorite quartet: 400 (bad input) vs 401 (not
  authenticated) vs 403 (not authorized) vs 404 (no such resource).
- PUT vs PATCH: replace-whole vs partial update. POST is not idempotent; PUT is.

**Worked example** *(cited: `demo/library-fulfillment/Library.ControllerApi/Controllers/InventoryController.cs`)*
The inventory controller's URL surface is the conventions in miniature — one noun, verbs do the work:
`GET /api/inventory` (200), `GET /api/inventory/{sku}` (200 or 404), `POST /api/inventory` (201 +
`Location`), `DELETE /api/inventory/{sku}` (204 or 404).

---

## 2. Consuming and Securing REST Services

**Objectives covered**
- *(Must)* Be capable of sending a GET request to an open source REST API using curl or Postman
- *(Should)* Be capable of sending a POST request to a REST API using curl or Postman and populating the request body
- *(Should)* Describe the difference between authorization and authentication.
- *(Nice)* Implement authentication and authorization using a popular RESTful framework (e.g. OAuth, JWT)
- *(Nice)* Compare and contrast RESTful and SOAP-based web services in terms of functionality, performance, and scalability

**Concept recap** *(sources: `content/02-rest-http/rest-principles.md`,
`content/08-security/authentication-jwt.md`, `content/07-soap/soap-vs-rest.md`)*
curl is the command-line HTTP client: bare `curl <url>` is a GET; `-X POST` picks the verb, `-H` adds
headers, `-d` supplies the body (with `Content-Type: application/json` for JSON APIs). Postman is the
same requests with a UI. **Authentication** establishes who you are (credentials -> in our stack a
signed **JWT**); **authorization** decides what you may do (roles/policies read from the token's
claims). The JWT flow as taught: `POST /auth/login` verifies the hashed password and returns a signed
token; the client sends `Authorization: Bearer <token>`; `AddJwtBearer` validates signature, issuer,
audience, lifetime; `[Authorize]` gates any authenticated user (else 401), `[Authorize(Roles = "admin")]`
gates by role claim (else 403). **SOAP** is the heavyweight contrast: XML envelopes POSTed to a single
endpoint under a strict WSDL contract — better for formal enterprise integration, worse for web-scale
(heavier payloads, no HTTP-native caching).

**Key points / pitfalls**
- The 401-vs-403 pair is the fastest way to prove you understand authn vs authz: 401 = no/invalid
  identity, 403 = valid identity, insufficient rights.
- JWTs are signed, not encrypted: anyone can read the payload; the signature only proves it wasn't
  altered. Never put secrets in claims.
- curl gotcha from the live demos: the port must match the actual binding (`launchSettings.json`) — a
  wrong port fails the whole beat, not just one request.

**Worked example** *(cited: `demo/walkthroughs/09-role-claims.md` run matrix)*
Three requests against the same protected endpoints tell the whole story:

```bash
curl http://localhost:5137/api/inventory/BK-001/supplier-price          # 401 - no token
curl -H "Authorization: Bearer $CONSUMER_TOKEN" \
     -X DELETE http://localhost:5137/api/inventory/BK-001               # 403 - authenticated, wrong role
curl -H "Authorization: Bearer $ADMIN_TOKEN" \
     -X DELETE http://localhost:5137/api/inventory/BK-001               # 204 - admin role claim
```

---

## 3. Big-O & Abstract Data Types

**Objectives covered**
- *(Must)* Describe algorithm complexity using Big O notation and analyze the efficiency of an algorithm or data structure
- *(Must)* Explain the list abstract data type and its common operations
- *(Must)* Explain the stack and queue abstract data types and their common operations
- *(Must)* Explain the hash table abstract data type and its common operations
- *(Must)* Analyze a given problem and determine the appropriate data structures and algorithms to use.
- *(Must)* Read, interpret, and debug existing code that utilizes data structures and algorithms, identifying their efficiency and purpose.
- *(Should)* Explain when to choose a stack, queue, or priority queue based on program requirements.
- *(Should)* Compare and contrast arrays, linked lists, and hash tables based on time efficiency for insertion, deletion, and lookup operations.
- *(Should)* Be capable of explaining how to solve a given problem using the appropriate data structure and algorithm

**Concept recap** *(sources: `content/03-dsa/big-o-complexity.md`, `content/03-dsa/collections-adts.md`)*
**Big-O** describes worst-case growth of work versus input size, dropping constants and lower-order
terms: O(1) constant, O(log n) halving, O(n) linear, O(n log n) efficient sorts, O(n^2) nested loops.
Reading code for its Big-O is pattern spotting — one loop over n is O(n); a loop in a loop is O(n^2); a
loop that halves its range is O(log n); a dictionary probe inside a loop is O(n) average, not O(n^2).
The **ADTs** and their C# faces: **list** — ordered, index-accessible (Add, Insert, RemoveAt, `list[i]`;
`List<T>`); **stack** — LIFO (Push/Pop/Peek; undo, backtracking); **queue** — FIFO (Enqueue/Dequeue/Peek;
process in arrival order); **priority queue** — serve by urgency, O(log n) enqueue/dequeue
(`PriorityQueue<TElement,TPriority>`); **hash table** — key-value with O(1) average add/remove/lookup
(`Dictionary<K,V>`, membership via `HashSet<T>`). Cross-structure trade-offs: array O(1) indexed read
but O(n) mid-insert (shifting); linked list O(1) splice at a known node but O(n) to find it; hash table
O(1) average everything, unordered.

**Key points / pitfalls**
- Say "average" for hash tables — worst case degrades with collisions. Interviewers listen for it.
- Choosing a structure is the applied Must: duplicates -> `HashSet`, ordered processing -> `Queue`,
  serve-by-priority -> `PriorityQueue`, fast key lookup -> `Dictionary`, sorted + searchable -> array +
  binary search.
- `List<T>` append is *amortized* O(1) — occasional resize copies the array.

**Worked example** *(cited: `content/03-dsa/collections-adts.md`; answer key
`demo/algorithms-threading-demo/` commit `02-dsa-complete` = `c70c979`)*
The fulfillment thread serves **expedited orders first** with a priority queue — the textbook
"choose the structure from the requirement" moment:

```csharp
var queue = new PriorityQueue<Order, int>();
queue.Enqueue(standardOrder, priority: 2);
queue.Enqueue(expeditedOrder, priority: 1);   // lower value = served first
var next = queue.Dequeue();                   // expedited jumps the line, O(log n)
```

---

## 4. Searching

**Objectives covered**
- *(Must)* Demonstrate the ability to perform linear search on arrays or lists and be able to identify the syntax.
- *(Must)* Demonstrate the ability to perform binary search on arrays or lists and be able to identify the syntax.
- *(Should)* Evaluate the trade-offs between linear search and binary search in terms of time complexity and required data structure conditions.

**Concept recap** *(source: `content/03-dsa/searching.md`)*
**Linear search** walks every element — O(n), works on anything. **Binary search** keeps `low`/`high`
bounds, probes the midpoint, and discards the impossible half — O(log n), but **only on sorted data**;
that precondition *is* the trade-off (sorting first costs O(n log n), so binary search pays off on
repeated lookups, not one-offs). A library helper exists (`Array.BinarySearch`), but the hand-rolled
shape is what "identify the syntax" means.

**Key points / pitfalls**
- Binary search on unsorted data doesn't error — it confidently returns garbage. State the precondition
  unprompted.
- `mid = low + (high - low) / 2` instead of `(low + high) / 2`: avoids integer overflow. Naming this is
  an easy depth signal.
- The live demo's original unbounded retry became a taught fix (`03a` rung): treat "loops that may never
  terminate" as part of reading/debugging algorithmic code.

**Worked example** *(cited: `demo/algorithms-threading-demo/DsaThreading/Searches.cs`)*

```csharp
// O(log n): halve the search space each step -- but ONLY on SORTED data.
public static int BinarySearch(int[] sorted, int target)
{
    int low = 0, high = sorted.Length - 1;
    while (low <= high)
    {
        int mid = low + (high - low) / 2;        // avoids overflow vs (low+high)/2
        if (sorted[mid] == target) return mid;
        if (sorted[mid] < target) low = mid + 1; // discard lower half
        else high = mid - 1;                     // discard upper half
    }
    return -1;
}
```

---

## 5. EF Core: Models, DbContext & Change Tracking

**Objectives covered**
- *(Must)* Create an EF Core model using EF Core code conventions.
- *(Must)* Explain the role of the DbContext class and how it manages database interactions.
- *(Must)* Explain how Entity Framework tracks changes in entities and persists them to the database.
- *(Should)* Create a dbcontext object in an application, and use it to manage persistance to a database.

**Concept recap** *(sources: `content/01-efcore/efcore-orm-dbcontext.md`,
`content/01-efcore/change-tracking-seeding.md`)*
EF Core is an ORM: C# classes (**entities**) map to tables, properties to columns. **Conventions** do
the first pass unaided — a property named `Id` (or `ProductId`) becomes the identity PK, a navigation
property plus `<Nav>Id` becomes an FK relationship, string props become `nvarchar`. The **DbContext**
subclass is the session with the database: it declares a `DbSet<T>` per entity, translates LINQ into
SQL, and owns the **change tracker**. Every tracked entity carries a state — `Added`, `Modified`,
`Deleted`, `Unchanged` — and `SaveChanges()` turns the delta into the matching INSERT/UPDATE/DELETE
batch inside a transaction. In ASP.NET Core the context comes from DI (constructor injection), is
**scoped** — one instance per request/unit of work — and is **not thread-safe**: never share one across
parallel tasks (`content/01-efcore/efcore-concurrency.md`).

**Key points / pitfalls**
- "How does EF know what SQL to run?" -> entity states in the change tracker; `SaveChanges()` reads
  them. Modified emits UPDATEs for changed columns only.
- Entities you `new` up are untracked until `Add`/`Attach`; entities from a query are tracked
  automatically (unless `AsNoTracking()`).
- One DbContext per unit of work. Sharing across threads throws or corrupts — the demo thread runs one
  context per parallel order for exactly this reason.

**Worked example** *(cited: `demo/library-fulfillment/Library.Data/LibraryDbContext.cs`)*

```csharp
public class LibraryDbContext : DbContext
{
    // DI calls this ctor; options carry the provider + connection string
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    // each DbSet registers an entity -> a table
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();
    public DbSet<Customer> Customers => Set<Customer>();
}
```

---

## 6. EF Core: Configuration, Migrations & Raw SQL

**Objectives covered**
- *(Must)* Use Entity Framework to generate a SQL schema with a code-first approach.
- *(Must)* Use Entity Framework to generate classes from a data source with a data-first approach.
- *(Must)* Describe the differences between the code-first and data-first approaches and when each should be applied.
- *(Should)* Configure a model using Data Annotations in the model class.
- *(Should)* Effectively manages migrations to avoid breaking changes and data loss.
- *(Should)* Describe the role of the Fluent API and when it is required instead of Data Annotations.
- *(Nice)* Call stored procedures and query  scalar types by dropping down to SQL, using FromSQL() and SqlQuery(). *(demo-waived; notes only)*
- *(Nice)* Modify a migration created by Entity Framework before execution. *(demo-waived; notes only)*

**Concept recap** *(sources: `content/01-efcore/code-first-data-first.md`,
`content/01-efcore/annotations-fluent-api.md`, `content/01-efcore/loading-strategies-raw-sql.md`)*
**Code-first**: the C# model is the source of truth — `dotnet ef migrations add <Name>` diffs the model
against the last migration and generates `Up()`/`Down()` code; `dotnet ef database update` applies it.
Right for greenfield. **Data-first**: an existing database is the truth —
`dotnet ef dbcontext scaffold "<conn>" Microsoft.EntityFrameworkCore.SqlServer` generates entities and a
context from the schema. Right for legacy. Configuration layers in precedence order **convention <
Data Annotations < Fluent API**: annotations decorate the model (`[Table("Customers")]`, `[Required]`,
`[MaxLength(100)]`, `[Key]`); the Fluent API in `OnModelCreating` expresses what annotations cannot —
unique non-key indexes, composite keys, column types, relationship ends — and wins conflicts. Migration
hygiene: incremental, review the generated SQL, never drop populated columns without a preserving step
(add-backfill-remove); a generated migration is editable code — inject `migrationBuilder.Sql(...)` into
`Up()` before applying. Escape hatch to SQL: `db.Books.FromSql($"EXEC GetBooks")` for entity sets,
`db.Database.SqlQuery<int>($"...")` for scalars — parameterized via the interpolation, injection-safe.

**Key points / pitfalls**
- "Which wins, annotation or Fluent?" — Fluent. Give the precedence chain unprompted.
- `HasData` seeding runs inside the migration, before the DB can issue identity values — seeds need
  explicit PKs (the demo comments call this out).
- The two Nice rows here are demo-waived: written note coverage only. Know the shape, don't expect
  muscle memory.

**Worked example** *(cited: `demo/library-fulfillment/Library.Data/LibraryDbContext.cs`,
`demo/library-fulfillment/Library.Data/Entities/Customer.cs`)*

```csharp
[Table("Customers")]                       // annotation: table name
public class Customer
{
    public int Id { get; set; }            // convention: PK, identity
    [Required, MaxLength(100)]             // annotations: NOT NULL, nvarchar(100)
    public string Name { get; set; } = default!;
}

protected override void OnModelCreating(ModelBuilder b)
{
    b.Entity<Customer>().Property(c => c.Email).HasMaxLength(256); // Fluent: length first...
    b.Entity<Customer>().HasIndex(c => c.Email).IsUnique();        // ...then unique index (SQL Server string rule)
    b.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(10,2)");
}
```

---

## 7. Threads, ThreadPool & the TPL

**Objectives covered**
- *(Must)* Create and manage threads in a C# application using the Thread class.
- *(Must)* Implement concurrency with the Task Parallel Library (TPL) for improved performance.
- *(Must)* Describe the lifecycle of a thread and how the runtime schedules execution.
- *(Must)* Explain how context switching affects performance in multithreaded applications.
- *(Must)* Explain the difference between parallelism and concurrency in the context of C# multithreading.
- *(Must)* Implement thread pooling using the ThreadPool class to manage lightweight parallel execution.
- *(Should)* Compare the performance impact of sequential vs. multithreaded execution.
- *(Should)* Apply async / await in combination with multithreading to optimize responsiveness.

**Concept recap** *(sources: `content/04-multithreading/threads-threadpool-lifecycle.md`,
`content/04-multithreading/parallelism-tpl.md`, `content/04-multithreading/async-await.md`)*
A **Thread** is the OS unit of execution: `new Thread(Work); t.Start(); t.Join();` — full control,
real cost (stack memory, scheduler load). Lifecycle: Unstarted -> Running -> (WaitSleepJoin) -> Stopped;
the OS **time-slices** runnable threads across cores, and each **context switch** saves/restores thread
state — pure overhead, so oversubscribing threads *reduces* throughput (thrashing). The **ThreadPool**
amortizes creation by reusing a right-sized pool (`ThreadPool.QueueUserWorkItem(_ => Work());`), and the
**TPL** is the modern face over it: `Task.Run` for one unit, `Parallel.For`/`ForEach` for data-parallel
CPU loops, `Task.WhenAll` for composition — with results, exceptions, and cancellation flowing properly.
**Concurrency** = interleaved progress (possible on one core); **parallelism** = simultaneous execution
on multiple cores. **async/await** is concurrency for I/O: while the awaited operation is in flight the
calling thread is freed; `await Task.Run(() => Heavy())` moves CPU work off a responsiveness-critical
caller. Speedup from parallelism is bounded by the serial fraction plus sync overhead (Amdahl's law) —
the demo measured sequential vs parallel fulfillment to make the gain, and its limits, concrete.

**Key points / pitfalls**
- Default answer for "how do I run this in the background" is `Task.Run`, not `new Thread` — reach for a
  dedicated thread only for long-lived, specially-configured work.
- More threads != faster: past core count you buy context switches, not parallelism.
- `async` does not create a thread. It releases one. Saying this cleanly separates you from most
  candidates.

**Worked example** *(cited: `demo/walkthroughs/dsa-02-tpl-sync.md`;
`demo/walkthroughs/04-priority-benchmark.md` applies it to the fulfillment benchmark)*

```csharp
var sw = Stopwatch.StartNew();
foreach (var order in orders) Fulfill(order);          // sequential baseline
var seq = sw.ElapsedMilliseconds;

sw.Restart();
Parallel.ForEach(orders, order => Fulfill(order));     // TPL: partitions across pooled threads
var par = sw.ElapsedMilliseconds;                      // faster - but not by core-count x (Amdahl)
```

---

## 8. Synchronization, Pitfalls & Cancellation

**Objectives covered**
- *(Must)* Explain synchronization techniques (e.g., lock, Monitor) and how they prevent race conditions.
- *(Must)* Identify and avoid common multithreading pitfalls such as deadlocks and thread starvation.
- *(Should)* Use thread-safe collections (e.g., ConcurrentDictionary, BlockingCollection) in multithreaded applications.
- *(Should)* Implement cancellation and exception handling in multithreaded code.
- *(Should)* Compare cooperative cancellation vs. preemptive interruption in managing multithreaded workloads.
- *(Should)* Evaluate trade-offs between different synchronization approaches (e.g., lock vs. Monitor vs. Interlocked).
- *(Nice)* Explain the use cases and differences between synchronization primitives such as SemaphoreSlim, Mutex, and ReaderWriterLockSlim.
- *(Nice)* Profile and analyze thread performance using Visual Studio diagnostics or similar tools. *(demo-waived; notes only)*

**Concept recap** *(sources: `content/04-multithreading/synchronization.md`,
`content/04-multithreading/synchronization-primitives.md`,
`content/04-multithreading/cancellation-exceptions.md`)*
A **race condition** is unsynchronized access to shared mutable state — `Balance += amount` is
read-modify-write, three steps that interleave and lose updates. `lock (_gate) { ... }` (syntactic sugar
over **Monitor** Enter/Exit with guaranteed release) serializes the critical section behind a dedicated
private lock object. **Interlocked** does single atomic hardware ops (increment, exchange, CAS) —
cheapest, but only for one operation. The primitives ladder *(Nice)*: `SemaphoreSlim(n)` admits at most
n concurrently (and awaits asynchronously); `Mutex` is a heavyweight **cross-process** named lock;
`ReaderWriterLockSlim` admits many readers or one writer. **Deadlock**: two threads each hold a lock the
other needs — prevent with a consistent acquisition order and small critical sections. **Starvation**: a
thread never scheduled. Thread-safe collections push locking into the structure:
`ConcurrentDictionary<K,V>` (`TryAdd`, `AddOrUpdate`), `BlockingCollection<T>` (producer/consumer),
`ConcurrentQueue<T>`. **Cancellation** is cooperative: a `CancellationTokenSource` hands out a token,
workers poll it (`ThrowIfCancellationRequested()`) and exit at safe points, surfacing
`OperationCanceledException` to awaiters — versus preemptive `Thread.Abort`, which killed threads
mid-statement, risked corrupt state, and is gone from modern .NET. Task exceptions arrive via `await`
(or `AggregateException` on `.Wait()`). Profiling tools *(Nice, notes only)*: VS Threads window,
Parallel Stacks, Concurrency Visualizer.

**Key points / pitfalls**
- Lock on a **dedicated private object** — never `this`, never a string, never a public object someone
  else can lock.
- `ConcurrentDictionary` makes each *operation* atomic, not multi-step *logic*: check-then-add is still
  a race unless you use `GetOrAdd`/`AddOrUpdate`.
- The demo's applied rule: **DbContext is not a thread-safe resource** — one per unit of work; the
  parallel fulfillment burst creates a context per order rather than locking a shared one.

**Worked example** *(cited: `demo/algorithms-threading-demo/DsaThreading/Bank.cs`, driven in
`demo/walkthroughs/dsa-02-tpl-sync.md`)*

```csharp
public class Bank
{
    public long Balance;
    private readonly object _gate = new();     // a dedicated lock object

    public void DepositUnsafe(long amount) => Balance += amount;   // read-modify-write: NOT atomic

    public void DepositSafe(long amount)
    {
        lock (_gate)                           // only one thread in here at a time
        {
            Balance += amount;
        }
    }
}
// Parallel.For(0, 100_000, _ => bank.DepositUnsafe(1)) -> Balance < 100000 (lost updates)
// Parallel.For(0, 100_000, _ => bank.DepositSafe(1))   -> Balance == 100000
```

---

## 9. ASP.NET Core: Pipeline, Middleware & Filters

**Objectives covered**
- *(Must, QC-4)* Describe the HTTP pipeline.
- *(Should, QC-4)* Implement native ASP.NET middleware, such as Logging or Identity.
- *(Nice, QC-4)* Implements third party or custom filters and middleware.

**Concept recap** *(sources: `content/06-aspnet-core/aspnet-pipeline-middleware.md`)*
The pipeline is an ordered chain of **middleware**: each component sees the request on the way down,
calls `next()`, and sees the response on the way back up — so order is behavior. Not calling `next()`
**short-circuits** the rest of the pipeline. Native middleware as taught, in the taught order: exception
handling first (it wraps everything below), Swagger, response caching, CORS, then
`UseAuthentication()` **before** `UseAuthorization()` (identity must exist before rights are checked),
and finally the mapped endpoints. Custom middleware is either inline (`app.Use(async (ctx, next) =>
{ ... })`) or a class (`app.UseMiddleware<ExceptionHandlingMiddleware>()`). **Filters** are the MVC-only
sibling: they wrap the *action* (not the whole pipeline) — `builder.Services.AddControllers(o =>
o.Filters.Add<TimingFilter>())` applies one to every controller. The taught litmus test: an `[Authorize]`
rejection short-circuits in the pipeline **before** MVC runs, so the 401 lacks the `X-Elapsed-ms` header
that the timing **filter** stamps on a 200.

**Key points / pitfalls**
- Auth order bug is the classic: `UseAuthorization()` before `UseAuthentication()` and every `[Authorize]`
  endpoint 401s. Recite the order.
- Middleware sees every request (even non-MVC); filters only run when routing reached a controller.
  Choose by scope.
- Exception middleware must be **first** so its try/catch wraps all downstream components.

**Worked example** *(cited: `demo/library-fulfillment/Library.ControllerApi/Program.cs`)*

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>(); // first: wraps everything below

app.Use(async (ctx, next) =>                      // custom inline middleware: timing + log
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    await next(ctx);                              // down the chain...
    sw.Stop();                                    // ...and back up
    Log.Information("{Method} {Path} -> {StatusCode} in {Elapsed} ms",
        ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode, sw.ElapsedMilliseconds);
});

app.Use(async (ctx, next) =>                      // short-circuit: 503 without calling next()
{
    if (ctx.Request.Headers.ContainsKey("X-Maintenance"))
    {
        ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await ctx.Response.WriteAsync("Down for maintenance");
        return;                                   // controllers never run
    }
    await next(ctx);
});

app.UseAuthentication();                          // WHO you are...
app.UseAuthorization();                           // ...then WHAT you may do
app.MapControllers();
```

---

## 10. ASP.NET Core: Controllers, DTOs, Validation & Cross-Cutting

**Objectives covered**
- *(Must, QC-4)* Implement controllers and action methods.
- *(Must, QC-4)* Describe the purpose and types of HTTP response codes.
- *(Must, QC-4)* Implement an API service.
- *(Must, QC-4)* Demonstrate functional knowledge of Data Transfer Objects, and their use.
- *(Must, QC-4)* Describe and implement a Minimal API endpoint.
- *(Must, QC-4)* Describe the function of HTTP method annotations.
- *(Should, QC-4)* Implement model binding effectively.
- *(Should, QC-4)* Describe and implement data validation using annotations.
- *(Should, QC-4)* Implement HTTP response codes effectively.
- *(Nice, QC-4)* Demonstrate understanding of caching in an API.
- *(Nice, QC-4)* Implement an API which consumes a 3rd party API.
- *(Should, QC-3 REST)* Build a RESTful web service using a popular framework (e.g. ASP.NET Core Minimal API)

**Concept recap** *(sources: `content/06-aspnet-core/controllers-actions.md`,
`content/06-aspnet-core/dtos-service-layer-automapper.md`,
`content/06-aspnet-core/model-binding-validation.md`, `content/06-aspnet-core/caching-consuming-apis.md`,
`content/02-rest-http/minimal-api-hosting.md`)*
Both API styles were built on the same Library domain. **Minimal API**: route + handler, no class —
`app.MapGet("/inventory", async (IInventoryRepository repo) => await repo.GetAllAsync());` — the Week-4
service (`Library.Api`) is entirely this shape. **Controllers** (`Library.ControllerApi`):
`[ApiController]` + `[Route("api/[controller]")]` on a `ControllerBase` subclass; **method annotations**
(`[HttpGet]`, `[HttpGet("{sku}")]`, `[HttpPost]`, `[HttpDelete("{sku}")]`) map actions to verb + route.
The **layered shape**: controller -> **service behind an interface** (`AddScoped<IInventoryService,
InventoryService>`) -> repository -> DbContext; the controller binds input, delegates, and translates
outcomes to codes — `Ok` 200, `CreatedAtAction` 201 + Location, `NoContent` 204, `NotFound` 404, with
400 automatic. **DTOs** shape what crosses the boundary (entities leak internals and cycle on
serialization); **AutoMapper** does the mechanical copying via a `MappingProfile` and
`_mapper.Map<InventoryDto>(entity)`. **Model binding** fills parameters from route/query/body/headers
(`[FromRoute]`, `[FromQuery]`, `[FromBody]`, `[FromHeader]`); **validation annotations** on the DTO run
during binding and `[ApiController]` turns failures into 400 ProblemDetails before the action runs.
Cross-cutting *(Nice, delivered)*: two-layer **caching** — `[ResponseCache]` + `UseResponseCaching()`
(HTTP) and `IMemoryCache.GetOrCreateAsync` with write-through eviction (server) — and **third-party
consumption** via a typed client (`AddHttpClient<ISupplierClient, SupplierClient>` with a
`BaseAddress`).

**Key points / pitfalls**
- Never return entities from actions — the demo's first attempt serialized an entity graph into an
  infinite loop; the DTO was the fix. Tell that story.
- .NET 10 gotcha (taught live): validation annotations on a **positional record** DTO must not be
  `[property:]`-targeted — that shape throws `InvalidOperationException` at runtime.
- Two caches, two invalidation stories: evicting `IMemoryCache` does **not** purge an HTTP-cached
  response — a `[ResponseCache]` duration can replay stale data after a write.
- `CreatedAtAction(nameof(GetBySku), ...)` — 201 must say *where* the new resource lives.

**Worked example** *(cited: `demo/library-fulfillment/Library.ControllerApi/Controllers/InventoryController.cs`)*

```csharp
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;   // API service via DI
    private readonly IMapper _mapper;              // AutoMapper

    [HttpGet("{sku}")]
    public async Task<ActionResult<InventoryDto>> GetBySku(string sku)
    {
        var item = await _service.BySkuAsync(sku);
        if (item is null) return NotFound();               // 404
        return Ok(_mapper.Map<InventoryDto>(item));        // 200 + DTO, never the entity
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<InventoryDto>> Create(InventoryCreateDto newInv)
    {
        var created = await _service.AddAsync(newInv);     // bound + validated from the body
        var response = _mapper.Map<InventoryDto>(created);
        _cache.Remove("inventory:all");                    // write-through cache invalidation
        return CreatedAtAction(nameof(GetBySku), new { sku = response.Sku }, response); // 201 + Location
    }
}
```
