# QC-6 Test Automation — Study Guide

Five topic clusters, matching the five rubric sections of `qc-criteria/QC-6-Test-Automation.md`.
Each cluster lists the objectives it serves (by tier), a concept recap with source pointers, key
pitfalls, and one annotated worked example from the taught material. The deep treatment lives in
the source notes — this guide is the map, not the territory. Read the note whenever a recap
sentence is not enough to *say it out loud*.

Sources root: `weeklytechrepo/Test-Automation/` (notes under `content/`, scripts under
`demo/walkthroughs/`). Demo code: xUnit suites in
`weeklytechrepo/EFCore-REST-SOAP/demo/library-fulfillment/tests/`, Cypress + Selenium suites in
`weeklytechrepo/Frontend-React/demo/react-spa-demo/` (`cypress/`, `e2e-selenium/`).

---

## Cluster 1 — Testing Philosophy

**Objectives served:** all 6 Must, all 4 Should, both Nice rows of the Testing Philosophy section.

**Primary source:** `content/01-xunit/testing-fundamentals.md` — read it in full; it was written
to interview-pass depth for exactly these rows, and its "Say It in an Interview" section is the
spoken-answer bank for this cluster.

### Concept recap

- **What testing is / why it matters** — evidence that the system meets expected behavior and that
  change did not break what worked. Three whys: catch defects early (cost curve), enable change
  (refactor without fear), document behavior (a well-named test is an executable spec). Testing
  shows the *presence* of defects, never their absence.
- **The testing process** (plan, design cases, prepare data/environment, execute, report and
  triage, close) is a loop, not a line — a defect found in execution can reopen design. Know where
  automation enters each stage (the note has the table): automation owns *execute*, seeding, and
  reporting; scope, technique choice, and triage stay human.
- **Testing principles** — seven, each a decision rule: presence-not-absence, exhaustive testing
  impossible, early testing ("shift left"), defects cluster, pesticide paradox, context dependence,
  absence-of-defects fallacy. Be ready to say *where each bites* across the lifecycle
  (`testing-fundamentals.md`, "Testing principles, and where they bite").
- **QA vs QC** — QA is the *process* that prevents defects (proactive: definition of done, CI
  gates, reviews); QC is checking the *product* against requirements (reactive: executing cases
  against a build). One-liner: QA builds quality in; QC checks quality out. Standard follow-up:
  your automated suite's *execution* is QC; the *rule* that every PR runs it is QA.
- **Verification vs validation** — building the product *right* (vs spec: reviews, unit and
  integration tests) vs building the *right* product (vs need: UAT, beta). The note's fine-cap
  example makes this concrete.
- **Defect vs error vs failure** — cause to effect: a human **error** leaves a **defect** in the
  artifact which *may* cause a **failure** at run time. "May" is the interview point: dead-branch
  defects never fail, which is why green suites are not proof.
- **Requirements in testing** — the oracle (no stated expectation, no such thing as wrong
  behavior), the trace target of every case (rolled up: the RTM), and the acceptance gate.
  Testability is a property of the requirement itself: "fast" is untestable; "under 300 ms at p95"
  is a test.
- **Positive vs negative testing** — intent of the case, not its verdict: positive proves valid
  input produces the right result; negative proves invalid input is rejected *gracefully and
  predictably* — a negative test **passes** on the 400. Decision test for a scenario: "would a
  correct system accept this input?"
- **Automated vs manual** — automation is expensive once and nearly free per run (wins on
  repetition: regression, smoke, data-driven); manual wins on judgment (exploratory, usability)
  and one-off checks. Know the cost-curve crossover argument.
- **Tester's mindset / objectives of testing** — the note's framing: testers produce *evidence
  and risk judgment*, not just pass/fail counts; objectives span defect-finding, confidence,
  information for decisions, and defect *prevention* (reviewing requirements is testing).

### Pitfalls

- Saying "no bugs" instead of "no known defects in what we tested".
- Confusing QA/QC with job titles — they are activity kinds, not departments.
- Calling a passing negative test a failure ("it returned 400!" — that is the expected result).
- Treating verification/validation as synonyms; the fine-cap story is the cheap disambiguator.

