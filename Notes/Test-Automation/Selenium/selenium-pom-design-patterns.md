# Page Object Model and Design Patterns in Test Suites

## Learning Objectives
- Organize Selenium code with the Page Object Model: one class per page, selectors as named `By`
  fields, actions that return page objects, page objects that own their readiness waits, and
  specs blind to URLs, selectors, and waits.
- Recognize PageFactory on sight in Java codebases (`@FindBy`, `initElements`) and give the
  accurate .NET answer: the official binding removed it, and plain page objects are the
  recommended shape.
- Structure shared test plumbing with a base class (driver lifecycle, unconditional cleanup) and
  know its limits (the non-virtual Dispose gap, ctor-failure leaks).
- Name the design patterns a mature UI suite already uses — facade (page objects), factory,
  builder, template method — and the shared-driver singleton as the anti-pattern.
- Apply the duplication-routing rule: repeated flows, repeated data, and repeated selectors each
  have a named home.

## Why This Matters
A UI suite's first fifty tests can survive on discipline; the next two hundred survive on
STRUCTURE. Selectors pasted into every test mean a redesign breaks the suite in forty places at
once; driver plumbing copied into every class means a browser flag change is a forty-file edit.
The Page Object Model is the industry's standard answer, it is named on job descriptions and in
interviews ("do you use page objects?" / "do you use PageFactory?"), and — quietly — it is a
design-patterns education: a team that builds a good test suite has used half the classic
catalog without opening the book. Being able to NAME what you built is what turns experience
into interview currency.

## The Concept

### The problem POM solves
The same selector string in five test files is five couplings to one UI decision. When the search
box changes, five files break separately, each with its own confusing failure. Multiply by every
element the suite touches and "we redesigned the header" becomes a week of test repair. The
maintenance bill of a UI suite IS its selector duplication.

### Page Object Model: the shape
One class per page (or per significant component). Selectors, actions, and waits live in the
class; tests speak user intent.

```csharp
public class CatalogPage
{
    private readonly IWebDriver _driver;

    // Selectors: named, private, in ONE place.
    private static readonly By Cards = By.CssSelector("article.card");
    private static readonly By SearchBox = By.CssSelector("input[type='search']");
    private static readonly By FirstTitle = By.CssSelector("article.card h3 a");

    public CatalogPage(IWebDriver driver)
    {
        _driver = driver;
    }

    public CatalogPage Visit()
    {
        _driver.Navigate().GoToUrl("https://catalog.example.com/");
        // The page object owns its own readiness condition - callers never wait.
        new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
            .Until(d => d.FindElements(Cards).Count > 0);
        return this;
    }

    public CatalogPage Search(string text)
    {
        _driver.FindElement(SearchBox).SendKeys(text);
        return this;
    }

    public int CardCount => _driver.FindElements(Cards).Count;
    public string FirstTitle_Text => _driver.FindElement(FirstTitle).Text;
}
```

The conventions, each carrying weight:
- **Field type `IWebDriver`, the interface** — a page object should not care which browser drives
  it; browser configuration belongs to the test plumbing. The same dependency-inversion instinct
  as coding services against interfaces.
- **Selectors are named `static readonly By` fields** — the name documents intent
  (`SearchBox`, not a string pasted mid-call), and the single location is the maintenance win: a
  redesign edits these lines, not specs.
- **Actions return a page object** — `Visit().Search("clean")` chains like a sentence. Crucially,
  an action that NAVIGATES returns the page it lands on:

  ```csharp
  public CatalogPage SignInAs(string user, string password)
  {
      // ...fill the form, click submit, wait out the transition...
      return new CatalogPage(_driver);   // a successful sign-in LEAVES this page
  }
  ```

  Page transitions become the type system's problem: a spec literally cannot ask the login page
  for the catalog's card count. The design question "where does a FAILED sign-in go?" has a
  design answer — nowhere, so a fuller model adds `SignInExpectingError()` returning the login
  page with an `ErrorMessage` read.
- **The page object owns its waits.** Readiness is a property of the page ("the catalog is ready
  when cards exist"), so the knowledge lives with the page — and specs go blind to waiting
  entirely. Specs blind to URLs, selectors, AND waits read as pure user intent, which is what a
  reviewer (or interviewer) can audit at a glance.
- **Reads are properties or getters** — the spec's assert vocabulary (`CardCount`,
  `FirstTitle_Text`), returning plain values so assertion libraries do the judging.

