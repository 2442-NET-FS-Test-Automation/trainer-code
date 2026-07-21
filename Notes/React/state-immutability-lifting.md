# State Immutability and Lifting State Up

## Learning Objectives

- Explain why React state must be treated as immutable, and how React decides a component needs to re-render.
- Produce new arrays and objects instead of mutating existing ones, using spread, `map`, and `filter`.
- Recognize the mutation traps that silently break re-rendering (`push`, index assignment, `Object.assign` onto old state).
- Lift shared state up to the closest common ancestor so sibling components stay in sync.

## Why This Matters

The single most common "my screen didn't update" bug in React is a mutation bug: you changed the data, but you
changed the *same object* React already had, so React saw no change and skipped the re-render. Immutability is
not a style preference here — it is how React's change detection works. And once two components need the same
data, the answer is almost always "lift the state up," which is a near-guaranteed interview question and the
foundation of one-way data flow. Get these two ideas right and most React state confusion disappears.

## The Concept

### React compares references, not contents

When state changes, React re-renders the component. But how does it know state changed? For state set with
`useState`, React compares the **new value to the old value by reference** (an `Object.is` comparison). If you
hand it the *same array or object* you already had — even after modifying its insides — the reference is
identical, so React concludes nothing changed and does not re-render.

```tsx
const [books, setBooks] = useState<Book[]>([]);

// BROKEN: mutates the existing array, then passes the SAME reference back
function addBroken(newBook: Book) {
  books.push(newBook);   // array contents changed...
  setBooks(books);       // ...but it is the same reference -> React skips the render
}

// CORRECT: build a brand-new array; new reference -> React re-renders
function addBook(newBook: Book) {
  setBooks([...books, newBook]);
}
```

The rule that follows: **never mutate state in place.** Always produce a *new* array or object and pass that to
the setter. "New" means a new reference, which is exactly the signal React is looking for.

### Updating arrays without mutating

Every array operation has an immutable form. The mutating methods (`push`, `pop`, `splice`, `sort`, `reverse`)
change the array in place; reach for the ones that return a new array instead.

```tsx
interface Book {
  id: number;
  title: string;
  available: boolean;
}

// ADD: spread the old items into a new array, append the new one
setBooks([...books, newBook]);

// REMOVE by id: filter returns a new array
setBooks(books.filter(b => b.id !== targetId));

// UPDATE one item: map returns a new array; replace the match with a NEW object
setBooks(books.map(b =>
  b.id === targetId ? { ...b, available: false } : b
));
```

Notice the update case does two immutable copies at once: `map` makes a new *array*, and `{ ...b, available:
false }` makes a new *object* for the one row that changed. The rows that did not change keep their old
references, which lets React skip re-rendering them.

### Updating objects without mutating

The same discipline applies to object state. Spread the old object, then override the fields you are changing.

```tsx
interface Profile {
  name: string;
  address: { city: string; zip: string };
}

const [profile, setProfile] = useState<Profile>({
  name: "Ada",
  address: { city: "London", zip: "N1" },
});

// CORRECT: new top-level object
setProfile({ ...profile, name: "Grace" });

// Nested update: spread every level you touch
setProfile({
  ...profile,
  address: { ...profile.address, city: "Oxford" },
});
```

A trap worth naming: `Object.assign(profile, { name: "Grace" })` **mutates** `profile` in place and returns
that same reference — the exact failure mode from the array example. `Object.assign({}, profile, {...})`, with a
fresh `{}` as the target, is safe because it writes into a new object. When in doubt, spread into a new literal.

### The functional updater form

When the next state depends on the previous state — and especially when several updates could be batched
together — pass a **function** to the setter. React calls it with the guaranteed-latest value.

```tsx
// Prefer this when new state is derived from old state
setBooks(prev => [...prev, newBook]);
setCount(prev => prev + 1);
```

This is still immutable: `prev` is the old array, and you return a new one. The updater form protects you from
stale-closure bugs where `books` captured an out-of-date value.

### Lifting state up

