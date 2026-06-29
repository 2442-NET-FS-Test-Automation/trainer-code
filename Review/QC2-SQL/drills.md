# QC-2 (SQL) — Drills

Short hands-on tasks, one or more per topic cluster. **Do the prompt in your own domain** (Inventory, Bank,
Garage, Music, Clinic — anything but Library), then check against the **model solution**, which uses the
trainer **Library** domain (`weeklytechrepo/SQL/demo/sql-training/LibraryDB.sql`). Run against SQL Server
(see `weeklytechrepo/SQL/demo/sql-training/docs/docker-setup.md`). These mirror the async-lab capabilities
in `weeklytechrepo/SQL/async-lab/sql-training-README.md`. Tip: try the prompt first, run it, then read the
solution.

---

## Drill 1 — Re-runnable two-table build *(DDL & constraints)*

**Prompt:** From an empty database, write a re-runnable build script for two related tables (a parent and a
child). Include `IDENTITY`, a primary key, and a foreign key. Run it twice and confirm no errors the second
time.

**Model solution** *(source: `weeklytechrepo/SQL/content/1-Monday/ddl.md`)*
```sql
DROP TABLE IF EXISTS dbo.Book;     -- children before parents
DROP TABLE IF EXISTS dbo.Author;
GO
CREATE TABLE dbo.Author (
    AuthorId  INT IDENTITY(1,1) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    CONSTRAINT PK_Author PRIMARY KEY (AuthorId)
);
GO
CREATE TABLE dbo.Book (
    BookId   INT IDENTITY(1,1) NOT NULL,
    Title    VARCHAR(200) NOT NULL,
    AuthorId INT NOT NULL,
    CONSTRAINT PK_Book PRIMARY KEY (BookId),
    CONSTRAINT FK_Book_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Author (AuthorId)
);
```
The `DROP ... IF EXISTS` block drops children -> parents; the `CREATE`s build parents -> children. Idempotent.

Proves QC: Construct SQL statements using DDL (CREATE, DROP, ALTER, TRUNCATE) keywords to generate tables;
Perform basic DDL operations.

---

## Drill 2 — All seven constraints on one table *(DDL & constraints)*

**Prompt:** Define one entity table that uses every constraint type: `PRIMARY KEY`, `FOREIGN KEY`,
`UNIQUE`, `NOT NULL`, `CHECK`, `DEFAULT`, and `IDENTITY`. Then write three `INSERT`s that each violate a
different rule and predict which constraint rejects each.

**Model solution** *(source: `weeklytechrepo/SQL/content/1-Monday/constraints.md`)*
```sql
CREATE TABLE dbo.Book (
    BookId          INT IDENTITY(1,1) NOT NULL,                      -- IDENTITY
    Title           VARCHAR(200) NOT NULL,                           -- NOT NULL
    ISBN            CHAR(13) NOT NULL,
    AuthorId        INT NOT NULL,
    TotalCopies     INT NOT NULL CONSTRAINT DF_Book_Total DEFAULT (1),     -- DEFAULT
    AvailableCopies INT NOT NULL DEFAULT (1),
    CONSTRAINT PK_Book PRIMARY KEY (BookId),                         -- PRIMARY KEY
    CONSTRAINT UQ_Book_ISBN UNIQUE (ISBN),                           -- UNIQUE
    CONSTRAINT CK_Book_Copies CHECK (AvailableCopies <= TotalCopies),-- CHECK
    CONSTRAINT FK_Book_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Author (AuthorId)
);
-- rejected by FK (no author 999); by CHECK (5 > 2); by NOT NULL (ISBN null):
INSERT INTO dbo.Book (Title, ISBN, AuthorId) VALUES ('X','9780000000001',999);
INSERT INTO dbo.Book (Title, ISBN, AuthorId, TotalCopies, AvailableCopies) VALUES ('Y','9780000000002',1,2,5);
INSERT INTO dbo.Book (Title, ISBN, AuthorId) VALUES ('Z',NULL,1);
```
Proves QC: Describe and utilize constraints in table creation (Unique, Not Null, Primary Key, Foreign Key,
Auto Incrementing, Default, Check).

---

