# Selenium Locators and Navigation: Finding Anything, Reliably

## Learning Objectives
- Choose among the `By` factory's locator strategies — Id, Name, TagName, ClassName, CssSelector,
  LinkText, PartialLinkText, XPath — and defend the choice with a stability argument.
- State and apply the locator preference order: id > name > user-visible text > CSS structure >
  XPath structure.
- Use the find-methods contract deliberately: `FindElement` (first match or throw) vs
  `FindElements` (all matches or empty list), including how to assert an element's absence.
- Control browser flow with the full navigation surface — `GoToUrl`, `Back`, `Forward`, `Refresh` —
  and assert location with the `Url` and `Title` properties, knowing how single-page apps change
  what `Title` can tell you.
- Capture screenshots at both granularities — the driver's viewport and a single element — and say
  where screenshot capture belongs in a real suite (on failure, automatically).

## Why This Matters
Every flaky UI-test autopsy starts the same way: a locator that matched yesterday and not today.
Locator choice is where end-to-end suites live or die, because the selector is the coupling point
between test code and an application that keeps changing. It is also among the most-asked
automation interview topics — "how do you find elements" sounds basic, but the follow-ups (what
happens on no match? why not absolute paths? when XPath over CSS?) separate people who have
maintained a suite from people who have read a tutorial.

Navigation is the quieter half: real user journeys involve the back button, refreshes mid-flow,
and deep links pasted into a new tab. A suite that only ever calls `GoToUrl` once per test is not
exercising the history behavior users depend on.

## The Concept

### The By factory: one language, many strategies
Every lookup goes through a `By` — a factory object describing HOW to match:

```csharp
driver.FindElement(By.Id("checkout-total"));
driver.FindElement(By.Name("email"));
driver.FindElement(By.TagName("h1"));
driver.FindElement(By.ClassName("card"));
driver.FindElement(By.CssSelector("article.card h3 a"));
driver.FindElement(By.LinkText("About"));
driver.FindElement(By.PartialLinkText("Abo"));
driver.FindElement(By.XPath("//article[@class='card']"));
```

- **`By.Id`** — matches the `id` attribute. Fastest, least ambiguous, first choice *when the app
  provides ids*. Many apps do not; apps you own should (a dedicated `data-testid` attribute is the
  gift a team leaves its own testers).
- **`By.Name`** — the form-post `name` attribute. Same virtues as Id where forms are classic HTML.
- **`By.TagName`** — the element name (`h1`, `select`). Only safe for one-of-a-kind elements; it
  silently returns the FIRST match the day someone adds a second.
- **`By.ClassName`** — a single class token. `By.ClassName("card")` works;
  `By.ClassName("article.card")` throws `InvalidSelectorException` — the moment you want to
  combine, you have outgrown the strategy.
- **`By.CssSelector`** — the full CSS language: descendants, attribute filters, pseudo-classes.
  The workhorse; the same selector text works in the browser DevTools console (`$$("...")`), which
  is where you should test a selector before it goes in a test.
- **`By.LinkText` / `By.PartialLinkText`** — anchor (`<a>`) elements only, matched by exact (or
  substring) visible text. The most user-shaped locators: stable across restyling, fragile across
  copy edits. They never match a `<button>`, however link-like it looks.
- **`By.XPath`** — a second selector language with capabilities CSS lacks (text matching, upward
  traversal). Deep enough to deserve its own study; see a dedicated XPath reference for the
  grammar, functions, axes, and the absolute-path trap.

### The preference order, and why
**id > name > user-visible text > CSS structure > XPath structure.** Reach rightward only when the
left options are unavailable. The ranking is a stability argument: ids and names are contracts the
application author wrote down; user-visible text is a contract with the USER (it breaks only when
the product deliberately rewords something); CSS structure encodes today's DOM shape, which
refactors freely; and structural XPath encodes it even more brittly. A selector that encodes
layout ("the third div inside the second section") breaks on every redesign — a selector that
encodes meaning ("the element labeled Checkout") breaks only when the meaning changes.

