# Cypress Advanced: Network Control, Test Data, and Scaling a Suite

## Learning Objectives
- Package repeated flows as custom commands and judge when a plain JavaScript helper is the better tool.
- Derive test data from the test objective, then choose its home from the four storage options — fixture
  file, seeded live database, network stub, created-in-test — by trade-off.
- Organize and maintain a growing suite: spec-per-flow, independent tests, the three homes for
  duplication, a selector policy, and keeping the run fast enough that people still run it.
- Explain Cypress's enqueued command model (why `const el = cy.get(...)` cannot work) and extract values
  with `.then()` and aliases.
- Spy and stub network traffic with `cy.intercept()`, force error responses to test failure UI, and wait
  on aliased requests instead of fixed sleeps.
- Drive forms end to end (`.type`, `.select`, `.check`), debug with time-travel, `.debug()`, and
  `.pause()`, weigh Page Object Model against custom commands, and name the reporting options.
- Integrate the suite into a CI pipeline: `cypress run` headless, starting the application before the
  tests, the exit-code contract, and uploading screenshots and video on failure.

## Why This Matters
The gap between "I wrote a passing Cypress test" and "I own a suite the team trusts" is everything in
this note. Real suites repeat flows (every test logs in), need controlled data (a search test against an
empty database proves nothing), hit asynchronous traps, and must test failure paths a healthy backend
never produces on demand, like a 500 from the catalog API. Network stubbing, deliberate data strategy,
and disciplined waiting keep a suite fast and deterministic as it grows from five specs to five hundred.

These are also the topics that separate candidates in interviews: "how do you test the error state?",
"why can't you assign a command to a variable?", and "do you use page objects?" all probe whether you
understand how the tool actually executes.

## The Concept

### Custom commands: DRY for whole-suite flows
A flow repeated in every spec — logging in is the classic — belongs in `cypress/support/commands.js` as
a custom command, available on `cy` everywhere:

```js
// cypress/support/commands.js
Cypress.Commands.add("login", (email, password) => {
  cy.visit("/login");
  cy.get("[data-cy=email]").type(email);
  cy.get("[data-cy=password]").type(password, { log: false });
  cy.get("[data-cy=login-submit]").click();
  cy.get("[data-cy=user-menu]").should("be.visible"); // command guarantees a logged-in state
});

// any spec:
cy.login("reader@library.test", "pass123!");
```

The trade-off: custom commands add indirection and live far from the specs that use them, so do not
reach for one by reflex. If the logic is pure (build a book payload, format a date), a plain imported JS
helper is simpler and returns values normally, which commands do not — reserve `Cypress.Commands.add`
for flows that act on the app. The standard follow-up: for login specifically, the faster pattern is
`cy.request` to the auth endpoint cached with `cy.session`, so only one test exercises the login UI.

### Test data: choose the data, then choose its home
Test data is a design decision with a fixed order, and both halves get asked in interviews.

**What data? Ask the test objective.** Derive the smallest data that could prove the thing under test,
never the other way around. "Filtering works" needs one row that matches and one that does not; "the
out-of-stock badge renders" needs a row whose stock is exactly zero; "the list is not empty" needs one
row of anything. That derivation is test-case design applied to data — the same reasoning that picks
equivalence-class and boundary rows for a parameterized unit test, one layer up the pyramid.

**Where does it live? Ask the test object.** What the thing under test actually reads decides the
storage. Four homes, and a real suite uses all four deliberately:

1. **Fixture file** — JSON checked into the repo under `cypress/fixtures/`, loaded with
   `cy.fixture("books.json")`. Data that belongs to the suite and is version-controlled beside the specs.
   It is *content*, and it usually reaches the app through home 3. Perfectly deterministic; the cost is
   drift — a fixture can quietly diverge from what the real API now returns.
2. **The live database, put into a known state** — `cy.request("POST", "/api/test/seed", { books: [...] })`
   against a seed or reset endpoint in `beforeEach`, or `cy.task` running Node (and direct database
   access) from the config file when the API cannot express the setup. Fast, and it keeps the stack
   genuinely end to end; the cost is needing backend support and shared-state discipline. This is the
   default recommendation for tests whose point is that the real system agrees.
3. **A stub handed to the network layer** — `cy.intercept` answering with a fixture, an inline body, or a
   forced status code. It is the *delivery mechanism*: nothing touches persistence, so error paths and
   impossible states become one-liners. The trade is that the test stops proving the API behaves.
