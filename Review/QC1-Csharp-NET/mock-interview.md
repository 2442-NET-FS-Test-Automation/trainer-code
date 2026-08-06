# QC-1 (.NET) — Mock Interview Bank

Practice out loud, then check against the model answer. Each entry carries a **tier badge**
(Must / Should / Nice), a concise **model answer**, the **QC objective it proves**, and a **source**.
Grouped by topic; a **behavioral** section (SDLC/Agile/Git) closes it. Every answer traces to a delivered
source file — nothing here covers a later-week topic.

---

## .NET platform & tooling

**[Must] Walk me through what happens from `.cs` file to running program.**
Model: The C# source compiles to **Intermediate Language (IL)**, a CPU-independent bytecode. At run time
the CLR's **Just-In-Time (JIT)** compiler turns that IL into native machine code for the current machine.
That two-step model is why one build runs cross-platform. `dotnet build` produces the IL assembly;
`dotnet run` builds then executes it.
Proves QC: Can describe the .NET compilation process and its steps.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/dotnet-and-csharp-intro.md`

**[Must] What is the difference between the .NET SDK and the runtime?**
Model: The **runtime** (CLR + base libraries) is what you need to *run* an existing .NET app. The **SDK**
is what you need to *build* one — it includes the runtime plus the compiler and the `dotnet` CLI. As a
developer you install the SDK; you can confirm it with `dotnet --version`.
Proves QC: Can describe the role of the .NET SDK and its use in development.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/dotnet-and-csharp-intro.md`

**[Must] How do you create and run a console app from the CLI?**
Model: `dotnet new console -o MyApp` scaffolds the project (a `.csproj` and a `Program.cs`), then `cd MyApp`
and `dotnet run` compiles and runs it. `dotnet run` builds first, so you don't need a separate
`dotnet build`.
Proves QC: Utilize the dotnet command line tools to generate and execute projects; Can initialize and run a
console application using the .NET CLI.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/dotnet-and-csharp-intro.md`,
`weeklytechrepo/Agile-Git-CoreCSharp/demo/walkthroughs/00-devsetup.md`

**[Should] How do you add a third-party dependency, and how would you organize a multi-project app?**
Model: `dotnet add package <Name>` (e.g. `Serilog`) installs a NuGet package into the project. For a larger
app you use a **solution** (`.sln`) grouping multiple projects — say a console app, a class library for
domain logic, and a test project — and reference one from another with `dotnet add reference
../Lib/Lib.csproj`. The class library is a reusable `.dll` with no entry point.
Proves QC: Uses the NuGet Package Manager to install and manage dependencies; Can organize applications
using solutions and multi-project setups.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/classes-and-projects.md`,
`trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`

---

## C# fundamentals

**[Must] What is control flow, and which loop do you reach for when?**
Model: Control flow decides which statements run and how often — `if`/`else if`/`else` and `switch` to
branch, loops to repeat. Pick the loop by what you know: `for` when you know the count, `while` when you
repeat on a condition checked first, `do/while` when the body must run at least once, `foreach` to walk
every item in a collection without managing an index.
Proves QC: Utilizes control flow where appropriate to achieve desired behavior during runtime.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/csharp-basics-overview.md`

**[Must] How do you convert a string of user input to a number safely?**
Model: Prefer `int.TryParse(input, out int n)` — it returns `false` on bad text and gives you `0` instead
of throwing, so the program doesn't crash. `int.Parse` and `Convert.ToInt32` throw on invalid input, which
is fine when you trust the source but risky for raw user input. An explicit cast `(int)someDouble`
truncates rather than parses.
Proves QC: Implement type conversion in an application; Can identify and use basic data types appropriately.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/csharp-basics-overview.md`

