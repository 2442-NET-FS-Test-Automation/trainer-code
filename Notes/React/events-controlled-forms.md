# Events and Controlled Forms in React

## Learning Objectives
- Handle DOM events in React with synthetic events, `onClick`, `onSubmit`, and `onChange`.
- Prevent default browser behavior (`e.preventDefault()`) and pass arguments to event handlers.
- Build controlled inputs whose value is driven by React state via `value` and `onChange`.
- Manage multi-field form state and handle submission without a full-page reload.

## Why This Matters
Almost every real interface is a form: a login box, a search bar, a "add to catalog" dialog. React's answer to
"how do I read what the user typed" is the **controlled input** — state is the single source of truth for the
field's value, and every keystroke flows through your code. Interviewers reliably ask you to explain controlled
vs uncontrolled inputs and to write an `onChange` handler from memory. Getting synthetic events, `preventDefault`,
and controlled state right is the difference between a form that works and one that reloads the page or drops
characters.

## The Concept

### Synthetic events
React does not attach native DOM listeners to each element. Instead it wraps native events in a
**SyntheticEvent** — a cross-browser wrapper with the same API you already know (`e.target`, `e.preventDefault()`,
`e.stopPropagation()`) — and delegates from the root. For everyday code this is invisible: you write handlers the
way you would expect, and they behave consistently across browsers.

```tsx
function LikeButton() {
  function handleClick(e: React.MouseEvent<HTMLButtonElement>) {
    console.log("clicked", e.currentTarget.textContent);
  }
  return <button onClick={handleClick}>Like</button>;
}
```

Two things to note. Handlers are named in **camelCase** (`onClick`, not `onclick`), and you pass the **function
itself**, not a call. `onClick={handleClick}` registers the handler; `onClick={handleClick()}` would call it
during render and pass the return value, which is almost never what you want.

### Passing arguments to a handler
When a handler needs extra data, wrap it in an arrow function so it is called at click time, not render time.

```tsx
function BookRow({ id }: { id: number }) {
  function remove(bookId: number) {
    console.log("removing", bookId);
  }

  // Arrow defers the call until the click; the native event is optional
  return <button onClick={() => remove(id)}>Remove</button>;
}
```

If you also need the event, include it: `onClick={(e) => remove(id, e)}`. The mistake to avoid is
`onClick={remove(id)}`, which invokes `remove` immediately on every render.

### Controlled inputs
A **controlled** input takes its displayed value from React state. You set `value={state}` and update that state
in `onChange`. The state is the single source of truth; the DOM never holds a value React does not know about.

```tsx
function TitleField() {
  const [title, setTitle] = useState("");

  return (
    <input
      value={title}
      onChange={e => setTitle(e.target.value)}
      placeholder="Book title"
    />
  );
}
```

The data loop is: user types -> `onChange` fires -> `setTitle` updates state -> component re-renders ->
`value={title}` shows the new text. Because state drives the value, you can transform input on the way in
(uppercase it, trim it, reject non-digits) simply by changing what you store.

The contrast is an **uncontrolled** input, where the DOM keeps the value and you read it later with a ref and
`defaultValue`. Controlled is the default choice in React: it makes validation, conditional disabling, and
resetting trivial because the value always lives in state you can see.

### Managing a whole form
For several fields, hold them in one state object and use a single change handler keyed by the input's `name`.
Spread the previous object so the update stays immutable.

```tsx
interface BookForm {
  title: string;
  author: string;
  available: boolean;
}

function AddBook() {
  const [form, setForm] = useState<BookForm>({
    title: "",
    author: "",
    available: true,
  });

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const { name, value, type, checked } = e.target;
    setForm(prev => ({
      ...prev,
      [name]: type === "checkbox" ? checked : value,
    }));
  }

  return (
    <>
      <input name="title" value={form.title} onChange={handleChange} />
      <input name="author" value={form.author} onChange={handleChange} />
      <label>
        <input
          name="available"
          type="checkbox"
          checked={form.available}
          onChange={handleChange}
        />
        Available
      </label>
    </>
  );
}
```

