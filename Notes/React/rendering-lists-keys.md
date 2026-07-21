# Rendering Lists, Keys, and Conditional UI

## Learning Objectives

- Render a dynamic list by mapping an array of data to an array of elements.
- Give each list item a stable, unique `key` and explain what React uses it for.
- Recognize why the array index is a poor key when the list can change.
- Conditionally render UI from state or interaction using a ternary and the `&&` operator.

## Why This Matters

Almost every real interface is a list: a catalog of books, rows in a table, search results, a cart. React
does not have a "for-each" template directive — you render lists with plain JavaScript's `.map()`, which
surprises people coming from other frameworks. And the very next thing an interviewer asks is "what's the
key prop for?" because keys are where beginners get subtle, hard-to-see bugs: inputs that keep the wrong
value, rows that animate wrong, checkboxes that jump. Pair lists with conditional rendering — showing a
spinner, an empty-state, or an error depending on state — and you have the two patterns that make up the
bulk of day-to-day component code.

## The Concept

### Rendering a list with `.map()`

To turn data into UI, call `.map()` on the array and return one element per item. `.map()` produces an
array of elements, and React knows how to render an array of elements directly inside JSX.

```tsx
type Book = { id: number; title: string; author: string };

function Catalog({ books }: { books: Book[] }) {
  return (
    <ul>
      {books.map((book) => (
        <li key={book.id}>
          {book.title} — {book.author}
        </li>
      ))}
    </ul>
  );
}
```

The `{ ... }` drops JavaScript into JSX; inside it, `books.map(...)` returns an array of `<li>` elements.
This is why data drives the UI: change the array, and the rendered list changes with it. There is no
special loop syntax to learn — if you can `.map()` an array in JavaScript, you can render a list in React.

### The `key` prop: how React tracks items across renders

Notice `key={book.id}` above. When a list re-renders, React needs to match each new element to the element
it drew last time, so it can figure out the minimal set of changes: which rows stayed, which were added,
which were removed, which reordered. The **key** is the identity tag React uses for that matching.

A key must be:

- **Unique among siblings** (two items in the same list cannot share a key), and
- **Stable** across renders — the same item keeps the same key every time.

The natural choice is a real, stable ID from your data: a database primary key, a slug, a UUID. React uses
the key purely internally; it is not passed to your component as a prop and never appears in the DOM.

```tsx
// Good: a stable identity from the data
{books.map((book) => <BookRow key={book.id} book={book} />)}
```

### Why the array index is usually a bad key

It is tempting to write `key={index}` using `.map((item, index) => ...)`. It silences React's
"each child needs a key" warning, so it looks fine — until the list changes order or items are inserted or
removed. Because the index describes a *position*, not an *item*, React mis-matches elements when positions
shift, and any per-item internal state (the text in an input, whether a checkbox is checked, an animation)
sticks to the wrong row.

Consider a to-remove list. If you delete the first book, every remaining book's index shifts down by one.
With index keys, React thinks item 0 "changed its title" rather than "item 0 was removed," and DOM state
attached to those rows follows the index, not the book.

```tsx
// Fragile: breaks on insert / delete / reorder
{books.map((book, index) => <BookRow key={index} book={book} />)}
```

Index keys are only acceptable when the list is static — never reordered, filtered, or edited — and the
items have no ID and no internal state. When in doubt, use a real ID. If your data genuinely has none,
generate a stable ID once when you create the item (for example with `crypto.randomUUID()`) and store it
alongside the data, rather than deriving a key from the position.

### Keys make reconciliation efficient

The payoff of good keys is performance and correctness. React compares the previous render's tree to the
new one (a process called reconciliation) and, guided by keys, applies only the differences to the real
DOM. Add one book to a list of a hundred and React inserts one `<li>` — it does not tear down and rebuild
the other ninety-nine. Stable keys are what let React do that minimal, targeted update instead of
re-creating the whole list.

### Conditional rendering with a ternary

