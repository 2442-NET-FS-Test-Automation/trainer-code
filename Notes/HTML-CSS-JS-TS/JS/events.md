# Events: Listeners, the Event Object, and Delegation

## Learning Objectives
- Register and remove handlers with `addEventListener` / `removeEventListener`.
- Name the everyday event types and when each fires.
- Use the event object: `target` vs `currentTarget`, `preventDefault()`, `stopPropagation()`.
- Explain capturing and bubbling, and apply event delegation to handle many elements with one listener.

## Why This Matters
Events are how a static page becomes an application: every click, keystroke, and form submit arrives as
an event that your code chose to listen for. The propagation model (capture down, bubble up) looks like
trivia until you meet delegation — the pattern that makes one listener serve a thousand table rows,
including rows that do not exist yet. Interviewers lean on this area because it separates "I have used
`onclick`" from "I understand the browser's event architecture," and the same model underlies how React
wires its own event system.

## The Concept

### Registering listeners
```js
const btn = document.querySelector("#save");
function handleSave(event) { console.log("saving...", event.type); }

btn.addEventListener("click", handleSave);
btn.removeEventListener("click", handleSave);   // must be the SAME function reference
```
The handler receives an **event object** describing what happened. `removeEventListener` only works if
you pass the identical function reference you registered — which is why registering an inline arrow
(`btn.addEventListener("click", () => ...)`) makes the listener effectively unremovable: the arrow was
anonymous and you kept no reference to it. Fine for listeners that live as long as the page; name the
function when you will ever need to detach it. Recognize the older property style on sight:
`btn.onclick = handleSave` — it works, but one property means one handler (assigning again overwrites),
while `addEventListener` stacks any number and supports options.

### Common events

| Event | Fires when | Typical target |
|---|---|---|
| `click` | pointer press+release (also keyboard activation of buttons/links) | buttons, links, anything |
| `submit` | a form is submitted | the `<form>`, not the button |
| `input` | a field's value changes, every keystroke | text fields, textareas |
| `change` | a field's value is committed (blur, or selection made) | selects, checkboxes, inputs |
| `keydown` | a key goes down (repeats while held) | inputs, or `document` for shortcuts |
| `DOMContentLoaded` | the HTML is fully parsed; DOM is queryable | `document` |
| `load` | the page **and** all assets (images, styles) finished | `window` |

`DOMContentLoaded` vs `load` is a standard follow-up: the first fires when the tree is ready (the usual
"start my script" hook if it loaded before the markup), the second waits for every image and stylesheet.

### The event object
```js
form.addEventListener("submit", (event) => {
  event.preventDefault();                 // stop the browser's default: full-page form submission
  const data = new FormData(event.target);
  // validate / send via fetch instead
});
```
- `event.target` — the element the event actually **originated** on (the clicked `<button>`).
- `event.currentTarget` — the element this **listener is attached to** (the `<form>`, above). Inside a
  handler they differ whenever the event bubbled up from a descendant.
- `event.preventDefault()` — cancels the default browser action while the event keeps propagating:
  stop a form's page-reload submit (form anatomy: `../01-html/forms-inputs.md`), stop a link navigating.
- `event.stopPropagation()` — halts the journey through other elements' listeners, but does **not**
  cancel default behavior. The two are independent and orthogonal.

### Propagation: capture down, bubble up, delegate
When you click a `<button>` inside a `<li>` inside a `<ul>`, the event travels in phases: **capturing**
from `document` down through `ul` and `li` to the target, then **bubbling** from the target back up to
`document`. Listeners fire during **bubbling by default**; pass `{ capture: true }` as the third
argument to fire on the way down instead (rarely needed — interception before children see the event).

Bubbling's practical payoff is **event delegation** — attach one listener to the container and let
descendants' events bubble to it:
```js
document.querySelector("#book-list").addEventListener("click", (event) => {
  const row = event.target.closest("li.book-row");   // find which row the click landed in
  if (!row) return;                                   // click was in the list but not on a row
  row.classList.toggle("selected");
});
```
Why this wins: **one** listener instead of one per row (less memory, nothing to rebind), and — the part
that matters most — rows appended later are handled automatically, because their clicks bubble to the
same container. Any list you render from data should be wired this way rather than looping
`addEventListener` over every row.

### Adjacent: listener options and synthetic events
The options object also takes `once: true` (auto-remove after the first firing — no manual
`removeEventListener` dance) and `passive: true` (a promise you will not call `preventDefault()`, letting
the browser scroll without waiting on scroll/touch handlers). "Synthetic events" is worth recognizing by
name: framework-wrapped event objects (React's) as opposed to the browser's native ones.

## Say It in an Interview
- *"I register with `addEventListener`, which stacks multiple handlers and takes an options object; to
  remove one I must pass the same function reference, so I avoid inline arrows for detachable
  listeners."*
- *"Day to day: `click`, `submit` on the form itself, `input` per keystroke versus `change` on commit,
  `keydown` for shortcuts, and `DOMContentLoaded` versus `load` — DOM parsed versus all assets in."*
- *"`target` is where the event originated, `currentTarget` is where my listener sits; they differ when
  the event bubbled. `preventDefault` cancels the browser's default action, `stopPropagation` stops the
  event travelling — independent concerns."*
- *"Events capture down to the target, then bubble back up, and listeners default to the bubbling phase.
  Delegation exploits that: one listener on the container handles every row — fewer listeners, and rows
  added later just work."*

## Check Yourself
1. Why can a listener registered as an inline arrow function not be removed later?
2. A user types one character into a text field. Which of `input` and `change` has fired so far?
3. In a delegated click handler on a `<ul>`, which is the `<ul>`: `event.target` or
   `event.currentTarget`? What is the other one?
4. You want a link to run JS instead of navigating, without affecting listeners on ancestors. Which
   event-object method, and why not the other one?
5. Give the two reasons event delegation beats a listener per row.

**Answers:** (1) `removeEventListener` needs the identical function reference; the anonymous arrow was
never stored, so nothing matches. (2) Only `input` — `change` waits for the value to be committed (for
example on blur). (3) `event.currentTarget` is the `<ul>` (where the listener is attached);
`event.target` is the actual element clicked inside it. (4) `preventDefault()` — it cancels the default
navigation; `stopPropagation()` would instead silence ancestor listeners and the link would still
navigate. (5) One listener instead of N (memory, no rebinding), and it automatically covers elements
added after the listener was attached, because their events bubble to the same container.

## Summary
- `addEventListener(type, handler, options)` stacks handlers; removal requires the same reference; the
  `onclick` property style allows only one handler.
- Core vocabulary: `click`, `submit` (on the form), `input` vs `change`, `keydown`,
  `DOMContentLoaded` vs `load`.
- Event object: `target` (origin) vs `currentTarget` (listener's element); `preventDefault()` cancels
  default actions; `stopPropagation()` halts travel — orthogonal.
- Propagation is capture down, bubble up; listeners default to bubbling; `{ capture: true }` opts in
  early.
- Delegation: one container listener + `event.target.closest(...)` — fewer listeners, future rows free.
- Options worth knowing: `once`, `passive`.

## Resources
- [EventTarget.addEventListener (MDN)](https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/addEventListener)
- [Bubbling and capturing (javascript.info)](https://javascript.info/bubbling-and-capturing)
- [Event reference (MDN)](https://developer.mozilla.org/en-US/docs/Web/Events)
