# Transactions, ACID & Isolation Levels

## Learning Objectives

- Explain what a **transaction** is and why databases need them.
- Use **TCL**: `BEGIN TRANSACTION`, `COMMIT`, `ROLLBACK` (with `TRY/CATCH`).
- Define the **ACID** properties and why each matters.
- Identify the four **isolation levels** and the concurrency phenomena they trade off.
- Use transactions to keep a multi-step change **consistent**.

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

Some operations are only correct **all-or-nothing**. Transferring a book between branches, or in the classic case moving money between two accounts, is two writes that must *both* happen or *neither* — a crash in the middle must never leave one done and the other not. A **transaction** is the database's tool for exactly that guarantee. This is the "protect" beat of the week's epic and the first half of Friday's `05-transactions` commit.

ACID and isolation levels are heavily tested: QC-2 makes "purpose of transactions," "use transactions to ensure consistency," and "explain ACID" all Must-know, with isolation levels a Should-know. They're also a favorite interview topic because they reveal whether you understand databases as *concurrent, durable* systems and not just spreadsheets.

## The Concept

### What a transaction is

A **transaction** groups a sequence of statements into a **single logical unit of work** that either **completes entirely (`COMMIT`)** or **undoes entirely (`ROLLBACK`)**. There is no partial state visible to anyone else and none left behind by a failure.

```sql
BEGIN TRANSACTION;
    UPDATE dbo.Book SET AvailableCopies = AvailableCopies - 1 WHERE BookId = 1;
    INSERT INTO dbo.Loan (BookId, MemberId, DueDate) VALUES (1, 2, '2026-07-15');
COMMIT;     -- both changes become permanent together
```

If anything between `BEGIN` and `COMMIT` fails, `ROLLBACK` reverts *every* change since `BEGIN` — as if none of it happened.

### TCL with TRY/CATCH (the safe pattern)

In T-SQL the robust shape wraps the transaction in error handling: commit on success, roll back on any error.

```sql
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE dbo.Book SET AvailableCopies = AvailableCopies - 1 WHERE BookId = 1;
        INSERT INTO dbo.Loan (BookId, MemberId, DueDate) VALUES (1, 2, '2026-07-15');
    COMMIT TRANSACTION;          -- all steps succeeded -> make it permanent
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;        -- any step failed -> undo everything
    THROW;                       -- re-raise so the caller knows it failed
END CATCH;
```

This is the atomicity proof: if the `CHECK` constraint rejects the `UPDATE` (e.g. zero copies left), control jumps to `CATCH`, the whole unit rolls back, and the loan is never recorded. The data is never left half-changed.

### ACID — the four guarantees

ACID is the set of properties a proper transaction provides. Memorize all four with a one-line meaning:

| Property | Meaning | Library example |
|---|---|---|
| **A — Atomicity** | all steps happen or none do | the copy decrement *and* the loan insert, together or not at all |
| **C — Consistency** | the DB moves from one **valid** state to another (all constraints hold) | available copies never goes negative — a `CHECK` would block the commit |
| **I — Isolation** | concurrent transactions don't corrupt each other | two members checking out the last copy don't both succeed |
| **D — Durability** | once committed, it survives crashes/power loss | after `COMMIT`, the loan is on disk even if the server dies a second later |

> "Consistency" here means *constraint-valid*, and it's the link back to Monday: the rules you declared (PK/FK/CHECK) are what a transaction must leave intact. A transaction that would violate a constraint can't commit.

### Concurrency phenomena (why isolation exists)

When transactions run **at the same time**, three read anomalies can occur:

| Phenomenon | What happens |
|---|---|
| **Dirty read** | you read another transaction's **uncommitted** change (which may roll back) |
| **Non-repeatable read** | you read a row twice and get **different values** (another txn updated it between reads) |
| **Phantom read** | you run the same query twice and **new rows appear** (another txn inserted matching rows) |

### The four isolation levels

Isolation levels are the dial that trades **correctness for concurrency**: higher levels prevent more phenomena but reduce throughput (more locking, less parallelism).

