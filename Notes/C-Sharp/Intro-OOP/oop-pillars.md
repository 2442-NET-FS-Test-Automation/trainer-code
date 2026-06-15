# The Four Pillars of OOP

## Learning Objectives
- Explain encapsulation, inheritance, polymorphism, and abstraction, and recognize each in code.
- Choose between an interface and an abstract class.
- Apply access modifiers to control visibility.
- Use the `static` keyword for class-level members.
- Distinguish `override` from `new`, and method overloading from overriding.

## Why This Matters
Yesterday you built classes; today you make them *object-oriented*. The four pillars are the design ideas that let a `core-csharp-kata` grow without collapsing into spaghetti — they are exactly what this week's epic means by an "OOP-structured app." Every framework you touch for the rest of the cohort (and in the job after) is built on these ideas. The trainer demo models them in a Library domain; you will mirror them in your own.

## The Concept

### Pillar 1: Encapsulation
Bundle data with the methods that operate on it, and hide the internals behind a controlled surface. State is `private`; access goes through properties and methods so the object enforces its own rules.

```csharp
public class Account
{
    private decimal _balance;          // hidden state
    public decimal Balance => _balance; // read-only view

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Must be positive");
        _balance += amount;             // the only way in
    }
}
```

Callers cannot set a negative balance because they cannot reach `_balance` directly. That is encapsulation protecting an **invariant**.

### Pillar 2: Inheritance
A class can derive from a base class, reusing its members and adding or specializing behavior. The relationship is "is-a."

```csharp
public class Item
{
    public string Title { get; set; }
}

public class Book : Item     // Book is-a Item
{
    public int PageCount { get; set; }
}
```

`Book` automatically has `Title`. Use inheritance for genuine specialization, not just code sharing.

A derived class reaches its parent with the `base` keyword — most often to call the base constructor or extend (rather than fully replace) a base method:

```csharp
public class Book : Item
{
    public Book(string title) : base()   // call the base constructor first
    {
        Title = title;
    }
}
```

### Pillar 3: Polymorphism
One interface, many implementations. A base-typed reference can hold any derived object and call the right behavior at runtime. This is enabled by `virtual` / `override`.

```csharp
public class Item
{
    public virtual string Describe() => "An item";
}

public class Book : Item
{
    public override string Describe() => "A book";
}

Item thing = new Book();
Console.WriteLine(thing.Describe()); // "A book" — resolved at runtime
```

When you hold a base-typed reference and need the specific derived type, test and convert with `is` / `as`:

```csharp
if (thing is Book b)             // pattern test + cast in one step
    Console.WriteLine(b.PageCount);

var maybe = thing as Book;       // null if 'thing' is not a Book (no exception)
```

Prefer `is`/`as` over a blunt `(Book)thing` cast, which throws if the object is not actually a `Book`.

### Pillar 4: Abstraction
Expose *what* an object does, hide *how*. Model the essential contract and omit the noise. Abstraction is delivered through **abstract classes** and **interfaces**.

### Interfaces vs Abstract Classes
Both define contracts, but they answer different needs:

| | Interface | Abstract class |
|---|-----------|----------------|
| Defines | a capability ("can do") | a base identity ("is a") |
| Implementation | traditionally none (members are a contract) | can include shared implemented members |
| Inheritance | a class can implement **many** | a class derives from **one** |
| Fields/state | no | yes |

```csharp
public interface ILendable
{
    void CheckOut(string borrower);
}

public abstract class Item
{
    public string Title { get; set; }
    public abstract string Describe(); // must be implemented by subclasses
}

public class Book : Item, ILendable
{
    public override string Describe() => $"Book: {Title}";
    public void CheckOut(string borrower) { /* ... */ }
}
```

Rule of thumb: use an **interface** for a capability several unrelated types might share; use an **abstract class** when subclasses share state or implemented behavior.

**`virtual` vs `abstract` methods** — both can be overridden, but:
- A `virtual` method **has a default body** the subclass *may* override.
- An `abstract` method **has no body** and the subclass *must* override it (and the class itself must be `abstract`).

