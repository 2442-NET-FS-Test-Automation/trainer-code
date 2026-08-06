# QC-5 (Front End) — Study Guide

Six topic clusters covering all 99 rubric objectives. Each cluster lists the objectives it covers with
their tier, recaps the concepts with pointers to the source note, flags the pitfalls that produce wrong
answers, and closes with one annotated worked example taken from the trainer demos.

Read this after `self-assessment-checklist.md` has told you where your gaps are. Follow a source pointer
whenever the recap is not enough — the notes are the full lesson, this is the revision layer.

---

## Cluster 1 — HTML: documents, tags, forms

**Sources:** `weeklytechrepo/Frontend-React/content/01-html/html-document-structure.md`,
`tags-elements-attributes.md`, `forms-inputs.md`. Demo: `demo/walkthroughs/01-html-css.md`,
end state `demo/frontend-demo/index.html` and `login.html`.

| Tier | Objective |
|---|---|
| Must | Describe what HTML is. |
| Must | Describe the structure of an HTML document and what is included in the different sections. |
| Must | List common HTML tags and describe why they are different from divs. |
| Must | Describe how/where you link an external CSS sheet into an HTML document. |
| Must | Describe how/where you link an external JS file into an HTML document. |
| Should | Construct an HTML form. |
| Should | Take in user input using a variety of input tags (text, checkbox, etc). |

### Concept recap

**What HTML is.** HyperText Markup Language — the markup language that defines the **structure and
content** of a page: headings, paragraphs, links, images, forms. It is not a programming language (no
logic, no loops) and it does not control presentation (CSS) or behavior (JavaScript). "HyperText" is the
defining idea: documents that link to other documents.
(`01-html/html-document-structure.md`)

**The document skeleton.** Four parts, always in this shape:

| Part | Job | Typical contents |
|---|---|---|
| `<!DOCTYPE html>` | tells the browser to parse as modern HTML | exactly that one line, first |
| `<html>` | root element wrapping everything | `lang` attribute for accessibility |
| `<head>` | metadata — machine-facing, nothing renders | `<title>`, `<meta charset>`, `<link>`, `<script>` |
| `<body>` | visible content — everything the user sees | headings, text, images, forms, scripts |

Omit the doctype and the browser drops into **quirks mode**, emulating 1990s rendering bugs — the doctype
is a rendering-mode switch, not decoration. The browser then parses the document into the **DOM**, an
in-memory tree of nodes that CSS matches against and JavaScript rewrites.

**Common tags versus divs.** The working vocabulary splits three ways: semantic page structure
(`<header>`, `<nav>`, `<main>`, `<section>`, `<article>`, `<footer>`), text content (`<h1>`-`<h6>`, `<p>`,
`<ul>`/`<ol>`/`<li>`, `<a href>`, `<img src alt>`), and generic containers (`<div>` block, `<span>`
inline). A `<div>` tells the browser, assistive tech, and search engines **nothing**. Semantic elements
carry meaning machines act on: screen readers announce `<nav>` and `<main>` as **landmarks** users jump
between; heading tags build the outline blind users navigate by; search engines weight `<h1>` and
`<article>`. Divs are not banned — they are correct when you need a styling or scripting hook and no
semantic element fits. (`01-html/tags-elements-attributes.md`)

**Anatomy.** A **tag** is the bracketed marker; **attributes** are the `name="value"` pairs inside the
opening tag; the **element** is the whole unit (opening tag + attributes + content + closing tag). **Void
elements** (`<img>`, `<br>`, `<hr>`, `<input>`, `<link>`, `<meta>`) have no content and no closing tag.
Global attributes to recognize on sight: `id` (unique per page), `class` (reusable groups), `style`
(inline one-off), `data-*` (custom data read from JS via `element.dataset`).

**Inline versus block.** Block elements start a new line, fill the parent's width, and accept
width/height (`div`, `p`, `h1`, `ul`, `section`, `form`). Inline elements flow within the line, size to
their content, and **ignore** width/height (`span`, `a`, `img`, `strong`, `label`). `inline-block` is the
hybrid. These are only defaults — CSS `display` overrides any of them.

**Linking CSS.** A void `<link>` element in the `<head>`:
`<link rel="stylesheet" href="css/styles.css">`. It goes in the head so styles are known *before* first
paint; put it late and users see a flash of unstyled content.

**Linking JS.** `<script src="js/app.js">`, and **placement is a performance decision**:

| Placement | Behavior | Use when |
|---|---|---|
| `<head>`, plain | blocks parsing immediately | almost never |
| end of `<body>` | runs after the HTML above it is parsed | classic safe default |
| `<head>` + `defer` | downloads in parallel, runs after parsing, in order | modern default |
| `<head>` + `async` | downloads in parallel, runs on arrival, order not guaranteed | independent scripts |

**Forms.** `<form action="/members/register" method="post">` says **where** and **how**. On submit the
browser collects every control that has a **`name`** and sends `name=value` pairs — no `name`, no
submission (`id` is for labels and scripts and never reaches the server). GET puts pairs in the URL
(bookmarkable reads); POST puts them in the body (state changes, secrets).
(`01-html/forms-inputs.md`)

**The input vocabulary.** One element, many types — `text`, `password`, `email`, `number`, `checkbox`,
`radio` (grouped by a **shared `name`** so only one can be chosen), `date`, `file` — plus `<select>` +
`<option>`, `<textarea>`, and `<button>` (default type inside a form is `submit`). Every control gets a
`<label>` via `for`/`id` or by wrapping it; placeholders are **not** labels.

### Key points and pitfalls

- **The missing-`name` bug.** A field renders, the user types, the server gets nothing — because only
  `name`d controls are submitted.
- **`method="get"` on a login** puts the password in the URL, and therefore in history, logs, and shared
  links.
- **Client-side validation is UX, never security.** `required`, `min`/`max`, `pattern` give instant free
  feedback, but anyone can delete the attributes in DevTools or skip the browser with curl. The server is
  the trust boundary and must revalidate everything.
- **A script in the `<head>` without `defer`** that queries an element in the body gets `null` — the
  parser has not reached that element yet.
- **Setting `width` on a `<span>` does nothing** — inline elements ignore it. Fix with
  `display: inline-block` or a block element.

### Worked example — the catalog page skeleton

From `demo/frontend-demo/index.html` (rung `01-html-css`), with the teaching comments the room typed:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <!-- Document metadata lives in the head - nothing here renders -->
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Library Catalog</title>
    <!-- External stylesheet: one file, every page links it -->
    <link rel="stylesheet" href="css/styles.css">
</head>
<body>
    <!-- Semantic layout tags: header/nav/main/section/footer describe STRUCTURE, not looks -->
    <header class="site-header">
        <h1>City Library</h1>
        <nav class="site-nav">
            <a href="index.html">Catalog</a>
            <a href="login.html">Sign in</a>
        </nav>
    </header>

    <main>
        <section id="catalog">
            <h2>Catalog</h2>
            <!-- Inline vs block: label/input/span flow INLINE; div/p/h2 are BLOCK and stack -->
            <div class="toolbar">
                <label for="search">Search:</label>
                <input type="search" id="search" name="search" placeholder="title or SKU">
                <span class="hint">try a SKU like BK-101</span>
            </div>

            <div class="cards" id="catalog-cards">
                <article class="card">
                    <h3>Clean Code</h3>
                    <dl>
                        <dt>SKU</dt><dd>BK-101</dd>
                        <dt>In stock</dt><dd>12</dd>
                    </dl>
                </article>
            </div>
        </section>
    </main>

    <footer class="site-footer">
        <p>City Library &mdash; frontend demo</p>
    </footer>

    <!-- the page's brain, deferred so the DOM exists when it runs -->
    <script src="js/app.js" defer></script>
