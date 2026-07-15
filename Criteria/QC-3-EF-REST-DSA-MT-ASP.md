# QC 3 (EF Core, DSA, REST, C# Multithreading, ASP.NET) Criteria

## REST

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Describe the purpose of REST and RESTful design. | An architectural style for stateless, resource-based web services that use HTTP verbs to operate on resources identified by URIs. |
| Must know | Describe the purpose of HTTP messaging. | HTTP is the request/response protocol that carries data between client and server as text-based messages with a start line, headers, and an optional body. |
| Must know | Describe the HTTP request/response lifecycle. | Client opens a connection and sends a request (method + URI + headers + body); the server processes it and returns a response (status code + headers + body). |
| Must know | Describe HTTP request methods (verbs). | GET (read), POST (create), PUT (replace), PATCH (partial update), DELETE (remove). |
| Must know | Describe HTTP response code classes (1xx,2xx,3xx,etc.) | 1xx informational, 2xx success, 3xx redirection, 4xx client error, 5xx server error. |
| Must know | Describe the REST principles. | Client-server, stateless, cacheable, uniform interface, layered system, and code-on-demand (optional). |
| Must know | Be capable of sending a GET request to an open source REST API using curl or Postman | `curl https://api.example.com/users` |
| Should know | Describe URL conventions when using REST. | Use plural nouns for resources and nest for relationships: `/users`, `/users/1`, `/users/1/orders`. |
| Should know | Describe the difference between authorization and authentication. | Authentication verifies who you are (login); authorization verifies what you are allowed to do (permissions). |
| Should know | Be capable of sending a POST request to a REST API using curl or Postman and populating the request body | `curl -X POST https://api.example.com/users -H "Content-Type: application/json" -d '{"name":"Alice"}'` |
| Should know | Build a RESTful web service using a popular framework (e.g. ASP.NET Core Minimal API) | `app.MapGet("/users", () => users); // ASP.NET Core Minimal API` |
| Nice to Have | Implement authentication and authorization using a popular RESTful framework (e.g. OAuth, JWT) | Issue a signed JWT on login and validate it on each request via middleware (e.g. `AddAuthentication().AddJwtBearer()`). |
| Nice to Have | Compare and contrast RESTful and SOAP-based web services in terms of functionality, performance, and scalability | REST is lightweight (JSON over HTTP, cacheable, scalable); SOAP is heavier (XML, strict WSDL contracts, built-in standards) and suited to formal enterprise transactions. |

## DSA

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Describe algorithm complexity using Big O notation and analyze the efficiency of an algorithm or data structure | Big O describes worst-case growth vs. input size: O(1) constant, O(log n), O(n) linear, O(n^2) quadratic. |
| Must know | Explain the list abstract data type and its common operations | Ordered, index-accessible collection supporting Add, Insert, RemoveAt, and indexed get/set (`list[i]`). |
| Must know | Explain the stack and queue abstract data types and their common operations | Stack is LIFO (Push/Pop/Peek); Queue is FIFO (Enqueue/Dequeue/Peek). |
| Must know | Explain the hash table abstract data type and its common operations | Key-value store with average O(1) Add, Remove, and lookup via hashing; e.g. `Dictionary<K,V>`. |
| Must know | Demonstrate the ability to perform linear search on arrays or lists and be able to identify the syntax. | `for (int i = 0; i < arr.Length; i++) if (arr[i] == target) return i;` |
| Must know | Analyze a given problem and determine the appropriate data structures and algorithms to use. | E.g. choose a hash table for fast lookups, a queue for FIFO processing, or binary search over sorted data. |
| Must know | Read, interpret, and debug existing code that utilizes data structures and algorithms, identifying their efficiency and purpose. | Trace the code's loops and structures to determine its Big-O cost and intent (e.g. nested loops = O(n^2)). |
| Must know | Demonstrate the ability to perform binary search on arrays or lists and be able to identify the syntax. | `int idx = Array.BinarySearch(sortedArr, target);` |
| Should know | Explain when to choose a stack, queue, or priority queue based on program requirements. | Stack for undo/backtracking (LIFO), queue for ordered processing (FIFO), priority queue when items are served by priority. |
| Should know | Evaluate the trade-offs between linear search and binary search in terms of time complexity and required data structure conditions. | Linear search is O(n) on any data; binary search is O(log n) but requires the data to be sorted. |
| Should know | Compare and contrast arrays, linked lists, and hash tables based on time efficiency for insertion, deletion, and lookup operations. | Array: O(1) lookup, O(n) insert/delete. Linked list: O(1) insert/delete at a known node, O(n) lookup. Hash table: O(1) average for all three. |
| Should know | Be capable of explaining how to solve a given problem using the appropriate data structure and algorithm | E.g. detect duplicates by adding items to a `HashSet<T>` and checking the boolean returned by `Add`. |