```csharp
public abstract class Item
{
    public abstract string Describe();          // no body — every subclass must supply one
    public virtual string Format() => Describe(); // has a body — override only if needed
}
```

### Access Modifiers
They control who can see a member:

- `public` — anyone.
- `private` — only this class (the default for class members).
- `protected` — this class and its subclasses.
- `internal` — anything in the same project/assembly.

`protected` is the inheritance-friendly middle ground: hidden from outside callers, but available to subclasses.

```csharp
public class Item
{
    protected string Id { get; set; }   // subclasses can use Id; outside code cannot
}

public class Book : Item
{
    public string Tag() => $"BOOK-{Id}";  // legal: Book derives from Item
}
```

Default to the most restrictive level that works; widen only when needed.

### The `static` Keyword
A `static` member belongs to the **class itself**, not to any instance — there is exactly one, shared across the program. No object needed to call it.

```csharp
public class MathHelper
{
    public static int Square(int n) => n * n;
}

int result = MathHelper.Square(5); // call on the type, not an object
```

Use `static` for utility helpers and shared constants. Instance data should not be static.

### override vs new
Both let a derived class redeclare a base member, but they behave differently through a base reference:

- **`override`** (with `virtual`) — true polymorphism; the derived version runs even through a base-typed reference.
- **`new`** — *hides* the base member; which version runs depends on the **reference type**, not the object. Usually a mistake unless you mean it.

```csharp
Item a = new Book();
a.Describe(); // override -> Book's version | new -> Item's version
```

To stop further overriding down the chain, mark an override `sealed` — subclasses then inherit it but cannot replace it:

```csharp
public sealed override string Describe() => "A book";
```

### Overloading vs Overriding
- **Overloading** — same method name, *different parameter lists*, in the **same** class. Resolved at compile time.
- **Overriding** — replacing a base class's `virtual` method in a **derived** class with the same signature. Resolved at runtime.

```csharp
// Overloading
public int Add(int a, int b) => a + b;
public double Add(double a, double b) => a + b;
```

## Code Example (All Four Pillars Together)
A small Library model that uses every pillar at once:

```csharp
// Abstraction + Encapsulation: an abstract contract with protected, controlled state
public abstract class Item
{
    public string Title { get; }
    protected Item(string title) => Title = title;   // base constructor
    public abstract string Describe();                // subclasses must implement
}

// Inheritance: Book is-a Item and reuses its state via base(...)
public class Book : Item
{
    public int PageCount { get; }
    public Book(string title, int pageCount) : base(title) => PageCount = pageCount;

    // Polymorphism: override the contract
    public override string Describe() => $"Book: {Title} ({PageCount} pages)";
}

public class Dvd : Item
{
    public Dvd(string title) : base(title) { }
    public override string Describe() => $"DVD: {Title}";
}

// One base-typed list, many behaviors resolved at runtime
Item[] catalog = { new Book("Clean Code", 464), new Dvd("Inception") };
foreach (Item item in catalog)
    Console.WriteLine(item.Describe());
// Book: Clean Code (464 pages)
// DVD: Inception
```

## Summary
- **Encapsulation** hides state behind a controlled surface; **inheritance** specializes a base type; **polymorphism** picks behavior at runtime; **abstraction** exposes the contract, not the mechanics.
- **Interfaces** model "can do" (many per class, no state); **abstract classes** model "is a" (one per class, can share implementation).
- Access modifiers go from `public` to `private`; `protected` opens a member to subclasses only; default to the tightest that works.
- `base` reaches the parent; `is`/`as` safely test-and-cast a base reference; `virtual` has a default body, `abstract` has none and must be overridden; `sealed override` stops further overriding.
- `static` belongs to the class, not an instance.
- `override` is polymorphic; `new` merely hides. **Overloading** = same name, different parameters; **overriding** = replacing a `virtual` base method.

## Additional Resources
- [Object-oriented programming in C# — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/oop)
- [Interfaces vs abstract classes — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/interfaces)
- [Access modifiers — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers)