One handler, driven by the computed key `[name]`, updates whichever field fired. Checkboxes are controlled with
`checked` (a boolean) rather than `value`, which is why the handler branches on `type`.

### Submitting without a page reload
A native `<form>` submit reloads the page — the classic full-page-refresh behavior React is built to avoid. Call
`e.preventDefault()` in the `onSubmit` handler to stop the browser, then do your work in JavaScript.

```tsx
function AddBook() {
  const [form, setForm] = useState<BookForm>({ title: "", author: "", available: true });

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();                 // stop the browser from reloading
    if (!form.title.trim()) return;     // simple validation on state
    console.log("submitting", form);
    setForm({ title: "", author: "", available: true }); // reset by resetting state
  }

  return (
    <form onSubmit={handleSubmit}>
      <input
        name="title"
        value={form.title}
        onChange={e => setForm(prev => ({ ...prev, title: e.target.value }))}
      />
      <button type="submit">Add book</button>
    </form>
  );
}
```

Putting `onSubmit` on the `<form>` (rather than `onClick` on the button) means the Enter key submits too, which
is the accessible, expected behavior. After a successful submit, resetting the form is just resetting the state
object — another payoff of keeping the value in state.

## Say It in an Interview
- *"React events are synthetic events: a cross-browser wrapper over the native event with the same API. Handlers
  are camelCase and I pass the function, not a call."*
- *"A controlled input gets its value from state via `value` and updates state in `onChange`, so state is the
  single source of truth. Uncontrolled inputs keep the value in the DOM and read it with a ref."*
- *"On submit I put `onSubmit` on the form and call `e.preventDefault()` to stop the page reload, then handle the
  data in JavaScript. To pass an argument to a handler I wrap it in an arrow so it runs on the event, not on
  render."*

## Check Yourself
1. What is a synthetic event, and why does React use one instead of the raw DOM event?
2. What is the difference between `onClick={handle}` and `onClick={handle()}`?
3. Which two props make a text input "controlled," and what is the data loop between them?
4. You need one handler to update any of five text fields. How do you write it so it knows which field changed?
5. Why call `e.preventDefault()` in `onSubmit`, and why put `onSubmit` on the form rather than `onClick` on the
   button?

**Answers:** (1) A cross-browser wrapper React puts around the native event (same `target`, `preventDefault`,
etc.); it gives consistent behavior across browsers and lets React use event delegation from the root. (2)
`handle` passes the function so React calls it on the event; `handle()` calls it immediately during render and
passes its return value. (3) `value={state}` and `onChange={e => setState(e.target.value)}`; user types ->
onChange -> setState -> re-render -> value shows new text. (4) Give each input a `name`, read `e.target.name` and
`e.target.value`, and update with a computed key: `setForm(p => ({ ...p, [name]: value }))`. (5)
`preventDefault` stops the browser's full-page reload so you handle submission in JS; `onSubmit` on the form also
fires when the user presses Enter, which is the expected accessible behavior.

## Summary
- React events are **synthetic events**: camelCase props (`onClick`, `onChange`, `onSubmit`), pass the function
  not a call, and wrap in an arrow to pass arguments.
- **Controlled inputs** drive their `value` from state and update it in `onChange`; state is the single source of
  truth, which makes validation and resets trivial.
- Manage multi-field forms with one state object and a single `[name]`-keyed handler; checkboxes use `checked`.
- On submit, call `e.preventDefault()` to stop the page reload and put `onSubmit` on the `<form>` so Enter works.

## Resources
- [Responding to Events (react.dev)](https://react.dev/learn/responding-to-events)
- [Reacting to Input with State (react.dev)](https://react.dev/learn/reacting-to-input-with-state)
- [`<input>` — Controlled components (react.dev)](https://react.dev/reference/react-dom/components/input)