### Worked example (annotated)

Walkthrough `01-xunit-fundamentals.md` Steps 1–2 delivered this entire cluster as a whiteboard
block (delivered Mon Jul 27, complete). The end-of-demo wrap-up questions in that script plus the
note's "Check Yourself" set are the self-test for this cluster.

---

## Cluster 2 — Designing Test Cases

**Objectives served:** all 10 Must, all 8 Should, all 4 Nice rows of the Designing section.
This is the exam's spine section, and it is examined *through your own P3 artifacts* as much as
through definitions — the P3 spec states that building its artifacts is the study guide.

**Primary sources:** `content/01-xunit/test-case-design.md` (the whole note),
`content/01-xunit/testing-fundamentals.md` (pyramid, box models, principles),
`content/01-xunit/xunit-fundamentals.md` (equivalence partitioning + boundary-value mechanics),
`weeklytechrepo/Test-Automation/project/p3-test-suites.md` +
`project/p3-design-artifacts-walkthrough.md` (your own applied practice).

### Concept recap

- **A test case is a repeatable experiment with a predicted outcome:** identifier, trace,
  preconditions, steps, expected result *written before execution*. The two skipped-but-critical
  fields: expected-result-in-advance (else it is an observation, not a test) and trace (else the
  case is unauditable). An automated test carries the same fields as name / arrange / act / assert.
- **Principles as design rules** — the same seven principles from Cluster 1, now as constraints on
  the case set: sample deliberately, weight toward churn/complexity (defects cluster), refresh the
  set (pesticide paradox), one behavior per case so a red result names one thing.
- **Test pyramid** — many unit, fewer integration, fewest E2E; realism costs speed, stability, and
  defect localization. Name the anti-pattern (ice-cream cone) and its symptoms. You lived the
  pyramid across the two weeks: xUnit units at the base, `WebApplicationFactory` integration in
  the middle, Cypress/Selenium E2E at the top.
