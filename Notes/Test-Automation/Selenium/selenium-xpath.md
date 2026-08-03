# XPath for Test Automation: Text, Axes, and the Absolute-Path Trap

## Learning Objectives
- Read and write the core XPath grammar: `/` (child step), `//` (descendant-anywhere), `[...]`
  (predicate), `@attr` (attribute), `text()` (text node).
- Apply XPath functions — `contains()`, `starts-with()`, `text()` — to locate dynamic elements
  that CSS selectors cannot reach.
- Distinguish absolute and relative XPath expressions, write both, and explain why absolute paths
  are a maintenance trap rather than a style choice.
- Walk the tree in directions CSS cannot: `ancestor::`, `parent::`, `following-sibling::`,
  `preceding-sibling::`.
- Make the CSS-vs-XPath call by capability, and dodge the classic traps: exact-match `@class`,
  one-based indexing.

## Why This Matters
CSS selectors cannot do two things a UI tester needs weekly: **match on visible text** (there is
no `:contains()` in real CSS) and **walk upward** from a known element to its container (CSS has
no parent selector). XPath does both, which is the entire reason to carry a second selector
language — not preference, capability. Generated ids change every build; the label the user must
be able to read is a contract. "Find the row that says *Overdue* and click the button inside it"
is an everyday test requirement, and it is XPath or nothing in most drivers.

It is also interview shorthand: "when do you use XPath over CSS?" is asked constantly, and the
strong answer names the two capabilities instead of mumbling about preference.

## The Concept

### The grammar in five tokens
```
//article[@class='card']//h3/a
```
Read it: anywhere in the document (`//`), an `article` whose `class` attribute is exactly `card`
(`[@class='card']`), then anywhere below it (`//`) an `h3`, then a direct child (`/`) anchor.

- `/` — one step down, direct child.
- `//` — any depth below (descendant). Leading `//` means "anywhere in the document".
- `[...]` — a predicate: a filter on the step it follows. Predicates nest and combine
  (`[@type='submit' and not(@disabled)]`).
- `@attr` — an attribute, in predicates (`[@id='total']`) or as the selected value.
- `text()` — the element's text node, for matching what the user reads.

### Functions: locating dynamic content
```csharp
// Anchor whose text contains "Overdue":
driver.FindElement(By.XPath("//a[contains(text(), 'Overdue')]"));

// Every dd whose text starts with a SKU prefix:
driver.FindElements(By.XPath("//dd[starts-with(text(), 'BK-')]"));

// Class CONTAINS a token (see the trap below):
driver.FindElement(By.XPath("//div[contains(@class, 'alert')]"));
```
`contains()` and `starts-with()` are the dynamic-content tools: build-stamped ids
(`order-summary-8f3a`) defeat exact matches but `starts-with(@id, 'order-summary')` survives.
Matching on `text()` is the user-contract locator — it breaks only when the product rewords the
label, which is exactly when a human should re-look at the test anyway.

**The `@class` trap.** XPath's `@class='card'` is EXACT-STRING match on the whole attribute: an
element with `class="card featured"` does NOT match. CSS's `.card` matches the token. The XPath
escape is `contains(@class, 'card')` — which brings its own false positive (`card-list` contains
`card`). The precise form, when it matters:
`contains(concat(' ', normalize-space(@class), ' '), ' card ')`. Recognize all three on sight;
reach for the precise form only when the false positive is live.

### Absolute vs relative: the trap, named
```
/html/body/div/div/header/h1        <- absolute: every step is a hard dependency
//header/h1                          <- relative: anchored on something meaningful
```
An absolute path starts at `/html` and encodes EVERY intermediate element. Wrap the header in one
more `div` — something frontend refactors do constantly — and the locator dies, with a
no-such-element error that says nothing about why. It is not slower or "less elegant"; it is a
maintenance trap because it maximizes the number of unrelated changes that can break it. Browser
DevTools' "Copy XPath" produces absolute paths, which is why suites built by copy-paste rot
fastest. Write relative paths anchored on the most meaningful nearby feature — an attribute, a
label, a landmark element — and let everything between it and the root change freely.

### Axes: walking where CSS cannot
```csharp
// From the text a user knows, UP to the container a test needs:
driver.FindElement(By.XPath("//a[text()='Clean Code']/ancestor::article"));

// From a label, SIDEWAYS to its value (reading a <dl> like a human):
driver.FindElement(By.XPath("//dt[text()='SKU']/following-sibling::dd[1]"));
```
- `ancestor::` — climbs to any enclosing element matching the step. THE killer feature: start
  from user-visible text, climb to the row/card that contains it, then act on buttons inside that
  container. This move has no CSS equivalent.
- `parent::` — exactly one level up (`..` is shorthand).
- `following-sibling::` / `preceding-sibling::` — same parent, after/before. Reads
  label-and-value structures (definition lists, form rows, table cells) the way a person does:
  find the label, take the value beside it.