</body>
</html>
```

Read it as the rubric reads it: doctype first, `lang` on the root, metadata and the stylesheet link in
the head, semantic regions in the body, `label`/`id` association on the search field, and the script at
the end of the body with `defer`. Notice `id="catalog-cards"` — that is the JavaScript hook the next two
demos query.

---

## Cluster 2 — CSS: rules, selectors, the cascade, the box model

**Sources:** `content/02-css/css-fundamentals-selectors.md`, `box-model-properties.md`,
`responsive-variables-animations.md`, `flexbox-grid.md`. Demo: `demo/walkthroughs/01-html-css.md`,
end state `demo/frontend-demo/css/styles.css`.

| Tier | Objective |
|---|---|
| Must | Describe the structure of a CSS style rule. |
| Must | Explain the CSS box model. |
| Must | Describe the different ways to add styling to an HTML document. |
| Must | Use the correct syntax for styling different elements such as by tag, class, id, etc. |
| Must | Describe CSS priority in regards to inline, internal, and external styles. |
| Nice | Describe the benefits of combinators and how to use them. |
| Nice | Make responsive webpages using CSS. |

### Concept recap

**What CSS is and the rule anatomy.** Cascading Style Sheets maps *selectors* (which elements) to
*declarations* (what visual properties). "Cascading" names the conflict-resolution algorithm.

```css
p {                    /* selector */
  color: red;          /* declaration: property, colon, value, semicolon */
  font-size: 16px;
}                      /* the braces enclose the declaration block */
```

Vocabulary you must produce on demand: **rule (ruleset)**, **selector**, **declaration block**,
**declaration**, **property**, **value**. (`02-css/css-fundamentals-selectors.md`)

**Three ways to attach styling.**

| Method | Syntax | When appropriate |
|---|---|---|
| Inline | `<p style="color: red;">` | one-off overrides, email HTML, script-computed values |
| Internal | `<style>` block in `<head>` | single-page prototypes, page-unique critical styles |
| External | `<link rel="stylesheet" href="site.css">` | everything else — the production default |

External wins on maintainability (one file, every page), caching (downloaded once), and separation of
concerns.

**Selector syntax.**

```css
p        { line-height: 1.5; }   /* type (tag) */
.card    { padding: 1rem; }      /* class */
#nav     { position: sticky; }   /* id */
p.card   { color: navy; }        /* compound: <p> that ALSO has .card */
h1, h2   { font-family: serif; } /* grouping */
```

Also recognize **attribute selectors** (`input[type="email"]`, `a[href^="https"]`) and **pseudo-classes**
which select by state or position (`a:hover`, `input:focus`, `li:first-child`, `tr:nth-child(even)`).

**Priority — who wins.** In order: (1) inline `style=""` beats internal and external rules; (2) among
stylesheet rules, higher **specificity** wins — counted as **(ids, classes, elements)** compared left to
right, so `#nav .card a:hover` scores (1, 2, 1) and one id beats any number of classes; (3) on a tie,
**source order** — the later rule applies. `!important` overrides all of it, and the only way to beat an
`!important` is another `!important`, which is exactly why you avoid it.

**The box model.** Content in the middle, then **padding** (inside the border), the **border**, then
**margin** (transparent space outside, separating the box from its neighbors). Padding takes the
element's background; margin never does. (`02-css/box-model-properties.md`)

```css
.card { width: 300px; padding: 20px; border: 5px solid; }
/* content-box (default): rendered width = 300 + 40 + 10 = 350px  */
/* border-box:            rendered width = 300px, content shrinks */
```

Which is why nearly every codebase opens with
`*, *::before, *::after { box-sizing: border-box; }`. Also know **margin collapsing**: adjacent vertical
margins merge to the larger of the two, not the sum.

**Combinators** select by document position without inventing classes: `div p` (descendant, any depth),
`div > p` (direct child), `h1 + p` (the immediately-next sibling), `h1 ~ p` (all later siblings).

**Responsive design** is three ingredients plus one tag: fluid layouts, relative units (`%`, `rem`,
`vw`/`vh`), media queries — and `<meta name="viewport" content="width=device-width, initial-scale=1.0">`
in the head, without which a phone lays out at a fake ~980px desktop width and your breakpoints never
fire. Mobile-first (`min-width` queries layering complexity upward) is the modern norm.
(`02-css/responsive-variables-animations.md`)

### Key points and pitfalls

- **A missing semicolon silently kills the *next* declaration.**
- **Specificity arithmetic:** `(1, 1, 1)` beats `(0, 4, 0)`. Say "ids, then classes, then elements,
  compared left to right."
- **`display: none` removes** the element from layout entirely; **`visibility: hidden` keeps its box** as
  a blank hole, so the layout does not jump when it toggles.
- **Units and accessibility:** users who raise their browser's base font size only get bigger text if you
  sized in `rem`/`em`; `px` font sizes ignore that preference.
- **Animate `transform` and `opacity`**, not `width`/`top`/`margin` — geometry changes force layout
  recalculation every frame.

### Worked example — the specificity question, answered concretely

Given this markup and stylesheet:

```html
<div id="catalog"><p class="card featured">Clean Code</p></div>
```

```css
p                 { color: black; }   /* (0, 0, 1) */
.card             { color: navy;  }   /* (0, 1, 0) */
.featured         { color: teal;  }   /* (0, 1, 0) — ties .card, declared later */
#catalog .card    { color: green; }   /* (1, 1, 0) — one id outranks the classes */
```

The paragraph renders **green**: the id selector's (1, 1, 0) beats every class rule regardless of order.
Add `<p class="card featured" style="color: crimson">` and it renders **crimson** — inline beats all
stylesheet rules. Add `!important` to the `.card` rule and it wins over even the inline style. That is
the whole priority ladder in one example, and it is the shape the exam asks for.

---

## Cluster 3 — JavaScript language core

**Sources:** `content/03-javascript/js-language-runtime.md`, `variables-scope-types-coercion.md`,
`objects-arrays-loops.md`, `functions-this-closures.md`, `error-handling.md`. Demo:
`demo/walkthroughs/02-js-page.md` and the lectured cheatsheet `demo/frontend-demo/js/js-basics.js`.

| Tier | Objective |
|---|---|
| Must | Describe what JS is. |
| Must | Describe what type coercion is. |
| Must | Describe what truthy/falsy is. |
| Must | Describe the different variable scopes in JS. |
| Must | Explain the different data types in JS. |
| Must | Create variables in JS. |
| Must | Create objects in JS. |
| Must | Handle errors in JS. |
| Must | Create arrays in JS. |
| Must | Describe the different array methods and how to use them. |
| Must | Loop through arrays. |
| Must | Describe the different types of functions in JS. |
| Should | Use template literals. |
| Should | Describe what the this keyword is. |
| Should | Explain the role of callbacks in JavaScript programming. |
| Should | Define arrow functions and explain the benefits of using arrow functions. |
| Should | Create arrow functions. |
| Should | Create anonymous functions. |
| Nice | Describe and explain a closure. |
| Nice | Describe what function and variable hoisting is. |
| Nice | Describe how inheritance works in JS. |

### Concept recap

