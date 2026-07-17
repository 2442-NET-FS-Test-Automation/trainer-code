# Tuples and Enums

## Learning Objectives
- Declare and use tuples: fixed-length, per-position typed arrays, including labeled and readonly forms.
- Use numeric enums: auto-increment, explicit values, and reverse mapping.
- Use string enums and explain why they trade reverse mapping for readable wire values.
- Weigh enums against the modern alternative — string-literal unions and `as const` objects.

## Why This Matters
Tuples and enums are the two TypeScript features developers from C# recognize instantly — and then misuse,
because the trade-offs differ. Enums in particular carry a surprise: almost everything in TypeScript
erases to nothing, but **enums emit real runtime JavaScript**, and "which TS features generate code?" is a
classic interview follow-up. Knowing when a plain literal union beats an enum marks you as someone current
with how the ecosystem actually writes TypeScript.

## The Concept

### Tuples: arrays with a fixed shape
A tuple is an array with a **fixed length and a type per position**:

```ts
let pair: [string, number] = ["overdue-fee", 1.5];
pair[0].toUpperCase();   // string methods available at index 0
pair[1].toFixed(2);      // number methods at index 1
pair = ["x", 2, true];   // error: source has 3 elements but target allows only 2
```

Positions can be **labeled** for readability (labels are documentation only, not accessors), and tuples
can be `readonly`:

```ts
type Money = [amount: number, currency: string];
const origin: readonly [number, number] = [0, 0];
origin[0] = 5;   // error: readonly
```

Where tuples show up naturally: multiple return values (`function minMax(xs: number[]): [number, number]`)
and entry pairs (`Object.entries` yields `[string, T]`-shaped pairs). The trade-off vs an object with
named fields is pure readability: `[number, number]` forces callers to remember which position means what,
while `{ min: number; max: number }` documents itself. Rule of thumb — two elements with obvious roles,
tuple is fine; anything more, use an object.

### Numeric enums
```ts
enum Direction { Up, Down, Left, Right }   // auto-increments: Up = 0, Down = 1, ...
enum HttpPort { Web = 80, Secure = 443 }   // explicit values

let d: Direction = Direction.Up;
Direction[0];        // "Up" — reverse mapping: value back to name
```

Numeric enums support **reverse mapping** — `Direction[0]` returns `"Up"` — because the compiler emits a
real object mapping both directions. That is the cost to see clearly: this is one of the few TS features
that **emits runtime code** instead of erasing:

```js
// emitted JavaScript for Direction
var Direction;
(function (Direction) {
    Direction[Direction["Up"] = 0] = "Up";
    Direction[Direction["Down"] = 1] = "Down";
    // ...
})(Direction || (Direction = {}));
```

### String enums
```ts
enum Status { Pending = "PENDING", Shipped = "SHIPPED" }
let s: Status = Status.Pending;   // runtime value: "PENDING"
```

Every member needs an explicit string. No reverse mapping (`Status["PENDING"]` does not give a name back),
but the runtime values are **readable on the wire** — logs, JSON payloads, and database rows show
`"SHIPPED"` instead of a bare `2` whose meaning depends on declaration order. For anything serialized,
string enums beat numeric ones.

A one-liner to recognize: `const enum Direction { ... }` inlines the values at each use site and emits
**no** runtime object at all — smaller output, but with tooling caveats that keep many teams away from it.

### The modern alternative: literal unions and as const
Much current TypeScript skips `enum` entirely in favor of plain values plus types derived from them:

```ts
type Status = "pending" | "shipped";              // just a type — zero runtime artifact

const STATUS = { Pending: "pending", Shipped: "shipped" } as const;
type Status2 = typeof STATUS[keyof typeof STATUS]; // "pending" | "shipped", derived
```

The union gives the same compile-time checking with **no emitted code** and values that are plain strings
any JS caller understands — no import needed to consume them. The `as const` object variant restores the
enum's dotted, greppable member access (`STATUS.Pending`) while still erasing to a plain object (mechanics
of `as const` in `casting-guards-asconst.md`). What enums still offer in exchange: a single named
declaration site, opaque nominal-ish member types, and familiarity for C#/Java teams. Both are defensible;
know the trade before an interviewer asks.

## Say It in an Interview
- *"A tuple is a fixed-length array typed per position, like [string, number] — great for a pair of return
  values, but past two elements I switch to an object with named fields for readability."*
- *"Numeric enums auto-increment from zero and emit a runtime object with reverse mapping, so Direction[0]
  gives back the name — that emitted object is the cost."*
- *"String enums require explicit values and lose reverse mapping, but the wire values are readable — for
  anything serialized I prefer them over numeric enums."*
- *"Enums are one of the few TypeScript features that generate runtime JavaScript; the modern alternative
  is a string-literal union, or an as-const object when you want dotted access — same checking, zero
  runtime artifact."*

## Check Yourself
1. What does `let pair: [string, number]` guarantee that `(string | number)[]` does not?
2. What does `Direction[0]` evaluate to for `enum Direction { Up, Down }`, and what compiler output makes
   that work?
3. Why do string enums have no reverse mapping, and what do they give you instead?
4. Name two advantages of `type Status = "pending" | "shipped"` over an equivalent enum, and one thing
   the enum still offers.
5. What does `const enum` change about the emit?

**Answers:** (1) Exactly two elements, string at index 0 and number at index 1; the union array allows any
length with either type anywhere. (2) `"Up"` — the emitted enum object maps names to values *and* values
back to names. (3) The emitted object only maps name to string, and string values could collide with names;
the payoff is human-readable runtime/wire values. (4) Union: no emitted runtime code, and values are plain
strings usable without importing anything; enum keeps a single named declaration with dotted member access
(recoverable via the `as const` object pattern). (5) Values are inlined at each use site and no runtime
object is emitted at all.

## Summary
- Tuples: fixed length, type per position; labels document positions; readonly blocks writes; prefer
  named-field objects beyond about two elements.
- Numeric enums auto-increment from 0, allow explicit values, and emit an object with reverse mapping.
- String enums: explicit values, no reverse mapping, readable wire/log values — best for serialized data.
- `const enum` inlines and emits nothing (name-level knowledge).
- Enums EMIT runtime code — the exception in an erased type system; string-literal unions and `as const`
  objects give the same checking with zero runtime artifact.

## Resources
- [Enums — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/enums.html)
- [Tuple Types — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/objects.html)
- [Everyday Types — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/everyday-types.html)
