# QC-2 (SQL) — Study Guide

Organized by topic cluster. Each cluster lists the QC-2 objectives it covers (verbatim, tagged with tier),
a concept recap synthesized from the Week-3 content notes with source pointers, the key points and pitfalls
interviewers probe, and one annotated worked example from the `sql-training` Library thread. Dialect is
**T-SQL / SQL Server** throughout.

Source roots:
- Content notes: `weeklytechrepo/SQL/content/{1-Monday,2-Tuesday,3-Wednesday,4-Thursday,5-Friday}/`
- Demo scripts: `weeklytechrepo/SQL/demo/walkthroughs/`
- End-state code (answer key): `weeklytechrepo/SQL/demo/sql-training/LibraryDB.sql`
- Trainer notes: `trainer-code/Notes/SQL/{SQL-Intro,SQL-Intermediate}/`

---

## 1. Relational Model & RDBMS Rationale

**Objectives covered**
- *(Must)* Articulate the reasons for using a Relational Database Model to represent data.

**Concept recap** *(source: `weeklytechrepo/SQL/content/1-Monday/sql-intro.md`)*
A **database** is persistent, shared, rule-enforcing storage managed by a **DBMS**. When the DBMS organizes
data into **tables of rows and columns** and lets tables relate to one another, it is a **relational** DBMS
(**RDBMS**) — SQL Server, PostgreSQL, MySQL, Oracle. The relational model (Codd, 1970) stores data as
**relations** (tables) and expresses relationships as **values**: a `Loan` row does not hold a memory
pointer to a `Book`, it holds a `BookId` **value** that matches a `Book` row's key. That is why
relationships survive restarts, copies, and backups. The five "why relational" reasons to memorize:
**structured organization** (typed columns, predictable shape), **data integrity** (constraints/types the
engine enforces), **powerful querying** (declarative SQL — say *what*, not *how*), **reduced redundancy**
(via normalization, each fact stored once), and **safe concurrency & durability** (many writers, committed
data survives crashes). The contrast is **ephemeral** in-memory state (a `List<Book>` lost on exit) versus
**persistent** storage that also enforces **consistency** — invalid states are impossible to write.

**Key points / pitfalls**
- "SQL is a database" is wrong: SQL is the *language*; the database (SQL Server) is the *system* you speak
  it to. Mixing these up reads as inexperience.
- Relational *reduces* redundancy via normalization; it does not magically *eliminate* all of it.
- Relational is the default, not the only option — document (MongoDB), key-value (Redis), and graph stores
  exist for shapes that do not fit tables. Knowing this signals depth.

**Worked example** *(cited: `weeklytechrepo/SQL/content/1-Monday/sql-intro.md`)*
```sql
-- Relationships are VALUES, not pointers: Loan.BookId matches Book.BookId.
-- This is the entire idea behind "relational" -- the link is durable data.
SELECT b.Title, l.DueDate
FROM   dbo.Loan l
JOIN   dbo.Book b ON b.BookId = l.BookId;   -- the value match IS the relationship
```

---

## 2. SQL Sublanguages

**Objectives covered**
- *(Must)* Explain the usage of the sublanguages in SQL

**Concept recap** *(source: `weeklytechrepo/SQL/content/1-Monday/sql-intro.md`)*
SQL is one language whose keywords group into five **sublanguages** by *what they do*. The whole week is
organized around them:

| Sublanguage | Stands for | Purpose | Keywords |
|---|---|---|---|
| **DDL** | Data Definition Language | define/shape structure | `CREATE`, `ALTER`, `DROP`, `TRUNCATE` |
| **DML** | Data Manipulation Language | change data in tables | `INSERT`, `UPDATE`, `DELETE` |
| **DQL** | Data Query Language | read/query data | `SELECT` |
| **TCL** | Transaction Control Language | group changes all-or-nothing | `BEGIN`, `COMMIT`, `ROLLBACK` |
| **DCL** | Data Control Language | manage permissions | `GRANT`, `REVOKE` |

**CRUD** maps onto the verbs: Create->`INSERT`, Read->`SELECT`, Update->`UPDATE`, Delete->`DELETE`.

**Key points / pitfalls**
- Most people remember DDL and DML and stall. Practice listing all five out loud (DDL, DML, DQL, TCL, DCL).
- Some texts fold DQL's `SELECT` into DML and call it four sublanguages. If asked, name all five but note
  that `SELECT` is sometimes grouped under DML — the nuance signals real understanding.
- `TRUNCATE` is DDL even though it removes rows (it deallocates storage pages, not row-by-row).

**Worked example** *(cited: `weeklytechrepo/SQL/content/1-Monday/sql-intro.md`)*
```sql
CREATE TABLE dbo.Member (                                  -- DDL: define
    MemberId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    Email VARCHAR(120) NOT NULL UNIQUE);
INSERT INTO dbo.Member (FirstName, Email)                  -- DML: change
    VALUES ('Ada', 'ada@example.com');
SELECT MemberId, FirstName FROM dbo.Member;                -- DQL: read
GRANT SELECT ON dbo.Member TO LibraryReadOnly;             -- DCL: permissions
```

