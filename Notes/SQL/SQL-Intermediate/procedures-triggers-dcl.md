# Stored Procedures, Functions, Triggers, Sequences & DCL

## Learning Objectives

- Create and **call** a **stored procedure** (`EXEC`).
- Create a **user-defined function (UDF)** and explain how it differs from a stored procedure.
- Describe a **trigger** and how it auto-runs on a DML event.
- Recognize a **sequence** for generating numbers.
- Manage access with **DCL**: `GRANT` and `REVOKE`.

> **Where to run this:** execute these statements against a SQL Server database using **SSMS** or **Azure Data Studio**. New to the setup? The Day-1 `00-setup-docker` walkthrough stands up SQL Server in a container — then run everything against your `sql-training` database.

## Why This Matters

This note completes the "package & protect" finale of Week 3 — the programmable, secured objects that turn a schema into a real application backend, and the second half of Friday's final `06-views-procs` commit. Stored procedures and UDFs package logic *inside* the database; triggers automate reactions; DCL controls who can do what. QC-2 makes "create and call a stored procedure," "create a UDF," and "explain SQL sublanguages" (which includes **DCL**) Must-know, with triggers a Should-know. These are exactly the objects an interviewer points at to ask "how would you enforce/automate/secure that in the database itself?"

## The Concept

### Stored procedures: named, callable logic

A **stored procedure** is a saved block of SQL you invoke by name, optionally with parameters. It can run multiple statements, take input, and change data.

```sql
CREATE PROCEDURE dbo.CheckoutBook
    @BookId   INT,
    @MemberId INT,
    @DueDate  DATE
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
            UPDATE dbo.Book SET AvailableCopies = AvailableCopies - 1 WHERE BookId = @BookId;
            INSERT INTO dbo.Loan (BookId, MemberId, DueDate) VALUES (@BookId, @MemberId, @DueDate);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
```

**Call it** with `EXEC`:

```sql
EXEC dbo.CheckoutBook @BookId = 1, @MemberId = 2, @DueDate = '2026-07-15';
```

Why procedures: **reuse** (write the checkout logic once), **security** (grant `EXECUTE` on the proc without granting direct table access), **performance** (the plan is cached), and **encapsulation** (the app calls one name instead of orchestrating several statements). Friday's proc reuses Friday's transaction pattern — the same `TRY/CATCH` atomicity from `transactions.md`, now packaged.

### User-defined functions (UDFs)

A **UDF** computes and **returns a value** (a scalar) or a table, and is meant to be used **inside queries**.

```sql
CREATE FUNCTION dbo.fn_DaysOverdue (@DueDate DATE, @ReturnDate DATE)
RETURNS INT
AS
BEGIN
    RETURN CASE
             WHEN @ReturnDate IS NULL THEN DATEDIFF(DAY, @DueDate, GETDATE())
             ELSE DATEDIFF(DAY, @DueDate, @ReturnDate)
           END;
END;
```

Use it like any built-in scalar function, right in a `SELECT`:

```sql
SELECT LoanId, dbo.fn_DaysOverdue(DueDate, ReturnDate) AS DaysOverdue
FROM   dbo.Loan;
```

### Procedure vs function — the key distinction

| | **Stored procedure** | **User-defined function** |
|---|---|---|
| Returns | optional output params / result sets | **must return** a value (scalar or table) |
| Called with | `EXEC` (a statement) | inside a query (`SELECT dbo.fn(...)`) |
| Can modify data | **yes** (INSERT/UPDATE/DELETE) | **no** (no side effects on tables) |
| Used for | actions / workflows | computations reused in queries |

The one-line rule: **functions compute and return (no side effects); procedures act (and can change data).** "Can a function `INSERT` into a table?" — no; that's a procedure's job.

### Triggers: code that fires on a DML event

A **trigger** is a special procedure the engine runs **automatically** in response to an `INSERT`, `UPDATE`, or `DELETE` on a table — you never call it directly. Use it to automate reactions: audit logging, derived-value maintenance, enforcing a rule a `CHECK` can't.

```sql
CREATE TRIGGER trg_Loan_AfterInsert
ON dbo.Loan
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AuditLog (Action, TableName, WhenUtc)
    SELECT 'INSERT', 'Loan', SYSUTCDATETIME() FROM inserted;
END;
```

- **`AFTER INSERT`** runs once after rows are inserted. (`AFTER UPDATE`/`AFTER DELETE`, and `INSTEAD OF` triggers, also exist.)
- Inside a trigger, the virtual tables **`inserted`** (new rows) and **`deleted`** (old rows) hold what changed — an `UPDATE` sees both.
- Triggers are powerful but **implicit**: logic that runs invisibly on every write is easy to forget and can hurt performance. Use sparingly, for genuine automation/audit needs.

