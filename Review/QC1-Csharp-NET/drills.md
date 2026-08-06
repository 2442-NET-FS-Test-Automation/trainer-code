# QC-1 (.NET) — Drills

Short hands-on tasks per topic. **Do the prompt in your own domain** (Inventory, Bank, Garage, Playlist —
not Library), the same convention as the async-labs. The **model solution** uses the trainer **Library**
domain so you can compare structure without copying. Each drill names the **QC objective** it exercises.
No drills exist for later-week topics (see `out-of-scope-register.md`).

Tip: try the prompt first, run it (`dotnet run`), then read the solution.

---

## Drill 1 — CLI project from scratch *(.NET platform & tooling)*

**Prompt:** From an empty folder, scaffold a console project, make it print one line about your domain, and
run it. Then add a NuGet package of your choice and confirm it still builds.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/dotnet-and-csharp-intro.md`)*
```bash
dotnet new console -o LibraryApp
cd LibraryApp
# edit Program.cs: Console.WriteLine("Library catalog starting...");
dotnet run                          # -> Library catalog starting...
dotnet add package Serilog          # NuGet dependency
dotnet build                        # still compiles
```
Proves QC: Utilize the dotnet command line tools to generate and execute projects; Uses the NuGet Package
Manager to install and manage dependencies.

---

## Drill 2 — Types, operators, a method, an array *(C# fundamentals)*

**Prompt:** Store 5 numeric values from your domain in an array (prices, balances, mileages...). Write a
method that returns their average as a `double`, and print whether the average is above a threshold using a
comparison + logical operator.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/csharp-basics-overview.md`)*
```csharp
int[] pageCounts = { 120, 464, 90, 300, 210 };

double Average(int[] values)
{
    int total = 0;
    foreach (int n in values) total += n;
    return (double)total / values.Length;     // cast to avoid integer division
}

double avg = Average(pageCounts);
Console.WriteLine($"Average: {avg:0.0}");
Console.WriteLine(avg > 200 && pageCounts.Length >= 5 ? "Long catalog" : "Short catalog");
```
Proves QC: Demonstrate proper syntax for working with arrays; Create methods that allow for the reusability
of code; Can use basic, comparison, equality, and logical operators in programming logic.

---

## Drill 3 — Safe input conversion *(C# fundamentals)*

**Prompt:** Read a quantity from the console and convert it to an `int` without crashing on bad input;
default to 0 and print a message when the input isn't a number.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/csharp-basics-overview.md`)*
```csharp
Console.Write("Enter copies: ");
string? input = Console.ReadLine();
int copies = int.TryParse(input, out int n) ? n : 0;
if (copies == 0 && input != "0") Console.WriteLine("Not a number; defaulting to 0.");
```
Proves QC: Implement type conversion in an application.

---

## Drill 4 — Recursion *(C# fundamentals, Nice)*

**Prompt:** Write a recursive method that computes a factorial (or a recursive sum of a list) in your
domain context. Identify the base case explicitly in a comment.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/csharp-basics-overview.md`)*
```csharp
int Factorial(int n)
{
    if (n <= 1) return 1;            // base case — stops recursion
    return n * Factorial(n - 1);     // recursive case
}
Console.WriteLine(Factorial(5));     // 120
```
Proves QC: Implement recursion in an application.

---

## Drill 5 — Value vs reference *(memory model)*

**Prompt:** Make a small `readonly struct` for a coordinate/identifier in your domain. Copy it into a second
variable, "change" the copy, and show the original is untouched. Then do the same with a class and show both
references see the change.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/collections-overview.md`)*
```csharp
public readonly struct ShelfLocation
{
    public int Aisle { get; }
    public int Shelf { get; }
    public ShelfLocation(int aisle, int shelf) => (Aisle, Shelf) = (aisle, shelf);
    public ShelfLocation WithShelf(int shelf) => new(Aisle, shelf);
}

var a = new ShelfLocation(1, 2);
var b = a.WithShelf(9);             // copy, independent
Console.WriteLine($"{a.Shelf} {b.Shelf}");   // 2 9  -> struct copy did not touch 'a'
```
Proves QC: Can differentiate between value and reference types, and describe stack vs. heap allocation.

---

## Drill 6 — Model an entity as a class *(OOP)*

