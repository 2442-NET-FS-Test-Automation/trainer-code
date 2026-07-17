# Basic, Special, and Object Types

## Learning Objectives
- Annotate the primitive types (number, string, boolean) and know when inference makes annotation
  unnecessary — and when to annotate anyway.
- Explain the special types — any, unknown, void, never — and null/undefined under strictNullChecks.
- Write inline object type annotations with optional and readonly properties, and both array syntaxes.
- Give the full any-vs-unknown comparison on demand.

## Why This Matters
Every other TypeScript feature — interfaces, generics, guards — is built on top of these atoms. The
special types are where the real judgment lives: `any` is the escape hatch that quietly deletes your type
safety, and `unknown` is its disciplined replacement. "What's the difference between any and unknown?" is
one of the most common TypeScript interview questions asked, precisely because the answer reveals whether
you treat the type system as a tool or as an obstacle.

## The Concept

### Primitives and inference
The three workhorse primitives annotate with a colon after the name:

```ts
let age: number = 34;
let title: string = "The Left Hand of Darkness";
let checkedOut: boolean = false;
```

But TypeScript **infers** types from initializers, so `let age = 34` is already a `number` — the
annotation adds nothing. The working rule: **let inference carry local variables; annotate function
boundaries.** Parameters cannot be inferred from a call site that doesn't exist yet, and an explicit
return type turns a function into a checked contract instead of whatever the body happens to produce:

```ts
function lateFee(daysLate: number, dailyRate: number): number {
  return daysLate * dailyRate;
}
```

### The special types

| Type | Meaning | Reach for it when |
|---|---|---|
| `any` | opt OUT of checking entirely | last resort: migration shims, hopeless library types |
| `unknown` | could be anything — must narrow before use | untyped input: JSON, catch clauses, external data |
| `void` | function returns nothing useful | return type of side-effect functions |
| `never` | this value cannot exist | unreachable code, exhaustiveness checks |

`any` doesn't just skip checks on one variable — it **spreads**: every property access on an `any` is
`any`, every value computed from it is `any`, and the compiler stays silent through all of it. One `any`
at a fetch boundary can silently untype an entire call chain. `unknown` is the safe version: it accepts
any value, but you can do **nothing** with it until you narrow it.

```ts
function handle(data: unknown) {
  data.toUpperCase();                    // error: 'data' is of type 'unknown'
  if (typeof data === "string") {
    data.toUpperCase();                  // fine — narrowed to string
  }
}
// vs any: (data as any).toUpperCase() compiles happily and crashes at runtime on a number
```

`never` shows up as the return type of functions that always throw, and as the type left over when a
union has been fully narrowed — which makes it the engine of exhaustiveness checks (worked example in
`casting-guards-asconst.md`).

Under `strictNullChecks` (see `tsconfig.md`), `null` and `undefined` are their own types and are **not**
assignable to `string`, `number`, etc. A value that might be absent must say so — `string | undefined` —
and callers must handle the absent arm before using it. That is the whole feature: absence becomes
visible in the signature instead of exploding at runtime.

### Object types, optional, readonly, arrays
Object shapes can be annotated inline (naming them comes in `aliases-interfaces-unions.md`):

```ts
let point: { x: number; y: number } = { x: 3, y: 7 };

let member: { id: number; email: string; nickname?: string; readonly joined: string } = {
  id: 42, email: "kim@example.com", joined: "2024-05-01",
};
member.nickname = "K";        // fine — optional property, type string | undefined when read
member.joined = "2025-01-01"; // error: cannot assign to 'joined' because it is read-only
```

`?` marks a property that may be missing (its read type gains `| undefined`); `readonly` blocks
reassignment after creation — a compile-time promise only, erased like all types. Arrays have two
equivalent spellings you must recognize on sight: `number[]` and `Array<number>` are the same type; the
bracket form dominates in the wild, the generic form appears in nested signatures like
`Array<{ id: number }>`. One trap: `string` (primitive) vs `String` (JS wrapper object) — always annotate
with lowercase primitives; the capitalized wrappers are almost never what you mean.

## Say It in an Interview
- *"The primitives are number, string, and boolean; I let inference type initialized locals and reserve
  annotations for function parameters and return types, where they form the checked contract."*
- *"any opts a value out of type checking entirely and it spreads through everything computed from it;
  unknown accepts anything too, but the compiler blocks every use until you narrow it — so unknown is the
  safe default for external data and any is a last resort."*
- *"void means a function returns nothing useful; never means a value can't exist at all — it's what's
  left after exhaustive narrowing, which is why it powers exhaustiveness checks."*
- *"With strictNullChecks on, null and undefined stop being assignable to everything — a maybe-absent
  value must be typed string-or-undefined and handled before use, which surfaces absence at compile time."*

## Check Yourself
1. `let count = 0;` — what is count's type, and why would you skip the annotation here but not on a
   function parameter?
2. Both `any` and `unknown` accept every value. What is the difference on the *use* side?
3. Why does one `any` at an API boundary endanger more than that one variable?
4. In `{ nickname?: string }`, what type does `nickname` have when you read it, and why?
5. What is the difference between `number[]` and `Array<number>`, and between `string` and `String`?

**Answers:** (1) `number`, by inference from the initializer; parameters have no initializer to infer
from, so boundaries need explicit types. (2) `any` can be used freely with no checks; `unknown` cannot be
used at all until narrowed with a guard. (3) `any` propagates — everything derived from it is also `any`,
silently untyping the downstream call chain. (4) `string | undefined` — optional means possibly missing,
so the compiler forces the undefined case into view. (5) None — they are the same array type, two
spellings; `string` is the primitive (use it), `String` is the runtime wrapper object (almost never
intended).

## Summary
- Primitives: `number`, `string`, `boolean`; inference covers initialized locals — annotate function
  boundaries.
- `any` = opt out, and it spreads; `unknown` = accepts anything but blocks use until narrowed — the safe
  choice for external data.
- `void` = returns nothing; `never` = cannot exist, the engine of exhaustiveness checking.
- strictNullChecks makes absence explicit: `string | undefined` must be handled before use.
- Inline object types support `?` (optional) and `readonly`; `number[]` and `Array<number>` are the same
  type; lowercase `string`, never `String`.

## Resources
- [Everyday Types — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/everyday-types.html)
- [Object Types — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/objects.html)
- [Narrowing — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/narrowing.html)
