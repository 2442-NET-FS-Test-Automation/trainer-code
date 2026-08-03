# Selenium Windows, Alerts, and Exceptions: The Sharp Edges, Catalogued

## Learning Objectives
- Model browser tabs/windows as handles: one driver conversation at a time, `CurrentWindowHandle`
  and `WindowHandles`, moving with `SwitchTo().Window()`, opening with `SwitchTo().NewWindow()`,
  and the `Close()`-vs-`Quit()` distinction.
- Handle the three JavaScript dialogs — `alert`, `confirm`, `prompt` — through
  `SwitchTo().Alert()`, and avoid the unhandled-dialog deadlock.
- Read the Selenium exception bestiary as a debugging discipline: what each exception ACCUSES
  (locator, timing, page lifecycle, visibility, condition, environment).
- Validate element state with the five reads — `Displayed`, `Enabled`, `Selected`, `Text`,
  `GetAttribute` — and keep exists/visible/usable/selected as four distinct questions.

## Why This Matters
Windows and dialogs are where the driver's mental model earns its keep: both live partly OUTSIDE
the DOM your locators reach, and both deadlock tests that pretend otherwise. Exceptions are the
day-to-day reality of maintaining a UI suite — a working automation engineer spends far more time
reading Selenium failures than writing Selenium successes, and the difference between an hour of
guessing and a two-minute fix is knowing which exception accuses what. Interviews mine this
territory too: "how do you handle multiple windows", "what causes a stale element", and "what's
the most common Selenium error you've debugged" are all standards.

## The Concept

### Windows are handles; the driver talks to one at a time
Every tab or window is an opaque string HANDLE. The driver holds a conversation with exactly one;
every command goes to the current one until you switch.

```csharp
var original = driver.CurrentWindowHandle;      // where am I
var all = driver.WindowHandles;                 // what exists (ReadOnlyCollection<string>)

driver.SwitchTo().NewWindow(WindowType.Tab);    // Selenium 4: open a tab from code
driver.Navigate().GoToUrl("https://catalog.example.com/about");

driver.Close();                                 // closes the CURRENT tab only...
driver.SwitchTo().Window(original);             // ...so you MUST move afterward
```

- **`SwitchTo().NewWindow`** (Selenium 4) opens a tab or window from code — before it, tests
  needed the app to open windows for them. "Compare two pages side by side" is now a test you
  can write with zero app cooperation.
- **`Close()` vs `Quit()`** — the classic. `Close` kills the CURRENT tab and leaves the driver
  pointing at nothing; every subsequent command throws `NoSuchWindowException` until you
  `SwitchTo().Window(...)`. `Quit` ends the whole session: every window plus the driver process.
  Mixing them up either strands the test or ends it.
- **New handles appear asynchronously.** After clicking a `target="_blank"` link, poll
  `WindowHandles.Count` with an explicit wait — element waits do not apply, because a tab is not
  an element. Finding the new handle is a set difference: the handle that is not the original.
- Window geometry lives on `Manage().Window`: `Size`, `Position`, `Maximize()`, `Minimize()`,
  `FullScreen()`. Pinning a size at launch (an options argument) keeps layout-dependent tests
  deterministic; the runtime property is the adjustable dial.

### Alerts: dialogs live outside the DOM
`alert()`, `confirm()`, `prompt()` freeze the page behind them, and no locator reaches them — they
are not elements. The only door is `SwitchTo().Alert()`:

```csharp
driver.FindElement(By.Id("delete-btn")).Click();   // raises confirm("Delete this book?")

IAlert dialog = driver.SwitchTo().Alert();          // throws NoAlertPresentException if none
string text = dialog.Text;                          // read the message
dialog.Accept();                                    // OK       (confirm -> true)
// dialog.Dismiss();                                // Cancel   (confirm -> false)
// dialog.SendKeys("Ada"); dialog.Accept();         // prompt: type, then OK
```

- **Test both branches of a confirm** — Accept and Dismiss are different application paths, the
  positive and negative case of a two-button feature.
- **`UnhandledAlertException`**: a dialog was up when you sent an ordinary command. The page is
  frozen; handle the dialog first. Mid-debugging, a quick `SwitchTo().Alert().Dismiss()` frees a
  stuck session.
- **`NoAlertPresentException`**: `SwitchTo().Alert()` with nothing up — which makes the call
  double as the "is a dialog showing" probe. Dialogs raised from asynchronous handlers can lag;
  a `WebDriverWait` whose lambda tries `SwitchTo().Alert()` (returning null on the exception)
  polls for one.
