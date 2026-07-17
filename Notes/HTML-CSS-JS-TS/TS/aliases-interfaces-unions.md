# Type Aliases, Interfaces, and Unions

## Learning Objectives
- Define user-defined types with `interface` and with `type`.
- Use type aliasing to name any type — object shapes, primitives, unions, function types.
- Compare interfaces and type aliases and apply a practical default for choosing between them.
- Build union types, including literal unions, and explain what the compiler allows before narrowing.
- Explain structural typing and how it differs from nominal typing in C# or Java.

## Why This Matters
Inline object annotations stop scaling the moment two functions share a shape. Named types — interfaces
and aliases — are how a codebase talks about its domain: `User`, `Order`, `ID`. Unions are TypeScript's
signature move, modeling "one of these, and the compiler makes you prove which" — a capability C# and
Java simply lack. And "interface vs type?" plus "structural vs nominal typing?" are two of the highest-
frequency TypeScript interview questions in circulation.

## The Concept

### Two ways to name a shape
```ts
interface User {
  id: number;
  name: string;
}

type Point = { x: number; y: number };

const u: User = { id: 1, name: "Ada" };
const p: Point = { x: 3, y: 7 };
```

For a plain object shape these are interchangeable: same checking, same erasure, zero runtime cost.
The differences appear at the edges (below).

### Aliases name ANY type, not just objects
`type` creates a name for *any* type expression — that is its superpower:

```ts
type ID = string | number;                    // a union
type Celsius = number;                        // a primitive, for intent
type Comparator = (a: ID, b: ID) => number;   // a function type
type Tags = string[];                         // an array
```

An interface can only describe object/function shapes; it cannot say "string or number." When you need to
name a union, an alias is the only tool.

### Interface vs type alias — the canonical compare

| | `interface` | `type` alias |
|---|---|---|
| Object shapes | yes | yes |
| Unions, primitives, tuples, function types | no | yes |
| Extension | `extends` (also mergeable across declarations) | intersections with `&` |
| Declaration merging | yes — two same-name declarations merge | no — duplicate name is an error |
| Mapped/conditional forms | no | yes |

Declaration merging means two `interface User` blocks in scope silently combine into one — useful for
augmenting library types, surprising everywhere else. A practical default that most style guides land on:
**interface for public object contracts** (things other code implements or consumes), **type for
everything else** (unions, function types, compositions). This table is the canonical compare for these
notes; `casting-guards-asconst.md` revisits the question only to add what assertions and `as const` change.

### Union types
A union says a value is one of several types:

```ts
let id: string | number;
id = "a-7431";   // fine
id = 7431;       // fine

type OrderStatus = "pending" | "shipped" | "delivered";
let status: OrderStatus = "shipped";
let bad: OrderStatus = "returned";   // error: not assignable
```

Unions of **string literals** like `OrderStatus` are the idiomatic alternative to enums for closed sets
of values — plain strings at runtime, exact values checked at compile time (trade-offs vs enums in
`tuples-enums.md`). The rule that trips everyone: before narrowing, you may only use members **common to
every arm**. `id.toUpperCase()` errors while `id` might be a number; `id.toString()` is fine because both
arms have it. Proving which arm you hold — narrowing with guards — is the subject of
`casting-guards-asconst.md`.

Recognize on sight the union's sibling, the **intersection**: `type Employee = Person & Payroll` means
*both* shapes at once — all members of each. And a one-liner worth knowing: object *literals* assigned
directly get **excess property checks** — `const u: User = { id: 1, name: "Ada", age: 40 }` errors on
`age` even though structurally it is a superset.

### Structural typing
TypeScript compares **shapes, not names**. Two independently declared types with identical members are
fully interchangeable:

```ts
interface Point2D { x: number; y: number }
type Coord = { x: number; y: number };
const c: Coord = { x: 1, y: 2 };
const q: Point2D = c;   // fine — same shape, names never compared
```

C# and Java are **nominal**: a value's type is the name it was declared with, and two identical classes
are unrelated. In TypeScript, anything with the right members *is* the type — which is why a plain object
literal can satisfy an interface no class ever implemented. This is the single biggest mental-model shift
for developers arriving from C#.

## Say It in an Interview
- *"I define domain shapes with interface User or type Point — both describe object shapes, both erase at
  compile time."*
- *"A type alias can name any type at all — unions, primitives, function types — like type ID = string or
  number; interfaces are limited to object shapes, so unions always need an alias."*
- *"Both handle object shapes; interfaces add extends and declaration merging, aliases add unions and
  mapped forms. My default: interface for public object contracts, type for everything else."*
- *"A union like string-or-number only lets me touch members common to all arms until I narrow it with a
  guard — the compiler forces me to prove which arm I actually hold."*
- *"TypeScript is structurally typed: it compares shapes, not names, so two identical shapes declared
  separately are interchangeable — unlike C# or Java, where the declared name is the identity."*

## Check Yourself
1. Write a type that says "an order ID is a string or a number." Could an interface express that?
2. Name two things only interfaces do and two things only aliases do.
3. Why does `id.toUpperCase()` fail on `string | number`, but `id.toString()` succeed?
4. What is the string-literal-union pattern, and what does it replace?
5. Your teammate declares `interface Book { title: string }` in one file and you declare an identical
   `type Novel` in another. Can a `Novel` be passed where a `Book` is expected? Why?

**Answers:** (1) `type ID = string | number;` — no: interfaces cannot express unions, only object/function
shapes. (2) Interfaces: `extends` plus declaration merging; aliases: naming unions/primitives/function
types plus mapped/conditional forms. (3) Before narrowing, only members present on *every* arm are
allowed; numbers lack `toUpperCase`, but both arms have `toString`. (4) A union of exact strings, e.g.
`"pending" | "shipped"`, as a closed value set — the plain-JS alternative to an enum. (5) Yes — structural
typing compares members, not declaration names; identical shape means interchangeable.

## Summary
- `interface X { ... }` and `type X = { ... }` both name object shapes at zero runtime cost.
- Aliases name ANY type — unions (`type ID = string | number`), primitives, function types.
- Compare: interfaces get `extends` + declaration merging; aliases get unions/mapped forms. Default:
  interface for public contracts, type for the rest.
- Unions restrict you to common members until narrowed; literal unions (`"pending" | "shipped"`) are the
  idiomatic enum alternative.
- Structural typing: shape is identity — identical shapes are interchangeable, unlike nominal C#/Java.
- Related: intersections (`&`) combine shapes; object literals face excess property checks.

## Resources
- [Everyday Types: interfaces and type aliases (typescriptlang.org)](https://www.typescriptlang.org/docs/handbook/2/everyday-types.html)
- [Object Types — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/objects.html)
- [Type Compatibility (structural typing) — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/type-compatibility.html)
