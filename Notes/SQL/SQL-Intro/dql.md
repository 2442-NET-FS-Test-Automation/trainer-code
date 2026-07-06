# DQL: Reading Data (SELECT, WHERE, ORDER BY, DISTINCT)

## Learning Objectives

- Read data with `SELECT`: pick columns, the whole row, or computed expressions.
- Filter rows with `WHERE` and its operators (`=`, `<>`, `<`, `>`, `BETWEEN`, `IN`, `LIKE`, `IS NULL`, `AND`/`OR`).
- Sort results with `ORDER BY` (ascending/descending, multi-key).
- Remove duplicate rows from output with `DISTINCT`.
- Recognize the **syntax** of `GROUP BY` / `HAVING` (full treatment Thursday).

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

Storing data is only half the point — the payoff is *asking questions* of it. `SELECT` is the most-used statement in all of SQL, and the one you'll write thousands of times in your career: "which loans are overdue?", "who are our members in this city?", "list books with no copies left." This is the "query" beat of the week's epic and the back half of the `02-dml-dql` commit — reading back the rows you just seeded.

`WHERE` filtering is a QC-2 Must-know ("filter records using the WHERE clause and operators"), and everything later builds on the clauses here: Thursday's joins, aggregates, and reports are all `SELECT` with more clauses bolted on. Get the `SELECT` skeleton and its clause order solid now and the rest of the week is incremental.

## The Concept

### SELECT: the shape

```sql
SELECT Title, PublishedYear        -- which columns (the projection)
FROM   dbo.Book                    -- which table
WHERE  PublishedYear >= 2000       -- which rows (the filter)
ORDER  BY PublishedYear DESC;      -- the output order
```

- `SELECT col1, col2` picks **columns**; `SELECT *` returns **every** column (handy when exploring, but name your columns in real queries — `*` breaks reports when the schema changes and pulls more than you need).
- `SELECT` can also return **computed** values: `SELECT Title, TotalCopies - AvailableCopies FROM dbo.Book` (copies currently out).

### Logical clause order (read this once, save hours)

SQL is written in one order but **evaluated** in another. The engine processes:

```mermaid
flowchart LR
    FROM --> WHERE --> GB["GROUP BY"] --> HAVING --> SELECT --> OB["ORDER BY"]
```

The two consequences that bite everyone:
- **`WHERE` runs before `SELECT`**, so `WHERE` can't use a column alias you create in `SELECT`.
- **`ORDER BY` runs last**, so it *can* use a `SELECT` alias and even columns not shown.

### WHERE and its operators

`WHERE` keeps only rows where its condition is **true**. The operator toolkit:

| Operator | Meaning | Example |
|---|---|---|
| `=` `<>` (or `!=`) | equal / not equal | `WHERE PublishedYear = 2008` |
| `<` `>` `<=` `>=` | comparison | `WHERE AvailableCopies < 2` |
| `BETWEEN a AND b` | inclusive range | `WHERE PublishedYear BETWEEN 2000 AND 2010` |
| `IN (...)` | matches any in a list | `WHERE AuthorId IN (1, 3, 5)` |
| `LIKE` | pattern match (`%` = any chars, `_` = one char) | `WHERE Title LIKE 'Clean%'` |
| `IS NULL` / `IS NOT NULL` | test for null | `WHERE ReturnDate IS NULL` |
| `AND` / `OR` / `NOT` | combine conditions | `WHERE A AND (B OR C)` |

> **NULL is not a value — it's "unknown."** So `ReturnDate = NULL` is **never true** (unknown isn't "equal" to anything). You must write `ReturnDate IS NULL`. This is the single most common `WHERE` mistake. In the Library model a `NULL` `ReturnDate` means "still checked out," so active loans are `WHERE ReturnDate IS NULL`.

Example — active loans only:

```sql
SELECT LoanId, BookId, MemberId, DueDate
FROM   dbo.Loan
WHERE  ReturnDate IS NULL;
```

| LoanId | BookId | MemberId | DueDate |
|---|---|---|---|
| 3 | 1 | 2 | 2026-07-10 |
| 5 | 4 | 1 | 2026-07-12 |

### ORDER BY: sort the output

```sql
SELECT Title, PublishedYear
FROM   dbo.Book
ORDER  BY PublishedYear DESC, Title ASC;
```

- `ASC` (ascending, the default) or `DESC` (descending).
- **Multi-key:** sort by `PublishedYear` descending, and *within the same year* by `Title` A→Z.
- Without `ORDER BY`, the database makes **no guarantee** about row order. If you need an order, say so — never rely on "insertion order."

