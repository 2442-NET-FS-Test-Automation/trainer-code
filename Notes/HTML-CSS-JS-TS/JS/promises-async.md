# Promises and async/await

## Learning Objectives
- Contrast synchronous and asynchronous execution, and explain why blocking freezes the page.
- Describe what a Promise is: its three states and consuming with `.then`/`.catch`/`.finally`.
- Choose among the static combinators: `Promise.all`, `allSettled`, `race`, `any`.
- Rewrite `.then` chains with `async`/`await` and handle errors with `try`/`catch`.
- Decide between sequential `await`s and `Promise.all` for multiple operations.
- Explain the event loop and predict output ordering for sync code, promise callbacks, and timers.

## Why This Matters
Every interesting thing a front end does — fetch data, wait for a timer — is asynchronous, and JavaScript
runs it all on one thread. Misunderstanding this produces the classic bugs: reading a result before it
exists, serial `await`s that triple your load time, rejections that vanish uncaught. And the event-loop
"what does this log?" question is one of the most reliable JavaScript interview questions in existence.

## The Concept

### Synchronous vs asynchronous on one thread
Synchronous code blocks until each line finishes, and JavaScript runs on a **single thread** — a long
synchronous computation freezes everything: no clicks, no rendering. Asynchronous operations (timers,
HTTP, I/O) are therefore **scheduled**: the work starts, your function returns immediately, and a
callback runs later with the result. Promises are the standard way to write that "later" part.

### What a Promise is
A **Promise** is an object representing the eventual result of an async operation. It is always in one
of three states: **pending**, **fulfilled** (has a value), or **rejected** (has a reason) — and once
settled it never changes. You consume one by attaching callbacks:
```js
fetchCatalog()
  .then((books) => render(books))     // runs on fulfillment, receives the value
  .catch((err) => showError(err))     // runs on rejection anywhere above
  .finally(() => hideSpinner());      // runs either way
```
Each `.then` returns a **new** promise, which is what makes chaining work — return a value and the next
`.then` sees it; return a promise and the chain waits for it. Recognize construction on sight:
`new Promise((resolve, reject) => { ... })` wraps a callback-style API so it can join chains — you will
mostly *consume* promises that libraries hand you, not construct them.

### The static combinators

| Method | Resolves with | Rejects | Use when |
|---|---|---|---|
| `Promise.all([...])` | array of all values, in order | on the **first** rejection (fails fast) | all results required; any failure sinks the operation |
| `Promise.allSettled([...])` | array of `{status, value/reason}` | never | you want every outcome, successes and failures alike |
| `Promise.race([...])` | first to **settle** (fulfill or reject) | if the first settler rejected | timeouts: race the work against a timer |
| `Promise.any([...])` | first to **fulfill** | only if all reject | redundant sources; first success wins |

The all-vs-allSettled trade-off: `all` is the default when results are jointly required, but fail-fast
discards the outcomes that succeeded; `allSettled` never rejects, at the cost of inspecting each
`{status}` yourself.

### async/await
`async`/`await` is syntax over promises — no new machinery. An `async` function **always returns a
Promise**; `await` unwraps a value and pauses *only that function* (the thread moves on — awaiting never
blocks the page). The same flow both ways:
```js
function loadThen() {
  return fetchBook(1)
    .then((book) => fetchAuthor(book.authorId))
    .then((author) => author.name);
}

async function loadAwait() {
  try {
    const book = await fetchBook(1);
    const author = await fetchAuthor(book.authorId);
    return author.name;
  } catch (err) {                     // one catch covers every await above it
    showError(err);
  }
}
```
The `await` version reads top-to-bottom like sync code, and rejections surface as ordinary exceptions
handled with `try`/`catch` — the same mechanics as `error-handling.md`.

### Sequential vs parallel
```js
const book = await fetchBook(1);                 // dependent: author id comes FROM book
const author = await fetchAuthor(book.authorId); // must be sequential

const [books, members] = await Promise.all([fetchBooks(), fetchMembers()]);
```
Sequential `await`s are correct only when call B needs call A's result. Two **independent** requests
awaited in sequence take `timeA + timeB`; started together under `Promise.all`, `max(timeA, timeB)`.
Serial awaits over independent calls is the most common self-inflicted latency bug in async code.