| Level | Dirty read | Non-repeatable read | Phantom read | Trade-off |
|---|---|---|---|---|
| **Read Uncommitted** | possible | possible | possible | fastest, least safe — can read garbage |
| **Read Committed** (default) | prevented | possible | possible | sensible default for most apps |
| **Repeatable Read** | prevented | prevented | possible | re-reads are stable; more locking |
| **Serializable** | prevented | prevented | prevented | safest, as if transactions ran one-at-a-time — most locking, least concurrency |

```sql
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRANSACTION;
    -- reads here are fully protected from other transactions' changes
COMMIT;
```

The progression is monotonic: each level down the table prevents one more phenomenon at the cost of concurrency. **Read Committed** is SQL Server's default and the right choice for most workloads; reach for **Serializable** only when correctness under concurrency truly demands it (e.g. financial postings), and accept the throughput cost.

### Producer–consumer (the concept)

A common concurrency pattern: one or more **producers** add work (rows) to a queue table, and **consumers** claim and process them. Transactions + an appropriate isolation level (or row locking) ensure **no two consumers grab the same item** and nothing is lost or double-processed. You don't implement it this week — just connect it to isolation: it's *why* isolation levels matter in real systems.

## Code Example

A book checkout as a correct transaction — the decrement and the loan must agree, and a constraint failure must undo both:

```sql
BEGIN TRY
    BEGIN TRANSACTION;
        -- 1) reserve a copy (CK_Book_Copies blocks this if AvailableCopies is already 0)
        UPDATE dbo.Book
        SET    AvailableCopies = AvailableCopies - 1
        WHERE  BookId = 1;

        -- 2) record the loan
        INSERT INTO dbo.Loan (BookId, MemberId, DueDate)
        VALUES (1, 2, '2026-07-15');
    COMMIT TRANSACTION;
    PRINT 'Checkout committed.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Checkout failed and was rolled back: ' + ERROR_MESSAGE();
END CATCH;
```

If `Book 1` had `AvailableCopies = 0`, the `CHECK` constraint rejects step 1, `CATCH` fires, the transaction rolls back, and **no loan row exists** — the database is never left claiming a book is out when no copy was actually available.

## Common Mistakes / Interview Traps

- **`COMMIT`ting in the middle of a multi-step change.** If a later step fails, the earlier commits can't be undone. Commit **once**, at the end, after all steps succeed.
- **No error handling.** A failure without `TRY/CATCH`/`ROLLBACK` can leave an open transaction holding locks. Always have a rollback path.
- **Confusing Consistency with Isolation.** Consistency = constraints stay valid; Isolation = concurrent transactions don't interfere. Different letters, different guarantees.
- **Using Read Uncommitted "for speed."** It can read uncommitted, soon-to-be-rolled-back data (dirty reads) — fine for rough estimates, dangerous for anything that must be correct.
- **Thinking higher isolation is always better.** Serializable is safest but slowest (most locking, deadlock risk). Match the level to the requirement.
- **Forgetting Durability is post-commit only.** Work *before* `COMMIT` is not guaranteed to survive a crash; only committed data is durable.

## Decision Guide: which isolation level?

| Need | Level |
|---|---|
| Rough counts/estimates, max speed, dirty reads OK | Read Uncommitted |
| Normal app behavior (no dirty reads) | **Read Committed** (default) |
| Re-read the same rows and need stable values | Repeatable Read |
| Full correctness under heavy concurrency (financial) | Serializable |

## Summary

- A **transaction** is an all-or-nothing unit: `BEGIN` → work → `COMMIT` (permanent) or `ROLLBACK` (undo), best wrapped in `TRY/CATCH`.
- **ACID** = **Atomicity** (all or none), **Consistency** (constraints stay valid), **Isolation** (no concurrent corruption), **Durability** (committed survives crashes).
- Concurrency causes **dirty / non-repeatable / phantom** reads; the four **isolation levels** (Read Uncommitted → Read Committed → Repeatable Read → Serializable) prevent progressively more, trading concurrency for safety.
- Use transactions to keep multi-step writes **consistent**; commit once at the end, and roll back on any error.

## Additional Resources

- [Transactions (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/language-elements/transactions-transact-sql)
- [SET TRANSACTION ISOLATION LEVEL — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/statements/set-transaction-isolation-level-transact-sql)
- [ACID — Wikipedia](https://en.wikipedia.org/wiki/ACID)
