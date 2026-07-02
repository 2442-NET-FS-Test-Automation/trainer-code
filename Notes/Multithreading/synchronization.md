# Synchronization: Races, Locks, Deadlocks, and Thread-Safe Collections

## Learning Objectives
- Explain what a race condition is and demonstrate one.
- Use `lock`/`Monitor` and `Interlocked`, and evaluate the trade-offs between them.
- Identify and avoid deadlocks and thread starvation.
- Use thread-safe collections (`ConcurrentDictionary`, `ConcurrentQueue`, `BlockingCollection`) and justify
  the choice.

## Why This Matters
The classic inventory oversell bug *is* a race condition — two threads read stock, both decide there is
enough, both write. Everything in this note is a tool for the same underlying problem: shared mutable
state plus concurrent access. A database-backed service ultimately solves its race at the database (the
`RowVersion` compare-and-swap in `../01-efcore/efcore-concurrency.md`), but the in-memory versions here
are the same logic at a different layer — and the layer interviewers probe first.

## The Concept

### The race, demonstrated
`Balance += amount` reads, adds, writes — three steps. Two threads interleave those steps and one update
vanishes:

```csharp
public class Bank
{
    public long Balance;
    public void DepositUnsafe(long amount) => Balance += amount;   // read-modify-write: NOT atomic
}

Parallel.For(0, 100_000, _ => bank.DepositUnsafe(1));
// Balance: usually < 100000 - lost updates, different every run
```

Non-deterministic, mostly-works, silently-wrong: the worst class of bug. Fixes below, in order of
preference for each shape.

### lock / Monitor: mutual exclusion
`lock` serializes access to a **critical section** — one thread inside at a time:

```csharp
private readonly object _gate = new();     // a DEDICATED lock object

public void DepositSafe(long amount)
{
    lock (_gate) { Balance += amount; }    // always exactly 100000
}
```

`lock` is compiler sugar for `Monitor.Enter`/`Monitor.Exit` in a try/finally; `Monitor` adds extras
(`TryEnter` with timeout, `Wait`/`Pulse` signaling) you reach for rarely. Rules of craft: lock on a
private dedicated object (never `this`, never a string, never something external code can also lock);
keep the section *small* — never do I/O or call out to unknown code while holding a lock.

### Interlocked: atomic single operations
For single arithmetic/exchange operations, the CPU can do it atomically without a lock:

```csharp
long counter = 0;
Parallel.For(0, 100_000, _ => Interlocked.Increment(ref counter));   // always 100000
// also: Interlocked.Add, Exchange, CompareExchange
```

