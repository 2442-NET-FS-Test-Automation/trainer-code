# QC-6 Test Automation — Cheat Sheet

Morning-of quick reference. Tables and minimal code, sourced from the Wk8–9 notes and rungs.
If a row surprises you, the study guide names the note that explains it.

---

## 1. Term pairs interviewers use as filters

| Pair | One-line separation |
|---|---|
| QA vs QC | QA = process, preventive ("PRs must pass the suite"); QC = product, detective (running the suite on a build). QA builds quality in; QC checks it out |
| Verification vs validation | Built the product *right* (vs spec) vs built the *right* product (vs need) |
| Error vs defect vs failure | Human mistake -> flaw in artifact -> observable wrong behavior; defect *may* cause failure |
| Positive vs negative | Valid input does the right thing vs invalid input is rejected gracefully — negative test PASSES on the 400 |
| Smoke vs sanity vs regression | Wide-shallow on a new build / narrow-deep on the change / broad net for everything that worked |
| Unit vs integration vs system vs UAT | One piece isolated / pieces wired / whole app / stakeholders sign acceptance |
| Black vs white vs gray box | No internals (users, QA) / full source (devs) / partial — API + schema (automation engineers) |
| Functional vs non-functional | Does it work vs how well — perf, load, security, usability, accessibility; NFRs need measurable targets |
| Manual vs automated | Wins on judgment (exploratory, usability) vs wins on repetition (regression, CI); automation = high upfront, near-zero marginal |
| Stub vs mock | Feeds data (state verification) vs verifies interaction (behavior verification) |

**Seven principles:** presence-not-absence; exhaustive impossible; test early; defects cluster;
pesticide paradox; context dependent; absence-of-defects fallacy.

**Pyramid:** many unit / fewer integration / fewest E2E. Up = more confidence, less speed, worse
localization. Anti-pattern: ice-cream cone.

## 2. Test design

**Test case fields:** identifier / trace / preconditions / steps / expected result written
BEFORE execution.

| Requirement shape | Technique | Produces |
|---|---|---|
| Limit ("at most", "between") | Boundary-value analysis | Edge, one below, one above |
| Input classes treated alike | Equivalence partitioning | One representative per class (valid + invalid) |
| Conditions combine ("A and B unless C") | Decision table | One case per rule; surfaces unasked combinations |
| Entity states ("pending/approved") | State transitions | Legal moves + illegal attempts |
| User sequence | Scenario testing | End-to-end path + deviations |
| Readable internals | Branch coverage | Cases reaching unexecuted branches |
| After all the above | Error guessing + exploratory | What the requirement never said |

**RTM:** requirements x cases. Empty row = coverage gap; empty column = waste; changed
requirement's row = impact analysis. Proves linkage, NOT adequacy.

**Coverage-optimized:** every requirement once before any twice; combine setup, never asserted
behavior; push cases down the pyramid; delete same-class redundancy. Cost = worse diagnosis.

**Test data:** objective picks WHAT (smallest set proving the one thing); object picks WHERE
(inline / file / seeded store / created-by-case / stub). Axis: determinism vs proof. No wall
clock, no unseeded random; cleanup in guaranteed teardown.

**Error guess format:** `Hypothesis / Source / Case / Result` — sourced from defect history,
fragile inputs (null, empty, boundary, max-length, Unicode), churn. Confirmed guess -> traced
regression case.

**Exploratory:** charter (narrow mission) + timebox (60–120 min) + session notes. Findings map
back: defect / requirements gap / surprise.

## 3. xUnit + Moq + coverage

```csharp
[Fact]
public void Checkout_ZeroStock_Throws() { /* Arrange, Act, Assert */ }

[Theory]
[InlineData(0, false)]  // boundary below
[InlineData(1, true)]   // boundary
[InlineData(3, true)]   // boundary
[InlineData(4, false)]  // boundary above
public void Quantity_RangeIsEnforced(int qty, bool ok) { ... }

result.Should().Be(expected);            // FluentAssertions
list.Should().NotBeEmpty().And.HaveCount(3);
act.Should().Throw<InvalidOperationException>();
```

