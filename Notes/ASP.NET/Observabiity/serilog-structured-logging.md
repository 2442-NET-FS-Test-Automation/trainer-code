# Serilog: Structured Logging with Severity Tiers

## Learning Objectives
- Configure Serilog once at startup, with console and rolling-file sinks, and flush it at shutdown.
- Write structured (templated) log lines instead of string interpolation, and explain why.
- Apply severity tiers deliberately: Information for expected outcomes, Warning for business-notable
  events, Error for failures.

## Why This Matters
When forty orders fulfill concurrently, `Console.WriteLine` debugging is over — you cannot re-run the
moment. Logs are how a service explains itself after the fact, and *structured* logs are how you query
that explanation ("show me every backorder for order 17") instead of grepping prose. Configuration is the
easy part; the durable skills are the templating habit and the severity-tier judgment — both standard
code-review material on any team.

## The Concept

### Configure once, flush once
Serilog is configured exactly one time, at startup — never per class, never per request:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()                                                        // dev eyes
    .WriteTo.File("logs/fulfilment-.log", rollingInterval: RollingInterval.Day) // one file per day
    .CreateLogger();
builder.Host.UseSerilog();       // ASP.NET Core's ILogger<T> now routes into Serilog too

app.Run();
Log.CloseAndFlush();             // AFTER Run returns (shutdown): flush buffered writes
```

`UseSerilog()` means both styles land in the same sinks: static `Log.Information(...)` calls and the
framework's injected `ILogger<T>` (exception middleware uses the latter — same pipeline). The
`CloseAndFlush()` at the very bottom is part of graceful shutdown: file sinks buffer, and a killed process
without a flush ends its log mid-line — precisely when you most need the ending (see
`../04-multithreading/cancellation-exceptions.md`).

### Structured, not interpolated
The habit that separates structured logging from `printf`:

```csharp
// YES - message template + named properties
Log.Information("Fulfilled {OrderId} ({LineCount} lines)", orderId, order.Lines.Count);

// NO - interpolation destroys the structure
Log.Information($"Fulfilled {orderId} ({order.Lines.Count} lines)");
```

Both print the same text. The difference: the template version stores `OrderId = 17` as a **property** on
the event. A JSON sink emits `{"OrderId": 17, ...}`; a log platform indexes it; "every event where
OrderId = 17" is a query, not a regex. The template string is also constant, so events group naturally
("this line fired 4,000 times") instead of every message being unique. Names in braces are PascalCase by
convention and should name the *thing*, not the sentence position.

### Severity tiers, tied to meaning
Levels are not decoration; they encode *who should care*. A worked scheme for an ordering service:

| Level | Meaning | Example lines |
|---|---|---|
| `Information` | expected, notable outcomes | `"Fulfilled {OrderId} ({LineCount} lines)"`; the shutdown drain notice |
| `Warning` | business-notable, not a malfunction | `"Backordered {OrderId}: insufficient stock"`; `"Rejected order: unknown SKU {Sku}"` |
| `Error` | a failure someone must look at | `Log.Error(ex, "Fulfillment failed for {OrderId}", orderId)`; a background task's observed fault |

The judgment call worth internalizing: **a backorder is a Warning, not an Error.** The system worked
correctly — stock was insufficient and the code took its designed path. Reserve Error for *the code or its
dependencies failing* (unhandled exception, database unreachable), and always pass the exception object as
the first argument (`Log.Error(ex, "...")`) so the stack trace travels with the event. Below Information
sit `Debug`/`Verbose` (high-volume diagnostics, usually off in production); above Error sits `Fatal` (the
process is dying). Filtering by level per sink — console at Information, file at Debug — is a one-line
`restrictedToMinimumLevel` when you need it.

Two placement rules: log at the point of *decision* (the service that decided "backorder" logs it — not
every caller up the stack), and never log inside a tight loop at Information (that is what Debug is for).

## Say It in an Interview
- *"I configure Serilog once at startup — console plus a rolling file — route the framework's
  `ILogger<T>` into it with `UseSerilog()`, and call `CloseAndFlush()` after `Run()` so buffered writes
  survive shutdown."*
- *"I write message templates, never interpolation: `{OrderId}` becomes an indexed property on the event,
  so finding every event for one order is a query, not a regex — and the constant template groups events
  naturally."*
- *"My tier rule: Information for expected outcomes, Warning for business-notable events that are not
  malfunctions — a backorder is a Warning, the system worked — and Error only for failures, always with
  the exception object attached."*
- *"I log at the point of decision, not at every layer above it, and keep hot loops at Debug."*

## Check Yourself
1. Why does `Log.Information($"Fulfilled {orderId}")` print correctly but still count as a bug in review?
2. Where exactly does `Log.CloseAndFlush()` go, and what goes wrong without it?
3. Stock was insufficient and an order became a backorder. Which level, and why not Error?
4. What is the practical difference between `Log.Error(ex, "msg")` and `Log.Error("msg: " + ex.Message)`?
5. A fulfillment failure is logged by the service *and* by two callers up the stack. What rule is broken?

**Answers:** (1) Interpolation bakes the value into a unique string — no `OrderId` property to index or
query, and every message is distinct so nothing groups. (2) After `app.Run()` returns; without it,
buffered file writes are lost on shutdown and the log ends mid-line exactly when you need the ending.
(3) Warning — business-notable but the designed path executed; Error is reserved for the code or its
dependencies failing. (4) The first attaches the full exception (type, stack trace) as structured data;
the second flattens it to prose and loses the stack. (5) Log at the point of decision — one event per
decision, not one per stack frame.

## Summary
- One configuration at startup (console + rolling file), `UseSerilog()` to capture `ILogger<T>`,
  `CloseAndFlush()` after `app.Run()`.
- Message templates with named properties — never interpolate — so logs are queryable data, not prose.
- Tiers by meaning: Information = expected outcome, Warning = notable business event (backorder, bad SKU),
  Error = failure with the exception attached.
- Log decisions where they are made; keep hot paths quiet; be able to state your tier scheme in one
  sentence.

## Resources
- [Serilog documentation](https://serilog.net/)
- [Serilog message templates](https://messagetemplates.org/)
- [Structured logging concepts (Serilog wiki)](https://github.com/serilog/serilog/wiki/Structured-Data)
