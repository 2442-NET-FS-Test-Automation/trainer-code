# Normalization: 0NF → 3NF (with Justification)

## Learning Objectives

- Explain why redundancy causes **insertion, update, and deletion anomalies**.
- Normalize a schema step by step from **0NF → 1NF → 2NF → 3NF**, justifying each step.
- State the rule each normal form enforces in one sentence.
- Resolve a **many-to-many** relationship with a **bridge/junction table**.
- Weigh the **benefits and drawbacks** of normalization.

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

Normalization is the single densest topic on the QC-2 exam ("normalize from 0NF to 3NF, providing step-by-step justification") and the one people most often fumble in interviews — not because it's hard, but because they memorized "1NF, 2NF, 3NF" without the *why*. This note gives you the why: each normal form removes a specific kind of redundancy that would otherwise let your data contradict itself.

This is the payoff of Wednesday's `03-keys-normalization` commit. Monday's first-pass Library schema deliberately had smells — `CategoryName`/`CategoryDescription` repeated on every `Book`, and a single `Book.AuthorId` that can't hold co-authors. Today we refactor those away, extracting a `Category` table and a `BookAuthor` bridge. Watching the smells turn into clean tables is how normalization stops being abstract.

## The Concept

### The problem: redundancy and anomalies

Suppose we kept everything about a book — including its category and *all* its authors — in one flat table:

| BookId | Title | CategoryName | CategoryDesc | Author1 | Author2 |
|---|---|---|---|---|---|
| 1 | Clean Code | Software | Programming craft | R. Martin | |
| 2 | Refactoring | Software | Programming craft | M. Fowler | K. Beck |
| 3 | Patterns | Software | Programming craft | Gamma | Helm |

"Programming craft" is copied on every Software row. That redundancy causes three **anomalies**:

- **Update anomaly** — fix the category description and you must update *every* book in that category; miss one and the data now contradicts itself.
- **Insertion anomaly** — you can't record a new category until at least one book uses it (where would the row go?).
- **Deletion anomaly** — delete the last Software book and the category's description vanishes with it.

Normalization is the disciplined removal of exactly these anomalies. Each **normal form** is a stricter rule; you satisfy them in order.

### 0NF → 1NF: atomic values, no repeating groups

**1NF rule:** every cell holds a **single (atomic) value**, and there are **no repeating groups** of columns. Each row is identified by a key.

The flat table violates 1NF: `Author1`/`Author2` is a repeating group (and what about a third author?). **Fix:** remove the repeating group into its own rows/table. First pass — one row per book-author:

| BookId | Title | CategoryName | CategoryDesc | Author |
|---|---|---|---|---|
| 1 | Clean Code | Software | Programming craft | R. Martin |
| 2 | Refactoring | Software | Programming craft | M. Fowler |
| 2 | Refactoring | Software | Programming craft | K. Beck |

Now every cell is atomic and there's no `Author1/Author2`. **Justification:** atomic columns are queryable (you can filter/join on `Author`) and you're no longer capped at a fixed number of authors.

### 1NF → 2NF: no partial dependencies

