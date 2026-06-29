# QC-2 (SQL) — Out-of-Scope Register

This package covers the **entire** QC-2 rubric — all 46 objectives (22 Must-know, 16 Should-know, 8
Nice-to-have). **Nothing is out of scope.** Every objective has a checklist box, study-guide treatment,
cheat-sheet entry, drill, and interview question.

Coverage was derived by walking each QC-2 objective in `qc-criteria/QC-2-SQL.md` to a supporting artifact
under `weeklytechrepo/SQL/**`.

## Awareness-level (note-only, not live-demoed)

Two **Nice-to-have** objectives are taught at **awareness level in the content notes** but are **not**
hand-coded in the live `sql-training` demo thread — by design, per the demo series anti-spoiler ledger
(`weeklytechrepo/SQL/demo/walkthroughs/README.md`):

| QC-2 objective (verbatim) | Tier | Where it's covered |
|---|---|---|
| Understand and know how to utilize Window Functions | Nice to Have | `content/4-Thursday/functions.md` (awareness subsection); study guide §11; cheat-sheet; Drill 19; mock-interview |
| Utilize Common Table Expressions (CTEs) | Nice to Have | `content/4-Thursday/joins.md` (awareness subsection); study guide §11; cheat-sheet; Drill 20; mock-interview |

"Awareness level" means: recognize the shape (`OVER (PARTITION BY ... ORDER BY ...)`, `WITH name AS (...)`)
and how each differs from its sibling (`GROUP BY`, a subquery) — not deep/timed mastery. They are folded
into the Thursday notes next to their closest contrast (window functions beside `GROUP BY`; CTEs beside
subqueries), exactly as the other Nice-to-have items are folded in. The live demo still defers them to keep
the week focused on the load-bearing relational skills (DDL through transactions and packaging). Including
them does **not** violate the anti-spoiler rule: neither topic is scheduled in any later week, so there is
no future lesson to spoil.

## Action for the QC-2 owner

QC-2 can be administered against the **full rubric** after Week 3 with no scheduling caveat. The only
nuance to flag: window functions and CTEs are taught at **awareness level** (in the notes, not the live
demo), so a trainee who only attended the demos meets them first in the notes/this package. If the sitting
expects deep/timed mastery of `OVER(...)` window functions and `WITH ... AS` CTEs, allocate a short
supplementary block — they are Nice-to-have and were originally outside the curriculum inventory.

## Note on set operations

`UNION`/`UNION ALL` (and `INTERSECT`/`EXCEPT`) — the QC-2 Nice-to-have "Utilize set operations between
multiple select statement" — are taught (awareness level) in
`weeklytechrepo/SQL/content/4-Thursday/joins.md` (Drill 14, a cheat-sheet entry, and an interview
question). Like window functions and CTEs, they are awareness-level, not deep-mastery, this week.
