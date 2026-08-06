# QC-6 Test Automation — Mock Interview Bank

Grouped by rubric section. Each entry: tier badge, the question, a concise model answer, the QC
objective it proves, and the source to study if the answer did not come out of you. Model answers
are *spoken-length* — if yours runs three times longer, trim it; if it is one fragment, grow it.

Practice protocol: answer out loud before reading the model. The "Say It in an Interview"
sections of the source notes are the extended bank behind this one.

---

## 1. Testing Philosophy

**[Must] What is software testing actually for — beyond finding bugs?**
Testing produces evidence that the system meets its expected behavior and that changes did not
break what already worked. It catches defects when they are cheap, it enables change — a
trustworthy suite is what lets you refactor without fear — and well-named tests document behavior
as an executable spec. And it shows the presence of defects, never their absence: a green suite
raises confidence, it is not a proof.
*Proves QC: Understand the importance of testing.*
*Source: `content/01-xunit/testing-fundamentals.md`, "What testing is, and why".*

**[Must] Walk me through the testing process end to end.**
Plan — scope, risk, exit criteria. Design cases — turn requirements into traced cases with
expected results; the techniques live here. Prepare data and environment. Execute. Report and
triage defects. Close against the exit criteria with a residual-risk verdict. It loops: a defect
in execution can send you back to design. Automation buys the execute stage plus seeding and
reporting; planning, technique choice, and triage stay human.
*Proves QC: Understand the testing process.*
*Source: `testing-fundamentals.md`, "The testing process, end to end".*

**[Must] Quality Assurance versus Quality Control — and which one is your automated suite?**
QA is about the process: preventive practices like definition of done, code review standards, CI
gates. QC is about the product: checking a specific build against its requirements. QA builds
quality in, QC checks quality out. The suite's execution against a build is QC; the standing rule
that every pull request must pass it before merging is QA.
*Proves QC: Describe Quality Assurance and Quality Control.*
*Source: `testing-fundamentals.md`, "Quality assurance vs quality control".*

**[Must] Give me an example of negative testing, and tell me when a negative test passes.**
Submitting a checkout with quantity -1 and asserting the API returns 400 with a useful message.
It passes when the system correctly *rejects* the input — graceful, predictable refusal is the
expected result. The sorting question I ask myself: would a correct system accept this input?
If no, it is a negative case.
*Proves QC: Define and differentiate between positive and negative testing.*
*Source: `testing-fundamentals.md`, "Positive vs negative testing".*

**[Must] When would you not automate a test?**
When judgment or novelty is the point — exploratory sessions, usability — or when the check runs
once and never repays the scripting cost, or when the feature is so volatile the test would need
constant rewriting. Automation is expensive upfront and nearly free per run, so it wins on
repetition; manual wins on judgment.
*Proves QC: Differentiate between Automated and Manual Testing.*
*Source: `testing-fundamentals.md`, "Manual vs automated".*

**[Must] Why do requirements matter to a tester?**
Three ways. They are the oracle — without a stated expectation there is no such thing as wrong
behavior. They are what every case traces to, and rolled up that mapping is the RTM, which shows
untested requirements and tests nobody asked for. And they gate acceptance. Testability is a
property of the requirement itself: "the page should be fast" cannot fail a test; "under 300 ms
at p95" can.
*Proves QC: Explain what software requirements are and their importance in testing.*
*Source: `testing-fundamentals.md`, "Software requirements and their role in testing".*

**[Should] Pick two testing principles and tell me where they change what you do.**
Defects cluster — so I weight effort toward recently changed, complex, high-churn modules instead
of spreading cases evenly. Pesticide paradox — a suite that never changes stops finding anything,
so case sets have a shelf life and need new cases and techniques over time, not just re-runs.
*Proves QC: Understand and be able to talk about Testing Principles.*
*Source: `testing-fundamentals.md`, "Testing principles, and where they bite".*

