# QC-3 + QC-4 — Drills

Short hands-on tasks per section. **Prompts are domain-neutral — do them in your own domain** (the domain
you used for your project), then compare against the model solution, which uses the trainer **Library**
domain. Each drill names the QC objective it exercises. No drills exist for the register-only profiling
item (see `out-of-scope-register.md`).

---

## REST

### Drill 1 — Read and create over HTTP
**Prompt:** With your API (or any open API) running, send a GET for a collection resource, then a POST
that creates one item, using curl or Postman. Predict the status code of each before sending.
*Proves QC (Must):* "Be capable of sending a GET request to an open source REST API using curl or Postman"
and *(Should)* the POST-with-body sibling.

**Model solution** *(source: `demo/walkthroughs/06b-layers-rest-middleware.md`)*:

```bash
curl http://localhost:5137/api/inventory                    # expect 200 + JSON array
curl -X POST http://localhost:5137/api/inventory \
  -H "Content-Type: application/json" \
  -d '{"sku":"BK-010","name":"Domain-Driven Design","price":54.99,"currentStock":4}'
# expect 201 Created + Location header pointing at /api/Inventory/BK-010
```

### Drill 2 — Design the URL surface
**Prompt:** For two related resources in your domain (a parent and its children), write the URL + verb
table for full CRUD, following REST conventions. No verbs in the paths.
*Proves QC (Should):* "Describe URL conventions when using REST."

**Model solution** *(source: `content/02-rest-http/rest-principles.md`)*:

| Operation | Verb | URL |
|---|---|---|
| list inventory | GET | `/api/inventory` |
| one item | GET | `/api/inventory/{sku}` |
| create | POST | `/api/inventory` |
| delete | DELETE | `/api/inventory/{sku}` |
| a customer's orders | GET | `/customers/{id}/orders` |

### Drill 3 — Status code prediction
**Prompt:** For your protected write endpoint, predict the response to: (a) no token, (b) a valid token
with the wrong role, (c) a valid admin token with an invalid body, (d) a valid admin token and body.
*Proves QC (Must):* "Describe HTTP response code classes (1xx,2xx,3xx,etc.)" and *(Should)* the
authentication-vs-authorization distinction.

**Model solution** *(source: `Library.ControllerApi/Controllers/InventoryController.cs` `Create`)*:
(a) **401** — authentication failed; (b) **403** — authenticated but not authorized;
(c) **400** — `[ApiController]` rejects the ModelState failure; (d) **201** with `Location`.

---

## DSA

### Drill 4 — Hand-run the searches
**Prompt:** On the sorted array `[3, 9, 14, 21, 30, 42]`, hand-trace a linear search and a binary search
for `30` — list every index each visits, then state each algorithm's Big-O and precondition.
*Proves QC (Must):* linear search, binary search, and the *(Should)* linear-vs-binary trade-off.

**Model solution** *(source: `demo/algorithms-threading-demo/DsaThreading/Searches.cs`)*:
Linear visits indexes 0,1,2,3,4 -> found (O(n), no precondition). Binary visits mid=2 (14 < 30), mid=4
(30 == 30) -> found in 2 probes (O(log n), **requires sorted data**).

### Drill 5 — Identify the sort
**Prompt:** Without labels, classify these three snippets as bubble / insertion / selection and give the
tell for each:

```csharp
// A: for(i) { min=i; for(j>i) if(a[j]<a[min]) min=j; swap(a[i],a[min]); }
// B: for(i) for(j < n-1-i) if(a[j]>a[j+1]) swap(a[j],a[j+1]);
// C: for(i=1..n) { key=a[i]; while(j>=0 && a[j]>key) shift; a[j+1]=key; }
```

*Proves QC (Should):* the three "perform X sort ... identify the syntax" rows and *(Must)* "Describe the
difference between common sorting algorithms."

**Model solution** *(source: `demo/algorithms-threading-demo/DsaThreading/Sorts.cs`)*:
A = **selection** (find min of unsorted region, swap into place). B = **bubble** (adjacent-pair swaps).
C = **insertion** (sorted prefix, shift right, drop the key). All O(n^2); merge sort is the O(n log n)
divide-and-conquer contrast.

### Drill 6 — Choose the structure
**Prompt:** For each requirement in your domain, name the data structure and justify with its operation
costs: (a) detect duplicate identifiers while importing, (b) process work items strictly in arrival
order, (c) undo the last action repeatedly, (d) always handle the most urgent item first.
*Proves QC (Must):* "Analyze a given problem and determine the appropriate data structures and
algorithms to use." and the *(Should)* stack/queue/priority-queue choice row.