### The event loop
JavaScript has one **call stack**; async callbacks wait in queues, and the **event loop** moves them
onto the stack only when it is empty. Two queue priorities exist: **microtasks** (promise callbacks)
drain completely before the next **macrotask** (`setTimeout`, events). The one-glance ordering example:
```js
console.log("one");
setTimeout(() => console.log("four"), 0);
Promise.resolve().then(() => console.log("three"));
console.log("two");
// output: one, two, three, four
```
Sync code runs to completion; then the microtask (promise `.then`); the timer macrotask last, even at
0ms delay.

### Adjacent: two one-liners
A rejected promise with no `.catch`/`try-catch` anywhere becomes an **unhandled rejection** — the browser
logs an error and nothing recovers it, so every chain needs a handler at its end. And `setTimeout(fn, 0)`
does not mean "now": it means "queue as a macrotask after the current stack and all pending microtasks."

## Say It in an Interview
- *"JavaScript is single-threaded, so long synchronous work freezes the page; async operations are
  scheduled instead — the call returns immediately and a callback delivers the result later."*
- *"A Promise is an object for an eventual result, in one of three states — pending, fulfilled,
  rejected — and once settled it's final. I consume it with `.then`, `.catch`, and `.finally`."*
- *"`Promise.all` fails fast on the first rejection, `allSettled` never rejects and reports every
  outcome, `race` takes the first to settle — good for timeouts — and `any` takes the first success."*
- *"`async`/`await` is sugar over promises: an async function returns a Promise, `await` pauses only
  that function, and rejections become exceptions I handle with ordinary `try`/`catch`."*
- *"I await sequentially only when calls depend on each other; independent calls I start together and
  `Promise.all` them, turning `timeA plus timeB` into `max` of the two."*
- *"The event loop runs queued callbacks when the call stack empties, and microtasks — promise
  callbacks — drain before macrotasks like `setTimeout`, which is why a resolved `.then` logs before a
  zero-millisecond timer."*

## Check Yourself
1. Name the three promise states and the rule about moving between them after settlement.
2. Five API calls must all succeed or the operation is void; separately, five health checks should each
   report success or failure. Which combinator for each?
3. What does an `async` function return if its body ends with `return 42`?
4. Two independent fetches take 300ms and 500ms. Total time awaited sequentially vs with `Promise.all`?
5. Predict the output order: `console.log("a"); setTimeout(() => console.log("b"), 0);
   Promise.resolve().then(() => console.log("c")); console.log("d");`

**Answers:** (1) Pending, fulfilled, rejected; once fulfilled or rejected (settled), the state and value
never change. (2) `Promise.all` (fails fast, all-or-nothing) for the first; `Promise.allSettled` (never
rejects, per-item `{status}`) for the health checks. (3) A Promise that fulfills with `42` — async
functions always return promises. (4) About 800ms sequentially (300 + 500) vs about 500ms with
`Promise.all` (max of the two). (5) `a, d, c, b` — sync first, then the microtask (`c`), then the timer
macrotask (`b`).

## Summary
- Single thread: sync blocks, async schedules; blocking the thread freezes the page.
- Promise = eventual result; pending → fulfilled/rejected, settled once; consume with
  `.then`/`.catch`/`.finally`; each `.then` returns a new promise (chaining).
- Combinators: `all` fails fast, `allSettled` never rejects, `race` first settled, `any` first fulfilled.
- `async` functions return promises; `await` pauses only that function; errors via `try`/`catch`.
- Dependent calls: sequential awaits. Independent calls: `Promise.all` — latency drops to the slowest.
- Event loop: stack empties → microtasks drain → next macrotask; hence sync → promise → timer ordering.

## Resources
- [Using promises (MDN)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Using_promises)
- [Promises, async/await (javascript.info)](https://javascript.info/async)
- [Event loop: microtasks and macrotasks (javascript.info)](https://javascript.info/event-loop)
