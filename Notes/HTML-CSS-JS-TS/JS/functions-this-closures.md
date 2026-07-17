# Functions, this, and Closures

## Learning Objectives
- Recognize every function form on sight: declarations, expressions, arrows, anonymous functions, and
  object methods.
- Explain callbacks: functions passed to other functions to be invoked later.
- Use arrow functions well — syntax variants, the lexical-`this` benefit, and when NOT to use them.
- Determine what `this` is in any call: method call, constructor, plain call, arrow.
- Explain closures: functions that retain their defining scope, with private state as the payoff.

## Why This Matters
Functions are JavaScript's unit of everything — event handlers, array callbacks, module boundaries all
pass functions around as values. The two ideas that separate working developers from snippet-copiers
are `this` (why did my handler lose its object?) and closures (why does every timer log the same
number?). Both are guaranteed interview material, usually with a code-reading trap attached.

## The Concept

### The function forms — recognize on sight
| Form | Syntax | Notes |
|---|---|---|
| Declaration | `function add(a, b) { return a + b; }` | hoists fully — callable before its line |
| Expression | `const add = function (a, b) { ... };` | a function as a value; not hoisted usable |
| Arrow | `const add = (a, b) => a + b;` | compact; lexical `this` (below) |
| Anonymous | `setTimeout(function () { ... }, 500);` | no name; usually an inline argument |
| Method | `const cart = { total() { ... } };` | function stored on an object property |

**Recognize on sight, adjacent forms:** default parameters `function f(x = 10)` supply a value when the
argument is `undefined`; rest parameters `function f(...args)` collect the remaining arguments into a
real array. An **IIFE** — `(function () { ... })()` — defines and immediately invokes a function,
historically used to create a private scope before modules existed.

### Callbacks
A **callback** is a function you hand to other code to be invoked later — later in time, or once per
item:

```js
setTimeout(() => console.log("one second later"), 1000);   // invoked later in time
[1, 2, 3].map(n => n * 2);                                 // invoked per element
button.addEventListener("click", handleClick);              // invoked per event
```
Callbacks are the primitive under every async and iteration API in the language. Nesting them for
sequential async work produces **callback hell** — pyramids of indentation with error handling smeared
everywhere; promises exist to fix exactly that (see `promises-async.md`).

### Arrow functions: variants, benefit, and the exclusion list
```js
const double = n => n * 2;             // single param: parens optional; implicit return
const add    = (a, b) => a + b;        // multiple params need parens
const make   = ()  => ({ id: 1 });     // returning an object literal needs wrapping parens
const logAll = xs => { xs.forEach(x => console.log(x)); };  // braces = explicit return needed
```
Benefits: shorter syntax for inline callbacks, and — the important one — **lexical `this`**: an arrow
has no `this` of its own; it uses whatever `this` was in the surrounding scope, so callbacks inside a
method keep the object. When **not** to use them: as **object methods** (their `this` is the outer
scope, not the object) and anywhere needing its own `this` or `arguments` — constructors, and DOM
handlers that rely on `this` being the element. The cost of the compact form is readability: past a
few lines, implicit return and terse syntax stop paying.

### The this keyword: decided at the call site
`this` is not "the function's object" — it is bound *per call*, by how the function is invoked:

```js
const counter = {
  count: 0,
  increment() { this.count++; }        // 1. method call: this = object before the dot
};
counter.increment();                    // this === counter

function Order(id) { this.id = id; }    // 2. constructor: this = the new instance
const o = new Order(7);

const fn = counter.increment;
fn();                                   // 3. plain call: this = undefined (strict) / globalThis (sloppy)
                                        //    — the classic "detached method" bug

const tick = () => this;                // 4. arrow: no own this — lexical, from surrounding scope
```
**Recognize on sight:** `fn.call(obj, a)` and `fn.apply(obj, [a])` invoke with an explicit `this`;
`fn.bind(obj)` returns a permanently bound copy — the pre-arrow fix for detached methods and callback
`this` loss. The interview trap is nearly always case 3: a method passed as a callback arrives as a
plain call and loses its object.

### Closures: functions that carry their scope
A **closure** is a function that retains access to the variables of the scope where it was *defined*,
even after that scope has returned. That gives JavaScript private state without classes:

```js
function makeCounter() {
  let count = 0;                        // lives on after makeCounter returns...
  return {
    increment: () => ++count,           // ...because these functions close over it
    current:   () => count
  };
}
const c = makeCounter();
c.increment(); c.increment();
c.current();                            // 2 — and no code outside can touch count directly
```
Each `makeCounter()` call creates an independent `count` — two counters never interfere. The classic
gotcha is loop-variable capture: `for (var i ...)` shares ONE `i` across all callbacks, so timers all
log the final value; `let` gives each iteration its own binding (worked example in
`variables-scope-types-coercion.md`). Trade-off: closed-over variables cannot be garbage-collected
while any closure referencing them lives — the standard closure-leak story.

## Say It in an Interview
- *"I can read all the forms: hoisted declarations, function expressions, arrows, anonymous inline
  callbacks, and methods on objects — the form changes hoisting and `this`, not passability."*
- *"A callback is a function handed to other code to run later — a timer, an event, or once per array
  element. Nested async callbacks become callback hell, which is what promises clean up."*
- *"Arrows buy shorter syntax and lexical `this` — they don't rebind, so callbacks inside a method keep
  the object. I avoid them as object methods and constructors, exactly because they have no `this` of
  their own."*
- *"`this` is decided at the call site: the object before the dot in a method call, the new instance
  under `new`, undefined in a strict plain call, and inherited from the enclosing scope in an arrow.
  `bind` fixes it when a method gets detached."*
- *"A closure is a function that keeps access to its defining scope after that scope returns — a counter
  factory with a private `count` is the canonical example. The cost: whatever it closes over can't be
  garbage-collected while the closure lives."*

## Check Yourself
1. Which function form is callable before the line that defines it, and why?
2. `const f = obj.method; f();` — what is `this` inside the call in strict mode, and name two fixes.
3. Why is an arrow function the wrong choice for an object method?
4. What does `makeCounter`'s returned object let you do that a plain `let count` cannot?
5. In `xs.map(n => n * 2)`, which concept is `n => n * 2` an instance of — and what is it called when
   such functions nest several levels deep in async code?

**Answers:** (1) A function declaration — it hoists fully, name and body. (2) `undefined` — a plain
call binds no receiver; fix with `obj.method.bind(obj)` or an arrow wrapper `() => obj.method()`.
(3) An arrow takes `this` from the surrounding scope, not the object it lives on, so `this.count`
would miss. (4) Private, tamper-proof state: `count` is reachable only through the returned functions,
and each factory call gets an independent copy. (5) A callback; deeply nested async callbacks are
"callback hell," solved by promises.

## Summary
- Five forms on sight: declaration (hoists), expression, arrow, anonymous, method; plus default/rest
  parameters and the IIFE pattern.
- Callbacks = functions invoked later by other code; the primitive under timers, events, and array
  methods; nesting them is callback hell.
- Arrows: terse variants, implicit return, lexical `this`; never as object methods or constructors.
- `this` by call site: dot-object, `new` instance, undefined in strict plain calls, lexical in arrows;
  `call`/`apply`/`bind` set it explicitly.
- Closures retain the defining scope: private state and factories; watch loop capture with `var` and
  memory held by long-lived closures.

## Resources
- [Closures (MDN)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Closures)
- [this (MDN reference)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/this)
- [Arrow functions, the basics (javascript.info)](https://javascript.info/arrow-functions-basics)
