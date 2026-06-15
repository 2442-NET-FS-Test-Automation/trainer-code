# Advanced Classes & Collections

## Learning Objectives
- Use `Dictionary<K,V>` for O(1) keyed lookup and `HashSet<T>` for uniqueness, and choose between them.
- Make a type iterable by implementing `IEnumerable<T>` with `yield`, and explain deferred execution.
- Filter a collection with a lambda passed as a `Predicate<T>` delegate, and sort one with a `Comparison<T>` lambda.
- Apply `partial` and `sealed` classes and expression-bodied members, and explain what garbage collection does for you.

## Why This Matters
Monday gave you containers chosen by access pattern; Tuesday wrapped storage behind a repository whose `GetById` still scanned a list. Today sharpens the internals so the kata behaves the way production code does: lookups become instant, uniqueness is enforced by the data structure, and the catalog becomes something you can `foreach` directly and filter on the fly. These are the building blocks under LINQ, under EF Core queries, and under most framework code you will read — so seeing the mechanism by hand, before the library magic, is what makes the magic stop being magic. It directly advances the week's epic: a production-shaped CLI whose data layer is fast and expressive, ready for a real database to slot in behind it.

## The Concept

### Garbage collection: you never free
Before adding data structures, one thing about memory. In C# you `new` objects onto the managed heap and never free them — the garbage collector reclaims what becomes unreachable, on its own schedule. Setting `reference = null` does not free anything; it just removes one reference. Objects start in Gen 0 (collected often and cheaply); survivors are promoted to Gen 1, then Gen 2 (collected rarely). Most objects die young, which is what makes it fast. Finalizers and `IDisposable` exist for *unmanaged* resources (files, sockets, database connections) — you will meet `using`/`IDisposable` when you open a network or DB connection later. For pure managed memory you do nothing: observe, do not manage.

### `Dictionary<K,V>`: O(1) keyed lookup
A dictionary maps a key to a value, and lookup by key is `O(1)` — a hash, not a scan. Swapping the repository's backing list for a dictionary keyed by id turns yesterday's linear `GetById` into a constant-time lookup without changing the public contract:

```csharp
private readonly Dictionary<int, LibraryItem> _byId = new();

public void Add(LibraryItem item) => _byId[item.Id] = item;   // adds or overwrites

public LibraryItem GetById(int id)
{
    if (_byId.TryGetValue(id, out LibraryItem? item)) return item;
    throw new ItemNotFoundException(id);
}
```

`dict[key] = value` adds or overwrites; `dict.Add(key, value)` throws on a duplicate key. Reading a missing key with the indexer throws `KeyNotFoundException`, so probe with `TryGetValue` or `ContainsKey`. (`TryGetValue` hands the result back through an `out` parameter — you will cover `out` formally tomorrow in `06`; for now it is how a method returns more than one thing.)

### `HashSet<T>`: uniqueness
A `HashSet<T>` is a set: unique membership, no order, `O(1)` `Contains`/`Add`/`Remove`. It uses the same hashing machinery as a dictionary, but it stores only the keys — the question it answers is "is this present?"

```csharp
private readonly HashSet<string> _authors = new();
// Add the same author twice -> one entry, no exception, no manual check.
public void Add(LibraryItem item) => _authors.Add(item.Author);
```

Reach for a `HashSet` when you only care about membership or uniqueness; a `Dictionary` when you need an associated value. Sets also support `UnionWith`, `IntersectWith`, and `ExceptWith` for combining.

### `IEnumerable<T>` and `yield`: make a type iterable
`IEnumerable<T>` is the contract behind `foreach`. Implement `GetEnumerator()` and the whole type becomes `foreach`-able directly — no `.Items` accessor needed. `yield return` builds the sequence lazily: the compiler turns the method into a state machine that hands back one item and pauses until the caller asks for the next.

```csharp
public partial class Catalog : IEnumerable<LibraryItem>
{
    public IEnumerator<LibraryItem> GetEnumerator()
    {
        foreach (LibraryItem item in _items)
            yield return item;          // one at a time, on demand
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();   // explicit, routes to the generic
}
```

The key idea is **deferred execution**: a `yield` method does no work when you call it — work happens only as you enumerate the result. Store it and nothing has happened yet. That laziness is the superpower and the footgun: if the source changes before you enumerate, you see the new state. Note the second, explicit `IEnumerable.GetEnumerator()` — the non-generic one the interface still requires, routed to the generic one.

