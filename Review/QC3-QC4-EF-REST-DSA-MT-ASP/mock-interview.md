# QC-3 + QC-4 — Mock Interview Bank

Rehearse **out loud**, then compare with the model answer. Each entry names the QC objective it proves
and a real source. Tier badges: **[Must]** / **[Should]** / **[Nice]**.

---

## REST

**[Must] What is REST, and what makes an API "RESTful"?**
Model answer: REST is an architectural style for stateless, resource-based web services. Resources are
identified by URIs, manipulated through the uniform HTTP verb set, and every request is self-contained —
the server keeps no client session state. An API is RESTful when it follows the principles:
client-server, stateless, cacheable, uniform interface, layered system, and optionally code-on-demand.
Proves QC: "Describe the purpose of REST and RESTful design." / "Describe the REST principles."
Source: `weeklytechrepo/EFCore-REST-SOAP/content/02-rest-http/rest-principles.md`

**[Must] Walk me through what happens between typing a request and seeing the response.**
Model answer: The client opens a connection and sends an HTTP request — start line (method + URI +
version), headers, optional body. The server routes it, processes it, and returns a response — status
line (code + reason), headers, optional body. In our API that round trip is visible in the pipeline: the
request passes through middleware down to the endpoint, and the response flows back out the same chain
in reverse.
Proves QC: "Describe the HTTP request/response lifecycle." / "Describe the purpose of HTTP messaging."
Source: `weeklytechrepo/EFCore-REST-SOAP/content/02-rest-http/http-fundamentals.md`

**[Must] Your POST returned 201 but your DELETE returned 204 — why not 200 for both?**
Model answer: Codes carry meaning per outcome. 201 Created signals a new resource exists and ships a
`Location` header pointing at it — our `CreatedAtAction` builds that URI from the GET-by-sku action.
204 No Content says the operation succeeded and there is deliberately no body to return. Both are 2xx
success; the class tells you where to look — 4xx means the client got it wrong, 5xx means the server did.
Proves QC: "Describe HTTP response code classes (1xx,2xx,3xx,etc.)"
Source: `weeklytechrepo/EFCore-REST-SOAP/demo/library-fulfillment/Library.ControllerApi/Controllers/InventoryController.cs`

**[Should] What's the difference between authentication and authorization?**
Model answer: Authentication proves who you are — the login, verified credentials, in our API a valid
JWT; failing it is a 401. Authorization decides what an authenticated identity may do — role or policy
checks; failing it is a 403. The demo shows three tiers on one controller: public reads, `[Authorize]`
for any logged-in user, `[Authorize(Roles = "admin")]` for writes.
Proves QC: "Describe the difference between authorization and authentication."
Source: `weeklytechrepo/EFCore-REST-SOAP/content/08-security/authentication-jwt.md`

**[Nice] When would you pick SOAP over REST?**
Model answer: SOAP when a formal machine-readable contract (WSDL), strict typing, and enterprise
standards like WS-Security matter, or when integrating with legacy systems that already speak it — every
call is an XML envelope POSTed to one endpoint. REST for everything web-scale: lighter JSON payloads,
HTTP-native caching, verb semantics, easier evolution.
Proves QC: "Compare and contrast RESTful and SOAP-based web services in terms of functionality, performance, and scalability"
Source: `weeklytechrepo/EFCore-REST-SOAP/content/07-soap/soap-vs-rest.md`

---

## DSA

**[Must] What is Big-O, and what's the complexity of this nested loop?**
Model answer: Big-O describes how work grows with input size in the worst case, dropping constants and
lower-order terms — O(1), O(log n), O(n), O(n log n), O(n^2) is the ladder to know. A loop over n inside
a loop over n is O(n^2); a loop that halves its range each pass is O(log n).
Proves QC: "Describe algorithm complexity using Big O notation and analyze the efficiency of an algorithm or data structure"
Source: `weeklytechrepo/EFCore-REST-SOAP/content/03-dsa/big-o-complexity.md`

**[Must] Explain binary search and its precondition.**
Model answer: Keep low and high pointers, probe the midpoint, and discard the half that cannot contain
the target — O(log n) because the space halves each step. The precondition is sorted data; on unsorted
data it silently returns wrong answers, which is why linear search — O(n), no precondition — still has a
job. One syntax tell: `mid = low + (high - low) / 2`, written that way to avoid integer overflow.
Proves QC: "Demonstrate the ability to perform binary search on arrays or lists and be able to identify the syntax." + the linear-vs-binary trade-off row.
Source: `weeklytechrepo/EFCore-REST-SOAP/demo/algorithms-threading-demo/DsaThreading/Searches.cs`

