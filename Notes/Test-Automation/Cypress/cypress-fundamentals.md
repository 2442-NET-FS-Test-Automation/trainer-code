# Cypress Fundamentals: End-to-End Testing from Inside the Browser

## Learning Objectives
- Place Cypress in the E2E framework landscape (Selenium WebDriver, Playwright) and explain what its
  in-browser architecture buys and what it costs.
- Install Cypress into a project and navigate the standard skeleton: `cypress.config.js`,
  `cypress/e2e/`, support files, and fixtures.
- Run tests interactively (`cypress open`) and headless (`cypress run`) and know when each mode is used.
- Write specs with Mocha's `describe`/`it` and the full hook family (`before`, `beforeEach`, `afterEach`,
  `after`), knowing which hook cleanup belongs in and why, plus the core commands `cy.visit`, `cy.get`,
  and `cy.contains`, chaining assertions with `should`/`and`.
- Explain automatic retry-ability — why good Cypress code almost never calls `cy.wait(ms)` — and select
  elements with dedicated `data-cy` attributes.

## Why This Matters
Unit tests prove functions and components work in isolation; nothing about them proves the deployed app
actually lets a user log in, search the catalog, and check out a book. End-to-end tests drive a real
browser against the running application and assert on what a user would see — and they are historically
the slowest, flakiest tests a team owns, the ones that go red for no reason until people stop trusting
the build. Cypress attacks that flakiness architecturally rather than with sprinkled sleeps, which is
why it became a default choice for testing JavaScript front ends.

Interviewers use Cypress questions to probe whether you understand *why* E2E tests flake and how the
tool's design addresses it. "How does Cypress wait for an element?" comes up far more often than any
specific API call, and the answer falls straight out of the architecture covered here.

## The Concept

### The E2E landscape in three lines
- **Selenium WebDriver** — the veteran: tests run as an external process (in any language) and
  remote-control the browser over the W3C WebDriver protocol, one HTTP command at a time.
- **Playwright** — modern and also out-of-process, but driving browsers over faster debugging protocols;
  multi-language, and handles multiple tabs and origins natively.
- **Cypress** — JavaScript-only: your test code is *loaded into the browser* and runs in the same event
  loop as the application under test.

That third bullet is the one architectural fact everything else in this note follows from.