**Model solution** *(source: `content/03-dsa/collections-adts.md`)*:
(a) `HashSet<string>` — `Add` returns false on duplicate, O(1) average; (b) `Queue<T>` — FIFO
Enqueue/Dequeue O(1); (c) `Stack<T>` — LIFO Push/Pop O(1); (d) `PriorityQueue<TElement,TPriority>` —
Enqueue/Dequeue O(log n), Peek O(1). The Library thread uses exactly this for expedited-first order
fulfillment.

### Drill 7 — Recursion + memoization (Nice)
**Prompt:** Write a recursive method for a self-similar computation in your domain; label base and
recursive case. Then add a `Dictionary` cache and explain what changes about the cost.
*Proves QC (Nice):* the recursion and memoization/tabulation rows.

**Model solution** *(source: `content/03-dsa/recursion-memoization.md`, answer key
`demo/algorithms-threading-demo/` commit `02-dsa-complete` = `c70c979`)*:

```csharp
static readonly Dictionary<int, long> Memo = new();
static long Fib(int n)
{
    if (n <= 1) return n;                       // base case
    if (Memo.TryGetValue(n, out var hit)) return hit;
    return Memo[n] = Fib(n - 1) + Fib(n - 2);   // recursive case, cached
}
```
Uncached: O(2^n) calls. Memoized: each n computed once -> O(n). Tabulation builds the same table
bottom-up in a loop.

---

## EF Core

### Drill 8 — Model by convention, then annotate
**Prompt:** Create an entity for your domain using conventions only (PK by name, string props), generate
a migration, and apply it. Then add `[Required]` + `[MaxLength]` and a second migration. Inspect what
each migration generated.
*Proves QC (Must):* "Create an EF Core model using EF Core code conventions." + code-first schema
generation; *(Should)* Data Annotations.

**Model solution** *(source: `Library.Data/Entities/Customer.cs`, migrations
`20260701163020_OrdersCustomers` -> `20260701164510_AnnotatedCustomer`)*:

```csharp
public class Customer
{
    public int Id { get; set; }                       // convention: "Id" -> PK, identity
    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;      // annotation pass -> nvarchar(100) NOT NULL
    public List<Order> Orders { get; set; } = new();  // convention: nav prop -> FK on Order
}
```
```bash
dotnet ef migrations add OrdersCustomers && dotnet ef database update
```

### Drill 9 — Fluent API where annotations can't reach
**Prompt:** Give one of your entities a unique index and a decimal column type via `OnModelCreating`,
and say why annotations alone couldn't do it.
*Proves QC (Should):* "Describe the role of the Fluent API and when it is required instead of Data
Annotations."

**Model solution** *(source: `Library.Data/LibraryDbContext.cs`)*:

```csharp
protected override void OnModelCreating(ModelBuilder b)
{
    b.Entity<Product>(e =>
    {
        e.HasIndex(p => p.Sku).IsUnique();                    // unique non-key index: Fluent-only
        e.Property(p => p.Price).HasColumnType("decimal(10,2)");
    });
}
```
Unique indexes, composite keys, and relationship fine-tuning have no annotation equivalent — Fluent API
(highest precedence: convention < annotations < Fluent) is the only home.

### Drill 10 — Narrate the change tracker
**Prompt:** Write three statements that add, modify, and delete entities, and for each, state the entity
state before `SaveChanges()` and the SQL emitted by it.
*Proves QC (Must):* "Explain how Entity Framework tracks changes in entities and persists them to the
database." + the DbContext role row.

**Model solution** *(source: `content/01-efcore/change-tracking-seeding.md`)*:

```csharp
db.Products.Add(newProduct);        // state Added     -> INSERT
existing.Price = 29.99m;            // state Modified  -> UPDATE (only changed columns)
db.Products.Remove(old);            // state Deleted   -> DELETE
db.SaveChanges();                   // one batch, in a transaction
```

---

## C# Multithreading

### Drill 11 — Race, then fix
**Prompt:** Create a shared counter and increment it 100,000 times from several parallel tasks without
synchronization; print the result. Then fix it twice — once with `lock`, once with `Interlocked` — and
explain when you'd pick each.
*Proves QC (Must):* synchronization/race rows; *(Should)* lock vs Monitor vs Interlocked trade-offs.

**Model solution** *(source: `demo/algorithms-threading-demo/DsaThreading/Bank.cs`,
`demo/walkthroughs/dsa-02-tpl-sync.md`)*:

```csharp
var bank = new Bank();
Parallel.For(0, 100_000, _ => bank.DepositUnsafe(1));   // result < 100000: lost updates
Parallel.For(0, 100_000, _ => bank.DepositSafe(1));     // lock(_gate) -> exactly 100000
Parallel.For(0, 100_000, _ => Interlocked.Increment(ref bank.Balance)); // atomic single op
```
`Interlocked` for a single atomic operation (cheapest); `lock` for multi-statement critical sections.

