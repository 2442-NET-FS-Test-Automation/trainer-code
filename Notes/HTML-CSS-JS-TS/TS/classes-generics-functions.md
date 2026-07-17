# Classes, Typed Functions, and Generics

## Learning Objectives
- Write TypeScript classes: typed fields, access modifiers, parameter properties, readonly,
  implements, and extends/super.
- Type functions fully: parameters, returns, optional/default parameters, function type expressions,
  and callbacks.
- Write and read basic generics: generic functions, constraints, and generic interfaces/classes.
- Read nested generic signatures — Array<T>, Promise<T>, Map<K, V> — out loud.

## Why This Matters
Classes are where developers coming from C# feel at home in TypeScript — and where the small differences
(parameter properties, structural `implements`, two kinds of private) bite. Generics are the opposite:
the feature people avoid until a signature like `Promise<Map<string, Order[]>>` stops them cold. Being
able to *read* generics fluently is the difference between using typed libraries comfortably and fighting
them, and "why generics instead of any?" is a staple interview question.

## The Concept

### Classes with types
Everything JS classes do, plus annotations and compile-time access control:

```ts
interface Shippable { ship(address: string): void }

class Order implements Shippable {
  readonly id: number;
  protected status = "pending";              // inferred string, visible to subclasses
  constructor(id: number, private items: string[]) {   // parameter property: declares AND assigns
    this.id = id;
  }
  ship(address: string): void { this.status = "shipped"; }
}

class RushOrder extends Order {
  constructor(id: number, items: string[]) { super(id, items); }
  ship(address: string): void { super.ship(address); /* then expedite */ }
}
```

The pieces: `public` (default) / `private` / `protected` control access at compile time; **parameter
properties** — a modifier on a constructor parameter like `private items: string[]` — declare the field
and assign it in one stroke, removing three lines of boilerplate per field; `readonly` allows assignment
only at declaration or in the constructor; `implements` checks the class against an interface (structural
— the interface adds no code, only a conformance check); `extends` + `super` work as in JS. One-liner to
know: TS `private` is erased and only compile-time-enforced, while JS `#field` is enforced at runtime —
`(order as any).items` sneaks past the first, not the second.

### Typing functions
```ts
function fee(days: number, rate: number = 0.25, note?: string): string {
  return `${(days * rate).toFixed(2)}${note ? " - " + note : ""}`;
}

type Formatter = (amount: number) => string;          // function type expression

function printTotals(orders: number[], format: Formatter): void {
  orders.forEach((o) => console.log(format(o)));      // typed callback
}
```

Parameters and return get colon annotations; `?` marks optional parameters (they type as
`type | undefined` inside), `= value` gives a default (and infers the type). A **function type
expression** `(a: number) => string` describes a function as a value — the tool for typing callbacks, so
`printTotals` rejects any `format` that does not take a number and return a string.

### Generics: keeping the type connection
The problem `any` cannot solve — relate the input type to the output type:

```ts
function firstAny(arr: any[]): any { return arr[0]; }
function first<T>(arr: T[]): T { return arr[0]; }

const a = firstAny(["x", "y"]);   // a: any — string-ness is lost, no checking downstream
const b = first(["x", "y"]);      // b: string — T inferred as string and PRESERVED
```

`<T>` declares a **type parameter** — a placeholder filled in per call, usually by inference. With `any`,
information flows in and dies; with `T`, whatever goes in comes back out still typed. Interfaces and
classes take type parameters too — recognize the shape:

```ts
interface Repository<T> {
  getById(id: number): T | undefined;
  add(item: T): void;
}
class InMemoryRepository<T> implements Repository<T> { /* ... */ }
```

**Constraints** with `extends` let the body use members of `T` by demanding callers supply them:

```ts
function byId<T extends { id: number }>(items: T[], id: number): T | undefined {
  return items.find((i) => i.id === id);   // .id is safe: every T has it
}
```

Two pocket rules: a generic beats a union when the *caller's* type must flow through to the result (a
union return forces every caller to narrow); and type parameters can have defaults (`<T = string>`) —
name-level knowledge.

### Reading nested generic signatures out loud
The built-ins you will read daily are generic: `Array<T>`, `Promise<T>`, `Map<K, V>`. The skill is
verbalizing them inside-out:

| Signature | Read as |
|---|---|
| `Promise<Order[]>` | "a promise that resolves to an array of Orders" |
| `Map<string, Order[]>` | "a map from string keys to arrays of Orders" |
| `Promise<Map<string, Order[]>>` | "a promise resolving to a map from strings to Order arrays" |

If you can say it, you can use it — the sentence tells you exactly what `await` and `.get()` will yield.

## Say It in an Interview
- *"TypeScript classes add typed fields, compile-time access modifiers, and parameter properties — a
  modifier on a constructor parameter declares and assigns the field in one line; implements is a
  structural conformance check against an interface."*
- *"I annotate parameters and returns, mark optionals with a question mark or give defaults, and type
  callbacks with function type expressions like (amount: number) => string."*
- *"A generic like first<T>(arr: T[]): T preserves the relationship between input and output — with any,
  the type dies at the boundary and everything downstream is unchecked; T extends lets me constrain what
  callers can pass so the body can use those members."*
- *"I read nested generics inside-out: Promise<Map<string, Order[]>> is a promise resolving to a map from
  strings to arrays of Orders."*

## Check Yourself
1. What two things does `constructor(private repo: BookRepo)` do that a bare parameter does not?
2. TS `private` vs JS `#private` — where is each enforced?
3. Write the function type for a callback taking a string and returning a boolean.
4. Why does `first<T>(arr: T[]): T` beat `first(arr: any[]): any` for the *caller*?
5. In `<T extends { id: number }>`, what does the constraint buy the function body, and what does it
   demand of callers?

**Answers:** (1) Declares a class field named `repo` and assigns the argument to it — declaration plus
assignment in one. (2) TS `private` at compile time only (erased); `#field` at runtime by the JS engine.
(3) `(s: string) => boolean`. (4) The result keeps the element type — `first(["x"])` is a `string`, so
downstream stays checked; `any` erases it and disables checking. (5) The body may safely use `.id`;
callers must pass elements that at least have a numeric `id`.

## Summary
- Classes: typed fields, `public`/`private`/`protected` (compile-time), parameter properties for
  declare-and-assign, `readonly`, structural `implements`, `extends`/`super`; JS `#private` is the
  runtime-enforced cousin.
- Functions: annotate parameters and returns; `?` optional, `=` default; function type expressions
  (`(a: number) => string`) type callbacks.
- Generics preserve type flow from input to output; `any` destroys it. Constrain with
  `<T extends { id: number }>`; `Repository<T>` is the generic-interface shape to recognize.
- Read nested generics inside-out: `Promise<Map<string, Order[]>>` = promise of a map from strings to
  Order arrays.

## Resources
- [Classes — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/classes.html)
- [Generics — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/generics.html)
- [More on Functions — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/functions.html)
