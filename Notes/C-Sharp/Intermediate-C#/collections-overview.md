# Collections: The Right Container for the Job

## Learning Objectives
- Choose between `List<T>`, `Stack<T>`, `Queue<T>`, and `LinkedList<T>` by matching a container to an access pattern.
- Use a multi-dimensional array for fixed, grid-shaped data.
- Read and write generic types, and explain what the `<T>` type parameter buys you.
- Distinguish a `struct` (value type) and an `enum` from a `class` (reference type), and pick the right one.

## Why This Matters
Week 1 left the catalog as a fixed `LibraryItem[]` — an array that cannot grow and offers exactly one access pattern: index in, index out. This week the epic is maturing that same `core-csharp-kata` into a production-shaped CLI, and the first move is replacing that rigid array with collections that match how a real library actually works. A return cart, a holds line, and a reading list are three different disciplines; storing all three in a plain array means re-implementing those disciplines by hand and getting them wrong at 2 a.m. The .NET collection types encode the discipline for you, so the container name *is* the documentation. Every framework you meet later — APIs, EF Core, test fixtures — is built on these types, so getting fluent now pays off all cohort.

## The Concept

### Match the container to the access pattern
There is one rule for today: pick the container that matches the access pattern. The wrong container still "works" and quietly costs you.

| Pattern in the library | .NET container | Shape |
|---|---|---|
| "the whole catalog" | `List<T>` | grows, ordered, index access |
| "return cart, reshelve top" | `Stack<T>` | LIFO (`Push` / `Pop`) |
| "holds line, first asked" | `Queue<T>` | FIFO (`Enqueue` / `Dequeue`) |
| "reading list I reorder" | `LinkedList<T>` | cheap insert anywhere, no index |
| "physical shelf unit (grid)" | `T[,]` | fixed rectangular block |

- **`List<T>` is the default.** Reach for it unless you have a reason not to. It is growable, ordered, `O(1)` to index, and `O(n)` to search.
- **`Stack<T>` and `Queue<T>` encode an order discipline.** They exist to make LIFO/FIFO intent unmissable, not to be faster than a list. `Stack` pushes and pops the top; `Queue` enqueues at the back and dequeues the front. Both expose `Peek` to look without removing. Neither is indexable — `stack[0]` does not compile, and that is deliberate.
- **`LinkedList<T>`** trades away index access for cheap insert/remove at any node (`AddFirst`/`AddLast` are `O(1)`). Use it when you reorder a lot and rarely random-access. Roughly 90% of the time `List<T>` is still the right answer; `LinkedList<T>` is the answer to a specific question — know it exists so you recognize the question.

### Multi-dimensional arrays: fixed grids
A multi-dimensional array `T[,]` is one rectangular block of `rows x cols`, fixed at creation. It models genuinely grid-shaped data — a shelf unit of aisles by shelves — not a general growable collection.

```csharp
LibraryItem?[,] grid = new LibraryItem?[2, 3];   // 2 aisles x 3 shelves
grid[0, 0] = catalog[0];
grid[1, 2] = catalog[2];
Console.WriteLine($"{grid.GetLength(0)} aisles x {grid.GetLength(1)} shelves");
```

`GetLength(0)` is rows, `GetLength(1)` is columns. The `?` marks slots that may be empty — a fresh reference array is all nulls. Note the contrast with a *jagged* array `T[][]`, which is an array of arrays where rows can differ in length. Use rectangular `[,]` when the grid is uniform; reach for neither as a general-purpose collection, because both are fixed-size.

### Generics: one type, every element type
Every container above carries a `<T>` — that is the real headline. A generic type takes a type parameter filled in at the use site: `List<LibraryItem>` is "a list *of* library items," checked at compile time. Add a `string` and it will not compile — no casting, no runtime surprises. You can write your own generic type, which demystifies how `List<T>` works:

```csharp
public class Shelf<T>
{
    private readonly T[] _slots;
    private int _used;

    public Shelf(int capacity) => _slots = new T[capacity];
    public int Count => _used;

    public bool TryAdd(T item)
    {
        if (_used == _slots.Length) return false;   // full: let the caller decide
        _slots[_used++] = item;
        return true;
    }
}
```

`new Shelf<LibraryItem>(2)` makes a shelf of items; `new Shelf<string>(10)` a shelf of strings — one class, full type safety. This is exactly what `List<T>`, `Stack<T>`, and `Queue<T>` are: generic classes in the library, not language keywords. Note the `TryAdd` returning `bool` rather than throwing when full — the `Try...` idiom you will see again in `int.TryParse`.

### Value types: `enum` and `struct` vs `class`
Not everything should be a class. Two value types fill specific roles:

- **An `enum` names a fixed, closed set of choices.** `if (kind == ItemKind.Magazine)` cannot be misspelled and is compiler-checked, unlike comparing a raw `"magazine"` string.
- **A `struct` bundles a small amount of data with no identity** — like `int` and `bool`. Two `ShelfLocation` values with the same aisle and shelf are equal, full stop.

```csharp
public enum ItemKind { Book, ReferenceBook, Magazine }

public readonly struct ShelfLocation
{
    public int Aisle { get; }
    public int Shelf { get; }
    public ShelfLocation(int aisle, int shelf) => (Aisle, Shelf) = (aisle, shelf);
}
```

The hinge is value vs reference (the Week 1 callback): assigning a struct **copies the data**, so the copy is independent; assigning a class copies the **reference**, so both names see the same object. Rule of thumb: small, immutable, identity-less → `struct`; has its own lifetime and identity (a `Book`) → `class`. Make structs `readonly` by default so a copy stored in a list cannot surprise you with a mutation that edits the wrong instance.

## Code Example
The `Catalog` exposes each access pattern through a narrow surface, keeping the real collections private:

```csharp
public class Catalog
{
    private readonly List<LibraryItem> _items = new();         // the whole catalog
    private readonly Stack<LibraryItem> _returnCart = new();    // LIFO
    private readonly Queue<string> _holdQueue = new();          // FIFO

    public void Add(LibraryItem item) => _items.Add(item);
    public LibraryItem this[int index] => _items[index];        // indexer: array-like read

    public void DropInReturnCart(LibraryItem item) => _returnCart.Push(item);
    public LibraryItem Reshelve() => _returnCart.Pop();         // last dropped, first reshelved

    public void PlaceHold(string member) => _holdQueue.Enqueue(member);
    public string ServeNextHold() => _holdQueue.Dequeue();      // first asked, first served
}
```

The collections stay `private`; callers get only the access pattern you chose to expose — encapsulation from Week 1, applied to data structures.

> Heads up: keyed lookup with `Dictionary<K,V>`, set membership with `HashSet<T>`, making a type iterable with `IEnumerable`/`yield`, and LINQ are *not* today's topic — they arrive Wednesday in `advanced-classes`.

## Summary
- **Match container to access pattern** — `List` (default), `Stack` (LIFO), `Queue` (FIFO), `LinkedList` (cheap reorder, no index).
- **`T[,]` is a fixed rectangular grid** — for genuinely grid-shaped data, not a growable collection; jagged `T[][]` differs in that rows can vary.
- **Generics (`<T>`) give one type, any element, full compile-time safety** — the collection types are themselves generic classes you could write.
- **`enum`** names a fixed set of choices; **`struct`** bundles small, immutable, identity-less data.
- **Value vs reference** — assigning a struct copies data (copy is independent); assigning a class copies the reference (both see changes).

## Additional Resources
- [Collections (C#) — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/collections)
- [Generics — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics)
- [Structure types and enums — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct)
