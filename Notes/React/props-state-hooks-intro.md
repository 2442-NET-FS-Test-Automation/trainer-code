# Props, State, and Your First Hooks

## Learning Objectives
- Pass data into a component with props and read them inside.
- Hold and update local component memory with the `useState` hook.
- Explain what a hook is, and place `useState`, `useEffect`, and `useContext` in the landscape.
- Wire component communication both directions: parent-to-child with props, child-to-parent with callbacks.
- State the one-way data flow rule and why React is built around it.

## Why This Matters
Props and state are the two ways data lives in a React app, and telling them apart is the single most
tested React concept in interviews. Props are the arguments a component receives; state is the memory a
component keeps between renders. Almost every bug a beginner hits — "why didn't my UI update," "why is my
child stuck on the old value" — traces back to confusing the two or fighting React's one-way data flow.
Nail this and the rest of React (lists, forms, effects, context) is just variations on the same theme:
data flows down, events flow up.

## The Concept

### Props: arguments passed into a component
Props (short for "properties") are how a parent hands data to a child. You pass them like HTML attributes;
the child receives a single object and reads the fields off it. Props are **read-only** — a component must
never reassign its own props. If the parent later passes different values, the child re-renders with the
new ones.

```tsx
function BookCard({ title, author }: { title: string; author: string }) {
  return (
    <article>
      <h3>{title}</h3>
      <p>by {author}</p>
    </article>
  );
}

function Catalog() {
  return (
    <div>
      <BookCard title="Clean Code" author="Robert C. Martin" />
      <BookCard title="Refactoring" author="Martin Fowler" />
    </div>
  );
}
```

The `{ title, author }: { title: string; author: string }` is object destructuring of the props argument,
plus an inline type telling TypeScript what shape to expect. Same-named data, two different `BookCard`
instances — that reuse is the whole point of props.

### State: memory a component keeps between renders
Props come from outside; **state** is memory a component owns. A component re-renders (React re-runs the
function) whenever its state changes, and the new UI reflects the new value. You create state with the
`useState` hook.

```tsx
import { useState } from "react";

function BorrowButton() {
  const [borrowed, setBorrowed] = useState(false);

  return (
    <button onClick={() => setBorrowed(!borrowed)}>
      {borrowed ? "Return book" : "Borrow book"}
    </button>
  );
}
```

`useState(false)` returns a pair: the current value (`borrowed`) and a setter function (`setBorrowed`). The
array destructuring names them whatever you want, by convention `x` and `setX`. The argument to `useState`
is the **initial** value, used only on the first render. Two rules that trip everyone up at first:

- **Never assign to the variable directly.** `borrowed = true` does nothing useful — React does not know it
  changed. You must call the setter (`setBorrowed(true)`) so React schedules a re-render.
- **Setting state is asynchronous-ish.** The new value shows up on the *next* render, not on the line after
  the setter. When the next value depends on the previous, pass a function:
  `setCount(prev => prev + 1)`.

State can hold anything: numbers, strings, booleans, objects, arrays. When it holds an object or array,
produce a **new** one rather than mutating the old (`setBooks([...books, newBook])`), because React decides
whether to re-render by comparing references.

### What a hook is
A **hook** is a function whose name starts with `use` that lets a plain function component "hook into"
React features — state, lifecycle side effects, shared context — that used to require class components.
Hooks follow two hard rules: call them only at the **top level** of a component (never inside loops,
conditions, or nested functions), and call them only from **components or other hooks**. React relies on
hooks being called in the same order every render, which is why they cannot hide inside an `if`.

You will meet three hooks constantly:

- **`useState`** — local component memory (this note).
- **`useEffect`** — run side effects (data fetching, subscriptions, timers) after render, with an optional
  cleanup. Covered in its own material.
- **`useContext`** — read shared data from a Context provider without threading props through every level.
  Covered in its own material.

For now, know that they exist and that `useState` is the one you reach for first. The mental slot to keep
open: "when I need to *do something outside rendering*, that's `useEffect`; when I need *shared app-wide
data*, that's `useContext`."

### Parent-to-child: pass data down as props
Communication downward is what you have already seen — the parent renders the child with props. The parent
can pass anything, including its own state, and when that state changes the child re-renders automatically.

```tsx
function Library() {
  const [featured] = useState("The Pragmatic Programmer");
  return <BookCard title={featured} author="Hunt & Thomas" />;
}
```