**[Must] Compare a stack, a queue, and a hash table — when do you reach for each?**
Model answer: A stack is LIFO — Push/Pop/Peek, O(1) — for undo and backtracking. A queue is FIFO —
Enqueue/Dequeue, O(1) — for processing in arrival order. A hash table is a key-value store with O(1)
average add/remove/lookup — `Dictionary<K,V>` — whenever fast lookup by key dominates. If items must be
served by urgency instead of arrival, that's the priority queue's job at O(log n) per operation.
Proves QC: the list/stack/queue/hash-table ADT Must rows + the stack/queue/priority-queue Should row.
Source: `weeklytechrepo/EFCore-REST-SOAP/content/03-dsa/collections-adts.md`

**[Should] Arrays vs linked lists vs hash tables for insert, delete, lookup?**
Model answer: Array: O(1) indexed lookup, O(n) insert/delete because elements shift. Linked list: O(1)
insert/delete at a node you already hold, O(n) to find it. Hash table: O(1) average for all three, at
the cost of ordering and some memory. So: index-heavy reads -> array; splice-heavy edits -> linked list;
key lookups -> hash table.
Proves QC: "Compare and contrast arrays, linked lists, and hash tables based on time efficiency for insertion, deletion, and lookup operations."
Source: `weeklytechrepo/EFCore-REST-SOAP/content/03-dsa/collections-adts.md`

---

## EF Core

**[Must] What does DbContext actually do for you?**
Model answer: It's the session with the database. It exposes a `DbSet<T>` per entity, translates LINQ to
SQL, and runs the change tracker: every tracked entity carries a state — Added, Modified, Deleted,
Unchanged — and `SaveChanges()` emits the matching INSERT/UPDATE/DELETE in one transaction. You subclass
it, register it in DI, and take it as a constructor dependency — scoped, one instance per unit of work,
never shared across threads.
Proves QC: "Explain the role of the DbContext class and how it manages database interactions." + the change-tracking row.
Source: `weeklytechrepo/EFCore-REST-SOAP/content/01-efcore/efcore-orm-dbcontext.md`, `demo/library-fulfillment/Library.Data/LibraryDbContext.cs`

**[Must] Code-first vs data-first — which did you use and why?**
Model answer: Code-first writes C# classes and generates the schema — `dotnet ef migrations add` then
`database update` — right for greenfield where the model is the source of truth; that is what our
Library thread does. Data-first scaffolds classes from an existing database with
`dotnet ef dbcontext scaffold` — right for legacy schemas you don't own. The deciding question is which
side already exists.
Proves QC: the code-first, data-first, and differences Must rows.
Source: `weeklytechrepo/EFCore-REST-SOAP/content/01-efcore/code-first-data-first.md`

**[Should] When do you need the Fluent API instead of Data Annotations?**
Model answer: Annotations handle per-property basics — `[Required]`, `[MaxLength]`, `[Key]` — right on
the model. Fluent API in `OnModelCreating` is required for what annotations can't express: unique
non-key indexes, composite keys, column types like `decimal(10,2)`, relationship fine-tuning. It also
wins on precedence: convention, then annotations, then Fluent.
Proves QC: "Describe the role of the Fluent API and when it is required instead of Data Annotations." + the annotations row.
Source: `weeklytechrepo/EFCore-REST-SOAP/content/01-efcore/annotations-fluent-api.md`, `demo/library-fulfillment/Library.Data/LibraryDbContext.cs`

**[Should] How do you keep migrations from destroying data?**
Model answer: Apply small incremental migrations, read the generated SQL before running it, and never
drop a populated column or table without a data-preserving step — add the new column, backfill, then
remove the old one. Every migration has `Up()` and `Down()`, and you can edit the generated code — e.g.
inject `migrationBuilder.Sql(...)` for a backfill — before `database update`.
Proves QC: "Effectively manages migrations to avoid breaking changes and data loss." + the Nice modify-a-migration row.
Source: `weeklytechrepo/EFCore-REST-SOAP/content/01-efcore/code-first-data-first.md`

**[Nice] Can EF Core call a stored procedure?**
Model answer: Yes — drop down to SQL: `db.Books.FromSql($"EXEC GetBooks")` materializes entities from a
procedure or raw query, and `db.Database.SqlQuery<int>(...)` returns scalars. Both use interpolated
parameters, so they're injection-safe. You reach for them when the set logic already lives in the
database or LINQ can't express the query.
Proves QC: "Call stored procedures and query  scalar types by dropping down to SQL, using FromSQL() and SqlQuery()."
Source: `weeklytechrepo/EFCore-REST-SOAP/content/01-efcore/loading-strategies-raw-sql.md`

