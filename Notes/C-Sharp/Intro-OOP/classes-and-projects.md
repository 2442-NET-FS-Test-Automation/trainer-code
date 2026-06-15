# Classes and Projects

## Learning Objectives
- Distinguish a solution, a project, and a class library, and explain how they nest.
- Organize code with namespaces and `using` directives.
- Declare a C# class and instantiate objects from it.
- Identify the four kinds of class members: fields, properties, methods, and constructors.
- Initialize objects with constructors, constructor chaining, primary constructors, and object initializers.
- Distinguish instance members from `static` members.

## Why This Matters
This morning's basics gave you the raw materials — types, operators, methods. This note gives you the *containers*. By Friday every learner ships a `core-csharp-kata` structured with real classes, and by next week that code splits across projects. Knowing how a solution holds projects, how a project holds namespaces, and how a class holds members is the difference between a script that works and a codebase a team can grow. The week's epic is an "OOP-structured app" — and the class is the unit of that structure.

## The Concept

### Solutions, Projects, and Class Libraries
.NET organizes code in a three-level hierarchy:

- A **solution** (`.sln`) is the top-level container. It groups one or more projects so they build and load together. Think of it as the workspace.
- A **project** (`.csproj`) is one buildable unit that produces one output. Two common kinds:
  - **Console app** — produces a runnable program with an entry point (`Program.cs` / `Main`).
  - **Class library** — produces a reusable `.dll` with no entry point; other projects reference it.
- A **class library** lets you separate logic (e.g., domain classes) from the app that runs it, so the same logic can be reused and tested independently — which matters a great deal once the test-automation track begins.

```
LibrarySolution.sln
├── LibraryApp/           (console project — has Main)
│   └── Program.cs
└── LibraryDomain/        (class library — reusable .dll)
    └── Book.cs
```

You will start with a single console project this week and grow into this layout later.

### Namespaces
A **namespace** is a logical grouping that prevents name collisions and organizes related types. It usually mirrors your folder/project structure.

```csharp
namespace LibraryDomain
{
    public class Book { }
}
```

To use a type from another namespace, add a `using` directive at the top of the file:

```csharp
using LibraryDomain;   // now 'Book' is available
```

Modern C# also supports **file-scoped namespaces** — one line, less nesting:

```csharp
namespace LibraryDomain;

public class Book { }
```

### Classes
A **class** is a blueprint. It defines what an object knows (data) and what it can do (behavior). An **object** is a concrete instance created with `new`.

```csharp
public class Book
{
    public string Title { get; set; }
}

Book b = new Book();   // object — an instance of the Book class
b.Title = "Clean Code";
```

One class, many objects — each with its own data.

### Class Members
A class is built from four kinds of members:

**Fields** — variables that hold an object's state. Usually `private`. Mark a field `readonly` when it should be set once (in the constructor) and never change afterward:

```csharp
private int _pageCount;
private readonly string _isbn;   // set in the constructor, then fixed
```

**Properties** — controlled access to state, with `get`/`set` accessors. Auto-properties generate the backing field for you:

```csharp
public string Title { get; set; }      // auto-property
public string Author { get; private set; } // read-only from outside
```

When a value needs a rule, write a full property with a backing field and validate in the `set`:

```csharp
private int _pageCount;
public int PageCount
{
    get => _pageCount;
    set => _pageCount = value < 0 ? throw new ArgumentException("Pages cannot be negative") : value;
}
```

This is the seam where a class protects its own correctness — the deeper idea behind encapsulation, covered Friday.

**Methods** — the behaviors an object performs:

```csharp
public void MarkAsRead()
{
    IsRead = true;
}
```

**Constructors** — special methods that run when an object is created, used to set initial state. They share the class name and have no return type:

```csharp
public Book(string title, string author)
{
    Title = title;
    Author = author;
}

var b = new Book("Clean Code", "Robert C. Martin");
```

If you write no constructor, C# supplies a default parameterless one.

A class can have **multiple constructors** (overloading) for different ways to create it, and one can **chain** to another with `: this(...)` so the shared setup lives in one place:

```csharp
public Book(string title) : this(title, "Unknown") { }   // delegates to the two-arg version
```

The `this` keyword refers to *the current object*. Use it to disambiguate a parameter from a field, or to pass the object itself:

```csharp
public Book(string title) { this.Title = title; }
```

### Primary constructors
Modern C# can declare the constructor parameters right on the class header — a **primary constructor**. Those parameters are in scope throughout the class body, which cuts the boilerplate of a field-by-field assignment for simple types:

```csharp
public class Member(string name, int id)
{
    public string Name { get; } = name;   // members initialized straight from the parameters
    public int Id { get; } = id;
}

var m = new Member("Ada", 1);
```

It is the same concept as a normal constructor with less ceremony — the header *is* the constructor. Reach for it on small, data-holding classes; keep a conventional constructor body when you need validation, multiple constructors, or extra setup logic.

### Object Initializers
For properties without a matching constructor, **object initializer** syntax sets them right after `new`:

```csharp
var book = new Book("Clean Code", "Martin")
{
    PageCount = 464
};
```

### Instance vs static members
Most members belong to an **instance** — each object has its own copy (`book.Title`). A `static` member belongs to the **class itself**, shared by all instances and accessed through the type name, not an object:

```csharp
public class Book
{
    public static int TotalCreated { get; private set; }   // one shared counter
    public Book() { TotalCreated++; }                       // every new Book bumps it
}

Console.WriteLine(Book.TotalCreated);   // called on the type, not an object
```

Use `static` for shared counters, constants, and utility helpers; use instance members for per-object state. (The `static` keyword gets a fuller treatment in *The Four Pillars of OOP*.)

## Code Example (Full Class)
```csharp
namespace LibraryDomain;

public class Book
{
    public string Title { get; set; }
    public string Author { get; set; }
    private bool _isRead;

    public Book(string title, string author)
    {
        Title = title;
        Author = author;
        _isRead = false;
    }

    public void MarkAsRead() => _isRead = true;

    public string Describe() =>
        $"{Title} by {Author} — {(_isRead ? "read" : "unread")}";
}
```

```csharp
var book = new Book("The Pragmatic Programmer", "Hunt & Thomas");
book.MarkAsRead();
Console.WriteLine(book.Describe());
```

## Summary
- **Solution → project → namespace → class → members** is the nesting that organizes every .NET app.
- A **class library** is a reusable `.dll` with no entry point; a **console app** has `Main`.
- **Namespaces** group types and prevent collisions; `using` imports them.
- A **class** is a blueprint; an **object** is an instance made with `new`.
- The four member kinds: **fields** (state), **properties** (controlled access), **methods** (behavior), **constructors** (initialization).
- Create objects with constructors (overload and **chain** via `: this(...)`, or use a **primary constructor** on the class header for simple types) or **object initializers**; validate values in a property `set`; `this` is the current object.
- **Instance** members are per-object; **static** members belong to the class and are shared.

> Tomorrow we apply these classes to the four pillars of object-oriented programming. Today, focus on getting the structure right.

## Additional Resources
- [Classes — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/classes)
- [Namespaces — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/namespaces)
- [Project and solution structure — Microsoft Learn](https://learn.microsoft.com/en-us/visualstudio/ide/solutions-and-projects-in-visual-studio)