**What JavaScript is.** A **dynamically typed** scripting language — variables carry no declared type and
a value's type is checked at runtime. Modern engines (V8 in Chrome and Node) **JIT-compile** it, so
"interpreted" is only half true today. It runs **single-threaded with an event loop**, in the browser to
make pages interactive and in **Node.js** everywhere else. ECMAScript is the standard; JavaScript is the
everyday name for implementations of it. (`03-javascript/js-language-runtime.md`)

**Variables and scope.** `const` by default, `let` when you will reassign, `var` recognized but never
written. `let`/`const` are **block-scoped** to the nearest braces; `var` is **function-scoped**. The
canonical proof:

```js
for (var i = 0; i < 3; i++) setTimeout(() => console.log(i));  // 3, 3, 3
for (let j = 0; j < 3; j++) setTimeout(() => console.log(j));  // 0, 1, 2
```

Nuance interviewers probe: `const` freezes the **binding**, not the value — a `const` array can still be
pushed to. (`03-javascript/variables-scope-types-coercion.md`)

**Data types.** Seven primitives — `string`, `number`, `boolean`, `null`, `undefined`, `symbol`,
`bigint` — and everything else is an **object**, including arrays and functions. `typeof null` returns
`"object"` (a historic bug kept for compatibility — memorize it); `typeof [1,2]` is `"object"`, so
`Array.isArray()` is the real test. `undefined` means "never assigned"; `null` means "deliberately
empty."

**Coercion.** JavaScript converts types implicitly, and `+` is the trap: if either operand is a string
`+` concatenates; every other arithmetic operator converts to number.

```js
"5" + 1   // "51"
"5" - 1   // 4
"5" == 5  // true  — == coerces before comparing
"5" === 5 // false — === compares type AND value
```

Use `===` by default: its result is predictable from the operands without reciting a coercion table.
`NaN` is the only value not equal to itself — test with `Number.isNaN(x)`.

**Truthy and falsy.** Exactly **six falsy values**: `false`, `0`, `""`, `null`, `undefined`, `NaN`.
Everything else is truthy — including `"0"`, `[]`, and `{}`. Hence `input || "guest"` wrongly replaces a
legitimate `0` or `""`, which is why `??` (nullish coalescing) exists.

**Objects and arrays.** Literals plus dot access (known keys) and bracket access (dynamic keys);
shorthand properties, computed keys, destructuring, and spread — remembering **spread is a shallow copy**
so nested objects are shared. Arrays are objects with numeric keys and a managed `length`.
(`03-javascript/objects-arrays-loops.md`)

**The array toolbox.**

| Method | Returns | Mutates? |
|---|---|---|
| `map(fn)` | new array, transformed | no |
| `filter(fn)` | new array, kept items | no |
| `reduce(fn, init)` | one accumulated value | no |
| `find(fn)` | first match or `undefined` | no |
| `forEach(fn)` | `undefined` — side effects only | no |
| `push`/`pop`, `shift`/`unshift`, `splice`, `sort` | varies | **yes** |
| `slice(a, b)`, `includes`, `some`/`every` | new array / boolean | no |

Two traps: **slice copies out, splice edits in place**; and **`sort` mutates and compares as strings** by
default, so `[10, 2].sort()` gives `[10, 2]` — always pass `(a, b) => a - b` for numbers.

**Loops.** `for` (index math, `break`, fastest), `for...of` (the **values** — the default for arrays),
`for...in` (the **keys** — for objects, not arrays), `forEach` (callback per item, **cannot break**),
`while`/`do-while`.

**Functions.** Declarations (hoist fully), expressions, arrows, anonymous functions, and object methods —
plus default parameters, rest parameters, and the IIFE shape. A **callback** is a function handed to
other code to run later; nesting them for sequential async work produces **callback hell**, which is what
promises fix. (`03-javascript/functions-this-closures.md`)

**Arrow functions.** `n => n * 2`, `(a, b) => a + b`, `() => ({ id: 1 })` (object literal needs wrapping
parens). Benefits: shorter syntax and — the important one — **lexical `this`**, so callbacks inside a
method keep the object. Do **not** use them as object methods or constructors.

**`this` is decided at the call site**: the object before the dot in a method call, the new instance
under `new`, `undefined` in a strict plain call (the classic detached-method bug), and lexically
inherited in an arrow. `call`/`apply`/`bind` set it explicitly.

**Closures.** A function that retains access to the variables of the scope where it was **defined**, even
after that scope returned — which gives private state without classes. The counter factory is the
canonical example; each call gets an independent `count`.

**Error handling.** `try` runs the risky code, `catch (e)` handles a throw, **`finally` runs on both
paths** — even past a `return` — so cleanup lives there. Throw `new Error("message")`, never a string: an
instance carries `message`, `name`, and `stack`. Recognize `TypeError`, `RangeError`, `SyntaxError`,
`ReferenceError`, and `class NotFoundError extends Error` for typed catching with `instanceof`.
(`03-javascript/error-handling.md`)

**Inheritance** is **delegation up the prototype chain**: a failed property lookup walks the object's
prototype, then that prototype's prototype, until `null`. `class Book extends Media` is syntax sugar over
exactly that — methods land on `Book.prototype` and instances delegate to it.

**Hoisting.** `var` hoists initialized to `undefined`; function declarations hoist fully; `let`/`const`
hoist but sit in the **temporal dead zone** until their declaration line, so reading them early throws a
`ReferenceError` — a loud failure instead of `var`'s silent `undefined`.

### Key points and pitfalls

- **`var` in a loop with async callbacks** logs the final value N times. This is the highest-frequency
  code-reading trap in the section.
- **A detached method loses `this`:** `const f = obj.method; f();` — fix with `obj.method.bind(obj)` or an
  arrow wrapper.
- **`catch (e) {}` that swallows** converts a loud failure into silent wrong behavior. Catch where you can
  act; otherwise rethrow.
- **`for...in` on an array** yields string indexes plus inherited enumerable keys. Use `for...of`.
- **Spread and `Object.assign` are shallow.** `copy.author === book.author` after a spread.

### Worked example — closures, delegation, and the array toolbox in one handler

From `demo/frontend-demo/js/app.js` (rung `03-http-fetch`), the live-search wiring:

```js
// live search: filter the in-memory list on every keystroke (closure over catalogItems)
document.querySelector("#search").addEventListener("input", (e) => {
    const q = e.target.value.trim().toLowerCase();
    renderCards(catalogItems.filter(item =>
        item.name.toLowerCase().includes(q) || item.sku.toLowerCase().includes(q)));
});
```

Four rubric rows in six lines. The arrow function passed to `addEventListener` is an **anonymous arrow
callback**. It **closes over** `catalogItems`, a variable declared in the module scope that outlives every
individual event — that is a closure doing real work, not a textbook counter. `filter` returns a **new**
array without touching the original, so the full catalog survives for the next keystroke. And `e.target`
is the event object's origin element, read for its current `value`.

---

## Cluster 4 — Browser JavaScript: DOM, events, promises, fetch

**Sources:** `content/03-javascript/dom-selection-manipulation.md`, `events.md`, `promises-async.md`,
`fetch-json-http.md`, `error-handling.md`. Demo: `demo/walkthroughs/02-js-page.md`,
`03-http-fetch.md`, end state `demo/frontend-demo/js/app.js`.

