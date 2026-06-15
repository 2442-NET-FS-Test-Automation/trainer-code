# Exceptions, Design Patterns & Logging

## Learning Objectives
- Decide when to return a value versus when to `throw`, and handle failures with `try`/`catch`/`finally`.
- Write a custom exception family that carries data, and catch from most specific to most general.
- Read a stack trace top-to-bottom to locate where an exception originated.
- Recognize the factory method, repository, unit of work, and singleton patterns, and map them onto SOLID.
- Configure Serilog and write structured log events with levels and sinks.

## Why This Matters
A console kata that prints fixed output is a toy; production code fails, and how it fails is the difference between a clear error near the cause and a `NullReferenceException` three layers downstream with no clue where it came from. This week's epic is maturing `core-csharp-kata` into a production-shaped CLI, and today is where it grows the spine: errors become loud and typed, storage hides behind an abstraction, object creation centralizes, and every meaningful action leaves a structured log trail. None of these are decoration — each pattern is a SOLID principle made concrete, and the same abstractions are exactly where a real database slots in during Week 3.

## The Concept

### Throw versus return
Week 1's `Book.Checkout()` returned `false` when there were no copies. That is fine for a routine, expected outcome. But asking for an id that does not exist is a different thing: returning `null` plants a landmine for whoever dereferences it later. The rule:

```
Routine, expected outcome   -> return a value (bool / null-object)   e.g. "no copies right now"
Broken assumption / misuse  -> throw an exception (fail loud, near)   e.g. "no such id"
```

### A custom exception family
Exceptions are just classes that derive from `Exception`. A shared base lets a caller mop up all your domain errors with one `catch`, or target a specific subtype. Carry data, not just a message:

```csharp
public class LibraryException : Exception
{
    public LibraryException(string message) : base(message) { }
}

public class ItemNotFoundException : LibraryException
{
    public int Id { get; }
    public ItemNotFoundException(int id) : base($"No library item with id {id}.") => Id = id;
}
```

Handle them, most specific first:

```csharp
try
{
    LibraryItem item = repo.GetById(999);
}
catch (ItemNotFoundException ex)        // specific
{
    Log.Error("Lookup failed for id {Id}", ex.Id);
}
catch (LibraryException ex)             // base, last
{
    Log.Error("Library error: {Message}", ex.Message);
}
finally
{
    // always runs, even after a throw — cleanup lives here
}
```

Catch order matters: a base `catch` placed before its subtype makes the specific one unreachable and will not compile. Inside a `catch`, use a bare `throw;` to rethrow (it preserves the original stack trace); `throw ex;` resets it. Never swallow an exception with an empty `catch {}`.

### Reading a stack trace
When an exception goes uncaught, the runtime prints a **stack trace** — the chain of calls that was active at the moment it threw. Read it **top to bottom**: the top frame is where the exception was actually raised; each line below is the caller that led there, down to `Main`. Every frame names the type, method, source file, and line number.

```
Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
   at Library.InMemoryLibraryRepository.GetById(Int32 id) in C:\kata\Repo.cs:line 42    <- thrown here
   at Library.BorrowService.Borrow(Int32 id) in C:\kata\BorrowService.cs:line 17         <- its caller
   at Library.Program.Main(String[] args) in C:\kata\Program.cs:line 9                   <- entry point
```

Start at the **top** frame (`Repo.cs:line 42`) — that file and line are where to look first; the frames beneath it tell you *how you got there*. This is the practical reason `throw;` matters: it keeps that original top frame, while `throw ex;` rewrites the trace to start at the rethrow and hides the real origin. When you log a failure, log `ex.ToString()` (or pass `ex` to Serilog), not just `ex.Message` — the full trace is what makes the bug findable later.

### Design patterns, mapped onto SOLID
Today's patterns each make a SOLID principle concrete:

```
S  Single Responsibility   each type owns one job (the repo owns storage)
O  Open/Closed             a new item kind changes only the factory, not every caller
L  Liskov Substitution     a ReferenceBook works anywhere a LibraryItem is expected
I  Interface Segregation   ILendable is a small, focused capability
D  Dependency Inversion    callers depend on ILibraryRepository, not a concrete store
```

**Repository (Dependency Inversion).** The interface is the contract; the class is one implementation. Callers type their variable as the abstraction, so the storage can change without touching them:

```csharp
public interface ILibraryRepository
{
    void Add(LibraryItem item);
    LibraryItem GetById(int id);          // throws ItemNotFoundException when absent
    IReadOnlyList<LibraryItem> GetAll();
}
```

**Factory method (Open/Closed).** One place decides which concrete type to build; callers pass an `enum` and never write `new Book(...)` themselves:

```csharp
public static LibraryItem Create(ItemKind kind, string title, string author) => kind switch
{
    ItemKind.Book          => new Book(title, author),
    ItemKind.ReferenceBook => new ReferenceBook(title, author),
    ItemKind.Magazine      => new Magazine(title, author),
    _ => throw new LibraryException($"Unknown item kind: {kind}")
};
```

**Unit of Work** groups related changes and commits them together — over an in-memory list it is a teaching stub, but against a database in Week 3 its `Commit()` becomes one transaction: all changes land, or none do. **Singleton** is one shared instance reachable across the process; you will use one immediately — Serilog's static `Log` is a singleton, configured once. Singletons are handy and easy to abuse; a DI container later gives the same single-instance guarantee without the global static.

### Structured logging with Serilog
`Console.WriteLine` cannot filter by severity, route to a file, or be queried later. A logging framework gives you three things: **levels**, **structure**, and **sinks**. Add the packages, then configure the logger once:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()   // Verbose < Debug < Information < Warning < Error < Fatal
    .WriteTo.Console()           // the sink: where events go
    .CreateLogger();
// ... app runs ...
Log.CloseAndFlush();             // flush buffered events on exit
```

The headline is **structured logging**:

```csharp
Log.Information("Added {Title} (id {Id})", item.Title, item.Id);
```

This is not string formatting — `Title` and `Id` are captured as named properties on the event. A structured sink (Seq, Elasticsearch) can then filter `Id = 12` across millions of lines. The message template is also the schema. A few rules: levels are a volume knob (`MinimumLevel` drops everything below it); a sink is just a destination, and adding `.WriteTo.File(...)` changes nothing at the call sites; Serilog matches `{Title}`, `{Id}` to arguments by **position**, so swapping the arguments mislabels the data; and never concatenate with `+`, which throws the structure away.

## Code Example
The repository throws on a real miss and logs at each layer:

```csharp
public class InMemoryLibraryRepository : ILibraryRepository
{
    private readonly List<LibraryItem> _items = new();

    public void Add(LibraryItem item)
    {
        _items.Add(item);
        Log.Information("Added {Title} (id {Id})", item.Title, item.Id);
    }

    public LibraryItem GetById(int id)
    {
        foreach (LibraryItem item in _items)
            if (item.Id == id) return item;

        Log.Warning("Lookup miss for id {Id}", id);
        throw new ItemNotFoundException(id);     // a miss is a broken assumption
    }
}
```

Running this prints `[INF] Added ...` and, on a bad lookup, `[WRN] Lookup miss for id 999` from the repo followed by `[ERR] Lookup failed ...` from the handler — the same failure logged with context at two layers.

> Heads up: the linear `GetById` scan above is `O(n)` on purpose — Wednesday's `advanced-classes` swaps the list for a `Dictionary` to make it `O(1)`. A real DI container (Week 5) and a persistent repository (the SQL weeks) come later; today we `new` these by hand.

## Summary
- **Return for routine outcomes, throw for broken assumptions** — fail loud and near the cause.
- **A custom exception family carries data**; catch most-specific first, base last; `finally` always runs; rethrow with bare `throw;`.
- **Read a stack trace top-to-bottom** — the top frame is the origin (file + line); `throw;` preserves it, `throw ex;` hides it; log `ex.ToString()`, not just `ex.Message`.
- **Patterns are SOLID made concrete** — repository = Dependency Inversion, factory = Open/Closed; unit of work groups changes, singleton shares one instance.
- **Serilog gives levels, structure, and sinks** — `Log.Information("{Title}", item.Title)` captures named properties you can query, not a flat string.
- **Configure once, flush once** — `CreateLogger()` at startup, `CloseAndFlush()` on exit.

## Additional Resources
- [Exceptions and exception handling (C#) — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/)
- [Design patterns / SOLID principles — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles)
- [Serilog — Getting Started](https://github.com/serilog/serilog/wiki/Getting-Started)