---

## 3. DDL & Constraints

**Objectives covered**
- *(Must)* Construct SQL statements using DDL (CREATE, DROP, ALTER, TRUNCATE) keywords to generate tables
- *(Must)* Describe and utilize constraints in table creation. (Unique, Not Null, Primary Key, Foreign Key, Auto Incrementing, Default, Check)
- *(Must)* Describe the difference between DROP, DELETE, and TRUNCATE functionality
- *(Must)* Perform basic DDL operations, such as creating, dropping, or truncating tables.
- *(Should)* Utilize cascades to define what happens to related tables during DML operations
- *(Nice)* Explain how to modify a table structure after creation using ALTER TABLE with examples (e.g., adding a column, modifying a data type).

**Concept recap** *(source: `weeklytechrepo/SQL/content/1-Monday/ddl.md`, `.../constraints.md`)*
**DDL** shapes structure: `CREATE` makes an object, `ALTER` changes an existing one in place, `DROP`
removes the whole object, `TRUNCATE` fast-empties a table's rows but keeps the table. `CREATE TABLE`
declares columns with types, nullability, and **`IDENTITY(1,1)`** auto-increment (start 1, step 1 — the
engine assigns it, you never insert it). `ALTER TABLE ... ADD` bolts on a column (a `NOT NULL` add on a
populated table needs a `DEFAULT`); `ALTER COLUMN` changes a type (widening is safe, narrowing risky).

**Constraints** are rules the *engine* enforces for every writer: **PRIMARY KEY** (unique + not null,
one per table), **FOREIGN KEY** (value must match a real parent row — referential integrity), **UNIQUE**
(no repeats, many allowed, may permit one NULL), **NOT NULL** (required), **CHECK** (a custom same-row
boolean), **DEFAULT** (value when none supplied). `NOT NULL` answers "required?"; `DEFAULT` answers "what
if omitted?" — they combine. A foreign key takes a **referential action** — `NO ACTION` (block, default),
`ON DELETE CASCADE` (delete children too), `ON DELETE SET NULL`, `ON DELETE SET DEFAULT`.

**DROP vs DELETE vs TRUNCATE** is the single most-asked SQL distinction:

| | Removes | Sublang | `WHERE`? | Resets IDENTITY? | Speed |
|---|---|---|---|---|---|
| `DROP` | the whole **table** (structure + data) | DDL | n/a | n/a (gone) | fast |
| `TRUNCATE` | **all rows**, keeps table | DDL | **no** | **yes** | very fast |
| `DELETE` | **chosen rows** (or all) | DML | **yes** | **no** | slower |

A **re-runnable** build drops children before parents, then creates parents before children — idempotent
every run.

**Key points / pitfalls**
- Wrong create/drop order -> *"foreign key references invalid table."* Create parents first; drop children
  first.
- A `NOT NULL` `ALTER ADD` on populated data without a `DEFAULT` fails — existing rows have no value.
- `TRUNCATE` is not "a fast `DELETE`": no `WHERE`, resets `IDENTITY`, minimally logged, can be blocked when
  the table is referenced by an FK.
