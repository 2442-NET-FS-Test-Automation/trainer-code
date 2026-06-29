# QC-2 (SQL) — Cheat Sheet

Dense quick-reference for the morning of the exam. T-SQL / SQL Server. Sourced from the Week-3 content
notes (`weeklytechrepo/SQL/content/**`) and the `sql-training` build (`weeklytechrepo/SQL/demo/sql-training/LibraryDB.sql`).

## Relational model — the "why relational" five

- **Structured organization** (typed columns) · **data integrity** (engine-enforced constraints) ·
  **powerful querying** (declarative SQL) · **reduced redundancy** (normalization) · **concurrency &
  durability**.
- Relationships are stored as **values** (FK = PK), not pointers — that's what makes it *relational*.
- Persistent (survives restart, many users, rules enforced) vs ephemeral (in-memory, lost on exit).

## SQL sublanguages

| Sublang | Purpose | Keywords |
|---|---|---|
| **DDL** | define structure | `CREATE` `ALTER` `DROP` `TRUNCATE` |
| **DML** | change data | `INSERT` `UPDATE` `DELETE` |
| **DQL** | read data | `SELECT` |
| **TCL** | transactions | `BEGIN` `COMMIT` `ROLLBACK` |
| **DCL** | permissions | `GRANT` `REVOKE` |

CRUD -> `INSERT` / `SELECT` / `UPDATE` / `DELETE`.

## DDL — CREATE / ALTER / DROP / TRUNCATE

```sql
CREATE TABLE dbo.Author (
    AuthorId  INT IDENTITY(1,1) NOT NULL,     -- IDENTITY(seed, step): auto-increment
    FirstName VARCHAR(50) NOT NULL,
    BirthYear INT NULL,
    CONSTRAINT PK_Author PRIMARY KEY (AuthorId)
);
ALTER TABLE dbo.Book ADD Edition INT NOT NULL DEFAULT (1);   -- add column (NOT NULL add needs DEFAULT)
ALTER TABLE dbo.Book ALTER COLUMN Title VARCHAR(250) NOT NULL; -- widen type (safe); narrow = risky
DROP TABLE IF EXISTS dbo.Loan;               -- re-runnable guard
TRUNCATE TABLE dbo.Loan;                      -- empties all rows, resets IDENTITY
```
Re-runnable build: **drop children -> parents**, then **create parents -> children**.

## DROP vs DELETE vs TRUNCATE

| | Removes | Sublang | `WHERE`? | Resets IDENTITY? | Speed |
|---|---|---|---|---|---|
| `DROP` | the **table** (structure+data) | DDL | n/a | n/a | fast |
| `TRUNCATE` | **all rows**, keeps table | DDL | **no** | **yes** | very fast |
| `DELETE` | **chosen rows** | DML | **yes** | **no** | slower |

## Constraints

| Constraint | Promise | Note |
|---|---|---|
| `PRIMARY KEY` | unique + not null, **one per table** | the row's identity |
| `FOREIGN KEY` | value matches a real parent PK | referential integrity |
| `UNIQUE` | no repeats, **many allowed** | may permit one NULL |
| `NOT NULL` | required | "is a value required?" |
| `CHECK` | custom same-row boolean | can't see other rows/tables |
| `DEFAULT` | value when omitted | "what if not supplied?" |
| `IDENTITY(1,1)` | auto-number | separate from being the key |

```sql
CONSTRAINT CK_Book_Copies CHECK (AvailableCopies >= 0 AND AvailableCopies <= TotalCopies)
CONSTRAINT FK_Book_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Author (AuthorId) ON DELETE CASCADE
```
Referential actions: `NO ACTION` (block, default) · `ON DELETE CASCADE` · `SET NULL` · `SET DEFAULT`.
(`ON UPDATE` has the same set.) Multiple cascade paths to one table -> SQL Server rejects the FK.

## DML — INSERT / UPDATE / DELETE

```sql
INSERT INTO dbo.Author (FirstName, LastName) VALUES ('Ada','Lovelace'),('Alan','Turing'); -- name columns; omit IDENTITY
UPDATE dbo.Book SET AvailableCopies = AvailableCopies - 1 WHERE BookId = 1;  -- WHERE or you hit every row
DELETE FROM dbo.Loan WHERE LoanId = 42;
INSERT INTO dbo.Archive (Name) SELECT Name FROM dbo.Member WHERE JoinedDate < '2020-01-01'; -- insert-from-select
```
Seed **parents before children**. Preview destructive writes with a `SELECT ... WHERE` first.

## DQL — SELECT / WHERE / ORDER BY / DISTINCT

```sql
SELECT Title, PublishedYear FROM dbo.Book
WHERE  AvailableCopies > 0 AND PublishedYear BETWEEN 2000 AND 2026
ORDER  BY PublishedYear DESC, Title ASC;
SELECT DISTINCT PublishedYear FROM dbo.Book;     -- de-dupes the whole selected row
```

