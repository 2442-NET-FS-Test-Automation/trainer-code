# Constraints: Rules the Database Enforces

## Learning Objectives

- Declare the full constraint set: `PRIMARY KEY`, `FOREIGN KEY`, `UNIQUE`, `NOT NULL`, `CHECK`, `DEFAULT`.
- Use **IDENTITY** for auto-incrementing keys.
- Declare a foreign key and control deletes/updates with **`ON DELETE` / `ON UPDATE` CASCADE** (and the alternatives).
- Explain the difference between **PRIMARY KEY** and **UNIQUE**, and between **NOT NULL** and **DEFAULT**.
- Read constraint **violation errors** and know which rule fired.

> **Scope split:** This note is about *declaring* constraints in DDL — the syntax and behavior. The *theory* of keys (candidate keys, composite keys, referential integrity as a concept, multiplicity) gets its own day Wednesday in `keys.md`. Here: how to write the rules. There: why keys work the way they do.

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

A constraint is the database keeping a promise your application code can't be trusted to keep alone. Your C# kata had to *hope* every code path validated input before saving; a constraint makes the rule a wall the bad data bounces off — enforced for *every* writer, every app, forever. That is the "data integrity" reason the relational model exists, made concrete.

This is the back half of Monday's `01-ddl` commit: tables without constraints are just spreadsheets; tables *with* them are a database that protects itself. QC-2 asks you to "describe and utilize constraints in table creation" and lists the exact set below — this note is that checklist, with the gotchas interviewers love.

## The Concept

### The constraint set at a glance

| Constraint | Promise it enforces | Library example |
|---|---|---|
| `PRIMARY KEY` | unique **and** not null — the row's identity | `BookId` identifies each book |
| `FOREIGN KEY` | the value must match a real row in another table | `Book.AuthorId` → a real `Author` |
| `UNIQUE` | no two rows share this value | `Member.Email`, `Book.ISBN` |
| `NOT NULL` | the column is required | `Title NOT NULL` |
| `CHECK` | a custom boolean rule must hold | `AvailableCopies >= 0` |
| `DEFAULT` | value to use when none is supplied | `JoinedDate DEFAULT today` |

### PRIMARY KEY + IDENTITY

```sql
CREATE TABLE dbo.Author
(
    AuthorId  INT IDENTITY(1,1) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    CONSTRAINT PK_Author PRIMARY KEY (AuthorId)
);
```

- A `PRIMARY KEY` is the row's **identity**: automatically **unique** and **not null**. A table has **exactly one**.
- **`IDENTITY(1,1)`** auto-generates the value (start 1, step 1) — you never insert it. It pairs naturally with the PK but is a *separate* feature (auto-numbering ≠ being the key).
- Naming it (`PK_Author`) makes errors readable and lets you reference it later.

### UNIQUE vs PRIMARY KEY

```sql
CREATE TABLE dbo.Member
(
    MemberId INT IDENTITY(1,1) NOT NULL,
    Email    VARCHAR(120) NOT NULL,
    CONSTRAINT PK_Member PRIMARY KEY (MemberId),
    CONSTRAINT UQ_Member_Email UNIQUE (Email)
);
```

Both forbid duplicates. The differences:

| | PRIMARY KEY | UNIQUE |
|---|---|---|
| How many per table | exactly **one** | **many** allowed |
| Allows NULL? | **no** | yes (typically one NULL in SQL Server) |
| Meaning | "this is *the* identifier" | "this value can't repeat" |

So `MemberId` is the identity (PK), while `Email` is *also* one-of-a-kind (UNIQUE) but not the chosen identifier. Marking `Email` both `NOT NULL` and `UNIQUE` makes it required *and* unique.

### NOT NULL vs DEFAULT

These are often confused but answer different questions:

- **`NOT NULL`** — "is a value **required**?"
- **`DEFAULT`** — "what value if **none is supplied**?"

```sql
JoinedDate DATE NOT NULL CONSTRAINT DF_Member_JoinedDate DEFAULT (GETDATE())
```

This column is required *and* auto-fills today's date when an insert omits it. A column can have either, both, or neither. `GETDATE()` is a built-in returning the current date/time.

### CHECK: your own rules

A `CHECK` is a boolean expression the engine evaluates on every write; `false` → the write is rejected.

```sql
CONSTRAINT CK_Book_Copies CHECK (TotalCopies >= 0 AND AvailableCopies >= 0
                                 AND AvailableCopies <= TotalCopies)
```

One constraint encodes three truths: copies can't be negative, and you can't have more available than you own. The database will now *refuse* a checkout that would drop available below zero — a rule you'd otherwise have to enforce (and re-enforce) in every code path. A `CHECK` can span several columns of the **same row** (as here) but cannot look at other rows or other tables.

### FOREIGN KEY and CASCADE

A foreign key is the guard rail between two tables — it guarantees a pointer is never dangling.

```sql
CREATE TABLE dbo.Book
(
    BookId   INT IDENTITY(1,1) NOT NULL,
    Title    VARCHAR(200) NOT NULL,
    AuthorId INT NOT NULL,
    CONSTRAINT PK_Book PRIMARY KEY (BookId),
    CONSTRAINT FK_Book_Author FOREIGN KEY (AuthorId)
        REFERENCES dbo.Author (AuthorId) ON DELETE CASCADE
);
```

