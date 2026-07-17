# Objects, Arrays, Loops, and Prototypal Inheritance

## Learning Objectives
- Create objects with literals; access properties via dot and bracket; recognize shorthand
  properties, nesting, `Object.create()`/`new Object()`, destructuring, and spread on sight.
- Create arrays with literals and explain why arrays are really objects with numeric keys.
- Use the core array methods — map, filter, reduce, find, and the mutators — and know which mutate.
- Loop with classic `for`, `while`/`do-while`, `for...of`, `for...in`, and `forEach`, choosing the
  right one for the job.
- Explain inheritance in JavaScript: the prototype chain, and `class ... extends` as syntax sugar.

## Why This Matters
Objects and arrays are the only data structures most JavaScript applications ever use: every JSON
payload from an API deserializes into them, and every UI list you render is an array transformation
away from that payload. The map/filter/reduce family is the daily vocabulary of front-end code, and
"how does inheritance work in JavaScript" remains a classic interview filter because the prototype
chain looks like other languages' classes but is not.

## The Concept

### Objects: literals, access, and the shorthand family
```js
const key = "isbn";
const book = {
  title: "Dune",
  author: { name: "Herbert" },          // nested object
  [key]: "978-0441172719",              // computed key
};
book.title;          // dot access — key known at write time
book["isbn"];        // bracket access — key is dynamic, or not a valid identifier
const title = "Emma", pages = 474;
const other = { title, pages };         // shorthand: variable name becomes the key
```
**Recognize on sight:** `Object.create(proto)` makes an object with an explicit prototype;
`new Object()` is the constructor form of `{}` (nobody writes it, everybody should recognize it).
**Destructuring** pulls properties into variables, and **spread** copies enumerable properties:

```js
const { title: t, author } = book;      // destructuring (with a rename)
const copy = { ...book, title: "New" }; // spread: copy + override
```
One-liner to keep: spread and `Object.assign` are **shallow** copies — nested objects are shared, so
`copy.author === book.author`. For a true deep copy, `structuredClone` exists (name-level).

### Arrays: literals, and what they really are
```js
const skus = ["BK-001", "BK-002"];
skus[0];        // "BK-001"     skus.length;  // 2     typeof skus;  // "object"
```
Arrays are ordinary objects whose keys happen to be numeric strings, plus a self-maintaining `length`
and a toolbox of methods on `Array.prototype`. That is why `typeof` says `"object"` and why
`Array.isArray(skus)` is the real test (see `variables-scope-types-coercion.md`).

### The array-method toolbox
| Method | Returns | Mutates? | Example |
|---|---|---|---|
| `map(fn)` | new array, transformed | no | `prices.map(p => p * 1.07)` |
| `filter(fn)` | new array, kept items | no | `books.filter(b => b.inStock)` |
| `reduce(fn, init)` | single accumulated value | no | `prices.reduce((sum, p) => sum + p, 0)` |
| `find(fn)` | first match or `undefined` | no | `books.find(b => b.isbn === id)` |
| `forEach(fn)` | `undefined` — side effects only | no | `books.forEach(b => log(b))` |
| `push` / `pop` | new length / removed item | **yes** — end | `skus.push("BK-003")` |
| `shift` / `unshift` | removed item / new length | **yes** — front | `skus.shift()` |
| `slice(a, b)` | new sub-array | no | `top3 = ranked.slice(0, 3)` |
| `splice(i, n, ...)` | removed items | **yes** — in place | `skus.splice(1, 1)` |
| `includes(x)` | boolean | no | `skus.includes("BK-001")` |
| `some(fn)` / `every(fn)` | boolean | no | `books.some(b => b.inStock)` |
| `sort(cmp)` | the same array | **yes** | `prices.sort((a, b) => a - b)` |

Two traps: **slice vs splice** — slice copies out, splice edits in place; and **sort** both *mutates*
and defaults to *string* comparison, so `[10, 2].sort()` gives `[10, 2]` — always pass a comparator
for numbers. The non-mutating style costs extra allocations but keeps data flow predictable — why UI
code overwhelmingly prefers map/filter over in-place edits.

