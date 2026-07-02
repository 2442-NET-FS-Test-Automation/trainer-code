# EF Core, the DbContext, and Dependency Injection

## Learning Objectives
- Explain what an ORM is and what EF Core does for you (and what it does not).
- Describe the role of the `DbContext` class and its `DbSet<T>` properties.
- Create an EF Core model using code conventions, and recognize what convention infers for you.
- Register a `DbContext` in the DI container with `AddDbContext` and explain why its lifetime is scoped.
- Say what Table-Per-Hierarchy mapping is at an awareness level.

## Why This Matters
Everything a data-backed service does — seed a catalog, take orders, decrement stock — is ultimately rows
in a relational database. You can write that SQL by hand; an ORM (object-relational mapper) is the layer
that lets your C# objects *be* the rows instead: you write C# and LINQ, and EF Core generates the
`SELECT`/`INSERT`/`UPDATE`/`DELETE`, tracks what changed, and keeps the schema in sync. Every entity,
every query, and every save flows through one class: the `DbContext`. Understanding what it is, what it
tracks, and how it reaches your endpoints through dependency injection is the foundation everything else
in EF stands on.

## The Concept

### What an ORM buys you
Without an ORM you write connection code, SQL strings, and row-to-object mapping for every operation. With
EF Core:

- **Classes are tables.** A `Product` class maps to a `Products` table; each instance is a row.
- **LINQ is your query language.** `db.Products.Where(p => p.Price > 30)` becomes a parameterized `SELECT`.
- **Change tracking replaces hand-written UPDATEs.** Modify a tracked object, call `SaveChanges()`, EF
  writes the SQL (covered in depth in `change-tracking-seeding.md`).
- **Migrations replace hand-run DDL.** Your model generates the schema (covered in
  `code-first-data-first.md`).

What it does *not* do: free you from understanding SQL. EF generates SQL, and when a query is slow or a
constraint fires, you read the generated SQL to know why. Raw SQL skills stay load-bearing.

### An entity by convention
An **entity** is a plain C# class EF maps to a table. Convention does most of the work:

```csharp
public class Product
{
    public int Id { get; set; }          // named "Id" -> primary key, identity
    public string Sku { get; set; }
    public string Name { get; set; }

    [Precision(10, 2)]                   // one annotation where convention isn't enough
    public decimal Price { get; set; }

    public InventoryItem? Inventory { get; set; }   // navigation property -> a relationship
}

public class InventoryItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }               // "{Nav}Id" -> foreign key by convention
    public Product Product { get; set; } = default!;
    public int CurrentStock { get; set; }
}
```

Conventions at work:

| You write | EF infers |
|---|---|
| property named `Id` (or `ProductId` on `Product`) | primary key, identity column |
| `public InventoryItem? Inventory` on `Product` | a relationship exists |
| `public int ProductId` next to `public Product Product` | that property is the foreign key |
| CLR types (`int`, `string`, `decimal`, `DateTime?`) | column types (`int`, `nvarchar`, `decimal`, nullable `datetime2`) |

Convention is the *lowest* layer of configuration. When it is not enough you add Data Annotations, and when
those are not enough you use the Fluent API — the full hierarchy is in `annotations-fluent-api.md`.

### The DbContext: your session with the database
All of the machinery — opening connections, generating SQL, tracking changes — lives in EF's `DbContext`
class. You never edit that class; you inherit from it:

```csharp
public class LibraryDbContext : DbContext
{
    // DI calls this constructor and supplies the options (provider + connection string)
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    // Each DbSet<T> registers an entity and becomes a queryable table
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();
    public DbSet<Order> Orders => Set<Order>();
}
```

Think of a `DbContext` instance as a **unit of work**: a short-lived session that loads some entities,
tracks your changes to them, and pushes those changes back in one `SaveChanges()`. It is deliberately cheap
to create and meant to be thrown away — not a long-lived god object, and **not thread-safe** (a fact that
becomes critical under concurrency — see `efcore-concurrency.md`).

### Registering it in DI
ASP.NET Core builds objects for you through its dependency-injection container. You register the context
once at startup:

```csharp
var conn = builder.Configuration.GetConnectionString("Library")
    ?? "Server=localhost,1433;Database=LibraryDb;User Id=sa;Password=<local-dev-only>;TrustServerCertificate=true";

builder.Services.AddDbContext<LibraryDbContext>(options => options.UseSqlServer(conn));
```

and then any endpoint or service just declares it as a parameter:

```csharp
app.MapGet("/inventory", async (LibraryDbContext db) =>
    await db.Inventory.ToListAsync());
```

`AddDbContext` registers the context with a **scoped** lifetime: one instance per HTTP request. That is
exactly the unit-of-work shape — each request gets a fresh session, and the context is disposed when the
response goes out. You never `new` it up and never dispose it yourself in request code; the container owns
the lifecycle. (The in-code fallback connection string above is a local-development convenience so a bare
`dotnet run` works — production services carry no credentials in source; configuration sources are covered
in `../02-rest-http/minimal-api-hosting.md`.)

### Table-Per-Hierarchy, at awareness level
If entities form an inheritance tree (say `Order` had subclasses `RushOrder` and `BulkOrder`), EF's default
mapping is **Table-Per-Hierarchy (TPH)**: one table for the whole tree plus a *discriminator* column that
records which subclass each row is. Alternatives (table-per-type, table-per-concrete-type) trade joins for
sparseness. Many domains deliberately keep inheritance out of the data model — know the term and the
discriminator idea; you configure it only when a domain genuinely needs polymorphic rows.

## Say It in an Interview
- *"An ORM maps classes to tables and objects to rows, generating the SQL for you — EF Core is .NET's
  default. It doesn't excuse you from SQL: when a query misbehaves, you read what it generated."*
- *"The `DbContext` is a unit of work — a short-lived session that exposes `DbSet<T>` properties, tracks
  changes to loaded entities, and persists them in one `SaveChanges()`. It's cheap to create and not
  thread-safe."*
- *"Convention infers most of the model: an `Id` property becomes the primary key, a navigation property
  plus `{Nav}Id` becomes a foreign key. Annotations, then Fluent API, layer on where convention falls
  short."*
- *"`AddDbContext` registers it scoped — one instance per HTTP request, matching the unit-of-work
  lifetime — and endpoints receive it by constructor or parameter injection; the container owns disposal."*

## Check Yourself
1. What three chores does an ORM take off your hands, and what SQL responsibility stays yours?
2. What does EF infer from `public int ProductId { get; set; }` sitting next to
   `public Product Product { get; set; }`?
3. Why is the scoped lifetime the right one for a `DbContext` in a web app?
4. Your teammate stores one `DbContext` in a static field "to save allocations." Give two reasons this
   breaks.
5. What is a discriminator column and which mapping strategy uses it?

**Answers:** (1) Connection/SQL generation, row-to-object mapping, and change tracking/DDL via
migrations; still yours — reading generated SQL to diagnose slowness and constraint errors. (2) That
`ProductId` is the foreign key for the `Product` relationship. (3) One instance per request = one unit of
work per request; fresh tracker, disposed with the response. (4) It is not thread-safe (concurrent
requests would share it), and its change tracker grows forever — a stale, ever-heavier session. (5) A
column recording each row's concrete subclass; Table-Per-Hierarchy.

## Summary
- An ORM maps classes to tables and generates SQL; EF Core is .NET's default ORM.
- Entities are plain classes; conventions infer keys, foreign keys, and column types from names and CLR
  types. Annotations and Fluent API layer on top when convention falls short.
- `DbContext` is a short-lived unit-of-work session: `DbSet<T>` properties expose tables, the change
  tracker records edits, `SaveChanges()` persists them.
- `AddDbContext` registers it scoped (one per request); endpoints receive it by parameter injection and
  never manage its lifetime.
- TPH = one table per inheritance tree + a discriminator column; know it exists.

## Resources
- [EF Core overview (Microsoft Learn)](https://learn.microsoft.com/en-us/ef/core/)
- [DbContext lifetime, configuration, and initialization](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [Keys and relationships by convention](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/conventions)
