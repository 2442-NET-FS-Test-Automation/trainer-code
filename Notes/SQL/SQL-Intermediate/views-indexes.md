# Views & Indexes

## Learning Objectives

- Create and use a **view** to store a query behind a name.
- Explain what an **index** is, how it speeds reads, and what it costs.
- Distinguish a **clustered** from a **nonclustered** index.
- Decide when an index helps and when it hurts.

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

The last beat of the week's epic is "package" — turning a working schema into something safe, fast, and reusable. **Views** package complex queries behind a simple name (and can hide columns you don't want exposed); **indexes** make reads fast at scale. These are part of Friday's final `06-views-procs` commit that completes the `sql-training` build. QC-2 makes "create and use views" a Must-know and "describe database indexing and its benefits" a Should-know — and both come up in interviews as "how would you make this query faster?" and "what's a view for?"

## The Concept

### Views: a saved query with a name

A **view** is a stored `SELECT` that you can query as if it were a table. It stores the *query*, not the data — every time you select from a view, the underlying query runs against the live tables.

```sql
CREATE VIEW dbo.ActiveLoans AS
SELECT  l.LoanId,
        m.FirstName + ' ' + m.LastName AS Member,
        b.Title,
        l.DueDate
FROM    dbo.Loan   AS l
INNER JOIN dbo.Member AS m ON l.MemberId = m.MemberId
INNER JOIN dbo.Book   AS b ON l.BookId   = b.BookId
WHERE   l.ReturnDate IS NULL;
```

Now anyone can ask for active loans without knowing the three-table join:

```sql
SELECT * FROM dbo.ActiveLoans ORDER BY DueDate;
```

| LoanId | Member | Title | DueDate |
|---|---|---|---|
| 3 | Ada Byron | Clean Code | 2026-07-10 |
| 5 | Alan M | Refactoring | 2026-07-12 |

**Why views earn their keep:**

- **Simplicity** — package a gnarly join once; everyone reuses it. Fix the logic in one place.
- **Security** — grant access to a view that exposes only some columns/rows, while keeping the base table locked down (e.g. a `MemberPublic` view without emails). Pairs with DCL (`GRANT`/`REVOKE`).
- **Consistency** — everyone computes "active loan" the same way, not five slightly different ad-hoc queries.

A view is **read-through by default** (you `SELECT` from it). Some simple views are updatable, but treat views primarily as read tools this week.

### Indexes: fast lookups, at a write cost

Without an index, finding rows by a column means scanning **every row** (a "table scan") — fine for 10 rows, ruinous for 10 million. An **index** is a separate sorted data structure (a **B-tree**) that lets the engine jump straight to matching rows, like a book's index instead of reading cover to cover.

```sql
-- speed up lookups/filters/joins on Book.AuthorId
CREATE INDEX IX_Book_AuthorId ON dbo.Book (AuthorId);
```

**The trade-off — this is the whole point:**

| Indexes **help** | Indexes **cost** |
|---|---|
| Faster `WHERE`, `JOIN`, `ORDER BY` on the indexed column(s) | Extra **storage** (a second structure) |
| Faster lookups by key | Slower **writes** — every `INSERT`/`UPDATE`/`DELETE` must also update the index |
| Can satisfy a query without touching the table ("covering") | Too many indexes slow the whole table down |

So index the columns you **search/join/sort on a lot**; don't blindly index everything — each index taxes every write.

### Clustered vs nonclustered

| | **Clustered** | **Nonclustered** |
|---|---|---|
| What it does | sorts the **table itself** by the key | a separate structure that **points back** to rows |
| How many per table | **one** (the data can only be physically sorted one way) | **many** |
| Created by | the **PRIMARY KEY** (by default in SQL Server) | extra `CREATE INDEX` statements |
| Analogy | a phone book sorted by last name | the index at the back of a textbook |

Your primary key already gave you one clustered index for free. Add **nonclustered** indexes on the other columns you frequently filter or join by (foreign keys are prime candidates — `Loan.BookId`, `Loan.MemberId`).

### How a B-tree helps (the intuition)

A B-tree keeps keys **sorted** in a shallow, branching tree, so finding a value takes a handful of steps (logarithmic) instead of scanning all N rows (linear). Sorted order is also why an index can speed up `ORDER BY` and range queries (`BETWEEN`, `<`, `>`) on the indexed column, not just equality.

## Code Example

Package the report as a view, and index the columns the report joins on:

```sql
-- index the foreign keys the join uses (nonclustered, many allowed)
CREATE INDEX IX_Loan_MemberId ON dbo.Loan (MemberId);
CREATE INDEX IX_Loan_BookId   ON dbo.Loan (BookId);

-- a view that everyone can reuse instead of re-typing the join
CREATE VIEW dbo.OverdueLoans AS
SELECT  l.LoanId, m.FirstName + ' ' + m.LastName AS Member, b.Title, l.DueDate
FROM    dbo.Loan AS l
INNER JOIN dbo.Member AS m ON l.MemberId = m.MemberId
INNER JOIN dbo.Book   AS b ON l.BookId   = b.BookId
WHERE   l.ReturnDate IS NULL AND l.DueDate < GETDATE();

-- use it
SELECT * FROM dbo.OverdueLoans ORDER BY DueDate;
```

The view hides the join; the indexes make it fast as the loan table grows.

## Common Mistakes / Interview Traps

- **"A view stores data."** It stores the **query**; it reads live base tables each time. (An *indexed/materialized* view is a different, advanced thing.)
- **"Indexes are free speed."** They cost storage and **slow every write**. Index selectively — the columns you actually filter/join/sort on.
- **Over-indexing.** Ten indexes on a write-heavy table can hurt more than help.
- **Thinking you can have many clustered indexes.** Only **one** per table (it *is* the physical row order); the rest are nonclustered.
- **Indexing a column you never filter by.** Pure cost, no benefit. Index for your query patterns.
- **Expecting an index to help a `LIKE '%x%'`.** A leading wildcard can't use the sorted B-tree; only prefix searches (`LIKE 'x%'`) benefit.

## Decision Guide

**View vs raw query:** use a **view** when the same complex query is reused, when you want to expose a safe subset of columns, or to standardize a definition. Use a raw query for one-off exploration.

**Index or not:**

| Index the column when… | Skip the index when… |
|---|---|
| It's used in `WHERE`/`JOIN`/`ORDER BY` often | The table is tiny (a scan is already fast) |
| It's a foreign key joined frequently | The table is write-heavy and the column is rarely queried |
| Lookups by it must be fast at scale | You'd be adding yet another index to an already-indexed hot table |

## Summary

- A **view** is a named, stored `SELECT` — it packages complex queries, standardizes definitions, and can expose a **safe column subset** (security); it stores the query, not data.
- An **index** is a sorted **B-tree** that makes reads fast by avoiding full scans, at the cost of **storage and slower writes** — index the columns you filter/join/sort on.
- **Clustered** index = the table's physical sort order (**one** per table, usually the PK); **nonclustered** = separate pointer structures (**many** allowed, great for foreign keys).
- Index **selectively**; over-indexing taxes every write.

## Additional Resources

- [Views (SQL Server) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/relational-databases/views/views)
- [Clustered and nonclustered indexes described — Microsoft Learn](https://learn.microsoft.com/en-us/sql/relational-databases/indexes/clustered-and-nonclustered-indexes-described)
- [CREATE INDEX (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-index-transact-sql)
