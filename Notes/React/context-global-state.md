# Context: Sharing State Without Prop-Drilling

## Learning Objectives
- Explain the prop-drilling problem and how React Context solves it.
- Build the full Context pattern: `createContext`, a Provider component that holds state, and a `useContext` consumer.
- Wrap a custom `useX()` hook around `useContext` with a null-guard so misuse fails loudly.
- Decide when Context is the right tool — and when it is the wrong one.

## Why This Matters
The moment an app grows past a couple of screens, some piece of state — the signed-in user, the theme, the
contents of a cart — needs to be readable in a dozen components scattered across the tree. Passing it down
by hand means threading the same prop through every intermediate component that does not care about it.
That is *prop-drilling*, and it makes components hard to move and hard to read. Context is React's built-in
answer: publish a value once at the top and let any descendant read it directly. "How would you share
global state in React without a library?" is a standard interview question, and "Context plus a custom
hook" is the answer interviewers are listening for.

## The Concept

### The problem: prop-drilling
Suppose the current user is held in state at the top of the app, but only a small `<Avatar>` buried several
layers deep actually needs it. Without Context, every component in between has to accept and forward the
prop:

```tsx
// Every layer forwards `user` even though only Avatar uses it.
function App() {
  const [user] = useState<Identity>({ name: "Ada", role: "member" });
  return <Layout user={user} />;
}
function Layout({ user }: { user: Identity }) {
  return <Header user={user} />;   // Layout does not care about user
}
function Header({ user }: { user: Identity }) {
  return <Avatar user={user} />;   // Header does not care either
}
function Avatar({ user }: { user: Identity }) {
  return <span>{user.name}</span>; // finally, the one component that needs it
}
```

`Layout` and `Header` are now coupled to a prop they only pass along. Add another shared value and every
one of these signatures grows again. Context removes the middlemen.

### Step 1: `createContext`
`createContext` makes a Context object. Its argument is the **default value** used only when a component
reads the Context without any Provider above it. Type the Context so consumers get autocomplete:

```tsx
import { createContext } from "react";

interface Identity {
  name: string;
  role: "member" | "librarian";
}

interface AuthContextValue {
  user: Identity | null;
  login: (name: string, role: Identity["role"]) => void;
  logout: () => void;
}

// The default is null; the null-guard hook below turns "no Provider" into a clear error.
const AuthContext = createContext<AuthContextValue | null>(null);
```

### Step 2: a Provider component that holds the state
A Context object exposes a `.Provider`. Whatever you pass to its `value` prop becomes visible to every
descendant. The idiom is to wrap the Provider in your own component that *owns the state* with `useState`
and hands both the data and the updater functions down through `value`:

```tsx
import { useState, type ReactNode } from "react";

function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<Identity | null>(null);

  const login = (name: string, role: Identity["role"]) => setUser({ name, role });
  const logout = () => setUser(null);

  // Everything inside can now read user/login/logout without props.
  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
```

`children` is the key move: the Provider does not know or care what it wraps. You place it high in the tree
and everything nested inside gains access:

```tsx
function App() {
  return (
    <AuthProvider>
      <Layout />   {/* no more user prop threaded through here */}
    </AuthProvider>
  );
}
```

### Step 3: consume with `useContext`
Any descendant reads the value with the `useContext` hook — no props, no matter how deep it sits:

```tsx
import { useContext } from "react";

function Avatar() {
  const auth = useContext(AuthContext);
  // auth could be null here if there is no Provider — TypeScript forces us to handle it.
  if (!auth?.user) return <span>Guest</span>;
  return <span>{auth.user.name}</span>;
}
```

When the Provider's state changes, every component reading that Context re-renders with the new value.
That is the whole payoff: one update at the top, seen everywhere below, with no forwarding.

### Step 4: wrap it in a custom `useX()` hook with a null-guard
Calling `useContext(AuthContext)` everywhere has two annoyances: consumers keep re-checking for `null`, and
nothing stops someone from reading the Context outside the Provider — which silently yields the default and
produces baffling bugs. The fix is a tiny custom hook that does the check once:

```tsx
function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (context === null) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context; // narrowed to AuthContextValue — never null past this point
}
```

Now consumers are clean, fully typed, and fail loudly the instant they are used in the wrong place:

```tsx
function Avatar() {
  const { user } = useAuth(); // no null check needed; guaranteed present
  return <span>{user ? user.name : "Guest"}</span>;
}

function LogoutButton() {
  const { logout } = useAuth();
  return <button onClick={logout}>Sign out</button>;
}
```

This `createContext` + Provider-holding-state + `useContext` + custom-hook shape is the standard way to
manage a slice of global application state in React.

### When NOT to use Context
Context is not a general-purpose state manager and it is not free. Two cautions:

- **It is not for state that changes constantly.** Every consumer re-renders on every value change. A value
  that updates many times per second (mouse position, a text field) will re-render half the tree. Keep
  fast-changing state local.
- **It is not a substitute for props.** If only one child needs a value, pass a prop. Reach for Context when
  a value is genuinely cross-cutting — auth, theme, locale, a cart — and drilling it would touch many
  uninterested components. Over-using Context couples components to a global and makes them harder to test.

For large apps with heavy, frequently-updated shared state, dedicated libraries (Redux Toolkit, Zustand)
add selective subscriptions that Context lacks. Context shines for a handful of stable, app-wide values.

## Say It in an Interview
- *"Prop-drilling is passing a prop through components that don't use it just to reach a deep one. Context
  lets you publish a value at the top with a Provider and read it anywhere below with useContext."*
- *"The pattern is createContext for the object, a Provider component that owns the state with useState and
  exposes it through value, and useContext to consume it."*
- *"I wrap useContext in a custom hook like useAuth that throws if the Context is null, so using it outside
  its Provider is a loud error instead of a silent default."*
- *"Context isn't a full state manager — every consumer re-renders when the value changes, so I keep it for
  stable cross-cutting state like auth or theme, not for fast-changing or purely local state."*

## Check Yourself
1. What exact problem does Context solve, and what is that problem called?
2. What are the three pieces of the core Context pattern?
3. What is the argument to `createContext` actually for?
4. Why wrap `useContext` in a custom hook, and what should that hook do when the Context is `null`?
5. Give one kind of state you should *not* put in Context, and why.

**Answers:** (1) Passing shared state through components that don't need it just to reach a deep consumer —
prop-drilling. (2) `createContext` (the Context object), a Provider component that holds the state and
exposes it via `value`, and `useContext` to read it. (3) The default value, used only when a component
reads the Context with no Provider above it. (4) To centralize the null-check and typing; it should `throw`
a clear error so using the Context outside its Provider fails loudly instead of returning the default. (5)
Fast-changing state (mouse position, an input's text) — every consumer re-renders on each change, so
frequent updates re-render large parts of the tree.

## Summary
- Context removes prop-drilling: publish a value once with a Provider, read it anywhere with `useContext`.
- The pattern has three parts: `createContext`, a Provider component that owns state and exposes it through
  `value`, and `useContext` consumers.
- `createContext`'s argument is the default, used only when there is no Provider above the consumer.
- A custom `useX()` hook wraps `useContext` with a null-guard, giving clean typed access that throws on
  misuse.
- Use Context for stable cross-cutting state (auth, theme, locale); avoid it for fast-changing or purely
  local state, since every consumer re-renders on each value change.

## Resources
- [Passing Data Deeply with Context (react.dev)](https://react.dev/learn/passing-data-deeply-with-context)
- [useContext (react.dev)](https://react.dev/reference/react/useContext)
- [createContext (react.dev)](https://react.dev/reference/react/createContext)