| Lifecycle piece | Behavior |
|---|---|
| Constructor | Runs per TEST (fresh instance each test) — setup |
| `Dispose()` | Per-test teardown (`IDisposable`) |
| `IClassFixture<T>` | One shared instance across a test class |
| `ICollectionFixture<T>` | Shared across classes (`[Collection]`) |
| `ITestOutputHelper` | Inject for per-test output |

```csharp
var repo = new Mock<IInventoryRepository>();
repo.Setup(r => r.GetByIsbn("BK-001")).Returns(book);   // stub
var svc = new InventoryService(repo.Object);
svc.Checkout("BK-001");
repo.Verify(r => r.Save(It.IsAny<Inventory>()), Times.Once);  // mock verify
```

Seam rule: mock interfaces / virtual members; sealed + static resist.

```
dotnet test --collect:"XPlat Code Coverage"    # coverlet -> Cobertura XML
```
Line vs branch coverage; signal not target.

**TDD:** red (failing test first) -> green (least code) -> refactor. Value: design pressure,
executable spec, tight loop. Fits specified logic; not spikes/UI churn.

## 4. Integration testing (WebApplicationFactory)

```csharp
public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public ApiTests(WebApplicationFactory<Program> f) => _client = f.CreateClient();

    [Fact]
    public async Task Catalog_Returns200AndSeedTitles()
    {
        var resp = await _client.GetAsync("/inventory");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await resp.Content.ReadFromJsonAsync<List<InventoryDto>>();
        items.Should().NotBeEmpty();
    }
}
```

- Real pipeline: routing, model binding, filters, middleware, auth all ride along.
- Swap services for tests: `WithWebHostBuilder(b => b.ConfigureTestServices(...))`.
- Auth matrix through real HTTP: 401 no token / 403 wrong role / 200 right role.
- Validation: assert 400 + problem details.
- EF strategies: InMemory (fast, not a DB) / SQLite in-memory (real SQL) / live engine
  (+ transaction rollback per test for isolation).

## 5. Serialization + file I/O + logging (the cross-week rows)

```csharp
File.WriteAllText("book.json", JsonSerializer.Serialize(book));       // persist object
var back = JsonSerializer.Deserialize<Book>(File.ReadAllText("book.json"));
File.AppendAllText("log.txt", line);           // append vs replace
var lines = File.ReadAllLines("log.txt");      // read
```
Serialize = object -> text/bytes; file write = persistence (survives the process).

```csharp
_logger.LogInformation("Checkout {MemberId} {Isbn}", memberId, isbn);  // structured template
```
Severity ladder: Trace, Debug, Information, Warning, Error, Critical (Serilog: Verbose..Fatal).
Injected `ILogger<T>` = test seam; static `Log` is not. `builder.Host.UseSerilog()` bridges.

## 6. Cypress

```javascript
describe('catalog', () => {
  beforeEach(() => { cy.resetSeed(); cy.visit('/'); });     // custom command + visit
  it('filters by title', () => {
    cy.get('[data-cy=search]').type('clean');
    cy.get('[data-cy=book-card]').should('have.length', 1);  // retries until timeout
  });
});
```