---

## C# Multithreading

**[Must] What's a race condition and how does lock prevent it?**
Model answer: A race is two threads doing an unsynchronized read-modify-write on shared state —
`Balance += amount` is three steps, and interleaving loses updates. `lock (_gate) { ... }` lets one
thread at a time through the critical section: it acquires a Monitor on a dedicated private object and
guarantees release even on exception. The demo's bank shows the unsafe deposit losing money under
`Parallel.For` and the locked version landing exact.
Proves QC: "Explain synchronization techniques (e.g., lock, Monitor) and how they prevent race conditions."
Source: `weeklytechrepo/EFCore-REST-SOAP/demo/algorithms-threading-demo/DsaThreading/Bank.cs`, `content/04-multithreading/synchronization.md`

**[Must] Concurrency vs parallelism?**
Model answer: Concurrency is structure — multiple tasks in flight, making progress by interleaving,
possible on one core. Parallelism is execution — tasks literally running at the same instant on multiple
cores. `async/await` gives you concurrency during I/O; `Parallel.For` gives you parallelism for CPU
work. Related, not synonyms.
Proves QC: "Explain the difference between parallelism and concurrency in the context of C# multithreading."
Source: `weeklytechrepo/EFCore-REST-SOAP/content/04-multithreading/parallelism-tpl.md`

**[Must] What is a deadlock and how do you avoid one?**
Model answer: Two threads each hold a lock the other needs — neither can proceed, forever. The standard
avoidance is a consistent global lock-acquisition order, plus keeping critical sections small and never
calling out to unknown code while holding a lock. Starvation is the gentler cousin — a thread that never
gets scheduled because others monopolize the resource.
Proves QC: "Identify and avoid common multithreading pitfalls such as deadlocks and thread starvation."
Source: `weeklytechrepo/EFCore-REST-SOAP/content/04-multithreading/synchronization.md`

**[Must] Why not just create a thousand threads?**
Model answer: Each thread costs stack memory and scheduler work; every context switch saves and restores
thread state, and past the core count you're paying switch overhead without gaining parallelism —
throughput drops. That's what the ThreadPool and TPL are for: a managed pool of reusable threads sized
to the machine, fed lightweight work items — `ThreadPool.QueueUserWorkItem` or, normally, `Task.Run`.
Proves QC: the context-switching, ThreadPool, and lifecycle Must rows.
Source: `weeklytechrepo/EFCore-REST-SOAP/content/04-multithreading/threads-threadpool-lifecycle.md`

**[Should] How does cooperative cancellation work, and why is it better than killing a thread?**
Model answer: You pass a `CancellationToken` from a `CancellationTokenSource`; the worker polls it —
`IsCancellationRequested` or `ThrowIfCancellationRequested()` — and exits cleanly at a safe point,
surfacing `OperationCanceledException` to the awaiter. Preemptive interruption like `Thread.Abort`
killed the thread mid-statement, risking corrupt shared state and leaked locks — which is why modern
.NET removed it.
Proves QC: the cancellation/exception and cooperative-vs-preemptive Should rows.
Source: `weeklytechrepo/EFCore-REST-SOAP/content/04-multithreading/cancellation-exceptions.md`

**[Should] When Interlocked over lock?**
Model answer: `Interlocked` does a single atomic hardware operation — increment, exchange, compare-and-
swap — with no blocking, so it's the fastest tool when the critical section is exactly one operation.
The moment the invariant spans multiple statements or multiple fields, you need `lock`/`Monitor`.
`SemaphoreSlim` when you want at-most-N concurrent workers, `ReaderWriterLockSlim` for many-readers-one-
writer, `Mutex` only when the lock must cross processes.
Proves QC: "Evaluate trade-offs between different synchronization approaches (e.g., lock vs. Monitor vs. Interlocked)." + the Nice primitives row.
Source: `weeklytechrepo/EFCore-REST-SOAP/content/04-multithreading/synchronization.md`, `content/04-multithreading/synchronization-primitives.md`

---

## ASP.NET Core

