# Cancellation, Exceptions in Tasks, Background Work, and Graceful Shutdown

## Learning Objectives
- Implement cooperative cancellation with `CancellationTokenSource`/`CancellationToken` and contrast it
  with preemptive interruption.
- Handle exceptions from tasks, including `AggregateException` and fire-and-forget fault observation.
- Run background work behind a `202 Accepted` without losing errors or ignoring shutdown.
- Drain in-flight work gracefully and verify no half-applied state survives a shutdown.

## Why This Matters
A burst endpoint answers in milliseconds while the work continues on a background task — which raises the
three questions this note answers. Who can stop that work (cancellation)? Where do its errors go
(exception observation)? And what happens to orders in flight when someone presses Ctrl-C mid-burst
(graceful shutdown)? Production readiness is all three; the self-verification procedure at the end is one
you should actually run against any service you build this way.

## The Concept

### Cooperative cancellation
.NET cancellation is **cooperative**: a `CancellationTokenSource` flips a flag; the running code must
*check* it and stop cleanly. Nothing is killed from outside:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));  // auto-cancel after 100ms
var work = Task.Run(() =>
{
    for (long i = 0; ; i++)
        cts.Token.ThrowIfCancellationRequested();   // the cooperative check -> OperationCanceledException
}, cts.Token);
```

Ways to honor a token: `ThrowIfCancellationRequested()` at loop boundaries, checking
`IsCancellationRequested` for a quiet exit, and — most common in service code — **passing the token down**
so the waiting happens inside cancelable calls (`SaveChangesAsync(ct)`, `Task.Delay(ms, ct)`,
`WaitAsync(ct)`). A method that accepts a `CancellationToken` and never forwards it is lying about being
cancelable.

**Cooperative vs preemptive (the standard contrast):** preemptive interruption (`Thread.Abort`, killing
threads) stops code at an *arbitrary instruction* — mid-write, lock held, invariant broken — risking
corrupt state, which is why modern .NET removed it. Cooperative cancellation stops only at points *you
chose*, where state is consistent. The cost: code must be written to check. The payoff: an order is never
half-fulfilled by a cancellation.

### Exceptions in tasks
A task that throws stores its exception and rethrows at the join point:

```csharp
var t = Task.Run(() => throw new InvalidOperationException("boom in a task"));
try { t.Wait(); }                                  // sync join -> AggregateException wrapper
catch (AggregateException ex)
{ Console.WriteLine(ex.InnerException!.Message); } // unwrap

try { await t; }                                   // await -> the ORIGINAL exception, no wrapper
catch (InvalidOperationException) { }
```

`Wait()`/`.Result` wrap in `AggregateException` (possibly holding several, e.g. from `WhenAll`); `await`
unwraps the first for you. Cancellation surfaces as `OperationCanceledException` — catch it *separately*
from failures; `catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)` is
the filter pattern for the sync-join case.

The dangerous case is the task nobody joins — **fire-and-forget**. An unobserved fault simply vanishes.
The rule: if you must fire-and-forget, observe inside:

```csharp
_ = Task.Run(async () =>
{
    try
    {
        using var scope = scopes.CreateScope();     // background work gets its OWN DI scope
        var svc = scope.ServiceProvider.GetRequiredService<IFulfillmentService>();
        await svc.FulfillBurstAsync(ids, appStopping);
    }
    catch (Exception ex) { Log.Error(ex, "Burst fulfillment failed"); }   // observe or lose it
}, appStopping);
return Results.Accepted("/orders/status", new { submitted = ids.Count }); // 202 NOW
```

Missing that `try/catch` is one of the most common production bugs in this pattern — the first version of
almost every fire-and-forget endpoint silently swallows faults. Note the DI detail too: the request's
scope dies with the 202, so the background task creates its own scope for its services.

### The right token for background work
A subtle bug worth knowing before you write it: the *request's* `CancellationToken` fires as soon as the
response completes — hand it to the background task and the work cancels itself immediately. Use
**`IHostApplicationLifetime.ApplicationStopping`** instead: a token that survives the request and fires
exactly when the host begins shutting down. Rule: request token for request-scoped work; application
lifetime token for work that outlives the request.

### Graceful shutdown and draining
When the host stops (Ctrl-C, deploy), `ApplicationStopping` fires and in-flight fulfillments see it via
the token they were passed. Because each order commits through **one `SaveChangesAsync` in one
transaction** (see `../01-efcore/efcore-concurrency.md`), every in-flight order either commits whole or is
abandoned whole — there is no decrement-without-status-change to leak. Register a log line so the drain is
visible, and flush the logger last (`Log.CloseAndFlush()` after `app.Run()` returns — see
`../05-observability-patterns/serilog-structured-logging.md`):

```csharp
app.Lifetime.ApplicationStopping.Register(() =>
    Log.Information("Shutdown requested - draining in-flight fulfillments"));
