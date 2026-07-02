# LINQ: Querying Data and Building Reports

## Learning Objectives
- Query a `DbSet<T>` with LINQ method syntax: `Where`, `Select`, `OrderBy`, `First`/`FirstOrDefault`.
- Explain deferred execution and what actually sends SQL to the database.
- Build aggregate reports with `GroupBy`, `Sum`, and `Count`, and join on foreign keys.
- Produce a sorted report and run a binary search against it.

## Why This Matters
LINQ (Language Integrated Query) is how every read in an EF-backed service happens — inventory lists,
stock-tier breakdowns, top-sellers reports. The logic maps straight from SQL DQL: `Where` is your `WHERE`,
`GroupBy` is your `GROUP BY`, `Join` is your `JOIN` — except the query is C#, checked by the compiler, and
composable like any other code. Reporting is where it earns its keep: group-and-aggregate shapes cover
most business questions, and a *sorted* report turns ranking into a binary search.

## The Concept

### Method syntax on a DbSet
A `DbSet<T>` is queryable like any collection. Chain operators; each returns a new query:

```csharp
// filter + shape (projection)
var lowStock = db.Inventory
    .Where(i => i.CurrentStock < 5)
    .Select(i => new { i.ProductId, i.CurrentStock })
    .ToList();

// single element: First throws if none; FirstOrDefault returns null
var product = db.Products.FirstOrDefault(p => p.Sku == "BK-001");
```

Two syntaxes exist — method syntax (above) and query syntax (`from i in db.Inventory where ... select ...`).
They compile to the same thing; this library uses method syntax because it chains naturally and covers
operators query syntax cannot express.

### Deferred execution: nothing happens until you materialize
A LINQ chain builds a *description* of a query — an `IQueryable`. No SQL is sent until you **materialize**
it:

```csharp
var query = db.Inventory.Where(i => i.CurrentStock < 5);  // no SQL yet - just a query object
var list  = query.ToList();                               // NOW one SELECT runs
var count = query.Count();                                // a SECOND, different SELECT (COUNT(*))
```

Materializers: `ToList()`, `ToArray()`, `First()`, `Count()`, `Sum()`, iterating with `foreach`. Two
consequences:

1. You can compose queries conditionally (add a `Where` only if a filter was supplied) and pay for exactly
   one SQL statement at the end.
2. Materializing the same query twice hits the database twice. Store the result, not the query, when you
   need the data more than once.

Everything *before* the materializer is translated to SQL and runs in the database. Everything *after*
(`AsEnumerable()` or operating on the returned list) runs in your process — filter in the database, not in
memory, whenever you can.

### GroupBy: aggregation, straight from SQL
A stock-tier endpoint — `GroupBy` + aggregate, exactly like a SQL `GROUP BY`:

```csharp
app.MapGet("/inventory/by-value", (LibraryDbContext db) =>
    db.Inventory.Include(i => i.Product)
      .GroupBy(i => i.CurrentStock >= 5 ? "well-stocked" : "low")
      .Select(g => new { tier = g.Key, count = g.Count(), units = g.Sum(i => i.CurrentStock) })
      .ToList());
```

Each group `g` has a `Key` (what you grouped by) and is itself a sequence you can aggregate with `Count()`,
`Sum()`, `Max()`, `Average()`.

### A real report: join, group, sort
A top-products report joins fulfillment events to order lines **on the FK scalars**, groups by product,
and sorts descending:

```csharp
var ranked = db.FulfillmentEvents
    .Where(e => e.Type == "Fulfilled")
    .Join(db.OrderLines, e => e.OrderId, l => l.OrderId, (e, l) => l)
    .GroupBy(l => l.ProductId)
    .Select(g => new { ProductId = g.Key, Units = g.Sum(l => l.Quantity) })
    .OrderByDescending(x => x.Units)     // a SORTED report
    .ToList();
```

Because the report is sorted, finding the rank of a value is a binary search — O(log n), not a scan
(`Array.BinarySearch` needs a descending comparer to match the descending sort):

```csharp
var idx = Array.BinarySearch(unitsDesc, units,
    Comparer<int>.Create((a, b) => b.CompareTo(a)));
```

**Variations on the shape:** most reports are this same skeleton with the pieces swapped. Top *customers*
is the identical group-and-sum grouped by `CustomerId` — and if a navigation property exists
(`Order -> Customer`) you can navigate instead of writing the `Join`; EF generates the join for you. A
fulfillment-*rate* report needs no join at all: group events by `Type` (or orders by `Status`) and divide
one count by the other. If you can read the top-products query, you can write both.

## Say It in an Interview
- *"LINQ lets me query data in C# — `Where`, `Select`, `GroupBy`, `Join` — and EF Core translates the
  chain into one parameterized SQL statement."*
- *"Execution is deferred: the chain builds an `IQueryable`, and SQL only runs at a materializer like
  `ToList`, `First`, or `Count` — each materialization is its own round trip."*
- *"I keep filtering and aggregation on the `IQueryable` side so it runs in the database; once I call
  `AsEnumerable` or hold a list, everything after runs in memory."*
- *"`GroupBy` plus `Sum`/`Count` covers most reporting; sorting the report once makes rank lookups an
  O(log n) binary search instead of a scan."*

## Check Yourself
1. `var q = db.Products.Where(p => p.Price > 30);` — has any SQL run? When will it?
2. Why does calling `ToList()` and then `Count()` on the same query object cost two database round trips,
   and what is the fix when you need both?
3. What is the difference between `First` and `FirstOrDefault` when nothing matches?
4. Rewrite the idea "top customers by units bought" in terms of the top-products skeleton — what changes?
5. Your rank endpoint returns garbage indexes from `Array.BinarySearch` against a descending report. Why?

**Answers:** (1) No — the chain is a query description; SQL runs at a materializer (`ToList`, `First`,
`foreach`...). (2) Each materializer translates and executes independently; materialize once
(`var list = q.ToList()`) and use `list.Count`. (3) `First` throws `InvalidOperationException`;
`FirstOrDefault` returns `null`/default. (4) Group by `CustomerId` instead of `ProductId` — or navigate
`Order -> Customer` and let EF generate the join; the group-sum-sort skeleton is unchanged. (5)
`Array.BinarySearch` assumes ascending order — pass a descending comparer so the search agrees with the
sort.

## Summary
- LINQ method syntax queries `DbSet<T>` with `Where`/`Select`/`OrderBy`/`GroupBy`; EF translates the chain
  to SQL.
- Execution is deferred: SQL runs at materialization (`ToList`, `First`, `Count`, ...), once per
  materializer.
- `GroupBy` + `Sum`/`Count` is SQL aggregation in C#; `Join` on FK scalars or a navigation property covers
  relationships.
- Sort a report once and rank lookups become binary searches.
- Most reports are the same group-aggregate-sort skeleton with different keys.

## Resources
- [LINQ overview (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/csharp/linq/)
- [Querying data with EF Core](https://learn.microsoft.com/en-us/ef/core/querying/)
- [Complex query operators (GroupBy, Join) in EF Core](https://learn.microsoft.com/en-us/ef/core/querying/complex-query-operators)
