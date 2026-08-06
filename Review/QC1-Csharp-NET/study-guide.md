# QC-1 (.NET) — Study Guide

Organized by topic cluster. Within each: the QC-1 objectives covered (with tier), a concept recap
synthesized from the trainer notes and generated content (with **source pointers**), key points and
pitfalls lifted from the notes, and one annotated **worked example** cited from a demo walkthrough or the
`core-csharp-kata` thread repo.

Source roots:
- Trainer notes: `trainer-code/Notes/C-Sharp/{Intro-OOP, Intermediate-C#}/`
- Content notes: `weeklytechrepo/Agile-Git-CoreCSharp/content/`, `weeklytechrepo/Intermediate-CSharp/content/`
- Demo scripts: `weeklytechrepo/{week}/demo/walkthroughs/`
- Thread repo (end-state): `weeklytechrepo/Agile-Git-CoreCSharp/demo/core-csharp-kata/`

---

## 1. .NET Platform & Tooling

**Objectives covered**
- *(Must)* Utilize the dotnet command line tools to generate and execute projects.
- *(Must)* Can describe the role of the .NET SDK and its use in development.
- *(Must)* Can initialize and run a console application using the .NET CLI.
- *(Must)* Can describe the .NET compilation process and its steps.
- *(Should)* Uses the NuGet Package Manager to install and manage dependencies.
- *(Should)* Can organize applications using solutions and multi-project setups.
- *(Nice)* Can create and use their own reusable utility or helper class library.
- *(Nice)* Can construct and use class libraries and reference them in multi-project solutions.

**Concept recap** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/dotnet-and-csharp-intro.md`, `.../classes-and-projects.md`)*
- **.NET** is a cross-platform platform: a **runtime** (the CLR), a standard library (the Base Class
  Library), and languages — chiefly **C#**.
- **SDK vs runtime:** the runtime *runs* a .NET app; the **SDK** *builds* apps (it bundles the runtime +
  compiler + the `dotnet` CLI). Developers install the SDK; verify with `dotnet --version`.
- **Compilation process:** C# source (`.cs`) is compiled to **Intermediate Language (IL)**; at run time
  the CLR's **Just-In-Time (JIT)** compiler turns IL into native machine code. This two-step model is why
  the same build runs on Windows, macOS, and Linux. *(QC example column,
  `qc-criteria/QC-1-NET.md`)*
- **Core CLI loop:** `dotnet new console -o App` -> `dotnet build` -> `dotnet run`. `dotnet new console`
  scaffolds a `.csproj` (project settings/dependencies) and a `Program.cs`.
- **NuGet** adds dependencies: `dotnet add package Serilog` (the Week-2 demo installs Serilog this way —
  `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`).
- **Solution -> project -> namespace -> class -> members** is the nesting. A **solution** (`.sln`) groups
  projects; a **console app** has an entry point (`Main`); a **class library** produces a reusable `.dll`
  with no entry point that other projects reference (`dotnet add reference ...`).

**Key points / pitfalls**
- Don't confuse SDK and runtime — installing only the runtime means you cannot build.
- `dotnet run` implicitly builds first; you do not have to call `dotnet build` separately to run.
- A class library has **no** `Main` — it cannot be `dotnet run` directly; a console app references it.

**Worked example** *(cited: `weeklytechrepo/Agile-Git-CoreCSharp/demo/walkthroughs/00-devsetup.md`)*
```bash
dotnet --version                  # confirm the SDK is installed
dotnet new console -o HelloApp    # scaffold: creates HelloApp.csproj + Program.cs
cd HelloApp
dotnet run                        # compile (source -> IL -> JIT -> native) and run
```
The `00-devsetup` walkthrough stands up exactly this loop live, then `git init` + push the result. The
`core-csharp-kata` thread (`weeklytechrepo/Agile-Git-CoreCSharp/demo/core-csharp-kata/`) is the project it
creates and grows for the rest of the cohort.

---

## 2. C# Fundamentals (types, operators, control flow, methods)

**Objectives covered**
- *(Must)* Create methods that allow for the reusability of code.
- *(Must)* Demonstrate proper syntax for working with arrays.
- *(Must)* Utilizes control flow where appropriate to achieve desired behavior during runtime.
- *(Must)* Can identify and use basic data types appropriately.
- *(Must)* Can use basic, comparison, equality, and logical operators in programming logic.
- *(Must)* Implement type conversion in an application.
- *(Nice)* Implement implicit typing using "var."
- *(Nice)* Implement recursion in an application.

**Concept recap** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/csharp-basics-overview.md`)*
- C# is **statically typed**: every variable's type is fixed at compile time. **Value types** (`int`,
  `long`, `double`, `decimal`, `bool`, `char`) hold data directly; **reference types** (`string`, arrays,
  classes) hold a reference. `var` infers a concrete static type from the right-hand side — it is not
  "any."
