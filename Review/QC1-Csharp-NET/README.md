# QC-1 (.NET) Review Package

Exam-prep material for **QC-1 (.NET)**, covering Git, Agile/SDLC, and C#/.NET. This package
**synthesizes** content that was already taught — it does not introduce new topics. Every entry traces
to a real source file (cited inline). Material is scoped to what has actually been delivered in
**Weeks 1–2**; rubric items scheduled for later weeks appear only in the out-of-scope register and the
checklist's "Not yet covered" section.

## Who this is for

Trainees in batch TRNG-00002442 preparing to sit QC-1 at the end of Week 2. Use it to find your gaps,
review the concepts, drill the syntax, and rehearse interview-style answers.

## QC-to-week mapping

| QC exam | Feeder weeks (`weeklytechrepo/`) | Trainer notes (`trainer-code/Notes/`) |
|---|---|---|
| QC-1 (.NET) | `Agile-Git-CoreCSharp` (Week 1), `Intermediate-CSharp` (Week 2) | `C-Sharp/Intro-OOP`, `C-Sharp/Intermediate-C#` |

Coverage was resolved against `docs/status/QC1-Coverage-Analysis.md` (the existing taught-vs-out-of-scope
map for QC-1).

## What each file is

| File | What it gives you |
|---|---|
| `self-assessment-checklist.md` | Every QC-1 objective, verbatim, as a `Can I ...?` checkbox grouped by tier and topic. Your gap finder. Includes a "Not yet covered" section for later-week items. |
| `study-guide.md` | Per topic cluster: the objectives covered, a concept recap with source pointers, key pitfalls, and one annotated worked example. |
| `cheat-sheet.md` | Dense syntax/command/comparison tables per topic. Skimmable the morning of the exam. |
| `drills.md` | Short hands-on tasks per topic. Prompts are domain-neutral (do them in your own domain); model solutions use the trainer Library domain. |
| `mock-interview.md` | A question bank by topic — technical plus behavioral (SDLC/Agile/Git) — each with a model answer, the QC objective it proves, and a source. |
| `out-of-scope-register.md` | QC-1 items the rubric lists but Weeks 1–2 do not teach, with the week each is scheduled for. |

## How to study with this

1. **Checklist first.** Open `self-assessment-checklist.md` and honestly tick what you can already do.
   Every unticked box is a target.
2. **Study guide next.** For each gap, read the matching cluster in `study-guide.md`. Follow the source
   pointers if you want the full lesson.
3. **Drill it.** Do the matching task in `drills.md` in your own domain (not Library) before peeking at
   the solution.
4. **Cheat-sheet to consolidate.** The night before / morning of, skim `cheat-sheet.md` to refresh
   syntax.
5. **Mock last.** Run through `mock-interview.md` out loud, then check your answer against the model.

## Scope note

Five rubric items (4 Must, 1 Should) are **not taught in Weeks 1–2** — unit-test/AAA, sorting algorithms
(describe and implement), Big-O / asymptotic notation, and SOA/microservices. They are taught in their
scheduled weeks (4, 5, 7, 8). They are listed in `out-of-scope-register.md` and flagged in the checklist,
but carry no study text, drill, or interview question here. If QC-1 is sat at the end of Week 2, scope the
sitting to the Week 1–2 subset or schedule the exam after the relevant weeks (see the register).