| Tier | Objective |
|---|---|
| Must | Describe what the DOM is. |
| Must | Query the DOM for elements. |
| Must | Describe what event listeners are. |
| Must | Insert new elements into the DOM. |
| Must | Explain what a JavaScript Promise is and when it is used to handle asynchronous operations. |
| Must | Describe what type of object the Fetch API returns. |
| Must | Explain what JSON is. |
| Must | Handle a failed request when using the Fetch API. |
| Must | Describe the different promise methods. |
| Must | Explain the difference between synchronous and asynchronous programming. |
| Should | List the steps to sending an HTTP request using the Fetch API. |
| Should | Describe what async/await is and how they compare to using .then(). |
| Should | Explain what JSON.stringify() and JSON.parse() are. |
| Nice | Describe what bubbling and capturing are and their difference. |
| Nice | Describe some methods on the event object and what they do. |
| Nice | Explain how to chain multiple asynchronous operations using Promises or async/await. |
| Nice | Implement error handling using try-catch blocks with async/await. |
| Nice | Describe and explain the event loop. |
| Nice | Describe the difference between Fetch and XHR. |

### Concept recap

**The DOM.** When the browser parses HTML it builds the **Document Object Model** — an in-memory tree of
node objects, one per element. The rendered page is a projection of that tree: change a node and the
browser re-renders that part. JavaScript reaches it through the global `document`. The DOM is a browser
API standard, not part of the JavaScript language.
(`03-javascript/dom-selection-manipulation.md`)

**Querying.**

```js
const nav   = document.getElementById("nav");           // fastest, id only, one element
const card  = document.querySelector(".card");          // first match for any CSS selector
const items = document.querySelectorAll("ul.books li"); // ALL matches, a static NodeList
```

`querySelectorAll` returns a **static NodeList** (a snapshot, supports `forEach`); the older
`getElementsByClassName` family returns a **live HTMLCollection** that updates as the document changes.

**Reading and modifying.** `textContent` (plain text, markup inert) versus `innerHTML` (parses as
markup). This is a **security boundary**, not a style choice: writing untrusted data with `innerHTML`
lets an attacker inject `<img onerror=...>` — a cross-site scripting hole. Data goes in via
`textContent`; `innerHTML` is for markup you wrote. Also `setAttribute`/`getAttribute`,
`classList.add/remove/toggle`, and `el.style.backgroundColor` (camelCase).

**Inserting.** `document.createElement("li")`, set `textContent` and classes, then `list.append(li)`
(accepts multiple nodes and strings; the older `appendChild` takes exactly one node). `el.remove()`
removes an element. The array-to-elements loop is the **render-a-collection** pattern — exactly what a
framework's list rendering automates. Batch writes (build detached, or use a `DocumentFragment`) because
every live-DOM write can force reflow and repaint.

**Template literals.** Backtick strings with `${}` interpolation and multi-line support — the standard
way to build any string from data. When the result is fed to `innerHTML`, every interpolated value is a
potential injection point.

**Event listeners.** `element.addEventListener(type, handler, options)` registers a callback to run when
the event fires. Removal requires the **same function reference**, which is why an inline arrow is
effectively unremovable. The older `btn.onclick = handler` property style allows only one handler.
(`03-javascript/events.md`)

Everyday events: `click`, `submit` (on the form, not the button), `input` (every keystroke) versus
`change` (on commit), `keydown`, `DOMContentLoaded` (HTML parsed, DOM queryable) versus `load` (page plus
all assets).

**The event object.** `event.target` is where the event **originated**; `event.currentTarget` is where
**your listener sits** — they differ whenever the event bubbled up from a descendant.
`event.preventDefault()` cancels the browser's default action (a form's page reload, a link's
navigation) while propagation continues; `event.stopPropagation()` halts the journey without cancelling
the default. They are independent.

**Capturing and bubbling.** An event travels **capturing** from `document` down to the target, then
**bubbles** back up. Listeners fire during **bubbling by default**; `{ capture: true }` opts into the way
down. Bubbling's payoff is **event delegation**: one listener on the container serves every descendant —
fewer listeners, and rows added later are handled automatically.

**Synchronous versus asynchronous.** Synchronous code blocks until each line finishes, and JavaScript
runs on a **single thread**, so a long synchronous computation freezes the page. Asynchronous operations
are **scheduled**: the work starts, your function returns immediately, and a callback runs later with the
result. (`03-javascript/promises-async.md`)

**What a Promise is.** An object representing the eventual result of an async operation, always in one of
three states — **pending**, **fulfilled** (has a value), **rejected** (has a reason) — and once settled it
never changes. Consume with `.then` / `.catch` / `.finally`; each `.then` returns a **new** promise,
which is what makes chaining work.

**The promise methods.** Instance: `.then`, `.catch`, `.finally`. Static combinators:

| Method | Resolves with | Rejects |
|---|---|---|
| `Promise.all` | array of all values, in order | on the **first** rejection (fails fast) |
| `Promise.allSettled` | array of `{status, value/reason}` | never |
| `Promise.race` | first to **settle** | if the first settler rejected |
| `Promise.any` | first to **fulfill** | only if all reject |

**async/await** is syntax over promises — no new machinery. An `async` function **always returns a
Promise**; `await` unwraps a value and pauses **only that function** (the thread moves on). Rejections
surface as ordinary exceptions handled with `try`/`catch`. Await sequentially only when call B needs call
A's result; independent calls belong in `Promise.all`, turning `timeA + timeB` into `max(timeA, timeB)`.

**The event loop.** One call stack; async callbacks wait in queues; the loop moves them onto the stack
only when it is empty. **Microtasks** (promise callbacks) drain completely before the next **macrotask**
(`setTimeout`, events) — which is why a resolved `.then` logs before a zero-millisecond timer.

**JSON.** JavaScript Object Notation — a language-independent **text** format for data exchange, with
rules stricter than JS object literals: **double-quoted keys**, no trailing commas, no comments, no
functions and no `undefined`. `JSON.stringify(obj)` serializes; `JSON.parse(str)` reconstructs — and
stringify silently **drops functions and `undefined`**, so the round trip is a lossy clone for anything
richer than plain data. (`03-javascript/fetch-json-http.md`)

**Fetch.** `fetch(url)` returns a **Promise that resolves to a `Response`**; reading the body is a second
async step, `response.json()`, which returns another Promise. The options object carries `method`
(GET default), `headers`, and `body` — for JSON you set `Content-Type: application/json` and stringify
yourself.

**The six steps of a request:** build the URL, build the options, `await fetch(url, options)`, check
`response.ok`, parse the body with `await response.json()`, use it.

**Handling failure — the part everyone gets wrong.** `fetch` rejects **only on network failure** (DNS
down, connection refused, CORS block). An HTTP **4xx or 5xx resolves normally** — a 404 is a successful
conversation whose answer was "not found." So there are two failure lanes: reachability (the `catch`) and
refusal (the `response.ok` check).

**Fetch versus XHR.** `XMLHttpRequest` is the older event/callback-based object
(`xhr.open`/`xhr.onload`/`xhr.send`). Fetch wins on ergonomics: promise-based, cleaner Request/Response
model, streaming. The shared quirk: **neither** treats an HTTP error status as a failure.

**Adjacent facts the exam likes.** **CORS** — the browser blocks your JS from reading a cross-origin
response unless the server opts in with `Access-Control-Allow-Origin`, which is the "works in curl, fails
in the browser" symptom. **401** means not authenticated; **403** means authenticated but not allowed.

### Key points and pitfalls

- **A 404 does not reject.** If you cannot say "fetch only rejects on network failure, so I check
  `response.ok`," you will lose the row.
