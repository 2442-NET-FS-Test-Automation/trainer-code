# Casting, Type Guards, and as const

## Learning Objectives
- Use type assertions (casting) with `as`, and explain why an assertion never converts anything at
  runtime — and what a wrong one costs.
- Narrow unions with type guards: typeof, instanceof, in, truthiness, and discriminated unions with an
  exhaustive switch.
- Use `as const` to freeze literals to their narrowest type and derive unions from data.
- Articulate assertion vs guard: "trust me" vs "prove it."

## Why This Matters
Unions gave you "one of these types" (`aliases-interfaces-unions.md`); this note is how you get back to
exactly one. There are two roads: **assert** (tell the compiler you know better) and **guard** (prove it
with runtime checks the compiler understands). Reaching for the first when you need the second is the most
common way TypeScript codebases crash at runtime while compiling clean — and "assertion vs guard" is a
standard interview follow-up for exactly that reason.

## The Concept

### Type assertions: overruling the compiler
The DOM is the classic case — `getElementById` returns `HTMLElement | null` because the compiler cannot
know what the id points at:

```ts
const input = document.getElementById("email") as HTMLInputElement;
console.log(input.value);   // .value exists on HTMLInputElement, not on HTMLElement
```

Recognize the older angle-bracket spelling on sight — `<HTMLInputElement>document.getElementById("email")`
— but write `as`: the bracket form collides with JSX tag syntax, so any code near React must use `as`.

The critical mental model: **an assertion converts nothing at runtime**. It is erased like every other
type; you are overruling the compiler's belief, not transforming the value. Assert wrongly and you have
manufactured a runtime bug with a clean compile:

```ts
const el = document.getElementById("email") as HTMLInputElement;
// if the id is actually on a <div>: compiles fine, then at runtime
el.value.trim();   // TypeError: undefined has no properties — .value never existed
```

The compiler rejects assertions between unrelated types, and the escape hatch — the **double assertion**
`value as unknown as T` — silences even that. Treat it as a smell: it appears in migrations and test
scaffolding, and every instance is a place the type system has been blindfolded. Nearby (name+one-line):
the `satisfies` operator checks a value against a type *without* changing its inferred type — checking
without the overruling.

### Type guards: proving it
A guard is a runtime check the compiler recognizes and uses to **narrow** a union inside the guarded
block:

```ts
function describe(id: string | number, when: Date | null, err: unknown) {
  if (typeof id === "string") id.toUpperCase();   // typeof: primitives
  if (when instanceof Date) when.getFullYear();   // instanceof: class instances
  if (when) when.getTime();                       // truthiness: strips null/undefined
  if (typeof err === "object" && err !== null && "message" in err) { /* in: shape check */ }
}
```

The flagship pattern is the **discriminated union**: every arm carries a literal `kind` property, and a
`switch` on it narrows perfectly — with `never` (see `basic-special-object-types.md`) enforcing
exhaustiveness:

```ts
type Shape =
  | { kind: "circle"; radius: number }
  | { kind: "rect"; width: number; height: number };

function area(s: Shape): number {
  switch (s.kind) {
    case "circle": return Math.PI * s.radius ** 2;   // s narrowed to the circle arm
    case "rect":   return s.width * s.height;
    default:
      const unreachable: never = s;   // add a "triangle" arm and THIS LINE errors
      return unreachable;
  }
}
```

That `never` line is the payoff: extend the union and every non-exhaustive switch in the codebase becomes
a compile error, pointing at exactly the logic that must be updated. For reusable guards, recognize the
**type predicate** signature `function isBook(x: unknown): x is Book` — a function whose `true` return
narrows the argument at the call site.

### as const: freezing literals
By default TS widens: `let role = "admin"` infers `string`. `as const` locks a value to its **narrowest
literal type** and makes it deeply readonly — which turns plain data into a source of types:

```ts
const ROLES = ["admin", "user", "guest"] as const;
// type: readonly ["admin", "user", "guest"]

type Role = typeof ROLES[number];   // "admin" | "user" | "guest" — derived, not repeated
function grant(role: Role) { /* ... */ }
grant("root");   // error: not assignable to Role
```

This is the config-object pattern: one runtime array to iterate for real work (dropdowns, validation
loops) and a union type derived from it — no drift possible between the two, no enum emit (the full
enum-vs-union trade lives in `tuples-enums.md`).

### Interface vs type, revisited
The canonical compare stays in `aliases-interfaces-unions.md`. What this note's tools add: aliases,
unions, and `as const` **compose** — `Shape` above and `Role` here are unions, so `type` was the only
option, and deriving `Role` from data is pure alias territory. Interfaces cannot express a union, so the
patterns on this page are alias-shaped end to end.

## Say It in an Interview
- *"An assertion like as HTMLInputElement overrules the compiler — nothing converts at runtime, so a wrong
  assertion compiles clean and crashes later; as-unknown-as-T silences even the sanity check, and I treat
  it as a smell."*
- *"Guards narrow a union with runtime proof: typeof for primitives, instanceof for classes, in for
  shapes, and my go-to is a discriminated union — a literal kind property, a switch on it, and a never
  default so adding an arm turns every stale switch into a compile error."*
- *"as const freezes a literal to its narrowest readonly type — I keep one const array of allowed values
  and derive the union from it with typeof and an index, so the data and the type can't drift."*
- *"Assertion versus guard: an assertion says trust me now and shifts the risk to runtime; a guard proves
  it at runtime and the compiler rewards the proof with narrowing. Default to guards at data boundaries."*

## Check Yourself
1. Does `value as HTMLInputElement` change `value` at runtime? What exactly does it change?
2. When must you avoid the angle-bracket assertion form, and why?
3. Match the guard to the target: a primitive, a class instance, a property's presence on an
   object-shaped union.
4. In an exhaustive switch over a discriminated union, what makes the `never` default line error, and why
   is that valuable?
5. What type does `typeof ROLES[number]` produce for `const ROLES = ["admin", "user"] as const`, and what
   would it produce without `as const`?

**Answers:** (1) No runtime change at all — it changes only the compiler's belief about the type; wrong
belief = runtime bug. (2) Anywhere near JSX — `<T>` parses as a tag, so `as` is the safe universal form.
(3) `typeof` for primitives, `instanceof` for class instances, `in` for property presence. (4) Adding a
new union arm leaves the default reachable, and a reachable value cannot be assigned to `never` — the
error lists every switch that must handle the new arm. (5) `"admin" | "user"`; without `as const` the
array widens to `string[]`, so the index type is just `string`.

## Summary
- Assertions (`as T`) overrule the compiler and convert nothing at runtime; wrong assertion = clean
  compile, runtime crash. `as unknown as T` is the smell-flagged escape hatch; prefer `as` over `<T>`
  near JSX.
- Guards narrow with runtime proof: typeof / instanceof / in / truthiness; type predicates (`x is Book`)
  package a guard for reuse.
- Flagship: discriminated union + exhaustive switch + `never` default — extending the union breaks every
  stale switch at compile time.
- `as const` = narrowest literal type + readonly; `typeof ARR[number]` derives a union from data — the
  config-object pattern, the no-emit alternative to enums.
- Assertion = trust me now; guard = prove it at runtime. Guards win at data boundaries.

## Resources
- [Narrowing — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/narrowing.html)
- [Everyday Types: type assertions and as const (typescriptlang.org)](https://www.typescriptlang.org/docs/handbook/2/everyday-types.html)
- [typeof operator (MDN)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/typeof)
