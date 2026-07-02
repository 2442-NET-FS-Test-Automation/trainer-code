# Debugging and Profiling Multithreaded Code

## Learning Objectives
- Debug multithreaded code with the Visual Studio Threads, Parallel Stacks, and Tasks windows.
- Recognize the debugging patterns for races, deadlocks, and starvation.
- Profile thread behavior (Concurrency Visualizer / dotnet-trace) to find contention and switching waste.

## Why This Matters
Single-threaded bugs reproduce; multithreaded bugs *schedule*. A race fires on some runs and not others, a
deadlock happens only under load, and stepping through with a debugger changes the timing enough to hide
the bug you are hunting ("Heisenbug"). You need tools that show **all threads at once** and techniques
that do not depend on lucky timing.

## The Concept

### First, think; then, look
Before any tool: multithreading bugs are almost always *shared mutable state without synchronization* or
*locks acquired in inconsistent order*. Re-read the code asking "what is shared? who writes it? under
what lock?" — an unsynchronized `balance += amount` is findable by inspection. Tools are for when
inspection fails.

### The Visual Studio windows
Break anywhere (or hit pause while the app is hung) and open **Debug > Windows**:

- **Threads** — every thread, its state, and where it currently is. For a *hang*: pause and read what each
  thread is waiting on. A deadlock is visually two threads each parked at a `lock` statement.
- **Parallel Stacks** — all call stacks at once, as a graph. A two-lock deadlock shows as two branches,
  each ending in `Monitor.Enter`; the cycle is legible in one screenshot.
- **Tasks** — live `Task` objects with status (`Awaiting`, `Blocked`, `Deadlocked` when VS can prove it) —
  the async-era counterpart to Threads.
- **Debug location toolbar / thread switcher** — step *one* thread while others stay frozen
  ("Freeze"/"Thaw" on the Threads window) to force the interleaving you suspect.

Practical drills for the three bug classes:

| Bug | How it looks | Debug move |
|---|---|---|
| Race | wrong values, varies per run | log the interleaving (below); freeze/thaw threads to force the bad order; then *fix the code*, not the timing |
| Deadlock | hang, CPU near 0 | pause; Threads/Parallel Stacks; find the two `Monitor.Enter`s and the lock cycle |
| Starvation | progress but one worker never wins | Threads over time: same thread perpetually WaitSleepJoin; check priorities and lock hold times |

### Logging as a debugging tool
A breakpoint perturbs timing; a structured log line barely does. For race hunting, a thread-stamped trail
is often the fastest tool you own:

```csharp
Log.Information("Fulfill {OrderId} read stock {Stock} on thread {Thread}",
    orderId, inv.CurrentStock, Environment.CurrentManagedThreadId);
```

Read the interleaving after the fact: two threads both reading `Stock = 5` before either writes is the
race, in writing. (Structured logging done right makes this trail queryable — see
`../05-observability-patterns/serilog-structured-logging.md`.)

### Profiling: seeing contention and switching
Debuggers show a moment; profilers show *time*.

- **Concurrency Visualizer** (VS extension, Analyze menu): per-thread timelines colored by state —
  green running, red blocked on synchronization, with the blocking stack on click. Lock convoys (threads
  taking turns blocking on one hot lock) and oversubscription (far more context switches than work) are
  instantly visible as red stripes.
- **dotnet-trace / dotnet-counters** (cross-platform CLI): `dotnet-counters monitor` live-streams
  `threadpool-thread-count`, `threadpool-queue-length`, and `monitor-lock-contention-count` — that last
  one rising fast is your lock bottleneck; `dotnet-trace collect` records events for offline analysis.
- **Visual Studio Performance Profiler** (Debug > Performance Profiler): CPU usage per thread over time —
  the quick answer to "is the parallel path actually running in parallel?"

Profiling a sequential-vs-parallel benchmark makes the Amdahl story concrete: if the parallel run's
speedup disappoints, the profiler shows whether the time went to lock/row contention (red synchronization
blocks), retries, or the pool simply not fanning out.

## Say It in an Interview
- *"Multithreaded bugs schedule rather than reproduce, so I reason about shared state first — what is
  shared, who writes it, under what lock — and reach for tools that show all threads at once."*
- *"For a hang I pause and read the Threads and Parallel Stacks windows: a deadlock is two threads parked
  at `Monitor.Enter` with a visible lock cycle. Freeze/thaw lets me force a suspected interleaving."*
- *"For races I prefer thread-stamped structured logs over breakpoints — a breakpoint perturbs the timing
  that causes the bug; a log line barely does."*
- *"For contention I profile: Concurrency Visualizer timelines show blocked-on-sync time, and
  `dotnet-counters`' lock-contention count rising fast points straight at the hot lock."*

## Check Yourself
1. Why can a debugger breakpoint make a race disappear, and what tool replaces it?
2. Your app hangs at CPU ~0%. Which two VS windows, and what visual pattern confirms deadlock?
3. What does a rapidly rising `monitor-lock-contention-count` tell you?
4. Starvation vs deadlock in the Threads window over time — how do they look different?
5. A parallel benchmark shows barely any speedup. Name the two profiler findings that explain it.

**Answers:** (1) The pause changes thread timing enough to hide the interleaving ("Heisenbug"); a
thread-stamped structured log records the interleaving with minimal perturbation. (2) Threads + Parallel
Stacks; two (or more) threads each parked at `Monitor.Enter`, forming a cycle. (3) Threads are fighting
over one hot lock — a synchronization bottleneck (lock convoy). (4) Deadlock: the involved threads never
change state again. Starvation: the system progresses but one thread stays perpetually WaitSleepJoin while
others win. (5) Heavy blocked-on-synchronization time (contention on shared rows/locks) or the pool not
fanning out (work effectively serialized / oversubscription switching waste).

## Summary
- Reason about shared state first; tools second — and never trust "it passed this run" for concurrent
  code.
- Threads / Parallel Stacks / Tasks windows: the hang-diagnosis kit; freeze/thaw forces interleavings.
- Deadlocks are diagnosed at pause time (two parked `Monitor.Enter`s); races are diagnosed from
  thread-stamped logs.
- Profilers (Concurrency Visualizer, dotnet-counters/trace) reveal contention and context-switch waste —
  the *why* behind a disappointing benchmark.

## Resources
- [Debug multithreaded applications in Visual Studio (Microsoft Learn)](https://learn.microsoft.com/en-us/visualstudio/debugger/debug-multithreaded-applications-in-visual-studio)
- [Concurrency Visualizer](https://learn.microsoft.com/en-us/visualstudio/profiling/concurrency-visualizer)
- [dotnet-counters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters)
