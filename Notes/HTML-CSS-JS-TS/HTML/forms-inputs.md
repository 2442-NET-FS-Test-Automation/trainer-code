# Forms and Inputs: Collecting User Data in HTML

## Learning Objectives
- Construct an HTML form and explain how `action`, `method`, and `name` shape what gets submitted.
- Take in user input with the full input vocabulary: text-family inputs, checkboxes, radios, dates,
  files, `<select>`, `<textarea>`, buttons, and properly associated `<label>`s.
- Recognize the built-in validation attributes and state the limits of client-side validation.

## Why This Matters
Forms are how the web takes input — logins, checkouts, search boxes, signup flows. Every field you
render ends up as a key-value pair hitting some server endpoint, so form markup is where front end and
back end shake hands: get the `name` attributes wrong and the server receives nothing. Forms are also
where accessibility failures cost most (an unlabeled input is unusable by screen reader) and where a
classic security mistake lives — trusting the browser to validate. Interviewers reliably ask GET vs
POST and "where should validation live?"

## The Concept

### The form element and submission
A `<form>` wraps controls and defines **where** and **how** their values are sent:

```html
<form action="/members/register" method="post">
  <label for="email">Email</label>
  <input type="email" id="email" name="email" required>
  <button type="submit">Register</button>
</form>
```

On submit, the browser collects every control that has a **`name`** and sends `name=value` pairs to the
`action` URL. No `name`, no submission — the single most common "why is my field missing?" bug (`id` is
for labels and scripts; only `name` reaches the server).

| Method | Where the data goes | Use for |
|---|---|---|
| `GET` | appended to the URL: `/search?title=dune&max=10` | reads: searches, filters — bookmarkable, visible |
| `POST` | in the request body | writes: registration, checkout — not in the URL or history |

Rule of thumb mirroring HTTP semantics: GET only fetches; POST for anything that changes state or
carries secrets (a password in a GET URL lands in server logs and history). Adjacent one-liner: when
JavaScript handles submission itself (validate, then send via `fetch`), the handler calls
`event.preventDefault()` to stop this native navigation — details in `../03-javascript/events.md`.

### The input vocabulary
One element, many personalities — `<input type="...">` — plus three non-input controls:

| Control | Renders as | Notes |
|---|---|---|
| `type="text"` | one-line text box | the default when `type` is omitted |
| `type="password"` | masked text box | masking is display-only, not encryption |
| `type="email"` / `type="number"` | text box with format rules | free format checking + mobile keyboards |
| `type="checkbox"` | independent on/off boxes | each box its own `name`; submits only when checked |
| `type="radio"` | pick-one-of-a-set | **grouped by sharing a `name`** — same name = one choice |
| `type="date"` | native date picker | submits `yyyy-mm-dd` |
| `type="file"` | file chooser | needs `method="post"` and `enctype="multipart/form-data"` |
| `<select>` + `<option>` | dropdown | submits the chosen option's `value` |
| `<textarea>` | multi-line text | a real element with content, not void |
| `<button type="submit">` | submits the form | the **default** type inside a form |
| `<button type="button">` | does nothing natively | for JS-driven actions — forgetting this causes surprise submits |

```html
<fieldset>
  <legend>Membership tier</legend>
  <label><input type="radio" name="tier" value="basic" checked> Basic</label>
  <label><input type="radio" name="tier" value="premium"> Premium</label>
</fieldset>

<label for="branch">Home branch</label>
<select id="branch" name="branch">
  <option value="north">North Branch</option>
  <option value="south">South Branch</option>
</select>

<label><input type="checkbox" name="newsletter" value="yes"> Email me the newsletter</label>
```

### Labels: the accessibility contract
Every control needs a `<label>`, associated by `for` pointing at the control's `id`, or by wrapping
the control (as the radio/checkbox examples above do). The payoff: screen readers announce the label
on focus, and clicking the label focuses or toggles the control — a big deal for small checkbox
targets. Placeholder text is **not** a label: it vanishes on typing and many screen readers skip it.

### Built-in validation — and its limits
Validation attributes go straight on the control; the browser blocks submission and shows a message
when they fail. Recognize these on sight:

| Attribute | Meaning |
|---|---|
| `required` | field must be filled/checked before submit |
| `min="1"` / `max="20"` | numeric or date bounds (`minlength`/`maxlength` for text length) |
| `pattern="[A-Z]{2}-\d{4}"` | value must match the regex (e.g., a SKU like `BK-1041`) |

```html
<input type="number" name="copies" min="1" max="20" required>
```

**The hard limit: client-side validation is UX, never security.** Anyone can bypass it — delete the
attributes in DevTools, or skip the browser entirely and send the request with curl. Built-in checks
exist for fast, friendly feedback; the server must independently revalidate everything it receives.
Complements, not alternatives: browser checks for convenience, server checks for safety.

## Say It in an Interview
- *"A form's action says where the data goes and method says how: GET puts name-value pairs in the URL —
  right for bookmarkable searches — and POST puts them in the body, right for state changes and anything
  sensitive. Only controls with a name attribute are submitted at all."*
- *"HTML gives one input element with many types — text, password, email, number, checkbox, date, file —
  plus radios grouped by a shared name so only one can be picked, select for dropdowns, textarea for
  multi-line text, and buttons where type submit is the default. Every control gets a label via for and
  id, which screen readers announce and clicks activate."*
- *"Attributes like required, min, max, and pattern give free browser-side validation, but that's purely
  user experience — anyone can bypass the browser — so the server always revalidates."*

## Check Yourself
1. A text field renders and the user types in it, but the server receives nothing for it. What is the
   most likely missing attribute, and why doesn't `id` fix it?
2. Why is `method="get"` wrong for a login form? Give two concrete consequences.
3. Write three radio buttons for sizes S/M/L that permit exactly one selection.
4. What does `<label for="qty">` require on the input, and name both things the association buys you.
5. Your form has `required` and a `pattern` on every field. Why must the server still validate?

**Answers:** (1) `name` — the browser submits only `name=value` pairs; `id` is for labels and scripts
and never reaches the server. (2) The password lands in the URL — browser history, server logs, shared
links; GET also signals a safe read, which a login is not. (3) `<label><input type="radio" name="size"
value="s"> S</label>` and likewise `m`/`l` — exclusivity comes from the shared `name`. (4) `id="qty"`
on the input; screen readers announce the label on focus, and clicking it focuses the control.
(5) Client-side checks run in software the user controls — DevTools or curl bypasses them — so they
are UX only; the server is the trust boundary.

## Summary
- `<form action method>`: destination and verb; only `name`d controls submit; GET = URL (bookmarkable
  reads), POST = body (state changes, secrets).
- Vocabulary: `text`, `password`, `email`, `number`, `checkbox`, `radio` (grouped by shared `name`),
  `date`, `file`, `<select>/<option>`, `<textarea>`, `<button>` (default type in a form is `submit`).
- Every control gets a `<label>` via `for`/`id` or wrapping; placeholders are not labels.
- `required`, `min`/`max`, `pattern` = free instant feedback — UX only; the server must revalidate.
- JS-handled forms call `event.preventDefault()` in the submit handler to stop native submission.

## Resources
- [Web forms — working with user data (MDN Learn)](https://developer.mozilla.org/en-US/docs/Learn_web_development/Extensions/Forms)
- [The input element (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input)
- [Learn Forms (web.dev)](https://web.dev/learn/forms)