- **Operators:** arithmetic (`+ - * / %` — integer `/` truncates, `%` is remainder), comparison
  (`< > <= >=`), equality (`==`/`!=`), logical (`&& || !`, short-circuiting). `==` compares *values* for
  value types and *references* for objects — `string` is the friendly exception that compares text.
- **Control flow:** `if`/`else if`/`else`, `switch`, and the loop family — `for` (known count), `while`
  (condition first), `do/while` (runs once), `foreach` (every item, no index).
- **Type conversion:** explicit cast `(int)d` (truncates), `Convert.ToInt32("42")`, and the safe
  `int.TryParse(input, out int n)` for user input.
- **Methods** make logic reusable and testable; name them as verbs. A method may call **itself**
  (**recursion**) — it needs a **base case** or it overflows the stack.
- **Arrays** are fixed-size, zero-indexed, single-type, stored on the heap; indexing past the end throws
  `IndexOutOfRangeException`.

**Key points / pitfalls**
- Prefer `int.TryParse` over `int.Parse` for user input — `Parse` throws on bad text, `TryParse` returns
  `false` and lets you handle it.
- Integer division surprises beginners: `7 / 2 == 3`, not `3.5`. Cast to `double` first for a real
  quotient.
- `==` on two non-string objects asks "same object?", not "same contents" — a classic bug.
- Recursion without a base case throws `StackOverflowException`; for plain counting, a `for` loop is
  clearer and cheaper.

**Worked example** *(cited: `weeklytechrepo/Agile-Git-CoreCSharp/demo/walkthroughs/01-basics.md`; recursion folded in per `docs/status/QC1-Coverage-Analysis.md` section 4)*
```csharp
int[] checkouts = { 3, 1, 4, 1, 5 };                      // array literal
Console.WriteLine($"Total: {Sum(checkouts)}");            // method reuse + interpolation

int Sum(int[] values)                                     // reusable method
{
    int total = 0;
    foreach (int n in values) total += n;                 // foreach over an array
    return total;
}

int Factorial(int n) => n <= 1 ? 1 : n * Factorial(n - 1); // recursion: base case n<=1
```
The `01-basics` walkthrough types this against the kata; the `Factorial` recursion case is the QC "Nice"
item folded into `csharp-basics-overview.md`.

---

## 3. Memory Model (value vs reference, stack vs heap, GC, boxing)

**Objectives covered**
- *(Must)* Can differentiate between value and reference types, and describe stack vs. heap allocation.
- *(Should)* Can explain how garbage collection works in .NET and avoid common memory leaks.

**Concept recap** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/csharp-basics-overview.md`, `.../Intermediate-C#/advanced-classes.md`, `.../async-http-networking-regex.md`)*
- The **stack** stores local variables and value types — fast, auto-cleaned when a method returns. The
  **heap** stores objects (reference types); a stack variable holds a *reference* (address) into the heap.
  So `int x = 5;` puts `5` on the stack, while `string s = "hi";` puts the reference on the stack and the
  data on the heap.
- **Value vs reference semantics:** assigning a value type copies the data (the copy is independent);
  assigning a reference type copies the reference (both names see the same object).
- **Garbage collection:** in C# you `new` objects and never `free` them — the GC reclaims what becomes
  unreachable, on its own schedule. Setting `reference = null` just drops one reference; it does not free
  anything. Objects start in **Gen 0** (collected often, cheaply); survivors promote to Gen 1 then Gen 2.
  Most objects die young.
- **Memory leaks** in managed code come from references you never release — e.g. not unsubscribing from
  events, or holding `IDisposable`/unmanaged resources (files, sockets, DB connections) open. Use
  `using`/`IDisposable` for unmanaged resources; for pure managed memory you do nothing.