- **`FOREIGN KEY (AuthorId) REFERENCES dbo.Author (AuthorId)`** — every `Book.AuthorId` **must** match a real `Author.AuthorId`. Insert a book with `AuthorId = 999` (no such author) and the engine rejects it. That guarantee is **referential integrity** (Wednesday's word).
- **`ON DELETE CASCADE`** answers "what happens to the children when the parent is deleted?" Here: delete an author and their books go too — the delete **flows down** the relationship.

The referential actions you can choose per FK:

| Action | On deleting the parent… |
|---|---|
| `NO ACTION` (default) | **block** the delete while children exist |
| `ON DELETE CASCADE` | delete the children too |
| `ON DELETE SET NULL` | null out the child's FK (FK column must allow NULL) |
| `ON DELETE SET DEFAULT` | set the child's FK to its DEFAULT |

`ON UPDATE` has the same options for when the parent **key value** changes. Cascade is powerful and a little dangerous — a single delete can remove a lot. You pick the action **per relationship**: cascade `Author`→`Book` (lose the books with the author), but **block** deleting a `Book` that still has `Loan` history (you don't want to silently erase the record).

### Two ways to write a constraint

- **Inline (column-level):** right after the column — `Email VARCHAR(120) NOT NULL UNIQUE`. Compact, good for single-column rules.
- **Table-level (named):** listed after the columns with `CONSTRAINT name ...`. Required for multi-column constraints (a composite PK, a cross-column CHECK) and preferred everywhere because the **name** gives readable errors and lets you `ALTER` it later.

Prefer named table-level constraints in real schemas; the demo does.

## Code Example

The constraint set working together on one table (the rich `Book` from the demo):

```sql
CREATE TABLE dbo.Book
(
    BookId          INT          IDENTITY(1,1) NOT NULL,           -- IDENTITY
    Title           VARCHAR(200) NOT NULL,                         -- NOT NULL
    ISBN            CHAR(13)     NOT NULL,
    PublishedYear   INT          NULL,                             -- optional
    AuthorId        INT          NOT NULL,
    TotalCopies     INT          NOT NULL DEFAULT (1),             -- DEFAULT
    AvailableCopies INT          NOT NULL DEFAULT (1),
    CONSTRAINT PK_Book PRIMARY KEY (BookId),                       -- PRIMARY KEY
    CONSTRAINT UQ_Book_ISBN UNIQUE (ISBN),                         -- UNIQUE
    CONSTRAINT CK_Book_Copies CHECK (AvailableCopies <= TotalCopies), -- CHECK
    CONSTRAINT FK_Book_Author FOREIGN KEY (AuthorId)               -- FOREIGN KEY
        REFERENCES dbo.Author (AuthorId) ON DELETE CASCADE         -- CASCADE
);
```

Now watch the rules fire (each write the engine **rejects**):

```sql
INSERT INTO dbo.Book (Title, ISBN, AuthorId) VALUES ('X', '978...', 999);
-- REJECTED: FK_Book_Author — no Author 999 (referential integrity)

INSERT INTO dbo.Book (Title, ISBN, AuthorId, TotalCopies, AvailableCopies)
VALUES ('Y', '978...', 1, 2, 5);
-- REJECTED: CK_Book_Copies — 5 available but only 2 owned

INSERT INTO dbo.Book (Title, ISBN, AuthorId) VALUES ('Z', NULL, 1);
-- REJECTED: NOT NULL — ISBN is required
```

Three bad rows the database stopped *for* you — that's integrity you didn't have to code.

## Common Mistakes / Interview Traps

- **"PRIMARY KEY and UNIQUE are the same."** No: one PK per table (never null); many UNIQUE columns (may allow a null). PK = identity; UNIQUE = no repeats.
- **Confusing `NOT NULL` with `DEFAULT`.** `NOT NULL` = required; `DEFAULT` = fallback when omitted. They combine; neither implies the other.
- **Forgetting a `DEFAULT` (or NULL) when cascading isn't wanted.** `ON DELETE SET NULL` needs the FK column to allow NULL — else it errors.
- **Cascade surprises.** `ON DELETE CASCADE` can wipe a lot from one delete; and **multiple cascade paths** to the same table cause SQL Server to refuse the FK ("may cause cycles or multiple cascade paths"). Choose actions deliberately.
- **Assuming a `CHECK` can reference another table.** It can't — only columns of the same row. Cross-table rules need a foreign key or a trigger (Friday).
- **Unnamed constraints.** The engine auto-names them cryptically; errors and later `ALTER`s become painful. Name them.

## Decision Guide: which referential action?

| Relationship intent | Action | Example |
|---|---|---|
| Children are meaningless without the parent | `ON DELETE CASCADE` | delete an `Author` → delete their `Book`s |
| The link is optional; keep the child, forget the parent | `ON DELETE SET NULL` | delete a `Category` → books become uncategorized |
| The history must never be silently erased | `NO ACTION` (block) | can't delete a `Book` that has `Loan`s |

## Summary

- Constraints make rules the **engine** enforces for every writer: `PRIMARY KEY`, `FOREIGN KEY`, `UNIQUE`, `NOT NULL`, `CHECK`, `DEFAULT`.
- **PRIMARY KEY** = unique + not null + one-per-table (the identity); **UNIQUE** = no repeats, many allowed. **IDENTITY** auto-numbers and is separate from being the key.
- **NOT NULL** (required) and **DEFAULT** (fallback) answer different questions and combine.
- **CHECK** enforces custom same-row rules; **FOREIGN KEY** enforces **referential integrity**, with **CASCADE / SET NULL / NO ACTION** chosen per relationship.
- Prefer **named, table-level** constraints for readable errors and future `ALTER`s.

## Additional Resources

- [Unique Constraints and Check Constraints — Microsoft Learn](https://learn.microsoft.com/en-us/sql/relational-databases/tables/unique-constraints-and-check-constraints)
- [Primary and Foreign Key Constraints — Microsoft Learn](https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints)
- [IDENTITY (Property) (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql-identity-property)