**[Must] Why use methods, and what does `==` compare?**
Model: Methods name a reusable block of logic so you don't repeat yourself, and they create a testable seam.
`==` compares **values** for value types, but **references** for objects (same object?) — `string` is the
exception that compares text. That value-vs-reference distinction on `==` is a classic bug source.
Proves QC: Create methods that allow for the reusability of code; Can use basic, comparison, equality, and
logical operators in programming logic.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/csharp-basics-overview.md`

**[Nice] What is recursion and what must every recursive method have?**
Model: Recursion is a method that calls itself to solve a smaller piece of the problem each time. It must
have a **base case** that returns without recursing — otherwise the calls never stop and you get a
`StackOverflowException`. `Factorial(n) => n <= 1 ? 1 : n * Factorial(n - 1)` stops at `n <= 1`.
Proves QC: Implement recursion in an application.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/csharp-basics-overview.md`

---

## Memory model

**[Must] Explain value vs reference types and stack vs heap.**
Model: **Value types** (`int`, `bool`, `struct`) hold their data directly and live on the **stack**;
assigning one copies the value, so the copy is independent. **Reference types** (`string`, arrays,
classes) store their data on the **heap**, and the stack holds a reference (address) to it; assigning one
copies the reference, so both names point at the same object. The stack is fast and auto-cleaned when a
method returns.
Proves QC: Can differentiate between value and reference types, and describe stack vs. heap allocation.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/csharp-basics-overview.md`,
`trainer-code/Notes/C-Sharp/Intermediate-C#/collections-overview.md`

**[Should] How does garbage collection work, and how do you avoid leaks?**
Model: You `new` objects onto the managed heap and never free them — the GC reclaims objects that become
**unreachable**, on its own schedule. Objects start in Gen 0 (collected often, cheaply); survivors promote
to Gen 1 then Gen 2. Setting a reference to `null` just drops one reference; it doesn't free memory.
Leaks in managed code come from references you never release — unsubscribed events, or unmanaged resources
(files, sockets, DB connections) you never dispose; use `using`/`IDisposable` for those.
Proves QC: Can explain how garbage collection works in .NET and avoid common memory leaks.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/advanced-classes.md`

---

## Object-oriented programming

**[Must] What are the four pillars of OOP?**
Model: **Encapsulation** — bundle data with the methods that act on it and hide internals behind a
controlled surface (private state, validated through properties/methods). **Inheritance** — a derived class
reuses and specializes a base ("is-a"). **Polymorphism** — a base-typed reference runs the right derived
behavior at runtime via `virtual`/`override`. **Abstraction** — expose what an object does, hide how, through
abstract classes and interfaces.
Proves QC: Understands and can explain the four pillars of OOP.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/oop-pillars.md`,
`weeklytechrepo/Agile-Git-CoreCSharp/demo/walkthroughs/02-oop.md`

**[Must] How do you model a real-world entity as a class?**
Model: A class is a blueprint with four member kinds: **fields** (private state), **properties** (controlled
access, often auto-properties), **methods** (behavior), and **constructors** (set initial state). You model,
say, a `Book` with a `Title`/`Author` property, a constructor that takes them, and methods like
`Describe()`; then create objects with `new Book("Clean Code", "Martin")`.
Proves QC: Can model real-world entities using classes, fields, methods, and constructors.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/classes-and-projects.md`

**[Should] Interface vs abstract class — when do you use each?**
Model: An **interface** defines a capability ("can do"), has no implementation or state traditionally, and a
class can implement **many** — use it for a behavior several unrelated types share (`ILendable`). An
**abstract class** defines a base identity ("is a"), can include implemented members and state, and a class
derives from **one** — use it when subclasses share state or implemented behavior. Rule of thumb: capability
across unrelated types -> interface; shared base with common code -> abstract class.
Proves QC: Can describe the difference between an Interface and Abstract class, and can appropriately
leverage either as needed in their program.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/oop-pillars.md`

