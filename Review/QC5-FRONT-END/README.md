# QC-5 (Front End) Review Package

Exam-prep material for the **QC-5 Front End** assessment — **Thursday July 30, AM block (Week 8)**. This
package **synthesizes** content that was already taught in Weeks 6 and 7; it introduces no new topics.
Every entry traces to a real source file, cited inline.

The rubric (`qc-criteria/QC-5-Front-End.md`) is **99 objectives** across five sections:

| Section | Must | Should | Nice | Total |
|---|---|---|---|---|
| HTML/CSS | 10 | 2 | 2 | 14 |
| JS Language | 12 | 6 | 3 | 21 |
| Browser Based JS | 10 | 3 | 6 | 19 |
| TypeScript | 5 | 5 | 2 | 12 |
| React | 17 | 10 | 6 | 33 |
| **Total** | **54** | **26** | **19** | **99** |

*(The rubric was trimmed against the actual portal curricula on 2026-07-09 — 11 rows cut, 4 downgraded
Must/Should to Nice; adjudication record: `docs/status/QC5-CUT-ADJUDICATION-2026-07-09.md`. The cut rows
are listed in `out-of-scope-register.md` so you know what is **not** on the paper.)*

## Who this is for

Trainees in batch TRNG-00002442 preparing to sit QC-5 after Weeks 6-7. Use it to find your gaps, review
the concepts, drill the syntax, and rehearse interview-style answers. The trainer demos use a **Library**
domain (the `frontend-demo` and `react-spa-demo` threads); the drill prompts are domain-neutral so you
can mirror them in **your own** domain — including your Project 2 app.

## QC-to-week mapping

| Rubric section | Taught | Feeder demo rungs |
|---|---|---|
| HTML/CSS | Wk6 Mon | `01-html-css` |
| JS Language | Wk6 Tue (core) + Wed (in the page) | `02-js-page` |
| Browser Based JS | Wk6 Wed | `02-js-page`, `03-http-fetch` |
| TypeScript | Wk6 Thu (basics) + Fri (advanced) | `04-ts-basics`, `05-ts-advanced` |
| React | Wk7 Mon-Wed | `06-vite-components`, `07-hooks-axios`, `08-router`, `09-auth-context`, `10-advanced` |

Source roots used throughout:

- **Concept notes** — `weeklytechrepo/Frontend-React/content/{01-html,02-css,03-javascript,04-typescript,05-react}/`
  (per-row coverage map: `weeklytechrepo/Frontend-React/content/README.md`)
- **Demo scripts** — `weeklytechrepo/Frontend-React/demo/walkthroughs/` (series READMEs: `README.md`
  for Wk6 / `README-react.md` for Wk7)
- **End-state code (answer keys)** — `weeklytechrepo/Frontend-React/demo/frontend-demo/` (HTML/CSS/JS
  pages plus the plain-TypeScript client in `ts/`) and
  `weeklytechrepo/Frontend-React/demo/react-spa-demo/` (the React SPA over the live
  `Library.ControllerApi`)

## What each file is

| File | What it gives you |
|---|---|
| `self-assessment-checklist.md` | All 99 objectives, verbatim, as checkboxes grouped by tier and section. Your gap finder. |
| `study-guide.md` | Per topic cluster: the objectives covered, a concept recap with source pointers, key points and pitfalls, and one annotated worked example from the demos. |
| `cheat-sheet.md` | Dense syntax and comparison tables per topic. Skimmable the morning of the exam. |
| `drills.md` | Short hands-on tasks per topic. Prompts are domain-neutral (do them in your own domain); model solutions use the trainer Library domain. |
| `mock-interview.md` | A question bank by topic, each with a tier badge, model answer, the QC objective it proves, and a source. |
| `out-of-scope-register.md` | Coverage-defect section (empty — that is the healthy state), the written-coverage-only Nice rows, and the rows cut from the rubric. |

## How to study with this

1. **Checklist first.** Open `self-assessment-checklist.md` and honestly tick what you can already do
   unaided. Every unticked box is a target.
2. **Study guide next.** For each gap, read the matching cluster in `study-guide.md`. Follow the source
   pointers if you want the full lesson.
3. **Drill it.** Do the matching task in `drills.md` in your own domain (not Library) before looking at
   the model solution.
4. **Cheat-sheet to consolidate.** The night before and the morning of, skim `cheat-sheet.md`.
5. **Mock last.** Work through `mock-interview.md` out loud, then compare against the model answer. Say
   the answer before you read it — recognition is not recall.

Budget your time by weight: React is a third of the paper (33 rows), and JS Language plus Browser JS
together are another 40. HTML/CSS and TypeScript are 26 between them. If you are short on time, the
highest-yield order is React, Browser JS, JS Language, TypeScript, HTML/CSS.

## Scope note

All **Must-know and Should-know** objectives were taught by end of Week 7, with a live demo beat for each
— verified at the Week 7 gate (2026-07-24) for the React section and by
`docs/status/QC5-P2-GAP-REANALYSIS-2026-07-09.md` for the Weeks-6 sections. There are **no coverage
defects**. Five Nice-to-have rows are written-coverage-only (notes, no live demo beat) and one Nice row
group landed only partially in the room; see `out-of-scope-register.md`.