- **Boxing** copies a value type onto the heap when assigned to `object`; **unboxing** copies it back (to
  the *exact* type). Generics exist to avoid boxing (`List<int>` never boxes; the old `ArrayList` did).

**Key points / pitfalls**
- "Observe, do not manage" — you do not call the GC for normal code.
- `reference = null` is not "free"; it removes a reference so the object *can* become collectable.
- Unbox to the exact boxed type: `object boxed = 5; int n = (int)boxed;` — not `(long)`.

**Worked example** *(cited: `weeklytechrepo/Intermediate-CSharp/demo/walkthroughs/03-collections.md` — value-vs-reference with a struct)*
```csharp
public readonly struct ShelfLocation     // value type: copy is independent
{
    public int Aisle { get; }
    public int Shelf { get; }
    public ShelfLocation(int aisle, int shelf) => (Aisle, Shelf) = (aisle, shelf);
}

var a = new ShelfLocation(1, 2);
var b = a;                                // copies the data
// mutating b would not touch a — struct assignment copies the value
// a class assignment would copy the reference, and both names would see one object
```

---

## 4. Object-Oriented Programming

**Objectives covered**
- *(Must)* Understands and can explain the four pillars of OOP.
- *(Must)* Can model real-world entities using classes, fields, methods, and constructors.
- *(Should)* Use encapsulation and abstration in applications, with appropriate access modifiers and modifiers on classes and methods.
- *(Should)* Use inheritance and polymorphism to create classes that have inherited members, and overrides or overloads members as necessary.
- *(Should)* Understands auto-property syntax for class fields.
- *(Should)* Can describe the difference between an Interface and Abstract class, and can appropriately leverage either as needed in their program.
- *(Should)* Understands the Primary Constructor syntax for classes.
- *(Should)* Can differentiate between static and instance members and explain when to use each.
- *(Nice)* Able to simulate multiple inheritance.
- *(Nice)* Can use partial classes to split functionality across multiple files.
- *(Nice)* Can apply sealed classes to enforce class design decisions.

**Concept recap** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/classes-and-projects.md`, `.../oop-pillars.md`, `.../Intermediate-C#/advanced-classes.md`)*
- A **class** is a blueprint; an **object** is an instance made with `new`. Four member kinds: **fields**
  (state, usually `private`), **properties** (controlled access via `get`/`set`; auto-property
  `public string Name { get; set; }` generates the backing field), **methods** (behavior), and
  **constructors** (initialization; overload them, and chain with `: this(...)`).
- **Primary constructor:** declare ctor parameters on the class header —
  `public class Member(string name, int id) { public string Name { get; } = name; }` — less ceremony for
  small data-holders.
- **The four pillars:**
  - **Encapsulation** — hide state behind a controlled surface; validate in a property `set` or a method
    so the object protects its own invariants.
  - **Inheritance** — `Book : Item` reuses base members ("is-a"); reach the parent with `base`.
  - **Polymorphism** — a base-typed reference calls the right override at runtime (`virtual` / `override`);
    test-and-cast with `is`/`as`.
  - **Abstraction** — expose *what*, hide *how*, via abstract classes and interfaces.
- **Interface vs abstract class:** an interface is a capability ("can do"), no state, a class implements
  **many**; an abstract class is a base identity ("is a"), can hold implemented members and state, a class
  derives from **one**. A `virtual` method has a default body (override optional); an `abstract` method has
  no body (override required).
- **Access modifiers:** `public` (anyone) > `internal` (same assembly) > `protected` (this class +
  subclasses) > `private` (this class, the default). Default to the tightest that works.
- **`static`** members belong to the class, shared across all instances; **instance** members are
  per-object.
- **`override` vs `new`:** `override` is true polymorphism (runs through a base reference); `new` merely
  *hides* (which runs depends on the reference type) — usually a mistake unless intended.
- **Overloading vs overriding:** overloading = same name, different parameters, same class (compile-time);
  overriding = replacing a `virtual` base method in a derived class (runtime).
- **Simulate multiple inheritance** by implementing multiple interfaces (`class Book : Item, ILendable`).
  **`partial`** splits one class across files; **`sealed`** forbids inheritance on a leaf type
  (`advanced-classes.md`).

