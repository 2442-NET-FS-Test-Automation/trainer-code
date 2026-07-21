# Functional Components, JSX, and Your First Vite App

## Learning Objectives

- Describe and write a functional component: a function that returns UI.
- Read and write JSX, and explain how it integrates with plain JavaScript expressions.
- Compose small components into nested trees that model a real interface.
- Style components three ways: inline styles, CSS Modules, and an external stylesheet.
- Scaffold and run a React project from scratch with the Vite CLI.

## Why This Matters

Every React interface you will ever build is a tree of functions that return markup. Once you internalize
that a component is *just a function* and JSX is *just JavaScript with a nicer face*, the whole library
stops feeling magical. Interviewers lean on this hard: "what is a functional component," "what actually is
JSX," and "how would you start a new React app" are three of the most common opening questions, and a shaky
answer to any of them signals you have only copied tutorials. Getting comfortable here — a function in, an
element out, running live in a Vite dev server in under a minute — is the foundation the rest of React
stands on.

## The Concept

### A functional component is a function that returns UI

A functional component is a JavaScript function that returns a React element (JSX). Two rules make it a
component rather than an ordinary function: its name is **capitalized** (React treats lowercase tags as raw
HTML and capitalized tags as components), and it returns something renderable — JSX, a string, a number, or
`null`.

```tsx
function Greeting() {
  return <h1>Welcome to the library catalog</h1>;
}
```

That is a complete, valid component. You use it by writing it as a tag:

```tsx
function App() {
  return <Greeting />;
}
```

There is no class, no `render` method, no boilerplate. Components accept an input object called **props**
(covered in depth elsewhere) and return output. The mental model is a pure function: same props in, same UI
out.

### JSX is JavaScript in disguise

JSX looks like HTML sitting inside your code, but it is not HTML and it is not a string. It is syntax sugar
that a compiler rewrites into ordinary function calls. This:

```tsx
const element = <h1 className="title">Hello</h1>;
```

compiles down to roughly:

```ts
const element = React.createElement("h1", { className: "title" }, "Hello");
```

Because JSX *is* JavaScript, a few HTML habits change:

- Attributes use camelCase and JS names: `className` (not `class`), `htmlFor` (not `for`), `onClick` (not
  `onclick`).
- Every element must be closed: `<br />`, `<img />`, `<input />`.
- A component can only return **one** root element. Wrap siblings in a parent, or in an empty
  **Fragment** (`<>...</>`) when you do not want an extra wrapper node.

```tsx
function BookHeader() {
  return (
    <>
      <h2>Clean Code</h2>
      <p>Robert C. Martin</p>
    </>
  );
}
```

### Embedding JavaScript with curly braces

The reason JSX is powerful is the escape hatch: any place you want a JavaScript **expression**, you open a
pair of curly braces `{ }` and write it. Variables, function calls, arithmetic, ternaries, `.map()` — all
of it.

```tsx
function BookCard() {
  const title = "The Pragmatic Programmer";
  const author = "Hunt & Thomas";
  const copiesAvailable = 3;

  return (
    <article>
      <h3>{title}</h3>
      <p>by {author}</p>
      <p>{copiesAvailable > 0 ? `${copiesAvailable} available` : "Out of stock"}</p>
    </article>
  );
}
```

The one thing braces cannot hold is a **statement**. `{ if (x) {...} }` is illegal inside JSX because `if`
is a statement, not an expression. You reach for a ternary (`cond ? a : b`), a logical `&&`, or you compute
the value in a plain variable *above* the `return` and drop the variable into the braces. That "do the
logic above, interpolate the result below" pattern is the everyday rhythm of writing components.

### Nested component structures model the UI as a tree

Real interfaces are not one giant component. You build small, single-purpose components and compose them,
exactly the way you compose functions. The result is a tree, and reading the JSX reads like an outline of
the page.

```tsx
function Cover({ title }: { title: string }) {
  return <div className="cover">{title}</div>;
}

function BookRow({ title, author }: { title: string; author: string }) {
  return (
    <li className="book-row">
      <Cover title={title} />
      <span>{author}</span>
    </li>
  );
}

function Catalog() {
  return (
    <section>
      <h1>Catalog</h1>
      <ul>
        <BookRow title="Clean Code" author="Martin" />
        <BookRow title="Refactoring" author="Fowler" />
      </ul>
    </section>
  );
}
```

`Catalog` contains `BookRow`s, each `BookRow` contains a `Cover`. Composition like this is *the* design
skill in React: break a screen into the smallest reusable pieces, then assemble them. Small components are
easier to name, test, reuse, and reason about than one 400-line function.

### Three ways to style a component

You will meet all three in production codebases; each has a place.

**1. Inline styles** — a JavaScript object passed to the `style` prop. Property names are camelCase and
values are strings (or numbers, which React treats as pixels). Good for one-off, dynamic values computed at
render time; poor for anything reused, because there is no reuse and no pseudo-selectors or media queries.

