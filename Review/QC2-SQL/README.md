# QC-2 (SQL) Review Package

Exam-prep material for **QC-2 (SQL)**, covering the relational model, DDL/DML/DQL, keys and
normalization, joins and aggregates, transactions/ACID, and views/procedures/triggers. This package
**synthesizes** content that was already taught — it does not introduce new topics. Every entry traces
to a real source file (cited inline). It covers the **entire** QC-2 rubric — every objective is delivered
in **Week 3 (Mon Jun 22 – Fri Jun 26)** (window functions and CTEs at awareness level in the Thursday
notes), so nothing is out of scope.

## Who this is for

Trainees in batch TRNG-00002442 preparing to sit QC-2 after the SQL week. Use it to find your gaps, review
the concepts, drill the syntax, and rehearse interview-style answers. The trainer demos use a **Library**
domain (`sql-training` thread); you mirror the same ideas in your **own** domain.

## QC-to-week mapping

| QC exam | Feeder week (`weeklytechrepo/`) | Trainer notes (`trainer-code/Notes/`) |
|---|---|---|
| QC-2 (SQL) | `SQL` (Week 3, Mon–Fri) | `SQL/SQL-Intro`, `SQL/SQL-Intermediate` |

The week's epic is **model -> create -> populate/query -> relate/normalize -> join/aggregate -> protect &
package**, one demo commit per topic on the `sql-training` thread (`00-intro` -> `06-views-procs`). Coverage
was derived by walking each QC-2 objective to its supporting artifact under `weeklytechrepo/SQL/**`.

## What each file is

| File | What it gives you |
|---|---|
| `self-assessment-checklist.md` | Every QC-2 objective, verbatim, as a checkbox grouped by tier and topic. Your gap finder. |
| `study-guide.md` | Per topic cluster: the objectives covered, a concept recap with source pointers, key pitfalls, and one annotated worked example (Library domain). |
| `cheat-sheet.md` | Dense syntax/comparison tables and SQL blocks per topic. Skimmable the morning of the exam. |
| `drills.md` | Short hands-on tasks per topic. Prompts are domain-neutral (do them in your own domain); model solutions use the trainer Library domain. |
| `mock-interview.md` | A question bank by topic, each with a model answer, the QC objective it proves, and a source. |
| `out-of-scope-register.md` | Confirms full rubric coverage, and notes the two items (window functions, CTEs) covered at awareness level in the Thursday content notes. |

## How to study with this

1. **Checklist first.** Open `self-assessment-checklist.md` and honestly tick what you can already do.
   Every unticked box is a target.
2. **Study guide next.** For each gap, read the matching cluster in `study-guide.md`. Follow the source
   pointers if you want the full lesson.
3. **Drill it.** Do the matching task in `drills.md` in your own domain (not Library) before peeking at the
   solution. Run it against SQL Server (see `weeklytechrepo/SQL/demo/sql-training/docs/docker-setup.md`).
4. **Cheat-sheet to consolidate.** The night before / morning of, skim `cheat-sheet.md` to refresh syntax.
5. **Mock last.** Run through `mock-interview.md` out loud, then check your answer against the model.

## Scope note

This package covers the **entire** QC-2 rubric — all 46 objectives across Must, Should, and Nice-to-have,
all grounded in the Week-3 teaching content (`weeklytechrepo/SQL/**`). Two Nice-to-have items — **Window
Functions** and **Common Table Expressions (CTEs)** — are taught at **awareness level** in the Thursday
content notes (`content/4-Thursday/functions.md` and `joins.md`): recognize the shape and how each differs
from its sibling (`GROUP BY`, a subquery), rather than deep/timed mastery. Their entries are labelled
"awareness level" accordingly. See `out-of-scope-register.md` for the coverage detail.