**[Should] What is a tester's mindset, in one minute?**
Professional skepticism in service of evidence: assume defects exist and go find where they
cluster, prefer "no known defects in what we tested" over "it works", think in risk — what breaks
worst, what changed last — and treat a surprising pass as suspicious as a failure. It is
adversarial toward the software and collaborative toward the team.
*Proves QC: Be able to describe a testing mindset from a Tester's perspective.*
*Source: `testing-fundamentals.md`, objectives + principles sections.*

**[Nice] A developer typo ships and a user gets wrongly blocked. Name the error, defect, and failure.**
The error is the human mistake — typing `<` for `<=`. The defect is the flaw it left in the
artifact — the wrong comparison in the source. The failure is the observable wrong behavior at
run time — the member being refused a loan they are entitled to. Error causes defect; defect
*may* cause failure — a defect on an unreachable path never fails, which is why green tests are
not proof.
*Proves QC: Understand the difference between defect, error, failure.*
*Source: `testing-fundamentals.md`, "Defect, error, failure".*

**[Nice] Verification versus validation?**
Verification: are we building the product right — does it conform to spec; reviews, unit and
integration tests. Validation: are we building the right product — does it meet the actual need;
UAT and beta. The spec can be correctly implemented and still wrong for the user — that is the
absence-of-defects fallacy, and it is why both exist.
*Proves QC: Explain the difference between verification and validation.*
*Source: `testing-fundamentals.md`, "Verification vs validation".*

---

## 2. Designing Test Cases

**[Must] Here is a requirement: "a checkout is for 1 to 3 books." Design the cases and name your techniques.**
The shape is a bounded range, so two techniques together. Equivalence partitioning gives three
classes — below range (invalid), 1–3 (valid), above range (invalid). Boundary-value analysis puts
cases on the edges: 0, 1, 3, 4. So roughly: 0 rejected, 1 accepted, 3 accepted, 4 rejected, plus
one interior representative. In xUnit those become `[Theory]` rows with the technique named in
the trace.
*Proves QC: Apply black-box testing techniques to validate application behavior; Recognize
appropriate general testing techniques to utilize based on documented requirements.*
*Source: `content/01-xunit/xunit-fundamentals.md` (EP + BVA); walkthrough `01` Step 6.*

**[Must] What is an RTM and what can it tell you that "200 tests, all passing" cannot?**
A grid of requirements against test cases. Read along a requirement row: an empty row is a
coverage gap — a stated requirement nobody tests, which "200 passing" actively conceals. Read
down a case column: an empty column is waste or an undocumented rule. And when a requirement
changes, its row is the impact analysis. The caveat I volunteer: it proves linkage, not adequacy
— one weak case fills a row.
*Proves QC: Design structured test cases using appropriate techniques aligned with an RTM.*
*Source: `content/01-xunit/test-case-design.md`, "The requirements traceability matrix".*

**[Must] Explain the test pyramid and why it is that shape — using your own project.**
Many unit tests at the base, fewer integration, fewest end-to-end. Going up, each test buys more
confidence — real wiring, real browser — but costs speed, stability, and defect localization: a
red E2E test says "something broke somewhere", a red unit test names the method. In P3 we had
xUnit units on the services, WebApplicationFactory integration through real HTTP, and
Cypress/Selenium flows on top — few of those, on the critical paths. The anti-pattern is the
ice-cream cone: mostly E2E, slow pipeline, red builds nobody trusts.
*Proves QC: Explain the structure and purpose of the testing pyramid.*
*Source: `testing-fundamentals.md`, "The test pyramid"; your P3 suite.*

**[Must] What does white-box testing add on top of black-box, concretely?**
Black-box derives cases from the requirement, so it can only reach what the requirement talks
about. White-box reads the code: run coverage, find the branch no case executes, and write a case
that reaches it — named as a branch-driven case. We did that with coverlet: `dotnet test
--collect:"XPlat Code Coverage"`, read the Cobertura report, add the missing-branch case. The
discipline: coverage is a signal, not a target — a covered line with no assertion proves nothing.
*Proves QC: Apply white-box testing techniques to verify internal logic and flow.*
*Source: `content/01-xunit/moq-coverage-service-testing.md` (coverage section); walkthrough `01`
Step 10.*