**2NF rule:** be in 1NF **and** every non-key column depends on the **whole** primary key — no column depends on just *part* of a composite key. (2NF only has teeth when the PK is **composite**; with a single-column PK you're automatically in 2NF.)

Our 1NF table's natural key is the composite **(BookId, Author)**. But `Title`, `CategoryName`, `CategoryDesc` depend only on `BookId` — *part* of the key — not on the author. That's a **partial dependency**. **Fix:** split into a table keyed by `BookId` and a bridge keyed by the pair:

`Book` (keyed by `BookId`):

| BookId | Title | CategoryName | CategoryDesc |
|---|---|---|---|
| 1 | Clean Code | Software | Programming craft |
| 2 | Refactoring | Software | Programming craft |

`BookAuthor` (keyed by `BookId + AuthorId`):

| BookId | AuthorId |
|---|---|
| 1 | 1 |
| 2 | 2 |
| 2 | 3 |

**Justification:** book facts now live with the book (stored once each), and the bridge holds *only* the relationship. This is also exactly how we resolve the **many-to-many** between books and authors — see below.

### 2NF → 3NF: no transitive dependencies

**3NF rule:** be in 2NF **and** no non-key column depends on **another non-key column** (no *transitive* dependency). Every non-key column must depend on "the key, the whole key, and nothing but the key."

In `Book`, `CategoryDesc` doesn't depend on `BookId` — it depends on `CategoryName`, which is itself a non-key column. That's a **transitive dependency** (`BookId → CategoryName → CategoryDesc`). It's why the description was duplicated. **Fix:** extract a `Category` table:

`Category`:

| CategoryId | CategoryName | CategoryDesc |
|---|---|---|
| 1 | Software | Programming craft |

`Book` (now references the category):

| BookId | Title | CategoryId |
|---|---|---|
| 1 | Clean Code | 1 |
| 2 | Refactoring | 1 |

**Justification:** "Programming craft" is now stored **once**. Change it in one place; no book can disagree about what "Software" means. All three anomalies are gone.

### The normal forms in one line each

| Form | Rule (one sentence) | Removes |
|---|---|---|
| **1NF** | atomic values, no repeating groups | multi-valued cells / repeated columns |
| **2NF** | 1NF + every non-key column depends on the **whole** key | partial dependencies (on part of a composite key) |
| **3NF** | 2NF + no non-key column depends on another non-key column | transitive dependencies |

The mnemonic for 2NF+3NF: each non-key column depends on **"the key, the whole key, and nothing but the key."**

### Resolving many-to-many with a bridge table

A book can have many authors; an author writes many books — **M:N**. You **cannot** model M:N with a foreign key on either side (a single `Book.AuthorId` allows only one author). The resolution is a **bridge / junction table** whose rows are the *pairings*:

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

Each row is one (book, author) pairing; the **composite primary key** stops the same pair appearing twice; each column is also a **foreign key**. M:N has become two 1:N relationships pointing into the bridge.

### Benefits and drawbacks

| Benefits | Drawbacks |
|---|---|
| Each fact stored **once** → no update/insert/delete anomalies | Data spread across more tables → **more joins** to reassemble |
| Less storage, smaller rows | Queries are more complex to write and can be slower |
| Integrity is easy to enforce (one source of truth) | Over-normalizing can hurt read-heavy/reporting workloads |

This is the real engineering judgment: normalize to **3NF** for transactional systems (the default, what we do), but **deliberately denormalize** read-heavy reporting/analytics tables where join cost outweighs the anomaly risk. "Normalize until it hurts, denormalize until it works."

## Code Example

The Library refactor end-state — the `03-keys-normalization` commit's shape:

```sql
-- 3NF: category lives once
CREATE TABLE dbo.Category
(
    CategoryId   INT IDENTITY(1,1) PRIMARY KEY,
    Name         VARCHAR(60)  NOT NULL UNIQUE,
    Description  VARCHAR(200) NULL
);

-- Book references the category instead of repeating its text
ALTER TABLE dbo.Book ADD CategoryId INT NULL
    CONSTRAINT FK_Book_Category REFERENCES dbo.Category (CategoryId);
-- (then backfill CategoryId from the old CategoryName, drop CategoryName/Description)

-- M:N authors via the bridge (replaces the single Book.AuthorId)
-- CREATE TABLE dbo.BookAuthor ( ... composite PK (BookId, AuthorId) ... );
```

Before: "Programming craft" on every Software book, one author per book. After: the category text exists once, and a book can have any number of authors.

## Common Mistakes / Interview Traps

- **Reciting the forms without the rule.** Be ready to say what each form *removes* (repeating groups → partial deps → transitive deps), not just "1, 2, 3."
- **Skipping 2NF's precondition.** Partial dependencies only exist with a **composite** key. A single-column PK table in 1NF is already in 2NF.
- **Confusing 2NF and 3NF.** 2NF = depends on part of the *key*; 3NF = depends on another *non-key* column. Different dependency, different fix.
- **Modeling M:N with a foreign key.** Always a **bridge table** with a composite key.
- **Believing more normalization is always better.** 3NF is the sweet spot; beyond it (BCNF/4NF — out of scope) and aggressive normalization can hurt query performance. Denormalization is a legitimate, deliberate trade-off.

## Decision Guide: normalize vs denormalize

| Normalize (to 3NF) when… | Denormalize when… |
|---|---|
| Writes are frequent; integrity is paramount | Reads dominate; the table is a report/cache |
| Data changes and must stay consistent | Data is mostly static or rebuildable |
| It's a transactional (OLTP) system | It's analytics/reporting (OLAP) and joins are the bottleneck |

Default to normalized; denormalize only with a measured reason.

## Summary

- Redundancy causes **insertion, update, and deletion anomalies**; normalization removes them.
- **1NF**: atomic values, no repeating groups. **2NF**: 1NF + no **partial** dependencies on part of a composite key. **3NF**: 2NF + no **transitive** dependencies on another non-key column.
- Mnemonic: every non-key column depends on **the key, the whole key, and nothing but the key.**
- **Many-to-many → bridge table** with a **composite primary key** that is also two foreign keys.
- Normalization trades **fewer anomalies + less redundancy** for **more joins**; 3NF is the default, denormalization a deliberate read-performance choice.

## Additional Resources

- [Database design basics (covers 1NF–3NF and relationships) — Microsoft Support (beginner)](https://support.microsoft.com/en-us/office/database-design-basics-eb2159cf-1e30-401a-8084-bd4f9c9ca1f5)
- [Database normalization (1NF/2NF/3NF) — Wikipedia](https://en.wikipedia.org/wiki/Database_normalization)
- [Many-to-many relationships (how an ORM models the M:N bridge) — Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many)
