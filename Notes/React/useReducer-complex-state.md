# useReducer: Centralizing Complex State Transitions

## Learning Objectives
- Read the reducer signature `(state, action) => newState` and explain each part.
- Model actions as a TypeScript discriminated union and narrow them in a `switch`.
- Drive a small state machine (idle to loading to success or error) with `useReducer` and `dispatch`.
- Choose between `useReducer` and `useState`, and always return new immutable state from the reducer.

## Why This Matters
`useState` is perfect until the pieces of state start depending on each other. When "loading is true" must
also mean "error is null and data is untouched," scattering three separate `setX` calls across event
handlers invites contradictory states — a spinner showing next to an error message that shouldn't exist.
`useReducer` gathers all the transitions into one function so the *rules* of how state may change live in a
single place, expressed as a state machine. Interviewers ask "when would you reach for useReducer over
useState?" precisely because knowing the answer signals you can manage non-trivial state without it turning
into spaghetti.

## The Concept

### The reducer signature: `(state, action) => newState`
A reducer is a pure function that takes the **current state** and an **action** describing what happened,
and returns the **next state**. It never mutates its inputs and never reaches outside itself (no `fetch`, no
timers) — same inputs, same output, every time:

```ts
function reducer(state: State, action: Action): State {
  // decide the next state based on what happened, and return it
}
```

`useReducer` wires this function into a component. It takes the reducer and an initial state and returns the
current state plus a `dispatch` function. You call `dispatch(action)` to send an action; React runs the
reducer and re-renders with whatever it returned:

```tsx
const [state, dispatch] = useReducer(reducer, initialState);
```

### Actions as a discriminated union
In TypeScript, model the set of possible actions as a **discriminated union** — a union of object types that
share a literal `type` field. This is what makes the reducer type-safe: once you `switch` on `action.type`,
TypeScript narrows the action inside each `case` so you get exactly the payload that action carries, and it
flags any action you forgot to handle.

```ts
interface Identity {
  name: string;
  role: "member" | "librarian";
}

// Each action names something that happened; some carry a payload, some don't.
type AuthAction =
  | { type: "login_start" }
  | { type: "login_success"; user: Identity }
  | { type: "login_error"; message: string }
  | { type: "logout" };
```

Because the union is closed, adding a fifth action type and forgetting a `case` becomes a compile-time
error (with `strict` settings), not a runtime surprise.

### A state machine with useReducer
Here is the classic use: a status field that can only be one of a few known values, with data and error
that must stay consistent with it. Modeling `status` as a union of string literals makes impossible states
unrepresentable.

```tsx
import { useReducer } from "react";

interface State {
  status: "idle" | "loading" | "success" | "error";
  user: Identity | null;
  error: string | null;
}

const initialState: State = { status: "idle", user: null, error: null };

function authReducer(state: State, action: AuthAction): State {
  switch (action.type) {
    case "login_start":
      // Entering loading clears any old error and old user in one atomic move.
      return { status: "loading", user: null, error: null };
    case "login_success":
      // action is narrowed here — TypeScript knows action.user exists.
      return { status: "success", user: action.user, error: null };
    case "login_error":
      return { status: "error", user: null, error: action.message };
    case "logout":
      return initialState;
    default:
      // Exhaustiveness guard: if a new action type is added and not handled,
      // this line fails to compile.
      const _exhaustive: never = action;
      return state;
  }
}
```

The component dispatches actions; it never manipulates the state shape directly:

```tsx
function LoginPanel() {
  const [state, dispatch] = useReducer(authReducer, initialState);

  async function handleLogin(name: string) {
    dispatch({ type: "login_start" });
    try {
      const user = await signIn(name); // some async call returning an Identity
      dispatch({ type: "login_success", user });
    } catch (err) {
      dispatch({ type: "login_error", message: (err as Error).message });
    }
  }

  if (state.status === "loading") return <p>Signing in...</p>;
  if (state.status === "error") return <p>Problem: {state.error}</p>;
  if (state.status === "success") return <p>Welcome, {state.user!.name}</p>;
  return <button onClick={() => handleLogin("Ada")}>Sign in</button>;
}
```