**[Must] Describe the ASP.NET Core HTTP pipeline.**
Model answer: An ordered chain of middleware the request flows down and the response flows back up in
reverse. Order is behavior: our API puts exception handling first so it wraps everything, a maintenance
check that can short-circuit with 503 by not calling `next()`, then response caching, CORS,
authentication, authorization, and finally the routed endpoint. Auth failures short-circuit before MVC —
which is why our 401s lack the timing header a filter adds to 200s.
Proves QC: "Describe the HTTP pipeline."
Source: `weeklytechrepo/EFCore-REST-SOAP/demo/library-fulfillment/Library.ControllerApi/Program.cs`, `content/06-aspnet-core/aspnet-pipeline-middleware.md`

**[Must] Why DTOs instead of returning your entities?**
Model answer: DTOs shape the data crossing the API boundary and decouple the public contract from the
internal model. Returning entities leaks fields you didn't mean to publish and — with navigation
properties — creates serialization cycles; our first attempt at returning the entity graph looped
infinitely until the DTO fixed it. Mapping is mechanical, so AutoMapper does it:
`_mapper.Map<InventoryDto>(item)` against a `MappingProfile`.
Proves QC: "Demonstrate functional knowledge of Data Transfer Objects, and their use."
Source: `weeklytechrepo/EFCore-REST-SOAP/content/06-aspnet-core/dtos-service-layer-automapper.md`, `demo/library-fulfillment/Library.ControllerApi/Controllers/InventoryController.cs`

**[Must] What does "implement an API service" mean in this stack?**
Model answer: Business logic lives behind an interface, registered in DI —
`builder.Services.AddScoped<IInventoryService, InventoryService>()` — and injected into the controller
by constructor. The controller stays thin: bind, delegate to the service, translate the result to a
status code. The service in turn depends on a repository interface, so each layer is swappable and
testable.
Proves QC: "Implement an API service."
Source: `weeklytechrepo/EFCore-REST-SOAP/demo/library-fulfillment/Library.ControllerApi/Services/InventoryService.cs`, `content/06-aspnet-core/dtos-service-layer-automapper.md`

**[Must] Minimal API vs controllers — when each?**
Model answer: A Minimal API endpoint is a route plus a handler — `app.MapGet("/users", () => users)` —
no controller class; great for small services and fast starts, and it's how our first API was built.
Controllers give the full MVC toolkit — `[ApiController]` model-state handling, filters, conventions —
which pays off as the surface grows; that's why the thread graduated to `InventoryController`. Same
pipeline underneath.
Proves QC: "Describe and implement a Minimal API endpoint." + controllers/actions row.
Source: `weeklytechrepo/EFCore-REST-SOAP/content/02-rest-http/minimal-api-hosting.md`, `content/06-aspnet-core/controllers-actions.md`

**[Should] How does model binding decide where a parameter comes from?**
Model answer: By source and convention: route templates fill route parameters, the query string fills
simple types, the JSON body fills complex types on POST/PUT, and you can force any of it with
`[FromRoute]`, `[FromQuery]`, `[FromBody]`, `[FromHeader]`. Validation annotations run during binding,
and `[ApiController]` turns a ModelState failure into an automatic 400 before the action executes.
Proves QC: "Implement model binding effectively." + the validation row.
Source: `weeklytechrepo/EFCore-REST-SOAP/content/06-aspnet-core/model-binding-validation.md`

**[Nice] Your GET is cached — how do you keep writes from serving stale data?**
Model answer: Two layers, invalidated differently. Server-side `IMemoryCache` entries get an absolute
expiry and are explicitly evicted in every write action — `_cache.Remove("inventory:all")` in Create and
Delete, write-through invalidation. HTTP response caching (`[ResponseCache]` + `UseResponseCaching`) is
client/proxy side and can replay a stale body after the server cache was evicted — the two are
independent, so keep HTTP durations short on data that writes touch.
Proves QC: "Demonstrate understanding of caching in an API."
Source: `weeklytechrepo/EFCore-REST-SOAP/content/06-aspnet-core/caching-consuming-apis.md`, `demo/library-fulfillment/Library.ControllerApi/Controllers/InventoryController.cs`

**[Nice] How would your API consume a third-party API?**
Model answer: A typed client: `AddHttpClient<ISupplierClient, SupplierClient>` with a `BaseAddress`, so
the factory manages handler lifetimes and the consumer just injects the interface. The controller action
awaits `GetListPriceAsync(sku)` and maps null to 404. That keeps the outbound HTTP concern in one class
instead of scattered `new HttpClient()` calls — which exhaust sockets.
Proves QC: "Implement an API which consumes a 3rd party API."
Source: `weeklytechrepo/EFCore-REST-SOAP/demo/library-fulfillment/Library.ControllerApi/Services/SupplierClient.cs`, `demo/library-fulfillment/Library.ControllerApi/Program.cs`