4. **Created by the test itself, then cleaned up** — drive the API (or, exactly once per flow, the UI)
   to create what the test needs and delete it at the end. Necessary when the data must be real and
   unique; the cost is that a failed run can leave the row behind. Never use the UI as everyone else's
   setup: when the create form breaks, half the suite fails for the wrong reason.

The trade in one line: fixtures and stubs buy determinism and speed and give up proof that the real
system agrees; seeded live data and in-test creation buy that proof and cost you cleanup discipline.
Deciding per test, out loud, is the skill; drifting into whatever the database happens to hold is the
failure mode.

### Commands are enqueued, not promises
Cypress commands do not execute when the line runs — they are **enqueued**, and the runner executes the
queue serially after the test function returns. They are chainables, not promises, so this is wrong:

```js
const button = cy.get("[data-cy=checkout]"); // NOT an element — a Chainable placeholder
button.click();                              // works only by accident; never branch on it
```

To use a yielded value, stay inside the chain with `.then()`, or name it with an alias:

```js
cy.get("[data-cy=book-card]").first().invoke("text").then((title) => {
  cy.get("[data-cy=search-input]").type(title);
});

cy.get("[data-cy=book-card]").its("length").as("initialCount"); // alias a value
cy.get("@initialCount").then((count) => {
  expect(count).to.be.greaterThan(0);
});
```

This is also why `async/await` does not mix with Cypress commands — the supported answer is `.then()`.
The honest reply to the follow-up "how do you avoid callback pyramids?": you rarely need raw values —
assert with `should` in the chain, and reach for `.then()` only when logic depends on a runtime value.

### cy.intercept: spy, stub, and wait done right
`cy.intercept()` sits between the app and the network, and it has two modes. **Spy**: let the real
request through, but alias it so you can wait for it and assert on it — this is the correct replacement
for `cy.wait(3000)`, which is a flake-hiding anti-pattern (too short on a slow day, wasted time on a
fast one, and silent about *what* it was waiting for):

```js
cy.intercept("GET", "/api/books*").as("getBooks");
cy.visit("/");
cy.wait("@getBooks").then((interception) => {
  expect(interception.response.statusCode).to.eq(200);
  expect(interception.request.url).to.include("page=1");
});
cy.get("[data-cy=book-card]").should("have.length.greaterThan", 0);
```

**Stub**: supply the response yourself — a fixture or an inline body — so the test is deterministic and
needs no backend at all, including states a healthy backend cannot produce on demand:

```js
cy.intercept("GET", "/api/books*", { fixture: "books.json" }).as("getBooks");

// forcing the error path — the UI's error state is now testable at will:
cy.intercept("GET", "/api/books*", { statusCode: 500, body: { error: "boom" } }).as("getBooksError");
cy.visit("/");
cy.wait("@getBooksError");
cy.contains("Something went wrong").should("be.visible");
```

The trade-off to say out loud: a fully stubbed suite can stay green while the real API is broken.
Stub for breadth and error paths, but keep a thin unstubbed smoke layer (or contract tests) that proves
the front end and the real backend still agree.

### Forms end to end
Form commands mirror user input, including special key sequences in `.type()`:

```js
cy.get("[data-cy=search-input]").clear().type("refactoring{enter}");
cy.get("[data-cy=format-select]").select("Hardcover");
cy.get("[data-cy=terms-checkbox]").check();
cy.get("[data-cy=checkout-form]").submit();

// assert both the rejection and the success path:
cy.contains("Email is required").should("be.visible");        // submitted empty
cy.get("[data-cy=confirmation]").should("contain", "Enjoy");  // submitted valid
```

Assert validation messages the way a user sees them (visible text), and always assert the post-submit
state — a form test that stops at `.submit()` has verified nothing.

### Debugging failing tests
The interactive runner's **time-travel command log** is the first tool: click any command to see DOM
snapshots before and after it ran, with intercepted requests inline. `.debug()` drops into the browser
debugger with the yielded subject available; `cy.pause()` halts the test for manual stepping. Headless
`cypress run` captures a screenshot on every failure and can record video — usually enough to diagnose
CI-only failures without reproducing locally.

### Page Object Model — and the honest counterpoint
POM wraps a page's selectors and actions in a class, so specs read as intent and selector changes are
one-file fixes:

```js
// cypress/pages/catalogPage.js
export class CatalogPage {
  visit() { cy.visit("/"); return this; }
  search(term) { cy.get("[data-cy=search-input]").clear().type(term); return this; }
  bookCards() { return cy.get("[data-cy=book-card]"); }
  checkoutFirst() { cy.get("[data-cy=book-card]").first().find("[data-cy=checkout]").click(); return this; }
}

// spec:
import { CatalogPage } from "../pages/catalogPage";
const catalog = new CatalogPage();
catalog.visit().search("pragmatic");
catalog.bookCards().should("have.length", 1);
```

The counterpoint, stated honestly: the Cypress documentation leans *away* from POM, favoring custom
commands and "app actions" (setting state programmatically) — retry-able chains do not fit the
return-an-element style POM inherited from Selenium, and class wrappers add a layer between the reader
and the command log. POM still earns its keep on large multi-contributor suites that want one enforced
place per page, and on teams migrating from Selenium who already share the vocabulary. Frame the choice
as suite size and team convention, not dogma.

### Organizing a suite that stays maintainable
The practices below are what "organizing and maintaining a suite" means concretely, and each is a
recognizable interview answer on its own:

- **One spec file per user-facing flow**, named for the flow (`login`, `catalog`, `checkout`). A failing
  filename in a CI log should already say what broke and for whom.
- **Every test independent.** No test may rely on a previous test having run: ordering is not a contract,
  and a suite you cannot run one test out of is a suite nobody debugs. Each test sets up its own world.
- **Three kinds of duplication, three homes.** Repeated app flows become custom commands; repeated data
  becomes a fixture; repeated selectors become a page object (or, per the counterpoint above, an app
  action). Naming the home is the entire refactoring vocabulary of a test suite.
- **Selector policy, decided once.** Dedicated test attributes (`data-cy` / `data-testid`) first, then
  text and roles a user actually perceives, and never CSS that encodes layout — a class named for a grid
  column is a redesign away from a red suite.
- **No fixed-millisecond waits** without a written justification; assertions retry and request aliases
  wait for the real event.
- **Keep it fast enough that people run it.** Exercise the login UI once and authenticate programmatically
  everywhere else, stub what the test is not actually testing, and keep artifacts (screenshots, videos)
  out of version control. A suite that takes twenty minutes gets skipped, and a skipped suite is a
  deleted suite.

### Running in a CI pipeline
A suite only earns its cost once it runs without anyone asking it to. That means a continuous-integration
pipeline runs it on every push, and a failure stops the change from merging.

The command is the headless one. `npx cypress open` launches the interactive runner and waits for a human,
so it can never run in CI; `npx cypress run` executes every spec headless, prints a per-spec results table,
and — the part that makes automation work — **exits non-zero when any test fails**. CI systems read that
exit code and nothing else to decide pass or fail, which is the entire contract between your suite and the
pipeline.

The one genuinely awkward part is that end-to-end tests need the application *running* before they start.
The pipeline has to build it, start it in the background, and wait until it answers before invoking
Cypress. `start-server-and-test` is the small npm package that does exactly this — start a command, poll a
URL until it responds, run the tests, then shut the server down and propagate the test exit code. Add it
alongside Cypress:

```bash
npm install --save-dev start-server-and-test
```

The pipeline itself is a configuration file, and every CI system expresses the same four ideas: check out
the code, install dependencies, start the app and run the tests, archive the evidence. The example below
is **GitHub Actions** dialect. Read it as: a *workflow* contains **jobs**; each job gets a fresh machine
and runs its **steps** in order; a step is either `run:` (execute this shell command) or `uses:` (pull in
a prebuilt action someone else maintains, configured by `with:`). Jenkins, GitLab CI, and Azure Pipelines
say all of this with different keywords — the four ideas transfer, the syntax does not.

```yaml
# .github/workflows/e2e.yml
name: e2e
on: [push, pull_request]                 # run on every push and every pull request

jobs:
  cypress:
    runs-on: ubuntu-latest               # a fresh Linux machine for this job
    steps:
      - uses: actions/checkout@v7        # get the repository onto the machine

      - uses: actions/setup-node@v7
        with:
          node-version: 22
          cache: npm

      - run: npm ci                      # reproducible install from the lockfile

      # Starts the app, waits for the URL to answer, runs the suite, then tears the server down.
      # The exit code of "cypress run" becomes the exit code of the step, so a failing test fails the job.
      # Replace the start script and the URL with your application's own.
      - run: npx start-server-and-test "npm run dev" http://localhost:5173 "npx cypress run --browser chrome"

      - name: Upload failure artifacts
        if: failure()                    # only on a red run - green runs need no evidence
        uses: actions/upload-artifact@v7
        with:
          name: cypress-artifacts
          path: cypress/screenshots
```