## Drill 3 — DROP vs DELETE vs TRUNCATE *(DDL & constraints)*

**Prompt:** In one or two sentences each (a comment is fine), state what `DROP`, `DELETE`, and `TRUNCATE`
do, and write the statement for each against a table in your domain. Note which resets the identity counter.

**Model solution** *(source: `weeklytechrepo/SQL/content/2-Tuesday/dml.md`)*
```sql
DELETE FROM dbo.Loan WHERE LoanId = 42;  -- removes CHOSEN rows (WHERE); does NOT reset IDENTITY
TRUNCATE TABLE dbo.Loan;                 -- removes ALL rows fast, keeps table, RESETS IDENTITY
DROP TABLE dbo.Loan;                     -- removes the TABLE itself (structure + data)
```
`DELETE` = surgical by condition (DML); `TRUNCATE` = empty-all fast and reset ids (DDL); `DROP` = the table
should not exist (DDL).

Proves QC: Describe the difference between DROP, DELETE, and TRUNCATE functionality.

---

## Drill 4 — Seed in dependency order, then edit *(DML)*

**Prompt:** Seed at least three related tables with `INSERT`s in parent-before-child order (explicit column
lists, no `IDENTITY` values). Then write one `UPDATE` and one `DELETE`, each scoped by a `WHERE`.

**Model solution** *(source: `weeklytechrepo/SQL/content/2-Tuesday/dml.md`)*
```sql
INSERT INTO dbo.Author (FirstName, LastName) VALUES ('Robert','Martin'),('Martin','Fowler');  -- parents
INSERT INTO dbo.Member (FirstName, LastName, Email) VALUES ('Ada','Byron','ada@lib.org');
INSERT INTO dbo.Book (Title, ISBN, AuthorId) VALUES ('Clean Code','9780132350884',1);          -- child of Author
INSERT INTO dbo.Loan (BookId, MemberId, DueDate) VALUES (1,1,'2026-07-10');                     -- child of Book+Member

UPDATE dbo.Book SET AvailableCopies = AvailableCopies - 1 WHERE BookId = 1;
DELETE FROM dbo.Loan WHERE ReturnDate IS NOT NULL AND LoanDate < '2019-01-01';
```
Proves QC: Construct SQL statements using DML (Insert, Update, Delete) keywords to manipulate pre-existing
data within tables.

---

## Drill 5 — Filter, sort, de-duplicate *(DQL)*

**Prompt:** Write a `SELECT` that uses a `WHERE` with at least two operators (pick from `BETWEEN`, `IN`,
`LIKE`, `IS NULL`, comparisons), an `ORDER BY`, and a separate `DISTINCT` query. Include one `IS NULL`
filter.

**Model solution** *(source: `weeklytechrepo/SQL/content/2-Tuesday/dql.md`)*
```sql
SELECT Title, PublishedYear, AvailableCopies
FROM   dbo.Book
WHERE  AvailableCopies > 0 AND PublishedYear BETWEEN 2000 AND 2026
ORDER  BY PublishedYear DESC, Title ASC;

SELECT LoanId, DueDate FROM dbo.Loan WHERE ReturnDate IS NULL;   -- still checked out

SELECT DISTINCT PublishedYear FROM dbo.Book ORDER BY PublishedYear DESC;
```
Remember: `ReturnDate = NULL` is never true — use `IS NULL`.

Proves QC: Demonstrate the ability to filter records using the WHERE clause and operators.

---

## Drill 6 — Identify candidate keys, choose a PK *(keys, schema)*

**Prompt:** For one entity in your domain, list every candidate key (columns or combinations that uniquely
identify a row). Choose one as the primary key and justify why; enforce the rest with `UNIQUE`.

**Model solution** *(source: `weeklytechrepo/SQL/content/3-Wednesday/keys.md`)*
```sql
-- Member candidates: MemberId (surrogate), Email (natural). (FirstName, LastName) is NOT unique.
CREATE TABLE dbo.Member (
    MemberId INT IDENTITY(1,1) NOT NULL,
    Email    VARCHAR(120) NOT NULL,
    CONSTRAINT PK_Member PRIMARY KEY (MemberId),     -- chosen: stable, meaningless, never changes
    CONSTRAINT UQ_Member_Email UNIQUE (Email)        -- alternate key (emails change -> not the PK)
);
```
Pick the surrogate `MemberId` as PK because a natural key like `Email` can change; keep `Email` as a
`UNIQUE` alternate key.

