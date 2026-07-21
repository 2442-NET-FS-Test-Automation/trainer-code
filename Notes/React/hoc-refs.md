# Refs and Higher-Order Components: Escaping and Reusing

## Learning Objectives

- Use `useRef` to hold a mutable `.current` value that survives re-renders without triggering one.
- Attach a ref to a DOM node to read or command it imperatively.
- Write a higher-order component `withX(Component)` that wraps a component to add behavior.
- Split UI into container (logic) and presentational (rendering) components, and know why hooks often
  replace HOCs today.

## Why This Matters

Two recurring needs sit just outside the everyday `useState`-and-props flow. First, sometimes you need to
remember a value *between renders without re-rendering* — a timer id, a previous value, a click count you
don't display — and state is the wrong tool because changing it would repaint the screen. Second, sometimes
several components need the *same wrapper behavior* — a permission check, a loading gate — and copy-pasting
it is a maintenance trap. Refs solve the first; higher-order and container components solve the second.
Both come up in interviews as "what is useRef for, and how is it different from state?" and "what's an HOC,
and would you still use one?"

## The Concept

### Refs: mutable values that don't trigger a re-render

`useRef` returns a plain object with a single mutable property, `.current`. React preserves that object
across every render, but — unlike state — **reassigning `.current` does not cause a re-render**. It is a box
for values that need to persist but should not drive the UI.

```tsx
import { useRef } from "react";

function ClickCounter() {
  const clicks = useRef(0); // { current: 0 }, stable across renders

  function handleClick() {
    clicks.current += 1;          // mutate freely; no re-render happens
    console.log("clicks so far:", clicks.current);
  }

  // The screen never shows clicks.current updating — that's the point.
  return <button onClick={handleClick}>Log a click</button>;
}
```

The contrast with `useState` is the whole lesson:

| `useState` | `useRef` |
| --- | --- |
| Changing it **re-renders** the component | Changing `.current` does **not** re-render |
| Value is what the UI reflects | Value is bookkeeping the UI does not directly show |
| Read the latest value after re-render | Read the latest value synchronously any time |

A common real use is holding a timer handle so you can clear it later — data the render output never needs:

```tsx
function AutoSaver() {
  const timerRef = useRef<number | null>(null);

  function scheduleSave() {
    if (timerRef.current !== null) clearTimeout(timerRef.current);
    timerRef.current = window.setTimeout(() => save(), 1000);
  }

  return <textarea onChange={scheduleSave} />;
}
```

### Refs to DOM nodes

The other half of `useRef` is reaching a real DOM element to do something React's declarative model does not
cover: focus an input, measure a box, scroll to a position, play a video. Create a ref, pass it to an
element's `ref` attribute, and after mount `.current` points at the live DOM node.

```tsx
import { useRef } from "react";

function SearchBox() {
  const inputRef = useRef<HTMLInputElement>(null);

  function focusInput() {
    // .current is the actual <input> element after render.
    inputRef.current?.focus();
  }

  return (
    <div>
      <input ref={inputRef} placeholder="Search the catalog" />
      <button onClick={focusInput}>Focus the field</button>
    </div>
  );
}
```

Use a DOM ref only for genuinely imperative jobs. If a value determines what the UI *looks like*, that is
state or props — reach for a ref to *command* a node, not to describe it.

### Higher-order components: `withX(Component)`

A higher-order component (HOC) is a **function that takes a component and returns a new component** wrapping
it with extra behavior. It is a pattern, not an API — just a function. The classic shape is a gate that adds
a cross-cutting concern (auth, logging, a loading guard) around any component:

```tsx
import { type ComponentType } from "react";

interface WithAuthProps {
  user: Identity | null;
}

// Takes a component; returns one that only renders it when a user is present.
function withAuth<P extends object>(Wrapped: ComponentType<P>) {
  return function WithAuth(props: P & WithAuthProps) {
    const { user, ...rest } = props;
    if (user === null) return <p>Please sign in to continue.</p>;
    return <Wrapped {...(rest as P)} />;
  };
}

// A plain component that assumes it only renders when signed in.
function Dashboard() {
  return <h1>Your borrowed books</h1>;
}

// Wrap once; reuse the guard on any component.
const ProtectedDashboard = withAuth(Dashboard);
```

`ProtectedDashboard` behaves like `Dashboard` but carries the sign-in check for free, and the same
`withAuth` can wrap a settings page, a profile page, anything. That reuse — one behavior, many components —
is the point of an HOC.

