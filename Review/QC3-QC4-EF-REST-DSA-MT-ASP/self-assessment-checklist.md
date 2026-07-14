# QC-3 + QC-4 — Self-Assessment Checklist

Every objective below is reproduced **verbatim** from `qc-criteria/QC-3-EF-Core-REST-DSA-MT.md` (REST,
DSA, EF Core, C# Multi-threading) and `qc-criteria/QC-4-ASP-NET-CORE.md` (ASP.NET Core), grouped by
priority tier and section. Read each as a self-question — *"Can I confidently do / explain this without
notes?"* Tick only what you can do unaided; every unticked box points you at the matching cluster in
`study-guide.md`, task in `drills.md`, and question in `mock-interview.md`.

## Must know

### REST (QC-3)
- [ ] Describe the purpose of REST and RESTful design.
- [ ] Describe the purpose of HTTP messaging.
- [ ] Describe the HTTP request/response lifecycle.
- [ ] Describe HTTP request methods (verbs).
- [ ] Describe HTTP response code classes (1xx,2xx,3xx,etc.)
- [ ] Describe the REST principles.
- [ ] Be capable of sending a GET request to an open source REST API using curl or Postman

### DSA (QC-3)
- [ ] Describe algorithm complexity using Big O notation and analyze the efficiency of an algorithm or data structure
- [ ] Explain the list abstract data type and its common operations
- [ ] Explain the stack and queue abstract data types and their common operations
- [ ] Explain the hash table abstract data type and its common operations
- [ ] Demonstrate the ability to perform linear search on arrays or lists and be able to identify the syntax.
- [ ] Analyze a given problem and determine the appropriate data structures and algorithms to use.
- [ ] Read, interpret, and debug existing code that utilizes data structures and algorithms, identifying their efficiency and purpose.
- [ ] Demonstrate the ability to perform binary search on arrays or lists and be able to identify the syntax.
- [ ] Explain the structure of tree and graph data structures
- [ ] Describe the difference between common sorting algorithms

### EF Core (QC-3)
- [ ] Create an EF Core model using EF Core code conventions.
- [ ] Use Entity Framework to generate a SQL schema with a code-first approach.
- [ ] Use Entity Framework to generate classes from a data source with a data-first approach.
- [ ] Explain the role of the DbContext class and how it manages database interactions.
- [ ] Explain how Entity Framework tracks changes in entities and persists them to the database.
- [ ] Describe the differences between the code-first and data-first approaches and when each should be applied.

### C# Multi-threading (QC-3)
- [ ] Create and manage threads in a C# application using the Thread class.
- [ ] Implement concurrency with the Task Parallel Library (TPL) for improved performance.
- [ ] Explain synchronization techniques (e.g., lock, Monitor) and how they prevent race conditions.
- [ ] Identify and avoid common multithreading pitfalls such as deadlocks and thread starvation.
- [ ] Describe the lifecycle of a thread and how the runtime schedules execution.
- [ ] Explain how context switching affects performance in multithreaded applications.
- [ ] Explain the difference between parallelism and concurrency in the context of C# multithreading.
- [ ] Implement thread pooling using the ThreadPool class to manage lightweight parallel execution.

### ASP.NET Core (QC-4)
- [ ] Describe the HTTP pipeline.
- [ ] Implement controllers and action methods.
- [ ] Describe the purpose and types of HTTP response codes.
- [ ] Implement an API service.
- [ ] Demonstrate functional knowledge of Data Transfer Objects, and their use.
- [ ] Describe and implement a Minimal API endpoint.
- [ ] Describe the function of HTTP method annotations.

## Should know

### REST (QC-3)
- [ ] Describe URL conventions when using REST.
- [ ] Describe SOA (Service Oriented Architecture) and be capable of diagramming the components of a sample system
- [ ] Describe the difference between authorization and authentication.
- [ ] Be capable of sending a POST request to a REST API using curl or Postman and populating the request body
- [ ] Build a RESTful web service using a popular framework (e.g. Spring, Flask, Express)

### DSA (QC-3)
- [ ] Explain when to choose a stack, queue, or priority queue based on program requirements.
- [ ] Evaluate the trade-offs between linear search and binary search in terms of time complexity and required data structure conditions.
- [ ] Compare and contrast arrays, linked lists, and hash tables based on time efficiency for insertion, deletion, and lookup operations.
- [ ] Demonstrate the ability to perform bubble sort on arrays or lists and be able to identify the syntax.
- [ ] Be capable of explaining how to solve a given problem using the appropriate data structure and algorithm
- [ ] Demonstrate the ability to perform insertion sort on arrays or lists and be able to identify the syntax.
- [ ] Demonstrate the ability to perform selection sort on arrays or lists and be able to identify the syntax.

### EF Core (QC-3)
- [ ] Create a dbcontext object in an application, and use it to manage persistance to a database.
- [ ] Configure a model using Data Annotations in the model class.
- [ ] Effectively manages migrations to avoid breaking changes and data loss.
- [ ] Describe the role of the Fluent API and when it is required instead of Data Annotations.

### C# Multi-threading (QC-3)
- [ ] Use thread-safe collections (e.g., ConcurrentDictionary, BlockingCollection) in multithreaded applications.
- [ ] Implement cancellation and exception handling in multithreaded code.
- [ ] Compare the performance impact of sequential vs. multithreaded execution.
- [ ] Apply async / await in combination with multithreading to optimize responsiveness.
- [ ] Compare cooperative cancellation vs. preemptive interruption in managing multithreaded workloads.
- [ ] Evaluate trade-offs between different synchronization approaches (e.g., lock vs. Monitor vs. Interlocked).

### ASP.NET Core (QC-4)
- [ ] Implement model binding effectively.
- [ ] Describe and implement data validation using annotations.
- [ ] Demonstrate the use of automatic mapping for objects and DTOs.
- [ ] Implement native ASP.NET middleware, such as Logging or Identity.
- [ ] Implement HTTP response codes effectively.

## Nice to Have

### REST (QC-3)
- [ ] Implement authentication and authorization using a popular RESTful framework (e.g. OAuth, JWT)
- [ ] Compare and contrast RESTful and SOAP-based web services in terms of functionality, performance, and scalability

### DSA (QC-3)
- [ ] Demonstrate the ability to perform merge sort on arrays or lists and be able to identify the syntax.
- [ ] Explain the concept of recursion and identify the base case and recursive case in a method.
- [ ] Demonstrate how to solve simple problems using recursion
- [ ] Demonstrate how to optimize algorithms using memoization and tabulation

### EF Core (QC-3)
- [ ] Call stored procedures and query  scalar types by dropping down to SQL, using FromSQL() and SqlQuery().
- [ ] Modify a migration created by Entity Framework before execution.

### C# Multi-threading (QC-3)
- [ ] Explain the use cases and differences between synchronization primitives such as SemaphoreSlim, Mutex, and ReaderWriterLockSlim.
- [ ] Profile and analyze thread performance using Visual Studio diagnostics or similar tools.

### ASP.NET Core (QC-4)
- [ ] Implements third party or custom filters and middleware.
- [ ] Demonstrate understanding of caching in an API.
- [ ] Implement an API which consumes a 3rd party API.

## Not yet covered

*(empty — every objective above was taught or is note-covered; see `out-of-scope-register.md` for the
three demo-waived Nice-to-have items, which are written-notes-only.)*