**Key points / pitfalls**
- A base `catch`/cast placed before a more specific one hides it — same idea applies to interface vs
  abstract design: pick interface for a shared capability across unrelated types, abstract class for shared
  state/implementation.
- `new` to "override" is a silent bug: through a base reference the base version runs. Use `virtual` +
  `override`.
- Use a primary constructor for simple data holders; keep a conventional constructor body when you need
  validation, multiple constructors, or extra setup.
- Splitting a class with `partial` does not fix a class doing too many jobs — that is still a
  Single-Responsibility problem.

**Worked example (all four pillars)** *(cited: `weeklytechrepo/Agile-Git-CoreCSharp/demo/walkthroughs/02-oop.md`, commit `02-oop` on `core-csharp-kata`)*
```csharp
public abstract class Item                        // Abstraction + Encapsulation
{
    public string Title { get; }
    protected Item(string title) => Title = title; // protected: subclasses only
    public abstract string Describe();             // subclasses MUST implement
}

public class Book : Item                          // Inheritance: Book is-a Item
{
    public int PageCount { get; }
    public Book(string title, int pageCount) : base(title) => PageCount = pageCount;
    public override string Describe() => $"Book: {Title} ({PageCount} pages)"; // Polymorphism
}

Item[] catalog = { new Book("Clean Code", 464), new Dvd("Inception") };
foreach (Item item in catalog) Console.WriteLine(item.Describe()); // right override at runtime
```

---

## 5. Collections & Generics

**Objectives covered**
- *(Must)* Describe the purpose and differences of collections.
- *(Must)* Demonstrates understanding of data structures and collections in C#.
- *(Must)* Demonstrate understand of Generic Types in C#.
- *(Should)* Utilize the collections namespace, and types that extend the IEnumerable interface in an application.
- *(Should)* Implement lambda expressions in an application.
- *(Nice)* Implement a lambda expression to perform filters and sorts.

**Concept recap** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/collections-overview.md`, `.../advanced-classes.md`)*
- **Match the container to the access pattern:** `List<T>` (default — growable, ordered, O(1) index,
  O(n) search), `Stack<T>` (LIFO — `Push`/`Pop`/`Peek`), `Queue<T>` (FIFO — `Enqueue`/`Dequeue`/`Peek`),
  `LinkedList<T>` (cheap insert/remove anywhere, no index), `T[,]` (fixed rectangular grid).
- **Arrays vs List vs Dictionary** *(QC example column)*: arrays are fixed-size; `List<T>` is dynamic;
  `Dictionary<K,V>` stores key-value pairs for O(1) lookup.
- **`Dictionary<K,V>`** is O(1) keyed lookup; `dict[key] = value` adds/overwrites, `dict.Add` throws on a
  duplicate, reading a missing key with the indexer throws `KeyNotFoundException` — probe with
  `TryGetValue`/`ContainsKey`. **`HashSet<T>`** enforces uniqueness with O(1) `Contains`/`Add`.
- **Generics (`<T>`)** give one type, any element, full compile-time safety. The collection types are
  themselves generic classes; you can write your own (`Shelf<T>`).
- **`enum`** names a fixed, closed set of choices; **`struct`** bundles small, identity-less data.
- **`IEnumerable<T>`** is the contract behind `foreach`; implement `GetEnumerator()` with `yield return`
  for lazy, **deferred** iteration.
- **Lambdas as behavior-as-data:** a `Predicate<T>` lambda filters (`catalog.Find(i => i.Author == "X")`);
  a `Comparison<T>` lambda **sorts** (`items.Sort((a,b) => string.Compare(a.Title, b.Title))`). These are
  the mechanism under LINQ.

**Key points / pitfalls**
- `Stack<T>`/`Queue<T>` are not indexable (`stack[0]` does not compile) — that is deliberate; they encode
  an order discipline, they are not "a faster list."
- Reading a missing `Dictionary` key with the indexer throws — use `TryGetValue`.
- Deferred execution footgun: a `yield`/lambda query does no work until you enumerate it; if the source
  changes first, you see the new state.
- `List<T>` is the right answer ~90% of the time; reach for `LinkedList<T>` only when you reorder a lot and
  rarely random-access.

**Worked example (filter + sort with lambdas)** *(cited: `weeklytechrepo/Intermediate-CSharp/demo/walkthroughs/05-advanced-classes.md`, commit `05-advanced-classes`)*
```csharp
// filter: Predicate<T> lambda decides membership
List<LibraryItem> byMartin = catalog.Find(item => item.Author == "Robert C. Martin");