- Recognize the boundary: most modern apps replaced native dialogs with styled modal `<div>`s —
  those ARE elements, tested with ordinary locators. `window.confirm` still rules
  "are you sure?" flows across enterprise software; knowing which kind a page uses is the skill.

### The exception bestiary: what each one accuses
Debugging a Selenium failure is 90% recognizing which exception you have and what it accuses.

- **`NoSuchElementException` — accuses the locator, the timing, or the environment.** Debug in
  that reverse order: is the app UP (Selenium blames the element when the server is down — the
  single most misleading failure in the tool); did you wait long enough; and only THEN suspect
  the selector (test it in DevTools). Under an explicit-only strategy the throw is immediate on a
  genuine miss — fast failure is a feature.
- **`StaleElementReferenceException` — accuses the page lifecycle.** You held a reference across
  a re-render or reload; the element object points at a document that no longer exists. In
  framework-rendered UIs re-renders are constant, so the rule is **re-find, never hoard** — look
  the element up when you need it, not minutes before. The fluent-wait pattern (ignore the stale
  exception, re-find in the loop) turns intermittent staleness into retries.
- **`ElementNotInteractableException` — accuses visibility/usability.** Found is not clickable:
  the element exists but is hidden (`display:none`), zero-sized, or covered. Existence and
  interactability are different questions — see the state reads below.
- **`WebDriverTimeoutException` — accuses the condition.** An explicit wait's condition never
  came true. Its message names what was polled and for how long, making it the MOST debuggable
  failure in Selenium — an underrated argument for explicit waits that has nothing to do with
  speed.
- **`NoSuchWindowException` / `NoAlertPresentException`** — the context accusations: you are
  talking to a closed window (switch after `Close()`!) or to a dialog that is not there.
- **`SessionNotCreatedException` ("session not created: this version of ChromeDriver only
  supports...")** — accuses the environment: browser/driver version drift, the classic breakage
  of manually managed drivers after a browser auto-update. Automated driver management (Selenium
  Manager) exists to end this class of failure.
- One more that is not an exception: **orphaned browser processes.** A test that dies before
  cleanup (a constructor throwing before any `Dispose`/`Quit` can run) leaks a browser and a
  driver process. Watch for zombie driver executables after crashed runs; unconditional cleanup
  (dispose patterns, try/finally) is the vaccine.

### Element-state validation: the five reads, four questions
```csharp
var el = driver.FindElement(By.Id("save-btn"));

el.Displayed;                 // could a user SEE it?
el.Enabled;                   // could a user OPERATE it?
el.Selected;                  // is it chosen? (checkboxes, radios, options)
el.Text;                      // what does it SAY?
el.GetAttribute("value");     // what does it HOLD? (inputs)
```
Keep the questions separate, because each read answers exactly one:
- **Exists** — `FindElement` succeeded (or `FindElements` non-empty). An element can exist and be
  invisible.
- **Visible** — `Displayed`. A hidden menu exists in the DOM with `Displayed == false`; clicking
  it anyway is `ElementNotInteractableException` — the exception and the read are two sides of
  one fact.
- **Usable** — `Enabled`. A disabled input is fully visible (`Displayed == true`) and inert
  (`Enabled == false`); users see it, cannot use it.
- **Selected** — `Selected`, for the checkable family only.

These five reads, plus an assertion library, are the whole element-state validation toolkit:
"the delete button is visible but disabled for read-only users" is three reads and two asserts.

## Say It in an Interview
- *"Every tab is a handle and the driver talks to exactly one at a time — `SwitchTo().Window`
  moves the conversation, and since Selenium 4 I can open tabs from code with
  `SwitchTo().NewWindow`. The trap is `Close()`: it kills the current tab and strands the driver,
  so a switch always follows. And a new handle after a target-blank click appears asynchronously
  — I poll `WindowHandles.Count` with an explicit wait, because element waits don't cover
  windows."*
- *"Native dialogs live outside the DOM — no locator reaches them; `SwitchTo().Alert()` is the
  only door, with Accept, Dismiss, and SendKeys for prompts. Leave one unhandled and the next
  command throws UnhandledAlertException. Styled modal divs are the opposite: ordinary elements,
  ordinary locators — the first question on any dialog is which kind it is."*
- *"I debug by what the exception accuses: NoSuchElement — environment first (is the app even
  up), then timing, then the locator; StaleElementReference — I hoarded a reference across a
  re-render, so re-find, never hoard; ElementNotInteractable — exists but hidden or covered;
  Timeout — read the message, it names the condition it polled. Session-not-created is version
  drift, which is why automated driver management exists."*
