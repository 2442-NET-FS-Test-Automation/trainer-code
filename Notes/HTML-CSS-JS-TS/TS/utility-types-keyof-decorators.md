# Utility Types, keyof, and Decorators

## Learning Objectives
- Apply the built-in utility types — Partial, Required, Readonly, Pick/Omit, Record — to derive types
  instead of duplicating them.
- Use keyof, alone and with generics, for compile-time-safe property access.
- Explain what decorators are, read the canonical class/method examples, and say where they appear in
  real frameworks.

## Why This Matters
A codebase that hand-writes `User`, `UserUpdate`, `UserView`, and `UserSummary` as four separate types
has four chances to drift. Utility types derive the last three from the first, so one edit propagates
everywhere — this is the difference between *using* TypeScript and *leveraging* it. keyof powers the
signatures you meet in every typed library, and decorators are the syntax behind the `@`-annotations
that dominate Angular, NestJS, and validation libraries — you will read them long before you write one.

## The Concept

### Utility types: deriving instead of duplicating
Utility types are built-in generic transformers: give them a type, get a systematically modified type
back. The working set, with `interface User { id: number; name: string; email: string }` as the base:

| Utility | One line |
|---|---|
| `Partial<T>` | every property optional — the natural type for update payloads |
| `Required<T>` | every property mandatory — undoes optionality |
| `Readonly<T>` | every property readonly — immutable views |
| `Pick<T, K>` | keep only the listed keys |
| `Omit<T, K>` | drop the listed keys |
| `Record<K, V>` | an object type with keys K and values V — typed maps |

The big four in action:

```ts
type UserUpdate = Partial<User>;            // { id?: number; name?: string; email?: string }
function patchUser(id: number, changes: UserUpdate) { /* merge only what was sent */ }

type UserView = Omit<User, "email">;        // public shape: { id: number; name: string }
type Credentials = Pick<User, "id" | "email">;

const config: Readonly<User> = { id: 1, name: "Ada", email: "ada@example.com" };
config.name = "Bea";                        // error: readonly

const usersByRole: Record<"admin" | "member", User[]> = { admin: [], member: [] };
```

`Partial` is the canonical update-DTO type: a PATCH-style payload legitimately sends any subset of
fields. `Pick`/`Omit` derive view models from a source of truth — when `User` gains a field, every
derived type updates itself. Recognize on sight two more: `ReturnType<F>` extracts a function type's
return type and `Parameters<F>` its parameter tuple — common when a library exports functions but not
the types they produce. The trade-off of heavy derivation: error messages and editor hovers show the
*expansion*, and a chain like `Partial<Omit<Pick<...>>>` can obscure what a type actually is — name
intermediate steps when hovers stop being readable.

### keyof: property names as a type
`keyof T` is the union of `T`'s property names as literal types:

```ts
type UserKey = keyof User;   // "id" | "name" | "email"
```

Alone it is a curiosity; combined with generics it is the flagship of type-safe property access:

```ts
function pluck<T, K extends keyof T>(obj: T, key: K): T[K] {
  return obj[key];
}

const u: User = { id: 1, name: "Ada", email: "ada@example.com" };
const n = pluck(u, "name");     // n: string — T[K] resolves to the actual property type
pluck(u, "nmae");               // compile error: typo caught, not a runtime undefined
```

Read the signature slowly once: `K extends keyof T` means the key must be one of `T`'s real property
names, and the return type `T[K]` is an **indexed access** — the type *of that property*. Misspelled keys
become compile errors and the result is precisely typed per key. Recognize on sight the companion:
**type-level `typeof`** lifts a value into type space, so `keyof typeof config` gets the key union of a
plain object — the same move `casting-guards-asconst.md` uses to derive unions from `as const` data.
Name-level awareness: **mapped types** (`{ [K in keyof T]: ... }`) are the machinery utility types are
built from, and **template literal types** build string types from unions.

### Decorators: annotations that run
A decorator is a **function** attached to a class or member with `@name`, executed **when the class is
defined** (not per instance) to observe or replace what it decorates:

```ts
function sealed(ctor: Function) {          // class decorator: runs once at definition time
  Object.seal(ctor);
  Object.seal(ctor.prototype);
}

@sealed
class OrderService {
  process(id: number) { /* ... */ }
}
```

The other canonical example is method logging — a decorator that wraps a method to log calls. Where you
will actually meet decorators is **framework metadata**: dependency injection (`@Injectable()`), routing
(`@Get("/orders")`), and validation (`@IsEmail()`) all use decorators to register classes and members
with a framework at definition time. For most developers the honest skill level is: read them fluently,
know they are functions running at class-definition time, write one only when building framework-like
infrastructure. One versioning line to know: TypeScript 5 implements the standardized TC39 decorators,
while older codebases use the legacy `experimentalDecorators` flag — the two dialects have different
function signatures, so check which one a project uses before writing any.

## Say It in an Interview
- *"I derive types instead of duplicating them: Partial<User> for update DTOs, Pick and Omit for view
  models, Record for typed maps — so when the source type changes, every derived type follows
  automatically."*
- *"keyof T is the union of T's property keys; the classic use is pluck with K extends keyof T returning
  T[K] — misspelled keys fail at compile time and each key returns its exact property type."*
- *"Decorators are functions attached with an at-sign that run at class-definition time to register or
  wrap the class or member — they're how frameworks do DI, routing, and validation annotations."*
- *"TypeScript 5 ships the standardized TC39 decorators; the legacy experimentalDecorators dialect is
  still common in older codebases, and the two aren't signature-compatible."*

## Check Yourself
1. Which utility type fits a PATCH-style update payload, and what does it do to the source type?
2. You need a `User` without its `email` for public API responses. Which utility, and why is deriving it
   better than declaring a fresh type?
3. In `pluck<T, K extends keyof T>(obj: T, key: K): T[K]`, what do `K extends keyof T` and `T[K]` each
   guarantee?
4. When does a class decorator execute — per instance, per call, or at definition time?
5. What is `Record<"admin" | "member", User[]>` in plain words?

**Answers:** (1) `Partial<T>` — makes every property optional, matching a payload that sends any subset.
(2) `Omit<User, "email">` — a fresh type drifts silently when `User` changes; the derived type updates
itself. (3) `K extends keyof T`: the key argument must be an actual property name of `T`; `T[K]`: the
return is the exact type of that property, not a loose union. (4) Once, at class-definition time — not
per instance or call. (5) An object type whose keys are exactly "admin" and "member", each mapped to an
array of Users.

## Summary
- Utility types transform existing types: Partial (update DTOs), Required, Readonly, Pick/Omit (view
  models), Record (typed maps); ReturnType/Parameters extract from function types.
- Derivation beats duplication — one source of truth, derived types follow changes; name intermediate
  types when hover-expansions get unreadable.
- `keyof T` = union of property names; `K extends keyof T` + `T[K]` = compile-time-safe property access;
  type-level `typeof` lifts values into type space.
- Mapped types are the underlying machinery (name-level); template literal types exist (name-only).
- Decorators: functions run at class-definition time to register/wrap classes and members — framework
  territory (DI, routing, validation); TC39-standard vs legacy experimentalDecorators dialects differ.

## Resources
- [Utility Types — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/utility-types.html)
- [Keyof Type Operator — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/keyof-types.html)
- [Decorators — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/decorators.html)