### Lambdas and `Predicate<T>`: behavior as data
A lambda is an inline, anonymous function. `item => item.Author == "Robert C. Martin"` reads "given an item, return whether its author is Martin" (`=>` is "goes to"). A `Predicate<T>` is a built-in delegate — a function taking a `T` and returning `bool` — and a delegate is a variable that holds a method, so a method can take *behavior* as an argument:

```csharp
public List<LibraryItem> Find(Predicate<LibraryItem> match)
{
    List<LibraryItem> hits = new();
    foreach (LibraryItem item in _items)
        if (match(item)) hits.Add(item);
    return hits;
}

var byMartin = catalog.Find(item => item.Author == "Robert C. Martin");
```

The caller decides the test; the catalog just applies it. This is exactly how `List<T>.FindAll` works, and the seed for LINQ — `Where`, `Select`, and `OrderBy` are the same idea (functions passed to collection methods). We hand-roll one filter here so the magic is gone before you meet the LINQ library later.

The same "behavior as data" idea **sorts**. `List<T>.Sort` takes a `Comparison<T>` lambda — given two items, return a negative number, zero, or positive to say which comes first — so the caller supplies the ordering rule just as it supplied the filter test:

```csharp
List<LibraryItem> items = catalog.Find(_ => true);          // grab them all

items.Sort((a, b) => string.Compare(a.Title, b.Title));     // A -> Z by title
items.Sort((a, b) => b.Title.Length - a.Title.Length);      // longest title first
```

Same lambda mechanism, now deciding *order* instead of *membership*. LINQ's `OrderBy` wraps this; we call `List.Sort` directly so the comparison is visible.

### `partial`, `sealed`, and expression-bodied members
- **`partial`** lets one class span multiple files; the compiler merges them into a single type. Use it to separate generated code from hand-written, or to group a big class by theme (storage in one file, queries in another). Splitting the file does not fix a class doing too many jobs — that is still the S in SOLID.
- **`sealed`** forbids inheritance. Use it on a leaf type that was never designed to be a base; it states intent, avoids fragile subclassing, and lets the runtime devirtualize some calls. It is the opposite end from an `abstract` base.
- **Expression-bodied members** use `=>` as shorthand for a single-expression body: `public bool IsEmpty => _items.Count == 0;` is exactly `get { return _items.Count == 0; }`. Use them for trivial members; if it needs branches or loops, write a full block — readability wins.

## Code Example
A lambda filter and a deferred iterator, driven together:

```csharp
Catalog catalog = new Catalog();
foreach (LibraryItem item in repo.GetAll())
    catalog.Add(item);

Console.WriteLine($"Unique authors: {catalog.Authors.Count}");          // HashSet de-duped
List<LibraryItem> byMartin = catalog.Find(item => item.Author == "Robert C. Martin");
Console.WriteLine($"By Martin: {byMartin.Count}");                       // lambda picked them

foreach (LibraryItem item in catalog.Lendable())                         // yield: lazy
    Console.WriteLine($"  - {item.Title}");
```

Two books by the same author collapse to one entry in the `HashSet`; the lambda picks both Martin books; the `Lendable()` iterator yields items only as the `foreach` pulls them.

> Heads up: LINQ itself, `IComparer<T>`/sorting, and `out` parameters in depth are not today's topic — the formal `out` lesson (with `int.TryParse`) and the rest of the language round-out arrive tomorrow in `async-http-networking`.

## Summary
- **Garbage collection reclaims unreachable objects** — you never `free`; `IDisposable`/`using` is only for unmanaged resources.
- **`Dictionary<K,V>` gives O(1) keyed lookup**; the indexer adds/overwrites, `TryGetValue` probes without throwing.
- **`HashSet<T>` enforces uniqueness** with O(1) membership — use it for "is this present?", a dictionary for an associated value.
- **`IEnumerable<T>` + `yield` make a type `foreach`-able lazily** — deferred execution: work happens during enumeration, not at the call.
- **A lambda passed as `Predicate<T>` is behavior as data** — the mechanism under `FindAll` and LINQ; the same idea sorts via `List.Sort` with a `Comparison<T>` lambda.
- **`partial`** splits a type across files, **`sealed`** locks a leaf type, expression-bodied `=>` trims trivial members.

## Additional Resources
- [Dictionary and HashSet collections — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)
- [Iterators (`yield`) — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/iterators)
- [Lambda expressions — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions)
