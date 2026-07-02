# Loading Strategies and the Raw-SQL Escape Hatch

## Learning Objectives
- Choose between eager, explicit, and lazy loading for related data, and spot the N+1 trap.
- Explain why an unloaded navigation property is `null` and what bug that produces.
- Drop down to raw SQL with `FromSql` and `SqlQuery` when LINQ is the wrong tool.

## Why This Matters
Every interesting query crosses a relationship: inventory rows need their product's SKU, orders need their
lines. EF will not load related data unless you ask — a deliberate design that protects you from
accidentally dragging the whole object graph out of the database, but also the source of one of the most
common production `NullReferenceException`s there is. And once in a while — a stored procedure, a
hand-tuned report — the right query is SQL you already know how to write. EF has a supported door for that
too.

## The Concept

### Nothing is loaded until you ask
Query an `InventoryItem` and its `Product` navigation is `null` — EF generated one `SELECT` against one
table:

```csharp
var inv = db.Inventory.First();
var sku = inv.Product.Sku;        // NullReferenceException - Product was never loaded
```

This is why code that only needs the id should use the FK scalar `inv.ProductId` (always loaded — it is a
column on the row) rather than `inv.Product.Id` (a navigation that is not).

### Eager loading: Include, one round trip
`Include` joins the related data into the same query — the default choice when you know you need it:

```csharp
var rows = db.Inventory.Include(i => i.Product).ToList();   // one SELECT with a JOIN
// safe: rows[0].Product.Sku
```

Chain `ThenInclude` for deeper graphs (`Orders.Include(o => o.Lines).ThenInclude(l => l.Product)`). Eager
loading is the workhorse for reports and any endpoint that renders related data.

### Explicit loading: on demand, second query
Load the navigation later, only if you turn out to need it:

```csharp
var product = db.Products.First();                     // Inventory NOT loaded
db.Entry(product).Reference(p => p.Inventory).Load();  // second SELECT, on demand
var onHand = product.Inventory!.CurrentStock;          // now safe
```

Use `.Reference(...)` for single navigations, `.Collection(...)` for lists. Good when the related data is
needed on a rare branch; wasteful if you do it in a loop (see N+1).

### Lazy loading: automatic, and mostly a trap
With lazy loading (opt-in: proxies package + `virtual` navigations), *touching* a navigation property
silently triggers a query. It reads conveniently and then destroys you in a loop:

```csharp
foreach (var inv in db.Inventory.ToList())      // 1 query
    Console.WriteLine(inv.Product.Sku);         // +1 query PER ROW  ->  "N+1 problem"
```

100 rows = 101 round trips, each with network latency. The same loop over
`db.Inventory.Include(i => i.Product)` is one query. Many teams leave lazy loading off on purpose: prefer
being told (`null` means "you did not load this") over being silently slow. When you *choose* a strategy:
eager for known needs, explicit for rare branches, projection (`Select` only the columns you want) when
you do not need entities at all — projections load nothing extra by construction.

### Raw SQL: FromSql and SqlQuery
When the query is easier said in SQL — stored procedures, vendor-specific constructs, a report you already
tuned — EF Core drops down without giving up materialization:

```csharp
// entities from raw SQL (must return columns matching the entity)
var lowStock = db.Inventory
    .FromSql($"SELECT * FROM Inventory WHERE CurrentStock < {threshold}")
    .Include(i => i.Product)          // composable: EF wraps your SQL and keeps building
    .ToList();

// stored procedure
var products = db.Products.FromSql($"EXEC GetTopProducts @count={5}").ToList();

// scalar / non-entity results
var total = db.Database.SqlQuery<int>($"SELECT SUM(CurrentStock) AS Value FROM Inventory").Single();
```

Two safety notes. First, `FromSql` takes an *interpolated string handler* — the `{threshold}` above becomes
a SQL **parameter**, not string concatenation, so it is injection-safe; `FromSqlRaw` exists for dynamic SQL
and puts parameterization back in your hands (be careful with it). Second, raw SQL rows are still tracked
entities if queried through a `DbSet` — change tracking and `SaveChanges` work as usual.

Reach for raw SQL when SQL is genuinely the clearer tool — procs and hand-tuned reports — not as a habit;
most application queries stay LINQ-expressible.

## Say It in an Interview
- *"EF loads no related data unless asked — an unloaded navigation is `null`, which is why I use the FK
  scalar when the id is all I need."*
- *"Eager loading with `Include` is one joined query for known needs; explicit loading fetches on demand
  for rare branches; lazy loading fires a silent query per touch and turns loops into the N+1 problem —
  one query per row plus one."*
- *"Projections sidestep loading entirely: `Select` the columns you need and no entity graph exists to be
  unloaded."*
- *"For stored procedures or hand-tuned SQL, `FromSql` returns composable, tracked entities and
  parameterizes interpolated values — injection-safe — while `SqlQuery<T>` handles scalars and ad-hoc
  shapes."*

## Check Yourself
1. `db.Orders.First().Lines.Count` throws `NullReferenceException`. Why, and give two different fixes.
2. What exactly makes the N+1 problem slow — and what is the one-line cure in a known-need loop?
3. When is explicit loading the right choice over `Include`?
4. Why is `FromSql($"... WHERE Sku = {sku}")` injection-safe despite the interpolation?
5. You only need `Sku` and `CurrentStock` for 10,000 rows. What beats every loading strategy here?

**Answers:** (1) `Lines` was never loaded — navigations default to `null`. Fix with
`Include(o => o.Lines)`, explicit `db.Entry(order).Collection(o => o.Lines).Load()`, or project the count
in the query. (2) One database round trip per element, each paying network latency; cure:
`Include` the navigation so it is one joined query. (3) The related data is needed only on a rare code
path — pay the second query only when that branch runs. (4) The interpolated string handler turns each
hole into a SQL parameter — no string concatenation reaches the server. (5) A projection:
`Select(i => new { i.Product.Sku, i.CurrentStock })` — no entities, no navigations, minimal columns.

## Summary
- Navigations are unloaded (`null`) by default; use the FK scalar when the id is all you need.
- Eager (`Include`) = one joined query, the default for known needs. Explicit (`Entry(...).Load()`) = on
  demand. Lazy = automatic and the classic N+1 trap; many teams leave it off.
- Projections (`Select`) sidestep loading entirely when you only need columns.
- `FromSql` (entities, composable, parameterized) and `SqlQuery<T>` (scalars/ad-hoc types) are the
  supported raw-SQL doors — for procs and tuned reports, not habit.

## Resources
- [Loading related data (Microsoft Learn)](https://learn.microsoft.com/en-us/ef/core/querying/related-data/)
- [SQL queries in EF Core (FromSql, SqlQuery)](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries)
- [Efficient querying / avoiding N+1](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying)
