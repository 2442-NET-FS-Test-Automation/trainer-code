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
`content/07-soap/soap-vs-rest.md`; its demo (`08-soap`) did not run Tuesday July 14 — the slot is
floating (any Wk6 PM, else Wk7). Note coverage carries the row either way (Nice tier, no R-002
exposure); the demo, when it lands, adds a live envelope round-trip on the same Library service.

## Notes-relegated DSA remainder (context, not a gap)

The `02-dsa-complete` demo opener never ran in class (user decision 2026-07-02). Its material — sound
binary search, measured Big-O timing, recursion + memoization, PriorityQueue, reflection — is covered by
the `content/03-dsa/` notes plus the runnable answer-key commit `02-dsa-complete` (`c70c979`) in
`weeklytechrepo/EFCore-REST-SOAP/demo/algorithms-threading-demo/`. The surviving rubric rows it grounds
(binary search Must, Big-O Must, stack/queue/priority-queue Should, arrays/linked-lists/hash-tables
Should) are note-and-answer-key grounded — study them from the notes, then read the answer-key code.
(The recursion/memoization and merge-sort rows were cut from the rubric 2026-07-15 —
`docs/status/QC3-QC4-CUT-ADJUDICATION-2026-07-15.md`; the notes remain for interview prep.)

## Future-week items

No objective in **either rubric** is scheduled for a later week, so nothing from the 67 rows is
anti-spoilered out of this package. (Frontend/React topics belong to QC-5 and appear nowhere here.)

Carried items from the QC-1 register (`docs/status/QC1-Coverage-Analysis.md` section 5) that are
**deferred to a later week** rather than absorbed here:

| Carry item | Disposition |
|---|---|
| AAA | Deferred to a later week (user call 2026-07-13). Not a row in the QC-3 or QC-4 rubric, so it gets no study material in this package; it lands with a future week's material and whatever future evaluation covers it. |
| SOA | Its absorbing row (QC-3 REST Should) was **cut 2026-07-15** — the topic never appeared on the delivered curricula (Microservices is Wk7). Re-registered as a future-week item; natural home = the Wk7 microservices material. Written coverage exists (`content/02-rest-http/rest-principles.md`). |
| Sorting (describe + implement) | Its absorbing rows (QC-3 DSA sorting Must/Should/Nice) were **cut 2026-07-15** — sorting never appeared on the delivered curricula. Taught material stays (`content/03-dsa/sorting.md`, `Sorts.cs` answer key); lands with whatever future evaluation covers it. |

The remaining QC-1 carry item, Big-O, stays absorbed: its QC-3 DSA Must row survives the cut. Cut
adjudication record: `docs/status/QC3-QC4-CUT-ADJUDICATION-2026-07-15.md`.
