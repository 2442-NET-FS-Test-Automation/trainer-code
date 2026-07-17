# Variables, Scope, Types, and Coercion

## Learning Objectives
- Create variables with `let` and `const` (and recognize legacy `var`), defaulting to `const`.
- Explain global, function, and block scope — and why `var` in a loop is a classic bug.
- Name the seven primitive types plus objects, and the `typeof` quirks worth knowing.
- Predict coercion results like `"5" + 1` vs `"5" - 1`, and defend `===` over `==`.
- List the six falsy values and use truthiness safely in conditions.
- Describe hoisting and the temporal dead zone for `var`, function declarations, and `let`/`const`.

## Why This Matters
Every JavaScript bug class has a variables-and-types story underneath it: a `var` leaking out of a loop,
a `==` comparison "helpfully" coercing a string, an `undefined` sneaking past a truthiness check. These
are also the highest-frequency screening questions in JavaScript interviews — `let` vs `var`, `==` vs
`===`, and "what does `typeof null` return" are asked practically verbatim. Getting this layer solid
makes everything above it (functions, async, frameworks) debuggable instead of mysterious.

## The Concept

### Declaring variables: const, let, and legacy var
```js
const taxRate = 0.07;   // cannot be reassigned — the default choice
let subtotal = 0;       // reassignable — use when the value genuinely changes
var legacy = "avoid";   // pre-2015 declaration — function-scoped, hoisted; recognize, don't write
```
Prefer `const` unless you will reassign: it documents intent and turns accidental reassignment into an
error. Note the nuance interviewers probe: `const` freezes the *binding*, not the value — a `const`
array can still be `push`ed to. The cost of `let`-everywhere is that readers must scan for reassignment;
`const` removes that question.

### Scope: global, function, block
- **Global scope** — declared outside everything; visible everywhere (and a namespace-pollution risk).
- **Function scope** — `var` and parameters live for the whole function body.
- **Block scope** — `let`/`const` live only inside the nearest `{ }` (an `if`, a loop body, a bare block).

The classic gotcha shows why block scope matters — `var` gives every timer callback the *same* variable:

```js
for (var i = 0; i < 3; i++) setTimeout(() => console.log(i));  // 3, 3, 3
for (let j = 0; j < 3; j++) setTimeout(() => console.log(j));  // 0, 1, 2
```
With `var` there is one function-scoped `i`, already `3` when the callbacks run; `let` creates a fresh
`j` per iteration, and each callback closes over its own (closures in depth: `functions-this-closures.md`).

### Data types
Seven **primitives**: `string`, `number`, `boolean`, `null`, `undefined`, `symbol`, `bigint`. Everything
else is an **object** — including arrays and functions, which are objects with extra behavior (see
`objects-arrays-loops.md`). `undefined` means "never assigned"; `null` means "deliberately empty."

| Expression | Result | Note |
|---|---|---|
| `typeof "hi"` | `"string"` | as expected |
| `typeof null` | `"object"` | historic bug, kept for compatibility — memorize it |
| `typeof [1,2]` | `"object"` | arrays are objects; use `Array.isArray()` |
| `typeof undefined` | `"undefined"` | as expected |
| `typeof function(){}` | `"function"` | the one object that reports specially |

### Coercion, == vs ===
JavaScript converts types implicitly, and `+` is the trap: if either operand is a string, `+`
concatenates; every other arithmetic operator converts *to number*.

```js
"5" + 1   // "51"  — string wins with +
"5" - 1   // 4     — minus forces numbers
"5" == 5  // true  — == coerces before comparing
"5" === 5 // false — === compares type AND value, no coercion
```
`===` is the default choice because its result is predictable from the operands alone; `==` runs a
coercion algorithm few people can recite (`0 == ""` is true, `null == undefined` is true), so it hides
bugs. Adjacent facts worth one line each: `NaN` is the only value not equal to itself — test it with
`Number.isNaN(x)`, never `x === NaN`; and `Object.is` exists as a third, stricter sameness check.

### Truthy and falsy
Exactly **six values are falsy**: `false`, `0`, `""`, `null`, `undefined`, `NaN`. *Everything* else is
truthy — including `"0"`, `[]`, and `{}`. Guard patterns rely on this:

```js
if (user) greet(user);            // guards against null/undefined
const name = input || "guest";    // fallback — but hides legitimate 0 or ""
const qty  = input ?? 0;          // nullish coalescing: only null/undefined trigger the fallback
```
The trade-off: `||` treats `0` and `""` as missing, which is wrong for quantities and optional text —
that is exactly why `??` exists.

### Hoisting and the temporal dead zone
Declarations are processed before code runs, but the three forms hoist differently:

```js
console.log(a);   // undefined — var hoists, initialized to undefined
hoisted();        // works — function declarations hoist fully, body included
console.log(b);   // ReferenceError — b is in the temporal dead zone (TDZ)
var a = 1;
function hoisted() {}
let b = 2;
```
`let`/`const` are hoisted too, but stay uninitialized in the **TDZ** until the declaration line executes
— reading them earlier throws. That is a feature: a loud `ReferenceError` beats `var`'s silent
`undefined` propagating through your math.

## Say It in an Interview
- *"I default to `const` and use `let` only when I'll reassign; `var` is legacy — function-scoped and
  hoisted, so it leaks out of blocks."*
- *"`let` and `const` are block-scoped to the nearest braces, `var` is function-scoped — that's why
  `var` in a loop with async callbacks logs the final value three times, while `let` gives each
  iteration its own binding."*
- *"There are seven primitives — string, number, boolean, null, undefined, symbol, bigint — and
  everything else is an object, including arrays and functions. `typeof null` returning `object` is a
  historic bug you just memorize."*
- *"`==` coerces before comparing and `===` doesn't, so I use `===` by default — its result is
  predictable without reciting the coercion table."*
- *"Six values are falsy — false, 0, empty string, null, undefined, NaN — everything else is truthy,
  including empty arrays and objects; for defaults I reach for `??` so 0 and empty string survive."*
- *"`var` hoists initialized to undefined, function declarations hoist fully, and `let`/`const` sit in
  the temporal dead zone — accessing them before the declaration throws instead of yielding undefined."*

## Check Yourself
1. What does this log, and why? `for (var i = 0; i < 2; i++) setTimeout(() => console.log(i));`
2. Predict: `"5" + 1`, `"5" - 1`, `"5" == 5`, `"5" === 5`.
3. List the six falsy values. Is `[]` truthy?
4. Why does `const name = input || "guest"` misbehave for `input = ""`, and what fixes it?
5. What is the temporal dead zone, and which declarations does it apply to?

**Answers:** (1) `2, 2` — one function-scoped `i` shared by both callbacks, already `2` when they run;
`let` would log `0, 1`. (2) `"51"` (string `+` concatenates), `4` (`-` forces numbers), `true` (`==`
coerces), `false` (`===` compares type too). (3) `false, 0, "", null, undefined, NaN`; yes, `[]` is
truthy. (4) `""` is falsy, so `||` replaces a legitimately empty string; `input ?? "guest"` falls back
only on `null`/`undefined`. (5) The window between a scope's start and the `let`/`const` declaration
line, during which access throws a `ReferenceError`; it applies to `let` and `const` (and `class`).

## Summary
- `const` by default, `let` when reassigning, `var` only recognized, never written.
- `let`/`const` are block-scoped; `var` is function-scoped — the loop/timer gotcha proves the point.
- Seven primitives + objects; `typeof null === "object"`; `Array.isArray` for arrays.
- `+` concatenates when a string is present; other operators coerce to number; use `===`.
- Falsy: `false, 0, "", null, undefined, NaN`; prefer `??` over `||` for defaults.
- Hoisting: `var` → `undefined`, function declarations → fully usable, `let`/`const` → TDZ, access
  throws.
- `NaN !== NaN`; check with `Number.isNaN`. `Object.is` is the third sameness algorithm.

## Resources
- [JavaScript data types and data structures (MDN)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Data_structures)
- [Equality comparisons and sameness (MDN)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Equality_comparisons_and_sameness)
- [The old "var" (javascript.info)](https://javascript.info/var)
