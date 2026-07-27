# EF Core Testing Strategies: Choosing and Managing a Test Database

## Learning Objectives
- Compare the three EF Core test-database strategies — InMemory provider, SQLite, real engine — and pick
  the right one for a given suite.
- Set up SQLite in-memory mode correctly (keep the connection open) and recognize its provider-fidelity
  limits, including the `rowversion` concurrency-token failure and its standard workaround.
- Seed test data deterministically: `EnsureCreated` vs migrations, a seed helper per fixture, small seeds.
- Choose an isolation strategy — fresh database per test vs shared database with transaction rollback —
  and state the trade-off of each.

## Why This Matters
Any test that runs a real LINQ query needs a database behind the `DbContext`, and which database you choose
quietly decides what your tests can prove. Pick the fastest option and your suite goes green while
foreign-key violations, transaction bugs, and untranslatable queries sail through to production. Pick the
highest-fidelity option everywhere and the suite takes minutes and needs infrastructure on every machine.
Teams that get this right treat it as an explicit point on the speed/fidelity curve, plus a seeding and
isolation discipline that keeps tests deterministic in parallel and in any order. It is also a reliable
interview topic, because the wrong answer ("just use the InMemory provider") is common enough that
interviewers probe for it.

## The Concept

Each strategy below is a provider package added to the **test** project; the application keeps whatever
provider it uses in production. Add only the one the strategy needs:

```bash
dotnet add YourProject.Tests package Microsoft.EntityFrameworkCore.InMemory   # Strategy 1
dotnet add YourProject.Tests package Microsoft.EntityFrameworkCore.Sqlite     # Strategy 2
```

Strategy 3 needs no new *provider* package — it reuses the production provider already referenced through
the project under test — though the containerized form of it does add one (`Testcontainers.MsSql` or its
sibling for your engine).

### Strategy 1: the EF InMemory provider — fast, and not a database
`Microsoft.EntityFrameworkCore.InMemory` stores entities in .NET collections. It is trivial to set up and
very fast, but it is **not relational**: it does not enforce foreign keys, ignores transactions (a rolled
back transaction leaves its writes in place), and performs no SQL translation — a LINQ query that could
never translate to SQL happily executes in memory. Microsoft explicitly discourages it for meaningful
tests. Setup is one call — `.UseInMemoryDatabase("checkout-tests")` on the options builder, where the
argument is a database *name*, not a connection string. A passing InMemory test tells you your LINQ runs
against a `List<T>`; it says little about your database code.

### Strategy 2: SQLite — a real relational engine with no infrastructure
`Microsoft.EntityFrameworkCore.Sqlite` runs real SQL against a real relational engine: foreign keys,
transactions, and actual query translation, all from a NuGet package. Its in-memory mode
(`DataSource=:memory:`) gives each open connection its own throwaway database — with one hard rule: **the
database lives exactly as long as the connection**, so the connection must stay open for the fixture's
lifetime or the schema and data vanish mid-test. A throwaway file works too when you want to inspect the
database afterward.

```csharp
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class SqliteLibraryFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public DbContextOptions<LibraryContext> Options { get; }

    public SqliteLibraryFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();                       // close this and the database is gone

        Options = new DbContextOptionsBuilder<LibraryContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new LibraryContext(Options);
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();
}
```

The cost is provider fidelity: SQLite is not SQL Server. Concrete worked failure — suppose `Inventory`
carries a SQL Server concurrency token:

```csharp
using System.ComponentModel.DataAnnotations;

public class Inventory
{
    public int Id { get; set; }
    public int CurrentStock { get; set; }

    [Timestamp]                                   // SQL Server rowversion
    public byte[] RowVersion { get; set; } = default!;
}
```

SQLite has no `rowversion` type and no database-generated concurrency tokens. `EnsureCreated` can fail
when seeding rows (a `SqliteException` in the SQLite Error 19 constraint-violation shape, because the
generated column cannot be populated the way SQL Server would populate it), and even when creation
succeeds, concurrency behavior differs from production. The standard workaround is a test-only
`DbContext` subclass that reconfigures or ignores the provider-specific property:

```csharp
using Microsoft.EntityFrameworkCore;

public class SqliteTestLibraryContext : LibraryContext
{
    public SqliteTestLibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // rowversion is SQL Server-specific; drop it from the test model
        modelBuilder.Entity<Inventory>().Ignore(i => i.RowVersion);
    }
}
```

That keeps the rest of the suite honest — but note the concession: the concurrency behavior itself is now
untested on SQLite, exactly the kind of gap the third strategy closes. Other fidelity gaps: case
sensitivity differs (SQL Server compares case-insensitively by default, SQLite does not), and
`decimal`/`DateTimeOffset` have limited support.

### Strategy 3: the real database engine
Run tests against the same engine production uses — for example SQL Server in a container. This is the
highest-fidelity option: every translation, constraint, transaction, and concurrency behavior is the real
one, and it is the only strategy that can test raw SQL or provider-specific functions. The costs are speed
(engine startup and real I/O) and infrastructure (an engine must exist wherever tests run — developer
machines and CI). Testcontainers, a library that manages the container lifecycle from test code, is the
one-line answer interviewers expect when they ask how you would run this in CI.

### Which to choose

| Strategy | Speed | Fidelity | Infrastructure |
| --- | --- | --- | --- |
| EF InMemory provider | Fastest | Lowest — not relational; no FKs, no transactions, no SQL | None |
| SQLite (in-memory or file) | Fast | Medium — real SQL and constraints, but not your engine's dialect | None (NuGet only) |
| Real engine (e.g. SQL Server container) | Slowest | Highest — production behavior exactly | Engine or container required |

Practical default: SQLite for the broad middle of query-and-save tests, the real engine for anything
touching provider-specific behavior (concurrency tokens, raw SQL, case-sensitivity-dependent queries), and
the InMemory provider only where the database is incidental to what the test proves.

### Seeding test data
In tests, `EnsureCreated()` builds the schema directly from the current model — fast and right for
throwaway databases, but it does not run migrations, so it cannot catch migration bugs; use
`context.Database.Migrate()` when migration correctness is itself under test (typically against the real
engine). Put seeding in one helper on the fixture so every test starts from the same known state, and keep
seeds **small and per-test-meaningful**: a three-book catalog you can hold in your head beats fifty rows
of noise, because when a count assertion fails you can see why.

```csharp
private static void Seed(LibraryContext context)
{
    context.Books.AddRange(
        new Book { Title = "Clean Code", Author = "Robert C. Martin" },
        new Book { Title = "The Pragmatic Programmer", Author = "Hunt and Thomas" });
    context.SaveChanges();
}
```

### Test isolation: fresh database per test vs shared database + rollback
Tests that write must not see each other's writes. Two standard strategies:

**Fresh database per test.** Each test gets its own database — a unique `UseInMemoryDatabase` name, a new
SQLite `:memory:` connection, or a uniquely named database on the real engine. Isolation is total and
parallel-safe; the cost is paying schema creation and seeding per test, which adds up on slower engines.

**Shared database + transaction rollback.** Build and seed once per fixture; each test opens a transaction
in the constructor and rolls it back in `Dispose`, so its writes never land:

```csharp
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

public class CheckoutTests : IClassFixture<SqliteLibraryFixture>, IDisposable
{
    private readonly LibraryContext _context;
    private readonly IDbContextTransaction _transaction;

    public CheckoutTests(SqliteLibraryFixture fixture)
    {
        _context = new LibraryContext(fixture.Options);
        _transaction = _context.Database.BeginTransaction();
    }

    public void Dispose()
    {
        _transaction.Rollback();
        _transaction.Dispose();
        _context.Dispose();
    }
}
```

This is fast — one schema build for the whole class — but it has a known breaking point: **it fails when
the code under test manages its own transactions**, because databases do not nest transactions; a service
that calls `BeginTransaction`/`Commit` itself either errors or commits for real, defeating the rollback.
Those tests need their own database plus explicit cleanup. Two adjacent tools worth naming: Respawn resets
a shared database to a checkpoint by intelligently deleting data (faster than rebuilding), and
`EnsureDeleted()` is the blunt alternative — correct, but drop-and-recreate per test is usually the
slowest option on a real engine.