### Loops: five ways through a collection
```js
for (let i = 0; i < skus.length; i++) { ... }  // classic: index math, break/continue, fastest
for (const s of skus) { ... }                  // for...of: the VALUES — default for arrays
for (const k in book) { ... }                  // for...in: the KEYS — for objects, not arrays
skus.forEach((s, i) => { ... });               // callback per item; cannot break out
while (queue.length) { ... }                   // condition-driven; do-while runs at least once
```
`for...in` on an array iterates *string* indexes plus any inherited enumerable keys — wrong tool; use
`for...of` for array values and `for...in` (or `Object.keys`) for object keys. A classic `for` still
wins when you need early exit (`break`), index arithmetic (stepping by 2, iterating backwards), or the
last drop of performance in a hot path — `forEach` can do none of those.

### Inheritance: the prototype chain
Objects do not copy behavior from parents — they **delegate**: a failed property lookup walks up the
object's prototype, then that prototype's prototype, until `null`. `class` syntax is **sugar over
exactly this mechanism**:

```js
class Media {
  constructor(title) { this.title = title; }
  describe() { return `Media: ${this.title}`; }
}
class Book extends Media {
  describe() { return `Book: ${this.title}`; }   // overrides by sitting EARLIER in the chain
}
const b = new Book("Dune");
b.describe();                                    // found on Book.prototype
// The equivalent prototype view:
Object.getPrototypeOf(b) === Book.prototype;                       // true
Object.getPrototypeOf(Book.prototype) === Media.prototype;         // true — the chain
```
**Recognize on sight:** `Book.prototype` is the object instances delegate to; an instance's own link
to it is exposed as `__proto__` (legacy — read with `Object.getPrototypeOf`). They are different
things: `prototype` lives on the constructor, `__proto__` on the instance. Trade-off of deep chains:
every miss walks the whole chain, and long trees are harder to reason about than composition — most
modern code keeps hierarchies one level deep or avoids them.

## Say It in an Interview
- *"I create objects with literals, use dot access for known keys and brackets for dynamic ones, and
  lean on destructuring and spread — remembering spread is a shallow copy."*
- *"Arrays are objects with numeric keys and a managed length, which is why `typeof` says object and
  `Array.isArray` is the real check."*
- *"Map transforms, filter keeps, reduce folds to one value, find grabs the first match. I watch the
  mutators — push, splice, sort — and remember sort is string-based unless you pass a comparator."*
- *"`for...of` gives values and is my default for arrays; `for...in` gives keys and belongs on
  objects. I drop to a classic `for` for break, index math, or raw speed — `forEach` can't break."*
- *"JavaScript inheritance is delegation up the prototype chain — a failed lookup walks the chain
  until null. `class extends` is syntax sugar over that: methods land on the constructor's prototype
  and instances delegate to it."*

## Check Yourself
1. What is the difference between `book.title` and `book[key]`, and when is bracket access required?
2. Which of these mutate the array: `map`, `push`, `slice`, `splice`, `sort`, `filter`?
3. Why does `[10, 2, 1].sort()` return `[1, 10, 2]`, and what fixes it?
4. Why is `for...in` the wrong loop for arrays, and which loop lets you `break` early?
5. In `class Book extends Media`, where does `describe` live, and how does an instance find it?

**Answers:** (1) Dot needs a literal, identifier-legal key; brackets take any expression — required for
dynamic keys or keys with spaces/dashes. (2) `push`, `splice`, `sort` mutate; `map`, `slice`, `filter`
return new arrays. (3) Default sort compares as *strings* (`"10" < "2"`); pass `(a, b) => a - b`.
(4) `for...in` yields string keys, not values — use `for...of`; classic `for` (or `for...of`) supports
`break`, `forEach` does not. (5) On `Book.prototype`; the instance's own lookup misses, then delegates
up the prototype chain and finds it there.

## Summary
- Object literals + dot/bracket access; shorthand, computed keys, destructuring, spread — spread
  copies shallow; `structuredClone` for deep.
- Arrays are objects with numeric keys; `Array.isArray` over `typeof`.
- Toolbox: map/filter/reduce/find return new values; push/pop, shift/unshift, splice, sort mutate;
  slice copies, splice edits; sort needs a numeric comparator.
- `for...of` = values (arrays), `for...in` = keys (objects); classic `for` for break/index/perf.
- Inheritance = prototype-chain delegation; `class extends` is sugar; `prototype` on constructors,
  `__proto__` (legacy) on instances.

## Resources
- [Array (MDN reference)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array)
- [Inheritance and the prototype chain (MDN)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Inheritance_and_the_prototype_chain)
- [Array methods (javascript.info)](https://javascript.info/array-methods)