### In-browser architecture: what it buys and what it costs
Because the test lives inside the browser, Cypress has native access to the DOM, `window`, and the
network layer — no protocol round-trip per command. That makes it fast, and it makes the automatic
waiting described below possible, which is where the flake-resistance comes from. The costs are the
mirror image: one browser, one tab at a time (no multi-tab scenarios); same-origin constraints within a
test (visiting a second domain needs the `cy.origin` escape hatch — the standard follow-up is "how would
you test an OAuth redirect to a third-party login page?", and `cy.origin` is the one-line answer); and
tests must be JavaScript, because they *are* browser code. Teams needing multiple languages, multi-tab
flows, or heavy cross-origin testing pick Selenium or Playwright; teams shipping a JS front end usually
accept the constraints for the speed and stability.

### Installing and the project skeleton
Cypress installs as a dev dependency, and its first interactive launch scaffolds the project:

```bash
npm install cypress --save-dev
npx cypress open   # first run creates the cypress/ folder and config
```

The pieces that matter:

```text
cypress.config.js        # project config; e2e.baseUrl lives here
cypress/
  e2e/                   # your specs: *.cy.js
  support/e2e.js         # runs before every spec (global setup)
  support/commands.js    # custom command definitions
  fixtures/              # static test data (JSON)
```

```js
// cypress.config.js
const { defineConfig } = require("cypress");

module.exports = defineConfig({
  e2e: {
    baseUrl: "http://localhost:5173", // your dev server; cy.visit("/") resolves against this
  },
});
```

Setting `baseUrl` is not cosmetic: it lets every spec say `cy.visit("/")`, so pointing the suite at a
different environment is a one-line config change instead of a find-and-replace.

### Interactive vs headless: the two ways tests run
`npx cypress open` launches the interactive Test Runner: you pick a spec, watch it execute in a real
browser, and get the **command log** with **time-travel snapshots** — click any past command and the app
pane shows the DOM exactly as it was at that moment. This is the development and debugging mode.
`npx cypress run` executes every spec headlessly, prints a pass/fail table, and captures screenshots on
failure (and can record video) — this is the CI mode. Same tests, two front ends; a suite that only ever
runs in `open` mode on someone's laptop is not really automation yet.

### A first spec: Mocha structure and the core commands
Cypress bundles Mocha, so specs use `describe` blocks, `it` tests, and hooks like `beforeEach`:

```js
// cypress/e2e/catalog.cy.js
describe("book catalog", () => {
  beforeEach(() => {
    cy.visit("/"); // fresh page per test — tests must not depend on each other
  });

  it("filters the list from the search box", () => {
    cy.get("[data-cy=search-input]").type("pragmatic");
    cy.contains("The Pragmatic Programmer").should("be.visible");
    cy.get("[data-cy=book-card]").should("have.length", 1);
  });
});
```

`cy.visit` loads a page, `cy.get` selects by CSS selector, `cy.contains` selects by visible text — the
closest thing Cypress has to querying "the way a user reads the page."

The full hook family is Mocha's, and all four are available: `before` runs once before the first test in
its block, `beforeEach` before every test, `afterEach` after every test, `after` once after the last one.
Hooks nest with their `describe` blocks — an outer `beforeEach` runs before an inner one — which is how a
file-wide setup and a block-specific setup compose.

Put cleanup in `beforeEach`, not in `after`/`afterEach`. The reason is not that teardown hooks are
skipped on failure — Mocha runs them after a failing test. It is that (a) the interruptions you actually
meet do skip them: refreshing the runner mid-suite, killing a headless run, or a hook that throws; and
(b) tearing state down immediately after a failure destroys the evidence you need to diagnose it — the
half-created record is gone before anyone inspects it. Setting the world up before the test that needs
it always runs, and leaves the failure intact.

### Chaining: commands yield subjects
Cypress commands form chains in which each command **yields a subject** to the next link. `.type()` acts
on the element `cy.get` yielded and yields it onward; `.should()` asserts on whatever it receives, and
`.and()` is just another `should` for stacking assertions without repeating the query:

```js
cy.get("[data-cy=search-input]")
  .type("dune")
  .should("have.value", "dune")
  .and("have.focus");
```

One caution before you build intuition on the wrong model: these chains look like promises, but they are
not — Cypress enqueues commands and runs them later, which is why you cannot `await` them or store an
element in a `const`. The full mechanics live in the advanced Cypress note; here it is enough to know the
chain is a queue, not a promise chain.

### Retry-ability: the core concept
This is the idea to internalize above all others. Modern front ends render asynchronously — the book list
appears only after a fetch resolves. Selenium-era code handled that with explicit waits or `sleep(3000)`.
Cypress instead makes queries and assertions **automatically retry**: `cy.get(...).should(...)` re-runs
the query and the assertion together until both pass or a timeout (4 seconds by default,
`defaultCommandTimeout`) expires. The test above never says "wait for the fetch" — `should("have.length", 1)`
simply keeps retrying until the filtered render lands. This is why well-written Cypress code almost never
contains `cy.wait(ms)`: a fixed sleep is either too long (slow suite) or too short (flaky suite), while a
retried assertion is exactly as long as it needs to be. The interviewer follow-up is "what if 4 seconds
is not enough?" — raise the timeout per command (`cy.get(sel, { timeout: 10000 })`) or, better, wait on
the specific network request, which the advanced note covers with `cy.intercept`.

Assertions come in two forms. **Implicit**: `cy.get` by itself asserts the element exists (a failing
selector fails the test with no `should` attached). **Explicit**: `should`/`and` (or `expect` inside a
callback) state what you actually care about — visibility, value, count.

### Selectors: give tests their own attributes
Selecting by CSS class (`.btn-primary`) welds the test to styling; the next design pass breaks the suite
without breaking the app. The best practice is a dedicated attribute that exists *only* for tests —
`data-cy` (or `data-testid`) — added to the markup: `<button data-cy="checkout-button">`. Tests then
select `cy.get("[data-cy=checkout-button]")` and survive any refactor that preserves behavior. The cost
is real but small: developers must add and maintain the attributes as part of building the UI. The
follow-up worth knowing: some teams instead query by ARIA role and accessible name (Testing Library
style), which doubles as an accessibility check — a defensible alternative with the same
refactor-resistance goal.

### Common failures and what they mean
The first week with Cypress produces a predictable set of failures, and every one of them has a tell.

**`cy.visit()` fails with a connection error.** Cypress is working correctly and the application is not
there. Either the dev server is not running, or it started on a different port than the one in your
`baseUrl` — dev servers routinely fall back to the next free port when their default is taken, print the
real one, and nobody reads the line. Check the port the server actually printed against your config.

**`npx cypress open` reports the binary is missing.** The npm package in `node_modules` is only a thin
wrapper; the Cypress application itself is a separate binary downloaded on install into a global cache
(`~/.cache/Cypress`, or `%LOCALAPPDATA%\Cypress\Cache` on Windows). The dependency was recorded but that
download did not finish. Run `npx cypress install` to fetch it. On a CI machine, this is also what a
caching misconfiguration looks like — the cache was restored without the binary.

**`cy.contains("Submit")` matched the wrong element.** Two rules interact, and neither is "first in
document order." `contains` yields the **deepest** element containing the text, so a word inside a
`<span>` inside a `<button>` yields the span. But it also **prefers certain elements higher in the
tree** — `button`, `a`, `label`, and `input[type=submit]` — which is usually what you wanted and
occasionally not. Either way, the fix is to stop leaving it to the rules: use the two-argument form
(`cy.contains("button", "Submit")`) or scope it to a region first
(`cy.get("[data-cy=checkout]").contains("Submit")`).

**A test asserts on data that is not there, and the page looks empty.** Distinguish "the app rendered
nothing" from "the app could not fetch." An empty region usually means the API call failed, not that the
data is missing — open the runner's console pane: a failed network request points at infrastructure (API
down, database unreachable, CORS), while a `401` or `404` points at the request itself. Applications
frequently render the same generic message for both, which is why the console beats the screen here.