- **Two awaits, two things:** the first waits for headers/status (the `Response`), the second for the
  parsed body.
- **`try { fetch(url).then(...) } catch {}` catches nothing** — nothing is awaited, so the try block
  finishes before the promise settles.
- **Serial awaits over independent calls** is the most common self-inflicted latency bug.
- **Ordering question:** sync code, then microtasks (promise callbacks), then macrotasks (`setTimeout`) —
  even at 0 ms.
- **Every chain needs a terminal `.catch`,** or an `await` inside a `try`; otherwise you get an unhandled
  rejection.

### Worked example — the fetch lifecycle with both failure lanes

From `demo/frontend-demo/js/app.js` (rung `03-http-fetch`):

```js
async function loadCatalog() {
    const container = document.querySelector("#catalog-cards");

    // createElement + appendChild: build a node in memory, then attach it
    const loading = document.createElement("p");
    loading.className = "hint";
    loading.textContent = "loading...";
    container.innerHTML = "";
    container.appendChild(loading);

    try {
        const res = await fetch(`${API}/api/Inventory`);   // GET, cross-origin - CORS's moment
        if (!res.ok) {
            container.innerHTML = `<p class="hint">API said ${res.status}</p>`;
            return;
        }
        catalogItems = await res.json();                   // body arrives as a second promise
        renderCards(catalogItems);
    } catch (err) {
        // network/CORS failure lands here - render it, AND log the real error.
        console.error(err);
        container.innerHTML = `<p class="hint">cannot reach the API - is it running on :5137?</p>`;
    }
}
```

Walk it as the rubric does. `createElement` + `textContent` + `appendChild` is the granular insertion API
(insert-new-elements, Must). The two `await`s are the Response and then the parsed body (fetch-returns,
Must). The `if (!res.ok)` branch handles **refusal** and the `catch` handles **unreachability** —
together, the complete answer to "handle a failed request" (Must). And the sibling function
`showSupplierPrice` in the same file branches on `res.status === 401` specifically, which is the
401-versus-403 fact rendered as UX.

---

## Cluster 5 — TypeScript

**Sources:** `content/04-typescript/why-typescript-tooling.md`, `basic-special-object-types.md`,
`aliases-interfaces-unions.md`, `casting-guards-asconst.md`, `tsconfig.md`,
`classes-generics-functions.md`. Demo: `demo/walkthroughs/04-ts-basics.md`, `05-ts-advanced.md`,
end state `demo/frontend-demo/ts/` (`types.ts`, `ts-client.ts`, `demo.ts`, `tsconfig.json`).

| Tier | Objective |
|---|---|
| Must | Compare/contrast TypeScript to JavaScript. |
| Must | Describe and implement basic types in TypeScript. |
| Must | Implement user defined types in TypeScript. |
| Must | Describe and implement casting in TypeScript. |
| Must | Describe and demonstrate the process to transpile and run TypeScript. |
| Should | Implement TypeScript outside of Angular/React environments using plain .ts files. |
| Should | Describe the purpose of the "strict" flag in the tsconfig.json file. |
| Should | Describe and implement union types. |
| Should | Describe and implement type guards. |
| Should | Describe and implement type aliasing. |
| Nice | Configure the TypeScript compiler using options in the tsconfig.json based on project needs. |
| Nice | Describe and leverage generic types. |

### Concept recap

**TypeScript versus JavaScript.** TypeScript is JavaScript **plus a static type layer checked at compile
time**. Every valid `.js` file is already valid `.ts`. When `tsc` transpiles, **all type information is
erased** and plain JavaScript comes out — there is no TypeScript runtime, and browsers and node never
execute `.ts`. The consequence to say out loud: **types cannot check anything at runtime**, so data
arriving from `fetch` or user input must be validated with actual code.
(`04-typescript/why-typescript-tooling.md`)

The payoff is early detection of wiring mistakes, editor autocomplete and safe refactoring, and
self-documenting signatures. The honest cost is a build step, a learning curve, and friction with untyped
libraries.

**Transpile and run.** The loop is always: edit `.ts` -> `tsc` -> run the `.js`.

```
npm install --save-dev typescript   # per-project (or -g for a global tsc)
npx tsc --init                      # generates tsconfig.json
npx tsc                             # config-driven: compiles the whole project
node dist/app.js                    # run the emitted JavaScript
```

`npx ts-node app.ts` collapses it to one step. One default that surprises people: **type errors do not
stop emit** — `tsc` reports the error and still writes the `.js`; `noEmitOnError` changes that.

**A standalone workflow with no framework** is exactly the above: `npm init -y`, install typescript,
`tsc --init`, write plain `.ts` files, compile, run with node. That is the Should-know row, and it is how
the Week 6 TypeScript client was built.

**Basic types.** `let age: number = 34;` — but TypeScript **infers** from initializers, so the working
rule is: let inference carry local variables, **annotate function boundaries** (parameters have no
initializer to infer from, and an explicit return type turns a function into a checked contract).
(`04-typescript/basic-special-object-types.md`)

**Special types.** `any` opts out of checking entirely **and it spreads** — every property access on an
`any` is `any`, so one `any` at a fetch boundary silently untypes a whole call chain. `unknown` accepts
any value too but blocks **every** use until you narrow it, which makes it the safe default for external
data. `void` = returns nothing useful; `never` = cannot exist, the engine of exhaustiveness checks.
Under `strictNullChecks`, `null` and `undefined` are their own types and are not assignable to `string`
or `number` — absence must be declared (`string | undefined`) and handled.

**User-defined types.** Two tools:

```ts
interface User { id: number; name: string; }
type Point = { x: number; y: number };
```

For a plain object shape they are interchangeable. The differences: an **alias can name any type** —
unions, primitives, function types, arrays — while an interface can only describe object/function shapes;
interfaces get `extends` and **declaration merging**, aliases get mapped/conditional forms. Practical
default: **interface for public object contracts, type for everything else**.
(`04-typescript/aliases-interfaces-unions.md`)

**Type aliasing** is that naming ability itself: `type ID = string | number;`,
`type Comparator = (a: ID, b: ID) => number;`, `type Celsius = number;`.

**Union types.** `let id: string | number;` — and the rule that trips everyone: **before narrowing you
may only use members common to every arm**, so `id.toUpperCase()` errors while `id.toString()` is fine.
Unions of string literals (`type OrderStatus = "pending" | "shipped" | "delivered"`) are the idiomatic
closed-value-set pattern. Also recognize the **intersection** `type Employee = Person & Payroll`.

**Structural typing.** TypeScript compares **shapes, not names** — two independently declared identical
types are interchangeable. C# and Java are **nominal**, where the declared name is the identity. This is
the biggest mental shift for developers arriving from C#.

**Casting (type assertions).** `const input = document.getElementById("email") as HTMLInputElement;`
overrules the compiler. The critical model: **an assertion converts nothing at runtime** — it is erased
like every other type, so a wrong assertion compiles clean and crashes later. Recognize the older
`<HTMLInputElement>value` spelling but write `as` (the angle-bracket form collides with JSX). The double
assertion `value as unknown as T` is a smell. (`04-typescript/casting-guards-asconst.md`)

**Type guards.** A runtime check the compiler recognizes and uses to **narrow**: `typeof` for primitives,
`instanceof` for class instances, `in` for property presence, truthiness for stripping `null`/`undefined`.
The flagship is the **discriminated union** — every arm carries a literal `kind`, a `switch` narrows
perfectly, and a `never` default makes any unhandled new arm a compile error. For reusable guards,
recognize the **type predicate** signature `function isBook(x: unknown): x is Book`.

