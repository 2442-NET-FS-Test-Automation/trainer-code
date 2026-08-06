# QC-5 (Front End) — Drills

Short hands-on tasks. **Do them in your own domain** — the prompts are deliberately domain-neutral, so
pick your Project 2 entities (orders, tickets, recipes, whatever your team chose) and build there. The
model solutions use the trainer **Library** domain; if yours looks different but does the same thing, you
are right.

Write the code before you look. A drill you read is worth nothing.

---

## HTML

### Drill H1 — Build a page skeleton from nothing

**Task.** From an empty file, write a complete HTML document for a list page in your domain: doctype,
language, character set, viewport, a title, an external stylesheet, semantic page regions (a header with
a nav, a main with a section, a footer), a search field with a proper label, and an external script wired
so the DOM is guaranteed to exist when it runs. No CSS, no JavaScript yet.

<details>
<summary>Model solution</summary>

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Library Catalog</title>
    <link rel="stylesheet" href="css/styles.css">
</head>
<body>
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
            <div class="toolbar">
                <label for="search">Search:</label>
                <input type="search" id="search" name="search" placeholder="title or SKU">
            </div>
            <div class="cards" id="catalog-cards"></div>
        </section>
    </main>

    <footer class="site-footer">
        <p>City Library</p>
    </footer>

    <script src="js/app.js" defer></script>
</body>
</html>
```

The two graded details: the stylesheet `<link>` is in the `<head>` (styles known before first paint) and
the `<script>` is at the end of the body with `defer` (the DOM exists when it runs). The `for`/`id` pair
is the label association.

`QC: Describe the structure of an HTML document...` / `List common HTML tags...` /
`Describe how/where you link an external CSS sheet...` / `...external JS file...`
`Source: demo/frontend-demo/index.html`
</details>

### Drill H2 — A form that actually submits what you think

**Task.** Build a registration form for your domain with: an email field, a password field, a
three-option exclusive choice (radio buttons), a single opt-in checkbox, a dropdown, and a multi-line
notes field. Every control must be labelled and must actually reach the server. Use the correct method
for a form that creates something, and add browser-side validation on at least two fields.

<details>
<summary>Model solution</summary>

```html
<form action="/members/register" method="post">
  <label for="email">Email</label>
  <input type="email" id="email" name="email" required>

  <label for="password">Password</label>
  <input type="password" id="password" name="password" minlength="8" required>

  <fieldset>
    <legend>Membership tier</legend>
    <label><input type="radio" name="tier" value="basic" checked> Basic</label>
    <label><input type="radio" name="tier" value="standard"> Standard</label>
    <label><input type="radio" name="tier" value="premium"> Premium</label>
  </fieldset>

  <label><input type="checkbox" name="newsletter" value="yes"> Email me the newsletter</label>

  <label for="branch">Home branch</label>
  <select id="branch" name="branch">
    <option value="north">North Branch</option>
    <option value="south">South Branch</option>
  </select>

  <label for="notes">Notes</label>
  <textarea id="notes" name="notes" rows="4"></textarea>

  <button type="submit">Register</button>