**[Must] You have eight cases of time and twelve requirements. What do you do?**
Cover by risk, once each, before covering anything twice — the three requirements in the module
with recent churn first, because defects cluster, then the highest-risk remainder. Then the four
uncovered requirements are reported explicitly as empty RTM rows — declared residual risk, a
decision on record, not a silence.
*Proves QC: Design test cases optimized to minimize time and effort required for creation while
maintaining high coverage.*
*Source: `test-case-design.md`, "Designing for coverage per case".*

**[Must] How do you decide what test data a case needs and where it lives?**
Two separate decisions. The test *objective* picks the data: the smallest set that proves the one
thing — to prove out-of-stock rejection, one title with zero stock, not a realistic catalog. The
test *object* picks the home: inline literals, a fixture file, a seeded store, created-by-the-
case, or a stub — and the axis is determinism versus proof. Stubs are repeatable and stop proving
the real system agrees; seeded data proves it and charges cleanup discipline. Plus two habits:
deterministic by construction — no wall clock, no unseeded random — and cleanup in guaranteed
teardown, because teardown after the assertions skips exactly when the test fails.
*Proves QC: Create test data determined by test objectives and test objects; Organize test data
into appropriate storage solutions determined by the test object.*
*Source: `test-case-design.md`, "Test data"; `cypress-advanced.md`, test-data section.*

**[Should] What makes error guessing a technique instead of a hunch?**
Sourcing and documentation. The guesses come from past defect history, known-fragile input
classes — null, empty, boundary, max length, odd Unicode — domain knowledge, and churn. Each one
is written as hypothesis / source / case / result, so another tester can rerun the exercise and
get comparable output. And a confirmed guess graduates into a permanent regression case with a
requirement trace.
*Proves QC: Use domain knowledge and past defect patterns to hypothesize and document likely
failure points for error guess testing; Perform structured and repeatable error guess testing.*
*Source: `test-case-design.md`, "Error guessing, made repeatable"; P3 hunting record.*

**[Should] Your teammate says exploratory testing is "just clicking around." Defend it — and criticize it.**
It is structured: a charter states the mission before the session, a timebox makes it schedulable
and comparable, and session notes are the deliverable — coverage, observations, defects,
questions. Findings map back three ways: a defect against a requirement, a requirements *gap* —
behavior nobody specified, which no scripted case could ever surface — or surprising-but-correct
behavior. The honest drawbacks: coverage depends on the tester, reproduction depends on the
notes, and it does not automate. So scripted proves the requirements are met; exploratory probes
whether the requirements were right. You run both.
*Proves QC: Plan and document exploratory testing strategies; Perform structured and repeatable
exploratory testing; Understand the benefits and drawbacks of exploratory and error guess
testing.*
*Source: `test-case-design.md`, "Exploratory testing"; P3 session notes.*

**[Nice] Walk me through choosing techniques for: "a discount applies to orders of 10+ items, unless the account is suspended."**
Two shapes. "10 or more" is a boundary — cases at 9, 10, 11. The suspension is a condition
combining with quantity — a decision table: suspended crossed with above/below threshold, four
rules. The table's payoff is the interaction row nobody wrote down: suspended at 25 items — no
discount — and the case that proves the exception overrides the quantity in both directions.
When a combination's expected result is not derivable from the requirements, that is a
requirement defect found at design time, the cheapest place to find one.
*Proves QC: Walk through a testing scenario and select optimal techniques and practices.*
*Source: `test-case-design.md`, "Selecting a technique from the requirement's shape".*

---

## 3. Testing and Logging .NET Applications