- Multiple cascade paths to the same table make SQL Server reject the FK ("may cause cycles or multiple
  cascade paths") — exactly why `LibraryDB` drops `FK_Book_Author` before adding the `BookAuthor` bridge.
- Prefer **named, table-level** constraints (`PK_`, `FK_`, `UQ_`, `CK_`, `DF_`) for readable errors.

**Worked example** *(cited: `weeklytechrepo/SQL/demo/sql-training/LibraryDB.sql`)*
```sql
CREATE TABLE dbo.Book
(
    BookId          INT          IDENTITY(1,1) NOT NULL,     -- auto-increment
    Title           VARCHAR(200) NOT NULL,                   -- NOT NULL
    ISBN            CHAR(13)     NOT NULL,
    PublishedYear   INT          NULL,                       -- optional
    AuthorId        INT          NOT NULL,
    TotalCopies     INT          NOT NULL CONSTRAINT DF_Book_TotalCopies DEFAULT (1),     -- DEFAULT
    AvailableCopies INT          NOT NULL CONSTRAINT DF_Book_AvailableCopies DEFAULT (1),
    CONSTRAINT PK_Book PRIMARY KEY (BookId),                 -- PRIMARY KEY
    CONSTRAINT UQ_Book_ISBN UNIQUE (ISBN),                   -- UNIQUE
    CONSTRAINT CK_Book_Copies CHECK (AvailableCopies <= TotalCopies),   -- CHECK
    CONSTRAINT FK_Book_Author FOREIGN KEY (AuthorId)         -- FOREIGN KEY
        REFERENCES dbo.Author (AuthorId) ON DELETE CASCADE   -- CASCADE
);
```
One table declares all seven constraint types; the engine now rejects a book with a missing ISBN, an
unknown author, or more available copies than it owns.

---

## 4. DML & DQL (Changing and Reading Data)

**Objectives covered**
- *(Must)* Construct SQL statements using DML(Insert, Update, Delete) keywords to manipulate pre-existing data within tables
- *(Must)* Demonstrate the ability to filter records using the WHERE clause and operators

**Concept recap** *(source: `weeklytechrepo/SQL/content/2-Tuesday/dml.md`, `.../dql.md`)*
**DML** changes data: `INSERT` (always name the columns; omit `IDENTITY`; seed **parents before
children**), `UPDATE ... SET ... WHERE`, `DELETE FROM ... WHERE`. The `WHERE` clause decides *which* rows —
an `UPDATE`/`DELETE` without it hits **every** row, with no undo outside a transaction. Preview destructive
writes with a matching `SELECT ... WHERE` first.

**DQL** reads data. `SELECT cols FROM table WHERE filter ORDER BY cols`. `WHERE` operators: `=`, `<>`,
`<`/`>`/`<=`/`>=`, `BETWEEN a AND b`, `IN (...)`, `LIKE` (`%` = any chars, `_` = one char), `IS NULL` /
`IS NOT NULL`, combined with `AND`/`OR`/`NOT`. `ORDER BY` sorts (`ASC` default / `DESC`, multi-key);
without it, order is **undefined**. `DISTINCT` removes duplicate **rows** from the output. The logical
clause order is **`FROM -> WHERE -> GROUP BY -> HAVING -> SELECT -> ORDER BY`** — which is why `WHERE` cannot
use a `SELECT` alias but `ORDER BY` can.

**Key points / pitfalls**
- `col = NULL` is **never true** (NULL = unknown). Use `IS NULL` / `IS NOT NULL`. In the Library model a
  NULL `ReturnDate` means "still checked out," so active loans are `WHERE ReturnDate IS NULL`.
- Positional `INSERT` (no column list) is brittle and forces every column — name your columns.
- `DISTINCT` de-dupes the whole **selected row**, not one column; add columns and "duplicates" reappear.
- Relying on row order without `ORDER BY` is luck, not a contract.

**Worked example** *(cited: `weeklytechrepo/SQL/demo/sql-training/queries/02-dml-dql.sql`, `content/2-Tuesday/dml.md`)*
```sql
-- seed in dependency order (parents first), then a scoped edit
INSERT INTO dbo.Loan (BookId, MemberId, DueDate)
VALUES (1, 1, '2026-07-10');                          -- LoanDate defaults; ReturnDate NULL = out

UPDATE dbo.Book SET AvailableCopies = AvailableCopies - 1 WHERE BookId = 1;   -- checkout

-- read it back: active loans only, soonest due first
SELECT LoanId, BookId, MemberId, DueDate
FROM   dbo.Loan
WHERE  ReturnDate IS NULL
ORDER  BY DueDate;
```

---

## 5. Keys, Schema & ERD

**Objectives covered**
- *(Must)* Understand the role of a primary key in a data-set.
- *(Must)* Understand the role of a foreign key in a data-set.
- *(Must)* Create a valid schema for a given data-set
- *(Must)* Describe referential integrity
- *(Should)* Demonstrate how to identify valid candidate keys for a primary key of an entity
- *(Should)* Read and understand ERD (Entity Relationship Diagram)
- *(Should)* Explain the concept of multiplicity in database relationships.
- *(Should)* Accurately describe database schemas, including tables, fields, and the relationships between them.
- *(Should)* Identify and implement common data typessuch as varchar, decimal, integer, and char.
- *(Should)* Translate real-world problem descriptions into ER diagrams and implement them as a working relational schema.
- *(Nice)* Recognize and explain less common key types beyond primary and foreign keys, such as candidate keys or composite keys.
- *(Nice)* Identify and implement advanced data types beyond the basics, such as BIGINT for handling very large integers

**Concept recap** *(source: `weeklytechrepo/SQL/content/3-Wednesday/keys.md`, `1-Monday/data-modeling-erd.md`)*
A **primary key** is the column(s) that **uniquely identifies each row** — its role is **identity**
(answers "which row?"), the **anchor** every foreign key points at, enforced **unique + not null**, and
backed by an index for fast retrieval. A PK can never be NULL (an unknown identity is a contradiction).
Prefer a **surrogate key** (a meaningless auto-numbered `IDENTITY`) over a **natural key** (email, SSN)
because real-world values change. A **foreign key** is a column whose value **must match a PK value in
another table** (or be NULL): it *encodes a relationship* and *enforces referential integrity*. A table has
**one** PK but may have **many** FKs (a `Loan` has two).

**Other key types:** **candidate key** (any minimal unique column-set eligible to be the PK), **alternate
key** (a candidate not chosen, enforced with `UNIQUE`), **composite key** (two+ columns together, e.g. a
bridge table's PK), **secondary key** (a non-unique lookup column).

**Referential integrity** is the property that every FK value refers to a row that actually exists (or is
NULL) — no dangling pointers, no orphans. The FK enforces it both directions: you cannot insert a child
pointing at a missing parent, and you cannot delete a parent a child still references unless a referential
action says otherwise.

An **ERD** pictures the model: boxes are **entities** (tables), their lines are **relationships** (foreign
keys), and keys are marked **PK**/**FK**. **Multiplicity** (cardinality) is **1:1** (FK + `UNIQUE`),
**1:N** (FK on the **many** side), or **M:N** (a **bridge table** with a composite key). To build a schema
from English: nouns->entities, properties->columns, verbs/links->relationships, decide multiplicity, pick a
PK and a data type per column. **Data types**: `INT` (default integer), `BIGINT` (very large ids/counts),
`VARCHAR(n)` (variable text), `CHAR(n)` (fixed-length codes like ISBN-13), `DECIMAL(p,s)` (exact money —
`DECIMAL(10,2)`), `DATE`.

**Key points / pitfalls**
- "A primary key is just an auto-increment id" misses the *role* — unique non-null identity and the anchor
  of relationships. `IDENTITY` is one way to generate one, not the definition.
- The **FK lives on the "many" side**: books name their author, authors do not list their books.
- M:N **cannot** be done with a single FK — always a bridge table.
- **Never store money as `FLOAT`/`REAL`** — binary floating point cannot represent `0.10` exactly. Use
  `DECIMAL`. Classic interview gotcha.
- `CHAR(50)` for a name wastes space and pads with spaces (sneaky comparison bugs) — use `CHAR` only for
  genuinely fixed-length codes.
- Referential integrity (the *guarantee*) is not the same as cascade (the *policy* for honoring it).

**Worked example** *(cited: `weeklytechrepo/SQL/demo/sql-training/LibraryDB.sql`)*
```sql
-- Member: surrogate PK + an alternate (candidate) key promoted via UNIQUE
CREATE TABLE dbo.Member
(
    MemberId  INT IDENTITY(1,1) NOT NULL,            -- surrogate PK (chosen identity)
    Email     VARCHAR(120) NOT NULL,                 -- a candidate key...
    JoinedDate DATE NOT NULL CONSTRAINT DF_Member_JoinedDate DEFAULT (GETDATE()),
    CONSTRAINT PK_Member PRIMARY KEY (MemberId),
    CONSTRAINT UQ_Member_Email UNIQUE (Email)        -- ...kept as an alternate key
);
-- referential integrity in action: rejected because author 999 does not exist
-- INSERT INTO dbo.Book (Title, ISBN, AuthorId) VALUES ('Orphan', '978...', 999);
```

---

## 6. Normalization (0NF -> 3NF)

**Objectives covered**
- *(Must)* Normalize a database schema from unnormalized form (0NF) to Third Normal Form (3NF), providing step-by-step justification.
- *(Should)* Explain the benefits and potential drawbacks of database normalization, including impacts on performance and data integrity.
- *(Nice)* Capable of implementing bridge tables to handle many-to-many relationships between entities.

**Concept recap** *(source: `weeklytechrepo/SQL/content/3-Wednesday/normalization.md`)*
Redundancy (a fact stored in many places) causes three **anomalies**: **update** (fix it in one row, the
others now contradict), **insertion** (can't record a fact until an unrelated row exists), **deletion**
(remove the last row and a fact vanishes with it). Normalization removes them step by step:

| Form | Rule (one line) | Removes |
|---|---|---|
| **1NF** | atomic values, no repeating groups | multi-valued cells / repeated columns (`Author1`, `Author2`) |
| **2NF** | 1NF + every non-key column depends on the **whole** key | partial dependencies (only bites with a **composite** key) |
| **3NF** | 2NF + no non-key column depends on another **non-key** column | transitive dependencies (`BookId -> CategoryName -> CategoryDesc`) |

Mnemonic: every non-key column depends on **"the key, the whole key, and nothing but the key."** A
single-column-PK table in 1NF is automatically in 2NF. **Many-to-many** resolves to a **bridge/junction
table** whose composite primary key is two foreign keys.

**Benefits**: each fact stored once -> no anomalies, less storage, integrity easy to enforce. **Drawbacks**:
data spread across more tables -> more **joins** to reassemble, more complex/slower queries. 3NF is the
default for transactional (OLTP) systems; **denormalize** deliberately for read-heavy reporting (OLAP).
"Normalize until it hurts, denormalize until it works."

**Key points / pitfalls**
- Recite the *rule each form removes*, not just "1, 2, 3."
- 2NF is about depending on part of the **key**; 3NF is about depending on another **non-key** column.
  Different dependency, different fix.
- More normalization is not always better — 3NF is the sweet spot; BCNF/4NF are out of scope.
- M:N is *always* a bridge table with a composite key, never a single FK.

**Worked example** *(cited: `weeklytechrepo/SQL/demo/sql-training/LibraryDB.sql`, `content/3-Wednesday/normalization.md`)*
```sql
-- 3NF fix: CategoryDescription depended on CategoryName (a non-key) -> extract a table.
CREATE TABLE dbo.Category
(
    CategoryId  INT IDENTITY(1,1) NOT NULL,
    Name        VARCHAR(60)  NOT NULL,
    Description VARCHAR(200) NULL,
    CONSTRAINT PK_Category PRIMARY KEY (CategoryId),
    CONSTRAINT UQ_Category_Name UNIQUE (Name)
);
-- M:N fix: a book can have many authors -> bridge table with a composite PK of two FKs.
CREATE TABLE dbo.BookAuthor
(
    BookId   INT NOT NULL,
    AuthorId INT NOT NULL,
    CONSTRAINT PK_BookAuthor PRIMARY KEY (BookId, AuthorId),          -- composite key
    CONSTRAINT FK_BookAuthor_Book   FOREIGN KEY (BookId)   REFERENCES dbo.Book (BookId)     ON DELETE CASCADE,
    CONSTRAINT FK_BookAuthor_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Author (AuthorId) ON DELETE CASCADE
);
```
"Programming craft" is now stored once in `Category`; co-authors become extra rows in `BookAuthor`.

---

## 7. Functions & Grouping

**Objectives covered**
- *(Must)* Understand difference between aggregate and scalar functions
- *(Must)* Utilize the GROUP BY clause.
- *(Should)* Utilize the HAVING clause to filter aggregated query results
- *(Should)* Identify and use commonly used aggregate functions (e.g., COUNT(), SUM(), AVG(), MIN(), MAX()) to summarize data in queries.
- *(Should)* Utilize column aliases to enhance readability and clarity of SQL queries.

**Concept recap** *(source: `weeklytechrepo/SQL/content/4-Thursday/functions.md`)*
An **aggregate** function *collapses* many rows into one value (`COUNT`, `SUM`, `AVG`, `MIN`, `MAX`); a
**scalar** function *transforms* one value per row (`UPPER`, `LEN`, `GETDATE`, `ROUND`, `YEAR`). That
contrast is the Must-know. **`GROUP BY`** collapses rows sharing a value into one row per group, computing
aggregates within each group — and the **golden rule** is that every column in `SELECT` must be either in
the `GROUP BY` or wrapped in an aggregate. **`HAVING`** filters **groups** *after* aggregation (and can use
an aggregate); **`WHERE`** filters **rows** *before* grouping (and cannot). A **column alias** (`AS`) names
an aggregate column or renames output (`[ ]` for spaces); a **table alias** shortens names and is essential
in joins.

**Key points / pitfalls**
- `WHERE COUNT(*) > 1` is illegal — aggregates filter in `HAVING`.
- `COUNT(*)` counts rows (including NULLs); `COUNT(col)` skips NULLs; `AVG`/`SUM` ignore NULLs (so `AVG`
  divides by the non-null count). Quiet trap.
- "Is `COUNT` scalar?" — no, it is aggregate (many rows -> one value).
- Read a grouped query as a pipeline: `WHERE` (keep rows) -> `GROUP BY` (bucket) -> `HAVING` (keep buckets) ->
  `ORDER BY` (rank).

**Worked example** *(cited: `weeklytechrepo/SQL/demo/sql-training/queries/04-joins-functions.sql`, `content/4-Thursday/functions.md`)*
```sql
SELECT   MemberId      AS Member,
         COUNT(*)      AS LoanCount,        -- aggregate, aliased
         MAX(LoanDate) AS MostRecentLoan
FROM     dbo.Loan
GROUP BY MemberId                            -- one row per member
HAVING   COUNT(*) > 1                        -- only repeat borrowers (group filter)
ORDER BY LoanCount DESC;
```
"Who are our most active borrowers, and when did they last borrow?" — `COUNT`/`MAX` aggregating, `GROUP BY`
bucketing, `HAVING` dropping one-offs, aliases naming.

---

## 8. Joins & Subqueries

**Objectives covered**
- *(Must)* Understand basic types of joins and demonstrate usage in select statements (inner, left/right outer, full outer, equi)
- *(Should)* Understand when to use subqueries versus joins in SQL logic
- *(Nice)* Utilize subquery structure to execute a select statement.
- *(Nice)* Utilize set operations between multiple select statement

**Concept recap** *(source: `weeklytechrepo/SQL/content/4-Thursday/joins.md`)*
Joins recombine the data normalization spread out. **`INNER JOIN`** keeps only rows that match on both
sides. **`LEFT JOIN`** keeps every left row (NULLs where the right has no match) — the tool for finding
unmatched rows (`... WHERE right.key IS NULL`). **`RIGHT JOIN`** is the mirror (any `RIGHT` can be flipped
to a `LEFT`). **`FULL OUTER JOIN`** keeps all rows from both sides. **`CROSS JOIN`** has no `ON` and pairs
every combination (Cartesian product). The **join type** decides which unmatched rows survive; the **`ON`
condition** decides what "matches" means — an **equi-join** uses equality (`b.AuthorId = a.AuthorId`, the
common FK=PK case), a **theta-join** uses a non-equality comparison (`<`, `>`, `BETWEEN`).

A **subquery** is a `SELECT` nested in another statement. Use a **join** when you need columns from both
tables; use a **subquery** to test membership/existence or to compare against a computed aggregate
(`WHERE Price > (SELECT AVG(Price) FROM Book)`). **`UNION`** stacks two same-shape result sets and dedupes;
`UNION ALL` keeps duplicates (faster); `INTERSECT`/`EXCEPT` complete the family — **awareness level**.

**Key points / pitfalls**
- **`LEFT JOIN` + a `WHERE` on the right table = a silent `INNER JOIN`** — it discards the NULL rows the
  outer join kept. Put right-table conditions in the **`ON`** clause to preserve outer rows. (Top join
  interview trap.)
- Forgetting the `ON` (or a wrong condition) explodes into a cross join — far too many rows.
- Qualify shared column names with a table alias (`b.AuthorId`) or it errors as ambiguous.
- `WHERE x = (SELECT ...)` errors if the subquery returns multiple rows — use `IN`.

**Worked example** *(cited: `weeklytechrepo/SQL/demo/sql-training/queries/04-joins-functions.sql`, `content/4-Thursday/joins.md`)*
```sql
-- the capstone report: every active loan with member and book, three tables joined
SELECT  m.FirstName + ' ' + m.LastName AS Member,
        b.Title                        AS Book,
        l.LoanDate, l.DueDate
FROM    dbo.Loan   AS l
INNER JOIN dbo.Member AS m ON l.MemberId = m.MemberId    -- equi-join (FK = PK)
INNER JOIN dbo.Book   AS b ON l.BookId   = b.BookId
WHERE   l.ReturnDate IS NULL
ORDER BY l.DueDate;
```

---

## 9. Transactions & ACID

**Objectives covered**
- *(Must)* Describe the purpose of transactions in a database and when they are used.
- *(Must)* Understand database consistency and utilize transactions to ensure data consistency in a set of SQL commands.
- *(Must)* Explain the ACID properties (Atomicity, Consistency, Isolation, Durability) and their importance in transaction management.
- *(Should)* Identify and compare different isolation levels (Read Uncommitted, Read Committed, Repeatable Read, Serializable) and their trade-offs.

**Concept recap** *(source: `weeklytechrepo/SQL/content/5-Friday/transactions.md`)*
A **transaction** groups statements into a **single logical unit of work** that either **completes
entirely (`COMMIT`)** or **undoes entirely (`ROLLBACK`)** — no partial state. Use one whenever several
writes must all agree (the classic money transfer; a Library checkout = decrement copies *and* insert the
loan). The robust T-SQL shape wraps `BEGIN TRANSACTION ... COMMIT` in `TRY/CATCH`, rolling back on any
error. **ACID**: **Atomicity** (all steps or none), **Consistency** (the DB moves from one *constraint-
valid* state to another — a write that would violate a PK/FK/CHECK cannot commit), **Isolation**
(concurrent transactions do not corrupt each other), **Durability** (once committed, it survives a crash).

Concurrency causes three read anomalies — **dirty read** (read uncommitted data), **non-repeatable read**
(a row's value changes between two reads), **phantom read** (new rows appear). **Isolation levels** trade
correctness for concurrency:

| Level | Dirty | Non-repeatable | Phantom | Trade-off |
|---|---|---|---|---|
| Read Uncommitted | possible | possible | possible | fastest, least safe |
| **Read Committed** (default) | prevented | possible | possible | sensible default |
| Repeatable Read | prevented | prevented | possible | stable re-reads, more locking |
| Serializable | prevented | prevented | prevented | safest, least concurrency |

**Key points / pitfalls**
- Commit **once**, at the end, after all steps succeed — committing mid-change means a later failure can't
  be undone.
- Always have a rollback path (`TRY/CATCH`); a failure with no handling can leave an open transaction
  holding locks.
- **Consistency ≠ Isolation**: Consistency = constraints stay valid; Isolation = concurrent transactions
  don't interfere. Different letters, different guarantees. (Common mix-up.)
- Higher isolation is not always better — Serializable is safest but slowest (most locking, deadlock risk).
- Durability is **post-commit only** — work before `COMMIT` is not guaranteed to survive a crash.

**Worked example** *(cited: `weeklytechrepo/SQL/demo/sql-training/LibraryDB.sql`, `content/5-Friday/transactions.md`)*
```sql
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE dbo.Book SET AvailableCopies = AvailableCopies - 1 WHERE BookId = 1;  -- CK blocks if 0
        INSERT INTO dbo.Loan (BookId, MemberId, DueDate) VALUES (1, 2, '2026-07-15');
    COMMIT TRANSACTION;            -- both steps succeeded -> permanent together
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;          -- any step failed -> undo everything
    THROW;                         -- re-raise so the caller knows
END CATCH;
```
If `AvailableCopies` is already 0, the `CHECK` rejects step 1, `CATCH` fires, the loan is never recorded —
the data is never left half-changed.

---

## 10. Views, Procedures, Functions, Triggers, Indexes & DCL

**Objectives covered**
- *(Must)* Be able to create a User Defined Function
- *(Must)* Be able to create and call a Stored Procedure
- *(Must)* Create and use views to store the results of a SQL query
- *(Should)* Configure triggers to execute the corresponding stored procedures when certain events occur
- *(Should)* Describe database indexing and its benefits
- *(Should)* Describe triggers and their use in automating tasks.

**Concept recap** *(source: `weeklytechrepo/SQL/content/5-Friday/views-indexes.md`, `.../procedures-triggers-dcl.md`)*
A **view** is a stored `SELECT` you query like a table — it stores the **query, not the data** (every
select re-runs against live tables). Views give **simplicity** (package a gnarly join once), **security**
(grant access to a view exposing only some columns while base tables stay locked), and **consistency**
(everyone computes "active loan" the same way). A **stored procedure** is named, callable (`EXEC`) logic
that can take parameters and **change data** — reuse, security (grant `EXECUTE` without table access),
cached plans, encapsulation. A **user-defined function (UDF)** **returns a value** (scalar or table) for
use **inside a query** and has **no side effects**. The line: *functions compute and return; procedures
act and can modify data.* A **trigger** is a special procedure the engine runs **automatically** on an
`INSERT`/`UPDATE`/`DELETE` — used for audit logging or automation; inside it the virtual tables
**`inserted`** (new rows) and **`deleted`** (old rows) hold what changed. An **index** is a sorted **B-tree**
that makes reads fast by avoiding a full table scan, at the cost of **storage and slower writes** — a
**clustered** index is the table's physical sort order (**one** per table, usually the PK), **nonclustered**
indexes are separate pointer structures (**many** allowed, great for foreign keys). **DCL** (`GRANT` /
`REVOKE`) manages permissions — the strong pattern is to expose views + procedures and lock the base
tables.

**Key points / pitfalls**
- "A view stores data" is wrong — it stores the **query** and reads live tables each time.
- "Indexes are free speed" is wrong — they cost storage and **slow every write**; index selectively (the
  columns you filter/join/sort on). Only **one** clustered index per table.
- An index can't help `LIKE '%x%'` (a leading wildcard can't use the sorted B-tree); only prefix searches
  (`LIKE 'x%'`) benefit.
- Procedure vs function: a function **cannot** `INSERT` (no side effects); that's a procedure's job. Call a
  proc with `EXEC`; use a function inside a `SELECT`.
- Triggers run **invisibly** on every write — powerful but easy to forget; prefer constraints for rules,
  triggers for genuine automation/audit. `inserted`/`deleted` can hold **multiple** rows — write set-based
  trigger code.

**Worked example** *(cited: `weeklytechrepo/SQL/demo/sql-training/LibraryDB.sql`)*
```sql
-- VIEW: a saved query behind a name
CREATE OR ALTER VIEW dbo.vw_ActiveLoans AS
    SELECT l.LoanId, m.FirstName + ' ' + m.LastName AS Member, b.Title, l.DueDate
    FROM dbo.Loan l
    JOIN dbo.Member m ON m.MemberId = l.MemberId
    JOIN dbo.Book   b ON b.BookId   = l.BookId
    WHERE l.ReturnDate IS NULL;
GO
-- STORED PROCEDURE: callable action wrapping a transaction
CREATE OR ALTER PROCEDURE dbo.usp_CheckoutBook @BookId INT, @MemberId INT, @Days INT = 14
AS BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
            IF (SELECT AvailableCopies FROM dbo.Book WHERE BookId = @BookId) <= 0
                THROW 50001, 'No copies available to check out.', 1;
            INSERT INTO dbo.Loan (BookId, MemberId, DueDate)
                VALUES (@BookId, @MemberId, DATEADD(DAY, @Days, GETDATE()));
            UPDATE dbo.Book SET AvailableCopies = AvailableCopies - 1 WHERE BookId = @BookId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION; THROW; END CATCH
END;
GO
EXEC dbo.usp_CheckoutBook @BookId = 1, @MemberId = 2;     -- call it
-- INDEX the foreign keys the reports join on; DCL grants the safe surface only
CREATE INDEX IX_Loan_MemberId ON dbo.Loan (MemberId);
GRANT SELECT ON dbo.vw_ActiveLoans TO LibraryReadOnly;   -- read the report, not the tables
```

A schema has become an application: actions are procedures, computed values are functions, automation is
triggers, reads are indexed, and access is controlled with DCL.

---

## 11. Window Functions & CTEs

> **Awareness level — note-only, not live-demoed.** Window functions and CTEs are covered at **awareness
> level** in the Week-3 content notes (`weeklytechrepo/SQL/content/4-Thursday/functions.md` and `joins.md`)
> but are **not** hand-coded in the live `sql-training` demo thread. The syntax below is grounded in those
> notes, the rubric's Example column (`qc-criteria/QC-2-SQL.md`), and standard T-SQL, in the Library domain.
> Neither topic is scheduled in a later week, so studying them now spoils nothing.

**Objectives covered**
- *(Nice)* Understand and know how to utilize Window Functions
- *(Nice)* Utilize Common Table Expressions (CTEs)

**Concept recap** *(source: `weeklytechrepo/SQL/content/4-Thursday/functions.md` (window functions), `weeklytechrepo/SQL/content/4-Thursday/joins.md` (CTEs))*
A **window function** computes a value across a set of rows **related to the current row** — the "window" —
**without collapsing them into one row** the way `GROUP BY` does. That is the whole point: a `GROUP BY`
query returns one row per group, but a window function keeps **every** detail row and attaches the computed
value alongside it. You define the window with **`OVER (PARTITION BY ... ORDER BY ...)`**: `PARTITION BY`
splits rows into groups (like `GROUP BY` but non-collapsing), and `ORDER BY` orders rows *within* each
partition (required for ranking and running totals). The common functions:

| Function | Returns |
|---|---|
| `ROW_NUMBER()` | a unique sequential number per row in the partition (1,2,3,4...) |
| `RANK()` | rank with **gaps** after ties (1,2,2,4...) |
| `DENSE_RANK()` | rank with **no gaps** after ties (1,2,2,3...) |
| `SUM()/COUNT()/AVG() OVER (...)` | a running or partition-wide aggregate kept per row |

A **Common Table Expression (CTE)** is a **named, temporary result set** defined with **`WITH name AS
(...)`** that exists only for the single statement that follows it. It is the rubric's example shape:
`WITH CTE AS (SELECT ...) SELECT * FROM CTE;`. CTEs make a complex query readable by naming an intermediate
step instead of nesting a subquery — and unlike a view they are not stored, they live for one query. A CTE
can also be **recursive** (a query that references itself, for hierarchies like an org chart) — awareness
level.

**Key points / pitfalls**
- A window function does **not** reduce row count; `GROUP BY` does. "Rank each book within its category but
  still show every book" is a window function, not a group-by.
- `OVER (...)` is what makes a function a window function — `COUNT(*)` is an aggregate, `COUNT(*) OVER
  (PARTITION BY CategoryId)` is a window function that keeps every row.
- `ROW_NUMBER` always breaks ties arbitrarily by its `ORDER BY`; `RANK`/`DENSE_RANK` give tied rows the same
  rank (gaps vs no gaps is the difference).
- You cannot use a window function in a `WHERE` (it is computed in the `SELECT` stage, after `WHERE`) — wrap
  it in a CTE or subquery, then filter the outer query. This is the classic "top-N-per-group" pattern.
- A CTE is scoped to the **one** statement right after it; reuse across statements needs a view, not a CTE.

**Worked example — window function** *(cited: `weeklytechrepo/SQL/content/4-Thursday/functions.md`)*
```sql
-- Rank books by copies owned WITHIN each category, keeping every book row.
SELECT  Title,
        CategoryId,
        TotalCopies,
        ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY TotalCopies DESC) AS RankInCategory
FROM    dbo.Book;
-- GROUP BY would collapse to one row per category; the window keeps all books and labels each.
```

**Worked example — CTE** *(cited: `weeklytechrepo/SQL/content/4-Thursday/joins.md`)*
```sql
-- Factor a filter into a named step, then query it -- reads top-down instead of nested.
WITH RecentBooks AS (
    SELECT BookId, Title, PublishedYear
    FROM   dbo.Book
    WHERE  PublishedYear >= 2000
)
SELECT Title, PublishedYear
FROM   RecentBooks
ORDER  BY PublishedYear DESC;

-- Combined: a CTE makes the "top book per category" pattern (which a WHERE can't do directly) readable.
WITH Ranked AS (
    SELECT Title, CategoryId,
           ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY TotalCopies DESC) AS rn
    FROM   dbo.Book
)
SELECT Title, CategoryId FROM Ranked WHERE rn = 1;   -- one top book per category
```