### Child-to-parent: pass a callback down, the child calls it
A child cannot reach up and change the parent's state directly — data does not flow upward. Instead the
parent passes a **function** down as a prop, and the child **calls** that function to send data back up.
The parent owns the state; the child merely requests a change.

```tsx
import { useState } from "react";

function SearchBox({ onSearch }: { onSearch: (term: string) => void }) {
  const [term, setTerm] = useState("");
  return (
    <div>
      <input value={term} onChange={(e) => setTerm(e.target.value)} />
      <button onClick={() => onSearch(term)}>Search</button>
    </div>
  );
}

function CatalogPage() {
  const [query, setQuery] = useState("");

  return (
    <div>
      <SearchBox onSearch={(term) => setQuery(term)} />
      <p>Showing results for: {query || "everything"}</p>
    </div>
  );
}
```

`CatalogPage` owns `query`. It passes `onSearch` down; `SearchBox` collects the term in its own local
state and, on click, calls `onSearch(term)`. That call runs the parent's `setQuery`, the parent
re-renders, and the new query flows back down. This callback pattern is how every "child tells parent
something happened" interaction works: form submits, button clicks, list-item selections, delete buttons.

### One-way data flow
Put the two directions together and you get React's defining principle: **data flows one way, downward.**
Parents pass data to children through props; children never mutate what they receive. When a child needs to
cause a change, it invokes a callback and the *parent* updates *its own* state, which then flows back down
as new props. This is often called "top-down" or "unidirectional" data flow.

The payoff is predictability. Because state has a single owner and children cannot secretly rewrite it, you
can always answer "where does this value come from and who can change it?" by walking up the tree to the
owner. Debugging becomes tracing a one-directional path instead of chasing mutations that could originate
anywhere. When two siblings need the same data, you **lift the state up** to their nearest common parent
and pass it (plus a setter callback) down to both — same principle, applied one level higher.

## Say It in an Interview
- *"Props are read-only inputs a parent passes to a child; state is private memory a component owns and
  updates with useState, which triggers a re-render."*
- *"A hook is a use-prefixed function that lets a function component tap into React features like state and
  effects; useState is local memory, useEffect is side effects, useContext is shared data."*
- *"Data flows one way — down via props. A child talks back to its parent by calling a callback the parent
  passed down; the parent owns the state and updates it."*
- *"I never mutate props or state in place. I call the setter with a new value, and for shared data I lift
  state up to the nearest common parent."*

## Check Yourself
1. What is the difference between props and state?
2. What does `const [count, setCount] = useState(0)` give you, and what is the `0` for?
3. Why does assigning `count = count + 1` fail to update the UI?
4. A child needs to notify its parent that a button was clicked. Describe the mechanism.
5. State the one-way data flow rule in a sentence, and explain what "lifting state up" means.

**Answers:** (1) Props are read-only data passed *into* a component from its parent; state is private memory
the component *owns* and updates over time. (2) A pair — the current value `count` and a setter
`setCount`; the `0` is the initial value used only on the first render. (3) React only re-renders when you
call the setter; a direct assignment bypasses React, which never learns the value changed (and the
reassignment is lost on the next render anyway). (4) The parent passes a callback function down as a prop;
the child calls that callback (optionally with data), which runs the parent's state setter. (5) Data flows
downward through props and children request changes via callbacks rather than mutating parent state;
"lifting state up" moves shared state to the nearest common ancestor so multiple children can use it.

## Summary
- **Props** are read-only inputs passed from parent to child; **state** is a component's own memory.
- `useState(initial)` returns `[value, setValue]`; call the setter (never assign) to trigger a re-render.
- A **hook** is a `use`-prefixed function; `useState` (memory), `useEffect` (side effects), and
  `useContext` (shared data) are the three you meet first. Call hooks only at the top level.
- Parent-to-child = props down; child-to-parent = a callback passed down that the child invokes.
- React is **one-way / unidirectional**: data flows down, events flow up; share data by lifting state up.

## Resources
- [Passing Props to a Component — react.dev](https://react.dev/learn/passing-props-to-a-component)
- [State: A Component's Memory — react.dev](https://react.dev/learn/state-a-components-memory)
- [Sharing State Between Components — react.dev](https://react.dev/learn/sharing-state-between-components)