Three things in that file are worth more than the syntax around them.

**`if: failure()` is the step people leave out, and it is the one that matters.** Cypress screenshots
automatically the moment a test fails, but the file lives on a CI machine that is destroyed when the job
ends. Without an upload step, a red build tells you a test failed and gives you no way to see *what* the
browser looked like when it did — which turns every CI failure into "try to reproduce it locally." Upload
it and the failure is diagnosable from the build page. Video is the same idea with one catch: it is **off
by default** (`video: false` in `cypress.config.js`), so turn it on before adding `cypress/videos` to the
upload path, or the step archives nothing. Keep all of it out of version control — build output, not
source.

**`npm ci`, not `npm install`.** It installs exactly the lockfile's versions and fails if the lockfile and
`package.json` disagree, so the pipeline cannot quietly drift from what you tested.

**`--browser chrome`, not the default.** "Whatever browser the runner image happened to ship this month"
is not a reproducible test environment. Pin it and know what you are testing against.

The start script and URL are placeholders: substitute whatever command serves your app and whatever URL
it listens on — a Vite project uses `npm run dev` on port 5173, other scaffolds differ, and getting this
pair wrong is the single most common reason a first pipeline hangs until it times out.

The trade-off to state plainly: the CI run is the slower, blinder mode. It is headless, it has no
interactive time-travel, and its feedback arrives minutes later attached to a build. That does not make it
the lesser mode — it is the one that actually gates merges — but it is why the interactive runner remains
where you *author* and debug tests, and CI is where you *enforce* them.

### Reporting at awareness depth
`cypress run` prints a per-spec pass/fail table with timings — enough for CI gates. **Cypress Cloud** is
the paid layer: recorded runs with video, parallelization across CI machines, and flake detection
(tests that pass on retry get flagged). For a free local HTML report, `mochawesome` is the common
Mocha-reporter choice.

### Common failures and what they mean
Nearly every confusing failure in an advanced suite is an **ordering** problem, a **matching** problem, or
**leftover state**. Recognizing which of the three you have is most of the debugging.

**`cy.intercept` never fires — the test still shows live data.** Almost always registered *after*
`cy.visit`. The request went out before the stub existed, so there was nothing to intercept. Intercept
first, visit second; there is no exception to this. If the ordering is right and it still does not match,
the pattern is wrong. Cypress tries the full URL first and then falls back to matching the URL *path*, so
`cy.intercept("/api/inventory")` does match `http://localhost:5173/api/inventory` — but matching is
case-sensitive, and a *partial* fragment needs a wildcard (`**/inventory`). One sharp edge: patterns are
shell-style glob patterns (Cypress uses the minimatch library), so `*` matches within a path segment,
`**` matches across segments, and `?` matches exactly one character — which means a literal `?` starting
a query string has to be escaped as `\\?`, doubled because you are writing it inside a JavaScript string
literal. The command log shows the real request URL — compare it to your
pattern rather than guessing.

**A programmatic login succeeds but the app renders as anonymous.** Same ordering trap in a different
costume: the token was written to storage after `cy.visit` had already mounted the app, so the app read an
empty store on boot. Command first, visit second. The other cause is a key mismatch — the app reads one
storage key and the command wrote another, which fails silently because "no token" and "wrong key" are
indistinguishable to the app.

**A test that passed yesterday now fails on its very first assertion, every run.** Suspect leftover state
from an interrupted run. A test that creates a record and deletes it at the end leaves the record behind
if it was stopped in between — a closed runner, a Ctrl-C, a `cy.pause()` you walked away from. The next
run's create then violates a unique constraint, the API returns an error, and the UI shows the failure
copy instead of the success copy. Note that a seed-reset endpoint usually will **not** rescue you here:
resets typically restore known rows rather than remove unknown ones. Delete the stray record directly,
then re-run.

**Everything passes in the interactive runner and fails headless.** The runner's human pace hid a race.
Do not add `cy.wait(ms)`; find the step whose wait is not an assertion on an outcome and convert it —
`should` for the DOM, `cy.wait("@alias")` for a request.