**[Must] Show me the shape of a good unit test.**
Arrange-Act-Assert, one behavior per test, named so the red entry in the runner reads as a
sentence — subject, scenario, expectation. Arrange builds the smallest world that proves the one
thing, act is a single call, assert states the expected result — with an assertion whose failure
message is informative: `result.Should().BeEquivalentTo(expected)` over a chain of bare
equality checks.
*Proves QC: Use a testing framework to create meaningful unit tests for an application.*
*Source: `xunit-fundamentals.md`; walkthrough `01` Step 5 (`TokenServiceTests`).*

**[Must] Unit versus integration testing — where is the line, in your own code?**
A unit test exercises one piece in isolation with its dependencies replaced — our service tests
mocked the repository with Moq. An integration test proves real pieces wired together: we booted
the actual API in memory with `WebApplicationFactory<Program>` and asserted through real HTTP —
status codes, JSON bodies, the auth pipeline, middleware. The integration tier catches what unit
tests structurally cannot: routing, configuration, serialization, filter and middleware order.
*Proves QC: Can distinguish between unit and integration testing.*
*Source: `content/01-xunit/integration-testing.md`; walkthrough `02` Steps 1–6.*

**[Must] How do you log vital events, and what makes logging test-relevant?**
Through the injected abstraction — `ILogger<T>` — at the right severity: the ladder from Trace to
Critical is the filter production actually uses. Structured templates, not string interpolation:
`_logger.LogInformation("Checkout {MemberId} {Isbn}", ...)` keeps the properties queryable.
Serilog plugs in under the same interface via `builder.Host.UseSerilog()`. Test-relevant because
the injected logger is a seam: you can hand the class a capturing fake and assert the vital event
was recorded — a static `Log` class is process-global and is not a seam.
*Proves QC: Use logging frameworks to record vital events within a running application.*
*Source: walkthrough `02` Step 6b; `EFCore-REST-SOAP/content/05-observability-patterns/
serilog-structured-logging.md`.*

**[Must] Serialize an object to a file and get it back. What are the moving parts?**
`JsonSerializer.Serialize(obj)` turns the object into JSON text; `File.WriteAllText` puts it on
disk — that is the persistence step, it survives the process. Reading back is
`File.ReadAllText` then `JsonSerializer.Deserialize<T>`. Same idea with binary formats for
bitstreams; JSON is the common case. We used the deserialize half constantly — every
`GetFromJsonAsync<T>` in the integration suite, and model binding is deserialization plus
validation on every request.
*Proves QC: Can serialize/deserialize objects to common formats such as bitstreams or JSON;
Basic file I/O; File I/O - serializing objects to a file to persist data.*
*Source: `Agile-Git-CoreCSharp/content/1-Thursday/os-cli-file-io.md`;
`Intermediate-CSharp/content/4-Thursday/async-http-networking.md`; walkthrough `02` Steps 3, 7.*

**[Should] Fact versus Theory?**
`[Fact]` is one invariant case. `[Theory]` runs once per data row — `[InlineData]` supplies the
rows — and the discipline is that the rows come from somewhere: equivalence partitions and
boundary values, one row per class and edge, so the parameterization is a technique made
executable, not copy-paste.
*Proves QC: Can explain and apply Fact and Theory attributes in xUnit.*
*Source: `xunit-fundamentals.md`; walkthrough `01` Step 6.*

**[Should] What does a mocking framework buy you, and what is the difference between a stub and a mock?**
Isolation: the unit under test gets scripted collaborators, so the test fails only for its own
reasons. With Moq: `new Mock<IInventoryRepository>()`, `.Setup(...).Returns(...)` to feed data,
`.Verify(...)` to prove an interaction happened. That is also the stub/mock line: a stub supports
*state* verification — feed data in, assert the output; a mock supports *behavior* verification —
assert the collaborator was called correctly. The seam requirement: interfaces or virtual
members; sealed and static resist mocking, which is a design signal.
*Proves QC: Uses mocking frameworks to isolate dependencies in tests; Can create and use stubs
to simulate external behavior in tests.*
*Source: `moq-coverage-service-testing.md`; walkthrough `01` Step 8.*

