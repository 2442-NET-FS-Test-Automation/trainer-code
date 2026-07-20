# Controlled vs Uncontrolled Components: Who Owns the Input's Value

## Learning Objectives
- Define a controlled component: its value is driven by React state through `onChange`.
- Define an uncontrolled component: the DOM holds the value, read on demand with a ref and `defaultValue`.
- Compare the two and choose deliberately, weighing the trade-offs of each.
- Implement the same form both ways and read the value out correctly in each.

## Why This Matters
Every form in React answers one question: **where does the input's current value actually live?** Either
React state is the source of truth (controlled) or the browser's DOM node is (uncontrolled). Getting this
wrong produces the two classic bugs — an input you cannot type into (you set `value` but forgot
`onChange`), or a form whose data you cannot read back (you never wired the value to anything). "Explain
controlled vs uncontrolled components" is a standard React interview question precisely because it forces
you to say out loud how data flows through a form.

## The Concept

### Controlled: React state is the single source of truth
In a controlled input, you bind the element's `value` to a piece of state and update that state on every
keystroke via `onChange`. The DOM node never holds an independent value — it renders whatever state says,
and every edit round-trips through React. This is the default you should reach for.

```tsx
import { useState, FormEvent } from "react";

export function ControlledSearch() {
  const [title, setTitle] = useState("");

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    console.log("searching for:", title); // value is already in state
  }

  return (
    <form onSubmit={handleSubmit}>
      <input
        value={title}                                  // state drives the DOM
        onChange={(e) => setTitle(e.target.value)}     // every keystroke updates state
        placeholder="Book title"
      />
      <button type="submit" disabled={title.trim() === ""}>Search</button>
    </form>
  );
}
```

Because state is updated on every keystroke, the value is always available to render *right now*. That
enables things uncontrolled inputs cannot do cleanly: disabling the submit button while the field is empty,
live validation messages, formatting as the user types (uppercasing an ISBN), or mirroring the value into
another part of the UI. The cost is a re-render per keystroke and the small ceremony of a state variable
plus a handler per field.

One rule that trips people up: `value={title}` **without** an `onChange` produces a read-only field React
will warn about. If you truly want a fixed value, use `readOnly`; otherwise the pair `value` + `onChange`
always travel together.

### Uncontrolled: the DOM holds the value, you read it with a ref
An uncontrolled input lets the browser do what it has always done — store the value in the DOM node itself.
React does not track it on every keystroke. You seed an optional initial value with `defaultValue` (not
`value`), and when you actually need the data — usually at submit — you read it off the node through a
`ref`.

```tsx
import { useRef, FormEvent } from "react";

export function UncontrolledSearch() {
  const inputRef = useRef<HTMLInputElement>(null);

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    console.log("searching for:", inputRef.current?.value); // read from the DOM on demand
  }

  return (
    <form onSubmit={handleSubmit}>
      <input ref={inputRef} defaultValue="" placeholder="Book title" />
      <button type="submit">Search</button>
    </form>
  );
}
```

Note the two swaps versus the controlled version: `value` becomes `defaultValue` (it sets the initial DOM
value once and then stops caring), and there is no `onChange` — you pull the value with `inputRef.current.value`
only when you need it. No re-render happens as the user types, which is the main appeal. The price is that
the value is invisible to React between reads: you cannot easily disable the button live, validate on the
fly, or react to the field mid-edit without adding back the very machinery you were avoiding.

### The `defaultValue` vs `value` distinction
This single prop choice *is* the fork in the road:

- `value={x}` makes the input **controlled** — React owns it, and you must supply `onChange` or the field
  is frozen.
- `defaultValue={x}` makes the input **uncontrolled** — it sets the starting value once at mount, and the
  DOM owns it thereafter.

Mixing them on one element (both `value` and `defaultValue`) is a mistake React warns about, and switching
a live input from one mode to the other (e.g. `value={x ?? undefined}`) triggers the infamous
"changing an uncontrolled input to be controlled" warning. Pick a mode per field and stay in it.

### Side by side, and when to use each
| | Controlled | Uncontrolled |
|---|---|---|
| Source of truth | React state | The DOM node |
| Initial value prop | `value` | `defaultValue` |
| Reading the value | already in state | `ref.current.value` on demand |
| Re-render per keystroke | yes | no |
| Live validation / conditional UI | easy | awkward |
| Boilerplate | a state var + handler per field | one ref, read at submit |

Reach for **controlled** when you need the value *during* editing — validation as they type, a disabled
submit until the form is valid, dependent fields, formatting, or a controlled component library. Reach for
**uncontrolled** for simple "collect it once on submit" forms, for wrapping non-React widgets, and for the
one case controlled inputs genuinely cannot cover: `<input type="file">`, whose value is always
read-only and therefore always uncontrolled. In modern React the honest default is controlled; uncontrolled
is the deliberate optimization or interop escape hatch, not the starting point.

## Say It in an Interview
- *"A controlled component's value lives in React state — you bind `value` and update it in `onChange`, so
  state is the single source of truth. An uncontrolled component leaves the value in the DOM and you read it
  with a ref when you need it."*
- *"Uncontrolled uses `defaultValue` to seed the field once; controlled uses `value` and requires an
  `onChange`, or the input is read-only."*
- *"Controlled is the default because the value is always available for validation and conditional UI;
  uncontrolled avoids a re-render per keystroke and is how you handle file inputs and wrap non-React
  widgets."*

## Check Yourself
1. What single prop decides whether an input is controlled or uncontrolled, and what is its uncontrolled
   counterpart?
2. You set `value={name}` on an input and it won't let you type. What is missing, and why?
3. In an uncontrolled form, where does the current value live and how do you get it out at submit time?
4. Name one thing a controlled input does easily that an uncontrolled one makes awkward, and one advantage
   of uncontrolled.
5. Which input type is *always* uncontrolled, no matter what you do?

**Answers:** (1) `value` makes it controlled; `defaultValue` seeds an uncontrolled input's initial value.
(2) There is no `onChange`, so state never updates and React keeps re-rendering the old `value` — the field
is effectively read-only; add an `onChange` that calls `setName`. (3) In the DOM node itself; read it via a
ref with `ref.current.value` (typically inside the submit handler). (4) Controlled makes live validation,
conditional/disabled UI, and as-you-type formatting easy; uncontrolled avoids a re-render on every
keystroke and needs less boilerplate. (5) `<input type="file">` — its value is read-only, so it is always
uncontrolled.

## Summary
- The core question is who owns the value: React state (controlled) or the DOM node (uncontrolled).
- Controlled = `value` + `onChange`, value always in state, ideal for validation and conditional UI.
- Uncontrolled = `defaultValue` + a `ref` read on demand, no re-render per keystroke, good for simple or
  interop forms.
- `value` vs `defaultValue` is the switch; never combine them or flip a live input between modes.
- Default to controlled; use uncontrolled deliberately — and always for file inputs.

## Resources
- [Sharing State Between Components / Controlling an input (react.dev)](https://react.dev/learn/sharing-state-between-components)
- [`<input>` component reference (react.dev)](https://react.dev/reference/react-dom/components/input)
- [Manipulating the DOM with Refs (react.dev)](https://react.dev/learn/manipulating-the-dom-with-refs)