### Sequences (recognize it)

A **sequence** is a standalone object that hands out numbers on request — like `IDENTITY`, but **not tied to one table**, so several tables can draw from the same number series.

```sql
CREATE SEQUENCE dbo.OrderNumberSeq START WITH 1000 INCREMENT BY 1;
SELECT NEXT VALUE FOR dbo.OrderNumberSeq;   -- 1000, then 1001, ...
```

Awareness level this week: know it exists and how it differs from `IDENTITY` (shared, independent of any table).

### DCL: GRANT and REVOKE

**DCL** (Data **Control** Language) manages **permissions** — who may do what. It's the fifth sublanguage and a guaranteed "name the sublanguages" exam point.

```sql
GRANT SELECT ON dbo.ActiveLoans TO LibraryStaff;   -- allow reading the view
GRANT EXECUTE ON dbo.CheckoutBook TO LibraryStaff; -- allow running the proc
REVOKE SELECT ON dbo.Member FROM LibraryStaff;     -- take back direct table access
```

- **`GRANT`** gives a permission to a user/role; **`REVOKE`** takes it back. (`DENY` explicitly blocks it.)
- The pattern that ties the day together: lock down base tables, then `GRANT` access to **views** (controlled reads) and **procedures** (controlled writes). Users touch the safe surface, never the raw tables — security by encapsulation.

## Code Example

The whole finale working together — a procedure (with a transaction), granted to a role, calling logic the app uses by name:

```sql
-- 1) package the checkout as a secured, reusable action (proc above)
-- 2) expose ONLY the safe surface to staff
GRANT EXECUTE ON dbo.CheckoutBook TO LibraryStaff;  -- can check out
GRANT SELECT  ON dbo.ActiveLoans  TO LibraryStaff;  -- can see active loans (a view)
-- base tables stay locked: staff never SELECT/UPDATE dbo.Book or dbo.Member directly

-- 3) staff use the safe names
EXEC dbo.CheckoutBook @BookId = 2, @MemberId = 1, @DueDate = '2026-07-20';
SELECT * FROM dbo.ActiveLoans;
```

A schema has become an application: actions are procedures, computed values are functions, automation is triggers, and access is controlled with DCL — the `sql-training` thread's final, packaged state.

## Common Mistakes / Interview Traps

- **Procedure vs function mix-up.** Functions **return** and have **no side effects**; procedures **act** and can modify data. A function can't `INSERT`.
- **Calling a proc without `EXEC`** (when it's not the first statement), or trying to use a proc inside a `SELECT` — that's a function's role.
- **Trigger overuse.** Hidden logic on every write is hard to debug and can slow writes. Prefer constraints for rules; use triggers for genuine automation/audit.
- **Forgetting `inserted`/`deleted`.** Inside a trigger these virtual tables are how you see what changed — and they can hold **multiple** rows (write set-based trigger code, not row-by-row assumptions).
- **Confusing `IDENTITY` and `SEQUENCE`.** `IDENTITY` is a column property tied to one table; a `SEQUENCE` is a shared, standalone generator.
- **Forgetting DCL is a sublanguage.** When asked to list the SQL sublanguages, **DCL** (`GRANT`/`REVOKE`) belongs with DDL/DML/DQL/TCL.

## Decision Guide

| You need to… | Use |
|---|---|
| Run a multi-step action / change data by name | **Stored procedure** (`EXEC`) |
| Compute a reusable value inside queries | **User-defined function** |
| React automatically to an INSERT/UPDATE/DELETE | **Trigger** |
| Generate numbers shared across tables | **Sequence** |
| Control who can read/run/change | **DCL** (`GRANT`/`REVOKE`) |

## Summary

- A **stored procedure** is named, callable (`EXEC`) logic that can take parameters and **change data** — reuse, security, cached plans.
- A **UDF** **returns a value** for use inside queries and has **no side effects** — the proc/function line is "act vs compute."
- A **trigger** runs **automatically** on a DML event (uses `inserted`/`deleted`); powerful but implicit — use sparingly.
- A **sequence** generates numbers independent of any table (vs table-bound `IDENTITY`).
- **DCL** — **`GRANT`** and **`REVOKE`** — controls access; the strong pattern is to expose **views + procedures** and lock the base tables.

## Additional Resources

- [CREATE PROCEDURE (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-procedure-transact-sql)
- [CREATE FUNCTION (Transact-SQL) — Microsoft Learn](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-function-transact-sql)
- [DML triggers — Microsoft Learn](https://learn.microsoft.com/en-us/sql/relational-databases/triggers/dml-triggers) · [GRANT (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/grant-transact-sql)