// sort: Comparison<T> lambda decides order (the QC "filters and sorts" Nice item)
List<LibraryItem> all = catalog.Find(_ => true);
all.Sort((a, b) => string.Compare(a.Title, b.Title));   // A -> Z by title
all.Sort((a, b) => b.Title.Length - a.Title.Length);    // longest title first
```
The note hand-rolls `Find` over a `foreach` so the mechanism is visible before LINQ
(`advanced-classes.md`). Per the anti-spoiler ledger, **LINQ `OrderBy`/`Where` is deferred** — use
`List.Sort` and a `Predicate<T>` for QC-1.

---

## 6. Exceptions & Debugging

**Objectives covered**
- *(Must)* Uses Try-Catch-Finally to avoid hard crashing when running "risky" operations.
- *(Must)* Effectively interprets stack traces in order to debug code files.
- *(Should)* Leverages manually thrown exceptions and bubbling to debug business logic.
- *(Nice)* Can create custom exceptions in their applications to fit specific use cases.

**Concept recap** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`, `.../Intro-OOP/csharp-basics-overview.md`)*
- **Throw vs return:** a routine, expected outcome returns a value ("no copies right now"); a broken
  assumption / misuse **throws** ("no such id"). Returning `null` on a real miss plants a landmine
  downstream.
- **`try`/`catch`/`finally`:** wrap risky work; catch **most specific first, base last** (a base `catch`
  before its subtype is unreachable and will not compile); `finally` always runs, even after a throw —
  cleanup lives there.
- **Custom exceptions** derive from `Exception`; a shared base lets a caller mop up all domain errors with
  one `catch` or target a subtype. Carry data, not just a message.
- **Bubbling:** an uncaught exception propagates up the call stack until something catches it (or it
  crashes). Inside a `catch`, bare `throw;` rethrows preserving the original stack trace; `throw ex;`
  resets it. Never swallow with an empty `catch {}`.
- **Reading a stack trace:** read **top to bottom** — the top frame is where the exception was raised
  (file + line); each frame below is the caller, down to `Main`. Log `ex.ToString()` (full trace), not
  just `ex.Message`.

**Key points / pitfalls**
- Catch order matters — specific before base, or it won't compile.
- `throw;` preserves the origin frame; `throw ex;` hides it. Prefer bare `throw;`.
- An empty `catch {}` hides bugs — never do it.
- The top stack-trace frame is the origin; the frames beneath only tell you *how you got there*.

**Worked example (custom exception + try/catch/finally + trace)** *(cited: `weeklytechrepo/Intermediate-CSharp/demo/walkthroughs/04-exceptions-patterns-logging.md`, commit `04-exceptions-patterns-logging`)*
```csharp
public class ItemNotFoundException : LibraryException
{
    public int Id { get; }
    public ItemNotFoundException(int id) : base($"No library item with id {id}.") => Id = id;
}

try { LibraryItem item = repo.GetById(999); }   // throws on a real miss
catch (ItemNotFoundException ex)                 // specific first
{ Log.Error("Lookup failed for id {Id}", ex.Id); }
catch (LibraryException ex)                      // base last
{ Log.Error("Library error: {Message}", ex.Message); }
finally { /* always runs */ }
```
Trace, read top-down — the top frame is the origin:
```
System.NullReferenceException: ...
   at Library.InMemoryLibraryRepository.GetById(Int32 id) in ...\Repo.cs:line 42   <- thrown here
   at Library.BorrowService.Borrow(Int32 id) in ...\BorrowService.cs:line 17        <- caller
   at Library.Program.Main(String[] args) in ...\Program.cs:line 9                  <- entry
```

---

## 7. Design Patterns & SOLID

**Objectives covered**
- *(Must)* Describe the purpose of a design pattern.
- *(Must)* Describe and utilize SOLID principles in application design.
- *(Should)* Describe the repository design pattern.
- *(Should)* Describe the singleton design pattern.
- *(Should)* Describe the unit-of-work design pattern.
- *(Should)* Implement the Repository pattern in an application.
- *(Nice)* Demonstrate the implementation of a design pattern.

