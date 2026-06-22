# Joins & Subqueries

## Learning Objectives

- Combine tables with `INNER`, `LEFT`, `RIGHT`, `FULL OUTER`, and `CROSS` joins.
- Explain **equi-joins** vs **theta-joins** and what the `ON` clause does.
- Choose between a **subquery** and a **join**.
- Combine result sets with **set operations** (`UNION`) at an awareness level.

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

Normalization (yesterday) spread data across many tables on purpose — each fact stored once. **Joins** are how you put it back together to answer real questions: "list each loan with the member's name and the book's title" needs `Loan`, `Member`, and `Book` recombined. This is the heart of the "join/aggregate" beat of the week's epic and the second half of Thursday's `04-joins-functions` commit. Without joins, a normalized database is a pile of disconnected fragments.

Joins are the **most-tested SQL skill** there is — QC-2 lists "basic types of joins... inner, left/right outer, full outer, equi" as Must-know, and almost every SQL interview includes a join question, usually with a deliberate NULL or "which rows survive?" twist. This note shows each join with its **result set** so you can see, not just recite, what each one keeps.

## The Concept

### The setup

Joins match rows from two tables on a condition (the `ON` clause), usually a foreign key equal to a primary key. Two small tables to make every result visible:

`Author`:

| AuthorId | Name |
|---|---|
| 1 | Martin |
| 2 | Fowler |
| 3 | Beck |

`Book` (note: author 3 wrote nothing yet; book 99 has a NULL author):

| BookId | Title | AuthorId |
|---|---|---|
| 1 | Clean Code | 1 |
| 2 | Refactoring | 2 |
| 99 | Orphan Draft | NULL |

### INNER JOIN — only matches

Keeps rows that match on **both** sides. Author 3 (no books) and book 99 (no author) both disappear.

```sql
SELECT b.Title, a.Name
FROM   dbo.Book AS b
INNER JOIN dbo.Author AS a ON b.AuthorId = a.AuthorId;
```

| Title | Name |
|---|---|
| Clean Code | Martin |
| Refactoring | Fowler |

This is the default and most common join. The table aliases (`b`, `a`) keep it readable and disambiguate the shared `AuthorId` column name.

### LEFT (OUTER) JOIN — all of the left, matches from the right

Keeps **every** left row; where the right has no match, its columns come back `NULL`. Book 99 survives with a NULL author.

```sql
SELECT b.Title, a.Name
FROM   dbo.Book AS b
LEFT JOIN dbo.Author AS a ON b.AuthorId = a.AuthorId;
```

| Title | Name |
|---|---|
| Clean Code | Martin |
| Refactoring | Fowler |
| Orphan Draft | NULL |

`LEFT JOIN` answers "all books, *with* their author if known" and is the tool for finding unmatched rows: add `WHERE a.AuthorId IS NULL` to list books with no author.

### RIGHT (OUTER) JOIN — all of the right

The mirror image: keeps every **right** row. Author 3 (no books) survives with NULL book columns.

```sql
SELECT b.Title, a.Name
FROM   dbo.Book AS b
RIGHT JOIN dbo.Author AS a ON b.AuthorId = a.AuthorId;
```

| Title | Name |
|---|---|
| Clean Code | Martin |
| Refactoring | Fowler |
| NULL | Beck |

> Any `RIGHT JOIN` can be rewritten as a `LEFT JOIN` by swapping the table order — most teams standardize on `LEFT` for readability.

### FULL OUTER JOIN — everything from both

Keeps **all** rows from both sides; fills NULLs wherever a match is missing. Both the orphan book *and* the bookless author appear.

```sql
SELECT b.Title, a.Name
FROM   dbo.Book AS b
FULL OUTER JOIN dbo.Author AS a ON b.AuthorId = a.AuthorId;
```

| Title | Name |
|---|---|
| Clean Code | Martin |
| Refactoring | Fowler |
| Orphan Draft | NULL |
| NULL | Beck |

Use it to reconcile two sets and see what's missing on *either* side.

### CROSS JOIN — every combination

No `ON` clause — pairs **every** left row with **every** right row (a Cartesian product). 3 books × 3 authors = 9 rows. Rarely what you want; useful for generating combinations (e.g. every shelf × every day).

```sql
SELECT b.Title, a.Name
FROM   dbo.Book AS b
CROSS JOIN dbo.Author AS a;     -- 3 x 3 = 9 rows
```

> An accidental cross join (forgetting the `ON`, or a wrong join condition) is the usual cause of a query returning *way* more rows than expected.

### Equi-join vs theta-join

- **Equi-join** — the `ON` uses **equality** (`b.AuthorId = a.AuthorId`). This is the overwhelmingly common case (FK = PK).
- **Theta-join** — the `ON` uses a **non-equality** comparison (`<`, `>`, `BETWEEN`, `<>`). Example: join loans to a fee schedule where `loan.DaysLate BETWEEN tier.MinDays AND tier.MaxDays`.

The join *type* (inner/left/...) decides which unmatched rows survive; the `ON` *condition* (equi/theta) decides what "matches" means. They're independent choices.

### Visualizing the four main joins