Adjacent question an interviewer will ask: *what do you do on an app with no ids and no test
attributes?* Answer: exactly this ranking from the third rung down — text and structure — plus a
conversation with the team about adding `data-testid` attributes in their own reviewed commits,
never smuggled in alongside test code.

### FindElement vs FindElements: the contract
Two methods, two different promises:

```csharp
// First match, or NoSuchElementException after any configured wait:
IWebElement one = driver.FindElement(By.CssSelector(".card"));

// ALL matches, or an EMPTY list - never a throw:
var all = driver.FindElements(By.CssSelector(".card"));
```

Consequences worth internalizing:
- **Asserting absence** is `FindElements(...).Count == 0` (or `.Should().BeEmpty()`), never a
  try/catch around `FindElement`. Exception-driven control flow in tests is slow and unreadable.
- **`FindElement` returns the FIRST match** in document order when several exist — a silent wrong
  answer if your locator is broader than you think. If you expect exactly one, asserting
  `FindElements(...).Count == 1` first makes the expectation explicit.
- With an implicit wait configured, BOTH methods keep retrying until the timeout — including a
  `FindElements` that will end up empty, which means every absence check pays the full wait. That
  hidden tax on negative lookups is one of the standard arguments against implicit waits.

### Navigation: the full surface
```csharp
driver.Navigate().GoToUrl("https://catalog.example.com/inventory/BK-001");
driver.Navigate().Back();
driver.Navigate().Forward();
driver.Navigate().Refresh();

string where = driver.Url;    // the address bar, right now
string title = driver.Title;  // the document title
```

- **`GoToUrl`** performs a full navigation, like typing in the address bar. Deep-linking straight
  into an inner route is itself a test: it proves bookmarks and shared links work cold.
- **`Back` / `Forward`** drive the browser's real history stack. In a single-page application the
  router listens to history events, so these calls exercise the client-side routing users hit with
  the back button — a classic source of SPA bugs.
- **`Refresh`** reloads the document. After client-side navigation, a refresh is a genuinely
  different code path (server round-trip, router re-reading the URL on boot); asserting the same
  view survives both is a cheap, high-value check.
- **`Url`** is the assertion target for "where am I". Prefer `Contains`/`EndsWith` over exact
  equality — schemes, ports, and trailing slashes vary between environments.
- **`Title`** asserts the document title — useful in multi-page apps where every page sets its
  own. Know your app: many SPAs never touch `document.title`, so the title is identical on every
  route and asserts nothing about location. Recognize-on-sight: a suite full of title asserts
  against an SPA is asserting one constant string over and over.

### Screenshots: viewport and element
```csharp
// Whole viewport:
driver.GetScreenshot().SaveAsFile(path);       // PNG

// One element - crop to a single widget:
var card = driver.FindElement(By.CssSelector("article.card"));
((ITakesScreenshot)card).GetScreenshot().SaveAsFile(path);
```

Practicalities that separate working captures from black rectangles:
- **Gate on content first.** A screenshot of a page that has not rendered is a picture of nothing;
  wait for a meaningful element before capturing.
- **Save into the test run's own output folder** (relative paths resolve against the test host's
  working directory, typically the build output). Screenshots are run artifacts: gitignored,
  attached to bug tickets or CI results, never committed.
- **The real-world home is capture-on-failure**: a hook in shared test plumbing that screenshots
  automatically when a test fails, so every red CI run comes with a picture. Manually sprinkled
  captures are for debugging sessions; the automatic hook is what a team actually relies on.
- Element-level capture is the bug-ticket attachment: just the failing widget, no noise.

Adjacent question: *screenshot testing vs visual-regression testing?* A screenshot is evidence for
a human; visual regression DIFFS screenshots against a committed baseline and fails a build on
pixel drift. Same artifact, different judge.