Two sibling components cannot see each other's state — state is local to the component that declares it. When
both need the same data, **move the state up to their closest common ancestor** and pass it down. The parent
owns the state; children receive the value as a prop and a callback to request changes.

Consider a search box and a results list that must react to the same query. Neither sibling can hold the query
alone. Lift it to the parent:

```tsx
function Library() {
  // State lives in the common ancestor
  const [query, setQuery] = useState("");
  const [books] = useState<Book[]>(initialBooks);

  const visible = books.filter(b =>
    b.title.toLowerCase().includes(query.toLowerCase())
  );

  return (
    <div>
      <SearchBox query={query} onQueryChange={setQuery} />
      <BookList books={visible} />
    </div>
  );
}

function SearchBox({ query, onQueryChange }: {
  query: string;
  onQueryChange: (next: string) => void;
}) {
  // No state of its own: reads the value, reports changes upward
  return (
    <input
      value={query}
      onChange={e => onQueryChange(e.target.value)}
      placeholder="Search titles"
    />
  );
}

function BookList({ books }: { books: Book[] }) {
  return (
    <ul>
      {books.map(b => <li key={b.id}>{b.title}</li>)}
    </ul>
  );
}
```

Data flows **down** as props (`query`, `books`); change requests flow **up** through the callback
(`onQueryChange`). This is one-way data flow: the single source of truth is the parent, and both children stay
consistent because they read from and write to the same place. Lift state to the *closest* common ancestor — no
higher — so the fewest components re-render and the data stays as local as it can be.

## Say It in an Interview

- *"React detects state changes by reference, not by deep comparison. If I mutate the existing object and pass
  it back, the reference is unchanged, so React skips the re-render. That is why state must be immutable."*
- *"To update immutably I build a new value: spread into a new array or object, use map to replace one item,
  filter to remove one. Mutating methods like push and Object.assign onto old state are the trap."*
- *"Lifting state up means moving shared state to the closest common ancestor of the components that need it,
  then passing the value down as props and a callback back up. That keeps siblings in sync with one source of
  truth."*

## Check Yourself

1. You call `books.push(newBook)` then `setBooks(books)` and the list does not update. Why not?
2. Which array methods return a new array (safe for state) and which mutate in place?
3. Write the immutable update that flips `available` to `false` for the book whose `id` is `7`, leaving all
   others untouched.
4. Why does `Object.assign(state, patch)` break re-rendering while `{ ...state, ...patch }` does not?
5. A search box and a results list both need the current query. Where should the query state live, and how does
   each component interact with it?

**Answers:** (1) `push` mutated the existing array; `setBooks(books)` passed back the same reference, so
React's `Object.is` check saw no change and skipped the render. (2) New array: `map`, `filter`, `concat`,
spread, `slice`. Mutating: `push`, `pop`, `shift`, `splice`, `sort`, `reverse`. (3) `setBooks(books.map(b =>
b.id === 7 ? { ...b, available: false } : b))`. (4) `Object.assign(state, patch)` writes into the existing
object and returns that same reference, so React sees no change; the spread creates a new object (new
reference). (5) In the closest common ancestor (their parent); the parent passes `query` down as a prop and a
setter/callback down, the search box reports changes up through it, the list reads the value down.

## Summary

- React decides whether to re-render by comparing state **by reference**; mutating in place keeps the same
  reference and silently skips the update.
- Update immutably: `[...arr, x]` to add, `filter` to remove, `map` (returning `{ ...item, ...change }`) to
  edit; spread objects and spread every nested level you touch.
- Avoid `push`/`splice`/`sort` and `Object.assign` onto existing state; use the functional updater
  (`setX(prev => ...)`) when the next value depends on the previous.
- Lift shared state to the **closest common ancestor**, pass data down as props and change requests up as
  callbacks — one source of truth, one-way data flow.

## Resources

- [Updating Objects in State (react.dev)](https://react.dev/learn/updating-objects-in-state)
- [Updating Arrays in State (react.dev)](https://react.dev/learn/updating-arrays-in-state)
- [Sharing State Between Components (react.dev)](https://react.dev/learn/sharing-state-between-components)
