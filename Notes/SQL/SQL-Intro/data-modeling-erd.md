# Data Modeling, ERDs & Data Types

## Learning Objectives

- Read an **Entity-Relationship Diagram (ERD)**: entities, attributes, primary/foreign keys, and relationship lines.
- Describe **multiplicity** (cardinality): one-to-one (1:1), one-to-many (1:N), and many-to-many (M:N).
- Choose appropriate SQL **data types** — `VARCHAR`, `CHAR`, `INT`, `BIGINT`, `DECIMAL`, `DATE` — and explain the trade-offs.
- Translate a **real-world problem description** into an ERD and then into a relational schema.

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

Before you type a single `CREATE TABLE`, you have to decide *what* the tables are, *what* columns they hold, and *how* they relate. Skipping that step is the number-one cause of databases that have to be torn down and rebuilt six months later. Modeling on paper (an ERD) is cheap; migrating a million rows because you picked the wrong shape is not.

This note is the bridge in Week 3's "model → create" opening: the ERD you read here is the exact blueprint the Monday demo turns into the `LibraryDB` schema, and the data-type choices here are the columns you'll declare tomorrow. Interviewers test this directly — "read this ERD," "what's the multiplicity here," "translate this requirement into tables" — because it proves you can design, not just transcribe.

## The Concept

### Entities, attributes, relationships

A **data model** describes the *things* a system tracks and *how they connect*. Three building blocks:

- **Entity** — a thing worth storing, becomes a **table** (e.g. `Author`, `Book`, `Member`, `Loan`).
- **Attribute** — a property of an entity, becomes a **column** (e.g. a Book's `Title`, `ISBN`, `PublishedYear`).
- **Relationship** — a meaningful link between entities, becomes a **foreign key** (e.g. a `Loan` connects a `Book` and a `Member`).

### Reading an ERD

An **ERD** is the picture of that model. Each box is an entity; the lines between boxes are relationships. Keys are marked: **PK** (primary key — the row's identity) and **FK** (foreign key — a pointer to another table's PK). Here is the first-pass Library model we build live this week:

```
AUTHOR                         BOOK                              MEMBER
------                         ----                              ------
AuthorId   PK                  BookId    PK                      MemberId  PK
FirstName                      Title                             FirstName
LastName                       ISBN      (unique)                LastName
BirthYear                      PublishedYear                     Email     (unique)
                               AuthorId  FK -> Author            JoinedDate

                       LOAN
                       ----
                       LoanId    PK
                       BookId    FK -> Book
                       MemberId  FK -> Member
                       LoanDate
                       DueDate
                       ReturnDate   (null = still out)
```

To **read** it: "An `Author` has an `AuthorId` that identifies it. A `Book` carries an `AuthorId` foreign key, so each book points at one author. A `Loan` carries *two* foreign keys — `BookId` and `MemberId` — so a loan ties one book to one member." You just read the whole system's structure off four boxes.

> This first pass is deliberately **not fully normalized** — the demo later adds `CategoryName`/`CategoryDescription` straight onto `Book`, which is a redundancy "smell" we fix Wednesday by extracting a `Category` table and turning Author↔Book into many-to-many via a bridge. For now, focus on reading boxes and lines.

### Multiplicity (cardinality)

**Multiplicity** answers "how many of A relate to how many of B?" Three cases:

| Multiplicity | Meaning | Library example | How it's implemented |
|---|---|---|---|
| **1:1** | one row ↔ at most one row | a Member ↔ one MembershipCard | FK + a UNIQUE constraint on the FK |
| **1:N** | one row ↔ many rows | one Author ↔ many Books | FK on the "many" side (`Book.AuthorId`) |
| **M:N** | many ↔ many | many Books ↔ many Authors (co-authors) | a **bridge/junction table** (Wednesday) |

The crucial implementation fact: **the foreign key always lives on the "many" side.** One author has many books, so the `AuthorId` column lives on `Book`, not on `Author`. M:N can't be done with a single FK at all — it needs a third table — which is exactly why Wednesday introduces the `BookAuthor` bridge.

### Choosing data types

Every column has a **data type** that fixes what it can hold and how much space it takes. Picking well is part integrity (a `DATE` column can never hold "banana") and part efficiency. The ones you need this week:

| Type | Holds | Use it for | Notes |
|---|---|---|---|
| `INT` | whole numbers ~±2.1 billion | ids, counts, years | the default integer |
| `BIGINT` | whole numbers ~±9.2 quintillion | very large ids/counts | when `INT` might overflow (e.g. global transaction ids) |
| `VARCHAR(n)` | **variable**-length text up to n | names, titles, emails | stores only the chars used + a length |
| `CHAR(n)` | **fixed**-length text, padded to n | codes that are always n long (ISBN-13) | pads with spaces — comparisons can trip on that |
| `DECIMAL(p,s)` | exact decimal, p digits, s after the point | money, precise measures | `DECIMAL(10,2)` = up to 99,999,999.99 |
| `DATE` | a calendar date | birth dates, loan dates | use `DATETIME2` when you also need a time |

Worked choices on the Library model:

```sql
Title    VARCHAR(200)   -- titles vary wildly in length; don't pad
ISBN     CHAR(13)       -- an ISBN-13 is ALWAYS exactly 13 chars -> fixed length fits
Price    DECIMAL(10,2)  -- money must be exact; never FLOAT (rounding errors)
BookId   INT            -- a library won't exceed 2.1 billion books
JoinedDate DATE         -- a calendar day, no time component needed
```

> **Never store money as `FLOAT`/`REAL`.** Binary floating point can't represent `0.10` exactly, so sums drift by cents. `DECIMAL` (a.k.a. `NUMERIC`) is exact. This is a classic interview gotcha.

`DECIMAL(p,s)`: **p** = total digits (precision), **s** = digits after the decimal (scale). `DECIMAL(10,2)` allows 8 digits before the point and 2 after.

### Translating a real-world description into a schema

The skill interviewers actually probe: turn English into tables. A repeatable recipe:

1. **Find the nouns → entities.** "Track **books**, the **members** who borrow them, and each **loan**." → `Book`, `Member`, `Loan`.
2. **Find the properties → attributes/columns.** "A book has a title, ISBN, and published year." → `Title`, `ISBN`, `PublishedYear`.
3. **Find the verbs/links → relationships.** "A member **borrows** books." → a `Loan` table linking `Member` and `Book`.
4. **Decide multiplicity.** "A member can have **many** loans; each loan is **one** member." → 1:N, so FK on `Loan`.
5. **Pick a primary key per entity** and **a data type per column.**

Run that on "A library lends books to members; record which member borrowed which book and when it's due" and you reconstruct the ERD above. That is the exercise — and it's exactly **Milestone 1** of this week's async lab, in your own domain.

## Code Example

The ERD's `Book`–`Author` relationship (1:N first pass), expressed as the schema it becomes tomorrow — note the data-type choices and the FK that encodes the relationship line:

```sql
CREATE TABLE dbo.Author
(
    AuthorId  INT IDENTITY(1,1) PRIMARY KEY,   -- INT id, auto-numbered
    FirstName VARCHAR(50) NOT NULL,            -- variable text
    BirthYear INT NULL                         -- a year is a whole number
);

CREATE TABLE dbo.Book
(
    BookId   INT IDENTITY(1,1) PRIMARY KEY,
    Title    VARCHAR(200) NOT NULL,            -- titles vary -> VARCHAR
    ISBN     CHAR(13) NOT NULL,                -- always 13 chars -> CHAR
    AuthorId INT NOT NULL,                     -- the FK column (many side)
    CONSTRAINT FK_Book_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Author (AuthorId)
);
```

`AuthorId` sitting on `Book` *is* the "one author, many books" relationship line from the ERD made real.

## Common Mistakes / Interview Traps

- **Putting the foreign key on the wrong side.** The FK goes on the **many** side. Authors don't list their books; books name their author.
- **Trying to model M:N with one foreign key.** You can't. Many-to-many *always* needs a bridge table (Wednesday). If asked "how do you model students↔courses," answer "junction table."
- **`CHAR` vs `VARCHAR` confusion.** `CHAR(50)` for a name wastes space and pads with spaces (causing sneaky comparison bugs). Use `CHAR` only for genuinely fixed-length codes; `VARCHAR` otherwise.
- **`FLOAT` for money.** Use `DECIMAL`. Always.
- **Confusing multiplicity with optionality.** "1:N" describes *how many*; whether the side is *required* (NOT NULL) is a separate question. Keep them apart.

## Decision Guide: `CHAR` vs `VARCHAR`

| Reach for `CHAR(n)` when… | Reach for `VARCHAR(n)` when… |
|---|---|
| Every value is exactly `n` long (ISBN-13, state code `CA`, country `US`) | Length varies (names, titles, emails, descriptions) |
| You want fixed, predictable storage | You want to store only what's used |
| (rare) | (almost always — this is the default) |

## Summary

- A **data model** turns nouns into **entities/tables**, properties into **attributes/columns**, and links into **relationships/foreign keys**; an **ERD** is the picture of it.
- **Multiplicity** is 1:1, 1:N, or M:N; the **FK lives on the "many" side**, and **M:N needs a bridge table**.
- Pick **data types** for integrity and efficiency: `VARCHAR` for variable text, `CHAR` for fixed codes, `INT`/`BIGINT` for whole numbers, `DECIMAL` for money, `DATE` for dates.
- **Translate requirements** with the nouns→verbs→multiplicity recipe — the same recipe drives this week's async-lab Milestone 1 in your own domain.

## Additional Resources

- [Database design basics — Microsoft Support](https://support.microsoft.com/en-us/office/database-design-basics-eb2159cf-1e30-401a-8084-bd4f9c9ca1f5)
- [Data types (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/data-types/data-types-transact-sql)
- [Entity-Relationship model (overview) — Wikipedia](https://en.wikipedia.org/wiki/Entity%E2%80%93relationship_model)
