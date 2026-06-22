# Keys & Referential Integrity

## Learning Objectives

- Explain the **role** of a primary key and a foreign key in a dataset.
- Identify **candidate keys** and explain how one becomes the primary key.
- Recognize **composite**, **unique/alternate**, and **secondary** keys.
- Define **referential integrity** and how foreign keys enforce it.
- Describe **multiplicity** (1:1, 1:N, M:N) and how each is keyed.

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

Keys are what make a relational database *relational*. Without them, you have disconnected tables; with them, you have a web of related facts that the engine keeps honest. Monday you *declared* keys as constraints; today you learn the *theory* — which interviewers probe hard, because understanding keys is understanding the whole model. "What's the role of a primary key?" and "what is referential integrity?" are QC-2 Must-knows and near-certain interview questions.

This is the "relate" beat of the week's epic. Today (`03-keys-normalization`) the Library schema stops being four loosely-typed tables and becomes a properly related model — and keys are the vocabulary for that. The companion note `normalization.md` uses everything here.

## The Concept

### What a primary key *is* (its role)

A **primary key (PK)** is the column (or set of columns) that **uniquely identifies each row** in a table. Its role:

- **Identity** — every row has exactly one PK value, and no two rows share it. It answers "which row?"
- **The anchor for relationships** — every foreign key elsewhere points at *a* primary key. `Loan.BookId` only means something because `Book.BookId` is a stable, unique identity.
- **Enforced uniqueness + not null** — the engine guarantees no duplicates and no missing identity (a PK can never be NULL — an unknown identity is a contradiction).
- **Efficient retrieval** — the PK is backed by an index (Friday's topic), so lookups by key are fast.

> A good PK is **unique, non-null, and stable** (it should never need to change). That's why we usually prefer a **surrogate key** — a meaningless auto-numbered `IDENTITY` id — over a **natural key** (real-world data like an email or SSN). Real-world values change and leak; a surrogate id never does.

### What a foreign key *is* (its role)

A **foreign key (FK)** is a column in one table whose value **must match a primary key value in another table** (or be NULL, if allowed). Its role:

- **It encodes a relationship.** `Book.AuthorId` *is* the "this book is by that author" link.
- **It enforces referential integrity** — the engine refuses any FK value that doesn't point at a real parent row, so you can never have an orphan (a loan for a book that doesn't exist).

A table can have **many** foreign keys (a `Loan` has two: `BookId` and `MemberId`) but **one** primary key.

### Candidate keys, and choosing the primary

A **candidate key** is *any* column or column-set that **could** serve as the primary key — i.e. it's unique and minimal. A table can have several candidate keys; you pick **one** to be the PK, and the rest become **alternate keys** (enforced with `UNIQUE`).

For `Member`, the candidate keys are:

| Candidate key | Unique? | Good PK? |
|---|---|---|
| `MemberId` (surrogate) | yes | **chosen PK** — stable, meaningless, never changes |
| `Email` | yes | could work, but emails change → alternate key (`UNIQUE`) |
| `(FirstName, LastName)` | **no** (two Ada Lovelaces possible) | not even a candidate |

So `MemberId` is the PK; `Email` is a candidate key promoted to an **alternate key** via `UNIQUE`. Being able to "identify valid candidate keys" is an explicit QC-2 Should-know.

### Other key types you should name

| Key | What it is |
|---|---|
| **Primary key** | the chosen unique, not-null identifier (one per table) |
| **Candidate key** | any minimal unique column-set eligible to be the PK |
| **Alternate key** | a candidate key *not* chosen as PK (enforced with `UNIQUE`) |
| **Composite key** | a key made of **two or more columns** together |
| **Foreign key** | a column referencing another table's PK |
| **Secondary key** | a non-unique column used mainly for **lookups/indexing** (e.g. `LastName`) |
| **Surrogate key** | a system-generated stand-in id (`IDENTITY`) with no business meaning |
| **Natural key** | a key drawn from real-world data (ISBN, email) |

### Composite keys

A **composite key** uses more than one column because no single column is unique. The classic case is a **bridge table** for a many-to-many relationship — Wednesday's `BookAuthor`:

```sql
CREATE TABLE dbo.BookAuthor
(
    BookId   INT NOT NULL,
    AuthorId INT NOT NULL,
    CONSTRAINT PK_BookAuthor PRIMARY KEY (BookId, AuthorId),   -- composite PK
    CONSTRAINT FK_BA_Book   FOREIGN KEY (BookId)   REFERENCES dbo.Book (BookId),
    CONSTRAINT FK_BA_Author FOREIGN KEY (AuthorId) REFERENCES dbo.Author (AuthorId)
);
```