## Say It in an Interview
- *"My locator preference order is id, then name, then user-visible text, then CSS structure, and
  XPath structure last — it is a stability ranking: ids and text are contracts, structure is
  whatever the DOM happens to look like today. On apps we own I push for dedicated test
  attributes; on apps we don't, I anchor on what the user can see."*
- *"`FindElement` returns the first match or throws; `FindElements` returns everything or an empty
  list and never throws — so absence is always asserted with `FindElements`, never a try/catch.
  And I stay conscious that `FindElement` on an over-broad locator silently gives you the first of
  many."*
- *"I test selectors in DevTools with `$$()` before they ever go into code — same CSS engine, ten
  times the feedback speed."*
- *"Back, Forward, and Refresh are real test moves, not plumbing — in an SPA they exercise the
  router's history handling and the reload path, which is exactly where client-side routing bugs
  live. And I assert URLs with Contains rather than equality so environment differences don't
  break the suite."*
- *"Screenshots earn their keep on failure, automatically — a capture hook in the shared test base
  writes a PNG whenever a test goes red, so CI failures arrive with pictures. I keep them out of
  version control; they're artifacts, not code."*

## Check Yourself
1. Recite the locator preference order and give the one-sentence reason ids beat CSS structure.
2. `By.ClassName("article.card")` — what happens, and what should the call have been?
3. Your test must prove that a "Delete" button is NOT on the page for a read-only user. Write the
   shape of the assertion, and say why the try/catch version is wrong.
4. A `FindElement` call keeps passing but the test asserts against the wrong card. What property
   of `FindElement` is biting, and what two fixes exist?
5. Why can `driver.Title` be a useless assertion in a single-page application, and what do you
   assert instead?
6. Where do screenshot files belong, and what is the difference between a screenshot in a bug
   ticket and a visual-regression baseline?

**Answers:** (1) id > name > user-visible text > CSS structure > XPath structure; ids are a
contract the app author wrote, CSS structure is today's DOM shape and refactors freely. (2)
`InvalidSelectorException` — ClassName takes a single token; compound selectors belong to
`By.CssSelector("article.card")`. (3) `driver.FindElements(By.CssSelector("button.delete"))` and
assert the collection is empty; `FindElements` never throws, while catching
`NoSuchElementException` around `FindElement` is exception-driven control flow that also pays the
full implicit wait and reads as error handling rather than assertion. (4) `FindElement` returns
the FIRST match in document order; either narrow the locator until it is unique, or assert
`FindElements(...).Count == 1` first so over-breadth fails loudly. (5) Many SPAs never update
`document.title`, so it is one constant string on every route; assert `driver.Url` (Contains) plus
a content element unique to the view. (6) In the test run's output folder, gitignored — captured
automatically on failure by shared plumbing; a bug-ticket screenshot is evidence for a human,
a visual-regression baseline is a committed expected value that a pixel-diff judges builds
against.

## Summary
- All lookups go through the `By` factory; know all eight strategies and choose by the stability
  ranking id > name > user-visible text > CSS structure > XPath structure.
- `By.ClassName` is one token; `By.CssSelector` is the workhorse; LinkText variants match anchors
  only, by what the user reads; test selectors in DevTools before committing them.
- `FindElement` = first match or throw; `FindElements` = all matches or empty, never throws;
  absence is asserted with `FindElements`, and implicit waits tax every negative lookup.
- Navigation is `GoToUrl`/`Back`/`Forward`/`Refresh` plus the `Url`/`Title` reads; history moves
  and refreshes are real SPA test cases; title asserts require an app that sets titles.
- Screenshots: driver-level viewport or element-level crop; gate on rendered content; artifacts
  stay out of git; the production pattern is capture-on-failure in shared plumbing.

## Resources
- [Locator strategies (selenium.dev)](https://www.selenium.dev/documentation/webdriver/elements/locators/)
- [Finding web elements (selenium.dev)](https://www.selenium.dev/documentation/webdriver/elements/finders/)
- [Browser navigation (selenium.dev)](https://www.selenium.dev/documentation/webdriver/interactions/navigation/)
