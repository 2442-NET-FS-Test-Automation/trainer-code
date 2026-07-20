# React with TypeScript: Type-Checked Props

## Learning Objectives
- Explain the concrete benefits TypeScript brings to a React codebase.
- Write components in `.tsx` and type their props explicitly.
- Build a reusable component with type-checked props, including optional props and typed callbacks.
- Recognize the everyday TypeScript patterns you meet in real React code.

## Why This Matters
The React ecosystem has largely converged on TypeScript, and "why use TypeScript with React?" is a standard
interview question with a very concrete answer: it makes the invisible contract between components visible
and enforced. In plain JavaScript, passing the wrong prop — a number where a string was expected, a
misspelled field, a forgotten required prop — fails silently at runtime, often as a blank screen or
`undefined` deep in the render. TypeScript turns those into red squiggles in your editor before you ever
run the code, and gives you autocomplete on every prop and every field. For any component you intend to
reuse, typed props *are* the documentation: the type tells the next developer exactly how to call it.

## The Concept

### The benefits of TypeScript in React
Three payoffs matter most in day-to-day React work:

- **Props become a checked contract.** A component declares exactly what props it accepts and their types.
  Pass the wrong type, misspell a prop, or forget a required one, and you get a compile-time error at the
  call site — not a runtime surprise.
- **Editor superpowers.** Autocomplete lists a component's props as you type its JSX; hovering shows the
  expected type; renaming a prop updates every usage safely. IntelliSense reaches into your data shapes
  too, so `book.` offers `title` and `author`.
- **Self-documenting components.** The prop type *is* the usage manual. A new teammate reads the type and
  knows how to use the component without opening its implementation or guessing.

The cost is honest and small in a React project: you write `.tsx` instead of `.jsx`, you annotate props,
and you occasionally install community type packages for untyped libraries. Vite scaffolds the whole
TypeScript setup for you with the `react-ts` template, so there is no extra configuration to get started.

### `.tsx` files and typing props inline
A React component written in TypeScript lives in a `.tsx` file (the `x` is for JSX). The core new habit is
annotating the props parameter. For a small component, an inline object type is perfectly idiomatic:

```tsx
function BookCard({ title, author }: { title: string; author: string }) {
  return (
    <article>
      <h3>{title}</h3>
      <p>by {author}</p>
    </article>
  );
}
```

Now the compiler enforces the contract at every call site:

```tsx
<BookCard title="Clean Code" author="Martin" />        {/* OK */}
<BookCard title="Refactoring" />                        {/* Error: 'author' is missing */}
<BookCard title="Dune" author={1965} />                 {/* Error: number is not a string */}
```

Those errors appear as you type, in the editor, before the app runs.

### Naming the props type with an interface or type alias
Once a component has more than a couple of props, pull the shape into a named `type` or `interface` above
the component. It reads better, is reusable, and is the more common style in real codebases.

```tsx
interface BookCardProps {
  title: string;
  author: string;
  year: number;
}

function BookCard({ title, author, year }: BookCardProps) {
  return (
    <article>
      <h3>{title}</h3>
      <p>{author}, {year}</p>
    </article>
  );
}
```

`interface` and `type` are interchangeable for this purpose; pick one and stay consistent. Many teams use
`interface` for props by convention.

### Optional props, defaults, and union types
Real components have optional and constrained props. A `?` marks a prop optional; a union type restricts a
prop to a fixed set of values; default values are given with ordinary JavaScript defaults in the
destructuring.

```tsx
interface BadgeProps {
  label: string;
  variant?: "info" | "warning" | "success"; // optional, restricted to three values
}

function Badge({ label, variant = "info" }: BadgeProps) {
  return <span className={`badge badge-${variant}`}>{label}</span>;
}

<Badge label="New" />                    {/* variant defaults to "info" */}
<Badge label="Low stock" variant="warning" />
<Badge label="x" variant="danger" />     {/* Error: "danger" is not a valid variant */}
```

The union type `"info" | "warning" | "success"` is a small but high-value pattern: it makes an entire class
of typos impossible and gives you autocomplete of the valid options.

### Typing children and callbacks
Two prop types come up constantly. When a component wraps other JSX, type the `children` prop as
`React.ReactNode`. When a child reports events back to a parent, type the callback as a function.