</form>
```

Every control has a `name` — without it nothing is submitted, however good the markup looks. The three
radios are exclusive because they **share** `name="tier"`. `method="post"` because this creates
something and carries a password. And say the caveat out loud: `required` and `minlength` are UX only;
the server revalidates.

`QC: Construct an HTML form.` / `Take in user input using a variety of input tags (text, checkbox,
etc).`
`Source: content/01-html/forms-inputs.md`
</details>

---

## CSS

### Drill C1 — Predict the winner

**Task.** Write four rules that target the same element — one by tag, one by class, one by a second
class declared later, and one compounding an id with a class. Predict which colour renders, then check
in the browser. Then add an inline style and predict again.

<details>
<summary>Model solution</summary>

```html
<div id="catalog"><p class="card featured">Clean Code</p></div>
```

```css
p              { color: black; }   /* (0, 0, 1) */
.card          { color: navy;  }   /* (0, 1, 0) */
.featured      { color: teal;  }   /* (0, 1, 0) — ties .card, declared later, so it beats it */
#catalog .card { color: green; }   /* (1, 1, 0) — the id outranks every class rule */
```

Renders **green**. Add `style="color: crimson"` to the paragraph and it renders **crimson** — inline
beats all stylesheet rules. Add `!important` to the `.card` declaration and that wins over even the
inline style, which is exactly why you avoid it.

`QC: Describe CSS priority in regards to inline, internal, and external styles.` / `Use the correct
syntax for styling different elements such as by tag, class, id, etc.`
`Source: content/02-css/css-fundamentals-selectors.md`
</details>

### Drill C2 — Make the box behave

**Task.** Style a card in your domain to be exactly 300 pixels wide on screen while carrying 20px of
padding and a 5px border. Do it twice: once by doing the arithmetic yourself under the default box
model, and once by changing the box model. Then give the card a hover lift that does not cause layout
work.

<details>
<summary>Model solution</summary>

```css
/* 1. content-box arithmetic: 300 - 40 padding - 10 border = 250 content */
.card { width: 250px; padding: 20px; border: 5px solid #ddd; }

/* 2. border-box: width now means the whole visible box */
*, *::before, *::after { box-sizing: border-box; }
.card { width: 300px; padding: 20px; border: 5px solid #ddd; }

/* Hover lift: transform + opacity composite on the GPU; animating top/margin
   would recalculate layout every frame. */
.card { transition: transform 200ms ease-out, box-shadow 200ms ease-out; }
.card:hover { transform: translateY(-4px); box-shadow: 0 8px 20px rgba(0,0,0,.25); }
```

Version 2 is what real codebases do — the reset costs one rule and removes a whole bug class.

`QC: Explain the CSS box model.` / `Make responsive webpages using CSS.` (motion depth)
`Source: content/02-css/box-model-properties.md`, `responsive-variables-animations.md`
</details>

### Drill C3 — Three-breakpoint responsive grid

**Task.** Lay out your list page's cards one per row on a phone, two on a tablet, and four on a wide
screen — mobile-first, with the breakpoints you consider sensible. Then state what one missing line in
the HTML would make all of it silently fail on a real phone.

<details>
<summary>Model solution</summary>

```css
.cards { display: grid; grid-template-columns: 1fr; gap: 1rem; }

@media (min-width: 600px)  { .cards { grid-template-columns: 1fr 1fr; } }
@media (min-width: 1024px) { .cards { grid-template-columns: repeat(4, 1fr); } }
```

The missing line is the viewport meta tag in the `<head>`:

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0">
```

Without it a phone lays the page out at a fake ~980px desktop width and shrinks the result, so no
`min-width` query ever fires — the classic "works in dev tools, broken on the actual device" symptom.

`QC: Make responsive webpages using CSS.`
`Source: content/02-css/responsive-variables-animations.md`
</details>

---

## JavaScript language

### Drill J1 — The coercion and truthiness quiz

**Task.** Without running anything, predict the value and the type of each expression. Then run them.

```js
"5" + 1
"5" - 1
"5" == 5
"5" === 5
Boolean([])
Boolean("0")
0 || "fallback"
0 ?? "fallback"
typeof null
typeof [1, 2]
NaN === NaN
```

<details>
<summary>Model solution</summary>

`"51"` (string — `+` concatenates when either side is a string) - `4` (number — every other arithmetic
operator forces numbers) - `true` (`==` coerces) - `false` (`===` compares type too) - `true` (an empty
array is truthy) - `true` (the *string* `"0"` is truthy; only the number `0` is falsy) - `"fallback"`
(`0` is falsy, so `||` replaces a legitimate zero) - `0` (`??` falls back only on null/undefined) -
`"object"` (historic bug) - `"object"` (use `Array.isArray`) - `false` (`NaN` is the only value not equal
to itself; test with `Number.isNaN`).

The six falsy values, for the exam: `false`, `0`, `""`, `null`, `undefined`, `NaN`.

`QC: Describe what type coercion is.` / `Describe what truthy/falsy is.` / `Explain the different data
types in JS.`
`Source: content/03-javascript/variables-scope-types-coercion.md`
</details>

### Drill J2 — Transform a collection four ways

**Task.** Start from an array of records in your domain, each with an id, a name, a numeric quantity,
and a boolean flag. Without mutating the original array, produce: (1) the names of every record whose
flag is true, (2) the total quantity, (3) the single record with a given id, and (4) the records sorted
by quantity, highest first. Then say which of your four steps would have mutated the array if you had
been careless.

<details>
<summary>Model solution</summary>

```js
const books = [
  { id: 1, title: "Clean Code",              copies: 5, available: true  },
  { id: 2, title: "The Pragmatic Programmer", copies: 3, available: false },
  { id: 3, title: "Refactoring",             copies: 8, available: true  },
];

const availableTitles = books.filter(b => b.available).map(b => b.title);
const totalCopies     = books.reduce((sum, b) => sum + b.copies, 0);
const one             = books.find(b => b.id === 2);
const byCopiesDesc    = [...books].sort((a, b) => b.copies - a.copies);
```

Step 4 is the trap: `sort` **mutates** and returns the same array, so without the `[...books]` copy you
would have reordered your source data. It also compares as strings by default, so the comparator is not
optional — `[10, 2].sort()` gives `[10, 2]`.

`QC: Describe the different array methods and how to use them.` / `Create arrays in JS.`
`Source: content/03-javascript/objects-arrays-loops.md`
</details>

### Drill J3 — Loop it four ways, then break

**Task.** Print every item in your collection using a classic `for`, `for...of`, and `forEach`. Then
print the object's *keys* with the right loop. Finally, stop at the first item whose quantity is zero —
and say which of your loops cannot do that.

<details>
<summary>Model solution</summary>

```js
for (let i = 0; i < books.length; i++) console.log(books[i].title);   // index math available
for (const b of books) console.log(b.title);                          // VALUES - default for arrays
books.forEach((b, i) => console.log(i, b.title));                     // callback per item

for (const key in books[0]) console.log(key);                         // KEYS - objects, not arrays

for (const b of books) {
  if (b.copies === 0) { console.log("out of stock:", b.title); break; }
}
```

`forEach` cannot `break` — that is the whole answer to the last part. `for...in` on an array yields
string indexes plus inherited enumerable keys, which is why it belongs on objects.

`QC: Loop through arrays.`
`Source: content/03-javascript/objects-arrays-loops.md`
</details>

### Drill J4 — A closure with private state

**Task.** Write a factory function that returns an object with two functions: one that records an event
in your domain and one that reports how many have been recorded. The count must be unreachable from
outside. Prove two instances do not interfere.

<details>
<summary>Model solution</summary>

```js
function makeLoanCounter() {
  let count = 0;                       // private: only the closures below can see it
  return {
    recordLoan: () => ++count,
    total:      () => count,
  };
}

const branchA = makeLoanCounter();
const branchB = makeLoanCounter();
branchA.recordLoan(); branchA.recordLoan();
branchB.recordLoan();
branchA.total();   // 2
branchB.total();   // 1  - independent scopes
branchA.count;     // undefined - nothing outside can touch it
```

Each call to the factory creates a fresh scope, and the returned arrows **close over** that scope, so it
survives after the factory returned. That is the definition, and the two-instance check is the proof.

`QC: Describe and explain a closure.` / `Describe the different types of functions in JS.`
`Source: content/03-javascript/functions-this-closures.md`
</details>

### Drill J5 — Fix the detached method

**Task.** Write an object with a counter and an `increment` method. Assign the method to a bare variable
and call it; observe what breaks in strict mode. Then fix it two different ways. Finally explain why
rewriting `increment` as an arrow function would be the wrong fix.

<details>
<summary>Model solution</summary>

```js
"use strict";
const counter = { count: 0, increment() { this.count++; } };

const detached = counter.increment;
detached();                       // TypeError - `this` is undefined in a strict plain call

const bound = counter.increment.bind(counter);   // fix 1: permanently bound copy
bound();

setTimeout(() => counter.increment(), 0);        // fix 2: arrow wrapper preserves the call shape
```

Why the arrow-as-method is wrong: an arrow has no `this` of its own and takes it lexically from the
surrounding scope, which for an object literal is *not* the object — so `this.count` would miss
entirely. `this` is decided at the **call site**, not by where the function was written.

`QC: Describe what the this keyword is.` / `Define arrow functions and explain the benefits...`
`Source: content/03-javascript/functions-this-closures.md`
</details>

### Drill J6 — Errors that survive the catch

**Task.** Write a parsing function for your domain that throws a `TypeError` for the wrong input type and
a plain `Error` for unparseable input. Call it inside a `try`/`catch`/`finally` that always hides a
spinner. Then add a custom error class and catch that one specifically while letting everything else
propagate.

<details>
<summary>Model solution</summary>

```js
class NotFoundError extends Error {
  constructor(resource, id) {
    super(`${resource} ${id} not found`);
    this.name = "NotFoundError";
  }
}

function parsePrice(raw) {
  if (typeof raw !== "string") throw new TypeError("raw must be a string");
  const price = Number(raw);
  if (Number.isNaN(price)) throw new Error(`not a price: "${raw}"`);
  return price;
}

const spinner = show("loading");
try {
  render(parsePrice(input));
} catch (e) {
  if (e instanceof NotFoundError) renderEmptyState();
  else if (e instanceof TypeError) renderBadInput(e.message);
  else throw e;                       // not mine to handle - let it propagate
} finally {
  hide(spinner);                      // runs on BOTH paths, even past a return
}
```

Two graded points: `finally` is where cleanup lives because it runs whether the try completed or threw;
and the `else throw e` encodes the placement rule — catch where you can act, propagate otherwise. Never
`throw "a string"`: no stack, no `name`, and `e.message` is undefined for whoever catches it.

`QC: Handle errors in JS.`
`Source: content/03-javascript/error-handling.md`
</details>

---

## Browser JavaScript

### Drill B1 — Render a collection safely

**Task.** Given an array of records, render one card per record into a container element. Use the
granular insertion API (create the node, set its text, attach it). Then render one field that comes from
untrusted user input and say which property you must use for it and why.

<details>
<summary>Model solution</summary>

```js
const books = [{ title: "Dune", author: "Herbert" }, { title: "Emma", author: "Austen" }];
const list = document.querySelector("#book-list");

for (const book of books) {
  const li = document.createElement("li");
  li.textContent = `${book.title} - ${book.author}`;   // data via textContent: XSS-safe
  li.classList.add("book-row");
  list.append(li);
}
```

Untrusted input goes in with **`textContent`**, never `innerHTML`. `innerHTML` parses its input as
markup, so a review body containing `<img src=x onerror=...>` would execute — a cross-site scripting
hole. `innerHTML` is for markup you wrote. Bonus point: building all the nodes first and attaching once
(or via a `DocumentFragment`) avoids a reflow per iteration.

`QC: Insert new elements into the DOM.` / `Query the DOM for elements.` / `Use template literals.`
`Source: content/03-javascript/dom-selection-manipulation.md`
</details>

### Drill B2 — One listener for a thousand rows

**Task.** Wire a click handler for a list whose rows are rendered from data and can grow later. Do it
with **one** listener total. Then wire a live filter on a search field that runs on every keystroke.
State which event you used for the field and why not the other one.

<details>
<summary>Model solution</summary>

```js
// Delegation: one listener on the container; clicks bubble up from any row, present or future.
document.querySelector("#book-list").addEventListener("click", (event) => {
  const row = event.target.closest("li.book-row");
  if (!row) return;                       // click landed in the list but not on a row
  row.classList.toggle("selected");
});

// input fires on every keystroke; change would wait for the value to be committed (blur).
document.querySelector("#search").addEventListener("input", (e) => {
  const q = e.target.value.trim().toLowerCase();
  renderCards(catalogItems.filter(i => i.name.toLowerCase().includes(q)));
});
```

Delegation wins twice: one listener instead of N (less memory, nothing to rebind), and rows appended
later are handled automatically because their clicks bubble to the same container. `event.target` is
where the click originated; `event.currentTarget` would be the list itself.

`QC: Describe what event listeners are.` / `Describe what bubbling and capturing are...` /
`Describe some methods on the event object...`
`Source: content/03-javascript/events.md`; `demo/frontend-demo/js/app.js`
</details>

### Drill B3 — Fetch with both failure lanes

**Task.** Write an async function that loads a collection from an API, renders it, and handles **both**
kinds of failure distinctly: the server refusing (an HTTP error status) and the server being unreachable.
Show a loading state first. Then answer: does a 404 reject the promise?

<details>
<summary>Model solution</summary>

```js
async function loadCatalog() {
  const container = document.querySelector("#catalog-cards");
  container.textContent = "loading...";

  try {
    const res = await fetch(`${API}/api/Inventory`);
    if (!res.ok) {                                   // lane 1: the server REFUSED
      container.textContent = `API said ${res.status}`;
      return;
    }
    const items = await res.json();                  // second await: the parsed body
    renderCards(items);
  } catch (err) {                                    // lane 2: the server was UNREACHABLE
    console.error(err);                              // keep the real error - do not hide the evidence
    container.textContent = "cannot reach the API - is it running?";
  }
}
```

No, a 404 does **not** reject. Fetch rejects only on network failure — DNS, connection refused, a CORS
block. A 4xx or 5xx is a successful conversation whose answer was "no," so it resolves and the status
check is your job.

`QC: Handle a failed request when using the Fetch API.` / `Describe what type of object the Fetch API
returns.` / `List the steps to sending an HTTP request using the Fetch API.`
`Source: content/03-javascript/fetch-json-http.md`; `demo/frontend-demo/js/app.js`
</details>

### Drill B4 — POST JSON, then round-trip it

**Task.** Send a POST that creates a record in your domain with a JSON body and the right header. Then,
separately, take an object that contains a method and an `undefined` property, stringify it, parse it
back, and report exactly what survived.

<details>
<summary>Model solution</summary>

```js
await fetch(`${API}/api/Inventory`, {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ sku: "BK-104", name: "Emma", currentStock: 4 }),
});
```

```js
const book = { title: "Dune", pages: 412, format: undefined, describe() {} };
const text  = JSON.stringify(book);   // '{"title":"Dune","pages":412}'
const clone = JSON.parse(text);       // describe() and format are GONE
```

`stringify` silently drops functions and `undefined`, so the round trip is a **lossy** clone for anything
richer than plain data. Also note fetch does not serialize for you — you set `Content-Type` and stringify
the body yourself.

`QC: Explain what JSON is.` / `Explain what JSON.stringify() and JSON.parse() are.` / `List the steps to
sending an HTTP request using the Fetch API.`
`Source: content/03-javascript/fetch-json-http.md`
</details>

### Drill B5 — Sequential versus parallel, and the output order

**Task.** You need two independent pieces of data plus one that depends on the first. Write it so the
independent calls do not queue behind each other. Then predict the log order of: a sync log, a
`setTimeout(..., 0)`, a resolved `.then`, and a second sync log.

<details>
<summary>Model solution</summary>

```js
// Dependent: the author id comes FROM the book, so this must be sequential.
const book = await fetchBook(1);
const author = await fetchAuthor(book.authorId);

// Independent: start them together - total time becomes max(A, B), not A + B.
const [books, members] = await Promise.all([fetchBooks(), fetchMembers()]);
```

```js
console.log("one");
setTimeout(() => console.log("four"), 0);
Promise.resolve().then(() => console.log("three"));
console.log("two");
// one, two, three, four
```

Synchronous code runs to completion, then **microtasks** (promise callbacks) drain, then the next
**macrotask** (the timer) — even at zero milliseconds. Serial awaits over independent calls is the most
common self-inflicted latency bug in async code.

`QC: Explain the difference between synchronous and asynchronous programming.` / `Describe the different
promise methods.` / `Describe and explain the event loop.` / `Explain how to chain multiple asynchronous
operations...`
`Source: content/03-javascript/promises-async.md`
</details>

---

## TypeScript

### Drill T1 — Standalone project from an empty folder

**Task.** With no framework and no bundler, take an empty folder to "running compiled TypeScript in
node." Write one `.ts` file with a typed function, compile the project, and run the output. Then
introduce a deliberate type error and report what happens to the emitted JavaScript.

<details>
<summary>Model solution</summary>

```
mkdir price-tool && cd price-tool
npm init -y
npm install --save-dev typescript
npx tsc --init            # generates tsconfig.json
npx tsc                   # config-driven: compiles the whole project
node dist/app.js
```

```ts
// src/app.ts
export function lateFee(daysLate: number, dailyRate: number): number {
  return daysLate * dailyRate;
}
console.log(lateFee(3, 0.25));
```

Introduce `lateFee("3", 0.25)` and `tsc` reports the error **and still writes the JavaScript** — type
errors do not block emit by default. Set `"noEmitOnError": true` in the config to make them block. If you
set `outDir` without `rootDir` you will meet error TS5011; add `"rootDir": "./src"`.

`QC: Describe and demonstrate the process to transpile and run TypeScript.` / `Implement TypeScript
outside of Angular/React environments using plain .ts files.`
`Source: content/04-typescript/why-typescript-tooling.md`, `tsconfig.md`
</details>

### Drill T2 — Model a domain shape and a closed value set

**Task.** Define a named type for one entity in your domain with a required id, a required name, an
optional field, and a field that cannot be reassigned after construction. Then define a status field
restricted to exactly three values, and try to assign a fourth. Say which of `interface` and `type` you
used and why either would or would not work.

<details>
<summary>Model solution</summary>

```ts
interface InventoryItem {
  readonly sku: string;          // set once, never reassigned
  name: string;
  currentStock: number;
  supplierNote?: string;         // optional - reads as string | undefined
}

type OrderStatus = "pending" | "shipped" | "delivered";
let status: OrderStatus = "shipped";
let bad: OrderStatus = "returned";   // Error: not assignable to type 'OrderStatus'
```

The entity shape works as either an `interface` or a `type` — same checking, same erasure. The status
line does **not**: an interface cannot express a union, only object or function shapes, so a `type` alias
is the only tool. The practical default is interface for public object contracts, type for everything
else.

`QC: Implement user defined types in TypeScript.` / `Describe and implement union types.` / `Describe
and implement type aliasing.`
`Source: content/04-typescript/aliases-interfaces-unions.md`
</details>

### Drill T3 — Guard it, do not assert it

**Task.** Write a function that takes an `unknown` value from an API and safely narrows it to your error
shape before use, as a reusable guard. Then write the same thing as an assertion, and state exactly what
the assertion version risks.

<details>
<summary>Model solution</summary>

```ts
export interface ApiError { status: number; message: string; }

// Type predicate: a true return narrows the argument at the call site.
export function isApiError(value: unknown): value is ApiError {
  if (typeof value !== "object") return false;
  if (value === null) return false;              // typeof null === "object" - exclude explicitly
  if (!("status" in value)) return false;
  if (!("message" in value)) return false;
  return true;
}

// Usage
const result: unknown = await getSomething();
if (isApiError(result)) console.log(result.status);   // narrowed - safe
```

```ts
const risky = result as ApiError;   // the assertion version
console.log(risky.status);          // compiles clean; crashes at runtime if the shape differs
```

The assertion converts **nothing** at runtime — it only changes the compiler's belief. Assertion says
"trust me now" and moves the risk to runtime; a guard proves it at runtime and the compiler rewards the
proof with narrowing. Default to guards at data boundaries.

`QC: Describe and implement type guards.` / `Describe and implement casting in TypeScript.`
`Source: content/04-typescript/casting-guards-asconst.md`; `demo/frontend-demo/ts/ts-client.ts`
</details>

### Drill T4 — A generic that keeps the type

**Task.** Write a function that fetches JSON and returns either the caller's expected type or your error
shape. Call it twice with two different entity types and show that the results stay typed. Then write the
`any` version and state what the caller loses.

<details>
<summary>Model solution</summary>

```ts
export class ApiClient {
  constructor(private readonly baseUrl: string = "http://localhost:5137") {}

  async getJson<T>(path: string): Promise<T | ApiError> {
    try {
      const res = await fetch(`${this.baseUrl}${path}`);
      if (!res.ok) return { status: res.status, message: `API said: ${res.status}` };
      return await res.json() as T;
    } catch (err) {
      console.log(err instanceof Error ? err.message : "unknown error");
      return { status: 0, message: "Cannot reach the API. Check if it's on, or CORS" };
    }
  }
}

const client = new ApiClient();
const items = await client.getJson<InventoryItem[]>("/api/Inventory");
const one   = await client.getJson<InventoryItem>("/api/Inventory/BK-101");
if (!isApiError(items)) items.forEach(i => console.log(i.name));   // narrow, then use
```

With `getJson(path): Promise<any>` the caller loses everything downstream: no autocomplete, no checking,
and every value derived from the result is also `any`. A generic preserves the connection between what
the caller asked for and what they get back. Note the `constructor(private readonly baseUrl: string =
...)` — a parameter property declares and assigns the field in one line.

`QC: Describe and leverage generic types.`
`Source: content/04-typescript/classes-generics-functions.md`; `demo/frontend-demo/ts/ts-client.ts`
</details>

---

## React

### Drill R1 — Scaffold, then compose

**Task.** Create and run a new React app with the TypeScript template. Then replace the starter content
with a three-level component tree for your domain — a page containing a list containing a row — where
the row takes typed props including one optional prop with a default.

<details>
<summary>Model solution</summary>

```
npm create vite@latest catalog-app -- --template react-ts
cd catalog-app
npm install
npm run dev            # http://localhost:5173
```

```tsx
interface BookCardProps {
  item: InventoryItem;
  compact?: boolean;              // optional
}

export function BookCard({ item, compact = false }: BookCardProps) {
  return (
    <article className="card">
      <h3>{item.name}</h3>
      <dl>
        <dt>SKU</dt><dd>{item.sku}</dd>
        {!compact && (<><dt>In stock</dt><dd>{item.currentStock}</dd></>)}
      </dl>
    </article>
  );
}

function BookList({ items }: { items: InventoryItem[] }) {
  return <div className="cards">{items.map(i => <BookCard key={i.sku} item={i} />)}</div>;
}

function CatalogPage({ items }: { items: InventoryItem[] }) {
  return <section><h2>Catalog</h2><BookList items={items} /></section>;
}
```

The lone `--` in the create command separates npm's arguments from the ones passed to the Vite
scaffolder. The default value for `compact` goes in the destructuring, and the Fragment `<>...</>` lets
you return siblings without an extra wrapper node.

`QC: Create and run a React application using Vite CLI.` / `Describe and implement functional components
in React.` / `Build and use nested component structures...` / `Build a reusable component using TSX with
type-checked props.`
`Source: demo/react-spa-demo/src/components/BookCard.tsx`
</details>

### Drill R2 — Load data on mount, with all three states

**Task.** Fetch a collection from an API when the component mounts, and render three distinct UIs:
loading, error, and the data. Make sure a response arriving after the user navigates away cannot set
state. Do it once with Axios and once with fetch, and note the difference in failure handling.

<details>
<summary>Model solution</summary>

```tsx
function BookList() {
  const [books, setBooks] = useState<Book[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;                          // guard against setting state after unmount

    async function load() {
      setIsLoading(true);
      setError(null);
      try {
        const res = await api.get<Book[]>("/books");   // Axios: parsed body on res.data
        if (active) setBooks(res.data);
      } catch {
        if (active) setError("Could not load books.");
      } finally {
        if (active) setIsLoading(false);
      }
    }

    load();
    return () => { active = false; };           // cleanup runs on unmount
  }, []);                                       // empty deps: mount only

  if (isLoading) return <p>Loading...</p>;
  if (error) return <p role="alert">{error}</p>;
  return <ul>{books.map(b => <li key={b.id}>{b.title}</li>)}</ul>;
}
```

With **fetch** the `try` needs an explicit `if (!res.ok) throw new Error(...)`, because a 404 or 500
resolves normally; **Axios** rejects on HTTP error statuses so those land in `catch` for free, and parses
the JSON onto `res.data`. Either way, clear the loading flag in `finally` so it clears on both paths.

`QC: Make HTTP requests using Axios or Fetch and handle the response.` / `Explain the lifecycle of a
React component.` / `Utilize and explain common React hooks...` / `Conditionally render a component...`
`Source: content/05-react/axios-fetch-data.md`; `demo/react-spa-demo/src/components/CatalogPage.tsx`
</details>

### Drill R3 — Break the render, then fix it

**Task.** Write an "add item" handler the wrong way — mutating the state array in place — and confirm
the UI does not update. Then fix it. Then write the immutable versions of remove-by-id, update-one-field,
and a nested object update.

<details>
<summary>Model solution</summary>

```tsx
// BROKEN: mutates the existing array, passes back the SAME reference
function addBroken(newBook: Book) {
  books.push(newBook);
  setBooks(books);              // React's Object.is check sees no change -> no re-render
}

// CORRECT
setBooks([...books, newBook]);                                        // add
setBooks(books.filter(b => b.id !== targetId));                       // remove
setBooks(books.map(b => b.id === targetId ? { ...b, available: false } : b));  // update one
setProfile({ ...profile, address: { ...profile.address, city: "Oxford" } });   // nested
setBooks(prev => [...prev, newBook]);                                 // functional updater
```

React decides whether to re-render by comparing the new state to the old **by reference**. The update
case does two immutable copies at once: `map` makes a new array and the spread makes a new object for the
one row that changed, so unchanged rows keep their old references and can be skipped. Watch for
`Object.assign(state, patch)` — it writes into the existing object and returns that same reference.

`QC: Explain and apply the principles of state immutability in React.`
`Source: content/05-react/state-immutability-lifting.md`
</details>

### Drill R4 — Lift the state

**Task.** Build a search field and a results list as **two sibling components** that must stay in sync.
Decide where the query lives, and wire both children. Neither child may hold the query in its own state.

<details>
<summary>Model solution</summary>

```tsx
function Library() {
  const [query, setQuery] = useState("");                 // the closest common ancestor owns it
  const [books] = useState<Book[]>(initialBooks);

  const visible = books.filter(b => b.title.toLowerCase().includes(query.toLowerCase()));

  return (
    <div>
      <SearchBox query={query} onQueryChange={setQuery} />
      <BookList books={visible} />
    </div>
  );
}

function SearchBox({ query, onQueryChange }: { query: string; onQueryChange: (n: string) => void }) {
  return <input value={query} onChange={e => onQueryChange(e.target.value)} />;
}

function BookList({ books }: { books: Book[] }) {
  return <ul>{books.map(b => <li key={b.id}>{b.title}</li>)}</ul>;
}
```

Data flows **down** as props (`query`, `books`); change requests flow **up** through the callback
(`onQueryChange`). Lift to the *closest* common ancestor — no higher — so the fewest components re-render
and the data stays as local as it can be.

`QC: Lift state up to a parent component to share data between child components.` / `Describe how
one-way data flow works in React.` / `Implement component communication through props and callbacks...`
`Source: content/05-react/state-immutability-lifting.md`
</details>

### Drill R5 — A controlled multi-field form

**Task.** Build a create form for your domain with three text fields and one checkbox, held in **one**
state object with **one** change handler. Submit without reloading the page, validate one field before
submitting, and reset the form afterwards. Then convert one field to uncontrolled and note both prop
changes.

<details>
<summary>Model solution</summary>

```tsx
interface BookForm { title: string; author: string; sku: string; available: boolean; }

function AddBook() {
  const [form, setForm] = useState<BookForm>({ title: "", author: "", sku: "", available: true });

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const { name, value, type, checked } = e.target;
    setForm(prev => ({ ...prev, [name]: type === "checkbox" ? checked : value }));
  }

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();                       // stop the full-page reload
    if (!form.title.trim()) return;           // validate off state
    console.log("submitting", form);
    setForm({ title: "", author: "", sku: "", available: true });   // reset = reset the state
  }

  return (
    <form onSubmit={handleSubmit}>
      <input name="title"  value={form.title}  onChange={handleChange} />
      <input name="author" value={form.author} onChange={handleChange} />
      <input name="sku"    value={form.sku}    onChange={handleChange} />
      <label>
        <input name="available" type="checkbox" checked={form.available} onChange={handleChange} />
        Available
      </label>
      <button type="submit">Add</button>
    </form>
  );
}
```

The computed key `[name]` is what lets one handler serve every field; checkboxes are controlled with
`checked`, not `value`, which is why the handler branches on `type`. `onSubmit` goes on the **form** so
the Enter key works.

Uncontrolled version of one field: swap `value` for `defaultValue`, drop `onChange`, add
`ref={inputRef}`, and read `inputRef.current?.value` in the submit handler. Never set both `value` and
`defaultValue` on the same element, and never flip a live input between the modes.

`QC: Handle user input through form elements and manage form state.` / `Compare and implement controlled
vs uncontrolled components in form handling.`
`Source: content/05-react/events-controlled-forms.md`, `controlled-uncontrolled.md`
</details>

### Drill R6 — Route it, including a detail page and a 404

**Task.** Add routing to your app: a list route, a detail route that takes an id in the URL, an about
page, and a catch-all. Navigate to the detail page from the list. Then navigate to the list
programmatically after a form submits.

<details>
<summary>Model solution</summary>

```tsx
<BrowserRouter>
  <nav>
    <Link to="/">Catalog</Link>
    <Link to="/about">About</Link>
  </nav>
  <Routes>
    <Route path="/" element={<CatalogPage />} />
    <Route path="/inventory/:sku" element={<BookDetail />} />
    <Route path="/about" element={<About />} />
    <Route path="*" element={<p>Page not found.</p>} />
  </Routes>
</BrowserRouter>
```

```tsx
function BookDetail() {
  const { sku } = useParams();            // always a string
  return <h2>Showing {sku}</h2>;
}

function AddBook() {
  const navigate = useNavigate();
  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    // ...save...
    navigate("/");                        // navigate(-1) goes back one entry
  }
  return <form onSubmit={handleSubmit}>{/* fields */}</form>;
}
```

Link from the list with `<Link to={`/inventory/${item.sku}`}>` — never a plain `<a href>`, which forces a
full page reload and discards the app's state.

`QC: Use React Router to implement navigation in a single-page application.` / `Route users between
components through the use of BrowserRouter.` / `Leverage advanced routing techniques...`
`Source: content/05-react/routing-react-router.md`; `demo/react-spa-demo/src/App.tsx`
</details>

### Drill R7 — A reducer state machine behind Context

**Task.** Model a process in your domain that has four states — idle, in-flight, succeeded, failed — with
`useReducer` and a discriminated union of actions. Put it behind a Context Provider so any component can
read it, and expose a custom hook that throws when used outside the Provider. Then gate one piece of UI
on the state.

<details>
<summary>Model solution</summary>

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
const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(authReducer, initialAuthState);
  // ...login / logout dispatch actions...
  return <AuthContext.Provider value={{ ...state, login, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}

// Gated UI, anywhere in the tree, with no prop-drilling:
const { status, user } = useAuth();
{user?.role === "admin" && <NavLink to="/admin">Admin</NavLink>}
```

Three graded points: the status literal union makes impossible states unrepresentable; the discriminated
union means the `switch` narrows `action.user` only in the success arm; and every case returns a **new**
object, because a reducer that mutates keeps the same reference and the UI silently fails to update.

`QC: Use useReducer for complex state management scenarios.` / `Use a Reducer to manage a set of complex
known states.` / `Use Context.Provider tags to wrap components...` / `Use createContext and
Context.Provider to manage global state.`
`Source: demo/react-spa-demo/src/auth/authReducer.ts`, `demo/react-spa-demo/src/auth/AuthContext.tsx`
</details>

### Drill R8 — Guard a route, then say why it is not security

**Task.** Protect one route so unauthenticated users are redirected to a login page, and a second route
so only one role may enter. Then write one sentence explaining to a reviewer why this is not a security
control.

<details>
<summary>Model solution</summary>

```tsx
function RequireAuth({ children, role }: { children: ReactNode; role?: string }) {
  const { status, user } = useAuth();
  if (status !== "authenticated") return <Navigate to="/login" replace />;
  if (role && user?.role !== role) return <Navigate to="/" replace />;
  return <>{children}</>;
}
```

```tsx
<Route path="/account" element={<RequireAuth><AccountPage /></RequireAuth>} />
<Route path="/admin"   element={<RequireAuth role="admin"><AdminPage /></RequireAuth>} />
```

The sentence: *the guard and the hidden admin link both run in the browser, where the user controls the
code, so they are user experience — the API authorizes every protected request independently, and that is
the actual boundary.* `replace` swaps the history entry so the guarded URL does not linger in the back
button.

`QC: Leverage route guards to change the routing behavior based on the given state.`
`Source: content/05-react/client-auth-arc.md`; `demo/react-spa-demo/src/components/RequireAuth.tsx`
</details>

### Drill R9 — Attach auth in exactly one place

**Task.** Add an HTTP client library to your project, configure a single instance with a base URL, and
make every outgoing request carry a bearer token without any call site knowing about it. Then explain the
CORS error a new project sees the first time this runs.

<details>
<summary>Model solution</summary>

```
npm install axios
```

```ts
export const api = axios.create({ baseURL: "http://localhost:5137" });

api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;                       // must return the (possibly modified) config
});
```

The CORS explanation: `Authorization` is not a "simple" header, so adding it makes a cross-origin request
non-simple and the browser sends an automatic `OPTIONS` **preflight** first. If the server's CORS policy
does not allow the calling origin, the method, and the `Authorization` header, the browser blocks the
real request before it is sent — so you see a CORS error, not a 401. The fix lives on the server.

`QC: Leverage NPM libraries in a React project to add functionality.` / `Make HTTP requests using Axios
or Fetch and handle the response.`
`Source: content/05-react/axios-fetch-data.md`, `client-auth-arc.md`;
`demo/react-spa-demo/src/api/client.ts`
</details>

### Drill R10 — Test behaviour, not internals

**Task.** Write two component tests: one asserting that a component renders the data it was given, and
one asserting that clicking a button invokes the callback prop. Query the way a user perceives the UI.
Then say what you must **not** assert on.

<details>
<summary>Model solution</summary>

```tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import "@testing-library/jest-dom";

test("renders the book's title and author", () => {
  render(<BookCard title="The Pragmatic Programmer" author="Hunt & Thomas" />);
  expect(screen.getByRole("heading", { name: "The Pragmatic Programmer" })).toBeInTheDocument();
  expect(screen.getByText("Hunt & Thomas")).toBeInTheDocument();
});

test("calls onCheckout when the button is clicked", async () => {
  const handleCheckout = vi.fn();          // Jest: jest.fn()
  render(<CheckoutButton onCheckout={handleCheckout} />);
  await userEvent.click(screen.getByRole("button", { name: "Borrow" }));
  expect(handleCheckout).toHaveBeenCalledTimes(1);
});
```

Do **not** assert on internal state, class names, or DOM structure — no
`container.querySelector(".btn-primary")`, no "this useState holds 3." Assert on the rendered output a
user would see and on whether callbacks fired, so a refactor that preserves behaviour keeps the test
green. Query families: `getBy*` throws (presence), `queryBy*` returns null (absence), `findBy*` awaits
(async).

`QC: Use Jest and a React testing library to test components.`
`Source: content/05-react/testing-jest-rtl.md`
</details>

---

## One last pass

If you have time for only one thing the night before, do **J1**, **B3**, **R3**, and **R7** again from
memory. Those four cover the traps that produce the most lost marks: coercion and truthiness, the fetch
failure lanes, mutation killing a re-render, and the reducer/Context shape.