- **Black-box / white-box application** — black-box techniques (EP, BVA, decision tables) derive
  cases from the requirement; white-box derives them from the code (branch coverage — the taught
  beat is the coverage run in walkthrough `01` Step 10 and the "coverage is a signal, not a
  target" framing in `moq-coverage-service-testing.md`).
- **Equivalence partitioning + boundary-value analysis** — mechanics in
  `xunit-fundamentals.md`; the live application is walkthrough `01` Step 6's `[Theory]` /
  `[InlineData]` rows sourced from partitions and boundaries. P3 requires each named per case.
- **Technique selection from the requirement's shape** — the lookup table in
  `test-case-design.md`: limit -> BVA; classes -> EP; combining conditions -> decision table;
  lifecycle -> state transitions; sequence -> scenario; readable internals -> branch coverage;
  then error guessing + exploratory for what the requirement never said. Requirements usually
  have more than one shape — apply all that fit.
- **Coverage-optimized design** — four moves in order: cover every requirement once before any
  twice; combine setup, never asserted behavior; push each case down the pyramid; delete
  redundancy deliberately. State the cost out loud: leaner sets diagnose worse — where defects
  cluster, un-optimize.
- **RTM** — requirements by cases; read *both* directions (empty row = coverage gap, empty column
  = waste), use rows for impact analysis on requirement change; it proves linkage, never adequacy.
  Your P3 RTM — both-direction reads, declared gaps, maintained through the sprint — is the
  concrete artifact to speak from.
- **Test data** — two separate decisions: the *objective* picks the data (smallest set that proves
  the one thing); the *object* picks its home (inline / data file / seeded store /
  created-by-case / stub-mock). The axis is determinism vs proof. Determinism by construction (no
  wall clock, no unseeded random) and cleanup in guaranteed teardown. The Cypress-side counterpart
  is `cypress-advanced.md` "Test data: choose the data, then choose its home", taught live in
  walkthrough `04` Step 1.
- **Error guessing** — a technique only when *sourced* (defect history, fragile-input classes,
  domain knowledge, churn) and *documented* (hypothesis / source / case / result one-liner) so a
  second tester can rerun it. Confirmed guesses graduate to traced regression cases. P3's hunting
  record requires 3+ sourced guesses in exactly this format.
- **Exploratory testing** — charter + timebox + session notes; findings map back three ways
  (defect vs requirement, requirements gap, surprising-but-correct). Know benefits and drawbacks
  *both directions* against scripted testing — the interview question is always comparative. P3
  requires two charter-driven sessions.
- **Stakeholder communication / data staleness / case efficacy** (Nice tier) — all three are P3
  required artifacts: the stakeholder-facing risk-and-coverage summary, the staleness/efficacy
  review. Speak from what your team wrote.

### Pitfalls

- An RTM row with one weak case is "covered" in the matrix and untested in reality — say
  "linkage, not adequacy" before the interviewer does.
- Cleanup placed after the assertions only runs on green — the failing run is exactly the one that
  leaks state. Guaranteed-teardown hooks.
- Error guessing presented as intuition. Without a source and a written hypothesis it is not a
  technique.
- Claiming EP and BVA are different activities — BVA lives on the edges of the partitions EP
  found; they are applied together.

### Worked example (annotated)

`project/p3-design-artifacts-walkthrough.md` designs two real requirements end to end — RTM
skeleton, first requirement (EP + BVA), second requirement (decision table with a deliberate
requirements-gap row), data decisions, hunting record. It teaches the method on two rows and
stops — your own P3 does the rest, which is exactly the shape an interviewer wants to hear
("here is my RTM; here is a case; here is the technique it names").

---

## Cluster 3 — Testing and Logging .NET Applications

**Objectives served:** all 6 Must, all 4 Should, both Nice rows of the .NET section.

**Primary sources:** `content/01-xunit/xunit-fundamentals.md`,
`content/01-xunit/moq-coverage-service-testing.md`, `content/01-xunit/integration-testing.md`,
`content/01-xunit/efcore-testing-strategies.md`. Cross-week:
`weeklytechrepo/Agile-Git-CoreCSharp/content/1-Thursday/os-cli-file-io.md` (file I/O),
`weeklytechrepo/Intermediate-CSharp/content/4-Thursday/async-http-networking.md`
(JSON deserialization),
`weeklytechrepo/EFCore-REST-SOAP/content/05-observability-patterns/serilog-structured-logging.md`
(logging).

### Concept recap

- **Unit tests with xUnit** — `[Fact]` for a single invariant case, `[Theory]` + `[InlineData]`
  for parameterized rows (source the rows from EP/BVA — that is the black-box bridge), AAA
  structure, FluentAssertions `Should()` chains vs bare `Assert`. Lifecycle: xUnit constructs a
  fresh test-class instance *per test* (constructor = setup, `Dispose` = teardown);
  `IClassFixture<T>` shares expensive state across a class, `ICollectionFixture<T>` across
  classes. Taught live: walkthrough `01` Steps 5–9, `02` Step 11.
- **Assertion types** — equality, null, boolean, ranges, collections, and exception assertions;
  choose the assertion that makes the *failure message* informative, not just one that goes red.
  (`xunit-fundamentals.md`, Assert vs FluentAssertions.)
- **Unit vs integration** — a unit test sees one piece in isolation (doubles for the rest); an
  integration test sees real wiring: `integration-testing.md` opens with exactly what an
  integration test can see that a unit test cannot (routing, filters, middleware, serialization,
  configuration). The taught line between them: `WebApplicationFactory<Program>` boots the real
  app in memory and you assert through HTTP (status codes, JSON bodies, validation problems) —
  walkthrough `02` Steps 1–7.
- **Mocking / test doubles** — the taxonomy (dummy / stub / fake / mock / spy) and the state-vs-
  behavior verification line: a *stub* feeds a test data, a *mock* verifies an interaction
  happened. Moq essentials: `new Mock<IDep>()`, `.Setup(...).Returns(...)`, `.Verify(...)`,
  interfaces (or virtual members) as the seam — and why sealed/static members resist mocking.
  Live: walkthrough `01` Step 8 (service tests, then the controller + `IMemoryCache` trap).
- **TDD (purpose and value)** — red-green-refactor: write the failing test first, make it pass
  with the least code, refactor under a green bar. Value: design pressure (test-first forces
  seams), executable spec, tight feedback. Also know where it does not fit (exploratory spikes,
  UI churn). Source: `xunit-fundamentals.md`, "Test-driven development: writing the test first".
  Honest scope note: the demos taught test-*after*; TDD is note-covered theory for this exam, and
  saying "I understand the loop and have practiced test-after" is a better answer than bluffing a
  TDD war story.
- **EF Core testing strategies** — InMemory provider (fast, and not a database), SQLite
  in-memory (real SQL semantics), the real engine (full fidelity); seeding; isolation via fresh
  DB per test vs shared DB + transaction rollback. Live: walkthrough `02` Steps 8–10.
- **Logging vital events** — severity ladder (`ILogger` Trace..Critical / Serilog
  Verbose..Fatal) as production's filter; structured message templates (properties are the
  point) vs interpolated strings; `builder.Host.UseSerilog()` as the bridge; injected
  `ILogger<T>` is a test seam (NullLogger / capturing fake), static `Log` is not. Taught live as
  walkthrough `02` Step 6b over the real middleware; depth in `serilog-structured-logging.md`.
