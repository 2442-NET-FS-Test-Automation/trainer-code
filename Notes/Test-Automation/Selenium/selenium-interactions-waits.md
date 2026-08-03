# Selenium Interactions and Waits: Driving Elements, Waiting on Purpose

## Learning Objectives
- Simulate user actions with the element verbs — `Click`, `SendKeys`, `Clear`, `Submit` — and read
  element state with `Text` and `GetAttribute`, including the input-value trap.
- Drive `<select>` elements with the `SelectElement` wrapper: by text, by value, by index; single
  and multiple; and know which operations are multi-select-only.
- Build compound gestures with the `Actions` API — hover, double-click, context-click,
  click-and-hold/drag, keyboard chords — and remember the call everyone forgets.
- Implement all three waits: implicit (and its real costs), explicit `WebDriverWait` with lambda
  conditions (the modern .NET idiom), and fluent (the same class with the polling and
  exception-ignoring dials turned).
- State the no-mixing rule with its concrete failure mode, and choose one wait strategy per
  driver, deliberately.

## Why This Matters
Finding an element is half the job; the other half is operating it the way a user does and
surviving the fact that web UIs render asynchronously. The waits topic in particular is where
suites earn or lose their reputation: almost every "flaky" UI test is a timing assumption wearing
an assertion. Interviewers probe it hard — "explain the three waits" and "why is `Thread.Sleep`
wrong" are near-universal questions — and the strong answers come from having felt the failure
modes, which this note names explicitly.

## The Concept

### The element verbs, and the two reads
```csharp
var search = driver.FindElement(By.CssSelector("input[type='search']"));

search.SendKeys("clean");                       // types with real key events
search.GetAttribute("value");                   // -> "clean"  (what the input HOLDS)
search.Clear();                                 // empties the field, fires change events

driver.FindElement(By.CssSelector("button[type='submit']")).Click();
```
- **`SendKeys` types like a user**: keystroke events fire one by one, so framework change
  handlers (React's `onChange`, Angular's bindings) run exactly as in real use. This is why
  Selenium works against any frontend framework — it never knows the framework exists.
- **The reading trap:** `Text` is the rendered text a user READS (trimmed, visibility-aware);
  `GetAttribute("value")` is what an input HOLDS. Inputs have empty `Text`. Reading the wrong one
  is a top-five beginner bug — recognize it instantly when an assert on an input's `Text` comes
  back empty.
- **Assert the consequence, not the survival.** After typing into a filter, assert the filtered
  RESULT (one card remains), not merely that typing didn't throw. Interaction tests that only
  survive their actions test nothing.
- `Submit` submits the form an element belongs to; clicking the real submit button is usually
  truer to the user journey and exercises the button's own handlers.

### The Select class: speaking dropdown
```csharp
using OpenQA.Selenium.Support.UI;

var format = new SelectElement(driver.FindElement(By.Id("format")));

format.SelectByText("Paperback");    // matches what the user READS
format.SelectByValue("soft");        // matches what the form POSTS
format.SelectByIndex(0);             // matches luck
format.SelectedOption;               // the current choice
format.IsMultiple;                   // which world are we in?
```
`SelectElement` (from the support package) wraps an element you already found; hand it a div
pretending to be a dropdown and it throws `UnexpectedTagNameException` — a useful early warning
that the "select" is a custom widget needing ordinary click choreography instead. The three
selection modes rank by the same stability logic as locators: text is the user contract, value is
the form contract, index is position. On a single select, a new selection REPLACES; on
`IsMultiple`, selections ACCUMULATE, and only there do `DeselectByText`/`DeselectAll` exist —
calling them on a single select throws.

Adjacent question: *most modern dropdowns aren't `<select>` at all.* Correct — styled listbox
divs are driven with ordinary clicks and keyboard events; `SelectElement` is for the native
element, which still rules forms in enterprise apps.

