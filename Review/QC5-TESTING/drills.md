# QC-6 Test Automation — Drills

Short hands-on tasks per topic cluster. Drill prompts are **domain-neutral** — do them in your
own P3 domain (that doubles as P3 progress). Model solutions use the trainer's Library domain
and point at the taught shape; they are illustrations, not files to copy.

Work a drill cold, then compare. If the model solution surprises you, the QC pointer names the
row to restudy.

---

## Cluster 1–2 — Philosophy + design (paper drills)

### Drill 1 — Classify the case
For each scenario in *your* domain, label it: positive or negative, which box model, which
level. (a) An API call with a malformed body asserting a 400. (b) A browser flow through your
main user journey. (c) A method-level test of your discount/price/limit rule with a mocked
repository. (d) A test that calls your API then queries the database to confirm the row
changed.

**Model solution (Library):** (a) negative, black-box, integration — malformed checkout JSON
-> 400 problem details. (b) positive, black-box, system/E2E — search-select-checkout in the
browser. (c) positive (or negative per input), white-box, unit — fine calculation with a
stubbed repo. (d) positive, gray-box, integration — POST /checkout then assert
`Inventory.CurrentStock` decremented.
*Proves QC: positive/negative; black/white box application; pyramid levels.*
*Source: `content/01-xunit/testing-fundamentals.md`.*

### Drill 2 — Build a mini-RTM
Take four requirements from your P3 skeleton. Draw the RTM against your existing cases. Find:
one empty row (or prove there is none), one case column that traces to nothing, and write the
one-line residual-risk statement for anything uncovered.

**Model solution (Library):** the note's own worked matrix — R-1 max loans / R-2 quantity
range / R-3 zero stock / R-4 unknown member, with R-4's row empty and TC-05 tracing to nothing.
The residual-risk line: "R-4 unknown-member rejection is stated and untested; declared as an
open gap in the README." Both-direction read is the point.
*Proves QC: Design structured test cases using appropriate techniques aligned with an RTM.*
*Source: `content/01-xunit/test-case-design.md`, RTM section; your P3 RTM.*

### Drill 3 — Technique from shape
Requirement, domain-neutral: "A booking may be cancelled up to 24 hours before start, except
by suspended accounts, which may never cancel." Name every technique the shape calls for and
list the case set each produces.

**Model solution:** boundary on the 24-hour limit (23h59m, 24h, 24h01m before start);
decision table on cancel-window x suspension (4 rules — the payoff row: suspended AND inside
the window, which message wins?); state transition if bookings have a lifecycle
(booked -> cancelled legal; cancelled -> cancelled illegal). The undefined combination goes
back to the requirement owner as a design-time defect.
*Proves QC: Recognize appropriate general testing techniques from documented requirements.*
*Source: `test-case-design.md`, technique-selection table.*

### Drill 4 — Source three error guesses
Against your own P3 app, write three error-guess one-liners (hypothesis / source / case /
result), each with a *real* source: a past defect, a fragile-input class, or churn. Execute
them. Graduate any confirmed guess to a traced regression case.

**Model solution (Library):**
```
GUESS-01  Hypothesis: checkout of 3 where stock is 2 partially succeeds
          Source:     partial-write defect pattern (returns flow, Wk8 demo discussion)
          Case:       TC-19  Result: pending
GUESS-02  Hypothesis: search accepts a 500-char string and the UI overflows
          Source:     fragile-input class (max length)
          Case:       TC-20  Result: pending
GUESS-03  Hypothesis: seed-reset during an open session leaves a stale catalog render
          Source:     defect history (stock drained to 0/0/0 during order demos)
          Case:       TC-21  Result: pending
```
This is the P3 hunting-record format verbatim.
*Proves QC: error guess testing rows (all three).*
*Source: `test-case-design.md`, error-guessing section; P3 spec hunting record.*

### Drill 5 — Charter an exploratory session
Write one charter for your app (area + condition + purpose), timebox it at 30 minutes, run it,
and produce session notes with at least one finding mapped back to your RTM as defect /
requirements-gap / surprise.

