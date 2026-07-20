# The Component Lifecycle with useEffect

## Learning Objectives
- Describe the three lifecycle phases of a React component: mount, update, and unmount.
- Model those phases in function components with `useEffect` and its cleanup return.
- Control when an effect runs with the dependency array (`[]`, `[dep]`, or none).
- Clean up subscriptions, timers, and listeners on unmount to prevent leaks and stale updates.

## Why This Matters
Components are not static — they appear on screen, react to changing data, and eventually disappear. Anything
that reaches outside React (fetching data, starting a timer, subscribing to an event) has to be started and,
crucially, stopped at the right moments, or you leak memory and get "set state on an unmounted component"
warnings. `useEffect` and its dependency array are how function components hook into this lifecycle, and "explain
the React component lifecycle" plus "what does the dependency array do" are among the most common interview
questions. Misunderstanding the deps array is the number-one source of infinite loops and stale data bugs.

## The Concept

### The three lifecycle phases
Every component moves through the same phases:

- **Mount** — the component is created and inserted into the DOM for the first time.
- **Update** — it re-renders because its state or props changed.
- **Unmount** — it is removed from the DOM (navigated away from, hidden, or its parent stopped rendering it).

Older class components had named methods for these (`componentDidMount`, `componentDidUpdate`,
`componentWillUnmount`). Function components collapse all three into a single hook: `useEffect`. You describe an
effect and *when* it should re-synchronize, and React runs it after the relevant renders and cleans it up before
the next run or on unmount.

### useEffect after render
`useEffect` runs a function **after** the component renders and the DOM is painted. It is where you put side
effects — work that touches something outside React's render output.

```tsx
import { useEffect, useState } from "react";

function Title() {
  const [count, setCount] = useState(0);

  useEffect(() => {
    // side effect: runs after render
    document.title = `Clicked ${count} times`;
  });

  return <button onClick={() => setCount(count + 1)}>Clicked {count}</button>;
}
```

### The dependency array controls when it runs
The second argument to `useEffect` decides how often the effect re-runs. This is the single most important thing
to get right.

```tsx
// No array: runs after EVERY render (rarely what you want)
useEffect(() => {
  console.log("every render");
});

// Empty array: runs ONCE, after the first render (mount only)
useEffect(() => {
  console.log("mounted");
}, []);

// With dependencies: runs on mount and again whenever a listed value changes
useEffect(() => {
  console.log("bookId changed to", bookId);
}, [bookId]);
```

Think of the array as "re-run this effect when any of these values change." React compares each dependency to its
previous value (by reference) after every render; if any differs, it runs the effect again. `[]` lists nothing to
watch, so the effect never re-runs after mount. Omitting the array entirely means "never skip," so it runs every
time — the usual cause of accidental infinite loops when the effect also sets state.

The rule of thumb: **every value from component scope that the effect reads should be in the array.** Leaving one
out gives you a stale closure (the effect keeps using an old value); adding unnecessary values makes it run too
often.

### The cleanup return: unmount and re-sync
If an effect starts something that keeps running — a timer, a subscription, an event listener — it must stop it.
Return a **cleanup function** from the effect. React calls it before the effect runs again *and* when the
component unmounts.

```tsx
import { useEffect, useState } from "react";

function Clock() {
  const [now, setNow] = useState(() => new Date());

  useEffect(() => {
    const id = setInterval(() => setNow(new Date()), 1000);  // start timer on mount
    return () => clearInterval(id);                          // stop it on unmount
  }, []);

  return <p>{now.toLocaleTimeString()}</p>;
}
```

Without the `clearInterval`, the timer would keep firing after the component was gone, trying to update state
that no longer exists — a leak and a warning. The cleanup return is the function-component equivalent of
`componentWillUnmount`.

The cleanup also runs *between* re-runs, which is what makes effects re-synchronize correctly. Subscribing to a
changing target shows the full pattern:

```tsx
useEffect(() => {
  function handleResize() {
    console.log(window.innerWidth);
  }
  window.addEventListener("resize", handleResize);   // subscribe

  return () => {
    window.removeEventListener("resize", handleResize); // unsubscribe before next run / on unmount
  };
}, []);
```

When a dependency changes, React runs the cleanup for the old value first, then the effect for the new value —
old subscription torn down, new one set up. That "clean up, then re-apply" cycle is the heart of how `useEffect`
keeps a component synchronized with the outside world.

### Fetching on mount
The most common effect is loading data once when the component appears. An empty dependency array runs it on
mount; the cleanup flag ignores a response that arrives after the component is gone.

```tsx
useEffect(() => {
  let active = true;
  fetchBooks().then(data => {
    if (active) setBooks(data);   // ignore if we already unmounted
  });
  return () => { active = false; };
}, []);   // mount-only
```

### A note on double-invocation in development
In development with React's Strict Mode, React intentionally mounts, unmounts, and remounts each component once,
so every effect runs twice on the first load. This is a check, not a bug: it surfaces effects that are missing
cleanup. If your effect cleans up properly, the double run is harmless, and it does not happen in production.

## Say It in an Interview
- *"A component mounts, updates when state or props change, and unmounts. Function components model all three with
  `useEffect`: the effect body handles mount and update side effects, and the returned cleanup function handles
  unmount."*
- *"The dependency array controls re-runs: `[]` runs once on mount, `[dep]` runs on mount and whenever `dep`
  changes, and no array runs after every render. Every value the effect reads should be listed."*
- *"Anything I start in an effect — a timer, a subscription, a listener — I stop in the cleanup return, which
  React calls before the next run and on unmount, so there are no leaks or stale updates."*

## Check Yourself
1. Name the three lifecycle phases and the single hook that models all of them in a function component.
2. What is the difference in behavior between `useEffect(fn, [])`, `useEffect(fn, [x])`, and `useEffect(fn)` with
   no array?
3. When does React call the function you return from an effect?
4. You start a `setInterval` in an effect but never clear it. What goes wrong, and what fixes it?
5. Your effect sets state and has no dependency array, and the app freezes. Why, and how do you fix it?

**Answers:** (1) Mount, update, unmount; `useEffect` models all three. (2) `[]` runs once after mount; `[x]`
runs after mount and again whenever `x` changes; no array runs after every render. (3) Before the effect runs
again (when a dependency changed) and when the component unmounts. (4) The timer keeps firing after the component
unmounts, trying to update state that no longer exists — a memory leak and a warning; return `() =>
clearInterval(id)` from the effect. (5) With no array the effect runs after every render; setting state triggers
a render, which reruns the effect, which sets state again — an infinite loop. Add a dependency array (`[]` for
mount-only, or the specific values it should react to).

## Summary
- Components **mount**, **update** (on state/prop change), and **unmount**; function components model all three
  with `useEffect`.
- The effect body runs after render; the **dependency array** controls re-runs: `[]` = mount only, `[dep]` = when
  `dep` changes, no array = every render.
- List every value the effect reads in the array — a missing dep gives stale data, an unnecessary one runs too
  often, and no array while setting state causes infinite loops.
- Return a **cleanup function** to stop timers, subscriptions, and listeners; React runs it before the next
  re-run and on unmount, keeping the component synchronized and leak-free.

## Resources
- [Synchronizing with Effects (react.dev)](https://react.dev/learn/synchronizing-with-effects)
- [Lifecycle of Reactive Effects (react.dev)](https://react.dev/learn/lifecycle-of-reactive-effects)
- [useEffect reference (react.dev)](https://react.dev/reference/react/useEffect)
