# QC-1 (.NET) — Cheat Sheet

Dense quick-reference for the morning of the exam. Syntax is drawn from the trainer notes code blocks
(`trainer-code/Notes/C-Sharp/**`) and the QC example column (`qc-criteria/QC-1-NET.md`). Skim top to
bottom; each block maps to a study-guide cluster.

---

## .NET CLI & platform

| Command | Does |
|---|---|
| `dotnet --version` | Print installed SDK version |
| `dotnet new console -o App` | Scaffold a console project (`.csproj` + `Program.cs`) |
| `dotnet build` | Compile |
| `dotnet run` | Build + run |
| `dotnet add package Serilog` | Add a NuGet dependency |
| `dotnet add reference ../Lib/Lib.csproj` | Reference a class library |

- **SDK** = build (runtime + compiler + CLI). **Runtime** = run only. Install the SDK.
- **Compilation:** C# source (`.cs`) -> **IL** -> CLR **JIT** -> native machine code at run time.
- **Nesting:** solution (`.sln`) -> project (`.csproj`) -> namespace -> class -> members. Console app has
  `Main`; class library is a `.dll` with no entry point.

## Data types & `var`

| Value types | Reference types |
|---|---|
| `int`, `long`, `double`, `decimal`, `bool`, `char`, `struct`, `enum` | `string`, arrays, `class`, every collection |
| copied on assignment (stack) | reference copied on assignment (heap) |

```csharp
int count = 10;          decimal price = 19.99m;   // m = decimal literal
var name = "Ada";        // inferred string (not "any")
const double Pi = 3.14159;
int? maybe = null;       // nullable value type
```

## Operators

```
arithmetic  + - * / %     // integer / truncates: 7/2 == 3 ; % is remainder
comparison  <  >  <=  >=
equality    ==  !=        // value types: by value | objects: by reference | string: by text
logical     &&  ||  !     // && and || short-circuit
shortcuts   += -= *=  ++ --   ternary: cond ? a : b
null-safe   ?.   ??           // x ?? 0 ; obj?.Prop
```

## Control flow

```csharp
if (s >= 90) g = "A"; else if (s >= 80) g = "B"; else g = "C";
switch (day) { case "Sat": case "Sun": ...; break; default: ...; break; }
for (int i = 0; i < n; i++) { }     // known count
while (cond) { }                     // check first
do { } while (cond);                 // run once
foreach (var x in items) { }         // every item, no index
```

## Type conversion

```csharp
int floored = (int)9.7;                       // cast, truncates -> 9
int n = Convert.ToInt32("42");
bool ok = int.TryParse(input, out int parsed); // SAFE for user input (no throw)
```

## Methods, recursion, arrays

```csharp
int Add(int a, int b) => a + b;               // expression-bodied
int Factorial(int n) => n <= 1 ? 1 : n * Factorial(n - 1);  // recursion needs a base case

int[] nums = new int[3] { 1, 2, 3 };          // fixed size, zero-indexed
string[] days = { "Mon", "Tue" };
days.Length;  days[0];                         // out-of-range -> IndexOutOfRangeException
```

## Strings

```csharp
$"Hello, {name}! {count} items"               // interpolation
raw.Trim();  raw.ToUpper();  raw.Contains("x");  raw.Replace("a","b");  "a,b".Split(',');
// strings are IMMUTABLE; build in a loop with StringBuilder
var sb = new StringBuilder(); sb.Append("x"); sb.ToString();
```

## Classes & members

```csharp
public class Book
{
    private readonly string _isbn;            // field (set once)
    public string Title { get; set; }         // auto-property
    public string Author { get; private set; }// read-only from outside
    public static int TotalCreated { get; private set; } // static: shared on the class

    public Book(string title, string author)  // constructor
    { Title = title; Author = author; TotalCreated++; }
    public Book(string title) : this(title, "Unknown") { } // chaining

    public void MarkRead() => _isRead = true; // method
}

public class Member(string name, int id)      // primary constructor
{ public string Name { get; } = name; public int Id { get; } = id; }

var b = new Book("Clean Code", "Martin") { PageCount = 464 }; // object initializer
```

## OOP — four pillars

| Pillar | Mechanism |
|---|---|
| Encapsulation | `private` state + property/method that validates (protects an invariant) |
| Inheritance | `class Book : Item` ("is-a"); `base(...)` calls parent |
| Polymorphism | `virtual` / `override`; right method at runtime through a base reference |
| Abstraction | abstract class / interface — expose *what*, hide *how* |

```csharp
public override string Describe() => "A book";   // override (virtual base) = polymorphic
if (thing is Book bk) { ... }                     // is/as: safe test + cast
var maybe = thing as Book;                         // null if not a Book
```

| | Interface | Abstract class |
|---|---|---|
| Means | "can do" (capability) | "is a" (identity) |
| Implementation | none (contract) | can include implemented members |
| Per class | implement **many** | derive from **one** |
| State/fields | no | yes |

```csharp
public abstract string Describe();   // no body, subclass MUST override
public virtual string Format() => Describe();  // default body, override optional
```

- **Access:** `public` > `internal` (assembly) > `protected` (subclasses) > `private` (default). Tightest
  that works.
- **`override` vs `new`:** `override` = polymorphic; `new` = hides (depends on reference type — usually a
  bug).
- **Overload** = same name, different params, same class (compile-time). **Override** = replace `virtual`
  base method (runtime).
- **Multiple inheritance:** implement multiple interfaces (`class Drone : IFlyable, ICamera`).
- `partial` splits a class across files; `sealed` forbids inheritance on a leaf type.

## Collections — pick by access pattern