**Model solution (Library):** "Explore checkout with members at or near the loan limit, on the
mobile viewport, to discover defects in limit enforcement and its error messaging." Finding
types to look for: the limit message when *two* rules fail at once is typically the
requirements gap.
*Proves QC: exploratory testing rows (all four).*
*Source: `test-case-design.md`, exploratory section; `p3-design-artifacts-walkthrough.md`.*

---

## Cluster 3 — .NET

### Drill 6 — Theory from partitions
Pick one bounded rule in your domain. Write the partitions and boundaries on paper, then a
`[Theory]` whose `[InlineData]` rows each carry a comment naming the class or edge it
represents. Run it; make one row fail deliberately; read the failure output.

**Model solution (Library):** the checkout quantity rule as in the cheat sheet — rows 0 / 1 /
3 / 4 commented "below / edge / edge / above". The taught shape is walkthrough `01` Step 6
(rung `01-xunit-fundamentals`, `library-fulfillment/tests/`).
*Proves QC: Fact/Theory; theory testing + multiple assertion types; EP/BVA.*
*Source: `xunit-fundamentals.md`.*

### Drill 7 — Mock the seam
Take one of your services with a repository dependency. Write two tests: one *stub*-style
(Setup feeds data, assert the return) and one *mock*-style (Verify the repository call
happened with the right argument). Then try to mock a static call and write one sentence on
what that teaches about design.

**Model solution (Library):** `InventoryServiceTests` — `Setup(r => r.GetByIsbn(...))` feeding
a zero-stock book and asserting the checkout throws (state), then
`Verify(r => r.Save(It.IsAny<Inventory>()), Times.Once)` on the happy path (behavior). The
static attempt fails to compile against `File.ReadAllText` — the sentence: "statics are not
seams; wrap them behind an interface if the boundary matters."
*Proves QC: mocking frameworks; stubs.*
*Source: `moq-coverage-service-testing.md`; walkthrough `01` Step 8.*

### Drill 8 — One integration test, real HTTP
Add (or rehearse) one `WebApplicationFactory` test in your P3 API suite proving your auth
matrix: same endpoint, three requests — no token (401), wrong role (403), right role (2xx).

**Model solution (Library):** `AuthApiTests` shape (rung `02-xunit-webapi`): client from the
factory, login via the real auth endpoint, three asserts. The point to say out loud: the test
crossed routing, binding, middleware, and the auth pipeline — things no unit test sees.
*Proves QC: distinguish unit vs integration; meaningful unit tests (contrast).*
*Source: `integration-testing.md`; walkthrough `02` Step 4.*

### Drill 9 — Persist and restore
Console scratch: serialize one of your domain objects to JSON, write it to a file, read it
back, deserialize, and assert round-trip equality in a quick test. Add one severity-correct
log line for the "vital event" of a failed restore.

**Model solution (Library):** the `book.json` round-trip from the cheat sheet (Section 5),
plus `_logger.LogError("Restore failed for {Path}", path)` — Error because data loss is
actionable, structured because `Path` should be queryable.
*Proves QC: serialize/deserialize; basic file I/O; file I/O for persistence; logging vital
events.*
*Source: `os-cli-file-io.md` (Wk1); `async-http-networking.md` (Wk2); walkthrough `02` S6b.*

---

## Cluster 4 — Cypress

### Drill 10 — Spy, stub, wait
On one list-rendering page of your app: (a) spy the backing API route and wait on the alias
before asserting the list; (b) stub the same route with a fixture of exactly one item and
assert the UI shows one; (c) stub a 500 and assert your error state renders.

**Model solution (Library):** the `intercept.cy.js` arc (rung `12-cypress-advanced`) against
`/inventory` — spy + `cy.wait('@load')`, fixture stub, forced-500 error state. Say which of
the three each test *proves* (integration vs determinism).
*Proves QC: intercepts stub/spy/mock; async behavior and API requests.*
*Source: `cypress-advanced.md`, intercept section.*

