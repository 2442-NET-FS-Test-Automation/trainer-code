# Functions & Grouping (Aggregate vs Scalar, GROUP BY / HAVING)

## Learning Objectives

- Distinguish **aggregate** functions from **scalar** functions.
- Use `COUNT`, `SUM`, `AVG`, `MIN`, `MAX` to summarize data.
- Group rows with `GROUP BY` and filter groups with `HAVING`.
- State precisely the difference between `WHERE` and `HAVING`.
- Improve readability with column **aliases**.
- *(Awareness)* Recognize **window functions** (`OVER (PARTITION BY ... ORDER BY ...)`) as aggregates that keep every row.

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

Raw rows answer "what do I have?"; functions and grouping answer "what does it *mean*?" — how many loans per member, the average book age per category, the busiest day. This is the "aggregate" beat of the week's epic and the first half of Thursday's `04-joins-functions` commit. It's also where Tuesday's `GROUP BY`/`HAVING` *syntax preview* becomes a real reporting skill: today you **apply** it with aggregate functions.

QC-2 makes several of these Must/Should: "difference between aggregate and scalar functions," "utilize the GROUP BY clause," "utilize HAVING," and "common aggregate functions." Grouping is also the concept interviewers most often use to separate people who can read SQL from people who can *write* it.

## The Concept

### Aggregate vs scalar — the core distinction

| | **Aggregate** function | **Scalar** function |
|---|---|---|
| Input | **many rows** | **one value** |
| Output | **one** summarizing value | **one value per row** |
| Examples | `COUNT`, `SUM`, `AVG`, `MIN`, `MAX` | `UPPER`, `LEN`, `GETDATE`, `ROUND`, `YEAR` |
| Used with | `GROUP BY` (or whole-table) | the `SELECT`/`WHERE` of any query |

The mental model: an **aggregate** *collapses* a set of rows into a single answer ("how many?"); a **scalar** *transforms* each row's value independently ("uppercase this title"). This precise contrast is a Must-know.

```sql
SELECT COUNT(*)        AS TotalBooks,     -- aggregate: one number from all rows
       AVG(TotalCopies) AS AvgCopies       -- aggregate
FROM   dbo.Book;

SELECT UPPER(Title)    AS LoudTitle,       -- scalar: one result per row
       YEAR(GETDATE()) - PublishedYear AS AgeYears   -- scalar arithmetic per row
FROM   dbo.Book;
```

### The aggregate functions

| Function | Returns | Note |
|---|---|---|
| `COUNT(*)` | number of rows | counts rows including NULLs |
| `COUNT(col)` | number of **non-NULL** values in `col` | NULLs are skipped |
| `COUNT(DISTINCT col)` | number of distinct non-NULL values | |
| `SUM(col)` | total of numeric values | ignores NULLs |
| `AVG(col)` | average of numeric values | ignores NULLs (divides by non-null count!) |
| `MIN(col)` / `MAX(col)` | smallest / largest | works on numbers, dates, text |

> **NULL handling is a classic trap.** `COUNT(*)` counts every row; `COUNT(ReturnDate)` counts only rows where `ReturnDate` is not null. And `AVG(col)` divides by the count of **non-null** values, not by the row count — so nulls can quietly change the average.

### GROUP BY: one answer per group

`GROUP BY` collapses rows that share a value into **one row per group**, and the aggregates are computed *within* each group.

```sql
SELECT   AuthorId, COUNT(*) AS BookCount
FROM     dbo.Book
GROUP BY AuthorId;
```

| AuthorId | BookCount |
|---|---|
| 1 | 3 |
| 2 | 2 |
| 5 | 1 |

**The golden rule of GROUP BY:** every column in the `SELECT` must be either (a) in the `GROUP BY`, or (b) wrapped in an aggregate. You can't `SELECT Title` alongside `GROUP BY AuthorId` — there are many titles per author, so which one would it show? The engine rejects it.

### HAVING: filter the groups

`WHERE` filters **rows** *before* grouping; `HAVING` filters **groups** *after* aggregation. Use `HAVING` when the condition involves an aggregate.

```sql
SELECT   AuthorId, COUNT(*) AS BookCount
FROM     dbo.Book
WHERE    PublishedYear >= 2000      -- row filter: only recent books (before grouping)
GROUP BY AuthorId
HAVING   COUNT(*) > 1;              -- group filter: authors with >1 such book
```

| AuthorId | BookCount |
|---|---|
| 1 | 2 |
| 2 | 2 |

Read it as a pipeline: keep recent books (`WHERE`) → bucket by author (`GROUP BY`) → keep only buckets with more than one (`HAVING`).

### WHERE vs HAVING (the exam line)

| | `WHERE` | `HAVING` |
|---|---|---|
| Filters | individual **rows** | **groups** |
| Runs | **before** `GROUP BY` | **after** `GROUP BY` |
| Can use an aggregate? | **no** | **yes** |

If a filter doesn't involve an aggregate, prefer `WHERE` — filtering rows early is cheaper than aggregating then discarding groups.

### Aliases for readability

A **column alias** renames a column or expression in the output with `AS`:

```sql
SELECT   AuthorId               AS Author,
         COUNT(*)               AS [Number Of Books]   -- brackets allow spaces
FROM     dbo.Book
GROUP BY AuthorId;
```

Without `AS BookCount`, an aggregate column has no name in the result — alias it. Use `[ ]` (or `" "`) when the alias has spaces. A **table alias** (`FROM dbo.Book AS b`) shortens long names and is essential in joins (tomorrow's neighbor topic).

### Window functions (awareness level)

A **window function** computes a value across a set of related rows — the *window* — **without collapsing them** the way `GROUP BY` does. A `GROUP BY` query returns one row per group; a window function keeps **every** row and attaches the computed value alongside it. You define the window with `OVER (PARTITION BY ... ORDER BY ...)`: `PARTITION BY` splits rows into groups (like `GROUP BY`, but non-collapsing), and `ORDER BY` orders rows *within* each partition.

```sql
-- rank books by copies WITHIN each category, keeping every book row
SELECT Title,
       CategoryId,
       TotalCopies,
       ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY TotalCopies DESC) AS RankInCategory
FROM   dbo.Book;
-- GROUP BY CategoryId would return ONE row per category; the window keeps every book and labels it.
```

The common ranking functions: `ROW_NUMBER()` numbers rows 1,2,3,4; `RANK()` leaves **gaps** after ties (1,2,2,4); `DENSE_RANK()` does not (1,2,2,3). Aggregates work as windows too — `COUNT(*) OVER (PARTITION BY CategoryId)` keeps a per-group total on every row. These are **awareness-level** this week — recognize the shape; you won't be tested deeply. (Deeper treatment lives in the QC-2 review guide's study guide and Drill 19.)

## Code Example

A real grouped report on the Library data — loans per member, busiest first, members with more than one loan:

```sql
SELECT   MemberId                AS Member,
         COUNT(*)                AS LoanCount,
         MAX(LoanDate)           AS MostRecentLoan
FROM     dbo.Loan
GROUP BY MemberId
HAVING   COUNT(*) > 1
ORDER BY LoanCount DESC;
```

| Member | LoanCount | MostRecentLoan |
|---|---|---|
| 2 | 4 | 2026-06-20 |
| 1 | 2 | 2026-06-18 |

One statement answers "who are our most active borrowers, and when did they last borrow?" — `COUNT` and `MAX` aggregating, `GROUP BY` bucketing per member, `HAVING` dropping one-off borrowers, `ORDER BY` ranking, aliases naming.

## Common Mistakes / Interview Traps

- **Putting an aggregate in `WHERE`.** `WHERE COUNT(*) > 1` is illegal — aggregates filter in `HAVING`.
- **Selecting a non-grouped, non-aggregated column.** `SELECT Title, COUNT(*) ... GROUP BY AuthorId` fails. Every selected column must be grouped or aggregated.
- **Confusing `COUNT(*)` and `COUNT(col)`.** `*` counts rows; `COUNT(col)` skips NULLs. Different numbers when nulls exist.
- **Forgetting NULLs in `AVG`/`SUM`.** They're ignored — `AVG` divides by non-null count, which may not be what you expect.
- **Mixing up aggregate and scalar.** "Is `COUNT` scalar?" — no, it's aggregate (many rows → one value). `UPPER`/`LEN`/`GETDATE` are scalar (per-row).
- **Filtering rows in `HAVING` that don't need an aggregate.** Works, but it's slower and less clear — use `WHERE`.
- **Putting a window function in `WHERE`.** It's computed *after* `WHERE`, so you can't filter on it directly — wrap it in a CTE/subquery, then filter the outer query (the top-N-per-group pattern).

## Decision Guide: `WHERE` or `HAVING`?

| The condition is about… | Use |
|---|---|
| a plain column value (date, status, id) | `WHERE` (before grouping) |
| a `COUNT`/`SUM`/`AVG`/`MIN`/`MAX` of a group | `HAVING` (after grouping) |
| both | `WHERE` for the row part, `HAVING` for the aggregate part |

## Summary

- **Aggregate** functions collapse many rows into one value (`COUNT`, `SUM`, `AVG`, `MIN`, `MAX`); **scalar** functions transform one value per row (`UPPER`, `LEN`, `GETDATE`).
- **`GROUP BY`** gives one row per group; every selected column must be grouped or aggregated.
- **`HAVING`** filters groups (can use aggregates); **`WHERE`** filters rows (cannot) and runs first.
- Watch **NULL** behavior: `COUNT(*)` vs `COUNT(col)`, and `AVG`/`SUM` ignoring nulls.
- **Aliases** (`AS`) name aggregate columns and shorten table names for readable output.
- **Window functions** (`OVER (PARTITION BY ... ORDER BY ...)`) compute across a row-set but **keep every row** — unlike `GROUP BY`, which collapses; awareness level.

## Additional Resources

- [SQL GROUP BY & aggregate functions — W3Schools (beginner)](https://www.w3schools.com/sql/sql_groupby.asp)
- [Aggregate functions (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/functions/aggregate-functions-transact-sql)
- [GROUP BY clause (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/queries/select-group-by-transact-sql)