| Need | Type | Ops |
|---|---|---|
| default, ordered, index | `List<T>` | `Add`, `[i]` (O(1) index, O(n) search) |
| LIFO (return cart) | `Stack<T>` | `Push` / `Pop` / `Peek` (no index) |
| FIFO (holds line) | `Queue<T>` | `Enqueue` / `Dequeue` / `Peek` |
| cheap reorder, no index | `LinkedList<T>` | `AddFirst` / `AddLast` (O(1)) |
| fixed grid | `T[,]` | `grid[r,c]`, `GetLength(0/1)` |
| O(1) keyed lookup | `Dictionary<K,V>` | `dict[k]=v` add/overwrite, `TryGetValue` |
| uniqueness, membership | `HashSet<T>` | O(1) `Add`/`Contains` |

```csharp
if (dict.TryGetValue(id, out var item)) { }    // probe (indexer on miss -> KeyNotFoundException)
public class Shelf<T> { private T[] _slots; }   // your own generic type
public enum ItemKind { Book, Magazine }         // fixed set of choices
```

## IEnumerable, yield, lambdas

```csharp
public IEnumerator<T> GetEnumerator()           // makes the type foreach-able
{ foreach (var x in _items) yield return x; }   // lazy / deferred execution

List<T> Find(Predicate<T> match) { ... }        // lambda = behavior as data
var hits = catalog.Find(i => i.Author == "Martin");      // filter
items.Sort((a, b) => string.Compare(a.Title, b.Title));  // sort (Comparison<T>)
// NOTE for QC-1: use List.Sort + Predicate<T>; LINQ Where/OrderBy is a later week.
```

## Exceptions & stack traces

```csharp
try { repo.GetById(999); }
catch (ItemNotFoundException ex) { Log.Error("id {Id}", ex.Id); }  // specific FIRST
catch (LibraryException ex) { Log.Error(ex.Message); }              // base LAST
finally { /* always runs, even on throw */ }

public class ItemNotFoundException : Exception   // custom exception carries data
{ public int Id { get; } public ItemNotFoundException(int id) : base($"no id {id}") => Id = id; }

throw new ArgumentException("Name cannot be null");  // manual throw
throw;     // rethrow, PRESERVES original trace      (throw ex; resets it — avoid)
```

- **Read a stack trace TOP -> bottom.** Top frame = origin (file + line); frames below = how you got there.
- Routine outcome -> **return**; broken assumption -> **throw**. Never empty `catch {}`.

## Design patterns & SOLID

| SOLID | One line |
|---|---|
| S — Single Responsibility | one job per type |
| O — Open/Closed | extend without modifying (factory absorbs a new kind) |
| L — Liskov Substitution | a subtype works where its base is expected |
| I — Interface Segregation | small focused interfaces |
| D — Dependency Inversion | depend on an abstraction, not a concrete type |

| Pattern | Purpose |
|---|---|
| Design pattern | standardized reusable solution to a recurring problem |
| Repository | abstraction between data access and business logic (DI) |
| Factory method | one place builds the concrete type from a kind (Open/Closed) |
| Unit of Work | groups changes, commits as one transaction |
| Singleton | one shared instance + a static access point (e.g. Serilog `Log`) |

```csharp
public interface ILibraryRepository { LibraryItem GetById(int id); }  // contract
public class InMemoryLibraryRepository : ILibraryRepository { ... }    // implementation
```

## Async, HttpClient, regex, round-out

```csharp
private static readonly HttpClient Http = new();   // ONE per process (per-call leaks sockets)

public async Task<string> FetchAsync()             // async method returns Task<T>
{ return await Http.GetStringAsync(url); }          // await frees the thread (I/O wait)
// NEVER .Result / .Wait() (deadlock) ; NEVER async void (except event handlers)

var all = await Task.WhenAll(tasks);               // overlap independent awaits

bool ok = Regex.IsMatch(isbn, @"^\d{13}$");        // verbatim @"", anchored ^...$
Match m = Regex.Match(s, @"isbn:(\d{13})");        // group 0 = whole, 1.. = captures

string shelf = item switch                          // pattern-matching switch
{ Book b => $"Lending {b.Copies}", _ => "Unsorted" };

if (int.TryParse("42", out int n)) { }             // out returns extra value
int copies = maybe ?? 0;                            // nullable + null-coalescing
object boxed = 5; int x = (int)boxed;               // boxing / unboxing (generics avoid it)
```

| Regex token | Means |
|---|---|
| `\d \w \s` | digit / word char / whitespace |
| `. [a-z] [^…]` | any char / class / negated class |
| `* + ? {n} {n,m}` | 0+ / 1+ / 0-1 / exactly n / n..m |
| `^ $ | (…)` | start / end / alternation / capture group |
| `\.` | literal dot (escape metacharacters) |

## SDLC / Agile / Git (process)

- **SDLC:** Requirements -> Design -> Implementation -> Testing -> Deployment -> Maintenance.
- **Waterfall** = one strict pass (predictable, slow to change). **Agile** = short sprints, working
  software, embraces change. **Scrum** = dominant framework.
- **Scrum:** roles (Product Owner, Scrum Master, Dev Team); artifacts (product/sprint backlog, increment,
  user story); ceremonies (planning, standup, review, retro).
- **User story:** *"As a [user], I want [goal] so that [benefit]"* + acceptance criteria.

```bash
git init ; git status
git add . ; git commit -m "Add greeting logic"   # imperative, say WHY
git remote add origin <url> ; git branch -M main
git push -u origin main                           # later: git push
# .gitignore (add BEFORE first commit): bin/  obj/  .vs/  *.user
```
