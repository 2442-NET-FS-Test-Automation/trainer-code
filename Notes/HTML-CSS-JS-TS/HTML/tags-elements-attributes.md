# Tags, Elements, and Attributes: The Anatomy of HTML

## Learning Objectives
- List the common HTML tags and explain why semantic tags beat generic divs.
- Distinguish tags, elements, and attributes, including void elements.
- Recognize the global attributes `id`, `class`, `style`, and `data-*` on sight.
- Explain inline vs block flow behavior and when each applies.

## Why This Matters
Two pages can look pixel-identical — one built from `<header>`, `<nav>`, and `<article>`, the other from
fifty nested divs — yet behave completely differently for a screen-reader user, a search-engine crawler,
and the next developer reading the code. Element choice is the cheapest accessibility and maintainability
win in the stack, and "what's the difference between a tag, an element, and an attribute?" plus "inline
vs block?" are staple screening questions for any front-end or full-stack role.

## The Concept

### Anatomy: tag vs element vs attribute
These three words are often blurred in conversation but have exact meanings:

```html
<a href="https://example.com" target="_blank">Catalog home</a>
```

| Term | What it is | In the example |
|---|---|---|
| **Tag** | the bracketed markers themselves | `<a ...>` (opening), `</a>` (closing) |
| **Attribute** | `name="value"` pairs inside the opening tag | `href="..."`, `target="_blank"` |
| **Content** | what sits between the tags | `Catalog home` |
| **Element** | the whole unit: opening tag + attributes + content + closing tag | all of the above |

**Void elements** have no content and no closing tag: `<img>`, `<br>`, `<hr>`, `<input>`, `<link>`,
`<meta>`. Writing `<img ...></img>` is invalid; a self-closing slash (`<img ... />`) is tolerated but
optional in HTML. Recognize-on-sight rule: if the element *is* the content (an image, a line break, an
input box), it is probably void.

### The common tags
The working vocabulary splits into structure, text, and generic:

| Group | Tags | Job |
|---|---|---|
| Page structure (semantic) | `<header>` `<nav>` `<main>` `<section>` `<article>` `<footer>` | label page regions by meaning |
| Text content | `<h1>`–`<h6>` `<p>` `<ul>` `<ol>` `<li>` | headings (one `<h1>`, no skipping levels), paragraphs, lists |
| Hypertext and media | `<a href>` `<img src alt>` | links; images (`alt` is mandatory for accessibility) |
| Generic containers | `<div>` (block) `<span>` (inline) | grouping with **no meaning** — the fallback, not the default |

```html
<main>
  <article>
    <h2>Overdue Loans</h2>
    <p>Items checked out more than <span class="highlight">30 days</span> ago.</p>
    <ul>
      <li><a href="/loans/1041">The Pragmatic Programmer</a></li>
    </ul>
  </article>
</main>
```

### Why semantic beats div soup
A `<div>` tells the browser, assistive tech, and search engines nothing. Semantic elements carry meaning
machines act on: screen readers announce `<nav>` and `<main>` as **landmarks** users jump between;
heading tags build the outline blind users navigate by; search engines weight `<h1>` and `<article>`
content when ranking. "Div soup" — everything a div with a class name — costs you all of that: a
screen-reader user gets one undifferentiated wall, and keyboard shortcuts like skip-to-main have nothing
to target. The adjacent follow-up: divs are not banned — they are correct when you need a hook purely
for styling or scripting and no semantic element fits.

### Global attributes: recognize on sight
Most attributes are element-specific (`href`, `src`, `alt`), but four **global** attributes appear on
anything:

| Attribute | Meaning | One-liner |
|---|---|---|
| `id="checkout-btn"` | unique identifier — one per page | targeted by CSS `#id`, JS lookups, `<label for>` |
| `class="card urgent"` | space-separated style/behavior groups | reusable; the workhorse of CSS |
| `style="color: red"` | inline CSS on this one element | works, but unmaintainable — prefer stylesheets |
| `data-sku="BK-1041"` | custom data, no rendering effect | read from JS via `element.dataset.sku` |