```

**Self-verification procedure (run it against your own service):**
1. Seed known stock, then `POST /orders/burst?n=<large>` so work will still be in flight.
2. Press **Ctrl-C mid-burst**; watch the drain line appear and the process exit cleanly.
3. Restart and check the data: no order decremented-but-unmarked — every order is either fully fulfilled
   (stock down *and* status/event written) or untouched/pending; the no-oversell verify still holds.
4. Check the log file ends with complete lines (the flush worked).
If step 3 finds a half-applied order, your fulfillment spans more than one transaction — that is the bug.

## Say It in an Interview
- *".NET cancellation is cooperative: a source signals, the code checks the token or forwards it into
  cancelable calls, and stops at a consistent point. Preemptive abort could stop mid-write with locks
  held, which is why `Thread.Abort` is gone."*
- *"A task's exception rethrows at the join: `Wait`/`.Result` wrap it in `AggregateException`, `await`
  rethrows the original. Cancellation surfaces as `OperationCanceledException` and I catch it separately
  from real failures."*
- *"Fire-and-forget tasks must observe their own faults — try/catch plus an error log inside the task —
  or the failure vanishes. In ASP.NET Core they also need their own DI scope, since the request's scope
  dies with the response."*
- *"Background work takes the application-lifetime token, not the request token — the request token fires
  when the response completes. And one transaction per unit of work is what makes a Ctrl-C drain clean:
  in-flight work commits whole or not at all."*

## Check Yourself
1. Why doesn't cancelling a token stop a tight loop that never checks it, and what three forms can the
   check take?
2. Same faulted task: what do you catch after `t.Wait()` vs after `await t`?
3. A background task's exception "disappeared." What was missing, and where does it go instead?
4. Why does handing the request's `CancellationToken` to post-202 background work break it?
5. In the Ctrl-C test, what data shape proves the shutdown was *not* graceful?
6. What makes preemptive interruption dangerous in one sentence?

**Answers:** (1) Cancellation is cooperative — nothing external stops the code; checks:
`ThrowIfCancellationRequested()`, testing `IsCancellationRequested`, or forwarding the token into
cancelable awaits. (2) `AggregateException` (unwrap `InnerException`) vs the original exception directly.
(3) A try/catch inside the fire-and-forget task observing the fault (e.g. `Log.Error`); unobserved task
faults vanish. (4) The request token fires when the response completes — the work cancels itself
immediately after the 202. (5) A half-applied order: stock decremented but status/event missing — evidence
the operation spans more than one transaction. (6) It stops code at an arbitrary instruction — locks held,
invariants broken — risking corrupt state.

## Summary
- Cancellation is cooperative: sources signal, code checks (`ThrowIfCancellationRequested`) or forwards the
  token into cancelable calls; preemptive abort corrupts state and is gone from modern .NET.
- `Wait()`/`.Result` wrap faults in `AggregateException`; `await` rethrows the original; catch
  `OperationCanceledException` separately.
- Fire-and-forget tasks must observe their own faults (try/catch + `Log.Error`) and create their own DI
  scope.
- Background work takes `ApplicationStopping`, not the request token; one transaction per unit of work is
  what makes shutdown drain clean — and the Ctrl-C mid-burst check proves it.

## Resources
- [Cancellation in managed threads (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)
- [Task exception handling](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/exception-handling-task-parallel-library)
- [IHostApplicationLifetime and graceful shutdown](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host)