**[Should] What is TDD and what does it actually buy?**
Red-green-refactor: write the failing test first, write the least code that passes, refactor
under a green bar. It buys design pressure — test-first forces you to invent the seam before the
implementation, so testable design falls out — plus an executable spec and a tight feedback
loop. It fits best on well-specified logic; it fits badly on exploratory spikes and churning
UI. Honest version of my experience: I know the loop, and our demos deliberately practiced
test-after — so I would say I understand TDD's value rather than claim it as my daily habit.
*Proves QC: Understands the purpose and value of test-driven development (TDD).*
*Source: `xunit-fundamentals.md`, "Test-driven development: writing the test first".*

**[Should] How do you test EF Core code without hitting a production-shaped database every time?**
Three strategies, honesty increasing with cost. The InMemory provider is fast and not a database
— no relational semantics. SQLite in-memory gives real SQL through the same code. The real engine
gives full fidelity, and then isolation is the question: fresh database per test, or shared
database with a transaction rolled back per test. We ran SQLite for the service tier and the live
engine with rollback for the top tier.
*Proves QC: (supports) distinguish unit vs integration; meaningful unit tests.*
*Source: `content/01-xunit/efcore-testing-strategies.md`; walkthrough `02` Steps 8–10.*

---

## 4. Testing Applications with Cypress

**[Must] What makes Cypress different from Selenium architecturally, and what does that buy and cost?**
Cypress runs *inside* the browser alongside the app; Selenium drives the browser from outside
over the WebDriver protocol. Inside buys automatic retry-ability, native access to the app, no
driver management, great debugging — the time-traveling runner. It costs reach: one browser
context, same-origin constraints, JavaScript only. Which is why the comparison is a portfolio
question, not a winner question — I have written the same flow in both and the trade is visible.
*Proves QC: (frames the whole section; pairs with Selenium pros/cons row).*
*Source: `content/02-cypress/cypress-fundamentals.md`, architecture section; your P3
Cypress-vs-Selenium comparison.*

**[Must] Why does a well-written Cypress test need no sleeps?**
Retry-ability. Commands and assertions retry until they pass or time out — `cy.get` keeps
querying, `.should` keeps asserting. For network timing you wait on the event itself:
`cy.intercept` the route, alias it, `cy.wait('@alias')`. A `cy.wait(3000)` is the smell that
says the author did not know what they were waiting *for*.
*Proves QC: Handle asynchronous behavior and API requests within Cypress tests.*
*Source: `cypress-fundamentals.md`, retry-ability; `cypress-advanced.md`, intercept section.*

**[Must] How do you select elements so the suite survives a redesign?**
Give tests their own contract: `data-cy` attributes, selected with `cy.get('[data-cy=...]')` —
the attribute exists only for tests, so styling refactors cannot break it. Role-based selectors
are the defensible alternative and double as an accessibility check. Never bind to CSS classes
or DOM position.
*Proves QC: Use Cypress commands for element selection and traversal effectively.*
*Source: `cypress-fundamentals.md`, selectors section.*

**[Must] What do fixtures and custom commands each solve?**
Fixtures are versioned test data — JSON files loaded with `cy.fixture`, one edit updates every
consumer. Custom commands are reusable *behavior*: `Cypress.Commands.add('login', ...)` — ours
were `cy.resetSeed()` and `cy.login()`, so every spec starts from a known state without
repeating the plumbing. Data reuse versus logic reuse — different duplication, different home.
*Proves QC: Implement Cypress fixtures and custom commands for reusable test logic.*
*Source: `cypress-advanced.md`; walkthrough `04` Steps 2–3.*