**Concept recap** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`)*
- A **design pattern** is a standardized, reusable solution to a commonly occurring problem in software
  design *(QC example column)*. The notes frame each pattern as a SOLID principle made concrete.
- **SOLID:** **S**ingle Responsibility (each type owns one job), **O**pen/Closed (open to extension, closed
  to modification — a new item kind changes only the factory), **L**iskov Substitution (a subtype works
  anywhere its base is expected), **I**nterface Segregation (small focused interfaces, e.g. `ILendable`),
  **D**ependency Inversion (depend on an abstraction, not a concrete type).
- **Repository (Dependency Inversion):** an interface is the contract, a class is one implementation;
  callers type their variable as the interface, so storage can change without touching them.
- **Factory method (Open/Closed):** one place decides which concrete type to build; callers pass an `enum`
  and never `new Book(...)` themselves.
- **Unit of Work:** groups related changes and commits them together — over an in-memory list it is a
  teaching stub; against a database its `Commit()` becomes one transaction (all or nothing).
- **Singleton:** one shared instance reachable across the process via a static access point. Serilog's
  static `Log` is a singleton, configured once. Handy but easy to abuse; a DI container (later) gives the
  same single-instance guarantee without the global static.

**Key points / pitfalls**
- "Describe the purpose" answers want the *why* (reusable solution to a recurring problem), then the
  specific pattern's job — not just a code dump.
- Repository = the seam where a real database slots in later (Week 3); callers depend on
  `ILibraryRepository`, not the in-memory class.
- Singletons are global mutable state — convenient, but overuse hurts testability.

**Worked example (Repository behind an interface)** *(cited: `weeklytechrepo/Intermediate-CSharp/demo/walkthroughs/04-exceptions-patterns-logging.md`, commit `04-exceptions-patterns-logging`)*
```csharp
public interface ILibraryRepository                 // the contract (Dependency Inversion)
{
    void Add(LibraryItem item);
    LibraryItem GetById(int id);                    // throws ItemNotFoundException when absent
    IReadOnlyList<LibraryItem> GetAll();
}

public class InMemoryLibraryRepository : ILibraryRepository  // one implementation
{
    private readonly List<LibraryItem> _items = new();
    public void Add(LibraryItem item) => _items.Add(item);
    public LibraryItem GetById(int id) { /* scan; throw on miss */ }
    public IReadOnlyList<LibraryItem> GetAll() => _items;
}
// callers type the variable as ILibraryRepository -> storage can change without touching them
```
*(Week 2 `05-advanced-classes` later swaps the backing `List` for a `Dictionary` keyed by id — O(n) -> O(1)
— without changing this public contract: Open/Closed in action.)*

---

## 8. Async, Networking & Language Round-out

**Objectives covered**
- *(Must)* Utilize the HttpClient object to make HTTP calls to external APIs.
- *(Must)* Program asynchronously in C# using async and await.
- *(Should)* Implement nullable types in an application.
- *(Nice)* Demonstrate an understanding of REGEX and pattern matching syntax.

**Concept recap** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/async-http-networking-regex.md`)*
- **Async is for I/O, not speed:** `async`/`await` frees the thread while waiting on network/disk — it
  does not compute faster. CPU-bound work wants threads/parallelism; I/O-bound work wants async.
- **`HttpClient`:** share **one** instance for the whole process (a new one per call leaks OS sockets —
  the single most common bug). Its methods return `Task<...>`, so `Main` becomes `async Task`.
- **`async`/`await` rules:** never call `.Result` or `.Wait()` (they deadlock and hide errors) — `await`
  all the way up; never use `async void` except for event handlers (its exceptions vanish). The `Async`
  suffix is the naming convention.
- **Deserialize JSON, then build your domain object** from the fields you read; `Task.WhenAll` overlaps
  independent awaits (launch the tasks, then await once — awaiting inside a loop is serial).
- **Nullable value types** `int?` can hold null; **lifted operators** propagate null (`null * 2 == null`)
  and `??` supplies a default. `?.` + `??` are the null-safe toolkit.