```tsx
function StockBadge({ inStock }: { inStock: boolean }) {
  return (
    <span style={{ color: inStock ? "green" : "crimson", fontWeight: 600 }}>
      {inStock ? "In stock" : "Unavailable"}
    </span>
  );
}
```

Note the double braces: the outer `{}` is "enter JavaScript," the inner `{}` is "an object literal."

**2. CSS Modules** — a regular CSS file named `Something.module.css`. You `import` it as an object and the
build tool renames every class to be globally unique, so two components can both define `.card` without
colliding.

```css
/* BookCard.module.css */
.card { border: 1px solid #ddd; padding: 1rem; border-radius: 6px; }
.title { font-size: 1.1rem; margin: 0; }
```

```tsx
import styles from "./BookCard.module.css";

function BookCard({ title }: { title: string }) {
  return (
    <div className={styles.card}>
      <h3 className={styles.title}>{title}</h3>
    </div>
  );
}
```

**3. External / global stylesheet** — a plain `.css` file imported once (often in the app entry point).
Classes are global, exactly like a traditional website. Simplest to start with; the tradeoff is that class
names live in one shared namespace, so large teams tend to prefer Modules to avoid accidental collisions.

```tsx
import "./index.css"; // classes defined here apply everywhere
```

There is no single "right" choice — inline for tiny dynamic bits, Modules for component-scoped styles,
global stylesheets for app-wide resets and design tokens.

### Create and run a React app with the Vite CLI

Vite is the standard modern tool for scaffolding and serving a React app: near-instant dev startup and hot
reloading. Three commands take you from an empty folder to a running app in the browser.

```bash
npm create vite@latest my-app -- --template react
cd my-app
npm install
npm run dev
```

Line by line:

- `npm create vite@latest my-app -- --template react` scaffolds a folder named `my-app` using the React
  template. The lone `--` separates npm's own arguments from the ones handed to the Vite scaffolder; the
  `--template react` after it is what selects React. Use `--template react-ts` for a TypeScript project.
- `npm install` downloads the dependencies listed in the generated `package.json`.
- `npm run dev` starts the dev server (by default at `http://localhost:5173`) and watches your files, so
  saving a change updates the browser instantly.

When you are ready to ship, `npm run build` produces an optimized static bundle in `dist/`, and
`npm run preview` serves that bundle locally so you can sanity-check the production output. The entry point
the template gives you is `src/main.tsx` (which mounts your app into the page) and `src/App.tsx` (the root
component you start editing).

## Say It in an Interview

- *"A functional component is just a capitalized function that returns JSX. Props go in, UI comes out —
  there is no class or render method involved."*
- *"JSX isn't HTML and isn't a string; it's syntax sugar that compiles to React.createElement calls. That's
  why I use className instead of class and why I can drop any JavaScript expression inside curly braces."*
- *"I build the UI as a tree of small components and compose them, the same way I compose functions —
  easier to reuse and reason about than one giant component."*
- *"To start a React app I run npm create vite@latest with the react template, npm install, then npm run dev
  — Vite gives me an instant dev server with hot reloading."*

## Check Yourself

1. What two things make a function a React component rather than an ordinary function?
2. JSX `<h1 className="x">Hi</h1>` compiles to what underlying JavaScript call?
3. Why can you write a ternary inside `{ }` in JSX but not an `if` statement?
4. Name the three styling approaches and one situation each is best suited for.
5. Write the three commands that scaffold and run a new React app with Vite, and explain what the `--` in
   the first command is for.

**Answers:** (1) A capitalized name (so React treats it as a component, not a raw HTML tag) and returning
something renderable such as JSX. (2) `React.createElement("h1", { className: "x" }, "Hi")`. (3) Braces
hold JavaScript *expressions*, which produce a value; `if` is a *statement*, so you use a ternary or `&&`,
or compute the value above the return. (4) Inline styles (one-off dynamic values), CSS Modules
(component-scoped, collision-free classes), external/global stylesheet (app-wide resets and design tokens).
(5) `npm create vite@latest my-app -- --template react`, `npm install`, `npm run dev`; the `--` separates
npm's arguments from the flags passed through to the Vite scaffolder so `--template react` reaches Vite.

## Summary

- A functional component is a capitalized function that returns JSX; props in, UI out.
- JSX is not HTML — it compiles to `React.createElement` calls, which is why attributes are camelCase and
  every tag closes.
- Curly braces embed any JavaScript **expression**; statements go above the `return`.
- Compose small components into a nested tree that mirrors the interface.
- Style with inline styles (dynamic one-offs), CSS Modules (scoped classes), or a global stylesheet.
- Scaffold and run with Vite: `npm create vite@latest my-app -- --template react`, `npm install`,
  `npm run dev`.

## Resources

- [Your First Component — react.dev](https://react.dev/learn/your-first-component)
- [Writing Markup with JSX — react.dev](https://react.dev/learn/writing-markup-with-jsx)
- [Scaffolding Your First Vite Project — vitejs.dev](https://vitejs.dev/guide/#scaffolding-your-first-vite-project)