**The test passes but proves nothing.** `cy.get("[data-cy=row]")` with no `should` asserts only that the
element exists. If you meant "exactly one row," say so — an implicit existence check is the single easiest
way to write a green test that would not notice the bug it was written for.

**You reach for `cy.wait(3000)`.** Treat this as a design smell rather than a fix. A fixed sleep is too
long on a fast run and too short on a loaded machine. Whatever you were waiting for, assert on it instead:
retry-ability exists precisely so that the waiting is the assertion.

## Say It in an Interview
- *"Selenium and Playwright drive the browser from an outside process over a protocol; Cypress runs
  inside the browser next to the app. That makes it fast and lets it wait intelligently, at the cost of
  being JavaScript-only, one tab, and same-origin by default."*
- *"A Cypress project is `cypress.config.js` with a `baseUrl`, specs in `cypress/e2e`, shared setup in
  `support/`, and JSON test data in `fixtures/`. You develop in `cypress open` with time-travel
  snapshots and run headless in CI with `cypress run`."*
- *"The hooks are Mocha's: `before` once per block, `beforeEach` per test, `afterEach` per test, `after`
  once at the end, and they nest with the `describe` blocks. I put setup and cleanup in `beforeEach` —
  not because teardown hooks get skipped on failure, they don't, but because a stopped or refreshed run
  never reaches them, and cleaning up right after a failure throws away the state I need to debug."*
- *"Tests are Mocha structure — `describe`, `it`, `beforeEach` — with commands that chain: each command
  yields its subject to the next, so `cy.get('input').type('dune').should('have.value', 'dune')` reads as
  one sentence."*