### Deterministic data
Tests that assert on counts or ordering are only as reliable as their baseline. `Assert.Equal(2,
books.Count)` is meaningless if a previously run test may have inserted a third book. Reset or seed
explicitly in the fixture or constructor, never assume leftover state, and never depend on execution
order — runners parallelize and reorder freely, and a suite that only passes in one order is already
broken.

## Say It in an Interview
- *"There are three test-database options: the EF InMemory provider — fastest but not relational, no FKs
  or transactions, and Microsoft discourages it; SQLite in-memory — real SQL and constraints with zero
  infrastructure; and the production engine in a container — highest fidelity, slowest, and the only way
  to test provider-specific behavior."*
- *"With SQLite `:memory:` the database lives as long as the connection, so the fixture opens it and holds
  it. Fidelity gaps show up on things like a SQL Server rowversion token — SQLite can't generate it, so
  the standard fix is a test-only DbContext subclass that ignores that property, and the concurrency
  behavior itself moves to real-engine tests."*
- *"For isolation I either give each test a fresh database — total isolation, but you pay schema and seed
  per test — or share one database and wrap each test in a transaction rolled back in Dispose, which is
  fast but breaks when the code under test commits its own transactions."*
- *"Seeds live in one fixture helper, small enough to reason about, and every test asserts against that
  explicit baseline — never against whatever a previous test left behind."*

## Check Yourself
1. Why does Microsoft discourage the InMemory provider for meaningful tests? Name two relational behaviors
   it lacks.
2. What is the one hard rule for using SQLite `DataSource=:memory:`, and what happens if you break it?
3. Your entity has a `[Timestamp]` rowversion property and `EnsureCreated` fails on SQLite when seeding.
   What is the standard workaround, and what does it concede?
4. When would you use `Migrate()` instead of `EnsureCreated()` in a test fixture?
5. The transaction-rollback isolation pattern is fast — when does it break, and why?

**Answers:** (1) It is not a relational database: it does not enforce foreign keys, ignores transactions,
and performs no SQL translation, so queries that would fail or behave differently on the real engine pass
silently. (2) The connection must stay open for the database's whole lifetime — an in-memory SQLite
database is destroyed the moment its connection closes, taking schema and data with it. (3) A test-only
`DbContext` subclass whose `OnModelCreating` ignores (or reconfigures) the provider-specific property,
since SQLite has no rowversion; the concession is that concurrency-token behavior is untested there and
needs a real-engine test. (4) When migration correctness is itself part of what you are testing —
`EnsureCreated` builds schema straight from the model and skips migrations entirely. (5) It breaks when
the code under test begins and commits its own transactions: transactions do not nest, so the inner commit
either errors or persists for real and the rollback no longer isolates anything — those tests need a
dedicated database with explicit cleanup (Respawn is the efficient tool for that reset).

## Summary
- Three strategies: InMemory (fastest, not relational, discouraged for meaningful tests), SQLite (real SQL
  with no infrastructure), the real engine (highest fidelity, slowest — Testcontainers automates it).
- SQLite `:memory:` databases live and die with their connection — open it in the fixture and hold it.
- Provider fidelity is real: SQL Server `rowversion` tokens fail on SQLite (Error 19 shape at seed time);
  the workaround is a test-only `DbContext` subclass ignoring the property, at the cost of not testing
  that behavior there.
- `EnsureCreated` builds schema from the model and skips migrations; use `Migrate()` when migrations are
  under test. Seed through one fixture helper, small and per-test-meaningful.
- Isolation: fresh database per test (total isolation, pay setup per test) or shared database +
  rollback-in-`Dispose` (fast, but breaks when code under test commits its own transactions).
- Deterministic tests reset or seed explicitly — never assert counts or order against leftover state or
  execution order. Respawn does checkpoint-based resets; `EnsureDeleted` per test is usually slowest.

## Resources
- [Choosing a testing strategy — EF Core (learn.microsoft.com)](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy)
- [Testing against your production database system — EF Core (learn.microsoft.com)](https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database)
- [SQLite provider limitations — EF Core (learn.microsoft.com)](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations)
