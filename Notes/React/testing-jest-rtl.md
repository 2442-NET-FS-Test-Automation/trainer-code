# Testing React Components with a Testing Library: Behavior, Not Internals

## Learning Objectives

- Use React Testing Library's core loop: `render`, query with `screen`, interact, then `expect` an outcome.
- Query the way a user perceives the UI — by role and text — rather than by implementation detail.
- Write a component test that asserts rendered content and that a click fires the expected callback.
- Understand that the assertions and queries are identical across the Jest and Vitest test runners.

## Why This Matters

A component test that breaks every time you rename a CSS class or restructure a `<div>` is worse than no
test — it costs you maintenance and teaches the team to ignore red builds. React Testing Library (RTL) was
designed around a single guiding principle: *test the software the way the user uses it.* You render the
component, find things the way a person (or a screen reader) would — "the button labeled Save", "the text
The Pragmatic Programmer" — interact with them, and assert on what the user would observe. Tests written
this way survive refactors and double as documentation of behavior, which is exactly what interviewers and
senior reviewers look for.

## The Concept

### The toolchain, and why the API is the same everywhere

There are two moving parts: a **test runner** (finds and executes tests, provides `describe`/`test`/`expect`)
and a **testing library** (renders components and queries the DOM). The runner is either **Jest** — long
the default in Create-React-App and many existing codebases — or **Vitest**, the modern default for
Vite-based projects because it reuses the project's existing build config. The component library is React
Testing Library in both cases.

The key fact: **RTL's API does not change between runners.** `render`, `screen`, `getByRole`, `fireEvent`,
and `userEvent` are identical; the `expect` assertion style is compatible; and the custom DOM matchers from
`@testing-library/jest-dom` (like `toBeInTheDocument`) work under both. In practice the only differences you
touch are the import for the test globals and the config file. Everything in this note applies verbatim
whether your `package.json` script runs `jest` or `vitest`.

```ts
// Jest:   imports come from "@jest/globals" (or are global with no import)
// Vitest: imports come from "vitest"
import { describe, test, expect } from "vitest"; // or "@jest/globals"
```

### The core loop: render, query, interact, assert

Every RTL test is the same four beats. Render the component into a virtual DOM, use `screen` to find
elements the way a user would, drive an interaction, then assert on the result.

```tsx
import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom"; // adds matchers like toBeInTheDocument

function BookCard({ title, author }: { title: string; author: string }) {
  return (
    <article>
      <h2>{title}</h2>
      <p>{author}</p>
    </article>
  );
}

test("renders the book's title and author", () => {
  render(<BookCard title="The Pragmatic Programmer" author="Hunt & Thomas" />);

  // heading role — how a screen reader would find it
  expect(screen.getByRole("heading", { name: "The Pragmatic Programmer" })).toBeInTheDocument();
  // plain visible text
  expect(screen.getByText("Hunt & Thomas")).toBeInTheDocument();
});
```

`screen` is the query entry point that searches the whole rendered document. The queries come in families:
`getBy*` throws if the element is missing (assert it exists), `queryBy*` returns `null` if missing (assert
it is *absent*), and `findBy*` returns a promise that waits for the element to appear (assert something
async). Prefer, in order: `getByRole` (buttons, headings, textboxes — accessible and robust), then
`getByLabelText` for form fields, then `getByText`. Fall to `getByTestId` only when nothing user-facing
identifies the element.

### Test behavior, not implementation

The discipline that makes these tests durable: assert on **what the user observes**, never on component
internals. Do not reach into state, do not assert "this `useState` holds 3", do not query by class name.
Assert that the count *shown on screen* reads 3. If a refactor changes the internals but the user-visible
behavior is unchanged, the test should stay green — that is the whole point. Concretely, this means querying
by role and text (not `container.querySelector(".btn-primary")`) and asserting on rendered output and on
whether callbacks fired, not on how the component computed them.

### Simulating interaction and asserting a callback fired

For a component that takes a callback prop, you render it with a **mock function**, simulate the user
action, and assert the mock was called. `fireEvent` dispatches a single raw DOM event; `userEvent` (from
`@testing-library/user-event`) simulates a real user more faithfully — a click is a hover, mouse-down,
focus, mouse-up, click — and is the recommended default. `userEvent` is async, so its calls are `await`ed.

```tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi } from "vitest"; // Jest: use jest.fn() instead of vi.fn()

function CheckoutButton({ onCheckout }: { onCheckout: () => void }) {
  return <button onClick={onCheckout}>Borrow</button>;
}

test("calls onCheckout when the button is clicked", async () => {
  const handleCheckout = vi.fn();        // Jest: const handleCheckout = jest.fn();
  render(<CheckoutButton onCheckout={handleCheckout} />);

  await userEvent.click(screen.getByRole("button", { name: "Borrow" }));

  expect(handleCheckout).toHaveBeenCalledTimes(1);
});
```

The mock (`vi.fn()` under Vitest, `jest.fn()` under Jest — same behavior) records every call, so
`toHaveBeenCalledTimes(1)` and `toHaveBeenCalledWith(...)` let you assert the component wired the event to
the prop correctly. Note again that only the mock factory import differs between runners; the query, the
interaction, and the assertion are identical.

### A stateful example: interaction changes what is rendered

Putting it together — drive an interaction and assert the *visible result* changed, without ever touching
state directly:

```tsx
import { useState } from "react";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import "@testing-library/jest-dom";

function Counter() {
  const [count, setCount] = useState(0);
  return <button onClick={() => setCount((c) => c + 1)}>Borrowed: {count}</button>;
}

test("increments the visible count on click", async () => {
  render(<Counter />);
  const button = screen.getByRole("button", { name: /borrowed: 0/i });

  await userEvent.click(button);

  expect(screen.getByRole("button", { name: /borrowed: 1/i })).toBeInTheDocument();
});
```

The assertion is on the rendered label the user reads, not on the `count` state variable — swap `useState`
for `useReducer` and this test does not change.

## Say It in an Interview

- *"React Testing Library's philosophy is to test the component the way a user uses it: render it, query by
  role and text, interact, and assert on what's on screen — not on internal state or class names."*
- *"The loop is render, then `screen.getByRole`/`getByText` to find elements, `userEvent` to interact,
  `expect` to assert. `getBy` throws if missing, `queryBy` returns null for absence, `findBy` awaits async."*
- *"To test a callback I render with a mock — `jest.fn()` or `vi.fn()` — click the button, and assert
  `toHaveBeenCalled`. Vitest is the modern default for Vite projects, but RTL's queries and assertions are
  identical to Jest."*

## Check Yourself

1. What are the four steps of a typical RTL test?
2. What does "test behavior, not implementation" forbid you from asserting on, and what should you assert on
   instead?
3. What is the difference between `getByRole`, `queryByRole`, and `findByRole`?
4. How do you check that a component invoked its `onSave` callback prop when a button was clicked?
5. If a project uses Vitest instead of Jest, what actually changes in the test code you write?

**Answers:** (1) Render the component, query with `screen`, simulate an interaction, assert with `expect`.
(2) Do not assert on internal state, class names, or DOM structure; assert on user-observable output —
rendered text/roles and whether callbacks fired. (3) `getByRole` throws if the element is missing (assert
presence), `queryByRole` returns `null` (assert absence), `findByRole` returns a promise and waits (assert
something that appears asynchronously). (4) Pass a mock (`jest.fn()`/`vi.fn()`) as the prop, `userEvent.click`
the button, then `expect(mock).toHaveBeenCalled()`/`toHaveBeenCalledTimes(1)`. (5) Almost nothing — the
mock factory (`vi.fn` vs `jest.fn`) and the globals import (`vitest` vs `@jest/globals`); `render`, `screen`,
the queries, `userEvent`, and the assertions are the same.

## Summary

- A test = a runner (Jest or Vitest) + React Testing Library; RTL's API and assertions are identical across
  runners, so tests port with only config/import changes.
- The loop is render -> query via `screen` -> interact -> `expect`; prefer `getByRole` and `getByText`.
- `getBy*` throws (presence), `queryBy*` returns null (absence), `findBy*` awaits (async).
- Test behavior, not implementation: assert on visible output and fired callbacks, never on internal state
  or class names, so tests survive refactors.
- Assert callbacks with a mock function and `toHaveBeenCalled`; `@testing-library/jest-dom` adds matchers
  like `toBeInTheDocument`.

## Resources

- [Guiding Principles (testing-library.com)](https://testing-library.com/docs/guiding-principles/)
- [React Testing Library — API & queries (testing-library.com)](https://testing-library.com/docs/react-testing-library/intro/)
- [About Queries — which query to use (testing-library.com)](https://testing-library.com/docs/queries/about/)