- *"Cypress's core idea is retry-ability: queries and assertions automatically re-run until they pass or
  time out, so you almost never write `cy.wait(3000)` — fixed sleeps are either wasted time or a flake."*
- *"I select elements with dedicated `data-cy` attributes instead of CSS classes, so tests survive
  styling and markup refactors."*

## Check Yourself
1. What is the fundamental architectural difference between Cypress and Selenium WebDriver, and name one
   consequence in each direction (a benefit and a limitation).
2. Where does `baseUrl` live, and what does setting it change about how specs are written?
3. In `cy.get("input").type("dune").should("have.value", "dune")`, what does "commands yield subjects"
   mean concretely?
4. Why is `cy.wait(3000)` an anti-pattern, and what does Cypress do instead?
5. Why does `cy.get("[data-cy=search-input]")` beat `cy.get(".search-box input")` in a long-lived suite?
6. Name the four hooks and when each fires. Where does cleanup belong, and give a reason that is not
   "a failing test skips teardown"?

**Answers:** (1) Selenium runs tests in an external process controlling the browser over the WebDriver
protocol; Cypress runs the test code inside the browser with the app. Benefit: speed and automatic,
DOM-aware waiting; limitation: JavaScript-only, single tab, same-origin constraints (`cy.origin` for
exceptions). (2) In `cypress.config.js` under `e2e.baseUrl`; specs can then call `cy.visit("/")` with
relative paths, so retargeting environments is a config change. (3) `cy.get` yields the matched element
to `.type`, which acts on it and yields it onward to `.should`, which asserts on it — each link receives
the previous link's output. (4) A fixed sleep is too long or too short and hides the real condition;
Cypress retries queries and assertions automatically until they pass or the timeout (default 4s)
expires, so the test waits exactly as long as needed. (5) `data-cy` exists only for tests, so it survives
styling and structural refactors that would break class- or hierarchy-based selectors. (6) `before` once
before the first test in its block, `beforeEach` before every test, `afterEach` after every test, `after`
once after the last; they nest with `describe` blocks. Cleanup belongs in `beforeEach`: a run stopped or
refreshed mid-suite never reaches the teardown hooks, and tearing down straight after a failure destroys
the state needed to diagnose it.

## Summary
- Cypress is a JavaScript E2E framework that runs inside the browser alongside the app — fast and
  flake-resistant, but JS-only, single-tab, and same-origin by default.
- Install with `npm install cypress --save-dev`; `npx cypress open` scaffolds `cypress.config.js`
  (holding `e2e.baseUrl`), `cypress/e2e/`, `support/`, and `fixtures/`.
- `cypress open` is the interactive Test Runner with a command log and time-travel snapshots;
  `cypress run` is headless for CI, with screenshots and video on failure.
- Specs are Mocha (`describe`/`it` plus the `before`/`beforeEach`/`afterEach`/`after` hooks, which nest
  with their blocks) plus Cypress commands: `cy.visit`, `cy.get`, `cy.contains`; commands chain by
  yielding subjects, and `should`/`and` attach assertions. Cleanup goes in `beforeEach` — interrupted
  runs never reach teardown, and post-failure teardown destroys the evidence.
- Retry-ability is the core concept: queries and assertions re-run until they pass or time out, which is
  why good Cypress code has no fixed sleeps.
- `cy.get` carries an implicit existence assertion; `should`/`expect` make the real expectations
  explicit.
- Prefer dedicated `data-cy`/`data-testid` attributes over CSS classes for refactor-proof selection.

## Resources
- [Introduction to Cypress (docs.cypress.io)](https://docs.cypress.io/app/core-concepts/introduction-to-cypress)
- [Retry-ability (docs.cypress.io)](https://docs.cypress.io/app/core-concepts/retry-ability)
- [Install Cypress (docs.cypress.io)](https://docs.cypress.io/app/get-started/install-cypress)