### Drill 11 — Extract the duplication
Find the login-or-seed plumbing repeated across your specs. Extract a custom command for the
behavior and a fixture for the data. Every spec should start from a known state in one line.

**Model solution (Library):** `cy.resetSeed()` + `cy.login()` in `support/commands.js` with
`fixtures/users.json` (rung `12`; walkthrough `04` Step 2). The rule of thumb: behavior ->
command, data -> fixture, page structure -> page object.
*Proves QC: fixtures and custom commands; suite organization best practices.*
*Source: `cypress-advanced.md`.*

### Drill 12 — Mount one component
Component-test one presentational component from your SPA: mount with controlled props,
assert the render; if it takes a callback, pass `cy.spy()` and assert the call.

**Model solution (Library):** `BookCard` mounted with a props object asserting title/stock
render; `SearchBar` with `cy.spy().as('onSearch')` asserting the spy fired on type
(walkthrough `06` Steps 3–4, rung `14-cypress-quality`).
*Proves QC: component-level testing strategies.*
*Source: walkthrough `06`; `cypress-advanced.md` CT section.*

---

## Cluster 5 — Selenium

### Drill 13 — Locator ladder
Pick one element rendered deep in your UI. Locate it five ways: id/test attribute, CSS,
XPath relative, XPath with `contains()`, and (once, to feel it break) an absolute XPath.
Then resize/rearrange the layout and note which locators survived.

**Model solution (Library):** the catalog card ladder from walkthrough `07` (rung
`15-selenium-locators`) — the absolute path is committed there under a comment that says
exactly what to think of it. Survivors: the test attribute and the relative forms.
*Proves QC: locator strategies; relative/absolute XPath; XPath functions.*
*Source: `selenium-locators-navigation.md`; `selenium-xpath.md`.*

### Drill 14 — Race, then fix
Write a spec that navigates to your list page and immediately asserts the list is non-empty —
no wait. Run it repeatedly; note the flake (or the shame of a fast machine). Fix it with a
`WebDriverWait` on the non-empty condition; then tune the same wait fluently
(`PollingInterval`, `IgnoreExceptionTypes`) and say what changed.

**Model solution (Library):** the `WaitTests.cs` arc verbatim in shape (rung
`16-selenium-elements-waits`): the race assertion ran red in the classroom (25/26) and green
at the close the same day — both legal, which is the lesson — then the explicit wait, then
fluent tuning. Do not port the mixing anti-pattern: implicit stays zero.
*Proves QC: implicit/explicit/fluent waits; debug and troubleshoot.*
*Source: `selenium-interactions-waits.md`; walkthrough `08` Steps 7–9.*

### Drill 15 — Page objects earn a journey
Take your two most-duplicated pages. Write page classes (private locators, intent-named
methods) and a base class for driver plumbing. Rewrite one existing multi-step spec as a
journey through the page objects. Add a capture-on-failure guard to the base and prove it by
breaking one assertion (then revert).

**Model solution (Library):** `CatalogPage` + `LoginPage` + `E2ETestBase` with `Guarded`
saving `FAILED-<TestName>.png` (rung `17-selenium-windows-pom`; walkthrough `09` Steps 3,
8–9). The journey spec reads `SignInAs -> Search -> AssertCard` — if your spec still names
selectors, the extraction is not done.
*Proves QC: page object model; capture screenshots.*
*Source: `selenium-pom-design-patterns.md`; walkthrough `09`.*

### Drill 16 — Alert and second window
Add (or use) a page with an `alert`, a `confirm`, and a `target=_blank` link. Script: accept
the alert, dismiss the confirm and assert the cancelled path, open the new window, assert
something in it, close, switch back, and assert you are back.

**Model solution (Library):** the `widgets.html` seed page drives exactly this
(`AlertTests` + `WindowTests`, rung `17`; walkthrough `09` Steps 4–5). The two claims to make
afterwards: alerts live outside the DOM; the driver talks to one handle at a time.
*Proves QC: window contexts; browser alerts.*
*Source: `selenium-windows-alerts-exceptions.md`.*