Often you want to show different UI depending on state — loading versus loaded, empty versus populated,
error versus content. Because JSX interpolates expressions, the ternary operator `cond ? a : b` is the
cleanest tool when you are choosing between **two** branches.

```tsx
function CatalogStatus({ books }: { books: Book[] }) {
  return (
    <div>
      {books.length > 0 ? (
        <ul>
          {books.map((b) => (
            <li key={b.id}>{b.title}</li>
          ))}
        </ul>
      ) : (
        <p>No books match your search.</p>
      )}
    </div>
  );
}
```

Either the list or the empty-state message renders, never both. Ternaries can wrap whole blocks of JSX like
this, and they read naturally as "if books, show the list; otherwise, show the message."

### Conditional rendering with `&&`

When you want to show something **or nothing** — a single branch with no "else" — the logical `&&` operator
is more concise. `condition && <Element />` evaluates to the element when the condition is truthy and to
the falsy value (which React renders as nothing) when it is not.

```tsx
function BookRow({ book, isNew }: { book: Book; isNew: boolean }) {
  return (
    <li>
      {book.title}
      {isNew && <span className="badge">New</span>}
    </li>
  );
}
```

One sharp edge worth knowing: `&&` with a **number** on the left can render the number. `{count && <X/>}`
renders `0` on screen when `count` is `0`, because `0` is falsy but still a renderable value. Guard with a
real boolean instead: `{count > 0 && <X/>}`. For anything more than a simple show/hide, prefer a ternary or
compute the element in a variable above the `return` and interpolate it — deeply nested ternaries inside
JSX get unreadable fast.

## Say It in an Interview

- *"You render lists in React by mapping an array of data to an array of elements — there's no special loop
  directive, it's just JavaScript's .map()."*
- *"The key prop gives each list item a stable identity so React can match items across renders and update
  only what changed. I use a real ID from the data, not the array index."*
- *"Index keys break on reorder, insert, or delete because the key describes a position, not an item — any
  per-row state ends up attached to the wrong row."*
- *"For two branches I use a ternary; for show-or-nothing I use &&, but I guard numbers so I don't
  accidentally render a 0."*

## Check Yourself

1. How do you render a list of items in JSX, and what does `.map()` return?
2. What does React use the `key` prop for, and what two properties must a key have?
3. Give a concrete scenario where using the array index as a key produces a visible bug.
4. When would you reach for a ternary versus `&&` for conditional rendering?
5. Why can `{count && <Badge/>}` render a stray `0`, and how do you fix it?

**Answers:** (1) Call `.map()` on the data array and return one element per item inside `{ }`; `.map()`
returns an array of elements, which React renders in order. (2) As a stable identity to match each element
to the one from the previous render so it can compute the minimal DOM update; a key must be unique among
its siblings and stable across renders. (3) In an editable/reorderable list — e.g. deleting the first item
shifts every index down, so per-row state like input text or checkbox state stays attached to the position
and jumps to the wrong item. (4) A ternary when choosing between two pieces of UI (this or that); `&&` when
showing one piece of UI or nothing. (5) `0` is falsy so `&&` returns `0`, and React renders the number
`0`; guard with a boolean expression such as `{count > 0 && <Badge/>}`.

## Summary

- Render lists with `.map()`; it returns an array of elements that React renders in order.
- Every list item needs a `key` that is **unique among siblings** and **stable across renders** — use a
  real ID, not the array index.
- Keys let React reconcile efficiently, applying only the minimal DOM changes instead of rebuilding the
  list.
- Index keys corrupt per-item state when a list is reordered, inserted into, or deleted from.
- Conditional UI: ternary `cond ? a : b` for two branches, `&&` for show-or-nothing — guard numbers so you
  do not render a `0`.

## Resources

- [Rendering Lists — react.dev](https://react.dev/learn/rendering-lists)
- [Conditional Rendering — react.dev](https://react.dev/learn/conditional-rendering)
- [Preserving and Resetting State — react.dev](https://react.dev/learn/preserving-and-resetting-state)