### Container vs presentational components

A related way to organize reusable logic is to split a feature in two:

- A **container** component holds the logic: it fetches data, keeps state, and computes values.
- A **presentational** component just receives that data as props and renders markup — no fetching, no
  state of its own.

```tsx
// Container: owns the data and the "how".
function BookListContainer() {
  const [books, setBooks] = useState<Book[]>([]);
  useEffect(() => {
    fetchBooks().then(setBooks);
  }, []);
  return <BookList books={books} />;
}

// Presentational: pure rendering, trivially reusable and easy to test.
function BookList({ books }: { books: Book[] }) {
  return (
    <ul>
      {books.map((b) => (
        <li key={b.id}>{b.title}</li>
      ))}
    </ul>
  );
}
```

The presentational `BookList` knows nothing about where books come from, so it can be dropped anywhere and
tested by simply handing it an array.

### Why hooks often replace HOCs today

HOCs and container components were the pre-hooks way to share logic, and they have real drawbacks: nesting
several HOCs buries the real component in a stack of wrappers ("wrapper hell"), and it gets murky which
props come from where. **Custom hooks** now cover most of the same ground more directly. Instead of wrapping
a component to inject behavior, a hook returns the behavior to the component that asks for it:

```tsx
// The custom-hook alternative to withAuth + BookListContainer combined.
function useCurrentUser(): Identity | null {
  const { user } = useAuth();
  return user;
}

function Dashboard() {
  const user = useCurrentUser();
  if (user === null) return <p>Please sign in to continue.</p>;
  return <h1>Your borrowed books</h1>;
}
```

No wrapper component, no prop plumbing, no ambiguity about where `user` came from. You should still
*recognize* HOCs and the container/presentational split — plenty of existing code and libraries use them,
and interviewers ask — but for new code, a custom hook is usually the cleaner reach.

## Say It in an Interview

- *"useRef gives you a mutable .current that survives re-renders but doesn't cause one — I use it for timer
  ids, previous values, or anything the UI doesn't need to display."*
- *"Attach a ref to an element's ref attribute and .current becomes the real DOM node after mount, for
  imperative jobs like focusing an input or measuring a box."*
- *"A higher-order component is a function that takes a component and returns a wrapped one with added
  behavior — withAuth(Dashboard) adds a sign-in gate reusable across pages."*
- *"Container components hold the logic and state, presentational ones just render props. These days I'd
  usually reach for a custom hook instead of an HOC to avoid wrapper hell."*

## Check Yourself

1. What does `useRef` return, and how does changing it differ from changing state?
2. Give two legitimate uses for a ref — one non-DOM, one DOM.
3. Define a higher-order component in one sentence. What does `withAuth(Dashboard)` produce?
4. What is the difference between a container and a presentational component?
5. Why do modern React codebases often prefer custom hooks over HOCs?

**Answers:** (1) An object `{ current }` that persists across renders; reassigning `.current` does not
trigger a re-render, whereas a state setter does. (2) Non-DOM: hold a timer id or a previous value the UI
doesn't display; DOM: attach it to an element to focus an input, measure, or scroll imperatively. (3) A
function that takes a component and returns a new component wrapping it with added behavior; `withAuth(Dashboard)`
produces a component that renders `Dashboard` only when a user is signed in. (4) The container owns logic,
state, and data-fetching; the presentational component only receives props and renders markup. (5) Custom
hooks share logic without wrapping components, avoiding wrapper hell and the ambiguity of where injected
props originate.

## Summary

- `useRef` holds a mutable `.current` that persists across renders and never triggers one — ideal for timer
  ids, previous values, and other non-visual bookkeeping.
- Attach a ref to an element's `ref` attribute to reach the real DOM node for imperative work (focus,
  measure, scroll).
- A higher-order component is a function `withX(Component)` returning a wrapped component with added
  behavior, reusable across many components.
- Container components hold logic and state; presentational components just render props and are easy to
  reuse and test.
- Custom hooks now cover most HOC use cases more cleanly; recognize HOCs and container/presentational splits
  in existing code, but prefer hooks for new work.

## Resources

- [useRef (react.dev)](https://react.dev/reference/react/useRef)
- [Manipulating the DOM with Refs (react.dev)](https://react.dev/learn/manipulating-the-dom-with-refs)
- [Reusing Logic with Custom Hooks (react.dev)](https://react.dev/learn/reusing-logic-with-custom-hooks)