| Concept | The line to say |
|---|---|
| Architecture | Runs IN the browser (vs Selenium's out-of-process WebDriver protocol) |
| Chaining | Commands are enqueued, not promises; each yields a subject |
| Retry-ability | Queries + assertions retry until timeout — no sleeps needed |
| Selectors | `data-cy` test-owned attributes; role-based = a11y-friendly alternative |
| Hooks | `before` / `beforeEach` / `afterEach` / `after`; cleanup in hooks that always run |
| Fixtures | `cy.fixture('users.json')` — versioned data files |
| Custom commands | `Cypress.Commands.add('login', ...)` — reusable behavior |
| Intercept spy | `cy.intercept('GET','/api/x').as('x')` ... `cy.wait('@x')` — real response observed |
| Intercept stub | `cy.intercept('GET','/api/x', {fixture:'x.json'})` — scripted response |
| Headless | `npx cypress run` (never `cy run`); `--browser chrome|edge` for the matrix |
| Component testing | `mount(<BookCard {...props}/>)` — real component, no app boot |
| Plugins | `setupNodeEvents` seam; `cy.task` crosses into Node; `@cypress/code-coverage` |
| Visual regression | Baseline -> diff -> threshold fail; baseline update = review step |
| CI (awareness) | Exit-code contract; app up first; upload screenshots/videos on red |

Suite-org checklist: spec-per-flow; independent tests; duplication homes = command / fixture /
page object; selector policy; no numeric waits; keep it fast.

## 7. Selenium (C#)

```csharp
using var driver = new ChromeDriver(options);      // Selenium Manager finds the driver
driver.Navigate().GoToUrl("http://localhost:5173/");
driver.Navigate().Back(); / .Forward(); / .Refresh();
```

| By | Use |
|---|---|
| `By.Id` / test attribute | First choice |
| `By.CssSelector` | Default workhorse |
| `By.TagName` / `ClassName` / `Name` | Simple cases |
| `By.LinkText` / `PartialLinkText` | Anchors by visible text |
| `By.XPath` | Text match, upward/sideways traversal |

**Find contract:** `FindElement` throws `NoSuchElementException`; `FindElements` returns empty
list — assert absence with the plural.

**XPath:** `//tag[@attr='v']`; `contains(@class,'card')`, `starts-with()`, `text()`; axes
`ancestor::`, `following-sibling::`. `[@class='card']` = EXACT string match (trap). Absolute
paths (`/html/body/...`) break on any change — never in specs.

```csharp
el.Click(); el.SendKeys("text"); el.Clear();
el.Text                       // rendered text
el.GetAttribute("value")      // an input's typed value lives HERE, not in .Text
el.Displayed / el.Enabled / el.Selected

new SelectElement(el).SelectByText("Fiction");   // real <select> only
new Actions(driver).MoveToElement(card).Perform();          // hover
new Actions(driver).DoubleClick(el).Perform();
```

**Waits:**

| Kind | Shape | Notes |
|---|---|---|
| Implicit | `driver.Manage().Timeouts().ImplicitWait = ...` | Driver-wide poll on every find |
| Explicit | `new WebDriverWait(driver, TimeSpan...).Until(d => condition)` | Specific condition at a specific point |
| Fluent | Same `WebDriverWait` + `PollingInterval` + `IgnoreExceptionTypes(...)` | .NET: `WebDriverWait` IS the FluentWait subclass |

Rules: never mix implicit + explicit (compounding timeouts) — taught end-state carries zero
implicit; never `Thread.Sleep`.

**Windows / alerts:**
```csharp
var handles = driver.WindowHandles;              // one active at a time
driver.SwitchTo().Window(handles.Last());
driver.SwitchTo().NewWindow(WindowType.Tab);
driver.SwitchTo().Alert().Accept();              // alerts live outside the DOM
```

**Exception bestiary — what each accuses:**

| Exception | Accusation |
|---|---|
| `NoSuchElementException` | Locator wrong, or asked too early |
| `StaleElementReferenceException` | DOM re-rendered under your reference — re-find |
| `ElementNotInteractableException` | Present but hidden/zero-size |
| `ElementClickInterceptedException` | Something overlays the target |
| `TimeoutException` | The wait's condition never held |

**Screenshots:** `((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(...)`; element-level
capture exists; capture-on-failure = guard wrapper saving `FAILED-<TestName>.png` + rethrow.

**POM:** page class = private locators + public intent-named methods; specs read as journeys;
shared plumbing in a base class (`E2ETestBase`). PageFactory (`[FindsBy]`/`InitElements`):
recognize it; deprecated in .NET — plain `By` + explicit waits is current.

**Ecosystem:** WebDriver / IDE (record-replay, brittle) / Grid (distribute). Framework types:
data-driven, keyword-driven, hybrid, BDD (Gherkin; SpecFlow -> Reqnroll).

## 8. Cypress vs Selenium (know your own comparison)

| Axis | Cypress | Selenium |
|---|---|---|
| Architecture | In-browser | Out-of-process, W3C WebDriver protocol |
| Waiting | Auto retry-ability | Explicit wait discipline on you |
| Language | JavaScript only | C#, Java, Python, JS... |
| Reach | Same-origin, one browser context | Tabs, windows, any browser |
| Debugging | Time-traveling runner | Screenshots, logs, exceptions |
| Sweet spot | Developer-loop E2E on your own SPA | Cross-language teams, multi-window flows, Grid scale |

P3 requires your own written comparison — cite it, not this table.
