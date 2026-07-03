# Project Runway — Order Fulfillment Service

Companion to `order-fulfillment-engine-v2.md` (the spec — it wins on any disagreement). This document
exists to get you **moving**, not to build for you. It contains **no C#, no schema, no domain logic** —
by design. Three things live here:

1. **Skeleton by CLI** — the exact tool commands that raise an empty, correctly-wired solution.
2. **Milestone ladder** — what your app can *do* after each stage, mapped to the trainer demo commit
   that shows the same stage in the Library domain.
3. **Acceptance probes** — the runnable checks that tell you a milestone is actually done.

If you can run every probe in Part 3 with the expected result, you have shipped the Floor and know
exactly which Target items remain.

---

## Part 1 — Skeleton by CLI

Everything below is tooling, not solution code — the `dotnet` CLI generates the same boilerplate for
everyone. Replace `Fulfillment` with a name from **your** domain. Run from an empty repo folder
(`FirstName-LastName` per the spec).

```bash
# Solution + the two projects (Minimal API host, EF Core class library)
dotnet new sln -n YourService
dotnet new web -n Fulfillment.Api
dotnet new classlib -n Fulfillment.Data
dotnet sln add Fulfillment.Api Fulfillment.Data
dotnet add Fulfillment.Api reference Fulfillment.Data

# Packages — data layer
dotnet add Fulfillment.Data package Microsoft.EntityFrameworkCore.SqlServer
dotnet add Fulfillment.Data package Microsoft.EntityFrameworkCore.Design

# Packages — API host
dotnet add Fulfillment.Api package Microsoft.EntityFrameworkCore.Design
dotnet add Fulfillment.Api package Serilog.AspNetCore
dotnet add Fulfillment.Api package Swashbuckle.AspNetCore

# Git hygiene
dotnet new gitignore
git init && git add -A && git commit -m "Empty wired skeleton"
```

The EF tooling (skip the install if `dotnet ef --version` already answers; safe to re-run):

```bash
dotnet tool install --global dotnet-ef
```