## EF Core

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Create an EF Core model using EF Core code conventions. | `public class Book { public int Id { get; set; } public string Title { get; set; } }` |
| Must know | Use Entity Framework to generate a SQL schema with a code-first approach. | `dotnet ef migrations add Init`<br>`dotnet ef database update` |
| Must know | Use Entity Framework to generate classes from a data source with a data-first approach. | `dotnet ef dbcontext scaffold "ConnString" Microsoft.EntityFrameworkCore.SqlServer` |
| Must know | Explain the role of the DbContext class and how it manages database interactions. | Represents a session with the database; exposes `DbSet<T>` properties, builds queries, and tracks/persists changes via `SaveChanges()`. |
| Must know | Explain how Entity Framework tracks changes in entities and persists them to the database. | The change tracker records entity states (Added/Modified/Deleted) and `SaveChanges()` generates the matching INSERT/UPDATE/DELETE SQL. |
| Must know | Describe the differences between the code-first and data-first approaches and when each should be applied. | Code-first: write C# classes and generate the schema (greenfield). Data-first: scaffold classes from an existing database (legacy schemas). |
| Should know | Create a dbcontext object in an application, and use it to manage persistance to a database. | `using var db = new AppDbContext(); db.Books.Add(book); db.SaveChanges();` |
| Should know | Configure a model using Data Annotations in the model class. | `[Key] public int Id { get; set; }`<br>`[Required] public string Title { get; set; }` |
| Should know | Effectively manages migrations to avoid breaking changes and data loss. | Apply incremental migrations, review the generated SQL, and avoid dropping populated columns/tables without a data-preserving step. |
| Should know | Describe the role of the Fluent API and when it is required instead of Data Annotations. | Configured in `OnModelCreating` for mappings annotations can't express (composite keys, many-to-many): `modelBuilder.Entity<Book>().HasKey(...)`. |
| Nice to Have | Call stored procedures and query  scalar types by dropping down to SQL, using FromSQL() and SqlQuery(). | `db.Books.FromSql($"EXEC GetBooks");`<br>`db.Database.SqlQuery<int>($"SELECT COUNT(*) FROM Books");` |
| Nice to Have | Modify a migration created by Entity Framework before execution. | Edit the generated `Up()`/`Down()` methods (e.g. add a raw `migrationBuilder.Sql(...)`) before running `database update`. |