- **Regex** validates input *shape*: verbatim string `@"..."`, anchored with `^` and `$` so the whole
  string must match. Capturing groups `(...)` extract values; group `0` is the whole match. A
  pattern-matching `switch` branches on runtime type. **`out`** returns extra values (`int.TryParse(s, out
  int n)`).

**Key points / pitfalls**
- One `HttpClient` per process; never one per call.
- `.Result`/`.Wait()` deadlock — `await` instead, all the way to `Main`.
- Unanchored regex (`\d{13}` without `^...$`) lets a partial match pass — a common validation bug.
- A foreseeable `HttpRequestException` should be caught and degrade gracefully (return null/"nothing"),
  not crash the run.

**Worked example (shared HttpClient + async/await + regex guard)** *(cited: `weeklytechrepo/Intermediate-CSharp/demo/walkthroughs/06-async-http.md`, commit `06-async-http`)*
```csharp
private static readonly HttpClient Http = new();   // shared, not per-call

public async Task<LibraryItem?> FetchByIsbnAsync(string isbn)
{
    if (!Regex.IsMatch(isbn, @"^\d{13}$")) return null;   // validate shape first, anchored
    try
    {
        string json = await Http.GetStringAsync(url);     // await frees the thread while waiting
        return Parse(json);                                // deserialize -> build via factory
    }
    catch (HttpRequestException ex)                        // foreseeable failure, handled near
    {
        Log.Warning("Network fetch failed for {Isbn}: {Message}", isbn, ex.Message);
        return null;
    }
}
```

---

## 9. Process & Delivery: SDLC, Agile, Git (and building a functional app)

**Objectives covered**
- *(Must)* Create a functional application to fulfill behavioural requirements and user stories.

> SDLC, Agile, and Git are taught topics (Week 1) and feed the **behavioral** section of
> `mock-interview.md`. The QC-1 rubric's only graded objective in this area is the functional-app one
> above; the process knowledge supports it.

**Concept recap** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/sdlc-agile.md`, `.../git-fundamentals.md`, `weeklytechrepo/Agile-Git-CoreCSharp/async-lab/core-csharp-kata-README.md`)*
- **SDLC phases:** Requirements -> Design -> Implementation -> Testing -> Deployment -> Maintenance.
  **Waterfall** runs them once in strict sequence; **Agile** runs them iteratively in short (1–2 week)
  sprints, favoring working software and responding to change. **Scrum** is the dominant Agile framework —
  roles (Product Owner, Scrum Master, Dev Team), artifacts (product/sprint backlog, increment, user
  story), ceremonies (planning, standup, review, retro).
- **A user story** reads *"As a [user], I want [goal] so that [benefit]"* with **acceptance criteria** —
  the form the kata is graded against.
- **Git core loop:** edit -> `git add` -> `git commit -m` (imperative mood, say *why*) -> `git push`;
  `git status` shows where you stand. A file moves working directory -> staging area -> repository. A
  **remote** (`origin`) hosts it on GitHub; add a `.gitignore` (`bin/`, `obj/`, `.vs/`) *before* the first
  commit.
- **Functional application:** the Week-1 capstone (`core-csharp-kata-README.md`) is a menu-driven console
  app meeting an acceptance-criteria rubric — the concrete instance of "fulfill behavioural requirements
  and user stories."

**Key points / pitfalls**
- "Fulfill behavioural requirements / user stories" = build to the **acceptance criteria**, not to your
  own idea of done. The kata rubric is the contract.
- Add `.gitignore` before the first commit so `bin/`/`obj/` are never tracked.
- Commit messages: imperative mood, explain *why* — good history is a gift to your reviewer.

**Worked example (functional app skeleton)** *(cited: `weeklytechrepo/Agile-Git-CoreCSharp/async-lab/core-csharp-kata-README.md`)*
```csharp
static void Main()
{
    var running = true;
    while (running)                       // run-until-quit loop
    {
        PrintMenu();
        int choice = int.Parse(Console.ReadLine());
        switch (choice)                   // command dispatch
        {
            case 1: AddItem(); break;
            case 2: ListItems(); break;   // lists seeded + added domain objects
            case 0: running = false; break;
        }
    }
}
```
`Main` orchestrates a menu loop; handlers operate on your domain **classes** — the app is the acceptance
criteria made runnable.