**An assertion on an input's value fails even though the typed text is visibly there.** Suspect a
*controlled* input — one whose rendered `value` is driven by framework state rather than by the DOM. The
DOM has your text, but the component re-renders the value from state that has not been committed yet
(debounced, updated only on blur, or waiting on an event the test never fired), and the assertion reads
the stale one. Assert on what the application actually *does* with the value — the rendered result, the
request payload — rather than on intermediate input state.

**A negative test unexpectedly passes the happy path.** The input you chose as "invalid" is valid. Check
the real validation rule before assuming: a lower bound of `0.01` accepts `0.01` and rejects only what is
genuinely below it. This is a boundary-value mistake in the test, not a bug in the app.

## Say It in an Interview
- *"I put repeated app flows like login in custom commands in `support/commands.js`, but pure logic goes
  in plain JS helpers — and for login specifically I'd seed the session with `cy.request` and
  `cy.session` so only one test exercises the login form."*
- *"I derive the data from what the test is trying to prove — the smallest set that could prove it — and
  then pick its home from four: a fixture file, the live database put into a known state by a seed call,
  a `cy.intercept` stub, or created by the test and cleaned up. Fixtures and stubs buy determinism and
  stop proving the backend works; seeded and created data prove it and cost cleanup discipline. Creating
  through the UI is a last resort — it couples tests to unrelated flows."*
- *"In CI I run `npx cypress run` — headless, and it exits non-zero if anything fails, which is what the
  pipeline gates on. The app has to be up first, so I use `start-server-and-test` to boot it, wait for the
  URL, run the suite, and tear it down. The step people forget is uploading `cypress/screenshots` on
  failure — Cypress captures them automatically, but they die with the CI container, and without them a
  red build isn't diagnosable. Video is the same idea but it's off by default, so you turn it on first."*
- *"Keeping a suite maintainable is mostly five habits: one spec per user-facing flow, every test
  independent, repeated flows into custom commands and repeated selectors into page objects, a decided
  selector policy with dedicated test attributes, and keeping the run fast — UI login once, everything
  else authenticated programmatically."*
- *"Cypress commands are enqueued, not promises — `const el = cy.get(...)` gives you a chainable, not an
  element. You use `.then()` for values and aliases with `.as()` to reference things later."*
- *"`cy.intercept` either spies on a real request — I alias it and `cy.wait('@alias')` instead of ever
  writing `cy.wait(3000)` — or stubs the response with a fixture or a forced 500 to test error UI
  deterministically. Since stubbing everything can hide a broken backend, I keep an unstubbed smoke
  layer."*
- *"Page objects work in Cypress and help large teams standardize, but the docs favor custom commands
  and app actions — I'd match the suite's existing convention and argue POM mainly for big multi-team
  suites or Selenium migrations."*

## Check Yourself
1. You find yourself copying the same five-line login sequence into a third spec. What are your two
   refactoring options, and what decides between them?
2. Why is creating test data through the UI in `beforeEach` an anti-pattern, and what should you do
   instead?
3. What does `const cards = cy.get("[data-cy=book-card]")` actually hold, and how do you correctly use
   the number of cards in a later assertion?
4. How do you test that the catalog shows its error message when the books API returns a 500?
5. What is the argument against Page Object Model in Cypress, and when is POM still the right call?
6. A test must prove the out-of-stock badge renders. Say what data that needs, pick its home from the
   four, and name what your choice stops proving.
7. A suite has grown to forty specs and the team has stopped trusting it. Name four organizing practices
   you would apply, and what each one fixes.
8. A pipeline is written with `npx cypress open`. Why can that never work in CI, what should the command
   be, and what one extra step turns a red build from "a test failed" into something you can actually
   diagnose?