**[Must] A spec fails in headless CI but passes when you watch it. How do you debug it?**
First reproduce with the runner: interactive mode's command log time-travels — click any command
and see the DOM snapshot at that moment, with browser devtools open on the runner. Then the
usual suspects for headless-only failures: timing masked by watching (fix with event waits, not
sleeps), viewport differences, state leaking between tests. Headless runs also leave screenshots
and videos as artifacts — that is what they exist for.
*Proves QC: Debug failing tests using Cypress Test Runner and browser developer tools.*
*Source: walkthrough `04` OF-3; `cypress-advanced.md` debugging section.*

**[Must] How would Cypress run in a CI pipeline?**
`npx cypress run` headless, and the exit code is the contract — non-zero fails the job. The app
under test has to be up before the suite runs, so the job starts it (or a service does), then
runs Cypress, then uploads screenshots and videos on failure as artifacts. Our notes carry a
sample GitHub Actions workflow with exactly that shape. I have not wired the pipeline myself
yet — that is our next block — but the contract and the failure-artifact pattern are clear.
*Proves QC: Integrate Cypress tests into CI/CD pipelines (awareness scope — see register).*
*Source: `cypress-advanced.md`, "Running in a CI pipeline".*

**[Should] Spy versus stub with cy.intercept?**
A spy observes: `cy.intercept('GET', '/api/inventory').as('load')` lets the real response
through and lets me wait on it and assert against what actually happened. A stub replaces:
give the intercept a scripted response and the test controls the data — including states the
real API will not produce on demand, like a 500. Spy proves the integration; stub buys
determinism. Choose per test objective.
*Proves QC: Use Cypress intercepts to stub, spy, and mock network requests.*
*Source: `cypress-advanced.md`, intercept section; `intercept.cy.js` (rung `12`).*

**[Should] What did component testing add on top of your E2E suite?**
A middle rung: `mount()` renders one real component — our `BookCard`, `SearchBar` — with
controlled props and spy callbacks, in a real browser, without booting the app. Faster and more
precise than E2E, more realistic than a pure unit test. It is the pyramid applied inside the
front end.
*Proves QC: Apply Cypress testing strategies for component-level testing.*
*Source: walkthrough `06` Steps 2–4.*

**[Should] Visual regression — how does it work and what is the maintenance catch?**
Baseline screenshot on the first run; every later run diffs against it and fails on pixel drift
past a threshold — we broke it deliberately with a CSS mutation and watched the red diff, then
reverted. The catch is baseline management: an *intended* redesign also fails the suite, so
updating baselines is a review step, and flaky rendering (fonts, animation) needs masking or
thresholds.
*Proves QC: Integrate visual regression testing into Cypress workflows.*
*Source: walkthrough `06` Step 7 (`cypress-image-diff-js`).*

**[Should] Plugins and cross-browser, briefly.**
`setupNodeEvents` in the config is the plugin seam — test code runs in the browser, `cy.task`
crosses to Node for anything the browser cannot do. Code coverage rode that seam:
`vite-plugin-istanbul` instruments the app, `@cypress/code-coverage` collects per test, and the
report shows what the E2E suite actually exercises. Cross-browser is the same suite under
`npx cypress run --browser chrome` or `edge` — the value is the matrix, the cost is run time,
which is what CI parallelization is for.
*Proves QC: Leverage Cypress plugins to extend functionality; Use Cypress for cross-browser
testing.*
*Source: walkthrough `06` Steps 5–6, 8.*

---

## 5. Web Automation with Selenium

**[Must] What is the Selenium ecosystem — the parts and what each is for?**
WebDriver: the library — your language drives real browsers over the W3C WebDriver protocol.
IDE: browser-extension record and replay — fast capture, brittle output, useful for prototyping
a locator, not for a maintained suite. Grid: distributes one suite across machines and browsers
for parallel and cross-browser runs. Around it, framework styles — data-driven, keyword-driven,
hybrid, BDD with Gherkin — are how teams structure the code that uses WebDriver.
*Proves QC: Understands the components of the Selenium ecosystem; Selenium IDE; Selenium Grid.*
*Source: `content/03-selenium/selenium-ecosystem.md`.*

