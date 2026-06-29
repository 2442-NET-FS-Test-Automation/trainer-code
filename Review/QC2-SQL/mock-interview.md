# QC-2 (SQL) — Mock Interview Bank

Practice out loud, then check against the model answer. Each entry carries a tier badge
(`[Must]` / `[Should]` / `[Nice]`), a concise model answer, the QC objective it proves (verbatim), and a
source. Grouped by topic cluster. Every answer traces to a Week-3 source file under
`weeklytechrepo/SQL/**`.

## Relational model & sublanguages

**[Must] Why would you use a relational database to represent data?**
Model: Five reasons: **structured organization** (data lives in typed tables, predictable to query),
**data integrity** (the engine enforces constraints and types, so data stays valid), **powerful querying**
(SQL declaratively filters/sorts/groups/joins), **reduced redundancy** (normalization stores each fact
once), and **safe concurrency and durability** (many users at once, committed data survives crashes). The
core idea is that relationships are stored as **values** (a foreign key equal to a primary key), not
pointers, so they survive restarts and backups.
Proves QC: Articulate the reasons for using a Relational Database Model to represent data.
Source: `weeklytechrepo/SQL/content/1-Monday/sql-intro.md`

---

**[Must] What are the SQL sublanguages and what is each for?**
Model: Five. **DDL** (Data Definition — `CREATE`/`ALTER`/`DROP`/`TRUNCATE`) shapes structure; **DML** (Data
Manipulation — `INSERT`/`UPDATE`/`DELETE`) changes data; **DQL** (Data Query — `SELECT`) reads it; **TCL**
(Transaction Control — `BEGIN`/`COMMIT`/`ROLLBACK`) groups changes into all-or-nothing units; **DCL** (Data
Control — `GRANT`/`REVOKE`) manages permissions. Some texts fold `SELECT` into DML and count four — I'd
name all five and mention that nuance.
Proves QC: Explain the usage of the sublanguages in SQL.
Source: `weeklytechrepo/SQL/content/1-Monday/sql-intro.md`

---

## DDL & constraints

**[Must] Walk me through creating a table. What goes into a CREATE TABLE?**
Model: Columns with a **data type** and **nullability**, an **`IDENTITY(1,1)`** for an auto-incrementing
key, and **constraints** — a `PRIMARY KEY`, any `FOREIGN KEY`s, `UNIQUE`, `CHECK`, `DEFAULT`. I name
constraints (`PK_`, `FK_`, `CK_`) for readable errors, and I write a re-runnable script: `DROP TABLE IF
EXISTS` children before parents, then `CREATE` parents before children so it runs twice with no errors.
Proves QC: Construct SQL statements using DDL (CREATE, DROP, ALTER, TRUNCATE) keywords to generate tables;
Perform basic DDL operations, such as creating, dropping, or truncating tables.
Source: `weeklytechrepo/SQL/content/1-Monday/ddl.md`

---

**[Must] Name the constraints and what each enforces.**
Model: **PRIMARY KEY** — unique and not null, the row's identity, one per table. **FOREIGN KEY** — the
value must match a real parent row (referential integrity). **UNIQUE** — no repeats, many allowed per
table. **NOT NULL** — required. **CHECK** — a custom same-row boolean. **DEFAULT** — the value used when
none is supplied. And **IDENTITY** auto-numbers a column. `NOT NULL` (required) and `DEFAULT` (fallback)
answer different questions and combine.
Proves QC: Describe and utilize constraints in table creation. (Unique, Not Null, Primary Key, Foreign Key,
Auto Incrementing, Default, Check).
Source: `weeklytechrepo/SQL/content/1-Monday/constraints.md`

---

**[Must] What's the difference between DROP, DELETE, and TRUNCATE?**
Model: **DROP** removes the whole **table** — structure and data (DDL). **TRUNCATE** fast-empties **all
rows** but keeps the table, takes no `WHERE`, and **resets IDENTITY** (DDL, minimally logged). **DELETE**
removes the **rows a `WHERE` picks** (or all), is logged per row, and does **not** reset identity (DML). One
line: DROP = the table shouldn't exist; TRUNCATE = empty it fast and reset ids; DELETE = remove these
particular rows.
Proves QC: Describe the difference between DROP, DELETE, and TRUNCATE functionality.
Source: `weeklytechrepo/SQL/content/2-Tuesday/dml.md`