**Answers:** (1) A custom command (`Cypress.Commands.add("login", ...)`) or a plain JS helper; use a
command when the code chains Cypress actions against the app, a helper for pure logic that returns
values — and for login, prefer `cy.request` + `cy.session` over driving the form every test. (2) It is
slow and couples every test to the creation flow, so one broken form fails unrelated tests; seed via
`cy.request` or `cy.task`, and test the UI creation flow exactly once. (3) A Chainable placeholder —
commands are enqueued, not executed inline; use `cy.get(...).its("length").as("count")` then
`cy.get("@count").then(...)`, or assert directly in the chain with `should`. (4)
`cy.intercept("GET", "/api/books*", { statusCode: 500 }).as("err")`, visit the page, `cy.wait("@err")`,
then assert the visible error text — the stub makes the failure path deterministic with no backend
changes. (5) Retry-able chains and the command log fit custom commands and app actions better than
element-returning page classes, so Cypress docs lean away from POM; POM still pays off for large
multi-contributor suites and teams carrying a Selenium convention. (6) One row whose stock is exactly
zero, plus whatever else the page needs to render; the cheapest home is a fixture served through a
`cy.intercept` stub, because zero-stock is then guaranteed rather than an accident of the database's
history — and the cost is that the test no longer proves the real API can produce that state, so a
seeded-live-data test should cover that separately. (7) One spec per user-facing flow (a failure names
its own flow); every test independent (any test can run alone, and a failure does not cascade);
duplication routed to its home — custom commands for flows, fixtures for data, page objects for
selectors (one edit per change instead of many); a selector policy of dedicated test attributes over
layout CSS (redesigns stop breaking tests); no fixed-millisecond waits (removes the flake-or-drag
trade); and keeping the run fast — programmatic login, stub what is not under test, artifacts out of
version control (a slow suite gets skipped, and a skipped suite is a deleted suite). (8) `open` launches
the interactive runner — a windowed application waiting for a human who is never coming, on a machine
with no display. CI must use `npx cypress run`, which is headless and exits non-zero on failure so the
pipeline can gate on it. The extra step is uploading `cypress/screenshots` as a build artifact under
`if: failure()` — Cypress captures them automatically, but they are destroyed with the CI container, so
without the upload you cannot see what the browser looked like when the test failed. Video adds more, but
it is off by default, so enable it before adding it to the upload path. (Also make sure the app is
actually running before the suite starts — `start-server-and-test` boots it, waits for the URL, and
propagates the test exit code.)

## Summary
- Custom commands (`support/commands.js`) DRY up app flows; plain helpers beat commands for pure logic;
  `cy.session` + `cy.request` beat both for repeated login.
- Test data: derive it from the test objective, then pick its home from four — fixture file (fast, can
  drift), the live database seeded to a known state in `beforeEach` (the default when the point is that
  the real system agrees), a `cy.intercept` stub (deterministic, stops proving the API), or created in
  the test and cleaned up (through the UI once per flow, never as shared setup); `cy.task` reaches the
  database when the API cannot.
- Organizing a suite: one spec per flow, independent tests, duplication routed to commands / fixtures /
  page objects, a decided selector policy, no fixed-millisecond waits, and a run fast enough to keep.
- Commands are enqueued chainables, not promises: no `const el = cy.get(...)`, no `await`; use `.then()`
  for values and `.as()`/`cy.get("@name")` for aliases.
- Wait on the thing itself: automatic retry for the DOM, `cy.wait("@alias")` for intercepted requests;
  bare `cy.wait(ms)` hides races and slows the suite.
- `cy.intercept` spies (real request, assert on `interception.request`/`response`) or stubs (fixtures,
  forced 500s for error UI); keep an unstubbed smoke layer so stubs cannot mask a broken backend.
- Forms: `.type()` with special keys like `{enter}`, `.clear()`, `.select()`, `.check()`, then assert
  validation messages and post-submit state.
- Debug with time-travel snapshots, `.debug()`, `cy.pause()`; headless runs capture screenshots and
  video. POM vs custom commands is a team-size call; Cypress Cloud adds recording, parallelization, and
  flake detection, and mochawesome is the common local HTML reporter.
- CI: `cypress run` (headless, non-zero exit on failure — the gate), the app started and waited for
  first via `start-server-and-test`, `npm ci` for a reproducible install, an explicit `--browser`, and
  screenshots uploaded as artifacts on failure or the red build is undiagnosable (video is off by
  default — enable it before archiving it). The dialect shown is GitHub Actions; the four ideas —
  checkout, install, start-and-test, archive — transfer to Jenkins and every other CI system.

## Resources
- [cy.intercept() (docs.cypress.io)](https://docs.cypress.io/api/commands/intercept)
- [Custom Commands (docs.cypress.io)](https://docs.cypress.io/api/cypress-api/custom-commands)
- [Best Practices (docs.cypress.io)](https://docs.cypress.io/app/core-concepts/best-practices)
- [Continuous Integration (docs.cypress.io)](https://docs.cypress.io/app/continuous-integration/overview)