**Assertion versus guard, in one line:** an assertion says *trust me now* and shifts risk to runtime; a
guard *proves it* at runtime and the compiler rewards the proof with narrowing. Default to guards at data
boundaries.

**tsconfig.json** marks a folder as the root of a TypeScript project — with it present, a bare `tsc`
compiles the whole project. Options you will touch: `target` (emitted JS version), `module`, `rootDir`
and `outDir`, `strict`, `sourceMap`, `esModuleInterop`, `include`/`exclude`. **`strict` is an umbrella**:
its two headline members are **`strictNullChecks`** (null/undefined stop being assignable to everything —
this kills the biggest class of JS runtime crashes) and **`noImplicitAny`** (every parameter must be
typed or inferred). Enable it day one; retrofitting it onto a mature codebase surfaces every loose spot at
once. (`04-typescript/tsconfig.md`)

**Generics.** `<T>` declares a type parameter filled in per call, usually by inference, and it
**preserves the type connection** that `any` destroys:

```ts
function firstAny(arr: any[]): any { return arr[0]; }   // string-ness lost
function first<T>(arr: T[]): T { return arr[0]; }       // T inferred and PRESERVED
```

Constrain with `<T extends { id: number }>` so the body may use `.id`. Read nested signatures inside-out:
`Promise<Map<string, Order[]>>` is "a promise resolving to a map from strings to arrays of Orders."
(`04-typescript/classes-generics-functions.md`)

### Key points and pitfalls

- **"Types are erased"** is the answer to half this section. Say it for the compare row, the casting row,
  and the "can a type reject bad fetch data" question.
- **`any` versus `unknown`:** both accept everything; the difference is on the **use** side.
- **A wrong assertion is a manufactured runtime bug with a clean compile.**
- **error TS5011** — `outDir` requires an explicit `rootDir`. Add `"rootDir": "./src"`.
- **`interface` cannot express a union.** If the question says "string or number," the answer is a `type`
  alias.

### Worked example — a type predicate guarding a generic client

From `demo/frontend-demo/ts/ts-client.ts` (rung `05-ts-advanced`), the shape the room typed:

```ts
export interface ApiError {
    status: number;
    message: string;
}

// "value is ApiError" - a Type Predicate. If this returns true, the compiler
// treats the value that was passed in as an ApiError from here on.
export function isApiError(value: unknown): value is ApiError {
    if (typeof value !== "object") return false;   // primitives cannot be an ApiError
    if (value === null) return false;              // typeof null === "object" - exclude explicitly
    if (!("status" in value)) return false;
    if (!("message" in value)) return false;
    return true;
}

export class ApiClient {
    // parameter property: access modifier + readonly + name + type + default, in one line
    constructor(private readonly baseUrl: string = "http://localhost:5137") {}

    async getJson<T>(path: string): Promise<T | ApiError> {
        try {
            const res = await fetch(`${this.baseUrl}${path}`);
            if (!res.ok) return { status: res.status, message: `API said: ${res.status}` };
            return await res.json() as T;          // assertion: "trust me, it matches T"
        } catch (err) {
            console.log(err instanceof Error ? err.message : "unknown error?");
            return { status: 0, message: "Cannot reach the API. Check if it's on, or CORS" };
        }
    }
}
```

Five rubric rows in one file. `interface ApiError` is a **user-defined type**. `Promise<T | ApiError>` is
a **union** return, so every caller must narrow before use — which is what `isApiError` is for: a **type
guard** written as a **type predicate**, using `typeof`, an explicit `null` exclusion (because
`typeof null === "object"`), and `in` checks. `getJson<T>` is a **generic** that keeps the caller's type
flowing through to the result. And `await res.json() as T` is **casting** — note honestly what it does
and does not do: it changes the compiler's belief, nothing else, so if the API's shape drifts, this line
compiles happily and the bug appears later. That is the assertion-versus-guard trade-off in live code.

---

## Cluster 6 — React

**Sources:** `content/05-react/` (all 17 notes; per-row map in `content/README.md`). Demo:
`demo/walkthroughs/06-vite-components.md` through `10-advanced.md`, end state
`demo/react-spa-demo/src/`.

| Tier | Objective |
|---|---|
| Must | Describe and implement functional components in React. |
| Must | Explain the difference between Single Page Applications and Multi Page Applications. |
| Must | Utilize and explain common React hooks: useState, useEffect, and useContext. |
| Must | Pass props to components and manage local component state. |
| Must | Create and run a React application using Vite CLI. |
| Must | Explain the lifecycle of a React component. |
| Must | Describe how the React Virtual DOM works and how it improves performance. |
| Must | Make HTTP requests using Axios or Fetch and handle the response. |
| Must | Write and explain JSX syntax and how it integrates with JavaScript. |
| Must | Use useReducer for complex state management scenarios. |
| Must | Explain and apply the principles of state immutability in React. |
| Must | Handle user input through form elements and manage form state. |
| Must | Implement component communication through props and callbacks (Parent to Child & vice versa). |
| Must | Build and use nested component structures to model UI architecture. |
| Must | Use React Router to implement navigation in a single-page application. |
| Must | Apply styling to components using inline styles, CSS modules, or external stylesheets. |
| Must | Use Lists and Keys correctly to render dynamic components efficiently. |
| Should | Use Context.Provider tags to wrap components and distribute application state. |
| Should | Route users between components through the use of BrowserRouter. |
| Should | Leverage route guards to change the routing behavior based on the given state. |
| Should | Use a Reducer to manage a set of complex known states. |
| Should | Conditionally render a component based on user interaction and/or state. |
| Should | Describe the benefits of TypeScript in React development. |
| Should | Leverage NPM libraries in a React project to add functionality. |
| Should | Lift state up to a parent component to share data between child components. |
| Should | Describe how one-way data flow works in React. |
| Should | Build a reusable component using TSX with type-checked props. |
| Nice | Use createContext and Context.Provider to manage global state. |
| Nice | Use refs to store information without triggering a re-render. |
| Nice | Use Jest and a React testing library to test components. |
| Nice | Leverage advanced routing techniques to create parent-child routing, or through passing variables into routes. |
| Nice | Explain and implement higher-order and container components for reusable logic. |
| Nice | Compare and implement controlled vs uncontrolled components in form handling. |

### Concept recap

**Functional components.** A capitalized function that returns something renderable. Two rules make it a
component: the **capitalized name** (React treats lowercase tags as raw HTML) and a renderable return.
Props in, UI out — no class, no `render` method. (`05-react/react-jsx-components.md`)

**JSX.** Not HTML and not a string — syntax sugar a compiler rewrites into `React.createElement("h1",
{ className: "title" }, "Hello")`. Because it is JavaScript: attributes are camelCase (`className`,
`htmlFor`, `onClick`), every element closes, and a component returns **one** root (wrap siblings in a
Fragment `<>...</>`). Curly braces embed any JavaScript **expression** — a statement like `if` is illegal,
so you use a ternary, `&&`, or compute the value above the `return`.

**Vite CLI.**

```
npm create vite@latest my-app -- --template react
cd my-app
npm install
npm run dev
```

The lone `--` separates npm's own arguments from the ones handed to the Vite scaffolder;
`--template react-ts` selects the TypeScript template. `npm run dev` serves on `http://localhost:5173`.