### Inline vs block
Every element has a default **display** behavior:

| | Block | Inline |
|---|---|---|
| Flow | starts on a new line, stacks vertically | flows within the line of text |
| Width | fills the parent's width | only as wide as its content |
| Accepts width/height? | yes | no (ignored) |
| Nesting | may contain block and inline | should contain only inline content |
| Examples | `div`, `p`, `h1`, `ul`, `section`, `form` | `span`, `a`, `img`, `strong`, `em`, `label` |

The canonical pair: `<div>` is the generic block container, `<span>` the generic inline one — same
meaninglessness, different flow. A classic invalid nesting is a `<div>` inside a `<p>` (the browser
force-closes the paragraph). **`inline-block`** is the name-drop third option: flows inline but accepts
width and height. And these are only *defaults* — CSS `display: block | inline | flex | grid` can
override any of them (see `../02-css/box-model-properties.md`), which is why semantics and presentation
are separate decisions: pick the element for meaning, then style it however layout demands.

## Say It in an Interview
- *"The common tags split into semantic structure — header, nav, main, section, article, footer — text
  content like headings, paragraphs, and lists, and generics like div. Semantic tags matter because
  screen readers expose them as landmarks and search engines read them; a div carries no meaning."*
- *"A tag is the bracketed marker, attributes are the name-value pairs inside the opening tag, and the
  element is the whole package — opening tag, attributes, content, closing tag. Void elements like img
  and input have no content or closing tag."*
- *"The global attributes are id — unique hook, one per page — class for reusable styling groups, style
  for one-off inline CSS, and data-* for stashing custom data that JavaScript reads via dataset."*
- *"Block elements start a new line and fill their parent's width — div, p, headings. Inline elements
  flow within text and size to their content — span, a, strong — and ignore width. Those are just
  defaults: CSS display can turn anything into anything, including inline-block, which flows inline but
  takes a width."*

## Check Yourself
1. In `<img src="cover.jpg" alt="Book cover" class="thumb">`, identify the tag, the attributes, and say
   why there is no closing tag.
2. Name four semantic page-structure elements and the generic element they replace.
3. Two concrete things a screen-reader user loses on a div-soup page — name them.
4. Which global attribute must be unique per page, and which one does `element.dataset` read?
5. Why does setting `width: 300px` on a `<span>` do nothing, and what two fixes are available?

**Answers:** (1) Tag: `<img ...>`; attributes: `src`, `alt`, `class`; `img` is a void element — the
image *is* the content, so there is nothing to enclose. (2) `header`, `nav`, `main`, `footer` (also
`section`, `article`) — all replacing `<div>`. (3) Landmark navigation (jumping straight to nav/main)
and a meaningful heading outline. (4) `id` must be unique; `data-*` feeds `dataset`. (5) `span` is
inline by default and inline elements ignore width; set `display: inline-block` (or `block`) in CSS, or
use a block element if the semantics allow.

## Summary
- Element = opening tag + attributes + content + closing tag; void elements (`img`, `input`, `br`) have
  neither content nor closing tag.
- Prefer semantic structure (`header`/`nav`/`main`/`section`/`article`/`footer`) over divs: landmarks
  for screen readers, outline for navigation, signal for search engines.
- Globals on sight: `id` (unique hook), `class` (reusable groups), `style` (inline one-off),
  `data-*` (custom data for JS).
- Block stacks and fills width; inline flows and hugs content; `inline-block` mixes the two; CSS
  `display` overrides all defaults — choose elements for meaning, not looks.

## Resources
- [HTML elements reference (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTML/Element)
- [Global attributes (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTML/Global_attributes)
- [Semantic HTML (web.dev Learn HTML)](https://web.dev/learn/html/semantic-html)
