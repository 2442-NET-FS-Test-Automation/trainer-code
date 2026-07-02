# Synchronization Primitives: SemaphoreSlim, Mutex, ReaderWriterLockSlim

## Learning Objectives
- Explain the use cases for `SemaphoreSlim`, `Mutex`, and `ReaderWriterLockSlim`.
- Choose the right primitive from the shape of the contention.
- Know the async caveat: which primitives can be awaited and which cannot.

## Why This Matters
`lock` and `Interlocked` cover most days. But three questions come up in real services that neither
answers: "at most N at a time" (throttling — capping concurrent database work or calls to a rate-limited
API), "one at a time *across processes*" (a shared file or machine-wide job), and "many readers, rare
writers" (a hot in-memory cache). Each has a dedicated primitive, and the differences between them are a
standard interview follow-up once you have said "lock."

## The Concept

### SemaphoreSlim: at most N at a time
A semaphore holds a count. `Wait` takes a slot (blocking if none free); `Release` returns one. With
N = 1 it acts like a lock; with N > 1 it is a **throttle**:

```csharp
private static readonly SemaphoreSlim Gate = new(4);   // at most 4 concurrent fulfillments

public async Task FulfillThrottledAsync(int orderId, CancellationToken ct)
{
    await Gate.WaitAsync(ct);              // NOTE: awaitable - no thread blocked while waiting
    try { await FulfillOneAsync(orderId, ct); }
    finally { Gate.Release(); }            // ALWAYS release, or the slots leak away
}
```

Two facts make `SemaphoreSlim` the workhorse of modern async code: it has a **`WaitAsync`** (the only
primitive here you can await — `lock` cannot even contain an `await`), and it is cheap ("slim" = purely
in-process, no kernel object until needed). Canonical uses: limit concurrent calls to a rate-limited API,
cap parallel database work, guard a critical section inside async methods.

### Mutex: one at a time, across processes
A `Mutex` is a kernel-level lock that can be **named**, which makes it visible machine-wide — the only one
of these that can coordinate *separate processes*:

```csharp
using var mutex = new Mutex(initiallyOwned: false, name: @"Global\InventorySeeder");
if (!mutex.WaitOne(TimeSpan.FromSeconds(5)))
    throw new TimeoutException("another process is seeding");
try { /* the one-at-a-time work, e.g. re-seeding a shared DB */ }
finally { mutex.ReleaseMutex(); }
```

Use it for: single-instance applications, coordinating scheduled jobs on one machine, protecting a shared
file. Do not use it as an in-process lock — it is dramatically slower than `lock` (every operation crosses
into the kernel) and it is ownership-tracked per *thread*, which makes it incompatible with `await`
(you may resume on a different thread than the one that acquired it).

### ReaderWriterLockSlim: many readers, one writer
A plain `lock` serializes *readers too*, which is a waste when reads vastly outnumber writes and reads are
safe together. `ReaderWriterLockSlim` splits the roles:

```csharp
private static readonly ReaderWriterLockSlim Cache = new();

public decimal? GetPrice(string sku)
{
    Cache.EnterReadLock();                  // MANY threads may hold this simultaneously
    try { return _prices.TryGetValue(sku, out var p) ? p : null; }
    finally { Cache.ExitReadLock(); }
}

public void SetPrice(string sku, decimal price)
{
    Cache.EnterWriteLock();                 // EXCLUSIVE: waits for all readers to drain
    try { _prices[sku] = price; }
    finally { Cache.ExitWriteLock(); }
}
```

Right shape: read-mostly shared structures (lookup tables, config caches). Honest caveat: for a plain
dictionary cache, `ConcurrentDictionary` is usually simpler and at least as fast — reach for RWLS when the
guarded state is *composite* (several fields that must be read consistently) rather than a single map. And
like `Mutex`, it is thread-affine: not for use across `await`.

### Choosing

| Contention shape | Primitive | Why |
|---|---|---|
| one at a time, in-process, sync code | `lock` | simplest, fastest for the job |
| one atomic arithmetic/swap op | `Interlocked` | no blocking at all |
| at most N at a time; or async code | `SemaphoreSlim` | counted; `WaitAsync` |
| one at a time **across processes** | `Mutex` (named) | kernel object, machine-wide |
| many readers, rare writers, composite state | `ReaderWriterLockSlim` | concurrent reads |

## Say It in an Interview
- *"`SemaphoreSlim` is a counted gate — at most N holders — and the workhorse of async code because it's
  the only one of these you can `await` (`WaitAsync`); `lock` can't even contain an `await`."*
- *"`Mutex` is a named kernel object, so it coordinates across processes — single-instance apps, shared
  files, machine-wide jobs. It's far slower than `lock` and thread-affine, so never across `await`."*
- *"`ReaderWriterLockSlim` lets many readers in simultaneously and gives writers exclusivity — right for
  read-mostly composite state, though `ConcurrentDictionary` usually beats it for a simple map."*
- *"I choose by contention shape: one-at-a-time sync = `lock`; single atomic op = `Interlocked`; at-most-N
  or async = `SemaphoreSlim`; cross-process = named `Mutex`; read-mostly composite = RWLS. And always
  release in `finally`."*

## Check Yourself
1. You must cap concurrent calls to a rate-limited API at 4, inside async methods. Which primitive and
   which call?
2. Why can't you guard an `await` with `lock`, and what do you use instead?
3. Two separate processes on one machine must never seed the database simultaneously. Which primitive, and
   what makes it capable of that?
4. When does `ReaderWriterLockSlim` genuinely beat both `lock` and `ConcurrentDictionary`?
5. What happens over time if a `SemaphoreSlim.Release()` is not in a `finally`?
6. `SemaphoreSlim(1)` vs `lock` — when would you pick the semaphore even for one-at-a-time?

**Answers:** (1) `SemaphoreSlim(4)` with `await Gate.WaitAsync(ct)`. (2) `lock`/`Monitor` ownership is
per-thread and an `await` may resume on a different thread; use `SemaphoreSlim(1)` with `WaitAsync`.
(3) A **named** `Mutex` — it is a kernel object, visible machine-wide. (4) Read-mostly *composite* state:
several fields that must be read consistently, where reads can safely overlap but a plain lock would
serialize them. (5) An exception path skips the release, a slot leaks each time, and eventually all slots
are gone — the gate seizes shut: an outage, not a bug message. (6) Inside async methods — it is the
awaitable one-at-a-time gate.

## Summary
- `SemaphoreSlim(N)`: counted gate; the throttle; the only awaitable one — default choice inside async
  methods.
- `Mutex`: named, kernel-level, cross-process exclusivity; slow; never across `await`.
- `ReaderWriterLockSlim`: parallel reads + exclusive writes for read-mostly composite state;
  `ConcurrentDictionary` often beats it for simple maps.
- Always release in `finally`; leaked slots and abandoned locks are outages, not bugs.

## Resources
- [SemaphoreSlim documentation (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim)
- [Mutexes](https://learn.microsoft.com/en-us/dotnet/standard/threading/mutexes)
- [ReaderWriterLockSlim documentation](https://learn.microsoft.com/en-us/dotnet/api/system.threading.readerwriterlockslim)