- **Serialization + file I/O (the cross-week rows)** — these were taught in Weeks 1–2 and named
  again in Week 8:
  - *Basic file I/O:* `File.WriteAllText` / `AppendAllText` write; `File.ReadAllText` /
    `ReadAllLines` read; streaming APIs (`StreamReader`) exist for large files. Persistence vs
    ephemeral: a file survives the process; a variable does not (`os-cli-file-io.md`, including
    the run-count program that persists across executions).
  - *Deserialize:* `JsonSerializer.Deserialize<T>(json)` turns JSON text into objects; the Wk2
    pattern deserializes into a built-in shape and builds the domain object from the fields read
    (`async-http-networking.md`). In Week 8 you met the same operation twice more:
    `GetFromJsonAsync<T>` in the integration tests and model binding as
    deserialization-then-validation (walkthrough `02` Steps 3 and 7 name both).
  - *Serialize an object to a file to persist it* — the round-trip that combines the two rows:

    ```csharp
    var book = new Book { Isbn = "BK-001", Title = "Clean Code", Stock = 5 };
    File.WriteAllText("book.json", JsonSerializer.Serialize(book));   // persist
    var restored = JsonSerializer.Deserialize<Book>(File.ReadAllText("book.json"));
    ```

    Say the shape: serialize = object -> text (or bytes); deserialize = text -> object; writing
    the text to a file is what makes it *persistence* (it survives the run). `bitstreams` in the
    rubric row = the same idea with binary formats; JSON is the format you used throughout.
- **Code coverage** (supporting the white-box rows) — `dotnet test --collect:"XPlat Code
  Coverage"` with coverlet, Cobertura output; line vs branch coverage; coverage is a signal, not
  a target (100% covered can still be 0% asserted). Live run: walkthrough `01` Step 10.

### Pitfalls

- Mocking what you own all the way down — a service test whose every dependency is mocked can
  pass while the real wiring is broken; that is what the integration tier is for.
- `InMemory` provider passing where SQL would fail (no relational semantics) — name SQLite
  in-memory as the middle option.
- Sharing one `WebApplicationFactory` per test *method* — expensive; share via fixture
  (`ICollectionFixture`, walkthrough `02` Step 11).
- Asserting log *calls* through a static logger — the static is process-global, not a seam;
  inject `ILogger<T>`.

### Worked example (annotated)

`InventoryServiceTests.cs` + `InventoryControllerTests.cs` (T4
`library-fulfillment/tests/`, rung `01-xunit-fundamentals`): a service isolated behind a Moq'd
repository, then the controller test that hits the `IMemoryCache` trap — the taught example of
choosing what to mock and what to keep real. The integration mirror is `AuthApiTests.cs` /
`InventoryApiTests.cs` (rung `02-xunit-webapi`): the same API proven through real HTTP.

---

## Cluster 4 — Testing Applications with Cypress

**Objectives served:** 10 Must, 6 Should, 4 Nice of the Cypress section — with the CI/CD Must row
at awareness depth (see the scope note below).