**[Should] What is the difference between `override` and `new`, and overloading vs overriding?**
Model: `override` (with `virtual`) is true polymorphism — the derived version runs even through a
base-typed reference. `new` *hides* the base member — which version runs depends on the reference type, not
the object, which is usually a mistake. **Overloading** is same method name with different parameter lists
in the same class (resolved at compile time); **overriding** replaces a `virtual` base method in a derived
class (resolved at runtime).
Proves QC: Use inheritance and polymorphism to create classes that have inherited members, and overrides or
overloads members as necessary.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/oop-pillars.md`

**[Should] What is a primary constructor, and when do you reach for one?**
Model: A primary constructor declares the constructor parameters right on the class header —
`public class Member(string name, int id) { public string Name { get; } = name; }`. The parameters are in
scope throughout the body, cutting field-assignment boilerplate. Reach for it on small data-holding
classes; keep a conventional constructor when you need validation, multiple constructors, or extra setup.
Proves QC: Understands the Primary Constructor syntax for classes.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/classes-and-projects.md`

**[Should] Static vs instance members?**
Model: An **instance** member belongs to a specific object — each object has its own copy (`book.Title`). A
**static** member belongs to the **class itself**, shared across all instances, accessed through the type
name (`Book.TotalCreated`). Use static for shared counters, constants, and utility helpers; use instance
members for per-object state.
Proves QC: Can differentiate between static and instance members and explain when to use each.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/classes-and-projects.md`,
`trainer-code/Notes/C-Sharp/Intro-OOP/oop-pillars.md`

**[Nice] How do you simulate multiple inheritance in C#?**
Model: C# allows only single class inheritance, but a class can implement **multiple interfaces** —
`public class Drone : IFlyable, ICamera` — so it satisfies several contracts at once. That is how you get
multiple-inheritance-like behavior without the diamond problem.
Proves QC: Able to simulate multiple inheritance.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/oop-pillars.md`

---

## Collections & generics

**[Must] Describe the purpose and differences of the main collections.**
Model: Collections store objects; pick by access pattern. An **array** is fixed-size; a **`List<T>`** is
dynamic and indexable; a **`Dictionary<K,V>`** stores key-value pairs for O(1) lookup. **`Stack<T>`** is
LIFO (`Push`/`Pop`), **`Queue<T>`** is FIFO (`Enqueue`/`Dequeue`), **`HashSet<T>`** enforces uniqueness with
O(1) membership. `List<T>` is the default; the others encode a specific discipline.
Proves QC: Describe the purpose and differences of collections; Demonstrates understanding of data
structures and collections in C#.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/collections-overview.md`,
`trainer-code/Notes/C-Sharp/Intermediate-C#/advanced-classes.md`