### PageFactory: recognize it, and the .NET truth
Java Selenium code is full of this shape:

```java
public class CatalogPage {
    @FindBy(css = "input[type='search']")
    private WebElement searchBox;

    public CatalogPage(WebDriver driver) {
        PageFactory.initElements(driver, this);   // fills the annotated fields
    }
}
```
`PageFactory` populates annotated element fields via proxies — you will meet it in most Java
codebases and in half the Selenium content on the internet. The accurate .NET answer, worth
stating precisely: **the official .NET binding removed PageFactory support**; it survived in a
community package (`DotNetSeleniumExtras`, largely unmaintained), and the Selenium project's own
guidance for C# is plain page objects — constructor takes the driver, `By` fields, methods.
Plain page objects are not the fallback for missing PageFactory; they are the recommended shape.
(They also dodge PageFactory's classic pain: proxied fields resolving at surprising times and
going stale invisibly.)

### Shared plumbing: the base class
Driver creation, configuration, and cleanup belong in one place:

```csharp
public abstract class E2ETestBase : IDisposable
{
    protected IWebDriver Driver { get; }

    protected E2ETestBase()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1280,900");
        Driver = new ChromeDriver(options);
        // Wait strategy is decided HERE, once, for every inheriting class -
        // e.g. explicit-only: no implicit wait line, page objects wait for themselves.
    }

    public void Dispose()
    {
        Driver.Quit();   // browser AND driver process - unconditional
    }
}
```
This is the **template method** pattern in miniature: the base owns the lifecycle skeleton
(construct-configure / test / cleanup), subclasses fill in the tests. Under test frameworks that
construct a fresh instance per test, every test gets a fresh browser and a guaranteed `Quit` —
isolation by construction. Two honest limits to name before copying this anywhere:
- **The non-virtual `Dispose` gap.** A plain `public void Dispose()` gives subclasses no clean
  extension point for extra teardown; the full pattern is `protected virtual void Dispose(bool)`.
  Fine for a small suite; know the gap exists.
- **Ctor-failure leaks.** If a constructor throws after the driver is created (a navigation
  against a downed app is the classic), the framework never disposes the half-built instance —
  the browser and driver processes orphan. Keep post-creation ctor work minimal, or wrap it so
  failure quits the driver before rethrowing.
- The base class is also the natural home for a capture-on-failure screenshot hook and any other
  cross-cutting test concern — one address for all shared behavior.

### The pattern map: naming what a good suite already uses
- **Page Object = a facade**: a domain-specific simplified interface over the raw driver API.
  Specs talk to `CatalogPage.Search`, not to locators and waits.
- **Factory**: `By.CssSelector(...)` IS a factory method — so is PageFactory, hence the name.
- **Builder**: the `Actions` gesture chain (queue verbs, fire with `Perform()`), and test-host
  configuration builders in integration testing frameworks — same shape, different layer.
- **Template method**: the test base class owning setup/teardown around subclass-supplied tests.
- **Singleton — as the anti-pattern**: ONE shared driver across all tests maximizes speed and
  state leakage simultaneously; cookies, localStorage, and window state bleed between tests, and
  parallel execution becomes impossible. Fresh-driver-per-test is the defensible default;
  session/driver REUSE is an optimization taken deliberately, with state-reset discipline, never
  an accident of a static field.
- The point of naming these is not ceremony — it is that "we use a facade over the driver, a
  template-method base for lifecycle, and we deliberately rejected the shared-driver singleton"
  is a defensible engineering narrative, and defensible narratives are what design interviews
  actually test.

### Duplication routing: the whole refactoring vocabulary
Three kinds of duplication, three named homes — the rule that keeps a growing suite maintainable:
- **Repeated flows** (sign in, seed data) -> shared helpers/commands, or an API-level shortcut
  when the UI walk is not the thing under test.
- **Repeated data** (test users, canned inventory) -> fixture files or seed scripts, versioned
  beside the tests.
- **Repeated selectors** -> page objects.
Anything appearing twice should be able to say which home it is heading to; duplication with no
routing plan is the maintenance bill compounding.

## Say It in an Interview
- *"My page objects follow four conventions: selectors as named By fields in one place; actions
  return page objects so specs chain like sentences — and a navigating action returns the page it
  LANDS on, which puts page transitions into the type system; the page object owns its own
  readiness wait; and reads are plain properties for the assert library. The result is specs
  blind to URLs, selectors, and waits — pure user intent."*