### DISTINCT: collapse duplicate rows

```sql
SELECT DISTINCT PublishedYear
FROM   dbo.Book
ORDER  BY PublishedYear;
```

`DISTINCT` removes duplicate **rows** from the result (it applies to the whole selected row, not one column). If five books were published in 2008, `DISTINCT PublishedYear` shows `2008` once.

| PublishedYear |
|---|
| 1999 |
| 2004 |
| 2008 |

### A first look at GROUP BY / HAVING (syntax only)

You will often want **per-group** answers — "how many books *per* author." That's `GROUP BY`, which collapses rows that share a value into one row per group, paired with aggregate functions. The **syntax** looks like this:

```sql
SELECT   AuthorId, COUNT(*) AS BookCount
FROM     dbo.Book
GROUP BY AuthorId
HAVING   COUNT(*) > 1;        -- HAVING filters the GROUPS (after aggregation)
```

- `GROUP BY AuthorId` → one output row per author.
- `HAVING` filters **groups** (using an aggregate), where `WHERE` filters **rows** (before grouping).

> **We are only previewing the shape here.** Aggregate functions (`COUNT`, `SUM`, `AVG`, `MIN`, `MAX`), the `WHERE`-vs-`HAVING` distinction in depth, and real grouped reports are **Thursday's** topic (`../SQL-Intermediate/functions.md`). Today: recognize the keywords and where they sit in the clause order. Don't go deep yet.

## Code Example

A realistic read against the seeded Library data, exercising filtering, sorting, and distinctness:

```sql
-- books with at least one copy available, newest first, title as tiebreak
SELECT Title, PublishedYear, AvailableCopies
FROM   dbo.Book
WHERE  AvailableCopies > 0
       AND PublishedYear BETWEEN 2000 AND 2026
ORDER  BY PublishedYear DESC, Title ASC;
```

| Title | PublishedYear | AvailableCopies |
|---|---|---|
| Refactoring | 2018 | 2 |
| Clean Code | 2008 | 2 |

```sql
-- the distinct set of years we hold books from
SELECT DISTINCT PublishedYear
FROM   dbo.Book
ORDER  BY PublishedYear DESC;
```

| PublishedYear |
|---|
| 2018 |
| 2008 |

## Common Mistakes / Interview Traps

- **`= NULL` instead of `IS NULL`.** `col = NULL` is never true. Use `IS NULL` / `IS NOT NULL`.
- **Relying on row order without `ORDER BY`.** No order is guaranteed unless you ask. "It came back sorted in testing" is luck, not a contract.
- **Thinking `DISTINCT` de-dupes one column.** It de-dupes the whole **selected row**; add more columns and "duplicates" reappear.
- **Using a `SELECT` alias in `WHERE`.** Illegal — `WHERE` runs before `SELECT`. Repeat the expression, or filter in a later stage. (`ORDER BY` *can* use the alias.)
- **`LIKE` without anchoring or with the wrong wildcard.** `%` = any run of characters, `_` = exactly one. `LIKE 'Clean'` (no `%`) matches only the exact string.
- **`SELECT *` in production queries.** Fine for exploring; brittle and wasteful in code and reports.

## Decision Guide: `WHERE` vs `HAVING`

| Filter… | Use | Runs… |
|---|---|---|
| individual **rows** (before grouping) | `WHERE` | early (before `GROUP BY`) |
| whole **groups** by an aggregate | `HAVING` | late (after `GROUP BY`) |

(You'll *apply* this Thursday; today just know `WHERE` = rows, `HAVING` = groups.)

## Summary

- **`SELECT ... FROM`** reads data; pick columns (not `*`) or compute expressions.
- **`WHERE`** filters rows with `=`, `<>`, `<`/`>`, `BETWEEN`, `IN`, `LIKE`, `IS NULL`, combined by `AND`/`OR`; remember **`IS NULL`**, never `= NULL`.
- **`ORDER BY`** sorts (ASC/DESC, multi-key); without it, order is undefined.
- **`DISTINCT`** removes duplicate **rows** from the output.
- **`GROUP BY` / `HAVING`** group and filter groups — previewed here, **applied Thursday**. Logical order: `FROM → WHERE → GROUP BY → HAVING → SELECT → ORDER BY`.

## Additional Resources

- [SELECT (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/queries/select-transact-sql)
- [WHERE clause (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/queries/where-transact-sql)
- [ORDER BY clause (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/queries/select-order-by-clause-transact-sql)