Neither `BookId` nor `AuthorId` is unique on its own (a book has several authors; an author has several books), but the **pair** is — so `(BookId, AuthorId)` together form the primary key. Each column is *also* a foreign key. (The normalization that motivates this table is `normalization.md`.)

### Referential integrity

**Referential integrity** is the property that **every foreign key value refers to a row that actually exists** (or is NULL). No dangling pointers, no orphan rows. The FK constraint enforces it in both directions:

- **On insert/update of the child:** you can't set `Book.AuthorId = 999` unless author 999 exists.
- **On delete/update of the parent:** you can't delete `Author 1` while a book still points at it — unless the FK's referential action (`CASCADE`, `SET NULL`) says what to do instead.

```sql
-- author 999 does not exist:
INSERT INTO dbo.Book (Title, ISBN, AuthorId) VALUES ('Orphan', '978...', 999);
-- REJECTED: FK_Book_Author — referential integrity violation
```

That guarantee is why a relational database can be trusted: the *relationships themselves* are validated data, not hopeful convention.

### Multiplicity, and how each is keyed

**Multiplicity** (cardinality) describes how many rows on one side relate to how many on the other — and each shape is implemented with keys:

| Multiplicity | Example | Implementation |
|---|---|---|
| **1:1** | Member ↔ LibraryCard | FK on one side **+ `UNIQUE`** on that FK (so it can't repeat) |
| **1:N** | Author → Books | FK on the **many** side (`Book.AuthorId`); no UNIQUE |
| **M:N** | Books ↔ Authors | a **bridge table** with a composite key of both FKs |

The rule to remember: **the foreign key lives on the "many" side**, and **many-to-many requires a third (bridge) table** — a single FK cannot express it.

## Code Example

The Library model's keys, read off the schema:

```sql
-- Author: surrogate PK
-- AuthorId  PK (IDENTITY)

-- Member: surrogate PK + an alternate (candidate) key
-- MemberId  PK
-- Email     UNIQUE  (candidate key, not chosen as PK)

-- Book: PK + FK (1:N to Author, first pass) + alternate key
-- BookId    PK
-- ISBN      UNIQUE  (natural candidate key)
-- AuthorId  FK -> Author

-- Loan: PK + two FKs (the relationship hub)
-- LoanId    PK
-- BookId    FK -> Book
-- MemberId  FK -> Member

-- BookAuthor (after normalization): composite PK that is also two FKs (M:N)
-- (BookId, AuthorId)  PK, and each column FK
```

Every relationship in the system is a foreign key pointing at a primary key — that's the entire wiring diagram.

## Common Mistakes / Interview Traps

- **"A primary key is just an auto-increment id."** The *role* is unique, non-null identity and the anchor of relationships. `IDENTITY` is one common *way* to generate one, not the definition.
- **Allowing a NULL primary key.** Impossible by definition — identity can't be unknown.
- **Confusing candidate key and primary key.** Candidate = *eligible*; primary = the *chosen one*. The others become alternate keys.
- **Thinking a composite key is "two primary keys."** It's **one** primary key made of two columns. A table still has exactly one PK.
- **Modeling M:N with a single FK.** Impossible — you need a bridge table with a composite key.
- **Conflating referential integrity with cascade.** Referential integrity is the *guarantee* (no orphans); `CASCADE`/`SET NULL`/`NO ACTION` are the *policies* for honoring it on parent changes.

## Decision Guide: surrogate vs natural primary key

| Prefer a **surrogate** (`IDENTITY`) when… | A **natural** key is okay when… |
|---|---|
| The natural candidate can change (email, name) | The value is truly immutable and stable |
| The natural key is large or composite | It's compact and single-column |
| You want relationships immune to business changes | The key is also how the world refers to the row |

Default to surrogate keys; expose natural keys as `UNIQUE` alternate keys.

## Summary

- A **primary key** is the unique, non-null **identity** of a row and the **anchor** every foreign key points at (one per table).
- A **foreign key** encodes a relationship and enforces **referential integrity** — no FK value without a matching parent row.
- **Candidate keys** are all eligible identifiers; one becomes the PK, the rest become **alternate keys** (`UNIQUE`). **Composite keys** span multiple columns (e.g. a bridge table).
- **Multiplicity**: FK on the **many** side for 1:N; FK + `UNIQUE` for 1:1; a **bridge table with a composite key** for M:N.

## Additional Resources

- [Primary and Foreign Key Constraints — Microsoft Learn](https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints)
- [Keys (relational model) — Wikipedia](https://en.wikipedia.org/wiki/Relational_database#Keys)
- [Referential integrity — Wikipedia](https://en.wikipedia.org/wiki/Referential_integrity)
