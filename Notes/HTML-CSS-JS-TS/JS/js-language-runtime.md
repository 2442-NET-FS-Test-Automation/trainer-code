# JavaScript, the Language and Its Runtimes: Browser, Node.js, and npm

## Learning Objectives
- Describe what JavaScript is: a dynamically typed, JIT-compiled scripting language that runs in
  browsers and in Node.js, is single-threaded, and makes web pages interactive.
- Explain what Node.js is, run a script with it, and name what Node adds and what it lacks.
- Use npm to initialize a project and install packages, and read a `package.json` — including the
  dependencies/devDependencies split, `node_modules`, `npx`, and semver range prefixes.

## Why This Matters
Every front end you will ever ship runs on JavaScript, and almost all of its tooling — bundlers, test
runners, TypeScript itself — runs on Node.js and is delivered through npm. Before writing a line of
application code you will type `npm install`, and the first interview question in any front-end loop is
some form of "what *is* JavaScript, and how does Node relate to the browser?" Knowing the runtime story
cold separates people who use the tools from people who understand them.

## The Concept

### What JavaScript is
JavaScript is a **dynamically typed** scripting language: variables carry no declared type, and a value's
type is checked at runtime, not compile time. Historically it was interpreted; modern engines (V8 in
Chrome and Node, SpiderMonkey in Firefox) **JIT-compile** it — they compile hot code paths to machine
code on the fly, so "interpreted language" is only half true today. Its original and still-primary job is
making web pages interactive: reacting to clicks, validating forms, fetching data, and rewriting the page
without a reload.

Execution is **single-threaded with an event loop**: one call stack runs your code, and asynchronous work
(timers, network calls) queues callbacks to run when the stack is free — depth on that model lives in
`promises-async.md`. A naming note interviewers like: **ECMAScript** is the language standard (ES6/ES2015
and yearly editions after); *JavaScript* is the everyday name for implementations of it. And
**TypeScript** is a statically typed superset that compiles down to plain JavaScript — see
`../04-typescript/why-typescript-tooling.md`.

### Node.js: JavaScript outside the browser
Node.js embeds the **V8 engine** in a standalone runtime, so the same language runs on servers, build
machines, and your laptop with no browser involved. Running a script is one command:

```bash
node app.js
```

```js
// app.js
const fs = require("fs");                       // Node's file-system module
fs.writeFileSync("hello.txt", "written by JS"); // no browser could do this
console.log("platform:", process.platform);     // process = info about the running program
```

What Node **adds**: file-system access (`fs`), the `process` object (arguments, environment variables,
exit codes), networking servers, and the whole npm ecosystem. What Node **lacks**: everything
browser-shaped — there is no `document`, no `window`, no DOM at all. Code that manipulates the page (see
`dom-selection-manipulation.md`) only runs in a browser; code that touches files only runs in Node. The
trade-off cuts both ways: shared *logic* is portable between the two, but each side has APIs the other
will throw `ReferenceError` on.

### npm: the package manager
npm installs third-party packages and records what your project depends on. The core loop:

| Command | What it does |
|---|---|
| `npm init -y` | create a `package.json` manifest for the project |
| `npm install <pkg>` | download the package into `node_modules/`, record it in `package.json` |
| `npm install <pkg> --save-dev` | record it under `devDependencies` instead |
| `npm install` | recreate `node_modules/` from `package.json` (fresh clone, CI) |
| `npx <tool>` | run a package's CLI without a global install — one line worth knowing |

```json
{
  "name": "catalog-ui",
  "dependencies":    { "react": "^18.3.1" },
  "devDependencies": { "vite": "~5.4.0" }
}
```

**dependencies** are what the shipped app needs at runtime; **devDependencies** are build-and-test-time
tools (bundlers, linters, test runners). `node_modules/` is the installed tree — it is huge, machine-
generated, and always git-ignored; `package.json` (plus the lockfile) is what you commit. The trade-off
of the ecosystem: enormous reuse, but every `npm install` pulls in transitive dependencies you did not
audit — which is exactly why the manifest and lockfile matter.

**Semver ranges — recognize on sight:** in `"^18.3.1"` the caret accepts any compatible *minor/patch*
update (`18.x.x` but not `19`); in `"~5.4.0"` the tilde accepts *patch* updates only (`5.4.x`). A bare
`"18.3.1"` pins the exact version.

## Say It in an Interview
- *"JavaScript is a dynamically typed scripting language — types are checked at runtime — and modern
  engines JIT-compile it rather than purely interpret it. It runs single-threaded with an event loop, in
  the browser to make pages interactive and in Node.js everywhere else."*
- *"Node.js is the V8 engine packaged as a standalone runtime: `node app.js` runs a script, and you gain
  server-side APIs like the file system and `process` — but there's no DOM or `window`, because there's
  no page."*
- *"npm manages dependencies through `package.json`: runtime packages under `dependencies`, build tools
  under `devDependencies`, installed code in a git-ignored `node_modules`. A caret range takes minor and
  patch updates; a tilde takes patch only."*

## Check Yourself
1. "JavaScript is interpreted" — what is the more accurate modern statement, and which engine does
   Chrome and Node share?
2. Name two things Node.js gives you that the browser does not, and one thing the browser has that Node
   does not.
3. Where does a test runner like a linter or bundler belong in `package.json`, and why?
4. What versions can `"^4.2.1"` resolve to? And `"~4.2.1"`?
5. What is the relationship between JavaScript and ECMAScript?

**Answers:** (1) Modern engines JIT-compile JavaScript to machine code at runtime; Chrome and Node both
use V8. (2) Node adds file-system access (`fs`) and `process` (env vars, args); the browser has the DOM
(`document`, `window`), which Node lacks. (3) `devDependencies` — it is needed to build and test, not by
the shipped application at runtime. (4) Caret: any `4.x.y` with `x >= 2` (up to but not including 5.0.0);
tilde: `4.2.x` patch releases only. (5) ECMAScript is the written standard; JavaScript is the common name
for the language that implements it.

## Summary
- JavaScript: dynamically typed, JIT-compiled by modern engines, single-threaded with an event loop;
  born in the browser to make pages interactive.
- Node.js: V8 outside the browser — run scripts with `node app.js`; gains `fs`/`process`, loses
  DOM/`window`.
- npm: `npm init`, `npm install <pkg>`; `package.json` splits runtime `dependencies` from
  `devDependencies`; `node_modules` is generated and git-ignored; `npx` runs package CLIs directly.
- Semver on sight: `^` takes minor+patch, `~` takes patch only, bare version pins exactly.
- ECMAScript is the standard; TypeScript is the typed superset that compiles to JavaScript.

## Resources
- [JavaScript (MDN overview)](https://developer.mozilla.org/en-US/docs/Web/JavaScript)
- [Introduction to Node.js (nodejs.org)](https://nodejs.org/en/learn/getting-started/introduction-to-nodejs)
- [About npm (npm docs)](https://docs.npmjs.com/about-npm)
