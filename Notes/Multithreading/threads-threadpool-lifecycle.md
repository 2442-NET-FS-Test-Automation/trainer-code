# Threads, the ThreadPool, and the Thread Lifecycle

## Learning Objectives
- Create and manage threads with the `Thread` class (`Start`, `Join`, `IsAlive`, `IsBackground`).
- Describe the thread lifecycle and how the OS schedules execution.
- Explain context switching and its performance cost.
- Use the `ThreadPool` for lightweight parallel work and say why pooling exists.

## Why This Matters
Any concurrent workload — a burst of orders fulfilled simultaneously, a server juggling requests — runs on
threads. Everything above them (`Task`, `async/await`, `Parallel.For`) is machinery *managing* threads for
you, and when that machinery misbehaves (a hang, a starved pool, CPU pinned at 100% doing nothing useful)
you debug it in these terms: what threads exist, what state each is in, and who is switching between them.
Start at the bottom so the abstractions above make sense.

## The Concept

### The Thread class
A thread is an independent path of execution through your code. The OS gives every process one ("the main
thread"); you can create more:

```csharp
Console.WriteLine($"main runs on thread #{Environment.CurrentManagedThreadId}");

var worker = new Thread(() =>
    Console.WriteLine($"hello from Thread #{Environment.CurrentManagedThreadId}"));

Console.WriteLine(worker.IsAlive);   // false - Unstarted
worker.Start();                      // Unstarted -> Running
worker.Join();                       // block HERE until worker finishes
Console.WriteLine(worker.IsAlive);   // false - Stopped
```

Key members: `Start()` begins execution, `Join()` blocks the calling thread until the target finishes
(`Join(timeout)` gives up after a bound — the standard way to demonstrate a deadlock without hanging the
program), `IsAlive` reports liveness, `Thread.Sleep(ms)` yields the CPU for a while.
`IsBackground = true` marks a thread as one that should **not** keep the process alive — foreground
threads do, which is why deliberately-deadlocked exhibit threads are made background: otherwise the
process would never exit.

### The lifecycle
```
Unstarted --Start()--> Running <--> WaitSleepJoin --> Stopped
                          ^   (Sleep/Join/lock wait)
                          |
                     (scheduler slices)
```

- **Unstarted**: constructed, not started.
- **Running**: eligible for CPU time — *not* necessarily on a CPU right now.
- **WaitSleepJoin**: blocked — sleeping, `Join`ing, or waiting on a lock/I/O. Uses no CPU.
- **Stopped**: its method returned (or threw). A stopped thread cannot restart; make a new one.

The **OS scheduler** time-slices Runnable threads across cores: each gets a quantum (a few ms), then the
scheduler may swap it for another. Your code cannot control *when* it runs — only make its dependencies
explicit (`Join`, locks, signals). That non-determinism is why a handful of started worker threads finish
in a different order every run, and why "it worked when I ran it" is not evidence of thread safety.

### Context switching: the tax
Swapping a core from thread A to thread B means saving A's registers/state, loading B's, and losing warm
CPU caches. Each switch is small; at scale it dominates: **more threads than cores does not mean more
throughput — it means more switching**. A thousand busy threads on eight cores spend their time trading
places (thrashing), not working. Symptoms: CPU high, useful work low. The lesson that follows from it:
threads help *CPU-bound* work up to about core count, and *waiting* (I/O) should not hold a thread at all —
that is what async is for (`async-await.md`).

### The ThreadPool
Creating a thread costs real memory (about 1 MB of stack) and kernel time. For lots of small work items,
.NET keeps a **pool** of pre-created worker threads and hands work to whichever is free:

```csharp
ThreadPool.QueueUserWorkItem(_ => done.Enqueue(n * n));   // borrowed thread, returned when done
```

The pool sizes itself to the machine, reuses threads, and queues excess work instead of spawning
unboundedly — the antidote to both the creation cost and the context-switch tax. You will rarely call it
directly: **`Task.Run`, `Parallel.For`, and the async machinery all run on the ThreadPool** (see
`parallelism-tpl.md`). A crude wait like `while (done.Count < 5) Thread.Sleep(5)` is what life looks like
without completion signaling — which is exactly what `Task` adds.

When *do* you make a raw `Thread`? Long-lived, dedicated work with custom needs (its own priority, a
guaranteed non-pool thread) — rare in application code. Default to the pool via tasks.

## Say It in an Interview
- *"A thread is an independent path of execution; with the `Thread` class I `Start` it, `Join` to wait for
  it, and check `IsAlive`. Foreground threads keep the process alive; background threads don't."*
- *"The lifecycle is Unstarted, Running, WaitSleepJoin while blocked, Stopped — and the OS scheduler
  time-slices runnable threads, so completion order is non-deterministic."*
- *"Context switching saves and restores thread state and evicts warm caches — oversubscribing cores buys
  switching, not throughput. Threads help CPU-bound work up to about core count; waiting belongs to
  async, not a parked thread."*
- *"The ThreadPool amortizes thread creation by reusing workers and queueing excess work — `Task.Run`,
  `Parallel.For`, and async continuations all ride it; raw `Thread` is for rare long-lived dedicated
  work."*

## Check Yourself
1. `worker.Start(); worker.Join();` — what does each call do to the calling thread?
2. A thread is in `WaitSleepJoin`. Is it consuming CPU, and what three things put it there?
3. Why do five identical worker threads print their finish lines in a different order each run?
4. Your service spawns 1,000 busy threads on 8 cores and throughput *drops*. Explain in one sentence.
5. Why does a deliberately deadlocked demo thread need `IsBackground = true`?
6. When would you reach for a raw `Thread` instead of `Task.Run`?

**Answers:** (1) `Start` begins the worker's execution (calling thread continues); `Join` blocks the
calling thread until the worker stops. (2) No — it is blocked by `Sleep`, `Join`, or a lock/I-O wait.
(3) The OS scheduler time-slices them non-deterministically; ordering is never guaranteed without explicit
coordination. (4) Far more runnable threads than cores means the CPUs spend their time context-switching
(thrashing) instead of working. (5) A foreground thread keeps the process alive — a deadlocked foreground
thread means the program never exits. (6) Long-lived, dedicated work needing custom properties (priority,
guaranteed non-pool thread) — rare; pool-backed tasks are the default.

## Summary
- `Thread`: `Start`/`Join`/`IsAlive`/`IsBackground`; foreground threads keep the process alive.
- Lifecycle: Unstarted -> Running <-> WaitSleepJoin -> Stopped; the OS scheduler time-slices, so ordering
  is non-deterministic.
- Context switches tax every over-subscription: threads ~ cores for CPU work; don't park threads on waits.
- The ThreadPool amortizes creation and bounds switching; `Task.Run`/`Parallel.For` ride it — raw threads
  are the rare case.

## Resources
- [Threads and threading (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/standard/threading/threads-and-threading)
- [The managed thread pool](https://learn.microsoft.com/en-us/dotnet/standard/threading/the-managed-thread-pool)
- [ThreadState enumeration](https://learn.microsoft.com/en-us/dotnet/api/system.threading.threadstate)