### Drill 12 — Cooperative cancellation
**Prompt:** Start a long-running task in your domain that honors a `CancellationToken`; cancel it after
two seconds and prove it stopped cleanly (no `Thread.Abort`-style kill).
*Proves QC (Should):* cancellation/exception handling + cooperative-vs-preemptive rows.

**Model solution** *(source: `content/04-multithreading/cancellation-exceptions.md`)*:

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
var worker = Task.Run(async () =>
{
    while (true)
    {
        cts.Token.ThrowIfCancellationRequested();   // cooperative exit point
        await ProcessNextOrderAsync();
    }
}, cts.Token);
try { await worker; }
catch (OperationCanceledException) { Console.WriteLine("stopped cleanly"); }
```

### Drill 13 — Thread vs ThreadPool vs Task
**Prompt:** Run the same unit of work three ways — dedicated `Thread`, `ThreadPool.QueueUserWorkItem`,
and `Task.Run` — and state one reason to choose each.
*Proves QC (Must):* Thread class, ThreadPool, and TPL rows.

**Model solution** *(source: `content/04-multithreading/threads-threadpool-lifecycle.md`,
`parallelism-tpl.md`)*:

```csharp
var t = new Thread(ScanShelves); t.Start(); t.Join();  // long-lived / full control
ThreadPool.QueueUserWorkItem(_ => ScanShelves());      // cheap fire-and-forget, pooled
await Task.Run(ScanShelves);                           // default: awaitable, composable, exceptions flow
```

---

## ASP.NET Core

### Drill 14 — Full REST controller
**Prompt:** Build a controller for one resource in your domain with GET-all, GET-by-key, POST, DELETE —
returning 200/201 + Location/204/404 correctly and taking/returning DTOs (never entities).
*Proves QC (Must):* controllers + action methods, DTOs, HTTP method annotations; *(Should)* effective
response codes.

**Model solution** *(source: `Library.ControllerApi/Controllers/InventoryController.cs`)*: see `Get`,
`GetBySku` (404 on miss), `Create` (`CreatedAtAction(nameof(GetBySku), new { sku = response.Sku },
response)`), `Delete` (`NoContent()` / `NotFound()`); DTO mapping via `_mapper.Map<InventoryDto>(...)`.

### Drill 15 — Short-circuit middleware
**Prompt:** Add an inline middleware that returns 503 when a special header is present, without calling
the rest of the pipeline; place it so it still gets exception handling above it. Prove the short-circuit.
*Proves QC (Should):* native middleware; *(Nice)* custom filters and middleware; *(Must)* the HTTP
pipeline.

**Model solution** *(source: `Library.ControllerApi/Program.cs`)*:

```csharp
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Headers.ContainsKey("X-Maintenance"))
    {
        ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await ctx.Response.WriteAsync("Down for maintenance");
        return;                     // no next() -> controllers never run
    }
    await next(ctx);
});
```
```bash
curl -H "X-Maintenance: 1" http://localhost:5137/api/inventory   # 503, no controller log line
```

### Drill 16 — Validation to 400
**Prompt:** Add `[Required]` and a range/length constraint to your create-DTO and prove that an invalid
body yields an automatic 400 with the field errors — no `if` statements in the action.
*Proves QC (Should):* "Describe and implement data validation using annotations." + model binding.

**Model solution** *(source: `content/06-aspnet-core/model-binding-validation.md`,
`Library.ControllerApi/DTOs/InventoryCreateDtos.cs`)*: annotate the DTO; `[ApiController]` checks
`ModelState` during binding and returns 400 ProblemDetails before the action executes. Watch the .NET 10
record gotcha: put plain annotations on record properties — `[property:]`-targeted attributes on
positional records throw at runtime.

### Drill 17 — Cache with write-through invalidation (Nice)
**Prompt:** Cache your collection GET server-side with a 2-minute absolute expiry, and evict the entry
in every write action. Prove eviction: GET, POST, GET — the second GET must show the new item.
*Proves QC (Nice):* "Demonstrate understanding of caching in an API."

**Model solution** *(source: `Library.ControllerApi/Controllers/InventoryController.cs`)*:
`_cache.GetOrCreateAsync("inventory:all", entry => { entry.AbsoluteExpirationRelativeToNow =
TimeSpan.FromMinutes(2); ... })` in `Get`; `_cache.Remove("inventory:all")` in `Create` and `Delete` —
a write invalidates because DB state changed.