**[Must] What do generics give you?**
Model: A generic type takes a type parameter filled in at the use site — `List<LibraryItem>` is "a list of
library items," checked at compile time. You get one implementation that works for any element type, with
full type safety (add a `string` to a `List<int>` and it won't compile) and no boxing. The built-in
collections are themselves generic classes; you can write your own, like `Shelf<T>`.
Proves QC: Demonstrate understand of Generic Types in C#.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/collections-overview.md`

**[Should] What is `IEnumerable<T>` and what does a lambda buy you?**
Model: `IEnumerable<T>` is the contract behind `foreach` — implement `GetEnumerator()` (often with
`yield return`) and the type is iterable, lazily. A **lambda** is an inline anonymous function; passed as a
`Predicate<T>` it lets a method take *behavior* as an argument — `catalog.Find(i => i.Author == "Martin")`
lets the caller decide the test. The same idea sorts via `List.Sort` with a `Comparison<T>` lambda.
Proves QC: Utilize the collections namespace, and types that extend the IEnumerable interface in an
application; Implement lambda expressions in an application.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/advanced-classes.md`

**[Nice] Show a lambda that filters and one that sorts.**
Model: Filter: `catalog.Find(item => item.Author == "Robert C. Martin")` returns the matches. Sort:
`items.Sort((a, b) => string.Compare(a.Title, b.Title))` orders A-Z; `items.Sort((a, b) => b.Title.Length
- a.Title.Length)` orders by descending title length. Same lambda mechanism — one decides membership, the
other decides order. (LINQ `OrderBy`/`Where` wraps this, but is a later topic.)
Proves QC: Implement a lambda expression to perform filters and sorts.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/advanced-classes.md`

---

## Exceptions & debugging

**[Must] How does try/catch/finally work, and what's the catch order rule?**
Model: `try` wraps risky work; `catch` handles a thrown exception; `finally` always runs — even after a
throw — so cleanup goes there. Catch **most specific first, base last**: a base `catch` placed before its
subtype makes the specific one unreachable and won't compile.
Proves QC: Uses Try-Catch-Finally to avoid hard crashing when running "risky" operations.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`

**[Must] How do you read a stack trace to debug?**
Model: Read it **top to bottom**. The top frame is where the exception was actually raised — it names the
type, method, source file, and line number, so that's where you look first. Each frame below is the caller
that led there, down to `Main`, telling you how you got there. When you log a failure, log
`ex.ToString()` (the full trace), not just `ex.Message`.
Proves QC: Effectively interprets stack traces in order to debug code files.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`

**[Should] When do you throw versus return, and what does bubbling mean?**
Model: A **routine, expected** outcome returns a value (e.g. `false` for "no copies"). A **broken
assumption** — like an id that doesn't exist — should **throw**, failing loud and near the cause, because
returning `null` plants a landmine downstream. **Bubbling** is the exception propagating up the call stack
until something catches it. Rethrow with bare `throw;` to preserve the original trace; `throw ex;` resets
it.
Proves QC: Leverages manually thrown exceptions and bubbling to debug business logic.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`

**[Nice] Why and how do you write a custom exception?**
Model: A custom exception is a class deriving from `Exception` (or your own base). It lets a caller catch a
specific failure or all your domain errors with one `catch`, and it can **carry data**, not just a message
— e.g. `ItemNotFoundException` exposes the missing `Id`. Define `public class ItemNotFoundException :
Exception { public int Id { get; } public ItemNotFoundException(int id) : base($"No item {id}") => Id =
id; }`.
Proves QC: Can create custom exceptions in their applications to fit specific use cases.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`

---

## Design patterns & SOLID

**[Must] What is a design pattern, and what are the SOLID principles?**
Model: A design pattern is a standardized, reusable solution to a commonly occurring problem in software
design. **SOLID** is five maintainability principles: **S**ingle Responsibility (one job per type),
**O**pen/Closed (open to extension, closed to modification), **L**iskov Substitution (a subtype works
wherever its base does), **I**nterface Segregation (small focused interfaces), **D**ependency Inversion
(depend on abstractions, not concretes).
Proves QC: Describe the purpose of a design pattern; Describe and utilize SOLID principles in application
design.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`

**[Should] Describe the repository, singleton, and unit-of-work patterns.**
Model: **Repository** — an abstraction between data-access and business logic; callers depend on an
interface (`ILibraryRepository`), so the storage can change without touching them (Dependency Inversion).
**Singleton** — restricts a class to one shared instance with a static access point (Serilog's static
`Log` is one). **Unit of Work** — groups a set of changes and commits them together as a single
transaction (all land, or none do).
Proves QC: Describe the repository / singleton / unit-of-work design pattern.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`

**[Should] How would you implement the repository pattern?**
Model: Define the contract as an interface — `interface ILibraryRepository { void Add(LibraryItem i);
LibraryItem GetById(int id); }` — then write a concrete implementation, e.g. `InMemoryLibraryRepository`
backed by a `List` (later a `Dictionary`, or a real database). Callers type their variable as the
interface, so you can swap the implementation freely. That's also where a real DB slots in later.
Proves QC: Implement the Repository pattern in an application; Demonstrate the implementation of a design
pattern.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`,
`weeklytechrepo/Intermediate-CSharp/demo/walkthroughs/04-exceptions-patterns-logging.md`

---

## Async, networking & language round-out

**[Must] How do you make an HTTP call, and why use async/await?**
Model: Use a single shared `HttpClient` (a new one per call leaks OS sockets) and `await` its async methods:
`string json = await Http.GetStringAsync(url);`. `async`/`await` is about **not blocking the thread while
waiting on I/O** — while the request is in flight, `await` hands the thread back so other work runs. It's
not about computing faster. Never call `.Result`/`.Wait()` (they deadlock); `await` all the way up to
`Main`, which becomes `async Task`.
Proves QC: Utilize the HttpClient object to make HTTP calls to external APIs; Program asynchronously in C#
using async and await.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/async-http-networking-regex.md`

**[Should] What are nullable types and how do you handle the null safely?**
Model: A nullable value type `int?` can hold a value or `null`. **Lifted operators** propagate null through
arithmetic (`null * 2 == null`), `?.` short-circuits a member access on null, and `??` supplies a default
(`int copies = maybe ?? 0;`). Check `HasValue` or use `??` before treating it as a plain value.
Proves QC: Implement nullable types in an application.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/async-http-networking-regex.md`

**[Nice] How do you validate input shape with regex?**
Model: Use a verbatim string so backslashes stay literal, and **anchor** with `^` and `$` so the whole
string must match — `Regex.IsMatch(isbn, @"^\d{13}$")` accepts exactly 13 digits. Without anchors a partial
match passes, a common bug. To pull values out, use a capturing group and read `m.Groups[1].Value` (group 0
is the whole match). Escape metacharacters you mean literally, e.g. `\.` for a real dot.
Proves QC: Demonstrate an understanding of REGEX and pattern matching syntax.
Source: `trainer-code/Notes/C-Sharp/Intermediate-C#/async-http-networking-regex.md`

---

## Behavioral (SDLC / Agile / Git)

**[Must] Tell me about your development process / how does a team build software?**
Model: We work in **Agile**, in short 1–2 week sprints, each touching all SDLC phases (a little design,
code, test, review) and producing a working increment — versus **Waterfall**, which runs requirements ->
design -> implementation -> testing -> deployment -> maintenance once in strict sequence. We use **Scrum**:
the Product Owner prioritizes the backlog, the Scrum Master removes blockers, the Dev Team builds. We pull
work from a sprint backlog, sync at a daily standup (what I did / will do / blockers), demo at the sprint
review, and improve at the retro. Work is tracked on a To Do / In Progress / Done board.
Proves QC: Create a functional application to fulfill behavioural requirements and user stories
(process context for delivering to requirements).
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/sdlc-agile.md`

**[Must] How do you turn a requirement into something you build to?**
Model: A requirement becomes a **user story** — *"As a [user], I want [goal] so that [benefit]"* — with
**acceptance criteria** that define done. I build to those criteria, not my own idea of done. In the Week-1
kata, for instance, the acceptance checklist (a runnable menu loop, seeded entities, the OOP pillars) was
the contract my PR was graded against.
Proves QC: Create a functional application to fulfill behavioural requirements and user stories.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/sdlc-agile.md`,
`weeklytechrepo/Agile-Git-CoreCSharp/async-lab/core-csharp-kata-README.md`

**[Should] Walk me through your everyday Git workflow.**
Model: Edit files, then `git status` to see what changed, `git add` to stage, `git commit -m "Add ..."`
with an imperative message that says *why*, and `git push` to the remote (`origin` on GitHub). A file moves
working directory -> staging area -> repository. I add a `.gitignore` (`bin/`, `obj/`, `.vs/`) before the
first commit so build output is never tracked. The first push is `git push -u origin main`; after that just
`git push`. Each deliverable is submitted as a commit or pull request.
Proves QC: (supports) Create a functional application to fulfill behavioural requirements and user stories —
delivery via version control.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/git-fundamentals.md`

**[Should] Why version control at all — what does Git solve?**
Model: Without it, "saving" means overwriting yesterday's file and collaboration means emailing
`final_v2.zip` around. Git is distributed version control: every clone has the full history, so you can work
offline, review past states, see who changed what and why, and combine work from many people without
overwriting each other. It's the universal "save game with history" for code.
Proves QC: (supports) functional-application delivery and team workflow.
Source: `trainer-code/Notes/C-Sharp/Intro-OOP/git-fundamentals.md`
