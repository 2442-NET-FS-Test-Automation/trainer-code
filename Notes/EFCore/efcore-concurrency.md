# Optimistic Concurrency: RowVersion, Retry, and No Oversell

## Learning Objectives
- Explain optimistic vs pessimistic concurrency control and why EF Core defaults to optimistic.
- Configure a `RowVersion` concurrency token and describe what it changes about EF's `UPDATE`.
- Handle `DbUpdateConcurrencyException` with the reload-and-retry pattern.
- Explain how the same mechanism, run under many threads with one `DbContext` per unit of work and one
  transaction per order, prevents overselling.

## Why This Matters
Two order fulfillments read `CurrentStock = 5` at the same moment; both subtract 3; both save. Naively,
stock lands at 2 — but six units left the building. That is the **oversell race**, the canonical
lost-update bug, and any inventory-shaped system must be able to prove it cannot happen: after a burst
whose demand exceeds stock, on-hand never goes negative and units-fulfilled equals units-depleted. The
mechanism is one Fluent API line plus one disciplined retry loop — best understood in two stages: the
conflict in isolation first, then the same loop under real concurrency.

## The Concept

### Optimistic vs pessimistic
- **Pessimistic**: lock the row when you read it; nobody else touches it until you finish. Safe, but locks
  held across user/think/network time strangle throughput and invite deadlocks.
- **Optimistic**: read freely, and at write time *detect* whether someone beat you. If they did, the write
  fails and *you* decide what to do — retry, merge, or give up.

Web workloads (many short, rarely-colliding writers) fit optimistic. EF Core supports it natively through
**concurrency tokens**.

### The RowVersion token
One property plus one Fluent API line:

```csharp
public class InventoryItem
{
    ...
    public int CurrentStock { get; set; }
    public byte[] RowVersion { get; set; } = default!;   // the token
}

// OnModelCreating
b.Entity<InventoryItem>().Property(i => i.RowVersion).IsRowVersion();
```

SQL Server now maintains `RowVersion` itself — every write to the row bumps it. And EF changes its
`UPDATE` to compare-and-swap:

```sql
UPDATE Inventory SET CurrentStock = @new
WHERE Id = @id AND RowVersion = @versionIRead;   -- 0 rows affected => somebody else wrote first
```

Zero rows affected means your read is stale: EF throws `DbUpdateConcurrencyException` instead of silently
overwriting the other writer (a *lost update*).

### Stage 1 — the conflict, single-threaded
You can manufacture the race deliberately with two scopes, i.e. two contexts holding the same row:

```csharp
var first  = firstDb.Inventory.First(i => i.Id == 1);   // RowVersion = A
var second = secondDb.Inventory.First(i => i.Id == 1);  // RowVersion = A (same row, second context)

first.CurrentStock--;
firstDb.SaveChanges();                                  // row now has RowVersion = B

second.CurrentStock--;                                  // still expects A
try { secondDb.SaveChanges(); }                         // throws: A != B
catch (DbUpdateConcurrencyException ex)
{
    var entry   = ex.Entries.Single();
    var current = entry.GetDatabaseValues();            // FRESH values from the DB
    entry.OriginalValues.SetValues(current!);           // accept the store's RowVersion
    ((InventoryItem)entry.Entity).CurrentStock =
        current!.GetValue<int>(nameof(InventoryItem.CurrentStock)) - 1;   // re-apply on fresh
    secondDb.SaveChanges();                             // retry succeeds
}
```

The retry recipe: **catch -> reload database values -> re-check/re-apply your change against the fresh
values -> save again.** The re-check matters: the world changed while you were stale, and your operation
must be re-decided, not just re-sent.

### Stage 2 — the same loop under threads (the no-oversell core)
Under a concurrent burst, three rules combine with the token:

1. **One `DbContext` per unit of work.** `DbContext` is *not thread-safe*; each order fulfillment creates
   its own from an injected `IDbContextFactory<LibraryDbContext>` (`AddDbContextFactory` at startup —
   a factory rather than the scoped context, because background work outlives the request scope).
2. **One transaction per order.** Decrement + status change + fulfillment event go through one
   `SaveChanges()`, so an order is fulfilled entirely or not at all — never a decremented-but-unmarked row.
3. **Bounded reload-retry with a fresh-stock re-check.** The heart of a `FulfillmentService`:

```csharp
for (var attempt = 0; ; attempt++)
{
    try { await db.SaveChangesAsync(ct); return true; }
    catch (DbUpdateConcurrencyException ex) when (attempt < 3)      // bounded, not forever
    {
        foreach (var entry in ex.Entries)
        {
            var current = await entry.GetDatabaseValuesAsync(ct);
            if (current is null) return false;                      // row deleted -> give up
            entry.OriginalValues.SetValues(current);
            if (entry.Entity is InventoryItem inv)
            {
                var fresh = current.GetValue<int>(nameof(InventoryItem.CurrentStock));
                var want  = requestedByProductId[inv.ProductId];
                if (fresh < want) return false;                     // re-CHECK: not enough now -> backorder
                inv.CurrentStock = fresh - want;                    // re-APPLY on the fresh value
            }
        }
    }
}
```

A loser of the race re-reads real stock; if enough remains it retries, if not the order becomes a
**backorder** instead of a negative stock row. That is the entire no-oversell argument: the database
enforces "no stale write lands," and the retry enforces "every landed write was decided on fresh stock."
The threading side (`Task.WhenAll`, background bursts) is covered in `../04-multithreading/`; the
severity-tiered logging around backorders in `../05-observability-patterns/`.

Verify it empirically — burst demand > stock, then:

```csharp
app.MapGet("/verify/no-oversell", (LibraryDbContext db) => new {
    anyNegative    = db.Inventory.Any(i => i.CurrentStock < 0),          // must be false
    unitsFulfilled = db.FulfillmentEvents.Count(e => e.Type == "Fulfilled")  // == units depleted
});
```

## Say It in an Interview
- *"Pessimistic concurrency locks the row at read; optimistic reads freely and detects a conflicting write
  at save time. Web workloads fit optimistic — short writes, rare collisions."*
- *"With `IsRowVersion()`, EF's UPDATE becomes compare-and-swap: `WHERE Id = @id AND RowVersion = @read`.
  Zero rows affected means a stale read, and EF throws `DbUpdateConcurrencyException` instead of losing an
  update."*
- *"My recovery loop is bounded: catch, reload database values, re-decide the operation on fresh state —
  retry if it still makes sense, take the give-up path if it doesn't. Blind resubmission just re-loses the
  race."*
- *"Under a concurrent burst I add two structural rules: one `DbContext` per unit of work — it isn't
  thread-safe — from a `DbContextFactory`, and one transaction per business operation so partial writes
  can't exist. Token + re-check + those two rules is a provable no-oversell."*

## Check Yourself
1. Two writers both read stock 5 and both subtract 3 with no token configured. What lands in the database,
   and what is this failure called?
2. What single configuration turns EF's UPDATE into compare-and-swap, and who maintains the version value?
3. In the retry loop, why is `entry.OriginalValues.SetValues(current)` necessary before saving again?
4. Why must the retry *re-check* stock instead of just re-applying the subtraction?
5. Why does a concurrent burst need a `DbContextFactory` instead of the request's scoped `DbContext`?
6. Name the two post-burst assertions that together prove no oversell.

**Answers:** (1) Stock 2, with 6 units gone — a lost update: the second write silently overwrote the
first. (2) `Property(x => x.RowVersion).IsRowVersion()`; SQL Server bumps the column on every write.
(3) It accepts the store's current `RowVersion` as the new expected value — otherwise the next save
compares against the old token and fails again. (4) The world changed: remaining stock may no longer cover
the request; re-applying blindly would drive stock negative — re-decide, and back-order if short. (5) The
scoped context belongs to one request thread and dies with it; concurrent tasks each need their own
context because `DbContext` is not thread-safe. (6) No inventory row negative; units fulfilled equals
units depleted.

## Summary
- Optimistic concurrency detects conflicting writes at save time instead of locking at read time; EF
  throws `DbUpdateConcurrencyException` when the token check fails.
- `IsRowVersion()` turns EF's UPDATE into compare-and-swap on a server-maintained version column.
- Recovery = reload fresh values, re-decide the operation, retry — bounded, with a give-up path
  (backorder), never blind resubmission.
- Under concurrency: context per unit of work + transaction per order + the retry = provably no oversell.

## Resources
- [Handling concurrency conflicts (Microsoft Learn)](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
- [DbContext factory and thread safety](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#using-a-dbcontext-factory-eg-for-blazor)
- [Transactions in EF Core](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