**The trade-off interviewers ask for:** `Interlocked` is the fastest — one atomic CPU instruction, no
blocking — but only covers *single* operations on a single field. `lock`/`Monitor` guards
*multi-statement* invariants (check-then-act, updating two fields together) at the cost of blocking and
switch pressure. Choose by shape: one counter -> `Interlocked`; "check stock then decrement it" -> a lock
(or, across processes, the database's optimistic version of the same idea).
`Interlocked.CompareExchange` is, not coincidentally, the in-memory sibling of the `RowVersion` UPDATE:
write only if the value is still what I read.

### Deadlock and starvation
**Deadlock**: two threads each hold a lock the other needs — permanent mutual wait. Built deliberately:

```csharp
var t1 = new Thread(() => { lock (lockA) { Thread.Sleep(50); lock (lockB) { } } });
var t2 = new Thread(() => { lock (lockB) { Thread.Sleep(50); lock (lockA) { } } });
// t1 holds A wants B; t2 holds B wants A -> neither ever proceeds
```

Avoidance: **acquire multiple locks in one global, consistent order** (everyone takes A then B); hold
fewer locks, for less time; use timeouts (`Monitor.TryEnter`) to detect and recover. If you ever build a
deadlock exhibit on purpose, mark the threads `IsBackground = true` and `Join` with a bound — contain your
failure demos.

**Starvation**: a thread never gets what it needs — not stuck on a cycle, just perpetually losing:
higher-priority work always jumps the queue, or a greedy path holds a lock so often others rarely enter.
Deadlock is *nobody* moves; starvation is *somebody* never moves. Cures: fair queuing, short critical
sections, not blocking pool threads on long waits.

### Thread-safe collections
Correctly locking around a `Dictionary` yourself is easy to get wrong; `System.Collections.Concurrent`
ships collections with the synchronization built in and APIs shaped for concurrency:

```csharp
// per-thread counters - AddOrUpdate is one ATOMIC check-and-modify
var counts = new ConcurrentDictionary<int, int>();
counts.AddOrUpdate(tid, 1, (_, prev) => prev + 1);

// a SKU index - built once at startup, read by every worker thread
_skuToProductId = new ConcurrentDictionary<string, int>(db.Products.ToDictionary(p => p.Sku, p => p.Id));

var queue = new ConcurrentQueue<int>();   // lock-free FIFO; TryDequeue
```

The API shape is the lesson: `TryAdd`/`TryGetValue`/`AddOrUpdate`/`GetOrAdd` fuse check-and-act into one
atomic call, because with plain methods the *gap between your check and your act* is where another thread
sneaks in. `BlockingCollection<T>` adds producer/consumer semantics on top — `Take()` blocks until an item
arrives, `CompleteAdding()` ends the party — the classic pipeline primitive. Justify the choice the same
way as any structure: shared across threads + mutated -> concurrent collection; confined to one thread or
read-only after construction -> the ordinary one is fine (and faster).

## Say It in an Interview
- *"A race condition is an unsynchronized read-modify-write on shared state — `balance += x` is three
  steps, two threads interleave them, and an update silently vanishes. Non-deterministic, which is what
  makes it nasty."*
- *"`lock` — sugar over `Monitor.Enter`/`Exit` in a try/finally — serializes a critical section. I lock on
  a private dedicated object, never `this` or a string, and keep the section small."*
- *"`Interlocked` does single atomic operations with one CPU instruction — fastest, but one field, one op;
  multi-statement invariants need a lock. `CompareExchange` is compare-and-swap — the in-memory sibling of
  an optimistic-concurrency UPDATE."*
- *"Deadlock is a cycle of lock waits — prevented by acquiring locks in one global order. Starvation is
  different: someone always loses the race but the system moves; fairness and short holds fix it."*
- *"Concurrent collections fuse check-and-act atomically — `AddOrUpdate`, `GetOrAdd` — closing the gap a
  hand-rolled check-then-add leaves open."*

## Check Yourself
1. Why does `Parallel.For(0, 100_000, _ => balance += 1)` land below 100,000, and why is the result
   different each run?
2. Name three things you should never lock on, and the one thing you should.
3. `Interlocked.Increment` vs `lock` around `counter++`: which is faster and why — and when does
   `Interlocked` stop being enough?
4. Two threads, two locks, opposite acquisition order. Name the failure and the standard prevention.
5. Deadlock vs starvation in one sentence each.
6. Why is `if (!dict.ContainsKey(k)) dict.Add(k, v)` broken under concurrency even on a
   `ConcurrentDictionary`, and what replaces it?

**Answers:** (1) `+= ` is read-modify-write; interleaved threads overwrite each other's writes (lost
updates), and the scheduler's interleaving differs per run. (2) Never `this`, a string, or any object
external code can reach; use a private dedicated `object _gate = new()`. (3) `Interlocked` — a single
atomic CPU instruction, no blocking; it stops being enough when the invariant spans multiple
statements/fields (check-then-act). (4) Deadlock; acquire multiple locks in one global, consistent order.
(5) Deadlock: a cycle of waits, nobody progresses. Starvation: no cycle, but one thread perpetually loses
and never progresses. (6) The gap between your check and your add is where another thread inserts — use
the fused atomic `TryAdd`/`GetOrAdd`.

## Summary
- Race = unsynchronized read-modify-write on shared state; symptoms are silent, non-deterministic loss.
- `lock` (Monitor) guards multi-statement critical sections; dedicated private lock object, small sections.
- `Interlocked` = atomic single ops, fastest, narrowest; `CompareExchange` mirrors the RowVersion idea.
- Deadlock: cyclic lock wait — break it with global lock ordering. Starvation: perpetual losing — fairness
  and short holds.
- Concurrent collections fuse check-and-act atomically (`AddOrUpdate`, `GetOrAdd`); `BlockingCollection`
  for producer/consumer.

## Resources
- [Overview of synchronization primitives (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/standard/threading/overview-of-synchronization-primitives)
- [Interlocked operations](https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked)
- [Thread-safe collections](https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)