Proves QC: Demonstrate how to identify valid candidate keys for a primary key of an entity; Recognize and
explain less common key types beyond primary and foreign keys.

---

## Drill 7 — Model an ERD into a schema *(keys, ERD, data types)*

**Prompt:** Take a one-sentence requirement in your domain ("track X, the Y who use them, and each Z
event"), sketch a small ERD (ASCII is fine), then implement it as tables with sensible data types. Use
`DECIMAL` for any money, `CHAR` for any fixed-length code, `VARCHAR` for variable text.

**Model solution** *(source: `weeklytechrepo/SQL/content/1-Monday/data-modeling-erd.md`)*
```
AUTHOR --< BOOK --< LOAN >-- MEMBER     (< = "many" side carries the FK)
```
```sql
CREATE TABLE dbo.Book (
    BookId   INT IDENTITY(1,1) PRIMARY KEY,
    Title    VARCHAR(200) NOT NULL,     -- variable text
    ISBN     CHAR(13) NOT NULL,         -- always 13 chars -> fixed
    Price    DECIMAL(10,2) NULL,        -- money -> DECIMAL, never FLOAT
    AuthorId INT NOT NULL,              -- FK on the many side
    CONSTRAINT FK_Book_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Author (AuthorId)
);
```
Nouns -> tables, properties -> columns, the "borrows/has" verb -> a FK on the many side.

Proves QC: Create a valid schema for a given data-set; Read and understand ERD; Translate real-world
problem descriptions into ER diagrams and implement them as a working relational schema; Identify and
implement common data types; Identify and implement advanced data types (BIGINT).

---

## Drill 8 — Normalize 0NF to 3NF with justification *(normalization)*

**Prompt:** Start from a flat, denormalized table in your domain (repeating columns and a transitive
dependency). Refactor it to 3NF in three steps, writing a one-line justification per step naming the
dependency you removed.

**Model solution** *(source: `weeklytechrepo/SQL/content/3-Wednesday/normalization.md`)*
```text
0NF: Book(BookId, Title, CategoryName, CategoryDesc, Author1, Author2)
1NF: remove the repeating Author1/Author2 group -> one row per (Book, Author). [atomic values]
2NF: Title/Category depend only on BookId, part of the (BookId,Author) key -> split Book out from a
     BookAuthor bridge. [no partial dependency]
3NF: CategoryDesc depends on CategoryName (a non-key) -> extract a Category table. [no transitive dependency]
```
```sql
CREATE TABLE dbo.Category (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY, Name VARCHAR(60) NOT NULL UNIQUE, Description VARCHAR(200) NULL);
-- Book now references CategoryId instead of repeating the description text.
```
Proves QC: Normalize a database schema from unnormalized form (0NF) to Third Normal Form (3NF), providing
step-by-step justification.

---

## Drill 9 — Resolve a many-to-many with a bridge table *(normalization, keys)*

**Prompt:** Find a many-to-many relationship in your domain and resolve it with a bridge/junction table.
The bridge's primary key must be a composite of the two foreign keys.

**Model solution** *(source: `weeklytechrepo/SQL/demo/sql-training/LibraryDB.sql`)*
```sql
CREATE TABLE dbo.BookAuthor (
    BookId   INT NOT NULL,
    AuthorId INT NOT NULL,
    CONSTRAINT PK_BookAuthor PRIMARY KEY (BookId, AuthorId),          -- composite PK
    CONSTRAINT FK_BA_Book   FOREIGN KEY (BookId)   REFERENCES dbo.Book (BookId)     ON DELETE CASCADE,
    CONSTRAINT FK_BA_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Author (AuthorId) ON DELETE CASCADE
);
```
Each row is one (book, author) pairing; the composite PK stops a pair repeating; each column is also a FK.

Proves QC: Capable of implementing bridge tables to handle many-to-many relationships between entities.

---

## Drill 10 — Demonstrate referential integrity and a cascade *(keys, cascades)*

**Prompt:** Show one rejected insert (an FK value with no matching parent), then declare a foreign key with
`ON DELETE CASCADE` and describe what deleting the parent does to the children.

**Model solution** *(source: `weeklytechrepo/SQL/content/3-Wednesday/keys.md`, `1-Monday/constraints.md`)*
```sql
-- rejected: referential integrity violation, author 999 does not exist
INSERT INTO dbo.Book (Title, ISBN, AuthorId) VALUES ('Orphan','9780000000003',999);

-- cascade: deleting an Author deletes their Books too (the delete flows down the relationship)
-- FK_Book_Author ... REFERENCES dbo.Author (AuthorId) ON DELETE CASCADE
DELETE FROM dbo.Author WHERE AuthorId = 4;   -- author 4 and all their books go
```
Proves QC: Describe referential integrity; Utilize cascades to define what happens to related tables during
DML operations.

---

## Drill 11 — Aggregate vs scalar, GROUP BY + HAVING *(functions & grouping)*

**Prompt:** Write one query using an aggregate function over groups (`GROUP BY` + `HAVING`) and one query
using a scalar function per row. Alias the aggregate column. State which is aggregate and which is scalar.

**Model solution** *(source: `weeklytechrepo/SQL/content/4-Thursday/functions.md`)*
```sql
-- aggregate: collapses rows -> one value per group
SELECT   AuthorId AS Author, COUNT(*) AS BookCount, AVG(TotalCopies) AS AvgCopies
FROM     dbo.Book
GROUP BY AuthorId
HAVING   COUNT(*) > 1;                       -- group filter (aggregate)

-- scalar: one result per row
SELECT UPPER(Title) AS LoudTitle, YEAR(GETDATE()) - PublishedYear AS AgeYears FROM dbo.Book;
```
Proves QC: Understand difference between aggregate and scalar functions; Utilize the GROUP BY clause;
Utilize the HAVING clause to filter aggregated query results; Identify and use commonly used aggregate
functions; Utilize column aliases.

---

## Drill 12 — Every join type *(joins)*

**Prompt:** With two related tables (deliberately leave one parent with no child and one child with a NULL
FK), write an `INNER`, `LEFT`, `RIGHT`, and `FULL OUTER` join and predict which rows each keeps. Use table
aliases.

**Model solution** *(source: `weeklytechrepo/SQL/content/4-Thursday/joins.md`)*
```sql
SELECT b.Title, a.LastName FROM dbo.Book b INNER JOIN dbo.Author a ON b.AuthorId = a.AuthorId; -- matches only
SELECT b.Title, a.LastName FROM dbo.Book b LEFT  JOIN dbo.Author a ON b.AuthorId = a.AuthorId; -- all books
SELECT b.Title, a.LastName FROM dbo.Book b RIGHT JOIN dbo.Author a ON b.AuthorId = a.AuthorId; -- all authors
SELECT b.Title, a.LastName FROM dbo.Book b FULL OUTER JOIN dbo.Author a ON b.AuthorId = a.AuthorId; -- all of both
-- find books with no author: LEFT JOIN then filter the join result
SELECT b.Title FROM dbo.Book b LEFT JOIN dbo.Author a ON b.AuthorId = a.AuthorId WHERE a.AuthorId IS NULL;
```
Proves QC: Understand basic types of joins and demonstrate usage in select statements (inner, left/right
outer, full outer, equi).

---

## Drill 13 — Subquery vs join *(joins & subqueries)*

**Prompt:** Answer the same question two ways — once with a subquery, once with a join — then write one
query that is *naturally* a subquery (a comparison against an aggregate). Note in a comment why.

**Model solution** *(source: `weeklytechrepo/SQL/content/4-Thursday/joins.md`)*
```sql
-- same answer, two shapes:
SELECT Title FROM dbo.Book WHERE AuthorId IN (SELECT AuthorId FROM dbo.Author WHERE BirthYear < 1950);
SELECT b.Title FROM dbo.Book b JOIN dbo.Author a ON b.AuthorId = a.AuthorId WHERE a.BirthYear < 1950;

-- naturally a subquery: compare each row to an aggregate of the whole set
SELECT Title, TotalCopies FROM dbo.Book WHERE TotalCopies > (SELECT AVG(TotalCopies) FROM dbo.Book);
```
Use a join when you need columns from both tables; a subquery for membership or an aggregate comparison.

Proves QC: Understand when to use subqueries versus joins in SQL logic; Utilize subquery structure to
execute a select statement.

---

## Drill 14 — Set operation *(set operations)*

**Prompt:** Combine two same-shape result sets in your domain with `UNION`, and explain the difference from
`UNION ALL` in a comment.

**Model solution** *(source: `weeklytechrepo/SQL/content/4-Thursday/joins.md`)*
```sql
SELECT LastName FROM dbo.Author
UNION                      -- removes duplicate rows (UNION ALL keeps them, and is faster)
SELECT LastName FROM dbo.Member;
```
Proves QC: Utilize set operations between multiple select statement.

---

## Drill 15 — A correct transaction *(transactions & ACID)*

**Prompt:** Wrap a multi-step write in your domain in a transaction with `TRY/CATCH` so that a failure in
any step rolls back **all** of them. Show (in a comment) what happens when a constraint blocks one step.

**Model solution** *(source: `weeklytechrepo/SQL/content/5-Friday/transactions.md`, `demo/sql-training/LibraryDB.sql`)*
```sql
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE dbo.Book SET AvailableCopies = AvailableCopies - 1 WHERE BookId = 1;  -- CK blocks if 0
        INSERT INTO dbo.Loan (BookId, MemberId, DueDate) VALUES (1, 2, '2026-07-15');
    COMMIT TRANSACTION;          -- both or neither
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;        -- if AvailableCopies was 0, the CHECK fires -> no loan row exists
    THROW;
END CATCH;
```
Atomicity proof: a half-done checkout is impossible. Commit once, at the end.

Proves QC: Describe the purpose of transactions in a database and when they are used; Understand database
consistency and utilize transactions to ensure data consistency in a set of SQL commands; Explain the ACID
properties.

---

## Drill 16 — View + stored procedure + index *(views, procs, indexes)*

**Prompt:** Package a useful multi-table query as a **view**, write a **stored procedure** that takes a
parameter and call it with `EXEC`, and add an **index** on a column you join/filter by (note the
read-vs-write trade-off).

**Model solution** *(source: `weeklytechrepo/SQL/demo/sql-training/LibraryDB.sql`, `content/5-Friday/views-indexes.md`)*
```sql
CREATE VIEW dbo.vw_ActiveLoans AS
    SELECT l.LoanId, m.FirstName + ' ' + m.LastName AS Member, b.Title, l.DueDate
    FROM dbo.Loan l JOIN dbo.Member m ON m.MemberId=l.MemberId JOIN dbo.Book b ON b.BookId=l.BookId
    WHERE l.ReturnDate IS NULL;
GO
CREATE PROCEDURE dbo.usp_GetMemberLoans @MemberId INT AS
    BEGIN SET NOCOUNT ON; SELECT b.Title, l.DueDate FROM dbo.Loan l JOIN dbo.Book b ON b.BookId=l.BookId
          WHERE l.MemberId=@MemberId AND l.ReturnDate IS NULL; END;
GO
EXEC dbo.usp_GetMemberLoans @MemberId = 1;
CREATE INDEX IX_Loan_MemberId ON dbo.Loan (MemberId);  -- faster joins/filters; costs storage + slower writes
```
Proves QC: Create and use views to store the results of a SQL query; Be able to create and call a Stored
Procedure; Describe database indexing and its benefits.

---

## Drill 17 — User-defined function and a trigger *(functions, triggers)*

**Prompt:** Write a scalar **user-defined function** that computes a value and use it in a `SELECT`. Then
write an **AFTER INSERT trigger** that logs each new row into an audit table. Note why a function cannot do
what the trigger does.

**Model solution** *(source: `weeklytechrepo/SQL/content/5-Friday/procedures-triggers-dcl.md`, `demo/sql-training/LibraryDB.sql`)*
```sql
CREATE FUNCTION dbo.fn_DaysOverdue (@dueDate DATE) RETURNS INT AS
BEGIN
    DECLARE @d INT = DATEDIFF(DAY, @dueDate, CAST(GETDATE() AS DATE));
    RETURN CASE WHEN @d > 0 THEN @d ELSE 0 END;     -- returns a value, NO side effects
END;
GO
SELECT LoanId, dbo.fn_DaysOverdue(DueDate) AS DaysOverdue FROM dbo.Loan;
GO
CREATE TRIGGER trg_Loan_AfterInsert ON dbo.Loan AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.LoanAudit (LoanId, BookId, MemberId)
    SELECT LoanId, BookId, MemberId FROM inserted;   -- 'inserted' = the new rows; auto-runs on every insert
END;
```
A function cannot `INSERT` (no side effects) — automating a write on a DML event is a trigger's job.

Proves QC: Be able to create a User Defined Function; Describe triggers and their use in automating tasks;
Configure triggers to execute the corresponding stored procedures when certain events occur.

---

## Drill 18 — ALTER an existing table *(DDL — Nice to Have)*

**Prompt:** On a populated table, add a new column and change an existing column's type with `ALTER TABLE`.
Explain why a `NOT NULL` column added to populated data needs a `DEFAULT`.

**Model solution** *(source: `weeklytechrepo/SQL/content/1-Monday/ddl.md`)*
```sql
ALTER TABLE dbo.Book ADD Edition INT NOT NULL DEFAULT (1);     -- existing rows need a value -> DEFAULT
ALTER TABLE dbo.Book ALTER COLUMN Title VARCHAR(250) NOT NULL; -- widening is safe; narrowing can truncate
```
Proves QC: Explain how to modify a table structure after creation using ALTER TABLE with examples.

---

## Drill 19 — Rank rows within a partition *(window functions — Nice to Have)*

> Awareness level — covered in the content notes (`content/4-Thursday/functions.md` / `joins.md`), not
> live-demoed. Grounded in those notes, the `qc-criteria/QC-2-SQL.md` Example column, and standard T-SQL.

**Prompt:** In your domain, rank rows **within groups** while keeping every detail row (e.g. rank each
product by price within its category). Use a window function with `OVER (PARTITION BY ... ORDER BY ...)`.
Note in a comment how this differs from `GROUP BY`.

**Model solution** *(source: `weeklytechrepo/SQL/content/4-Thursday/functions.md`)*
```sql
SELECT  Title,
        CategoryId,
        TotalCopies,
        ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY TotalCopies DESC) AS RankInCategory
FROM    dbo.Book;
-- GROUP BY CategoryId would return ONE row per category; the window keeps every book and labels it.
-- RANK() leaves gaps after ties (1,2,2,4); DENSE_RANK() does not (1,2,2,3).
```
Proves QC: Understand and know how to utilize Window Functions.

---

## Drill 20 — Factor a query with a CTE *(CTEs — Nice to Have)*

> Awareness level — covered in the content notes (`content/4-Thursday/functions.md` / `joins.md`), not
> live-demoed. Grounded in those notes, the `qc-criteria/QC-2-SQL.md` Example column, and standard T-SQL.

**Prompt:** Take a query in your domain that nests a subquery, and rewrite it with a **CTE** (`WITH name AS
(...)`) so it reads top-down. As a stretch, use a CTE plus a window function to return the **top row per
group** (a filter a `WHERE` can't do directly on a window function).

**Model solution** *(source: `weeklytechrepo/SQL/content/4-Thursday/joins.md`)*
```sql
-- a named step instead of a nested subquery
WITH RecentBooks AS (
    SELECT BookId, Title, PublishedYear FROM dbo.Book WHERE PublishedYear >= 2000
)
SELECT Title, PublishedYear FROM RecentBooks ORDER BY PublishedYear DESC;

-- top-N-per-group: window function ranked in a CTE, then filtered in the outer query
WITH Ranked AS (
    SELECT Title, CategoryId,
           ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY TotalCopies DESC) AS rn
    FROM dbo.Book
)
SELECT Title, CategoryId FROM Ranked WHERE rn = 1;   -- one top book per category
```
A CTE lives only for the single statement that follows it; for reuse across statements, use a view instead.

Proves QC: Utilize Common Table Expressions (CTEs).