---

**[Should] What do cascades do on a foreign key?**
Model: A foreign key's **referential action** says what happens to children when the parent is deleted or
its key changes. `NO ACTION` (the default) blocks the delete while children exist; `ON DELETE CASCADE`
deletes the children too; `ON DELETE SET NULL` nulls the child's FK; `SET DEFAULT` sets it to its default.
You choose per relationship — cascade Author->Book (lose the books with the author), but block deleting a
Book that still has Loan history. Watch for multiple cascade paths to one table, which SQL Server rejects.
Proves QC: Utilize cascades to define what happens to related tables during DML operations.
Source: `weeklytechrepo/SQL/content/1-Monday/constraints.md`

---

**[Nice] How do you change a table after it already has data?**
Model: `ALTER TABLE`. `ADD` bolts on a column — but a `NOT NULL` add on a populated table needs a
`DEFAULT` so existing rows get a value, or you add it `NULL` and backfill. `ALTER COLUMN` changes a type;
widening (`VARCHAR(200)` to `(250)`) is safe, narrowing can truncate or fail. In a rebuildable teaching
script you usually fold the change into the original `CREATE` to stay idempotent; in a live system you
can't drop the table, so `ALTER` (a migration) is the only way.
Proves QC: Explain how to modify a table structure after creation using ALTER TABLE with examples (e.g.,
adding a column, modifying a data type).
Source: `weeklytechrepo/SQL/content/1-Monday/ddl.md`

---

## DML & DQL

**[Must] How do you insert, update, and delete data safely?**
Model: `INSERT` with an **explicit column list**, omitting `IDENTITY` columns, and seed **parents before
children** so foreign keys hold. `UPDATE ... SET ... WHERE` and `DELETE FROM ... WHERE` are always scoped
by a `WHERE` — without it they hit every row, with no undo outside a transaction. I preview a destructive
write by running the matching `SELECT ... WHERE` first to see exactly which rows I'll affect.
Proves QC: Construct SQL statements using DML(Insert, Update, Delete) keywords to manipulate pre-existing
data within tables.
Source: `weeklytechrepo/SQL/content/2-Tuesday/dml.md`

---

**[Must] How do you filter rows, and what's the trap with NULL?**
Model: `WHERE` with operators — `=`, `<>`, comparisons, `BETWEEN`, `IN`, `LIKE` (`%` any chars, `_` one
char), `IS NULL`, combined with `AND`/`OR`. The trap: NULL means "unknown," so `col = NULL` is **never
true** — you must write `IS NULL` / `IS NOT NULL`. In the Library model a NULL `ReturnDate` means "still
checked out," so active loans are `WHERE ReturnDate IS NULL`.
Proves QC: Demonstrate the ability to filter records using the WHERE clause and operators.
Source: `weeklytechrepo/SQL/content/2-Tuesday/dql.md`

---

**[Should] Why can't WHERE use a column alias you defined in SELECT?**
Model: Because of the **logical clause order**: `FROM -> WHERE -> GROUP BY -> HAVING -> SELECT -> ORDER BY`.
`WHERE` runs *before* `SELECT`, so the alias doesn't exist yet — repeat the expression or filter in a later
stage. `ORDER BY` runs last, so it *can* use a `SELECT` alias. Same reason `HAVING` (post-grouping) can use
an aggregate but `WHERE` can't.
Proves QC: Demonstrate the ability to filter records using the WHERE clause and operators.
Source: `weeklytechrepo/SQL/content/2-Tuesday/dql.md`

---

## Keys, schema & ERD

**[Must] What is the role of a primary key?**
Model: It **uniquely identifies each row** — that's its identity role, answering "which row?" It's enforced
**unique and not null** (an unknown identity is a contradiction), it's the **anchor** every foreign key
points at, and it's backed by an index for fast retrieval. A table has exactly one. I prefer a **surrogate
key** — a meaningless auto-numbered `IDENTITY` — over a natural key like email, because real-world values
change.
Proves QC: Understand the role of a primary key in a data-set.
Source: `weeklytechrepo/SQL/content/3-Wednesday/keys.md`

---