**One-based indexing.** `following-sibling::dd[1]` is the FIRST sibling — XPath counts from 1,
not 0. So does CSS `nth-child`, but every array in every programming language you use counts from
0; this off-by-one lives in interview questions and in 2 a.m. debugging sessions alike. Also
subtle: `//a[1]` means "the first `a` within EACH parent context", not "the first `a` in the
document" — that would be `(//a)[1]`.

### CSS vs XPath: the decision
Same speed in modern drivers — engines have long since closed the old performance gap — so choose
by CAPABILITY:
- **CSS** for structure, classes, attributes: shorter, more readable, and testable in DevTools
  with `$$()`.
- **XPath** when you need TEXT anchors or UPWARD/SIDEWAYS movement — the two things CSS cannot
  express. (XPath is testable in DevTools too: `$x("//h1")`.)
- The locator preference order still rules both: a good id beats either language.

Adjacent question: *is XPath only for Selenium?* No — it is a general XML/HTML query language
(it powers XML document queries in many ecosystems, including .NET's `XPathNavigator`), which is
why the skill transfers beyond UI testing.

## Say It in an Interview
- *"I reach for XPath for exactly two capabilities CSS lacks: matching on visible text and
  walking upward. 'Find the row that says Overdue and click its Renew button' is
  `//tr[contains(., 'Overdue')]` and then a relative step inside — no CSS selector can write
  that."*
- *"Absolute paths are a maintenance trap, not a style choice: every step from `/html` down is a
  hard dependency on the DOM's current shape, so any layout refactor anywhere on the chain kills
  the locator. I write relative paths anchored on something meaningful — and I never ship what
  DevTools' Copy-XPath produces, because that IS an absolute path."*
- *"The classic XPath trap is `@class='card'` — exact string match on the whole attribute, so a
  second class breaks it. `contains(@class, 'card')` is the working form, with a substring false
  positive you occasionally have to engineer around."*
- *"XPath indexes from one, and a bare `//a[1]` is first-per-context, not first-in-document —
  that needs `(//a)[1]`. Knowing those two off-by-ones has saved me real debugging time."*
- *"CSS and XPath perform the same in modern drivers, so my split is purely capability: CSS for
  structure and classes because it is shorter and DevTools-testable, XPath for text anchors and
  axes."*

## Check Yourself
1. Write the XPath that finds the `article` element containing a link whose text is exactly
   "Clean Code", and name the axis you used.
2. Your locator `//div[@class='alert']` stopped matching after a styling change added a second
   class. What happened, what is the quick fix, and what is the precise fix?
3. Why is `/html/body/div[2]/main/section/table/tbody/tr[3]/td[2]` a bad locator even though it
   works today? Give two distinct reasons.
4. A definition list shows `<dt>Due date</dt><dd>Tomorrow</dd>`. Write the XPath that reads the
   value for the "Due date" label.
5. What does `//li[1]` actually select on a page with four separate `<ul>` lists, and how do you
   select only the document's very first `li`?
6. An interviewer asks when you would use XPath over CSS. Give the two-capability answer in one
   sentence.

**Answers:** (1) `//a[text()='Clean Code']/ancestor::article` — the `ancestor::` axis, climbing
from user-visible text to its container. (2) XPath `@class=` is exact-match on the full attribute
string, so `class="alert alert-warning"` no longer equals `alert`; quick fix
`contains(@class, 'alert')`; precise fix
`contains(concat(' ', normalize-space(@class), ' '), ' alert ')` to avoid substring false
positives like `alert-box`. (3) Every step is a hard dependency, so unrelated layout refactors
anywhere on the chain break it; and when it breaks, the no-such-element failure carries no clue
which step went stale — brittle AND undebuggable. (Bonus: positional indexes like `tr[3]` also
break on data changes.) (4) `//dt[text()='Due date']/following-sibling::dd[1]`. (5) The first
`li` within EACH list — up to four elements; the document-first one is `(//li)[1]`. (6) XPath
when the locator needs a text anchor or upward/sideways traversal — the two things CSS selectors
cannot express; otherwise CSS.

## Summary
- Grammar: `/` child, `//` descendant-anywhere, `[...]` predicate, `@attr` attribute, `text()`
  text node — five tokens read 95% of real-world XPath.
- Functions `contains()`/`starts-with()` locate dynamic content; text matching is the
  user-contract locator; `@class=` is exact-match (the trap), `contains(@class, ...)` the working
  form.
- Absolute paths hard-depend on every ancestor and are a maintenance trap; write relative paths
  anchored on meaning; never ship DevTools Copy-XPath output.
- Axes are the capability CSS lacks: `ancestor::` (text-to-container climb), `parent::`,
  `following-sibling::`/`preceding-sibling::` (label-to-value reads).
- One-based indexing; `//x[1]` is first-per-context, `(//x)[1]` is first-in-document.
- CSS vs XPath is a capability call, not performance: CSS for structure, XPath for text and axes;
  ids beat both.

## Resources
- [XPath in the browser (MDN)](https://developer.mozilla.org/en-US/docs/Web/XML/XPath)
- [Locator strategies incl. XPath (selenium.dev)](https://www.selenium.dev/documentation/webdriver/elements/locators/)