**SPA versus MPA.** An **MPA** requests a whole new HTML document from the server on every navigation and
repaints — simple, great SEO and first paint, but a full reload each time and lost client state. An
**SPA** loads one shell plus a JS bundle once, then swaps views client-side and fetches data as JSON —
instant navigation and persistent state, at the cost of a heavier first load and extra work for SEO and
first paint. React apps are SPAs by default. (`05-react/spa-vs-mpa-virtualdom.md`)

**The Virtual DOM.** A lightweight in-memory JavaScript tree of plain objects describing what the UI
*should* be. It is cheap to build and compare, unlike real DOM nodes, whose every change can trigger style
recalculation, layout, and repaint. A state update runs three steps:

1. **Render** — React re-runs the component and builds a new Virtual DOM tree.
2. **Diff (reconcile)** — it compares the new tree to the previous one.
3. **Commit** — it applies only the differences to the real DOM.

That is why "re-render on every state change" is fast: the re-render produces cheap objects, and only the
genuine differences reach the costly real DOM. Keys give list items stable identity so the diff can match
items across renders.

**Props and state.** **Props** are read-only inputs a parent passes down; **state** is memory a component
owns. `const [borrowed, setBorrowed] = useState(false)` returns the current value and a setter; the
argument is the initial value, used only on the first render. Never assign to the variable directly —
call the setter, and when the next value depends on the previous, pass a function
(`setCount(prev => prev + 1)`). (`05-react/props-state-hooks-intro.md`)

**What a hook is.** A `use`-prefixed function that lets a function component tap React features. Two hard
rules: call them only at the **top level** (never inside loops, conditions, or nested functions) and only
from components or other hooks — React relies on the same call order every render. The three you meet
first: **`useState`** (local memory), **`useEffect`** (side effects after render), **`useContext`** (read
shared data).

**Component communication and one-way data flow.** Parent to child is props. Child to parent is a
**callback passed down that the child invokes** — the parent owns the state and updates it, and the new
value flows back down. Data flows one way (down), events flow up. When two siblings need the same data,
**lift the state up** to their closest common ancestor. (`05-react/state-immutability-lifting.md`)

**The lifecycle.** **Mount** (inserted into the DOM), **update** (re-render on state/prop change),
**unmount** (removed). Function components model all three with `useEffect`. The dependency array
controls re-runs: `[]` = once on mount; `[dep]` = on mount and whenever `dep` changes; **no array** = after
every render (the usual cause of infinite loops when the effect also sets state). The **cleanup return**
runs before the next run and on unmount — that is where timers, subscriptions, and listeners get stopped.
(`05-react/component-lifecycle.md`)

**State immutability.** React decides whether to re-render by comparing state **by reference**
(`Object.is`). Hand it the same array you already had — even after mutating its contents — and it
concludes nothing changed. So: add with `[...books, newBook]`, remove with `filter`, update one item with
`map` returning `{ ...b, available: false }`, and spread **every nested level you touch**. The traps are
`push`/`splice`/`sort` and `Object.assign(state, patch)`, which writes into the existing object and
returns that same reference.

**Lists and keys.** Render with `.map()` — there is no loop directive, it is just JavaScript. Every item
needs a `key` that is **unique among siblings** and **stable across renders**; use a real ID. The array
**index** is a poor key because it describes a *position*, not an *item*: delete the first row and every
index shifts, so React mis-matches elements and per-row DOM state (input text, checkbox state, focus)
sticks to the wrong row. (`05-react/rendering-lists-keys.md`)

**Conditional rendering.** Ternary `cond ? a : b` for two branches; `&&` for show-or-nothing. Guard
numbers — `{count && <Badge/>}` renders a stray `0`, so write `{count > 0 && <Badge/>}`.

**Events and controlled forms.** React wraps native events in a **SyntheticEvent** (same `target`,
`preventDefault`, `stopPropagation`) and delegates from the root. Handlers are camelCase and you pass the
**function**, not a call — `onClick={handle()}` invokes it during render. Wrap in an arrow to pass
arguments. A **controlled input** binds `value={state}` and updates state in `onChange`, so state is the
single source of truth; the contrast is **uncontrolled**, where the DOM holds the value, seeded with
`defaultValue` and read with a ref on demand. `<input type="file">` is always uncontrolled. On submit,
put `onSubmit` on the `<form>` (so Enter works) and call `e.preventDefault()` to stop the page reload.
(`05-react/events-controlled-forms.md`, `controlled-uncontrolled.md`)

**HTTP with Axios or Fetch.** `fetch` is built in but only rejects on network failure (check `res.ok`)
and needs an explicit `.json()`. **Axios** (`npm install axios` — the "leverage an NPM library" row)
parses JSON onto `res.data`, **rejects on HTTP error statuses**, and supports a configured instance plus
interceptors. Create one client with `axios.create({ baseURL })`, import it everywhere, and attach auth in
a **request interceptor** so no call site has to remember the header. Model every request as
**loading / error / data** and render conditionally, clearing loading in `finally`; keep an `active` flag
so a late response cannot set state on an unmounted component. (`05-react/axios-fetch-data.md`)

**Styling.** Three ways, all in production codebases: **inline styles** (a JS object on the `style` prop,
camelCase properties — note the double braces, outer "enter JavaScript" plus inner object literal), **CSS
Modules** (`Something.module.css` imported as an object; the build tool makes every class globally
unique), and an **external/global stylesheet** imported once. Inline for dynamic one-offs, Modules for
component-scoped styles, global for resets and design tokens.

**React Router.** `npm install react-router-dom`. Wrap once in `BrowserRouter`, declare a `Route` per
`path` inside `Routes`, and navigate with `Link`/`NavLink` — **never a plain `<a href>`** for in-app
navigation, which triggers a full reload and discards app state. `path="*"` is the catch-all 404. Read a
dynamic segment (`path="/books/:id"`) with `useParams` (always a string); navigate from code with
`useNavigate`. Nested routes share a layout: the parent renders an `<Outlet />` where the matched child
appears, child paths are relative, and `index` is the default child.
(`05-react/routing-react-router.md`)

**Context.** Solves **prop-drilling** — threading a prop through components that do not use it just to
reach a deep one. Three pieces: `createContext` (its argument is the default, used only when there is no
Provider above), a **Provider component that owns the state** and exposes it through `value`, and
`useContext` to read it. Wrap `useContext` in a custom `useX()` hook that throws when the context is
`null`, so misuse fails loudly. Context is **not** a general state manager: every consumer re-renders on
every value change, so keep it for stable cross-cutting state (auth, theme, locale).
(`05-react/context-global-state.md`)

**useReducer.** A reducer is a pure function `(state, action) => newState`; `useReducer(reducer, initial)`
returns `[state, dispatch]`. Type actions as a **discriminated union** on a literal `type` field so the
`switch` narrows each case and the compiler flags unhandled ones. Reach for it over `useState` when
several fields must change together or the state is really a **machine** with named transitions
(idle/loading/success/error). The reducer must return **new** state — same reference, no re-render. A
useful tell: if you are calling several setters together to keep them in sync, that is a reducer waiting
to be written. (`05-react/useReducer-complex-state.md`)

**Route guards.** A wrapper that renders `<Navigate to="/login" replace />` when the user is not
authenticated (and optionally lacks a role) instead of the protected element. Say the caveat out loud:
hiding an admin button and redirecting away from an admin route are **UX, not security** — both run in
the browser, and the server must authorize every request. (`05-react/client-auth-arc.md`)

