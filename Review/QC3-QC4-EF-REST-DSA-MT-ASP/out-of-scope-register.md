# QC-3 + QC-4 — Out-of-Scope Register

## COVERAGE DEFECTS

**None.** Every Must-know and Should-know objective of both rubrics maps to a taught artifact:

- QC-3 Must/Should (REST, DSA, EF Core, Multithreading) — complete by end of Week 4 (Week-4 Log
  coverage note, `docs/status/CMA-EFCore-REST-SOAP-Log.md`); the REST "build a RESTful web service"
  Should was extended to the controller stack in Week 5 (`06a`/`06b`).
- QC-4 Must/Should (ASP.NET Core) — complete by end of Week 5 (`06a`/`06b` bridge + the Minimal API
  taught across `01`-`04`; `docs/status/CMA-ASPNET-WebAPI-Log.md`).

Coverage was derived by walking each objective of `qc-criteria/QC-3-EF-Core-REST-DSA-MT.md` and
`qc-criteria/QC-4-ASP-NET-CORE.md` to its supporting artifact via the coverage map
`weeklytechrepo/EFCore-REST-SOAP/content/README.md`.

## Register: demo-waived Nice-to-have items (written notes only)

Three **Nice-to-have** objectives carry a user-signed demo waiver (2026-07-02, governance-logged per
R-002). Each is covered **in writing** in the content notes; none was live-demoed. They still get
checklist boxes and (except profiling) study material grounded in the notes:

| Objective (verbatim) | Tier | Waiver depth | Written coverage |
|---|---|---|---|
| Call stored procedures and query  scalar types by dropping down to SQL, using FromSQL() and SqlQuery(). | Nice to Have | Named only in demos | `content/01-efcore/loading-strategies-raw-sql.md` |
| Modify a migration created by Entity Framework before execution. | Nice to Have | Conceptual only | `content/01-efcore/code-first-data-first.md` |
| Profile and analyze thread performance using Visual Studio diagnostics or similar tools. | Nice to Have | Not live-demoed | `content/04-multithreading/debugging-profiling.md` |

The profiling row is **register-only** in this package: no drill and no worked example (nothing was
typed in class to cite); the note carries the written treatment, and the cheat-sheet names the tools.

## Scheduling note: REST vs SOAP (Nice-to-have)

"Compare and contrast RESTful and SOAP-based web services..." is covered in writing in
`content/07-soap/soap-vs-rest.md`, and its demo (`08-soap`) is scheduled **Tuesday July 14 PM** — before
the Friday sitting. Study material in this package is grounded in the note; the demo adds a live
envelope round-trip on the same Library service.

## Notes-relegated DSA remainder (context, not a gap)

The `02-dsa-complete` demo opener never ran in class (user decision 2026-07-02). Its material — sound
binary search, measured Big-O timing, recursion + memoization, PriorityQueue, reflection — is covered by
the `content/03-dsa/` notes plus the runnable answer-key commit `02-dsa-complete` (`c70c979`) in
`weeklytechrepo/EFCore-REST-SOAP/demo/algorithms-threading-demo/`. The affected rubric rows (binary
search Must, Big-O Must, priority-queue Should, arrays/linked-lists/hash-tables Should, the four
recursion/memoization and merge-sort Nice rows) are all note-and-answer-key grounded — study them from
the notes, then read the answer-key code.

## Future-week items

No objective in **either rubric** is scheduled for a later week, so nothing from the 78 rows is
anti-spoilered out of this package. (Frontend/React topics belong to QC-5 and appear nowhere here.)

One carried item from the QC-1 register (`docs/status/QC1-Coverage-Analysis.md` section 5) is
**deferred to a later week** rather than absorbed here:

| Carry item | Disposition |
|---|---|
| AAA | Deferred to a later week (user call 2026-07-13). Not a row in the QC-3 or QC-4 rubric, so it gets no study material in this package; it lands with a future week's material and whatever future evaluation covers it. |

The other QC-1 carry items are absorbed by this sitting's rubrics: Big-O and sorting land in the
QC-3 DSA rows, SOA in the QC-3 REST Should row — all covered above.
