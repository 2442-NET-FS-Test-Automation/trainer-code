# QC-3 + QC-4 Combined Review Package

Exam-prep material for the **combined QC-3 (EF Core, DSA, REST, C# Multithreading) + QC-4 (ASP.NET Core)
sitting — one exam, Friday July 17, full AM block**. This package **synthesizes** content that was already
taught — it does not introduce new topics. Every entry traces to a real source file (cited inline). It
covers the union of both rubrics — **78 objectives** (63 QC-3 + 15 QC-4) across five sections: REST, DSA,
EF Core, C# Multithreading, and ASP.NET Core.

The two rubrics remain separate canonical files (`qc-criteria/QC-3-EF-Core-REST-DSA-MT.md` and
`qc-criteria/QC-4-ASP-NET-CORE.md`); the trainer's merged exam paper
(`trainer-code/Criteria/QC-3-EF-REST-DSA-MT-ASP.md`) is row-identical to their union, so studying this
package covers the merged paper exactly.

## Who this is for

Trainees in batch TRNG-00002442 preparing to sit the combined QC-3 + QC-4 exam after Weeks 4-5. Use it to
find your gaps, review the concepts, drill the syntax, and rehearse interview-style answers. The trainer
demos use a **Library** domain (the `library-fulfillment` and `algorithms-threading-demo` threads); you
mirror the same ideas in your **own** domain.

## QC-to-week mapping

| QC exam | Sections | Feeder weeks (`weeklytechrepo/EFCore-REST-SOAP/`) | Trainer notes (`trainer-code/Notes/`) |
|---|---|---|---|
| QC-3 (EF/REST/DSA/MT) | REST, DSA, EF Core, C# Multithreading | Weeks 4-5 (`01`-`05` + `dsa-01`/`dsa-02` demos) | `EFCore/`, `DSA/`, `Multithreading/`, `ASP.NET/REST-HTTP/` |
| QC-4 (ASP.NET Core) | ASP.NET Core | Week 5 (`06a`/`06b` controller bridge + `07` cross-cutting) | `ASP.NET/ASPNET-CORE/`, `ASP.NET/Security/` |

Source roots used throughout:

- Content notes: `weeklytechrepo/EFCore-REST-SOAP/content/{01-efcore,02-rest-http,03-dsa,04-multithreading,06-aspnet-core,07-soap,08-security}/`
  (coverage map: `weeklytechrepo/EFCore-REST-SOAP/content/README.md`)
- Demo scripts: `weeklytechrepo/EFCore-REST-SOAP/demo/walkthroughs/`
- End-state code (answer keys): `weeklytechrepo/EFCore-REST-SOAP/demo/library-fulfillment/` (the
  `Library.Api` Minimal API + `Library.ControllerApi` controller API + shared `Library.Data`) and
  `weeklytechrepo/EFCore-REST-SOAP/demo/algorithms-threading-demo/DsaThreading/` (DSA + threading console)

## What each file is

| File | What it gives you |
|---|---|
| `self-assessment-checklist.md` | Every objective from both rubrics, verbatim, as a checkbox grouped by tier and section. Your gap finder. |
| `study-guide.md` | Per topic cluster: the objectives covered, a concept recap with source pointers, key pitfalls, and one annotated worked example (Library domain). |
| `cheat-sheet.md` | Dense syntax/comparison tables per topic. Skimmable the morning of the exam. |
| `drills.md` | Short hands-on tasks per topic. Prompts are domain-neutral (do them in your own domain); model solutions use the trainer Library domain. |
| `mock-interview.md` | A question bank by topic, each with a model answer, the QC objective it proves, and a source. |
| `out-of-scope-register.md` | Confirms rubric coverage and lists the waived/awareness-level Nice-to-have items. |

## How to study with this

1. **Checklist first.** Open `self-assessment-checklist.md` and honestly tick what you can already do.
   Every unticked box is a target.
2. **Study guide next.** For each gap, read the matching cluster in `study-guide.md`. Follow the source
   pointers if you want the full lesson.
3. **Drill it.** Do the matching task in `drills.md` in your own domain (not Library) before peeking at
   the solution.
4. **Cheat-sheet to consolidate.** The night before / morning of, skim `cheat-sheet.md` to refresh syntax.
5. **Mock last.** Run through `mock-interview.md` out loud, then check your answer against the model.

## Scope note

All **Must-know and Should-know** objectives of both rubrics were taught by end of Week 5 (QC-3
Must/Should by end of Week 4; QC-4 by end of Week 5). Three Nice-to-have items carry user-signed demo
waivers and are covered in the written notes only; the REST-vs-SOAP comparison is note-covered with its
demo (`08-soap`) scheduled Tuesday July 14, before the sitting. See `out-of-scope-register.md` for the
detail. There are **no coverage defects**.