**TypeScript in React.** Props become a **compile-time-checked contract** at the call site; you get
autocomplete on every prop and field plus safe renames; and the prop type doubles as documentation. Write
`.tsx`, annotate props (inline for small components, a named `interface` for larger ones), mark optional
props with `?`, restrict values with unions (`"compact" | "full"`), type `children` as `React.ReactNode`
and callbacks as `(id: number) => void`. Vite's `react-ts` template sets it all up.
(`05-react/react-with-typescript.md`)

**Refs, HOCs, containers, testing (Nice tier).** `useRef` returns `{ current }` that persists across
renders and **does not trigger one** — for timer ids, previous values, or a DOM node to command
imperatively (focus, measure, scroll). A **higher-order component** is a function that takes a component
and returns a wrapped one with added behavior (`withAuth(Dashboard)`); a **container** holds logic and
state while a **presentational** component just renders props. Custom hooks now cover most HOC ground more
cleanly. For **testing**, React Testing Library's loop is render, query with `screen` (prefer `getByRole`,
then `getByLabelText`, then `getByText`), interact with `userEvent`, assert with `expect` — testing
behavior, never internal state or class names. `getBy*` throws (presence), `queryBy*` returns null
(absence), `findBy*` awaits (async). RTL's API is identical under Jest and Vitest; only the mock factory
(`jest.fn()` versus `vi.fn()`) and the globals import differ.
(`05-react/hoc-refs.md`, `testing-jest-rtl.md`)

### Key points and pitfalls

- **Mutation is the number-one "my screen didn't update" bug.** `books.push(x); setBooks(books)` does
  nothing, because the reference is unchanged.
- **`useEffect` with no dependency array that sets state** is an infinite loop.
- **Index keys** break on insert, delete, and reorder — and the symptom is per-row state jumping rows,
  not a crash.
- **`onClick={handle()}`** calls the handler during render.
- **`value` without `onChange`** produces a frozen field React warns about.
- **A plain `<a href>` inside an SPA** does a full page reload and throws away your state.
- **Context re-renders every consumer** on every value change — do not put fast-changing state in it.
- **Client-side guards are not security.** Expect the follow-up question; answer it before it is asked.

### Worked example 1 — the data-loading component, end to end

From `demo/react-spa-demo/src/components/CatalogPage.tsx` (rung `07-hooks-axios`):

```tsx
export function CatalogPage() {
    const [items, setItems] = useState<InventoryItem[]>([]);
    const [state, setState] = useState<FetchState>("idle");

    // Search + sort state, owned here (lifted) and shared with the children.
    const [query, setQuery] = useState("");
    const [dir, setDir] = useState<SortDirection>(SortDirection.Ascending);

    // useEffect with an empty dependency array runs once, right after the first
    // render - the mount phase. The function it returns runs on unmount.
    useEffect(() => {
        let active = true;
        setState("loading");
        getInventory()
            .then((data) => {
                if (!active) return;
                setItems(data);
                setState("loaded");
            })
            .catch(() => { if (active) setState("failed"); });
        return () => { active = false; };
    }, []);

    // Derived view: filter + sort build a NEW array every render. We never sort
    // `items` in place - state stays the untouched source of truth.
    const visible = [...items]
        .filter((i) => i.name.toLowerCase().includes(query.toLowerCase()))
        .sort((a, b) => dir === SortDirection.Ascending
            ? a.name.localeCompare(b.name)
            : b.name.localeCompare(a.name));

    // Conditional rendering off the fetch state machine.
    if (state === "idle" || state === "loading") return <p>Loading catalog...</p>;
    if (state === "failed") return <p>Could not reach the API. Is it running on :5137?</p>;

    return (
        <section>
            <SearchBar value={query} onChange={setQuery} />
            {visible.length === 0 ? (
                <p>No books match "{query}".</p>
            ) : (
                <div className="cards">
                    {visible.map((item) => (
                        <BookCard key={item.sku} item={item} />
                    ))}
                </div>
            )}
        </section>
    );
}
```

Count the rubric rows: `useState` and `useEffect` (Must), the **mount** phase and its **cleanup return**
(lifecycle, Must), an HTTP call with success and failure handled (Must), **lifted state** — `query` lives
here and is handed to `SearchBar` as a value plus a callback (Should, and one-way data flow with it),
**immutability** — `[...items]` copies before sorting because `sort` mutates (Must), **conditional
rendering** in both the ternary and the early returns (Should), and **lists with a stable key** from real
data (`item.sku`, not the index) (Must). One component, eight objectives.

### Worked example 2 — auth as a reducer state machine behind Context

From `demo/react-spa-demo/src/auth/authReducer.ts` and `demo/react-spa-demo/src/auth/AuthContext.tsx`
(rung `09-auth-context`):

```ts
export interface AuthState {
    status: "anonymous" | "authenticating" | "authenticated" | "error";
    user: Identity | null;
    error: string | null;
}

export type AuthAction =
    | { type: "login_start" }
    | { type: "login_success"; user: Identity }
    | { type: "login_failure"; error: string }
    | { type: "logout" };

export function authReducer(state: AuthState, action: AuthAction): AuthState {
    switch (action.type) {
        case "login_start":   return { ...state, status: "authenticating", error: null };
        case "login_success": return { status: "authenticated", user: action.user, error: null };
        case "login_failure": return { status: "error", user: null, error: action.error };
        case "logout":        return { ...initialAuthState };
    }
}
```

```tsx
export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [state, dispatch] = useReducer(authReducer, undefined, initAuthState);

    async function login(username: string, password: string): Promise<boolean> {
        dispatch({ type: "login_start" });
        try {
            const token = await loginRequest(username, password);
            const user = decodeToken(token);
            if (!user) throw new Error("token missing expected claims");
            setToken(token);
            dispatch({ type: "login_success", user });
            return true;
        } catch {
            dispatch({ type: "login_failure", error: "Invalid username or password." });
            return false;
        }
    }

    return (
        <AuthContext.Provider value={{ ...state, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
}
```

This is the Should-know reducer row and the Context rows in one artifact. `AuthState` makes impossible
states unrepresentable (a `status` literal union), `AuthAction` is a **discriminated union** so the
`switch` narrows `action.user` only in the `login_success` arm, and every case returns a **new** object.
The Provider owns the state and publishes both data and behavior (`login`, `logout`) through `value`, so
any component calls `useAuth()` instead of receiving drilled props. Consumers then gate on it — in
`App.tsx`, `{user?.role === "admin" && <NavLink to="/admin">Admin</NavLink>}` is role-gated **UI** and
`<Route path="/account" element={<RequireAuth><AccountPage /></RequireAuth>} />` is the role-gated
**route**. Both are UX; the API still authorizes every call.

### Worked example 3 — the token attached in exactly one place

From `demo/react-spa-demo/src/api/client.ts`:

```ts
export const api = axios.create({
    baseURL: "http://localhost:5137",
});

// Request interceptor: attach the bearer token to EVERY call from this ONE place.
// Because Authorization is not a "simple" header, adding it makes the browser send
// a CORS preflight (OPTIONS) before the real request - the API must answer that.
api.interceptors.request.use((config) => {
    const token = getToken();
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
});
```

Small file, three exam-relevant facts: one configured Axios **instance** so the base URL lives in one
place; a **request interceptor** as the single seam auth hangs on (no call site can forget the header);
and the reason the first authenticated request in a new project fails with a CORS error rather than a
401 — the custom `Authorization` header makes the cross-origin request non-simple, so the browser sends
an `OPTIONS` **preflight** the server's CORS policy must explicitly allow.
