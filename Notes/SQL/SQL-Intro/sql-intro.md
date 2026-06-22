# SQL & the Relational Model

## Learning Objectives

- Explain what a database is, and why a **relational** database is the default choice for structured business data.
- Define the core schema vocabulary: database, table, row, column, field, schema, RDBMS.
- Distinguish **persistent** storage from **ephemeral** in-memory state, and explain what "consistency" buys you.
- Name the five SQL **sublanguages** (DDL, DML, DCL, TCL, DQL) and say what each one is for.
- Map the four **CRUD** operations onto SQL keywords.

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

Every app you have written so far in this cohort kept its data in memory — a `List<LibraryItem>`, a `Dictionary<int, Book>`. Close the program and the data is gone. Real systems cannot work that way: a library's catalog, a bank's ledger, an e-commerce cart all have to survive a restart, be read by many users at once, and never silently corrupt. That durable, shared, rule-enforcing store is a **database**, and the language you talk to it with is **SQL**.

This is the opening move of Week 3's epic: **model → create → populate/query → relate/normalize → join/aggregate → protect & package.** Today is "model" — the mental model of what a relational database *is* — before tomorrow we start creating real tables on the `sql-training` thread. Get the vocabulary right now and every later day (DDL, joins, transactions) is just adding verbs to nouns you already understand. This is also the densest QC-2 exam territory of your training: interviewers open with "why a relational database?" and "what are the SQL sublanguages?" almost every time.

## The Concept

### What is a database?

A **database** is an organized, persistent collection of data managed by software whose whole job is to store it safely and hand it back fast. The software is a **DBMS** (Database Management System). When that software organizes data into **tables with rows and columns** and lets tables relate to one another, it is a **Relational** DBMS — an **RDBMS**. SQL Server (what we use this week), PostgreSQL, MySQL, and Oracle are all RDBMSs.

The relational model, proposed by E. F. Codd in 1970, is built on one simple, powerful idea: store data as **relations** (tables), and let the *data itself* — not pointers or file offsets — express relationships. A `Loan` row does not hold a memory address of a `Book`; it holds a `BookId` **value** that matches a `Book` row's key. Relationships are values, and that is why they survive restarts, copies, and backups.

### Persistent vs ephemeral, and why consistency matters

| | Ephemeral (in-memory) | Persistent (database) |
|---|---|---|
| Survives restart? | No | **Yes** — written to disk |
| Shared by many users? | One process | **Many** concurrent clients |
| Rules enforced? | Only what your code remembers | **The engine enforces** constraints |
| Lost on crash? | Everything | **Committed data is durable** |

The third row is the quiet superpower. A relational database does not just *hold* data — it *guards* it. You declare rules once (a member's email must be unique; a book can't have negative copies; a loan must point at a real book) and the engine refuses every write that breaks them, no matter which app or which developer sent it. That property is **consistency**: the data is always in a valid state because invalid states are impossible to write. Your C# kata had to *hope* every code path validated input; a database makes the rule a wall the bad data bounces off.

### Why use the relational model? (the QC answer)

When an interviewer asks "why a relational database?", these are the reasons — memorize them:

- **Structured organization.** Data lives in a defined shape (tables, typed columns), so it is predictable to query and reason about.
- **Data integrity.** Constraints (primary keys, foreign keys, checks) and types are enforced by the engine, so the data stays valid.
- **Powerful querying.** SQL expresses rich questions — filter, sort, group, join across tables — declaratively: you say *what* you want, the engine figures out *how*.
- **Reduced redundancy.** Through **normalization** (Wednesday's topic), each fact is stored once, which prevents update anomalies and saves space.
- **Concurrency & durability.** Many users read and write at once safely, and committed data survives failures (Friday's transactions/ACID topic).

> Relational is the default, not the only option. Document stores (MongoDB), key-value stores (Redis), and graph databases exist for shapes that don't fit tables well. They are out of scope this week — just know "relational" is a deliberate choice, not the only one.

### The vocabulary (get these exact)

```
DATABASE  ── a named container of related tables (e.g. LibraryDB)
  └─ TABLE      ── one entity type, a grid of rows and columns (e.g. Book)
       ├─ COLUMN  ── one attribute, with a fixed data type (e.g. Title VARCHAR(200))
       ├─ ROW     ── one record / one instance of the entity (one specific book)
       └─ FIELD   ── the single cell where a row meets a column
```

- A **schema** has two meanings you will hear both of: (1) the *structure* of your database — the tables, columns, types, and relationships, the "blueprint"; and (2) in SQL Server, a **namespace** that groups objects, written as a prefix like `dbo.Book` (`dbo` = "database owner", the default schema). Context tells you which.
- A **key** is a column (or set of columns) used to identify rows and to link tables. The **primary key** uniquely identifies each row; a **foreign key** points at another table's primary key. Keys get a full day Wednesday — for now just know they are how tables connect.

### The five SQL sublanguages

SQL is one language, but its keywords group into five **sublanguages** by *what they do*. This split is a guaranteed QC-2 question. The week is literally organized around them:

| Sublanguage | Stands for | Purpose | Keywords | When this week |
|---|---|---|---|---|
| **DDL** | Data **Definition** Language | Define/shape the structure | `CREATE`, `ALTER`, `DROP`, `TRUNCATE` | Mon (`ddl.md`) |
| **DML** | Data **Manipulation** Language | Change the data in tables | `INSERT`, `UPDATE`, `DELETE` | Tue (`dml.md`) |
| **DQL** | Data **Query** Language | Read/query the data | `SELECT` | Tue (`dql.md`) |
| **TCL** | **Transaction** Control Language | Group changes into all-or-nothing units | `BEGIN`, `COMMIT`, `ROLLBACK` | Fri (`transactions.md`) |
| **DCL** | Data **Control** Language | Manage permissions/access | `GRANT`, `REVOKE` | Fri (`procedures-triggers-dcl.md`) |

> Some texts fold DQL's `SELECT` into DML and call it four sublanguages. Both framings are "correct" — if asked, name all five but mention that `SELECT` is sometimes grouped under DML. Knowing the nuance signals you actually understand it.

### CRUD ↔ SQL

**CRUD** is the four things every data app does. Each maps to a SQL keyword:

| CRUD | SQL | Example intent |
|---|---|---|
| **Create** | `INSERT` | Add a new member |
| **Read** | `SELECT` | List all overdue loans |
| **Update** | `UPDATE` | Change a book's available copies |
| **Delete** | `DELETE` | Remove a cancelled loan |

Your Week 1/2 kata had add / list / update / remove menu commands — that was CRUD in memory. This week is the same four verbs against a real, persistent, multi-user store.

## Code Example

A first taste — the shape of each sublanguage, all against the **Library** domain we build live this week. Do not worry about the precise syntax yet; each gets its own day. Read it as "this is what each sublanguage *looks* like":

```sql
-- DDL: define the structure (Monday)
CREATE TABLE dbo.Member
(
    MemberId  INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    Email     VARCHAR(120) NOT NULL UNIQUE
);

-- DML: put data in (Tuesday)
INSERT INTO dbo.Member (FirstName, Email)
VALUES ('Ada', 'ada@example.com');

-- DQL: read it back (Tuesday)
SELECT MemberId, FirstName, Email FROM dbo.Member;
```

That last `SELECT` returns:

| MemberId | FirstName | Email |
|---|---|---|
| 1 | Ada | ada@example.com |

One table defined, one row created, one row read — Create and Read of CRUD, two sublanguages, the whole loop in miniature.

## Common Mistakes / Interview Traps

- **"SQL is a database."** No — SQL is the *language*; the database (SQL Server, PostgreSQL) is the *system* you speak it to. Mixing these up reads as inexperience.
- **Confusing the two meanings of "schema."** Be ready to say which you mean: the overall structure (blueprint) or the SQL Server namespace prefix (`dbo.`).
- **Forgetting DQL/TCL/DCL.** Most people remember DDL and DML and stall. Practice listing all five out loud.
- **Saying relational "has no redundancy."** It *reduces* redundancy via normalization; it does not magically eliminate all of it. Precision matters.
- **Treating a primary key as "just an ID column."** It is the enforced, unique, not-null *identity* of a row — the thing every relationship leans on. (Full treatment Wednesday.)

## Decision Guide: relational vs in-memory

| Use a relational database when… | An in-memory structure is fine when… |
|---|---|
| Data must survive a restart | Data is throwaway / recomputable |
| Many users/processes share it | Single process owns it |
| Rules must be enforced for *all* writers | Only your code touches it |
| You need rich ad-hoc queries across entities | Access is one simple lookup pattern |

Your kata was the right call for a console toy. The library, bank, or inventory system it grows into is squarely the left column — which is why this week exists.

## Summary

- A **database** is persistent, shared, rule-enforcing storage managed by a **DBMS**; a **relational** one (**RDBMS**) organizes data as **tables of rows and columns** and expresses relationships as **values** (keys).
- Use relational databases for **structured organization, data integrity, powerful querying, reduced redundancy, and safe concurrency/durability** — the five "why relational" points.
- **Persistent** beats **ephemeral** because it survives restarts, serves many users, and enforces **consistency** (invalid states can't be written).
- The five **sublanguages**: **DDL** (define), **DML** (change), **DQL** (read), **TCL** (transactions), **DCL** (permissions).
- **CRUD** = `INSERT` / `SELECT` / `UPDATE` / `DELETE` — the four verbs of every data app.

## Additional Resources

- [Query and modify data with Transact-SQL — Microsoft Learn (beginner learning path)](https://learn.microsoft.com/en-us/training/paths/get-started-querying-with-transact-sql/) — start here if you're new.
- [SQL Introduction — W3Schools (short, interactive)](https://www.w3schools.com/sql/sql_intro.asp)
- [The relational model (overview/history) — Wikipedia](https://en.wikipedia.org/wiki/Relational_model)