Every legal transition is visible in one `switch`. To understand how login behaves, you read the reducer —
not five scattered handlers.

### Immutable returns are mandatory
A reducer must return a **new** object, never mutate the old one. React decides whether to re-render by
comparing the returned reference to the previous state; mutating in place keeps the same reference and the
UI can silently fail to update. Build new objects and arrays with spreads:

```ts
// WRONG: mutates the existing state, same reference — React may not re-render.
case "add_book":
  state.books.push(action.book);
  return state;

// RIGHT: new array, new state object — a fresh reference React can detect.
case "add_book":
  return { ...state, books: [...state.books, action.book] };
```

The same immutability rule that governs `useState` governs reducers; the reducer just concentrates it in
one auditable place.

### useReducer vs useState
Both manage state and both trigger re-renders. Choose by the shape of the problem:

| Reach for `useState` when | Reach for `useReducer` when |
|---|---|
| One or two independent values | Several fields that must change together consistently |
| Updates are simple `setX(newValue)` | State is a machine with named transitions (idle/loading/success/error) |
| No interdependence between pieces | The next state depends on the current state in non-trivial ways |
| Logic is trivial and local | You want transition logic testable in isolation (a reducer is a pure function) |

`useReducer` is not "better" — it is heavier ceremony that pays off when state gets complex. A counter
stays `useState`. A multi-step form, an async status machine, or a cart with add/remove/update-quantity is
where a reducer earns its keep. A useful tell: if you find yourself calling several `setX` functions
together to keep them in sync, that is a reducer waiting to be written.

## Say It in an Interview
- *"A reducer is a pure function (state, action) => newState. useReducer gives you the current state and a
  dispatch function; you dispatch actions and React runs the reducer to get the next state."*
- *"I type actions as a discriminated union on a literal `type` field, then switch on it — TypeScript
  narrows each case to its payload and flags any action I forgot to handle."*
- *"I reach for useReducer over useState when several fields have to change together or the state is really
  a machine with named transitions like idle, loading, success, error."*
- *"The reducer must return new immutable state — a fresh reference — or React can't detect the change and
  the UI won't update."*

## Check Yourself
1. Write the reducer signature and say what each of the three parts is.
2. What does `useReducer` return, and how do you trigger a state change?
3. Why model actions as a discriminated union instead of a loose object with an optional payload?
4. Give a concrete situation where `useReducer` is clearly the better choice than `useState`.
5. What goes wrong if a reducer mutates and returns the same state object?

**Answers:** (1) `(state, action) => newState`: the current state, an action describing what happened, and
the returned next state. (2) A `[state, dispatch]` pair; you call `dispatch(action)` and React runs the
reducer to produce the next state and re-renders. (3) The union closes the set of actions so `switch`
narrows each case to exactly its payload and the compiler catches unhandled action types — a loose object
gives no narrowing and no exhaustiveness checking. (4) An async status machine (idle to loading to success
or error) where status, data, and error must stay mutually consistent — or any state where several fields
must update together. (5) React compares references to decide whether to re-render; the same reference looks
unchanged, so the component can silently fail to update.

## Summary
- A reducer is a pure function `(state, action) => newState`; `useReducer(reducer, initial)` returns
  `[state, dispatch]`.
- Type actions as a discriminated union on a literal `type` field so a `switch` narrows each case and
  catches unhandled actions.
- `useReducer` shines for state machines and interdependent fields — model `status` as a union of literals
  to make impossible states unrepresentable.
- Always return new immutable state; mutating in place keeps the same reference and can skip the re-render.
- Prefer `useState` for simple, independent values; reach for `useReducer` when several fields must move
  together or transitions get complex — a sign is calling several setters in sync.

## Resources
- [useReducer (react.dev)](https://react.dev/reference/react/useReducer)
- [Extracting State Logic into a Reducer (react.dev)](https://react.dev/learn/extracting-state-logic-into-a-reducer)
- [Discriminated unions (TypeScript Handbook)](https://www.typescriptlang.org/docs/handbook/2/narrowing.html#discriminated-unions)