- *"Element state is four separate questions — exists, visible, usable, selected — answered by
  FindElements, Displayed, Enabled, and Selected; Text and GetAttribute read content. 'Visible
  but disabled' is a real requirement and it is two different reads."*

## Check Yourself
1. After `driver.Close()`, every command throws `NoSuchWindowException`. Why, and what was the
   missing line?
2. You need the app's About page and its Catalog page open simultaneously, and the app has no
   target-blank links. What call solves it, and which Selenium version introduced it?
3. Why can't an implicit or explicit ELEMENT wait detect that a new tab has opened, and what do
   you wait on instead?
4. A test clicks Delete, a `confirm` appears, and the next `FindElement` throws
   `UnhandledAlertException`. Explain the state of the page and the two-line fix.
5. Give the "accusation" of each: `NoSuchElementException`, `StaleElementReferenceException`,
   `ElementNotInteractableException`, `WebDriverTimeoutException` — and the FIRST thing you check
   for the first of those.
6. A test intermittently throws stale-element on a list that re-renders as data streams in. Name
   the code-discipline fix and the wait-configuration fix.
7. Write (out loud) the three asserts for "read-only users see the Save button but cannot use
   it, and their 'read-only' checkbox is ticked".

**Answers:** (1) `Close` kills only the current tab and does NOT move the driver — it points at a
dead window; the missing line is `driver.SwitchTo().Window(someOtherHandle)`. (2)
`driver.SwitchTo().NewWindow(WindowType.Tab)` — Selenium 4. (3) Waits built on element lookup
poll the DOM of the CURRENT window; a tab is a browser-level object, not an element — poll
`driver.WindowHandles.Count` in a `WebDriverWait` lambda. (4) The page is frozen behind the
dialog and ordinary commands cannot proceed; fix: `var d = driver.SwitchTo().Alert();` then
`d.Accept()` (or `Dismiss()`), and only then continue — and test both branches deliberately. (5)
Locator/timing/environment — check the ENVIRONMENT first (is the application actually running;
Selenium blames the element when the server is down); page lifecycle (reference hoarded across a
re-render); visibility/usability (exists but hidden, zero-sized, or covered); the wait's
condition (read the message — it names what it polled). (6) Discipline: re-find the element at
use time instead of hoarding the reference; configuration: a fluent wait that ignores
`StaleElementReferenceException` and re-finds inside the polling loop. (7)
`saveBtn.Displayed` is true; `saveBtn.Enabled` is false; `readOnlyBox.Selected` is true.

## Summary
- Tabs/windows are handles; one conversation at a time; `SwitchTo().Window` moves it,
  `SwitchTo().NewWindow` (Selenium 4) opens from code; `Close` = current tab and ALWAYS switch
  after; `Quit` = whole session; new handles are polled with an explicit wait on
  `WindowHandles.Count`.
- Native dialogs are outside the DOM: `SwitchTo().Alert()` with `Text`/`Accept`/`Dismiss`/
  `SendKeys`; unhandled dialogs freeze everything (`UnhandledAlertException`); styled modal divs
  are ordinary elements — know which kind you face.
- Exceptions accuse: NoSuchElement -> locator/timing/environment (check the app is up FIRST);
  StaleElementReference -> hoarded reference, re-find never hoard; ElementNotInteractable ->
  exists-but-hidden; Timeout -> the condition (most debuggable message in the tool);
  session-not-created -> version drift; crashed-before-cleanup runs leak browser processes.
- Element state is five reads answering four questions: exists (find), visible (`Displayed`),
  usable (`Enabled`), selected (`Selected`) — plus `Text`/`GetAttribute` for content.

## Resources
- [Working with windows and tabs (selenium.dev)](https://www.selenium.dev/documentation/webdriver/interactions/windows/)
- [JavaScript alerts, prompts and confirmations (selenium.dev)](https://www.selenium.dev/documentation/webdriver/interactions/alerts/)
- [Understanding common errors (selenium.dev)](https://www.selenium.dev/documentation/webdriver/troubleshooting/errors/)