The database is the classroom SQL Server container from Week 3 — reuse it, point your connection
string at a database **name of your own** (do not share the trainer's `LibraryMinimalDb`):

```bash
docker start librarysqlserver
# fresh machine only:
docker run -d --name librarysqlserver -p 1433:1433 \
  -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='LibraryPass1!' \
  mcr.microsoft.com/mssql/server:2022-latest
```

Once your `DbContext` and first entities exist (that part is yours), the migration loop you will run
all week:

```bash
dotnet ef migrations add <MigrationName> --project Fulfillment.Data --startup-project Fulfillment.Api
dotnet ef database update            --project Fulfillment.Data --startup-project Fulfillment.Api
```

**Stop here and check:** `dotnet build` is green and `dotnet run --project Fulfillment.Api` serves a
page before you write any domain code. An empty app that runs beats a full app that doesn't compile.

---

## Part 2 — Milestone ladder

The trainer's Library demo is built as a git ladder — one commit per stage, in
`weeklytechrepo/EFCore-REST-SOAP/demo/library-fulfillment/` (flat end-of-Week-4 snapshot:
`demo/library-fulfillment-minapi/`). Your build mirrors the **stages**, in **your** domain, with
**your** names. "Mirror" means: after your version of stage N, your app can do what the checklist
says — not that your code looks like the demo's.

Do the stages **in order**. Each one is a natural commit in your repo.

### M0 — Skeleton (Part 1 above)

- [ ] Solution builds; empty Minimal API runs; git history started.

### M1 — Data in, data out — mirrors demo `01-efcore-minimal-api`

- [ ] Your entities exist as a code-first EF model; the first migration created your schema in SQL Server.
- [ ] `DbContext` registered in DI (never `new`-ed in `Program.cs` top-level code).
- [ ] One GET endpoint returns real rows from the database.
- **Probe P1 partially passes** (inventory listing; seeding may still be manual).

### M2 — Seed, reset, report — mirrors demo `02-persistence-reports`

- [ ] Migration-time seed (catalog **and** customers) plus a reset endpoint that restores baseline stock.
- [ ] At least one LINQ report endpoint (grouping/aggregation, not a raw table dump).
- [ ] Fluent API and Data Annotations both in use; at least one non-key index you can justify.
- **Probes P1 and P7 pass.**

### M3 — The concurrent core — mirrors demo `03-concurrent-fulfillment`

The hard one; everything else is decoration around it.

- [ ] Concurrency token (`RowVersion`) on the inventory row.
- [ ] One order fulfilled correctly: own `DbContext` (factory), one transaction, decrement + status +
      audit event land atomically.
- [ ] Race loser catches the concurrency exception, reloads, re-checks, retries (bounded) or backorders.
- [ ] Burst endpoint: fan-out over the single-order path, returns immediately, drains on a background
      task that survives the request ending.
- [ ] Serilog structured stream shows fulfilled/backordered per order.
- **Probes P2 and P3 pass. This is the Floor — a submission frozen here passes.**

### M4 — Priority + benchmark (Target) — mirrors demo `04-priority-benchmark`

- [ ] Expedited-first via `PriorityQueue<T>` (or a justified two-lane equivalent).
- [ ] Sequential-vs-parallel benchmark with stock reset between runs, both timings + speedup printed.
- **Probes P4 and P5 pass.**

### M5 — Resilience + observability (Target) — mirrors demo `05-resilience-observability`

- [ ] Graceful stop: cancellation honored, no half-applied order, logs flushed on exit.
- [ ] Severity tiers in the log; a custom exception that carries data, caught specific-before-base.
- **Probe P6 passes. Full Target.**

Demo stages `06`–`08` (controllers, SOAP, middleware) are **Project 2 material — not this build**.

---

## Part 3 — Acceptance probes

Runnable versions of the spec's acceptance criteria — the same sequence as the Friday live demo, so
practicing the probes *is* rehearsing the presentation. Route names below are the spec's suggestions;
yours may differ, the **semantics** may not. Run them with curl, Postman, a `.http` file, or Swagger.

Convention: `STOCK` = total units your seed creates (know this number cold); `N` = a burst size
comfortably larger than `STOCK`.

### P1 — Seed and inspect

```bash
curl -X POST http://localhost:5000/seed
curl http://localhost:5000/inventory
```

**Pass:** every product appears with its baseline quantity; running the pair again restores the same
baseline (reset is repeatable, not additive).

### P2 — Burst without blocking

```bash
curl -X POST "http://localhost:5000/orders/burst?n=N"
curl http://localhost:5000/inventory        # immediately after, while the burst drains
```

**Pass:** the burst call returns at once (202-shaped, not after N orders complete); the inventory call
answers mid-burst; the log streams per-order outcomes while both happen.

### P3 — No oversell (the headline)

After a P2 burst where `N` demanded more units than `STOCK`:

```bash
curl http://localhost:5000/inventory
```

**Pass, all three, every run:**
- no on-hand quantity is negative;
- units fulfilled `==` units depleted (`STOCK` minus final on-hand equals the sum fulfilled);
- every order is terminal — Fulfilled or Backordered, nothing Pending, nothing partial.

Prove it however you like — a verify endpoint (the trainer demo's shape), a report, or a SQL query you
paste into the README — but it must be checkable on demand, not asserted by you.

### P4 — Expedited first (Target)

Submit a **mixed** wave (expedited + normal queued together), then compare completion order/timestamps.

**Pass:** expedited orders reach a terminal state before normal ones.

### P5 — Benchmark (Target)

```bash
curl -X POST http://localhost:5000/benchmark
```

**Pass:** stock is reset between the two runs; both timings and a speedup factor are reported; parallel
wins (or your README explains why it didn't on your machine).

### P6 — Graceful stop (Target)

Start a large burst, then `Ctrl+C` the app mid-drain. Restart and inspect.

**Pass:** no order is half-applied (inventory decremented but order not marked, or vice versa); the log
file is complete through shutdown.

### P7 — Reports

```bash
curl http://localhost:5000/reports/top-products
```

**Pass:** ranked output is actually sorted; two different runs produce different results; you can look
up one product's rank quickly (your binary search's job).

---

**Friday self-check:** P1 → P2 → P3 → P5 in a row, cold start, one take. If that works unrehearsed,
the demo will too.