- *"Do I use PageFactory? In Java codebases I read it fluently — @FindBy plus initElements — but
  the official .NET binding removed PageFactory, and the Selenium project's guidance for C# is
  plain page objects. So in .NET I build exactly that, not as a workaround but as the recommended
  shape."*
- *"Driver lifecycle lives in a template-method base class: options, creation, and an
  unconditional Quit in Dispose, with the suite's single wait strategy decided there. I keep
  ctor work after driver creation minimal because a throwing constructor orphans the browser —
  the framework never disposes an instance that failed to construct."*
- *"A test suite is quietly a design-patterns showcase: page objects are a facade, By is a
  factory, gesture chains are builders, the test base is template method — and the shared-driver
  singleton is the anti-pattern: fastest way to leak login state between tests and lose parallel
  execution. Fresh driver per test unless we deliberately optimize otherwise."*
- *"My duplication rule: flows go to shared helpers, data goes to fixtures, selectors go to page
  objects. If something appears twice and can't say which home it's heading to, that's the
  maintenance bill starting to compound."*

## Check Yourself
1. Name the four page-object conventions and, for each, the failure it prevents.
2. Why does `SignInAs` return a `CatalogPage`, and what does the fuller design return for a
   FAILED sign-in?
3. Where do waits live in a suite with page objects, and who is never allowed to wait?
4. An interviewer asks "do you use PageFactory?" Give the .NET-accurate three-sentence answer.
5. Your teammate proposes one static shared driver "because startup is slow". Name the two
   concrete costs, and the legitimate version of the optimization.
6. A subclass of the test base needs to delete temp files after each test. What limitation of the
   simple base class does this expose, and what is the full pattern's name?
7. Match each to its pattern: `By.XPath(...)`; the gesture chain ending in `Perform()`; the
   page object itself; the test base class.

**Answers:** (1) Named `By` fields in one place (a redesign edits one file, not every spec);
actions return page objects (specs chain as intent; navigation returns the landing page, so
wrong-page calls become compile errors); the page object owns its readiness wait (specs stop
encoding timing knowledge they don't have); reads as plain properties (assert libraries judge,
pages don't). (2) A successful sign-in LEAVES the login page — the return type encodes the
transition; a failed sign-in stays, so the fuller design adds `SignInExpectingError()` returning
the login page exposing an `ErrorMessage` read. (3) Inside page objects (readiness and
transition waits) and nowhere else; SPECS never wait — a wait in a spec is page knowledge that
leaked. (4) "I recognize it fluently in Java — @FindBy fields filled by initElements. The
official .NET binding removed PageFactory; it only survived in an unmaintained community
package. The Selenium project's own C# guidance is plain page objects — ctor takes the driver,
By fields, methods — so that is what I build, as the recommended shape rather than a fallback."
(5) State leakage (auth tokens, cookies, localStorage bleed between tests — order-dependent
flake) and lost parallelism (one browser is a global lock); the legitimate version is deliberate
session reuse WITH explicit state reset between tests, taken as a measured optimization. (6) The
non-virtual `public void Dispose()` gives no extension point — a subclass can only shadow it;
the full shape is the dispose pattern, `protected virtual void Dispose(bool disposing)` with the
public method delegating. (7) Factory; builder; facade; template method.

## Summary
- POM: one class per page; selectors as named `By` fields (one edit per redesign); actions return
  page objects — navigating actions return the LANDING page (transitions in the types); page
  objects own their readiness waits; reads are plain properties. Specs end up blind to URLs,
  selectors, and waits.
- PageFactory: fluent recognition for Java (`@FindBy` + `initElements`); in .NET it was removed
  from the official binding and plain page objects are the recommended shape — not a fallback.
- Shared plumbing: a template-method base class owns driver options, creation, the suite's single
  wait strategy, and unconditional `Quit`; mind the non-virtual-Dispose gap and ctor-failure
  process leaks; it is the home for capture-on-failure hooks.
- Pattern map: facade (page object), factory (`By`), builder (gesture chains), template method
  (test base), singleton-as-anti-pattern (shared driver = state leaks + no parallelism).
- Duplication routing: flows -> helpers, data -> fixtures, selectors -> page objects.

## Resources
- [Page Object Models (selenium.dev)](https://www.selenium.dev/documentation/test_practices/encouraged/page_object_models/)
- [Design patterns and development strategies (selenium.dev)](https://www.selenium.dev/documentation/test_practices/design_strategies/)