### The Actions API: gestures as sentences
```csharp
using OpenQA.Selenium.Interactions;

new Actions(driver)
    .MoveToElement(menu)          // hover - the mouse POSITION is the event
    .Perform();

new Actions(driver)
    .Click(input)
    .KeyDown(Keys.Shift)
    .SendKeys("ada")              // arrives as "ADA"
    .KeyUp(Keys.Shift)
    .Perform();
```
`Element.Click()` is a single verb; `Actions` is a builder for compound gestures: `MoveToElement`
(hover — how every hover-revealed menu in every enterprise app is tested), `DoubleClick`,
`ContextClick` (right-click), `ClickAndHold`/`Release` and `DragAndDrop`, and keyboard chords via
`KeyDown`/`KeyUp` around `SendKeys`. `Keys.` carries the special-key vocabulary (`Keys.Enter`,
`Keys.Tab`, `Keys.Control`).

**The call everyone forgets: `Perform()`.** The chain queues; nothing happens until `Perform()`.
Forgetting it produces no error — just a gesture that never fired and asserts failing downstream
against stale state. When a gesture "did nothing", check the chain's last link first.

### Waits: the arc from implicit to fluent
Web UIs render asynchronously: the DOM at look-time is whatever it is, and a test that looks too
early sees a half-rendered page. Waiting is not an annoyance to suppress — it is a statement
about WHAT the test depends on. Three named forms:

**Implicit** — one global setting on the driver:
```csharp
driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
```
Every `FindElement`/`FindElements` now retries up to the timeout. Blunt: it applies to lookups
only (an element can EXIST but not yet be clickable), it taxes every negative lookup with the
full timeout (absence checks get slow), and it hides what each test actually waits for.

**Explicit** — `WebDriverWait`, one condition on one call:
```csharp
using OpenQA.Selenium.Support.UI;

var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
var cards = wait.Until(d => {
    var found = d.FindElements(By.CssSelector("article.card"));
    return found.Count > 0 ? found : null;   // null/false => keep polling
});
```
`Until` takes a lambda over the driver; return something truthy and that value comes out, return
`null`/`false` and it polls again (default every 500 ms) until `WebDriverTimeoutException`. The
lambda IS the modern .NET idiom: the old `ExpectedConditions` catalog was dropped from the .NET
binding (Java still has it; the community `SeleniumExtras.WaitHelpers` package is its legacy
echo — recognize it in older codebases, write lambdas in new ones). An explicit wait documents
what the test depends on: "this test proceeds when the cards exist" is in the code.

**Fluent** — the same machine with the dials turned:
```csharp
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
{
    PollingInterval = TimeSpan.FromMilliseconds(250),
};
wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

var el = wait.Until(d => d.FindElement(By.CssSelector("article.card h3 a")));
```
In Java, `FluentWait` is a separate class; in .NET, `WebDriverWait` IS a FluentWait subclass —
"fluent" means you set `PollingInterval` (how often to re-ask) and `IgnoreExceptionTypes` (which
exceptions count as "not yet" instead of "failed"). With `NoSuchElementException` ignored, you
can wait on `FindElement` directly — every miss becomes a retry. Reach for the dials for
elements that go stale mid-render (ignore `StaleElementReferenceException`, poll fast) and for
slow backends where 500 ms polling hammers a struggling server. Default first; dials when the
default demonstrably fights you.