**[Must] What is the role of a foreign key, and what's referential integrity?**
Model: A **foreign key** is a column whose value must match a primary key in another table (or be NULL). It
**encodes a relationship** and **enforces referential integrity** — the property that every FK value points
at a row that actually exists, so there are no orphans. The engine refuses to insert a child pointing at a
missing parent, and refuses to delete a parent a child still references unless a referential action says
otherwise. A table has one PK but can have many FKs.
Proves QC: Understand the role of a foreign key in a data-set; Describe referential integrity.
Source: `weeklytechrepo/SQL/content/3-Wednesday/keys.md`

---

**[Should] What's a candidate key, and how do you pick the primary key?**
Model: A **candidate key** is any minimal column or column-set that could uniquely identify a row — a table
can have several. You pick **one** as the primary key (ideally stable and meaningless — a surrogate id),
and the rest become **alternate keys**, enforced with `UNIQUE`. For a Member, `MemberId` and `Email` are
both candidates; I'd make `MemberId` the PK and keep `Email` as a `UNIQUE` alternate key because emails
change.
Proves QC: Demonstrate how to identify valid candidate keys for a primary key of an entity.
Source: `weeklytechrepo/SQL/content/3-Wednesday/keys.md`

---

**[Should] How do you read an ERD, and what is multiplicity?**
Model: In an ERD each box is an **entity** (a table), each line is a **relationship** (a foreign key), and
keys are marked PK/FK. **Multiplicity** (cardinality) is how many of one side relate to the other: **1:1**
(implemented as a FK plus a `UNIQUE` on it), **1:N** (a FK on the **many** side — one author, many books),
or **M:N** (a **bridge table** with a composite key). The rule: the foreign key lives on the many side, and
many-to-many always needs a third table.
Proves QC: Read and understand ERD (Entity Relationship Diagram); Explain the concept of multiplicity in
database relationships.
Source: `weeklytechrepo/SQL/content/1-Monday/data-modeling-erd.md`, `3-Wednesday/keys.md`

---

**[Should] Turn a requirement into a schema: how do you approach it?**
Model: Nouns become **entities/tables**, properties become **columns**, verbs/links become
**relationships/foreign keys**. I decide the **multiplicity** of each relationship (FK on the many side),
pick a **primary key** per entity, and choose a **data type** per column for integrity and efficiency —
`VARCHAR` for variable text, `CHAR` for fixed codes, `INT`/`BIGINT` for whole numbers, `DECIMAL` for money,
`DATE` for dates. That's how you go from "a library lends books to members" to Book/Member/Loan tables with
the right keys.
Proves QC: Translate real-world problem descriptions into ER diagrams and implement them as a working
relational schema; Create a valid schema for a given data-set; Accurately describe database schemas,
including tables, fields, and the relationships between them.
Source: `weeklytechrepo/SQL/content/1-Monday/data-modeling-erd.md`

---

**[Should] How do you choose a data type? When would you reach for BIGINT or DECIMAL?**
Model: Match the type to the data for integrity and space. `INT` for ordinary ids/counts; **`BIGINT`** when
values could exceed about 2.1 billion (global transaction ids). `VARCHAR(n)` for variable text; `CHAR(n)`
only for genuinely fixed-length codes like an ISBN-13 (it pads with spaces otherwise). **`DECIMAL(p,s)`**
for money or any exact number — **never `FLOAT`**, because binary floating point can't represent values
like 0.10 exactly and sums drift. `DATE` for a calendar day.
Proves QC: Identify and implement common data types such as varchar, decimal, integer, and char; Identify
and implement advanced data types beyond the basics, such as BIGINT for handling very large integers.
Source: `weeklytechrepo/SQL/content/1-Monday/data-modeling-erd.md`

---

**[Nice] Name some key types beyond primary and foreign keys.**
Model: A **candidate key** is any minimal unique set eligible to be the PK; an **alternate key** is a
candidate not chosen (enforced with `UNIQUE`); a **composite key** spans two or more columns — the classic
case is a bridge table's primary key like `(BookId, AuthorId)`; a **secondary key** is a non-unique lookup
column; and a **surrogate** vs **natural** key is a generated id vs a real-world value.
Proves QC: Recognize and explain less common key types beyond primary and foreign keys, such as candidate
keys or composite keys.
Source: `weeklytechrepo/SQL/content/3-Wednesday/keys.md`