**Primary sources:** `content/02-cypress/cypress-fundamentals.md`,
`content/02-cypress/cypress-advanced.md`. Live suites: `react-spa-demo/cypress/` (rungs
`11-cypress-fundamentals`, `12-cypress-advanced`, `14-cypress-quality`).

### Concept recap

- **Install / configure** — Cypress installs as a dev dependency into the SPA's own npm project;
  `cypress.config.js` + support files; the runner artifacts are gitignored. Interactive mode
  (`npx cypress open`) for building tests, headless (`npx cypress run`) for suites. The taught
  command is `npx cypress run` — never `cy run`.
- **Test structure** — Mocha shape: `describe` blocks, `it` cases, and the full hook family
  (`before` / `beforeEach` / `afterEach` / `after`, and nesting) with the xUnit analogue for each
  (walkthrough `03` Step 6). Cleanup belongs in hooks that run regardless of outcome.
- **UI interaction + selection** — `cy.visit`, `cy.get`, `.click()`, `.type()`, assertions via
  `.should(...)`. Core concept: **commands are enqueued, not promises** — they chain, each command
  yields a subject to the next. **Retry-ability** is the headline: queries and assertions retry
  until timeout, which is why well-written Cypress tests need no explicit sleeps.
- **Selectors** — give tests their own attributes (`data-cy`); role-based selectors are the
  defensible alternative that doubles as an accessibility check. Never selectors coupled to
  styling.
- **Forms** — type / select / submit through the real form and assert the outcome the user would
  see, plus the server-visible effect where the case owns it (walkthrough `04` Step 6's admin
  flow).
- **Fixtures + custom commands** — fixtures are versioned JSON test data (`cy.fixture`);
  custom commands (`Cypress.Commands.add`) are the reuse seam — the taught pair is
  `cy.resetSeed()` and `cy.login()` (walkthrough `04` Step 2). Test-data strategy: the four
  homes (fixture / seeded live DB / intercept stub / created in test) and the trade each makes.
- **`cy.intercept`** — the network seam: *spy* (observe and let pass), *stub* (scripted
  response), and *wait-on-alias* (`cy.wait('@alias')`) as the correct way to handle async — wait
  for the *event*, never a duration. This is also the async-handling answer: retry-ability plus
  intercept aliases replace sleeps.
- **Debugging** — the Test Runner's time-traveling command log, DOM snapshots per command,
  browser devtools on the runner, `.debug()` / `cy.pause()` (walkthrough `04` OF-3).
- **Suite organization** — spec-per-flow, independent tests, three homes for duplication
  (custom command / fixture / page object), selector policy, no numeric waits, keep it fast —
  the suite-maintenance checklist (walkthrough `04` OF-1). POM in Cypress exists
  (`pages/CatalogPage.js`, `catalog-pom.cy.js`) *with the honest counterpoint*: custom commands +
  fixtures already cover much of what POM buys elsewhere.
- **Quality-bar Should rows** (all delivered live Mon Aug 3, walkthrough `06-cypress-quality`):
  - *Component testing* — `mount()` a real component (`BookCard`, `SearchBar`) with props and
    callback spies; the pyramid bridge between vitest units and E2E.
  - *Plugins* — `setupNodeEvents` is the seam; `cy.task` runs Node code from a test;
    `@cypress/code-coverage` + `vite-plugin-istanbul` instrument and collect coverage, and you
    read the report.
  - *Visual regression* — baseline screenshot, mutation, red diff, revert
    (`cypress-image-diff-js`).
  - *Cross-browser* — the same suite under `--browser chrome` / `--browser edge`.
  - *Dashboards / Cypress Cloud* — awareness depth: recorded runs, parallelization, flake
    tracking (walkthrough `05` Step 1 / OF-2).
- **CI/CD (Must row — awareness scope for this sitting).** What integration looks like:
  `npx cypress run` obeys the exit-code contract (non-zero on failure fails the pipeline job),
  the app must be started before the suite (or by the job), and failure artifacts (screenshots,
  videos) are uploaded on red. `cypress-advanced.md` "Running in a CI pipeline" carries the shape
  including a sample GitHub Actions workflow. The hands-on pipeline lands in Week 10's CI/CD
  work (deferral on record, user call 2026-07-27) — for this exam you need the *shape*, not a
  built pipeline.

