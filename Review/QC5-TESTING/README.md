# QC-6 Test Automation — Review Package

Exam-prep package for the QC-6 (Test Automation) competency exam. The portal names the sitting
"Selenium/Cypress"; the rubric (`qc-criteria/QC-6-Test-Automation.md`) is wider — five sections,
92 objectives (46 Must / 30 Should / 16 Nice) — and this package covers all five. **Prepare to
the rubric, not the name.**

**Who it is for:** every trainee sitting QC-6 — and, this cohort, everyone walking into final
interviews (Mon–Wed Aug 10–12): the same material is the interview surface, so this package
was shipped ahead of interview week. The exam itself sits in Week 10 (date announced at Week 10
planning).

**Sitting scope note:** cloud and CI/CD content lands in Week 10, after this exam's material
window. The one rubric row it touches — Cypress CI/CD integration (Must) — is examined at
awareness depth; the register carries the record.

## What each file is

| File | What it is | When to use it |
|---|---|---|
| `self-assessment-checklist.md` | Every rubric objective verbatim as a checkbox, grouped by section and tier | **Start here.** Anything unchecked routes your study |
| `study-guide.md` | Five clusters: objectives, concept recap with source pointers, pitfalls, one worked example each | The map back into the notes for whatever the checklist surfaced |
| `drills.md` | Hands-on tasks (domain-neutral) with Library-domain model solutions | After reading — doing beats rereading; drills double as P3 progress |
| `mock-interview.md` | Tier-badged question bank with spoken-length model answers, each tracing to a QC objective and a source | Interview week + the night before the sitting; answer out loud first |
| `cheat-sheet.md` | Dense tables and minimal code per topic | The morning of |
| `out-of-scope-register.md` | Coverage defects (ledgered), untaught Nice rows, and Week-10 material excluded by the anti-spoiler rule | So you know what is legitimately NOT on the table |

## How to study with this

1. **Checklist first** — one honest pass; mark what you cannot say out loud.
2. **Study guide** for each unchecked cluster; follow its source pointers into the notes —
   every note carries "Say It in an Interview" and "Check Yourself" sections that go deeper
   than this package.
3. **Drills** for anything you have read twice but never typed. Do them in your own P3 domain.
4. **Mock interview** out loud — alone, then with a teammate asking follow-ups.
5. **Cheat sheet** on exam morning, not instead of the above.

Your strongest asset is not in this folder: it is your P3 artifact set (RTM, technique-named
cases, hunting record, both E2E suites, the Cypress-vs-Selenium comparison). The Designing
section of the rubric is examined *through* work shaped exactly like it — speak from your own
artifacts wherever you can.

## Exam-to-week mapping

| Rubric section | Feeder |
|---|---|
| 1. Testing Philosophy | Wk8 Mon theory block; `content/01-xunit/testing-fundamentals.md` |
| 2. Designing Test Cases | `content/01-xunit/test-case-design.md`; EP/BVA in `xunit-fundamentals.md` + walkthrough `01` Step 6; applied via the P3 required set (`project/p3-test-suites.md` + `p3-design-artifacts-walkthrough.md`) |
| 3. Testing and Logging .NET | Wk8 Mon–Tue (`01`/`02` walkthroughs, four xUnit-cluster notes); cross-week: file I/O (Wk1 `os-cli-file-io.md`), JSON deserialization (Wk2 `async-http-networking.md`), Serilog (Wk4–5 `serilog-structured-logging.md`) |
| 4. Cypress | Wk8 Wed–Thu + Wk9 Mon (`03`/`04`/`06` walkthroughs; `content/02-cypress/` notes; suites in `react-spa-demo/cypress/`) |
| 5. Selenium | Wk8 Fri + Wk9 Mon–Thu (`05`/`07`/`08`/`09` walkthroughs; `content/03-selenium/` notes x7; suite in `react-spa-demo/e2e-selenium/`) |

Coverage accounting (every Must/Should row -> named source or dated ledger entry):
`docs/status/2026-08-06-QC6-gate-coverage-checklist.md`.
