# DOM Selection and Manipulation

## Learning Objectives
- Describe what the DOM is and how `document` is the entry point JavaScript uses to reach the page.
- Query elements with `querySelector`, `querySelectorAll`, and `getElementById`.
- Read and modify elements: `textContent` vs `innerHTML`, `setAttribute`, `classList`, and `style`.
- Insert and remove elements: `createElement` + `append`, `element.remove()`, and rendering a list
  from an array of data.
- Use template literals to build strings and markup with `${}` interpolation.

## Why This Matters
Every dynamic behavior on a web page — showing search results, marking a row selected, painting an
error message — is JavaScript editing the DOM. Frameworks like React exist precisely to manage these
same operations for you, so understanding raw DOM work is understanding what your framework does under
the hood. It is also a stock interview area: `textContent` vs `innerHTML` and "how would you render a
list from an array" come up constantly, and the XSS angle turns a syntax question into a security one.

## The Concept

### The DOM: the page as a live tree
When the browser parses HTML it builds the **Document Object Model**: an in-memory tree of node objects,
one per element/attribute/text run. The rendered page is a projection of this tree — change a node and
the browser re-renders that part of the screen. JavaScript reaches the tree through the global
`document` object, the root of every query. The DOM is an API standard, not part of the JavaScript
language: it is the browser environment's object model that JS happens to script against.

### Querying elements
```js
const nav   = document.getElementById("nav");          // fastest, id only, returns one element
const card  = document.querySelector(".card");         // first match for any CSS selector
const items = document.querySelectorAll("ul.books li"); // ALL matches, as a static NodeList
```
`querySelector`/`querySelectorAll` accept full **CSS selector strings** — the exact syntax covered in
`../02-css/css-fundamentals-selectors.md` (`"#id"`, `".class"`, `"li.done"`, `"form input[type=email]"`).
`querySelectorAll` returns a **static NodeList**: a snapshot that does not change as the page changes,
and which supports `forEach`. The older `getElementsByClassName`/`getElementsByTagName` return a **live
HTMLCollection** that updates itself as matching elements appear — convenient occasionally, but a
classic source of surprise when you mutate the DOM while iterating it. A missed match returns `null`
(single) or an empty list, so `document.querySelector(".gone").textContent` throws — check before use.

### Reading and modifying elements

| API | What it does | Watch for |
|---|---|---|
| `el.textContent` | get/set plain text; markup is inert | safe default for any data |
| `el.innerHTML` | get/set the element's HTML markup | **parses and executes as HTML — XSS risk** |
| `el.setAttribute("href", url)` / `el.getAttribute(...)` | generic attribute access | many attributes also exist as properties (`el.href`) |
| `el.classList.add/remove/toggle("done")` | class management | far better than rewriting `className` |
| `el.style.backgroundColor = "gold"` | inline styles, camelCased properties | prefer toggling a class for anything reusable |

The `textContent` vs `innerHTML` distinction is the one to internalize: `innerHTML` interprets its input
as markup, so writing **untrusted data** (user input, API responses) with it lets an attacker inject
`<img onerror=...>` and run script in your page — a cross-site scripting (XSS) hole. Rule: data goes in
via `textContent`; `innerHTML` is for markup *you* wrote.

### Creating, inserting, and removing
```js
const books = [{ title: "Dune", author: "Herbert" }, { title: "Emma", author: "Austen" }];
const list = document.querySelector("#book-list");

for (const book of books) {
  const li = document.createElement("li");
  li.textContent = `${book.title} — ${book.author}`;   // data via textContent: XSS-safe
  li.classList.add("book-row");
  list.append(li);
}

list.firstElementChild.remove();                        // elements remove themselves
```
This **render-a-collection** pattern — array of data in, one element per item out — is the heart of
every list, table, and card grid you will ever build, and it is exactly what a framework's list
rendering automates. `append` accepts multiple nodes and plain strings; the older `appendChild` takes
exactly one node and returns it. Recognize `insertAdjacentHTML("beforeend", "<li>...</li>")` on sight:
it parses a markup string into a position relative to an element — concise, but it is `innerHTML`'s
sibling and carries the same XSS caveat for untrusted input.