### Pitfalls

- `cy.wait(3000)` — a duration wait is the smell; wait on an aliased intercept or an assertion.
- Treating command chains as promises (`await cy.get(...)` — no; commands enqueue).
- Selectors coupled to CSS classes that styling refactors will break — `data-cy`.
- Tests that depend on each other's leftover state — independence is the suite-org rule the
  checklist leads with.

### Worked example (annotated)

`intercept.cy.js` (rung `12-cypress-advanced`): the same endpoint spied, stubbed, and waited on
by alias — one spec demonstrating all three intercept roles and why the wait targets the event.
Pair it with `admin-form.cy.js` for the full form-flow shape.

---

## Cluster 5 — Web Automation with Selenium

**Objectives served:** all 14 Must, all 8 Should, all 4 Nice rows of the Selenium section.

**Primary sources:** `content/03-selenium/` — `selenium-intro.md`, `selenium-ecosystem.md`,
`selenium-locators-navigation.md`, `selenium-xpath.md`, `selenium-interactions-waits.md`,
`selenium-windows-alerts-exceptions.md`, `selenium-pom-design-patterns.md`. Live suite:
`react-spa-demo/e2e-selenium/` (rungs `13` and `15`–`17`).

### Concept recap

- **Ecosystem + use cases** — Selenium is out-of-process automation over the W3C WebDriver
  protocol (vs Cypress's in-browser model — know the trade both ways). The suite is three tools:
  WebDriver (the library), IDE (record/replay — honest assessment: quick capture, brittle
  output), Grid (distribute one suite across many machines/browsers). Framework *types* around
  it: data-driven, keyword-driven, hybrid, and BDD (Gherkin Given/When/Then; SpecFlow
  historically, Reqnroll as successor). RPA is a sibling, not a synonym.
- **Driver management** — Selenium Manager resolves the browser/driver pair automatically (the
  taught default); manual driver setup exists and its version-match rule is why you prefer not
  to (walkthrough `05` Steps 5–6). `IWebDriver` is the session; always `Quit()` (orphaned
  browser problem); Options classes customize launch (headless is the taught example).
- **Locators + find methods** — the `By` factory: `Id`, `Name`, `TagName`, `ClassName`,
  `CssSelector`, `LinkText`, `PartialLinkText`, `XPath`. Preference order: id/test-attribute
  first, CSS next, XPath where structure demands it. **The find-methods contract:**
  `FindElement` throws `NoSuchElementException` when nothing matches; `FindElements` returns an
  empty list — which is how you assert *absence* without a try/catch.
- **XPath** — grammar in five tokens (`//`, `/`, `@`, `[]`, `*`); functions `contains()`,
  `starts-with()`, `text()`; axes (`ancestor::`, `following-sibling::`, `preceding-sibling::`);
  absolute vs relative — absolute paths break on any structural change (the taught trap);
  CSS-vs-XPath decision: prefer CSS until you need text matching or upward/sideways traversal.
  Interview trap on record: `[@class='card']` is an *exact string* match — an element with
  `class="card featured"` does NOT match it (use `contains(@class,'card')` — with its own
  substring caveat).
- **Navigation** — `Navigate().GoToUrl/Back/Forward/Refresh`; deep routes against the SPA
  (walkthrough `07` Step 6).
- **Interactions + element state** — the verbs: `Click`, `SendKeys`, `Clear`, `Submit`; the two
  reads: `Text` (rendered text) vs `GetAttribute(...)` (DOM attribute — an input's typed value
  lives in the `value` attribute, not `Text`); state reads `Displayed` / `Enabled` / `Selected`.
  The **Select class** wraps `<select>` elements (`SelectByText/Value/Index`, multi-select).
  The **Actions API** is for gestures: hover (`MoveToElement`), double-click, keyboard chords —
  build then `Perform()`.
- **The three waits** (all Must, all implemented live):
  - *Implicit* — one driver-wide poll-on-find setting; applies to every `FindElement`.
  - *Explicit* — `WebDriverWait` + a lambda condition; waits for a *specific condition* at a
    specific point; the taught fix for the race you watched fail on screen.
  - *Fluent* — explicit waiting with tuning: `PollingInterval`, `IgnoreExceptionTypes`. The .NET
    truth: `WebDriverWait` *is* a FluentWait subclass — there is no separate `FluentWait` class
    as in Java; "fluent" is the tuning knobs, not a different type.
  - **The no-mixing rule:** implicit + explicit waits interact unpredictably (compounded
    timeouts) — the taught end-state (`E2ETestBase` and the page objects) carries *no* implicit
    wait; everything is explicit.
- **Windows, alerts** — a window is a *handle*; the driver talks to one at a time
  (`WindowHandles`, `SwitchTo().Window(...)`, `SwitchTo().NewWindow(...)`, `target=_blank`
  flows). Alerts live outside the DOM: `SwitchTo().Alert()` then `Accept` / `Dismiss` /
  `SendKeys` for `alert` / `confirm` / `prompt`.
- **Exceptions as diagnosis** — the bestiary and what each one *accuses*:
  `NoSuchElementException` (locator wrong, or too early), `StaleElementReferenceException` (the
  DOM re-rendered under your reference — re-find), `ElementNotInteractableException` (present
  but not clickable/visible), `TimeoutException` (the wait's condition never held),
  `ElementClickInterceptedException` (something overlays it). Debugging Selenium *is* reading
  these correctly (`selenium-windows-alerts-exceptions.md`).
- **Screenshots** — `GetScreenshot()` on the driver (viewport) and on an element; the
  capture-on-failure pattern: a `Guarded` wrapper that saves `FAILED-<TestName>.png` on
  exception and rethrows (walkthrough `09` Steps 3 and 9).
- **POM** — page structure captured once in a page class (locators + intent-named methods),
  specs read as user journeys; shared plumbing in a base class (`E2ETestBase`). **PageFactory:**
  recognize `[FindsBy]` + `InitElements` on sight — and know the .NET truth: it is deprecated
  in .NET Selenium; plain `By` fields + explicit waits are the current idiom
  (`selenium-pom-design-patterns.md`, including the design-pattern map: facade, factory,
  builder, template method).
- **Grid + pros/cons** (Nice tier) — Grid distributes tests across nodes for cross-browser and
  parallel scale (awareness). Selenium pros: any browser, any language, out-of-process realism,
  huge ecosystem; cons: more setup than Cypress, flakier without wait discipline, slower
  feedback loop.

### Pitfalls

- `Thread.Sleep` anywhere — the P3 spec bans it; explicit waits are the answer.
- Mixing implicit and explicit waits — compounding timeouts; pick explicit, zero the implicit.
- Reading an input's content via `.Text` — it lives in `GetAttribute("value")`.
- Reusing an element reference across a React re-render — stale element; re-find, or wait on a
  condition that re-queries.
- Absolute XPath in a spec — one layout change breaks it; the taught rung keeps one only as the
  labeled "don't ever do this" example.

### Worked example (annotated)

`WaitTests.cs` (rung `16-selenium-elements-waits`): the API-render race asserted *without* a
wait — which legitimately runs red or green depending on timing (it ran red in the room on
Wed Aug 5, 25/26, and green at the close: both outcomes are the lesson) — then the same
assertion under `WebDriverWait`, then the fluent tuning. That arc, plus the POM refactor in
`PomTests.cs` (rung `17-selenium-windows-pom`) with capture-on-failure demonstrated red, is
the strongest Selenium story you can tell in an interview.

---

## Cross-cluster: the comparison questions

Two-week material makes three comparisons fair game; prepare all three:

1. **Cypress vs Selenium** — in-browser vs WebDriver protocol; auto-retry vs explicit waits;
   JS-only vs any language; same-origin trade vs multi-tab/window reach. P3 requires your own
   written comparison from your own two suites — use it.
2. **Unit vs integration vs E2E** — speak the pyramid with your own artifacts at each level.
3. **Scripted vs exploratory** — repeatable/automatable/auditable vs finds-what-was-never-
   specified; every serious strategy runs both.
