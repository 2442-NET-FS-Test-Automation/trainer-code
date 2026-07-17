# Why TypeScript: The Superset, the Payoff, and the Toolchain

## Learning Objectives
- Compare TypeScript to JavaScript: a superset adding static types that are checked, then erased.
- Weigh why teams adopt TypeScript — and what it costs.
- Run the transpile-and-run loop: install `tsc`, compile a `.ts` file, run the emitted `.js` with node.
- Set up a minimal standalone TypeScript workflow with no framework involved.

## Why This Matters
JavaScript happily lets you call `book.titel` on an object that only has `title`, and tells you at 2 a.m.
in production. TypeScript is the industry's answer: the same language, plus a compile-time type checker
that catches wiring mistakes before the code ever runs. Angular requires it, React codebases have largely
converged on it, and "explain TypeScript vs JavaScript" is a near-guaranteed front-end interview opener.
Understanding *what the compiler adds and what it erases* is the foundation for everything else in TS.

## The Concept

### TypeScript is a superset of JavaScript
TypeScript = JavaScript + a **static type layer** checked at **compile time**. Every valid `.js` file is
already valid `.ts` — you can rename a file and start adding annotations gradually. The types exist only
for the compiler: when `tsc` transpiles, **all type information is erased** and what comes out is plain
JavaScript. There is no TypeScript runtime; browsers and node execute the emitted JS, never `.ts` itself.

```ts
// app.ts
function total(price: number, qty: number): number {
  return price * qty;
}
total("12", 3); // compile-time error: Argument of type 'string' is not assignable to 'number'
```

```js
// app.js — what tsc emits: the annotations are gone, nothing else changed
function total(price, qty) {
  return price * qty;
}
```

Erasure has a consequence worth saying out loud: types cannot check anything **at runtime**. Data arriving
from `fetch` or user input is whatever it is — the annotations only constrain the code you wrote.

### Why use it — and what it costs

| Payoff | What it looks like |
|---|---|
| Wiring mistakes caught early | typos, wrong argument order, missing properties — flagged at compile time |
| Editor superpowers | autocomplete on every object, safe rename-across-project, jump-to-definition |
| Self-documenting APIs | `function ship(order: Order): Receipt` reads as its own documentation |

Costs, honestly: a **build step** (you can no longer just refresh the browser on a raw file), a **learning
curve** (generics, narrowing, config), and **type friction** with untyped third-party libraries — you may
need community `@types/*` packages or have to write declarations yourself. For a ten-line script the
ceremony can outweigh the payoff; for a multi-file codebase with more than one author, it rarely does.

### The transpile-and-run process
Install the compiler globally, or (better for real projects) as a devDependency run through `npx`:

```
npm install -g typescript      # global: tsc available everywhere
npm install --save-dev typescript   # per-project: run as npx tsc
tsc app.ts                     # type-checks and emits app.js next to it
node app.js                    # run the emitted JavaScript
```

The loop is always: edit `.ts` → `tsc` → run the `.js`. One-step alternatives exist: `npx ts-node app.ts`
compiles and runs in one command, and recent node versions can strip types and run `.ts` files directly
(`node app.ts`) — both are conveniences over the same pipeline, not a different execution model.

### A minimal standalone workflow (no Angular, no React)
TypeScript needs no framework. A complete project from an empty folder:

```
mkdir price-tool && cd price-tool
npm init -y
npm install --save-dev typescript
npx tsc --init            # generates tsconfig.json (see tsconfig.md)
```

Write plain `.ts` files, compile the whole project with a bare `npx tsc` (no file arguments — the config
drives it), and run the output with `node`. This is the shape to reach for when practicing the language
itself: no bundler, no JSX, just the compiler and node.

One default that surprises people: **type errors do not stop emit**. `tsc` reports the error *and still
writes the `.js`*, because the output is valid JavaScript either way; set `noEmitOnError` in
`tsconfig.md`'s option table to make errors block the build. And to place the browser correctly: a browser
never loads a `.ts` file — you ship the emitted JS (directly or via a bundler), same as always.

## Say It in an Interview
- *"TypeScript is a superset of JavaScript that adds static types checked at compile time. The types are
  erased when it transpiles, so what actually runs is plain JavaScript — every .js file is already valid
  TypeScript."*
- *"The payoff is catching wiring mistakes at compile time plus real autocomplete and safe refactoring;
  the costs are a build step, a learning curve, and friction with untyped libraries."*
- *"The loop is: write .ts, run tsc to type-check and emit .js, run that with node — or one-step it with
  ts-node. Browsers only ever see the emitted JavaScript."*
- *"Outside a framework it's just npm install typescript, tsc --init, plain .ts files, compile with tsc,
  run with node — no bundler required to learn or use the language."*

## Check Yourself
1. In one sentence: what does TypeScript add to JavaScript, and when does that addition disappear?
2. Why is every `.js` file automatically a valid `.ts` file, and why is the reverse not true?
3. You see a type error in the terminal but `app.js` was still written. Is that a bug? What option
   changes it?
4. Name the three commands that take a fresh folder to "running compiled TypeScript in node."
5. Can a type annotation reject bad data coming back from a `fetch` call at runtime? Why or why not?

**Answers:** (1) A static type layer checked at compile time; it is erased on transpile, so none of it
exists at runtime. (2) TS is a superset — all JS syntax is legal TS; but TS adds syntax (annotations,
interfaces, enums) that a JS engine cannot parse. (3) Not a bug — errors do not block emit by default;
`noEmitOnError: true` makes them block. (4) `npm install --save-dev typescript`, `npx tsc --init`, then
`npx tsc` + `node out.js` to compile and run. (5) No — types are erased; runtime data must be validated
with actual code (guards, validators), because annotations only constrain code the compiler saw.

## Summary
- TypeScript = JavaScript + static types, checked at compile time, **erased** at transpile time.
- Adopt it for early error detection, autocomplete/refactoring, and self-documenting signatures; budget
  for a build step, a learning curve, and untyped-library friction.
- Core loop: `tsc app.ts` → `node app.js`; `ts-node` or node's type-stripping collapse it to one step.
- Standalone workflow: `tsc --init`, plain `.ts` files, compile with `tsc`, run the emitted JS with node.
- Errors do not stop emit by default (`noEmitOnError`); browsers only ever run the emitted JavaScript.

## Resources
- [TypeScript for the New Programmer (typescriptlang.org)](https://www.typescriptlang.org/docs/handbook/typescript-from-scratch.html)
- [The Basics — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/basic-types.html)
- [Download and install TypeScript](https://www.typescriptlang.org/download/)