---

## Normalization

**[Must] Normalize a table from 0NF to 3NF — walk me through it.**
Model: **1NF** — atomic values, no repeating groups: replace `Author1`/`Author2` columns with one row per
book-author. **2NF** — in 1NF and every non-key column depends on the **whole** key: with a composite key
like (BookId, Author), `Title` depends only on `BookId` (a partial dependency), so split the book facts off
from a `BookAuthor` bridge. **3NF** — in 2NF and no non-key column depends on another non-key column:
`CategoryDescription` depends on `CategoryName` (transitive), so extract a `Category` table. The mnemonic:
every non-key column depends on "the key, the whole key, and nothing but the key." Each step removes a
redundancy that caused insertion/update/deletion anomalies.
Proves QC: Normalize a database schema from unnormalized form (0NF) to Third Normal Form (3NF), providing
step-by-step justification.
Source: `weeklytechrepo/SQL/content/3-Wednesday/normalization.md`

---

**[Should] What are the benefits and drawbacks of normalization?**
Model: Benefits: each fact is stored **once**, so update/insertion/deletion anomalies disappear, storage is
smaller, and integrity is easy to enforce. Drawbacks: data is spread across more tables, so reassembling it
needs **more joins**, and queries get more complex and can be slower. 3NF is the default for transactional
systems; for read-heavy reporting you might **deliberately denormalize** to cut join cost — "normalize
until it hurts, denormalize until it works."
Proves QC: Explain the benefits and potential drawbacks of database normalization, including impacts on
performance and data integrity.
Source: `weeklytechrepo/SQL/content/3-Wednesday/normalization.md`

---

**[Nice] How do you model a many-to-many relationship?**
Model: With a **bridge (junction) table** whose rows are the pairings. You can't use a single foreign key —
that only expresses one-to-many. The bridge has a **composite primary key** made of the two foreign keys,
e.g. `BookAuthor(BookId, AuthorId)` with `PRIMARY KEY (BookId, AuthorId)` where each column is also a FK.
That composite key stops the same pair repeating, and M:N becomes two one-to-many links into the bridge.
Proves QC: Capable of implementing bridge tables to handle many-to-many relationships between entities.
Source: `weeklytechrepo/SQL/content/3-Wednesday/normalization.md`

---

## Functions & grouping

**[Must] Difference between an aggregate and a scalar function?**
Model: An **aggregate** function takes **many rows and returns one** summarizing value — `COUNT`, `SUM`,
`AVG`, `MIN`, `MAX` — usually with `GROUP BY`. A **scalar** function takes **one value and returns one per
row** — `UPPER`, `LEN`, `GETDATE`, `ROUND`, `YEAR`. Aggregates *collapse* a set; scalars *transform* each
row independently. "Is `COUNT` scalar?" — no, it's aggregate.
Proves QC: Understand difference between aggregate and scalar functions.
Source: `weeklytechrepo/SQL/content/4-Thursday/functions.md`

---

**[Must] How does GROUP BY work, and what's the golden rule?**
Model: `GROUP BY` collapses rows that share a value into **one row per group**, and aggregates are computed
within each group. The golden rule: every column in the `SELECT` must be either in the `GROUP BY` or
wrapped in an aggregate — otherwise the engine rejects it, because there'd be many values per group and it
wouldn't know which to show.
Proves QC: Utilize the GROUP BY clause.
Source: `weeklytechrepo/SQL/content/4-Thursday/functions.md`

---

**[Should] WHERE vs HAVING?**
Model: `WHERE` filters individual **rows** *before* grouping and cannot use an aggregate; `HAVING` filters
**groups** *after* aggregation and can. Read it as a pipeline: `WHERE` keeps rows, `GROUP BY` buckets them,
`HAVING` keeps buckets. If a condition doesn't involve an aggregate, use `WHERE` — filtering early is
cheaper than aggregating then discarding.
Proves QC: Utilize the HAVING clause to filter aggregated query results.
Source: `weeklytechrepo/SQL/content/4-Thursday/functions.md`

---