**Prompt:** Define one entity class in your domain with a private field, an auto-property, a constructor, and
a method. Validate one value in a property `set` so a bad value is rejected.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/classes-and-projects.md`)*
```csharp
public class Book
{
    public string Title { get; set; }
    private int _pageCount;
    public int PageCount
    {
        get => _pageCount;
        set => _pageCount = value < 0 ? throw new ArgumentException("Pages cannot be negative") : value;
    }
    public Book(string title, int pageCount) { Title = title; PageCount = pageCount; }
    public string Describe() => $"{Title} ({PageCount} pages)";
}
```
Proves QC: Can model real-world entities using classes, fields, methods, and constructors; Understands
auto-property syntax for class fields.

---

## Drill 7 — Four pillars in one small model *(OOP)*

**Prompt:** Build an abstract base entity with one abstract method, two derived types that override it, then
loop a base-typed array calling that method (polymorphism). Add a `protected` member used by a subclass and
one `static` counter.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/oop-pillars.md`,
`weeklytechrepo/Agile-Git-CoreCSharp/demo/walkthroughs/02-oop.md`)*
```csharp
public abstract class Item
{
    public static int Count { get; private set; }
    public string Title { get; }
    protected Item(string title) { Title = title; Count++; }
    public abstract string Describe();          // polymorphic contract
}
public class Book : Item { public Book(string t) : base(t) {} public override string Describe() => $"Book: {Title}"; }
public class Dvd  : Item { public Dvd(string t)  : base(t) {} public override string Describe() => $"DVD: {Title}"; }

Item[] catalog = { new Book("Clean Code"), new Dvd("Inception") };
foreach (Item i in catalog) Console.WriteLine(i.Describe());   // right override at runtime
Console.WriteLine(Item.Count);                                  // 2 (static)
```
Proves QC: Understands and can explain the four pillars of OOP; Use encapsulation and abstration ... with
appropriate access modifiers; Use inheritance and polymorphism ...; Can differentiate between static and
instance members.

---

## Drill 8 — Interface as a capability *(OOP)*

**Prompt:** Define an interface for a capability in your domain (e.g. `ILendable`, `ISellable`,
`IServiceable`). Implement it on one entity and explain in a comment why an interface fits better than an
abstract class here.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intro-OOP/oop-pillars.md`)*
```csharp
public interface ILendable { void CheckOut(string borrower); }   // capability, "can do"

public class Book : Item, ILendable                               // also simulates multiple inheritance
{
    public Book(string t) : base(t) {}
    public override string Describe() => $"Book: {Title}";
    public void CheckOut(string borrower) => Console.WriteLine($"{Title} -> {borrower}");
}
// Interface (not abstract class): "lendable" is a capability several unrelated types could share.
```
Proves QC: Can describe the difference between an Interface and Abstract class ...; Able to simulate multiple
inheritance.

---

## Drill 9 — Pick the right container *(collections & generics)*

**Prompt:** In your domain, model three access patterns: the whole collection, a LIFO "undo last" stack, and
a FIFO "request line" queue. Add to each and demonstrate the order with output.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/collections-overview.md`)*
```csharp
List<string> catalog = new() { "Clean Code", "Refactoring" };   // ordered, indexable
Stack<string> returnCart = new();  returnCart.Push("Clean Code"); // LIFO
Queue<string> holds = new();       holds.Enqueue("Ada"); holds.Enqueue("Lin"); // FIFO

Console.WriteLine(returnCart.Pop());     // last pushed
Console.WriteLine(holds.Dequeue());      // first enqueued -> "Ada"
```
Proves QC: Describe the purpose and differences of collections; Demonstrates understanding of data
structures and collections in C#.

---

## Drill 10 — Write your own generic type *(collections & generics)*

**Prompt:** Write a small generic container `Bin<T>` / `Slot<T>` with a fixed capacity and a `TryAdd`
returning `bool`. Use it with two different element types.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/collections-overview.md`)*
```csharp
public class Shelf<T>
{
    private readonly T[] _slots;
    private int _used;
    public Shelf(int capacity) => _slots = new T[capacity];
    public int Count => _used;
    public bool TryAdd(T item)
    {
        if (_used == _slots.Length) return false;
        _slots[_used++] = item;
        return true;
    }
}
var books = new Shelf<string>(2);  books.TryAdd("Clean Code");
var ids   = new Shelf<int>(5);     ids.TryAdd(101);
```
Proves QC: Demonstrate understand of Generic Types in C#.

---

## Drill 11 — Dictionary lookup + HashSet uniqueness *(collections)*

**Prompt:** Index your entities by a natural key in a `Dictionary` and look one up safely without throwing.
Track the distinct values of one attribute with a `HashSet` and show the distinct count is below the entity
count when duplicates exist.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/advanced-classes.md`)*
```csharp
var byId = new Dictionary<int, string> { [1] = "Clean Code", [2] = "Refactoring" };
if (byId.TryGetValue(3, out var title)) Console.WriteLine(title);
else Console.WriteLine("not found");          // no crash

var authors = new HashSet<string>();
authors.Add("Martin"); authors.Add("Martin"); authors.Add("Fowler");
Console.WriteLine(authors.Count);             // 2, de-duped
```
Proves QC: Demonstrates understanding of data structures and collections in C#.

---

## Drill 12 — IEnumerable + lambda filter and sort *(collections & language)*