**The no-mixing rule.** Never combine an implicit wait and explicit waits on the same driver: the
timeouts COMPOUND unpredictably (the official documentation's own warning) — a 2-second implicit
inside a 10-second explicit's polling loop can stretch a failure to well past either number,
producing tests that take strangely long to fail and flake under load. One strategy per driver,
chosen deliberately; in a suite with shared plumbing, the shared constructor is where that choice
lives, and the explicit strategy is the one that scales — it fails fast on misses and documents
its dependencies.

**And `Thread.Sleep` is not a wait — it is a guess wearing a number.** Too short on a loaded
machine (flake), pure waste on every green run (drag). A fixed sleep in a UI test should require
a written justification; the replacement is always a condition: what, specifically, are you
waiting FOR?

## Say It in an Interview
- *"`Text` is what the user reads; `GetAttribute(\"value\")` is what an input holds — inputs have
  empty Text, and knowing that has saved me from a very confusing green-field debugging session
  more than once."*
- *"For dropdowns I wrap the element in `SelectElement` and select by TEXT first — it is the user
  contract; by value when the form contract matters; by index basically never. `IsMultiple` tells
  me whether selections accumulate, and the deselect APIs only exist there."*
- *"The Actions builder queues gestures — hover, chords, drag — and fires on `Perform()`.
  Forgetting `Perform()` is the classic silent bug: no error, no gesture, failing asserts
  downstream."*
- *"Three waits: implicit is one global lookup-retry — blunt, taxes every absence check, and hides
  intent; explicit is `WebDriverWait` with a condition on one call — in modern .NET that is a
  lambda, since `ExpectedConditions` left the binding; fluent is the same `WebDriverWait` with
  `PollingInterval` and `IgnoreExceptionTypes` tuned. I default to explicit, everywhere, because
  it documents what each test depends on."*
- *"The no-mixing rule: implicit and explicit on one driver compound unpredictably — the docs say
  so outright — so a suite picks ONE strategy at the shared-plumbing level. And `Thread.Sleep` is
  a guess wearing a number: too short under load, wasted time when green. My review comment is
  always the same — what are you waiting FOR? Wait on that."*

## Check Yourself
1. An assert on a text input reads `element.Text` and gets `""` although the user typed a value.
   What is wrong and what is the fix?
2. Name the three `SelectByX` modes in stability order and say which exception tells you the
   element was never a real `<select>`.
3. A hover-menu test finds the trigger, chains `MoveToElement`, and the menu never appears — no
   exception anywhere. Name the most likely one-word cause.
4. Write (out loud) the explicit wait for "the error paragraph appears after a failed login",
   and say what the lambda must return to keep the wait polling.
5. What concretely goes wrong when a 2-second implicit wait coexists with 10-second explicit
   waits on one driver?
6. When would you actually change `PollingInterval` or `IgnoreExceptionTypes`? One real scenario
   each.
7. A teammate defends `Thread.Sleep(3000)` with "it only fails without it". Give the review
   response: why it is still wrong, and the replacement shape.

**Answers:** (1) `Text` reads rendered text, which is empty for inputs; read
`GetAttribute("value")`. (2) ByText (user contract) > ByValue (form contract) > ByIndex
(position/luck); `UnexpectedTagNameException` means the element is not a native `<select>` — it
is a custom widget needing ordinary clicks. (3) `Perform()` — the chain was built but never
fired. (4) `new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(d => { var found =
d.FindElements(By.CssSelector("p.error")); return found.Count > 0 ? found[0] : null; })` — the
lambda must return `null` (or `false`) to keep polling, anything truthy to finish. (5) The
implicit wait runs inside every lookup the explicit wait's condition performs, so each poll
blocks up to 2 s: failures take far longer than either configured timeout, the effective timeout
is unpredictable, and the suite reads as mysteriously slow-to-fail — the documented
compounding. (6) Faster polling + ignoring `StaleElementReferenceException` for elements
re-rendered mid-wait (find-retry becomes the loop); slower polling for a struggling backend that
500 ms hammering makes worse. (7) It works until machine load changes — too short = flake, and
on every green run it burns three full seconds; replace with an explicit wait on the actual
condition the sleep was papering over ("what are you waiting for? wait on that"), which is both
faster when things are fast and more patient when things are slow.

## Summary
- Verbs: `Click`, `SendKeys` (real key events — framework-agnostic), `Clear`, `Submit`; reads:
  `Text` for rendered text, `GetAttribute("value")` for inputs — never confuse them; assert
  consequences, not survival.
- `SelectElement` wraps native selects: ByText/ByValue/ByIndex in that stability order,
  `IsMultiple` gates the deselect APIs, `UnexpectedTagNameException` exposes fake dropdowns.
- `Actions` builds compound gestures (hover, double/context-click, drag, keyboard chords) and
  fires on `Perform()` — the forgotten call is the classic silent bug.
- Implicit = global lookup retry (taxes absence checks, hides intent); explicit =
  `WebDriverWait.Until(lambda)` — the modern .NET idiom, documents the dependency; fluent = same
  class, `PollingInterval` + `IgnoreExceptionTypes` dials.
- One wait strategy per driver (the no-mixing rule — compounding timeouts are the failure mode);
  prefer explicit; `Thread.Sleep` is a guess, and the review answer is always a condition.

## Resources
- [Waiting strategies (selenium.dev)](https://www.selenium.dev/documentation/webdriver/waits/)
- [Actions API (selenium.dev)](https://www.selenium.dev/documentation/webdriver/actions_api/)
- [Support features - Select lists (selenium.dev)](https://www.selenium.dev/documentation/webdriver/support_features/select_lists/)