**[Should] Which aggregate functions do you use, and how do they handle NULLs?**
Model: `COUNT`, `SUM`, `AVG`, `MIN`, `MAX`. The NULL trap: `COUNT(*)` counts every row, but `COUNT(col)`
counts only non-NULL values, and `SUM`/`AVG` ignore NULLs — so `AVG` divides by the non-null count, which
can surprise you. `MIN`/`MAX` work on numbers, dates, and text. I alias aggregate columns (`COUNT(*) AS
BookCount`) so the output has a name.
Proves QC: Identify and use commonly used aggregate functions (e.g., COUNT(), SUM(), AVG(), MIN(), MAX()) to
summarize data in queries; Utilize column aliases to enhance readability and clarity of SQL queries.
Source: `weeklytechrepo/SQL/content/4-Thursday/functions.md`

---

## Joins & subqueries

**[Must] Explain the join types and which rows each keeps.**
Model: `INNER JOIN` keeps only rows that match on both sides. `LEFT JOIN` keeps every left row, with NULLs
where the right has no match. `RIGHT JOIN` is the mirror (keeps all of the right). `FULL OUTER JOIN` keeps
all rows from both, NULLs where unmatched. `CROSS JOIN` has no `ON` and pairs every combination. The join
**type** decides which unmatched rows survive; the `ON` **condition** decides what matches — an **equi-
join** uses equality (the usual FK=PK), a **theta-join** uses `<`/`>`/`BETWEEN`.
Proves QC: Understand basic types of joins and demonstrate usage in select statements (inner, left/right
outer, full outer, equi).
Source: `weeklytechrepo/SQL/content/4-Thursday/joins.md`

---

**[Must] There's a classic LEFT JOIN trap — what is it?**
Model: Putting a condition on the **right** table in the `WHERE` clause silently turns a `LEFT JOIN` into an
`INNER JOIN`. The outer join keeps unmatched left rows with NULLs on the right, but `WHERE right.col = x`
throws those NULL rows away. The fix is to put right-table conditions in the **`ON`** clause, which keeps
the outer rows. It's the most common join mistake in interviews.
Proves QC: Understand basic types of joins and demonstrate usage in select statements (inner, left/right
outer, full outer, equi).
Source: `weeklytechrepo/SQL/content/4-Thursday/joins.md`

---

**[Should] When do you use a subquery versus a join?**
Model: Use a **join** when you need **columns from both tables** in the output, or you're combining/
reporting across tables. Use a **subquery** when you only need to **test membership/existence** (`WHERE
AuthorId IN (SELECT ...)`) or to compare each row against a **computed aggregate** (`WHERE Price > (SELECT
AVG(Price) FROM Book)`). They're often interchangeable and the optimizer treats them similarly, but joins
usually read more clearly, and a subquery shines for "rows compared to an aggregate."
Proves QC: Understand when to use subqueries versus joins in SQL logic; Utilize subquery structure to
execute a select statement.
Source: `weeklytechrepo/SQL/content/4-Thursday/joins.md`

---

**[Nice] What does UNION do, and how is it different from UNION ALL?**
Model: `UNION` stacks two result sets that have the same column shape into one and **removes duplicate
rows** (which means it also sorts). `UNION ALL` keeps duplicates and is **faster** because it skips the
dedupe. `INTERSECT` (rows in both) and `EXCEPT` (rows in the first not the second) round out the set
operations. If you know there are no duplicates, prefer `UNION ALL`.
Proves QC: Utilize set operations between multiple select statement.
Source: `weeklytechrepo/SQL/content/4-Thursday/joins.md`

---

## Transactions & ACID

**[Must] What is a transaction and when do you use one?**
Model: A transaction groups statements into a **single logical unit of work** that either **commits
entirely or rolls back entirely** — no partial state. You use one whenever several writes must all agree:
the classic money transfer, or a Library checkout that must decrement available copies **and** insert the
loan together. The safe T-SQL shape wraps `BEGIN TRANSACTION ... COMMIT` in `TRY/CATCH`, rolling back on
any error, and commits once at the end.
Proves QC: Describe the purpose of transactions in a database and when they are used; Understand database
consistency and utilize transactions to ensure data consistency in a set of SQL commands.
Source: `weeklytechrepo/SQL/content/5-Friday/transactions.md`

---

