# Parallelism, Concurrency, and the Task Parallel Library

## Learning Objectives
- Explain the difference between parallelism and concurrency in C# terms.
- Use the TPL: `Task.Run`, `Task<T>`, `Task.WhenAll`, `Parallel.For`.
- Compare sequential vs multithreaded performance, benchmark it honestly, and explain when parallel loses.

## Why This Matters
The TPL is the layer you will actually write: real services fan work out with `Task.WhenAll` (a burst of
orders, a page of downstream calls) and justify the design with a measured sequential-vs-parallel
comparison. The parallelism-vs-concurrency distinction is also the single most reliable screening question
in .NET interviews — best answered from code you have built, because one method can exhibit both at once.

## The Concept

### Parallelism vs concurrency
- **Concurrency**: multiple tasks *make progress* in overlapping time — interleaved, even on one core.
  About **structure**: dealing with many things at once.
- **Parallelism**: multiple tasks execute *literally simultaneously* on multiple cores. About
  **execution**: doing many things at once.

A single-core machine can be concurrent (the scheduler interleaves) but never parallel. A burst endpoint
is both at once: fulfillments run **in parallel** across pool threads, while the batch as a whole is
**concurrent** with the HTTP request that triggered it (a 202 returns while work continues). One more
pairing to keep straight: parallelism is about *using* many threads for speed; concurrency *control*
(locks, tokens — `synchronization.md`) is about staying correct while doing so.

### Task: work as a value
A `Task` represents in-flight work; `Task<T>` is in-flight work with a future result:

```csharp
Task<long> half1 = Task.Run(() => SumRange(data, 0, data.Length / 2));      // starts NOW, pool thread
Task<long> half2 = Task.Run(() => SumRange(data, data.Length / 2, data.Length));
long total = half1.Result + half2.Result;   // .Result BLOCKS until done (console/sample only)
```

`Task.Run` queues the delegate to the ThreadPool and returns immediately. `.Result`/`.Wait()` block the
calling thread — acceptable in a console sample, a deadlock-and-starvation hazard in server code, where
you `await` instead (`async-await.md`).

### Task.WhenAll: fan out, join once
The pattern concurrent services are built on — start everything, then await the set:

```csharp
var tasks   = planned.Select(id => FulfillOneAsync(id, ct));   // all start; each gets its own DbContext
var results = await Task.WhenAll(tasks);                       // completes when ALL do, results in order
```

Contrast with awaiting inside the loop, which would serialize the orders one by one. `WhenAll` returns the
results as an array (order preserved) and aggregates any exceptions. This is "implement concurrency with
the TPL" in one line.

### Parallel.For: data parallelism over a range
When the work is a CPU-bound loop, `Parallel.For` partitions the range across cores for you:

```csharp
long parallelTotal = 0;
Parallel.For(0, data.Length,
    () => 0L,                                            // per-thread local seed
    (i, _, local) => local + data[i],                    // no sharing inside the loop
    local => Interlocked.Add(ref parallelTotal, local)); // combine partials ONCE per thread
```

The thread-local overload above is the professional form: each thread sums privately and synchronizes only
at the end — synchronizing *inside* the loop body would serialize the whole thing. Use `Parallel.For`/
`Parallel.ForEach` for CPU-bound loops over data; use `Task.WhenAll` for I/O-bound or heterogeneous work.

### Benchmarking sequential vs parallel, honestly
`Stopwatch` around each strategy — with two rules that make the numbers mean something:

```csharp
var ids1 = seeder.ResetAndCreateOrders(n);          // 1) SAME starting state for both runs
var sw1 = Stopwatch.StartNew();
foreach (var id in ids1) await svc.FulfillOneAsync(id, ct);      // sequential
sw1.Stop();

var ids2 = seeder.ResetAndCreateOrders(n);          // reset AGAIN - else run 2 races empty stock
var sw2 = Stopwatch.StartNew();
await svc.FulfillBurstAsync(ids2, ct);                           // parallel
sw2.Stop();
// speedup = sequentialMs / parallelMs   (a typical contended-workload result: ~2x on 8 cores)
```

1. **Reset state between runs.** The first run depleted the stock; timing the second against an empty
   catalog measures nothing. Any benchmark whose runs start from different states is junk.
2. **Report the ratio and explain it.** Speedup is bounded by the *serial fraction* of the work
   (Amdahl's law): the database serializes conflicting writes on the contended rows, retries add work, and
   context switches tax the pool. That is why 8 cores do not give 8x, and why a heavily-contended workload
   can even make parallel *lose*. When parallel does not win, the one-line explanation of why (contention
   on few rows, retry cost) is worth as much as a speedup.

## Say It in an Interview
- *"Concurrency is overlapping progress — structure, even on one core; parallelism is simultaneous
  execution on multiple cores. A burst endpoint shows both: fulfillments run in parallel while the batch
  runs concurrently with the request that queued it."*
- *"A `Task` is work as a value; `Task.Run` queues it to the ThreadPool. I never block on `.Result` in
  server code — that's a deadlock hazard — I `await`."*
- *"`Task.WhenAll` fans out and joins once, preserving result order and aggregating exceptions; awaiting
  inside the loop would serialize everything."*
- *"For CPU-bound loops I use `Parallel.For` with thread-local accumulators so threads only synchronize
  once at the end. And I benchmark honestly: identical starting state per run, report the ratio, explain
  it with Amdahl — the serial fraction and contention bound the speedup."*

## Check Yourself
1. One-core machine: can it be concurrent? Parallel? Why?
2. What is wrong with `foreach (var id in ids) await FulfillOneAsync(id);` when the goal is a concurrent
   burst, and what replaces it?
3. In the `Parallel.For` thread-local overload, why does the body avoid touching `parallelTotal` directly?
4. Your parallel benchmark shows 2x on 8 cores. Name two reasons it is not 8x.
5. Why must a benchmark reset state between the sequential and parallel runs?
6. When do you pick `Parallel.For` over `Task.WhenAll`?

**Answers:** (1) Concurrent yes — the scheduler interleaves; parallel no — simultaneity needs multiple
cores. (2) The `await` serializes: each order waits for the previous; start all tasks first, then
`await Task.WhenAll(tasks)`. (3) Every iteration hitting the shared variable would need synchronization,
serializing the loop; per-thread locals synchronize once per thread at the combine step. (4) Amdahl's
serial fraction (e.g. the database serializing conflicting writes on contended rows), retry work, and
context-switch/pool overhead. (5) Run 1 changed the world (stock depleted); run 2 would measure a
different, cheaper workload — benchmarks require identical starting states. (6) CPU-bound loops over a
data range; `Task.WhenAll` for I/O-bound or heterogeneous work.

## Summary
- Concurrency = overlapping progress (structure); parallelism = simultaneous execution on cores. A burst
  is both.
- `Task.Run` = pool-backed work as a value; avoid `.Result`/`.Wait()` in server code.
- `Task.WhenAll` fans out and joins once; awaiting in a loop serializes.
- `Parallel.For` partitions CPU-bound loops; keep loop bodies share-free, combine at the end.
- Benchmarks: identical starting state per run, report the speedup, explain it (serial fraction,
  contention, switching).

## Resources
- [Task Parallel Library overview (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl)
- [Task.WhenAll documentation](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall)
- [Amdahl's law (Wikipedia)](https://en.wikipedia.org/wiki/Amdahl%27s_law)