**[Must] How does a Selenium test project come together in .NET?**
An xUnit project with two NuGet packages — Selenium.WebDriver and Selenium.Support. Selenium
Manager resolves the matching browser driver automatically now, which is why the manual
chromedriver dance — download, version-match, PATH — is the fallback you know rather than the
default you do. `new ChromeDriver()` opens the session; `Quit()` in `Dispose`, always, or you
orphan browsers. `ChromeOptions` customizes launch — headless for CI-style runs.
*Proves QC: Incorporate Selenium WebDriver into a project utilizing the automated Driver
management; Manually configure and instantiate a WebDriver object; Use option classes.*
*Source: `content/03-selenium/selenium-intro.md`; walkthrough `05` Steps 4–7.*

**[Must] FindElement versus FindElements — and how do you assert an element is absent?**
`FindElement` returns the first match or *throws* `NoSuchElementException`. `FindElements`
returns a collection — empty on no match, no exception. So absence is asserted with
`FindElements(...).Should().BeEmpty()` — never a try/catch around `FindElement`, and never an
exception as control flow.
*Proves QC: Utilize appropriate find methods for accessing web elements in Selenium scripts.*
*Source: `content/03-selenium/selenium-locators-navigation.md`, find-methods contract;
walkthrough `07` Step 5.*

**[Must] Your locator strategy, in preference order — and where does XPath earn its place?**
Test-dedicated attribute or id first, CSS selector next — fast, readable, resilient. XPath where
CSS cannot go: matching on text (`text()`, `contains()`), or traversing *up* or sideways —
`ancestor::`, `following-sibling::`. Absolute XPath never: one structural change breaks it.
And the trap I would flag in review: `[@class='card']` is exact-string matching — an element
with `class="card featured"` does not match; `contains(@class, 'card')` is the substring form,
with its own false-positive caveat.
*Proves QC: Utilize appropriate locator strategies; Apply Xpath functions to locate dynamic
elements; Understand and apply absolute and relative Xpath expressions.*
*Source: `content/03-selenium/selenium-xpath.md`; walkthrough `07` Steps 7–9.*

**[Must] Implicit, explicit, fluent — define all three, then tell me your policy.**
Implicit: one driver-wide setting — every `FindElement` polls up to that long before giving up.
Explicit: `WebDriverWait` with a lambda condition — wait for *this specific state* at this point:
element visible, text changed, list non-empty. Fluent: explicit waiting with tuned
`PollingInterval` and `IgnoreExceptionTypes` — and in .NET, `WebDriverWait` literally *is* the
FluentWait subclass; there is no separate class like Java's. Policy: explicit everywhere,
implicit zeroed — mixing the two compounds timeouts unpredictably. And never `Thread.Sleep`:
it always waits the full duration and still races on a slow day.
*Proves QC: Understand and implement implicit waits; explicit waits; fluent waits.*
*Source: `content/03-selenium/selenium-interactions-waits.md`, the waits arc; walkthrough `08`
Steps 7–9.*

**[Must] Tell me about a flaky test you have actually seen, and what fixed it.**
Our race demo: a spec navigated to the catalog and immediately asserted the rendered list was
non-empty — no wait. The React app fetches from the API after first paint, so the assertion
races the render: it failed in class at 25 of 26, and passed on a warm re-run the same
afternoon. Both outcomes are legal, which is the definition of flaky. The fix was waiting on
the condition, not the clock: a `WebDriverWait` until the list has items, and the race is gone
regardless of machine speed.
*Proves QC: debug and troubleshoot common Selenium errors; explicit waits.*
*Source: walkthrough `08` Step 7 (`WaitTests.cs`, rung `16`).*