### Template literals
Backtick strings interpolate expressions with `${...}` and may span multiple lines:
```js
const banner = `<article class="card">
  <h2>${book.title}</h2>
  <p>${book.author}</p>
</article>`;
```
They are the standard way to build any string from data — messages, URLs, markup. When the built string
is fed to `innerHTML`/`insertAdjacentHTML`, every interpolated value is a potential injection point, so
the same rule applies: only interpolate untrusted input into markup after escaping it, or assign it
separately with `textContent`.

### Adjacent: the cost of touching the DOM in a loop
Every DOM write can force the browser to recompute layout (**reflow**) and redraw pixels (**repaint**),
so appending to a live list 1,000 times inside a loop is far slower than building the elements first and
attaching once. A `DocumentFragment` is a lightweight off-screen container: append rows to it in the
loop, then `list.append(fragment)` for a single live-tree insertion.

## Say It in an Interview
- *"The DOM is the browser's in-memory tree of the page — one node per element. JavaScript reads and
  edits that tree through the `document` object, and the browser re-renders whatever changed."*
- *"I query with `querySelector` and `querySelectorAll`, which take any CSS selector and return the
  first match or a static NodeList; `getElementById` is the fast path when I have an id."*
- *"`textContent` treats input as inert text, `innerHTML` parses it as markup — so untrusted data goes
  through `textContent`, because writing it with `innerHTML` is an XSS vulnerability."*
- *"To render a collection I loop the array, `createElement` a row per item, set its text and classes,
  and append — ideally building everything before attaching, so I only trigger one reflow."*
- *"Template literals are backtick strings with `${}` interpolation and multi-line support; they're how
  I compose markup strings, with the same escaping care for anything user-supplied."*

## Check Yourself
1. What does `querySelectorAll` return, and how does it differ from what `getElementsByClassName`
   returns?
2. You need to display a user-supplied comment inside a `<p>`. Which property do you assign, and what
   goes wrong with the other one?
3. Write the three `classList` calls that add, remove, and flip a `hidden` class on `el`.
4. Sketch the render-a-collection pattern: given `const tags = ["js", "css"]`, produce an `<li>` per
   tag inside `#tag-list`.
5. Why is "build all the rows, then attach once" faster than appending inside the loop?

**Answers:** (1) A static NodeList — a snapshot supporting `forEach`; `getElementsByClassName` returns a
live HTMLCollection that mutates as the document changes. (2) `p.textContent = comment` — using
`innerHTML` would parse the comment as HTML, letting injected tags execute (XSS). (3)
`el.classList.add("hidden")`, `el.classList.remove("hidden")`, `el.classList.toggle("hidden")`.
(4) `for (const t of tags) { const li = document.createElement("li"); li.textContent = t;
document.querySelector("#tag-list").append(li); }` (5) Each live-DOM write can trigger reflow/repaint;
batching into a DocumentFragment (or building detached) reduces that to one layout pass.

## Summary
- The DOM is the in-memory tree of the page; `document` is the entry point and edits re-render the page.
- Query with CSS selectors: `querySelector` (first match), `querySelectorAll` (static NodeList),
  `getElementById` (fast id path); live HTMLCollections come from the older `getElementsBy*` family.
- `textContent` for data, `innerHTML` only for trusted markup — the difference is an XSS boundary.
- `classList.add/remove/toggle` for classes; `createElement` + `append` + `remove()` for structure; the
  array-to-elements loop is the render-a-collection pattern.
- Template literals build interpolated, multi-line strings; interpolating untrusted input into markup
  re-opens the XSS door.
- Batch DOM writes (DocumentFragment) to avoid reflow-per-iteration.

## Resources
- [Introduction to the DOM (MDN)](https://developer.mozilla.org/en-US/docs/Web/API/Document_Object_Model/Introduction)
- [Document.querySelector (MDN)](https://developer.mozilla.org/en-US/docs/Web/API/Document/querySelector)
- [Modifying the document (javascript.info)](https://javascript.info/modifying-document)