| Operator | Means |
|---|---|
| `=` `<>` (`!=`) | equal / not equal |
| `<` `>` `<=` `>=` | comparison |
| `BETWEEN a AND b` | inclusive range |
| `IN (...)` | matches any in list |
| `LIKE` | `%` = any chars, `_` = one char |
| `IS NULL` / `IS NOT NULL` | test for null (**never** `= NULL`) |
| `AND` `OR` `NOT` | combine |

Logical clause order: **`FROM -> WHERE -> GROUP BY -> HAVING -> SELECT -> ORDER BY`**. So `WHERE` can't use a
`SELECT` alias; `ORDER BY` can.

## Keys & data types

| Key | What |
|---|---|
| Primary | chosen unique, not-null identity (one per table) |
| Candidate | any minimal unique set eligible to be PK |
| Alternate | a candidate not chosen -> enforce with `UNIQUE` |
| Composite | key spanning 2+ columns (bridge table PK) |
| Foreign | column referencing another table's PK |
| Surrogate / Natural | system id (`IDENTITY`) / real-world value (ISBN, email) |

| Type | Use | Note |
|---|---|---|
| `INT` | ids, counts, years | ±2.1B |
| `BIGINT` | very large ids/counts | when `INT` overflows |
| `VARCHAR(n)` | variable text | names, titles, emails |
| `CHAR(n)` | fixed-length codes | ISBN-13; pads with spaces |
| `DECIMAL(p,s)` | money / exact | **never `FLOAT`** for money |
| `DATE` | calendar date | `DATETIME2` if time needed |

Multiplicity: **1:1** = FK + `UNIQUE`; **1:N** = FK on the **many** side; **M:N** = **bridge table** with a
composite key. FK always lives on the "many" side.

## Normalization

| Form | Rule | Removes |
|---|---|---|
| 1NF | atomic values, no repeating groups | multi-valued cells |
| 2NF | 1NF + non-key depends on **whole** key | partial deps (composite-key only) |
| 3NF | 2NF + no non-key depends on another **non-key** | transitive deps |

Mnemonic: "the key, the whole key, and nothing but the key." Anomalies removed: **insertion / update /
deletion**. Benefit: each fact once, integrity. Drawback: more joins, slower reads -> denormalize for OLAP.
M:N -> bridge table:
```sql
CREATE TABLE dbo.BookAuthor (
    BookId INT NOT NULL, AuthorId INT NOT NULL,
    CONSTRAINT PK_BookAuthor PRIMARY KEY (BookId, AuthorId),  -- composite PK = two FKs
    CONSTRAINT FK_BA_Book FOREIGN KEY (BookId) REFERENCES dbo.Book(BookId),
    CONSTRAINT FK_BA_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Author(AuthorId));
```

## Functions & grouping

| | Aggregate | Scalar |
|---|---|---|
| Input -> output | **many rows -> one** | **one value -> one per row** |
| Examples | `COUNT` `SUM` `AVG` `MIN` `MAX` | `UPPER` `LEN` `GETDATE` `ROUND` `YEAR` |

```sql
SELECT AuthorId AS Author, COUNT(*) AS [Book Count]   -- [ ] for aliases with spaces
FROM   dbo.Book
WHERE  PublishedYear >= 2000        -- WHERE filters ROWS (before grouping, no aggregates)
GROUP  BY AuthorId
HAVING COUNT(*) > 1                  -- HAVING filters GROUPS (after, aggregates allowed)
ORDER  BY [Book Count] DESC;
```
Golden rule: every `SELECT` column is grouped or aggregated. `COUNT(*)` counts rows; `COUNT(col)` skips
NULLs; `AVG`/`SUM` ignore NULLs.

## Joins

| Join | Keeps |
|---|---|
| `INNER` | only rows matching both sides |
| `LEFT` | all of left, NULLs for unmatched right |
| `RIGHT` | all of right (flip to LEFT for readability) |
| `FULL OUTER` | all of both, NULLs where unmatched |
| `CROSS` | every combination (no `ON`) — Cartesian |

```sql
SELECT b.Title, a.LastName
FROM   dbo.Book b
LEFT JOIN dbo.Author a ON b.AuthorId = a.AuthorId;   -- equi-join (=); theta uses <,>,BETWEEN
```
Trap: **`LEFT JOIN` + `WHERE` on the right table = silent `INNER`** — filter the right table in `ON`.
Subquery vs join: join -> need columns from both; subquery -> membership/existence or compare to an aggregate
(`WHERE Price > (SELECT AVG(Price) FROM Book)`). `UNION` stacks + dedupes; `UNION ALL` keeps dupes
(awareness).

## Transactions & ACID

```sql
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE dbo.Book SET AvailableCopies = AvailableCopies - 1 WHERE BookId = 1;
        INSERT INTO dbo.Loan (BookId, MemberId, DueDate) VALUES (1, 2, '2026-07-15');
    COMMIT TRANSACTION;            -- once, at the end
END TRY
BEGIN CATCH ROLLBACK TRANSACTION; THROW; END CATCH;
```