**Prompt:** Make your manager type `foreach`-able with `IEnumerable<T>` + `yield`. Add a `Find` that takes a
`Predicate<T>` lambda, and sort a result list with a `Comparison<T>` lambda. **No LINQ.**

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/advanced-classes.md`,
`weeklytechrepo/Intermediate-CSharp/demo/walkthroughs/05-advanced-classes.md`)*
```csharp
public List<Book> Find(Predicate<Book> match)
{
    var hits = new List<Book>();
    foreach (var b in _items) if (match(b)) hits.Add(b);
    return hits;
}
public IEnumerator<Book> GetEnumerator() { foreach (var b in _items) yield return b; }

var byMartin = catalog.Find(b => b.Author == "Robert C. Martin");   // filter
byMartin.Sort((a, b) => string.Compare(a.Title, b.Title));          // sort A->Z
```
Proves QC: Implement lambda expressions in an application; Utilize the collections namespace, and types that
extend the IEnumerable interface in an application; Implement a lambda expression to perform filters and
sorts.

---

## Drill 13 — try/catch/finally + custom exception + read the trace *(exceptions)*

**Prompt:** Write a lookup that throws a **custom exception carrying the missing id** when an entity isn't
found. Call it inside `try`/`catch`/`finally`, catching the specific type before any base type. Run it, let
one call miss, and read the printed stack trace top-down to name the origin line.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`,
`weeklytechrepo/Intermediate-CSharp/demo/walkthroughs/04-exceptions-patterns-logging.md`)*
```csharp
public class ItemNotFoundException : Exception
{
    public int Id { get; }
    public ItemNotFoundException(int id) : base($"No item with id {id}.") => Id = id;
}

Book GetById(int id) =>
    _byId.TryGetValue(id, out var b) ? b : throw new ItemNotFoundException(id);

try { var book = GetById(999); }
catch (ItemNotFoundException ex) { Console.WriteLine($"Missing id {ex.Id}"); }  // specific first
finally { Console.WriteLine("done"); }                                          // always runs
```
Proves QC: Uses Try-Catch-Finally ...; Leverages manually thrown exceptions and bubbling ...; Can create
custom exceptions ...; Effectively interprets stack traces in order to debug code files.

---

## Drill 14 — Repository behind an interface *(design patterns & SOLID)*

**Prompt:** Define a repository **interface** for your domain entity and one in-memory implementation.
Have callers depend on the interface. In a comment, name which SOLID principle the interface gives you.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/exceptions-patterns-logging.md`)*
```csharp
public interface ILibraryRepository                  // contract -> Dependency Inversion
{
    void Add(Book b);
    Book GetById(int id);
}
public class InMemoryLibraryRepository : ILibraryRepository
{
    private readonly Dictionary<int, Book> _byId = new();
    public void Add(Book b) => _byId[b.Id] = b;
    public Book GetById(int id) =>
        _byId.TryGetValue(id, out var b) ? b : throw new ItemNotFoundException(id);
}
ILibraryRepository repo = new InMemoryLibraryRepository();   // caller depends on the abstraction
```
Proves QC: Implement the Repository pattern in an application; Describe and utilize SOLID principles in
application design; Demonstrate the implementation of a design pattern.

---

## Drill 15 — Async HTTP fetch with a regex guard *(async, networking, round-out)*

**Prompt:** Write an `async` method that validates an identifier with an anchored regex, then fetches a
string from a public API using a **shared** `HttpClient`, awaiting the call. Handle `HttpRequestException`
so a network failure returns nothing instead of crashing.

**Model solution** *(source: `trainer-code/Notes/C-Sharp/Intermediate-C#/async-http-networking-regex.md`,
`weeklytechrepo/Intermediate-CSharp/demo/walkthroughs/06-async-http.md`)*
```csharp
private static readonly HttpClient Http = new();    // one per process

public async Task<string?> FetchAsync(string isbn)
{
    if (!Regex.IsMatch(isbn, @"^\d{13}$")) return null;   // anchored shape check
    try
    {
        return await Http.GetStringAsync($"https://openlibrary.org/isbn/{isbn}.json");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Fetch failed: {ex.Message}");
        return null;                                       // degrade, don't crash
    }
}
```
Proves QC: Utilize the HttpClient object to make HTTP calls to external APIs; Program asynchronously in C#
using async and await; Demonstrate an understanding of REGEX and pattern matching syntax.

---

## Drill 16 — Build a functional menu app *(delivery / functional application)*

**Prompt:** Wrap your entity + repository in a runnable console app: a `while` loop prints a menu, a
`switch` dispatches commands (add / list / one domain action), and 2–3 entities are seeded at startup. Keep
`Main` thin — handlers do the work.

**Model solution** *(source: `weeklytechrepo/Agile-Git-CoreCSharp/async-lab/core-csharp-kata-README.md`)*
```csharp
static void Main()
{
    var running = true;
    while (running)
    {
        Console.WriteLine("1) Add  2) List  0) Quit");
        switch (int.Parse(Console.ReadLine()!))
        {
            case 1: AddBook(); break;
            case 2: ListBooks(); break;
            case 0: running = false; break;
        }
    }
}
```
Proves QC: Create a functional application to fulfill behavioural requirements and user stories.
