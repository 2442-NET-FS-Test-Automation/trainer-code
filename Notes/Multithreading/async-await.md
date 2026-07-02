# async/await: Responsiveness Without Blocking

## Learning Objectives
- Explain what `async`/`await` does and what problem it solves (I/O-bound waiting).
- Distinguish CPU-bound from I/O-bound work and pick threads vs async accordingly.
- Apply async with multithreading to optimize responsiveness, and avoid the classic traps
  (`.Result`, `async void`).

## Why This Matters
Every endpoint in a well-built .NET service is async: `await db.Inventory.ToListAsync()`,
`await SaveChangesAsync(ct)`, `await Task.WhenAll(tasks)`. Web servers live or die on this — a thread that
*blocks* on the database is a thread that cannot serve the next request, and the pool runs dry exactly
when traffic peaks. Once async composes with real concurrency, the distinction between "freeing a thread"
and "using more threads" becomes a daily design decision — and one of the most reliable interview
questions in .NET.

## The Concept

### Two kinds of work, two tools
```
CPU-bound   busy computing              -> threads/parallelism help (Parallel.For, Task.Run)
I/O-bound   waiting on network/disk/DB  -> async helps (free the thread while waiting)

sync  : call --[ thread BLOCKED waiting ]--> result
async : call --await--> (thread returned to pool) ... (resumes when I/O completes) --> result
```

`async`/`await` does **not** make work faster and does not create threads. It makes *waiting* free: at the
`await`, the method pauses, its thread goes back to the pool, and when the awaited I/O completes the method
resumes (often on a different pool thread). One sentence for interviews: *async is about not blocking
threads while waiting; parallelism is about using more threads to compute.*

### The mechanics
```csharp
app.MapGet("/inventory", async (LibraryDbContext db) =>
    await db.Inventory.ToListAsync());
```

- `async` marks a method that may pause; it returns `Task` (or `Task<T>`).
- `await` unwraps a task's result and yields the thread until it completes; read it like blocking code
  that simply does not block.
- Async is contagious upward by design: `ToListAsync` makes the lambda `async`, which is fine — Minimal
  API handlers, controller actions, even `Main` can be async. Fighting the contagion is where bugs live.

The rules that keep you out of trouble (worth reciting):

1. **Never `.Result` or `.Wait()` in server code.** They block the very thread await exists to free, and
   in contexts with synchronization they deadlock. `await` all the way up.
2. **Never `async void`** (except event handlers). Its exceptions bypass every catch you own. Return
   `Task`.
3. **Pass the `CancellationToken` through** to every awaitable that takes one (see
   `cancellation-exceptions.md`).

### Async + multithreading: a burst, dissected
A concurrent fulfillment burst composes both tools, each doing its own job:

```csharp
var tasks   = planned.Select(id => FulfillOneAsync(id, ct));   // concurrency: all start
var results = await Task.WhenAll(tasks);                       // async: no thread blocks on the join
```

Inside each `FulfillOneAsync`, every database call is awaited — so while order 17's `SaveChangesAsync` is
in flight over the wire, that pool thread is fulfilling order 23. A handful of threads drive dozens of
concurrent orders because *nobody holds a thread just to wait*. That is "apply async/await in combination
with multithreading to optimize responsiveness" in one method. The same shape gives a burst endpoint its
responsiveness: the handler `await`s nothing long — it queues the background task and returns 202
immediately.

Sequential-await vs concurrent-await is the perf lever you control daily:

```csharp
var a = await GetAAsync(); var b = await GetBAsync();   // serial: t(A) + t(B)
var ta = GetAAsync();      var tb = GetBAsync();
await Task.WhenAll(ta, tb);                             // overlapped: max(t(A), t(B))
```

And the boundary case: wrapping *CPU-bound* work in `Task.Run` inside a web handler does not add capacity —
it just moves the burn to another pool thread. `Task.Run` in servers is for genuine compute you must keep
off the request path, not a cargo-cult async-ifier.

### One more visible detail
Log the thread id before and after an `await Task.Delay(50)` — they often differ. Resumption is a pool
thread, not "your" thread: never stash thread-local state across an `await`, and now the
`Mutex`/`ReaderWriterLockSlim` thread-affinity warnings in `synchronization-primitives.md` have their
reason.

## Say It in an Interview
- *"`await` doesn't create a thread — it releases one. The method pauses, the thread returns to the pool,
  and the method resumes when the I/O completes, often on a different thread."*
- *"Async is for I/O-bound waiting; parallelism is for CPU-bound computing. A web server needs async so
  threads aren't held hostage to database calls — that's what keeps the pool alive at peak traffic."*
- *"My iron rules: never `.Result` or `.Wait()` in server code — that's the classic deadlock; never
  `async void` because its exceptions bypass every catch; always forward the `CancellationToken`."*
- *"To overlap independent calls I start both tasks first and `await Task.WhenAll` — serial awaits cost
  the sum, overlapped awaits cost the max."*

## Check Yourself
1. Does `await Task.Delay(50)` start a new thread? What happens to the current one?
2. Why is `var x = svc.GetDataAsync().Result;` in a controller a production incident waiting to happen?
3. Two independent HTTP calls take 300ms and 500ms. Total time if awaited serially vs overlapped — and
   write the overlapped shape.
4. What makes `async void` uniquely dangerous compared to `async Task`?
5. Why doesn't wrapping CPU-bound work in `Task.Run` inside a request handler add server capacity?
6. Why must you not hold a `Mutex` across an `await`?

**Answers:** (1) No — the method pauses and its thread returns to the pool; resumption may be on a
different pool thread. (2) It blocks a pool thread on work that needs a thread to complete —
deadlock-prone in synchronized contexts and pool-starving under load; `await` instead. (3) 800ms serial vs
~500ms overlapped: `var ta = A(); var tb = B(); await Task.WhenAll(ta, tb);`. (4) There is no `Task` to
await or observe — exceptions escape every catch block you own. (5) The compute still burns a pool
thread — it just moved; async only wins where the work is *waiting*, not computing. (6) Mutex ownership is
per-thread and the continuation may resume on a different thread — release would fail or corrupt
ownership.

## Summary
- I/O-bound -> async (free the thread); CPU-bound -> parallelism (use more threads); a burst uses both
  correctly.
- `await` = non-blocking pause; the method resumes later, possibly on a different thread.
- Iron rules: no `.Result`/`.Wait()` in servers, no `async void`, forward tokens, start-then-await to
  overlap independent work.
- Responsiveness = never holding a thread (or a caller) hostage to a wait: 202 + background task at the
  API level, awaited I/O at the data level.

## Resources
- [Asynchronous programming with async and await (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)
- [Async/await best practices (David Fowler's guidance)](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md)
- [There Is No Thread (Stephen Cleary)](https://blog.stephencleary.com/2013/11/there-is-no-thread.html)
