# tsconfig.json: Configuring the TypeScript Compiler

## Learning Objectives
- Explain what tsconfig.json is for and how it changes what `tsc` does.
- Recognize and explain the key compilerOptions: target, module, rootDir/outDir, strict, sourceMap,
  esModuleInterop, include/exclude.
- Explain the `strict` flag as an umbrella and name what strictNullChecks and noImplicitAny enforce.
- Shape a config for a project's needs: a browser-app layout vs a node-script layout.

## Why This Matters
Run `tsc app.ts` and you get one file compiled with defaults. Real projects have dozens of files, a source
folder, an output folder, and opinions about how strict the checking should be — all of that lives in
`tsconfig.json`. It is the first file you read to understand any TypeScript project, and questions like
"what does strict actually turn on?" are standard interview probes for whether you have configured a
project yourself or only worked inside someone else's.

## The Concept

### What tsconfig.json is
A `tsconfig.json` in a folder **marks that folder as the root of a TypeScript project**. With it present,
a bare `tsc` (no file arguments) compiles the whole project according to the config; without file args and
without a config, `tsc` has nothing to do. The file holds `compilerOptions` (how to check and emit) plus
`include`/`exclude` (which files are in the project). Generate a starter with every option documented in
comments via `npx tsc --init`.

```json
{
  "compilerOptions": {
    "target": "es2020",
    "module": "commonjs",
    "rootDir": "./src",
    "outDir": "./dist",
    "strict": true,
    "sourceMap": true,
    "esModuleInterop": true
  },
  "include": ["src/**/*"],
  "exclude": ["node_modules"]
}
```

### The options you will actually touch

| Option | One line |
|---|---|
| `target` | which JS version to emit (`es5` ... `es2022`) — older targets get more down-leveling |
| `module` | module system for the emit: `commonjs` (classic node) vs `esnext`/`nodenext` (ESM) |
| `rootDir` | where source `.ts` files live (typically `src`) |
| `outDir` | where emitted `.js` goes (typically `dist`) — keeps output out of your source tree |
| `strict` | umbrella switch for the whole strict-checking family (below) |
| `sourceMap` | emit `.js.map` files so debuggers step through your `.ts`, not the emitted JS |
| `esModuleInterop` | makes `import express from "express"` work with CommonJS packages |
| `include` / `exclude` | glob arrays defining project membership (exclude `node_modules`, `dist`) |

Recognize this compiler error on sight — current `tsc` versions **require an explicit `rootDir` when
`outDir` is set**:

```
error TS5011: Option 'outDir' requires an explicit 'rootDir' to compute the output structure...
```

The fix is always the same: add `"rootDir": "./src"` (or wherever the sources are) beside `outDir`.

### The strict flag: an umbrella
`"strict": true` enables a family of checks at once. The two you must be able to name:

- **strictNullChecks** — `null` and `undefined` stop being assignable to everything. `let name: string =
  maybeName()` errors if the function can return `undefined`; you must handle the absent case. This kills
  the largest single class of JS runtime crashes ("cannot read properties of undefined").
- **noImplicitAny** — every parameter must have a type, from an annotation or from inference. Untyped
  parameters silently becoming `any` is how type safety leaks out of a codebase.

Friends in the family (name-level): `strictFunctionTypes`, `strictPropertyInitialization`,
`alwaysStrict`. New projects turn `strict` on **day one**, because the cost is near zero when there is no
code yet. Retrofitting it onto a mature codebase surfaces hundreds of pre-existing loose spots at once —
teams end up migrating file-by-file for weeks. Cheap now, expensive later.

### Shaping a config per project
Two common shapes, differing mainly in target/module and what the output is for:

```json
// browser app (output handed to a bundler or script tags)
{ "compilerOptions": { "target": "es2017", "module": "esnext",
    "rootDir": "./src", "outDir": "./dist", "strict": true, "sourceMap": true } }
```

```json
// node script/tool (run directly with node)
{ "compilerOptions": { "target": "es2022", "module": "commonjs",
    "rootDir": "./src", "outDir": "./dist", "strict": true, "esModuleInterop": true } }
```

The browser shape targets what shipping browsers support and leaves module wiring to a bundler; the node
shape can target modern JS (you control the runtime) and needs `esModuleInterop` for the npm ecosystem.
For sharing options across many projects, a config can `extends` a base file — know the keyword exists.

## Say It in an Interview
- *"tsconfig.json marks the project root and drives tsc — with it in place a bare tsc compiles the whole
  project by its compilerOptions instead of needing file arguments."*
- *"The ones I touch most: target for the emitted JS version, module for the module system, rootDir and
  outDir to separate source from emit, sourceMap for debugging, esModuleInterop for CommonJS imports, and
  include/exclude for project membership."*
- *"strict is an umbrella — it turns on strictNullChecks, so null and undefined must be handled
  explicitly, and noImplicitAny, so parameters can't silently become any. I enable it day one because
  retrofitting strict onto an existing codebase means fixing every loose spot at once."*
- *"For a browser app I target what browsers run and let the bundler own modules; for a node tool I can
  target modern JS and emit commonjs or ESM to match how it will be executed."*

## Check Yourself
1. What two things does the presence of a tsconfig.json change about running `tsc`?
2. You set `outDir` and get error TS5011. What is the compiler asking for, and what is the fix?
3. Name the two headline checks inside `strict` and the runtime bug class each one prevents or catches.
4. Why is enabling `strict` cheaper on day one than in month six?
5. Which option makes the debugger step through your `.ts` source instead of the emitted `.js`?

**Answers:** (1) The folder becomes the project root, and a bare `tsc` with no file arguments compiles the
whole project using the config's options. (2) An explicit `rootDir` so it can compute the output folder
structure; add `"rootDir": "./src"` next to `outDir`. (3) strictNullChecks — stops null/undefined being
assignable everywhere, preventing "cannot read properties of undefined"; noImplicitAny — forces parameters
to be typed or inferred, preventing silent `any` leaks. (4) With no code there is nothing to fix; later,
every existing loose spot errors at once and must be migrated. (5) `sourceMap` — it emits `.js.map` files
linking emitted lines back to source.

## Summary
- tsconfig.json marks the project root, lets a bare `tsc` compile everything, and holds compilerOptions.
- Core options: target, module, rootDir/outDir (TS5011 if outDir lacks a rootDir), strict, sourceMap,
  esModuleInterop, include/exclude.
- `strict` is an umbrella: strictNullChecks + noImplicitAny and friends — on from day one; retrofitting
  is the expensive path.
- Different projects, different shapes: browser apps target browser JS and defer modules to a bundler;
  node tools target modern JS and match node's module system.
- `tsc --init` generates a fully commented starter; `extends` shares a base config across projects.

## Resources
- [What is a tsconfig.json (typescriptlang.org)](https://www.typescriptlang.org/docs/handbook/tsconfig-json.html)
- [TSConfig Reference — every option explained](https://www.typescriptlang.org/tsconfig/)
- [The Basics — TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/2/basic-types.html)