```tsx
import { ReactNode } from "react";

interface PanelProps {
  title: string;
  children: ReactNode;              // any renderable JSX passed between the tags
}

function Panel({ title, children }: PanelProps) {
  return (
    <section className="panel">
      <h2>{title}</h2>
      {children}
    </section>
  );
}

interface BookRowProps {
  title: string;
  onRemove: (title: string) => void; // a callback that takes a string, returns nothing
}

function BookRow({ title, onRemove }: BookRowProps) {
  return (
    <li>
      {title}
      <button onClick={() => onRemove(title)}>Remove</button>
    </li>
  );
}
```

`ReactNode` covers anything React can render — elements, strings, numbers, arrays, `null`. The callback
type `(title: string) => void` guarantees the parent's handler has the right signature and that the child
calls it correctly.

### Putting it together: a reusable, type-checked component
Here is a small reusable component that combines the patterns — a named props interface, a required field,
an optional field with a default, a union type, and a typed callback. Because every prop is typed, any
caller gets checked and autocompleted.

```tsx
interface Book {
  id: number;
  title: string;
  author: string;
}

interface BookListItemProps {
  book: Book;
  highlighted?: boolean;                 // optional
  size?: "compact" | "full";             // restricted options
  onSelect: (id: number) => void;        // typed callback
}

function BookListItem({
  book,
  highlighted = false,
  size = "full",
  onSelect,
}: BookListItemProps) {
  return (
    <li
      className={`book book-${size} ${highlighted ? "book-highlighted" : ""}`}
      onClick={() => onSelect(book.id)}
    >
      <strong>{book.title}</strong> — {book.author}
    </li>
  );
}

// Usage — fully checked and autocompleted:
function Catalog({ books }: { books: Book[] }) {
  return (
    <ul>
      {books.map((b) => (
        <BookListItem key={b.id} book={b} size="compact" onSelect={(id) => console.log(id)} />
      ))}
    </ul>
  );
}
```

Every call to `BookListItem` is verified: forget `onSelect` and it will not compile; pass
`size="medium"` and TypeScript rejects it; typo `book.titel` inside the component and the editor flags it.
That is the whole value proposition — the contract is written down once and enforced everywhere the
component is used.

## Say It in an Interview
- *"TypeScript turns a component's props into a checked contract: pass the wrong type, misspell a prop, or
  omit a required one and it's a compile-time error at the call site, not a runtime surprise."*
- *"On top of catching bugs, I get autocomplete on every prop and field and safe renames — and the prop
  type doubles as the component's documentation."*
- *"I write components in .tsx and annotate props, usually with a named interface. Optional props get a ?,
  and I restrict values with union types like 'compact' | 'full'."*
- *"I type children as React.ReactNode and callbacks as function types like (id: number) => void so
  parent-child communication is verified in both directions."*

## Check Yourself
1. Name three concrete benefits of using TypeScript in a React project.
2. What is the difference between a `.jsx` and a `.tsx` file, and what habit changes when you write props?
3. How do you make a prop optional, and how do you restrict it to a fixed set of allowed values?
4. What type do you use for a component's `children`, and why?
5. Write the props type for a component that takes a required `title: string` and an `onClose` callback
   that takes no arguments and returns nothing.

**Answers:** (1) Props become a compile-time-checked contract; editor autocomplete/safe-refactoring on
props and data shapes; self-documenting components whose prop types describe how to use them. (2) `.tsx`
supports both JSX and TypeScript types; the new habit is annotating the props parameter with a type (inline
or a named interface). (3) Mark it optional with `?` (e.g. `variant?: string`); restrict it with a union
type such as `"info" | "warning" | "success"`. (4) `React.ReactNode`, because it covers everything React
can render — elements, strings, numbers, arrays, and `null`. (5)
`interface CloseableProps { title: string; onClose: () => void; }`.

## Summary
- TypeScript makes props a **checked contract**: wrong/missing/misspelled props fail at compile time, at
  the call site.
- Extra payoffs: autocomplete, safe renames, and prop types that serve as documentation.
- Write components in `.tsx` and annotate props — inline for small components, a named `interface`/`type`
  for larger ones.
- Use `?` for optional props, union types (`"a" | "b"`) to restrict values, and JS defaults in the
  destructuring for defaults.
- Type `children` as `React.ReactNode` and callbacks as function types like `(id: number) => void`; Vite's
  `react-ts` template sets all of this up for you.

## Resources
- [Using TypeScript — react.dev](https://react.dev/learn/typescript)
- [React TypeScript Cheatsheet — react-typescript-cheatsheet.netlify.app](https://react-typescript-cheatsheet.netlify.app/)
- [TypeScript template — Vite guide (vitejs.dev)](https://vitejs.dev/guide/#scaffolding-your-first-vite-project)