**[Must] Explain the ACID properties.**
Model: **Atomicity** — all steps happen or none do. **Consistency** — the database moves from one
**constraint-valid** state to another; a write that would violate a PK/FK/CHECK can't commit. **Isolation**
— concurrent transactions don't corrupt each other. **Durability** — once committed, the change survives a
crash or power loss. The link back to constraints is Consistency: the rules you declared are what a
transaction must leave intact.
Proves QC: Explain the ACID properties (Atomicity, Consistency, Isolation, Durability) and their importance
in transaction management.
Source: `weeklytechrepo/SQL/content/5-Friday/transactions.md`

---

**[Should] What are the isolation levels and their trade-offs?**
Model: Four, trading correctness for concurrency. **Read Uncommitted** allows dirty, non-repeatable, and
phantom reads — fastest, least safe. **Read Committed** (SQL Server's default) prevents dirty reads.
**Repeatable Read** also prevents non-repeatable reads. **Serializable** prevents all three including
phantoms — safest, as if transactions ran one at a time, but the most locking and least concurrency. Each
step up prevents one more phenomenon at a throughput cost; pick Serializable only when correctness truly
demands it.
Proves QC: Identify and compare different isolation levels (Read Uncommitted, Read Committed, Repeatable
Read, Serializable) and their trade-offs.
Source: `weeklytechrepo/SQL/content/5-Friday/transactions.md`

---

**[Should] People confuse Consistency and Isolation — what's the difference?**
Model: **Consistency** is about constraints: every transaction leaves the database in a valid state, so a
commit that would break a PK/FK/CHECK is refused. **Isolation** is about concurrency: simultaneous
transactions don't interfere or read each other's half-done work. Different letters of ACID, different
guarantees — one is about rules, the other about parallelism.
Proves QC: Explain the ACID properties (Atomicity, Consistency, Isolation, Durability) and their importance
in transaction management.
Source: `weeklytechrepo/SQL/content/5-Friday/transactions.md`

---

## Views, procedures, functions, triggers, indexes & DCL

**[Must] What is a view and why use one?**
Model: A view is a **stored `SELECT` you query like a table** — it stores the **query, not the data**, so
each select re-runs against the live tables. Three payoffs: **simplicity** (package a gnarly multi-table
join behind one name), **security** (grant access to a view that exposes only some columns while base
tables stay locked), and **consistency** (everyone computes "active loans" the same way). You create it
with `CREATE VIEW ... AS SELECT ...`.
Proves QC: Create and use views to store the results of a SQL query.
Source: `weeklytechrepo/SQL/content/5-Friday/views-indexes.md`

---

**[Must] What's a stored procedure, and how do you call it?**
Model: A stored procedure is **named, callable logic** you invoke with `EXEC`, optionally with parameters.
It can run multiple statements and **change data**, so it's used for actions and workflows — like a
`CheckoutBook` proc that wraps a transaction. Benefits: reuse, security (grant `EXECUTE` without giving
direct table access), a cached execution plan, and encapsulation. You call it `EXEC dbo.usp_CheckoutBook
@BookId = 1, @MemberId = 2`.
Proves QC: Be able to create and call a Stored Procedure.
Source: `weeklytechrepo/SQL/content/5-Friday/procedures-triggers-dcl.md`

---

**[Must] What's a user-defined function, and how is it different from a stored procedure?**
Model: A UDF **computes and returns a value** — scalar or table — and is meant to be used **inside a
query**, like `SELECT dbo.fn_DaysOverdue(DueDate) ...`. The key difference: a function has **no side
effects** — it can't `INSERT`/`UPDATE`/`DELETE` — whereas a procedure **acts** and can change data, and is
called with `EXEC` as its own statement. One line: functions compute and return; procedures act.
Proves QC: Be able to create a User Defined Function.
Source: `weeklytechrepo/SQL/content/5-Friday/procedures-triggers-dcl.md`

---

**[Should] What's a trigger and when would you use one?**
Model: A trigger is a special procedure the engine runs **automatically** in response to an `INSERT`,
`UPDATE`, or `DELETE` — you never call it directly. Use it to automate reactions like audit logging or
maintaining a derived value. Inside it the virtual tables **`inserted`** (new rows) and **`deleted`** (old
rows) hold what changed, and they can hold multiple rows, so write set-based code. They're powerful but
implicit — logic that runs invisibly on every write is easy to forget and can hurt performance, so prefer
constraints for rules and triggers for genuine automation.
Proves QC: Describe triggers and their use in automating tasks; Configure triggers to execute the
corresponding stored procedures when certain events occur.
Source: `weeklytechrepo/SQL/content/5-Friday/procedures-triggers-dcl.md`

---

**[Should] What's an index, and what does it cost?**
Model: An index is a separate sorted **B-tree** that lets the engine jump straight to matching rows instead
of scanning every row — huge on large tables. The trade-off: it costs extra **storage** and **slows every
write**, because each `INSERT`/`UPDATE`/`DELETE` must also update the index. So you index the columns you
**filter, join, or sort on** a lot — foreign keys are prime candidates — not everything. A **clustered**
index is the table's physical sort order (one per table, usually the PK); **nonclustered** indexes are
separate pointer structures (many allowed).
Proves QC: Describe database indexing and its benefits.
Source: `weeklytechrepo/SQL/content/5-Friday/views-indexes.md`

---

**[Should] How does DCL fit in, and what's the secure pattern?**
Model: **DCL** — `GRANT` and `REVOKE` (and `DENY`) — manages **permissions**: who may read, run, or change
what. The strong pattern is to lock down the base tables and `GRANT` access only to **views** (controlled
reads) and **stored procedures** (controlled writes), so users touch a safe surface and never the raw
tables — security by encapsulation. It's also the fifth sublanguage, so it belongs in any "name the
sublanguages" answer.
Proves QC: Explain the usage of the sublanguages in SQL.
Source: `weeklytechrepo/SQL/content/5-Friday/procedures-triggers-dcl.md`

---

## Window functions & CTEs

> Awareness level — covered in the content notes (`content/4-Thursday/functions.md` / `joins.md`), not
> live-demoed. Grounded in those notes, the `qc-criteria/QC-2-SQL.md` Example column, and standard T-SQL.

**[Nice] What is a window function, and how is it different from GROUP BY?**
Model: A **window function** computes a value across a set of rows related to the current row — the
"window" — **without collapsing them**. That's the key contrast: `GROUP BY` returns **one row per group**,
but a window function **keeps every detail row** and attaches the computed value alongside it. You define
the window with **`OVER (PARTITION BY ... ORDER BY ...)`** — `PARTITION BY` groups the rows, `ORDER BY`
orders them within each group. So "rank each book within its category but still show every book" is a
window function, not a group-by.
Proves QC: Understand and know how to utilize Window Functions.
Source: `weeklytechrepo/SQL/content/4-Thursday/functions.md`

---

**[Nice] What does OVER(PARTITION BY ...) do, and what's the difference between ROW_NUMBER, RANK, and DENSE_RANK?**
Model: `OVER(...)` is what makes a function a window function. `PARTITION BY` splits rows into groups (like
`GROUP BY` but non-collapsing), and `ORDER BY` orders rows within each partition so ranking is
well-defined. `ROW_NUMBER()` gives a unique sequential number (1,2,3,4) and breaks ties arbitrarily;
`RANK()` gives tied rows the same rank but leaves **gaps** afterward (1,2,2,4); `DENSE_RANK()` gives tied
rows the same rank with **no gaps** (1,2,2,3). A gotcha: you can't filter a window function in `WHERE`
(it's computed after `WHERE`), so for top-N-per-group you wrap it in a CTE or subquery and filter the outer
query.
Proves QC: Understand and know how to utilize Window Functions.
Source: `weeklytechrepo/SQL/content/4-Thursday/functions.md`

---

**[Nice] What is a CTE and when would you use one?**
Model: A **Common Table Expression** is a **named temporary result set** defined with **`WITH name AS
(...)`** that exists only for the single statement right after it — the rubric shape is `WITH CTE AS
(SELECT ...) SELECT * FROM CTE;`. You use one to make a complex query **readable** by naming an
intermediate step instead of nesting a subquery, and they pair naturally with window functions for the
top-N-per-group pattern. Unlike a **view**, a CTE isn't stored and can't be reused across statements — it
lives for one query. CTEs can also be **recursive** (reference themselves) for hierarchies like an org
chart.
Proves QC: Utilize Common Table Expressions (CTEs).
Source: `weeklytechrepo/SQL/content/4-Thursday/joins.md`