**[Must] StaleElementReferenceException — what happened and what do you do?**
I held a reference to an element and the DOM re-rendered underneath it — with a React SPA,
constantly. The reference points at a node that no longer exists. Fix: re-find after the
re-render, or wait on a condition that re-queries inside the lambda rather than closing over
the old element. The exception bestiary generally is diagnosis: NoSuchElement accuses the
locator or the timing, ElementNotInteractable accuses visibility, ClickIntercepted accuses an
overlay, Timeout accuses the wait's condition.
*Proves QC: debug and troubleshoot common Selenium errors.*
*Source: `content/03-selenium/selenium-windows-alerts-exceptions.md`, the bestiary; walkthrough
`09` Step 6.*

**[Must] What problem does the Page Object Model solve, and what does yours look like?**
Locator and interaction duplication. When fifteen specs each know the sign-in form's selectors,
one UI change is fifteen edits. A page class captures the page once — locators private,
intent-named methods public — and specs read as user journeys: `LoginPage.SignInAs(user)` then
`CatalogPage.Search("clean code")`. Shared plumbing — driver setup, waits, capture-on-failure —
lives in a base class; ours is `E2ETestBase`. And PageFactory: I recognize `[FindsBy]` +
`InitElements` on sight, and I know it is deprecated in .NET Selenium — plain `By` fields with
explicit waits are the current idiom.
*Proves QC: Organize code using the page object model design pattern.*
*Source: `content/03-selenium/selenium-pom-design-patterns.md`; walkthrough `09` Steps 8–10.*

**[Should] Select class and Actions API — when does each come out?**
`Select` wraps a real `<select>` element: `SelectByText`, `SelectByValue`, `SelectByIndex`,
multi-select support — the wrapper exists because option-clicking by hand is browser-flaky.
Actions is for gestures a single click cannot express: hover (`MoveToElement`), double-click,
click-and-hold, keyboard chords — composed, then `Perform()` executes the chain.
*Proves QC: Utilize the Select class; Perform complex user interactions using the Actions API.*
*Source: `content/03-selenium/selenium-interactions-waits.md`; walkthrough `08` Steps 5–6.*

**[Should] Windows and alerts — how does the driver deal with each?**
Windows are handles: the driver talks to exactly one at a time. A `target=_blank` link makes a
second handle appear in `WindowHandles`; you `SwitchTo().Window(handle)` to it, work, close,
and switch back. Alerts live *outside* the DOM — no locator reaches them:
`SwitchTo().Alert()` then `Accept()`, `Dismiss()`, or `SendKeys` for a prompt. Forgetting to
switch back — in either mechanism — is the classic follow-on failure.
*Proves QC: Manage browser window contexts during code execution; Handle browser alerts during
code execution.*
*Source: `content/03-selenium/selenium-windows-alerts-exceptions.md`; walkthrough `09` Steps
4–5.*

**[Should] How do screenshots earn their place in a suite?**
Two uses. Ad hoc: `GetScreenshot()` on the driver captures the viewport, or capture a single
element — useful while building. Systematic: capture-on-failure — our base class wraps the act
in a guard that saves `FAILED-<TestName>.png` and rethrows, so every red test leaves evidence
of what the browser actually showed. That is what turns "it failed in CI at 2am" from a
mystery into a picture.
*Proves QC: Capture screenshots during code execution.*
*Source: walkthrough `07` Step 10, `09` Steps 3 and 9.*

**[Nice] Sell me Selenium — then talk me out of it.**
For: any real browser, any major language, out-of-process realism over the W3C standard
protocol — multiple tabs, multiple origins — and the largest ecosystem and hiring base in
browser automation. Against: more setup and plumbing than Cypress, no built-in retry-ability
so wait discipline is on you, and a slower feedback loop while developing tests. Portfolio
answer: we run both — Cypress for developer-loop E2E, Selenium where language and reach
matter; my P3 comparison doc walks the same flow in both.
*Proves QC: Evaluate the pros and cons of using Selenium for automated testing.*
*Source: `selenium-intro.md`, `selenium-ecosystem.md`; your P3 comparison.*