```
INNER         LEFT          RIGHT         FULL OUTER
 (A∩B)        (A + A∩B)     (B + A∩B)     (A ∪ B)
  ▓▓            ▓▓▓▓          ░▓▓▓          ▓▓▓▓▓▓
 only          all A,        all B,        everything,
 matches       matched B     matched A     nulls where unmatched
```

### Subquery vs join

A **subquery** is a `SELECT` nested inside another statement. Often a subquery and a join answer the same question:

```sql
-- subquery: books by authors born before 1950
SELECT Title FROM dbo.Book
WHERE AuthorId IN (SELECT AuthorId FROM dbo.Author WHERE BirthYear < 1950);

-- same answer as a join
SELECT b.Title
FROM   dbo.Book AS b
INNER JOIN dbo.Author AS a ON b.AuthorId = a.AuthorId
WHERE  a.BirthYear < 1950;
```

**When to use which:**

| Reach for a **join** when… | Reach for a **subquery** when… |
|---|---|
| You need **columns from both** tables in the output | You only need to **test membership/existence** |
| Combining/reporting across tables | Filtering against a **computed/aggregated** set (e.g. `> AVG(...)`) |
| It reads more clearly as a single set | The logic is naturally "rows where X is in (some set)" |

Joins are usually clearer and the optimizer often treats them similarly, but a subquery shines for "rows compared to an aggregate," e.g. `WHERE Price > (SELECT AVG(Price) FROM Book)`.

### Set operations (awareness level)

`UNION` stacks two result sets (with the same column shape) into one:

```sql
SELECT Name FROM dbo.Author
UNION                       -- removes duplicates (UNION ALL keeps them)
SELECT Name FROM dbo.Member;
```

`UNION` removes duplicate rows; `UNION ALL` keeps them (and is faster). `INTERSECT` (rows in both) and `EXCEPT` (rows in the first not the second) round out the family. These are **awareness-level** this week — recognize them; you won't be tested deeply.

## Code Example

The capstone multi-table report — every active loan with member and book details, three tables joined:

```sql
SELECT  m.FirstName + ' ' + m.LastName AS Member,
        b.Title                        AS Book,
        l.LoanDate,
        l.DueDate
FROM    dbo.Loan   AS l
INNER JOIN dbo.Member AS m ON l.MemberId = m.MemberId
INNER JOIN dbo.Book   AS b ON l.BookId   = b.BookId
WHERE   l.ReturnDate IS NULL            -- still out
ORDER BY l.DueDate;
```

| Member | Book | LoanDate | DueDate |
|---|---|---|---|
| Ada Byron | Clean Code | 2026-06-18 | 2026-07-10 |
| Alan M | Refactoring | 2026-06-20 | 2026-07-12 |

The normalized model, reassembled into the human-readable report it was always meant to produce.

## Common Mistakes / Interview Traps

- **`LEFT JOIN` + a `WHERE` on the right table = silent `INNER JOIN`.** `WHERE a.BirthYear > 1900` throws away the NULL-author rows the LEFT JOIN kept. Put right-table conditions in the **`ON`** clause to preserve the outer rows.
- **Forgetting the `ON` condition.** No `ON` (or a bad one) explodes into a cross join — far too many rows.
- **Ambiguous column names.** Selecting `AuthorId` when both tables have it errors; qualify with the table/alias (`b.AuthorId`).
- **Assuming `INNER` keeps unmatched rows.** It doesn't — only outer joins keep unmatched rows (as NULLs).
- **`UNION` vs `UNION ALL`.** `UNION` does a dedupe (and a sort) — if you know there are no duplicates, `UNION ALL` is faster and clearer.
- **Subquery returning multiple rows with `=`.** `WHERE AuthorId = (SELECT ...)` errors if the subquery yields more than one row — use `IN`.

## Decision Guide: which join?

| You want… | Use |
|---|---|
| only rows that match in both | `INNER JOIN` |
| all of table A, matched B (or to find A's with no B) | `LEFT JOIN` |
| all of table B, matched A | `RIGHT JOIN` (or flip to `LEFT`) |
| everything from both, reconciled | `FULL OUTER JOIN` |
| every combination of A and B | `CROSS JOIN` |

## Summary

- **`INNER JOIN`** keeps only matches; **`LEFT`/`RIGHT`** keep all of one side (NULLs for the other); **`FULL OUTER`** keeps all of both; **`CROSS`** pairs every combination.
- The **join type** decides which unmatched rows survive; the **`ON` condition** (equi = `=`, theta = `<`/`>`) decides what matches.
- **Subqueries** test membership/aggregate comparisons; **joins** combine columns from multiple tables — often interchangeable, joins usually clearer.
- **`UNION`** stacks result sets (dedupes; `UNION ALL` keeps duplicates) — awareness level.
- Watch the **`LEFT JOIN` + `WHERE`** trap: filter the right table in `ON`, not `WHERE`, to keep outer rows.

## Additional Resources

- [SQL JOINs explained with diagrams — W3Schools (beginner)](https://www.w3schools.com/sql/sql_join.asp)
- [FROM clause + JOIN syntax (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/queries/from-transact-sql)
- [Subqueries (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/relational-databases/performance/subqueries)

*(Going deeper, later: [physical join algorithms / query optimizer](https://learn.microsoft.com/en-us/sql/relational-databases/performance/joins) — internals, not needed for week 3.)*
