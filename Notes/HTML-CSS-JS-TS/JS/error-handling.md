# Error Handling in JavaScript

## Learning Objectives
- Handle errors with `try` / `catch` / `finally`, explain what `finally` guarantees, and `throw`
  proper `Error` objects — knowing the Error anatomy and the built-in error types on sight.
- Explain why throwing non-Error values is legal but bad practice.
- Recognize a custom error class (`class NotFoundError extends Error`) and say what it buys.
- Handle errors in async code: `try/catch` around `await`, `.catch` on chains, and why an uncaught
  rejection is a crash-in-waiting.

## Why This Matters
Networks drop, users type garbage, and APIs return bodies you did not expect — error paths are half
the code. The difference between an app that shows "could not load your orders, retry?" and one that
silently renders a blank page is a small toolset used with discipline: `try/catch/finally`, real
`Error` objects, and knowing where a rejected promise lands. Interviewers probe this with "what does
finally guarantee" and "how do you catch an async error" — both have precise answers.

## The Concept

### try / catch / finally, and throw
```js
function parsePrice(raw) {
  if (typeof raw !== "string") throw new TypeError("raw must be a string");
  const price = Number(raw);
  if (Number.isNaN(price)) throw new Error(`not a price: "${raw}"`);
  return price;
}

let spinner = show("loading");
try {
  const price = parsePrice(input);
  render(price);
} catch (e) {                       // runs only if the try block threw
  render(`error: ${e.message}`);
} finally {                         // runs on BOTH paths — success or throw
  hide(spinner);                    // cleanup belongs here
}
```
`finally` guarantees its block runs whether the `try` completed or threw — even if the `catch`
rethrows, and even past a `return` — making it the home for cleanup: spinners, resources, state. The
cost of wrapping everything is real, though: a `catch` that just swallows (`catch (e) {}`) converts a
loud failure into silent wrong behavior — the worst outcome.

### The Error object, and the built-in types
Every `Error` carries three fields worth knowing: **`message`** (your human-readable description),
**`name`** (the error's type, e.g. `"TypeError"`), and **`stack`** (a snapshot of the call stack at
construction — the thing that makes errors debuggable). Built-ins to recognize on sight:

| Type | The runtime throws it when... |
|---|---|
| `TypeError` | a value has the wrong type: `null.foo`, calling a non-function |
| `RangeError` | a value is out of range: `new Array(-1)`, `x.toFixed(200)` |
| `SyntaxError` | code (or `JSON.parse` input) fails to parse |
| `ReferenceError` | an undeclared identifier is read (see `variables-scope-types-coercion.md`) |

Throwing non-Error values — `throw "oops"` or `throw 404` — is legal, and bad practice: a string has
**no stack trace**, no `name`, and `catch (e)` code that reads `e.message` gets `undefined`. Always
`throw new Error("...")` or a subclass. (`throw` also accepts any expression, which is exactly why the
convention has to be a discipline.)

### Custom errors — recognize on sight
```js
class NotFoundError extends Error {
  constructor(resource, id) {
    super(`${resource} ${id} not found`);
    this.name = "NotFoundError";
  }
}
// later...
catch (e) {
  if (e instanceof NotFoundError) renderEmptyState();
  else throw e;                                  // not mine to handle — let it propagate
}
```
A custom class buys **typed catching**: `instanceof` lets a handler treat "missing record" differently
from "server on fire" without string-matching messages. The `else throw e` line encodes the placement
rule: **catch where you can act** — retry, substitute a fallback, or tell the user something useful —
and let everything else propagate to a boundary that can. A `catch` that cannot act is a silencer.

### Async errors: where rejected promises land
A rejected promise does not enter a surrounding `try/catch` on its own — you must either `await` it
inside one, or attach `.catch`:

```js
// 1. async/await: try/catch works because await unwraps the rejection
async function loadOrders() {
  try {
    const res = await fetch("/api/orders");
    if (!res.ok) throw new Error(`HTTP ${res.status}`);   // fetch does NOT throw on 404/500
    return await res.json();
  } catch (e) {                    // network failure, the thrown HTTP error, bad JSON — all land here
    showRetryBanner(e.message);
    return [];
  }
}

// 2. promise chains: .catch is the catch block
fetch("/api/orders")
  .then(res => res.json())
  .then(renderOrders)
  .catch(e => showRetryBanner(e.message));   // one .catch covers every step above it
```
If neither is present, the rejection becomes an **unhandled rejection** — a crash-in-waiting: nothing
visible happens at first, then the runtime reports it (browsers fire an event; server runtimes can
terminate the process). Every chain must end in a `.catch`, or be awaited inside a `try`. Promise and
rejection mechanics get full depth in `promises-async.md`; the `fetch` "ok is not thrown" trap gets
more in `fetch-json-http.md`. Name-only: `window.onerror` catches uncaught synchronous errors
page-wide, and `unhandledrejection` is its promise-side twin — reporting hooks, not a substitute for
local handling.

## Say It in an Interview
- *"I wrap the risky call in try/catch and put cleanup in finally — finally runs whether the try
  succeeded or threw, so spinners and resources always get released. I throw `new Error` with a clear
  message, and I can read message, name, and stack off anything I catch."*
- *"Throwing a string is legal but loses the stack trace and the Error shape — code catching it can't
  read `e.message` or tell types apart, so I always throw Error instances."*
- *"A custom class like `NotFoundError extends Error` gives me typed catches — `instanceof` handling
  for expected failures, rethrow for everything else."*
- *"With async code, try/catch only works around `await`; chains need a `.catch`. A rejection with
  neither is an unhandled rejection — a crash-in-waiting that surfaces far from the cause. And I catch
  where I can act — retry, fallback, or a user message — otherwise I let it propagate."*

## Check Yourself
1. In a try/catch/finally where the `try` returns early, does `finally` still run?
2. Name the three standard properties on an Error instance, and the one you lose by `throw "oops"`.
3. Which built-in error fits: reading `.length` of `null`; `JSON.parse("{bad")`; `(5).toFixed(500)`?
4. Why does `try { fetch(url).then(r => r.json()) } catch (e) {}` fail to catch a network error?
5. When should a function catch an error rather than let it propagate?

**Answers:** (1) Yes — `finally` runs on completion, throw, or return; that guarantee is its purpose.
(2) `message`, `name`, `stack`; throwing a string loses the stack (and the rest of the Error shape).
(3) `TypeError`; `SyntaxError`; `RangeError`. (4) Nothing is awaited — the try block finishes before
the promise settles, so the rejection bypasses the catch; `await` the chain inside the try, or attach
`.catch`. (5) When it can *act*: retry, substitute a fallback value, or inform the user; otherwise
rethrow or let it propagate to a boundary that can.

## Summary
- `try` runs the risky code, `catch (e)` handles a throw, `finally` runs on both paths — cleanup lives
  there.
- Throw `new Error("message")`; instances carry `message`, `name`, `stack`. Strings and numbers are
  throwable but stackless — never do it.
- On sight: `TypeError` (wrong type), `RangeError` (out of range), `SyntaxError` (unparseable),
  `ReferenceError` (undeclared); custom `class X extends Error` enables `instanceof` handling.
- Async: `try/catch` needs `await`; chains need `.catch`; an unhandled rejection is a crash-in-waiting.
- Catch where you can act; propagate otherwise. `window.onerror` / `unhandledrejection` exist as
  last-resort reporting hooks.

## Resources
- [try...catch (MDN reference)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/try...catch)
- [Error (MDN reference)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Error)
- [Custom errors, extending Error (javascript.info)](https://javascript.info/custom-errors)