| ACID | Means |
|---|---|
| **A**tomicity | all steps or none |
| **C**onsistency | moves between **constraint-valid** states |
| **I**solation | concurrent txns don't corrupt each other |
| **D**urability | committed survives a crash (post-commit only) |

| Isolation level | Dirty | Non-repeat | Phantom |
|---|---|---|---|
| Read Uncommitted | yes | yes | yes |
| **Read Committed** (default) | no | yes | yes |
| Repeatable Read | no | no | yes |
| Serializable | no | no | no |

Higher level = fewer anomalies, less concurrency. Consistency ≠ Isolation.

## Views, procs, functions, triggers, indexes, DCL

```sql
CREATE VIEW dbo.vw_ActiveLoans AS                 -- stores the QUERY, not data
    SELECT l.LoanId, b.Title FROM dbo.Loan l JOIN dbo.Book b ON b.BookId=l.BookId
    WHERE l.ReturnDate IS NULL;

CREATE PROCEDURE dbo.usp_GetMemberLoans @MemberId INT AS   -- callable, can change data
    BEGIN SET NOCOUNT ON; SELECT * FROM dbo.Loan WHERE MemberId=@MemberId; END;
EXEC dbo.usp_GetMemberLoans @MemberId = 1;        -- call with EXEC

CREATE FUNCTION dbo.fn_DaysOverdue (@due DATE) RETURNS INT AS  -- returns a value, NO side effects
    BEGIN RETURN CASE WHEN DATEDIFF(DAY,@due,GETDATE())>0 THEN DATEDIFF(DAY,@due,GETDATE()) ELSE 0 END; END;
SELECT dbo.fn_DaysOverdue(DueDate) FROM dbo.Loan; -- use inside a query

CREATE TRIGGER trg_Loan_AfterInsert ON dbo.Loan AFTER INSERT AS  -- auto-runs on DML
    BEGIN INSERT INTO dbo.LoanAudit (LoanId,BookId,MemberId)
          SELECT LoanId,BookId,MemberId FROM inserted; END;      -- 'inserted'/'deleted' virtual tables

CREATE INDEX IX_Loan_MemberId ON dbo.Loan (MemberId);  -- B-tree: faster reads, slower writes
GRANT SELECT ON dbo.vw_ActiveLoans TO LibraryReadOnly; -- DCL: grant the safe surface, lock base tables
REVOKE EXECUTE ON dbo.usp_CheckoutBook FROM LibraryReadOnly;
```

| | Stored procedure | UDF |
|---|---|---|
| Returns | optional output / result sets | **must return** a value |
| Called | `EXEC` (a statement) | inside a query (`SELECT dbo.fn(...)`) |
| Modify data | **yes** | **no** (no side effects) |

| Index | Clustered | Nonclustered |
|---|---|---|
| What | sorts the **table** itself | separate structure pointing back |
| How many | **one** per table (usually PK) | **many** (great for FKs) |

## Window functions & CTEs

*(Awareness level — covered in the content notes (`content/4-Thursday/functions.md` / `joins.md`), not live-demoed. Grounded in those notes + the `qc-criteria/QC-2-SQL.md` Example column + standard T-SQL.)*

```sql
-- WINDOW FUNCTION: computes over a row-set WITHOUT collapsing rows (unlike GROUP BY)
SELECT Title, CategoryId, TotalCopies,
       ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY TotalCopies DESC) AS RankInCat
FROM   dbo.Book;
-- PARTITION BY = split into groups (non-collapsing); ORDER BY = order within each window
```

| Window function | Per partition |
|---|---|
| `ROW_NUMBER()` | unique 1,2,3,4 (ties broken arbitrarily) |
| `RANK()` | gaps after ties: 1,2,2,4 |
| `DENSE_RANK()` | no gaps after ties: 1,2,2,3 |
| `SUM()/COUNT()/AVG() OVER (...)` | running / partition-wide aggregate kept per row |

Window vs `GROUP BY`: `GROUP BY` returns one row per group; a window function **keeps every row** and labels
it. A window function can't go in `WHERE` (computed after) -> wrap in a CTE/subquery, then filter.

```sql
-- CTE: a named temporary result set for ONE following statement (WITH name AS (...))
WITH RecentBooks AS (
    SELECT BookId, Title, PublishedYear FROM dbo.Book WHERE PublishedYear >= 2000
)
SELECT Title FROM RecentBooks ORDER BY PublishedYear DESC;

-- top-N-per-group: CTE + window function + outer filter
WITH Ranked AS (
    SELECT Title, CategoryId,
           ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY TotalCopies DESC) AS rn
    FROM dbo.Book)
SELECT Title, CategoryId FROM Ranked WHERE rn = 1;   -- one top book per category
```
CTE vs view: a CTE is scoped to the one next statement (not stored); a view is reusable and stored. CTEs can
be recursive (self-referencing, for hierarchies) — awareness level.