## C# Multi-threading

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Create and manage threads in a C# application using the Thread class. | `var t = new Thread(Work); t.Start(); t.Join();` |
| Must know | Implement concurrency with the Task Parallel Library (TPL) for improved performance. | `Parallel.For(0, 100, i => Process(i));` |
| Must know | Explain synchronization techniques (e.g., lock, Monitor) and how they prevent race conditions. | `lock (_gate) { _balance += amount; } // serializes access to shared state` |
| Must know | Identify and avoid common multithreading pitfalls such as deadlocks and thread starvation. | Deadlock: two threads each hold a lock the other needs; avoid by acquiring locks in a consistent order. |
| Must know | Describe the lifecycle of a thread and how the runtime schedules execution. | Unstarted -> Running -> (WaitSleepJoin) -> Stopped; the OS scheduler time-slices runnable threads across cores. |
| Must know | Explain how context switching affects performance in multithreaded applications. | Saving/restoring thread state on each switch adds CPU overhead; too many threads cause thrashing and lower throughput. |
| Must know | Explain the difference between parallelism and concurrency in the context of C# multithreading. | Concurrency: tasks make progress by interleaving (even on one core). Parallelism: tasks run literally at the same time on multiple cores. |
| Must know | Implement thread pooling using the ThreadPool class to manage lightweight parallel execution. | `ThreadPool.QueueUserWorkItem(_ => Work());` |
| Should know | Use thread-safe collections (e.g., ConcurrentDictionary, BlockingCollection) in multithreaded applications. | `var map = new ConcurrentDictionary<int, string>(); map.TryAdd(1, "a");` |
| Should know | Implement cancellation and exception handling in multithreaded code. | `var cts = new CancellationTokenSource(); Task.Run(() => Work(cts.Token), cts.Token);` |
| Should know | Compare the performance impact of sequential vs. multithreaded execution. | Multithreading speeds up CPU- or I/O-bound work but adds synchronization overhead; gains are bounded by Amdahl's law. |
| Should know | Apply async / await in combination with multithreading to optimize responsiveness. | `await Task.Run(() => HeavyWork()); // frees the calling thread` |
| Should know | Compare cooperative cancellation vs. preemptive interruption in managing multithreaded workloads. | Cooperative: the task polls a `CancellationToken` and stops gracefully. Preemptive (`Thread.Abort`): forcibly kills the thread, risking corrupt state. |
| Should know | Evaluate trade-offs between different synchronization approaches (e.g., lock vs. Monitor vs. Interlocked). | `Interlocked` is fastest for single atomic ops; `lock`/`Monitor` guard multi-statement critical sections at higher cost. |
| Nice to Have | Explain the use cases and differences between synchronization primitives such as SemaphoreSlim, Mutex, and ReaderWriterLockSlim. | SemaphoreSlim limits concurrent count; Mutex is a named cross-process lock; ReaderWriterLockSlim allows many readers but one writer. |
| Nice to Have | Profile and analyze thread performance using Visual Studio diagnostics or similar tools. | Use the VS Concurrency Visualizer / Threads window to inspect thread states, contention, and CPU usage. |

## ASP.NET Core

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Describe the HTTP pipeline. | A request flows through an ordered chain of middleware (e.g. routing, auth, endpoint) and the response flows back out through the same chain in reverse. |
| Must know | Implement controllers and action methods. | `[ApiController] public class UsersController : ControllerBase { [HttpGet] public IActionResult Get() => Ok(_users); }` |
| Must know | Describe the purpose and types of HTTP response codes. | Status codes signal the result of a request: 2xx success, 3xx redirection, 4xx client error, 5xx server error. |
| Must know | Implement an API service. | Register behind an interface in DI and inject it into the controller: `builder.Services.AddScoped<IUserService, UserService>();` |
| Must know | Demonstrate functional knowledge of Data Transfer Objects, and their use. | DTOs shape the data crossing the API boundary, decoupling the public contract from internal models: `public record UserDto(int Id, string Name);` |
| Must know | Describe and implement a Minimal API endpoint. | `app.MapGet("/users", () => users); // route handler with no controller class` |
| Must know | Describe the function of HTTP method annotations. | Attributes map an action to a verb and route: `[HttpGet("{id}")]`, `[HttpPost]`, `[HttpDelete("{id}")]`. |
| Should know | Implement model binding effectively. | Bind values from route, query, body, and headers: `public IActionResult Get([FromRoute] int id, [FromQuery] int page)` |
| Should know | Describe and implement data validation using annotations. | `[Required] public string Name { get; set; }` is checked automatically; `[ApiController]` returns 400 on `ModelState` failure. |
| Should know | Implement native ASP.NET middleware, such as Logging or Identity. | Add built-in middleware in the pipeline: `app.UseAuthentication(); app.UseAuthorization();` |
| Should know | Implement HTTP response codes effectively. | Return the result that matches the outcome: `return NotFound();`, `return CreatedAtAction(nameof(Get), new { id }, dto);` |
| Nice to Have | Implements third party or custom filters and middleware. | `app.Use(async (ctx, next) => { /* pre */ await next(); /* post */ });` or a class implementing `IActionFilter`. |
| Nice to Have | Demonstrate understanding of caching in an API. | Cache responses to cut repeat work: `[ResponseCache(Duration = 60)]` or `IMemoryCache` for server-side values. |
| Nice to Have | Implement an API which consumes a 3rd party API. | Inject `IHttpClientFactory`, call the external service, and return its data: `var data = await client.GetFromJsonAsync<Rate>("https://api.example.com/rate");` |
