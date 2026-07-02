# Code-First vs Data-First, and the Migrations Workflow

## Learning Objectives
- Describe the code-first and data-first approaches and choose the right one for a given situation.
- Scaffold entity classes from an existing database (data-first).
- Generate and apply migrations to evolve a schema (code-first) without breaking changes or data loss.
- Modify a generated migration before executing it, and say when you would.

## Why This Matters
On a greenfield build the C# model is the source of truth and the schema is *generated* from it — that is
code-first. But most jobs hand you a database that already exists — DBA-owned, shared, older than the team
— and there the schema is the source of truth: that is data-first, and scaffolding is how you get classes
from it. Knowing both directions, and knowing how to evolve a code-first schema through migrations without
destroying data, is the difference between a toy project and a service someone can keep running.

## The Concept

### Two directions, one mapping

| | Code-first | Data-first (database-first) |
|---|---|---|
| Source of truth | your C# classes | the existing database |
| Schema comes from | migrations generated from the model | already there |
| Classes come from | you write them | `dotnet ef dbcontext scaffold` generates them |
| When to use | greenfield, you own the schema | legacy/shared databases, DBA-owned schemas |

They are not rivals; they are directions. Some teams even scaffold once from a legacy database, then switch
to code-first for everything after.

### Data-first: scaffold from an existing database
One command reads the schema and writes the entity classes plus a configured `DbContext`:

```
dotnet ef dbcontext scaffold "Server=localhost,1433;Database=LibraryDB;User Id=sa;Password=<local-dev-only>;TrustServerCertificate=true" Microsoft.EntityFrameworkCore.SqlServer -o Scaffolded
```

Inspect what it generated: every table becomes a class, every FK becomes a navigation property, and
constraints it cannot express in convention land in `OnModelCreating` as Fluent API. Scaffolding is a
snapshot, not a sync — if the database changes you re-scaffold or hand-edit.

(`dotnet ef` is a *tool*: `dotnet tool install --global dotnet-ef` — running the install twice is safe; it
just tells you it is already installed. Verify with `dotnet ef --version`.)

### Code-first: the migrations workflow
With code-first you change the model, then record the change as a **migration** — a C# file with an `Up()`
(apply) and `Down()` (revert) that EF generates by diffing your model against the last snapshot:

```
dotnet ef migrations add Init            # generate Migrations/..._Init.cs
dotnet ef database update                # run pending migrations against the DB
```

A healthy migration chain reads like a story:
`Init` (Products + Inventory) -> `DataSeeded` (HasData rows) -> `OrdersCustomers` (the order aggregate) ->
`AnnotatedCustomer` (constraint tweaks). Each is small, named for its intent, and committed with the code
that motivated it. That is the rhythm: **model change -> `migrations add <Name>` -> review -> `database
update` -> commit both.**

Rules that keep migrations from breaking things or losing data:

- **Review the generated code before applying.** `migrations add` is a diff guess; read the `Up()` and make
  sure it says what you meant.
- **Small and incremental.** One intent per migration; a migration named `Stuff` is a smell.
- **Never edit or delete an applied migration.** The database remembers (table `__EFMigrationsHistory`);
  rewriting history strands every copy of the DB. Fix forward with a new migration.
- **Watch for destructive diffs.** Renaming a property generates *drop column + add column* — your data
  leaves with the dropped column. For a rename, edit the migration to use `RenameColumn` instead (below).
- `dotnet ef migrations remove` un-generates the latest migration **only if it has not been applied**.

### Modifying a migration before execution
A migration file is code — you are allowed to edit it *before* running `database update`. Typical reasons:
turn a drop/add pair into a rename, backfill a new non-null column, or run a bit of raw SQL mid-migration:

```csharp
public partial class RenameQuantity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // hand-edited: the generator guessed Drop + Add, which loses data
        migrationBuilder.RenameColumn(name: "Qty", table: "OrderLines", newName: "Quantity");

        // raw SQL escape hatch inside a migration
        migrationBuilder.Sql("UPDATE OrderLines SET Quantity = 1 WHERE Quantity IS NULL;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.RenameColumn(name: "Quantity", table: "OrderLines", newName: "Qty");
}
```

Keep `Down()` a true mirror of your edited `Up()`, and never touch the `.Designer.cs` snapshot by hand.

### Where the connection string lives
Both directions need a connection string. It should come from configuration
(`ConnectionStrings__<Name>` environment variable or `appsettings.json`; see
`../02-rest-http/minimal-api-hosting.md`). The `dotnet ef` tools use the same source because they boot your
`Program.cs` to discover the context.

## Say It in an Interview
- *"Code-first: my classes are the source of truth and migrations generate the schema — the greenfield
  default. Data-first: the database already exists and `dotnet ef dbcontext scaffold` generates the
  classes — the legacy/shared-schema mode."*
- *"My migrations rhythm is model change, `migrations add` with an intent name, review the generated
  `Up()`, `database update`, commit code and migration together."*
- *"Applied migrations are immutable — the database tracks them in `__EFMigrationsHistory` — so I fix
  forward with a new migration rather than editing history."*
- *"I edit a migration before applying it when the diff guessed wrong: a rename generates drop-plus-add,
  which loses the column's data, so I swap it for `RenameColumn` — and `migrationBuilder.Sql` handles
  backfills."*

## Check Yourself
1. Your team inherits a ten-year-old, DBA-owned database. Which approach, and what one command starts you
   off?
2. Why must you read a generated migration's `Up()` before running `database update`? Give the classic
   destructive example.
3. A migration was applied to production and turns out wrong. What do you do — and what must you never do?
4. What two artifacts should land in the same commit after a model change?
5. When is `dotnet ef migrations remove` safe?

**Answers:** (1) Data-first; `dotnet ef dbcontext scaffold "<conn>" Microsoft.EntityFrameworkCore.SqlServer`.
(2) It is a diff guess — a property rename becomes drop column + add column, silently discarding data;
hand-edit to `RenameColumn`. (3) Fix forward with a new corrective migration; never edit or delete the
applied one — `__EFMigrationsHistory` strands every database that already ran it. (4) The model change and
its generated migration. (5) Only while the latest migration has not been applied to any database.

## Summary
- Code-first: classes generate the schema via migrations — greenfield default. Data-first: `scaffold`
  generates classes from an existing schema — the legacy-database mode.
- Migrations workflow: change model -> `migrations add <IntentName>` -> review generated `Up()` ->
  `database update` -> commit code + migration together.
- Applied migrations are immutable history; fix forward. Watch for drop/add diffs that destroy data.
- You may edit a migration before applying it — renames, backfills, and `migrationBuilder.Sql` are the
  common cases.

## Resources
- [Migrations overview (Microsoft Learn)](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Reverse engineering (scaffolding)](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/)
- [Customizing migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/#customize-migration-code)
