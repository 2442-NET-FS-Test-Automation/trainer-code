# DDL: Defining the Schema (CREATE / ALTER / DROP / TRUNCATE)

## Learning Objectives

- Use the four **DDL** keywords — `CREATE`, `ALTER`, `DROP`, `TRUNCATE` — and say what each one shapes.
- Write a `CREATE TABLE` statement: columns, data types, nullability, and the **IDENTITY** auto-increment.
- Modify an existing table with `ALTER TABLE` (add a column, change a type).
- Distinguish `DROP` from `TRUNCATE` (and preview `DELETE`).
- Build a **re-runnable** schema script with a drop-then-create guard.

> **Scope split:** This note covers DDL *statements* and table shape. The **constraints** that enforce rules (PK, FK, UNIQUE, CHECK, DEFAULT, CASCADE) get their own companion note, `constraints.md` — read both for Monday.

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

DDL is the load-bearing skill of Week 3: it is how an ERD on paper becomes a real, queryable structure. Every later day — inserting rows, joining tables, wrapping writes in transactions — assumes a schema already exists, and DDL is how it got there. This is the "create" beat of the week's epic, the first commit (`01-ddl`) on the `sql-training` thread where `LibraryDB` stops being a diagram and starts being a database.

It is also where good habits start. A schema you can rebuild from one script, on any machine, every time, is the difference between a reproducible project and a pile of one-off clicks. QC-2 asks you to "construct DDL statements to generate tables" and "perform basic DDL operations" — this note is that, end to end.

## The Concept

### The four DDL keywords

DDL (Data **Definition** Language) shapes *structure*, not data:

| Keyword | Does | Granularity |
|---|---|---|
| `CREATE` | makes a new object (table, view, index…) | whole object |
| `ALTER` | changes an existing object's structure | part of an object |
| `DROP` | removes an object **entirely** | whole object |
| `TRUNCATE` | empties a table's **rows** fast, keeps the table | all rows |

Note that `TRUNCATE` is the odd one: it touches data (removes all rows) but is classed as DDL because of *how* it works — it deallocates the table's storage pages rather than logging row-by-row deletions. That mechanism is also why it is fast and why it can't have a `WHERE` clause.

### CREATE TABLE: the anatomy

```sql
CREATE TABLE dbo.Author
(
    AuthorId  INT          IDENTITY(1,1) NOT NULL,
    FirstName VARCHAR(50)  NOT NULL,
    LastName  VARCHAR(50)  NOT NULL,
    BirthYear INT          NULL,
    CONSTRAINT PK_Author PRIMARY KEY (AuthorId)
);
GO
```

Reading it line by line:

- **`dbo.Author`** — the schema-qualified table name. `dbo` is SQL Server's default schema (namespace). Get in the habit of writing it.
- **`AuthorId INT IDENTITY(1,1) NOT NULL`** — a column: name, type, options.
  - **`IDENTITY(1,1)`** = auto-increment: start at **1**, step by **1**. The engine assigns the value; you never type it. Insert three authors → ids 1, 2, 3. (MySQL spells this `AUTO_INCREMENT`; PostgreSQL `GENERATED ... AS IDENTITY` — same idea.)
  - **`NOT NULL`** = the column is **required**. `BirthYear NULL` (the default) = optional / may be unknown.
- **`CONSTRAINT PK_Author PRIMARY KEY (AuthorId)`** — names the primary key. Naming constraints (`PK_`, `FK_`, `CK_`, `DF_`, `UQ_`) gives readable errors and lets you alter them later. Constraints get the full treatment in `constraints.md`.
- **`GO`** — a SQL Server **batch separator** (a tool directive, not SQL itself). It ends one batch so the next can start clean. Some statements (like `CREATE PROCEDURE`) must be first in their batch, which is why `GO` matters.

After creating, you can confirm the shape (empty, but well-formed):

```sql
SELECT * FROM dbo.Author;   -- 0 rows, but the column headers show your design
```

| AuthorId | FirstName | LastName | BirthYear |
|---|---|---|---|
| *(no rows yet)* | | | |

> Reading rows with `SELECT` is tomorrow's topic (`dql.md`). Here it's only a peek to confirm the table exists.

### ALTER TABLE: change a table that already has data

Schemas evolve. `ALTER` changes a table **in place** — no drop, no data loss (when done safely):

```sql
-- add a new column; on a populated table a NOT NULL add needs a DEFAULT
ALTER TABLE dbo.Book ADD Edition INT NOT NULL DEFAULT (1);

-- change a column's type (widen the title)
ALTER TABLE dbo.Book ALTER COLUMN Title VARCHAR(250) NOT NULL;
```

- **`ADD`** bolts on a column. If the table already has rows and the new column is `NOT NULL`, you must supply a `DEFAULT` so existing rows get a value — otherwise the engine has no value for them and the `ALTER` fails.
- **`ALTER COLUMN`** changes a type. **Widening** (`VARCHAR(200)` → `(250)`) is safe; **narrowing** can truncate or fail if existing data won't fit.
- In a teaching build that rebuilds from zero, you usually **fold** the change into the original `CREATE TABLE` instead of `ALTER`-ing — same end-state, and the script stays idempotent. In a *live* system you can't drop the table, so `ALTER` (i.e. a migration) is the only way. Both are correct; the context decides.

### DROP vs TRUNCATE (and a preview of DELETE)

Three ways to "get rid of" things — a guaranteed exam question:

```sql
TRUNCATE TABLE dbo.Loan;   -- empties ALL rows fast, keeps the table, resets IDENTITY
DROP TABLE dbo.Loan;       -- removes the table ENTIRELY — structure and all
```

| | Removes | `WHERE`? | Resets IDENTITY? | Logged? | Speed | Sublanguage |
|---|---|---|---|---|---|---|
| `DROP` | the whole **table** | n/a | n/a (table gone) | minimal | fast | DDL |
| `TRUNCATE` | **all rows** | **no** | **yes** | minimal (per page) | very fast | DDL |
| `DELETE` *(tomorrow)* | **chosen rows** | **yes** | no | per row | slower | DML |

- **`DROP`** — "this table should not exist." Gone, structure included. You'd `CREATE` again to get it back.
- **`TRUNCATE`** — "empty this table." All rows, no filter, identity counter back to 1. Fast because it doesn't log each row.
- **`DELETE`** (tomorrow, `dml.md`) — surgical: remove only the rows a `WHERE` clause picks.

### The re-runnable build (a habit that pays all week)

Run `CREATE TABLE Book` twice and the second run errors — it already exists. So make the script **idempotent**: drop first, then create.

```sql
-- drop children before parents (a table can't be dropped while another references it)
DROP TABLE IF EXISTS dbo.Loan;
DROP TABLE IF EXISTS dbo.Book;
DROP TABLE IF EXISTS dbo.Member;
DROP TABLE IF EXISTS dbo.Author;
GO

-- then create parents before children (a child FK needs its parent to exist first)
-- CREATE TABLE dbo.Author ...  Member ...  Book ...  Loan ...
```

- **`DROP TABLE IF EXISTS`** = drop only if present → no error on the first run.
- **Order is mirror-image:** drop children → parents; create parents → children. `Loan` references `Book` and `Member`; `Book` references `Author`. You can't drop a parent a child still points at, and you can't create a child before its parent exists.

Running the whole file top-to-bottom twice with identical results *is* what "re-runnable" means — and it's how the `sql-training` thread stays clean at every commit.

## Code Example

A minimal but complete re-runnable two-table build (constraints kept light here — see `constraints.md`):

```sql
DROP TABLE IF EXISTS dbo.Book;
DROP TABLE IF EXISTS dbo.Author;
GO

CREATE TABLE dbo.Author
(
    AuthorId  INT IDENTITY(1,1) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName  VARCHAR(50) NOT NULL,
    CONSTRAINT PK_Author PRIMARY KEY (AuthorId)
);
GO

CREATE TABLE dbo.Book
(
    BookId   INT IDENTITY(1,1) NOT NULL,
    Title    VARCHAR(200) NOT NULL,
    AuthorId INT NOT NULL,
    CONSTRAINT PK_Book PRIMARY KEY (BookId),
    CONSTRAINT FK_Book_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Author (AuthorId)
);
GO

-- evolve it later without a rebuild:
ALTER TABLE dbo.Book ADD PublishedYear INT NULL;
```

Run it once: two tables. Run it again: no errors, same two tables. That is a reproducible schema.

## Common Mistakes / Interview Traps

- **Wrong create/drop order.** Symptom: *"Foreign key references invalid table"* or *"could not create constraint."* Fix: create parents before children; drop children before parents.
- **`NOT NULL` `ALTER ADD` on a populated table without a `DEFAULT`.** The existing rows have no value → the `ALTER` fails. Add a `DEFAULT`, or add as `NULL` then backfill.
- **Re-running `CREATE` with no drop guard.** *"There is already an object named 'Book'."* Fix: the `DROP TABLE IF EXISTS` block on top.
- **Thinking `TRUNCATE` is just a fast `DELETE`.** It can't take a `WHERE`, it resets `IDENTITY`, and it is minimally logged — different tool. (And in some engines it can't run while the table is referenced by an FK.)
- **Expecting `IDENTITY` to reuse numbers.** It doesn't — delete row 3 and the next insert is still 4. Ids are never recycled, on purpose.

## Decision Guide: `DROP` vs `TRUNCATE` vs `DELETE`

| You want to… | Use | Why |
|---|---|---|
| Remove the table object entirely | `DROP TABLE` | structure + data both gone |
| Empty a table completely, fast, reset ids | `TRUNCATE TABLE` | all rows, minimal logging, resets IDENTITY |
| Remove only some rows (by a condition) | `DELETE ... WHERE` | the only one that filters; per-row, logged, rollback-friendly |

## Summary

- **DDL = `CREATE` / `ALTER` / `DROP` / `TRUNCATE`** — it shapes structure (and `TRUNCATE` fast-empties rows).
- **`CREATE TABLE`** declares columns with types, nullability, and `IDENTITY(1,1)` for auto-increment.
- **`ALTER TABLE`** evolves a table in place; a `NOT NULL` add on populated data needs a `DEFAULT`; widening types is safe, narrowing is risky.
- **`DROP`** removes the whole table; **`TRUNCATE`** fast-empties all rows and resets identity; **`DELETE`** (tomorrow) removes chosen rows.
- A **re-runnable** script drops children→parents, then creates parents→children — idempotent every run.

## Additional Resources

- [CREATE TABLE (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql)
- [ALTER TABLE (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/statements/alter-table-transact-sql)
- [DROP TABLE vs TRUNCATE vs DELETE — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/statements/truncate-table-transact-sql)
