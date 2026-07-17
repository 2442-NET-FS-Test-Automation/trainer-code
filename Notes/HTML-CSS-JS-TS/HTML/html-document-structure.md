# HTML Document Structure: Doctype, Head, Body, and the DOM

## Learning Objectives
- Describe what HTML is and what job it does in a web page.
- Describe the structure of an HTML document and what belongs in each section.
- Explain what the DOM tree is and how the browser builds it from the document.
- Link an external CSS stylesheet in the right place.
- Link an external JavaScript file and choose between end-of-body, `defer`, and `async`.

## Why This Matters
Every page a browser renders — hand-written, server-generated, or emitted by a framework — is one HTML
document with the same skeleton. Knowing what lives in `<head>` versus `<body>`, and where a stylesheet
or script tag belongs, is the difference between a page that renders instantly and one that flashes
unstyled or blocks on a download. It is also the first thing interviewers probe for front-end roles.

## The Concept

### What HTML is
HTML — **HyperText Markup Language** — is the markup language that defines the **structure and content**
of a web page: headings, paragraphs, links, images, forms. It is not a programming language (no logic,
no loops) and it does not control presentation — that is CSS's job (see
`../02-css/css-fundamentals-selectors.md`) — or behavior, which is JavaScript's. "HyperText" is the
defining idea: documents that link to other documents, which is what makes the web a web.

### The document skeleton
Every valid document has the same four-part shape:

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8">
    <title>Order Service — Dashboard</title>
    <link rel="stylesheet" href="styles/site.css">
  </head>
  <body>
    <h1>Open Orders</h1>
    <p>Nothing visible on the page lives outside body.</p>
    <script src="scripts/app.js" defer></script>
  </body>
</html>
```

| Part | Job | Typical contents |
|---|---|---|
| `<!DOCTYPE html>` | tells the browser "parse this as modern HTML" | exactly that one line, first |
| `<html>` | root element wrapping everything | `lang` attribute for accessibility/translation |
| `<head>` | **metadata** — machine-facing, nothing renders | `<title>`, `<meta charset>`, `<link>`, `<script>` |
| `<body>` | **visible content** — everything the user sees | headings, text, images, forms, scripts |

The `<title>` is the tab label, the bookmark name, and the search-result headline.

**Adjacent trap: the missing doctype.** Omit `<!DOCTYPE html>` and browsers drop into **quirks mode**,
emulating 1990s bugs (a different box-model, odd table layout). Symptoms: CSS that "works everywhere
else" misbehaves. The doctype is not decoration; it is a rendering-mode switch.

### The DOM tree
The browser does not work from your text file. It **parses** the HTML into an in-memory tree of nodes —
the **Document Object Model (DOM)**: `html` is the root, `head` and `body` are its children, and so on
down. Every element becomes a node with parent/child/sibling relationships:

```
document → html → head → title
                → body → h1
                       → p
```

CSS selectors match against this tree and JavaScript reads and rewrites it — which is why the rendered
page can drift far from the source. Working the DOM from JavaScript is its own topic: see
`../03-javascript/dom-selection-manipulation.md`.

### Linking an external stylesheet
CSS is attached with a **void `<link>` element in the `<head>`**:

```html
<link rel="stylesheet" href="styles/site.css">
```

`rel="stylesheet"` declares the relationship; `href` is the path or URL. It goes in the head so the
browser knows the styles *before* painting — put it late and users see a flash of unstyled content
(FOUC). The trade-off: stylesheets are render-blocking by design, so a huge sheet delays first paint;
production sites keep critical CSS small.

### Linking an external script
Scripts attach with `<script src="...">`, and **placement is a performance decision** because a classic
script blocks HTML parsing while it downloads and runs:

| Placement | Behavior | Use when |
|---|---|---|
| `<head>`, plain | blocks parsing immediately — page stalls | almost never |
| end of `<body>` | runs after all HTML above it is parsed | classic safe default |
| `<head>` + `defer` | downloads in parallel, runs after parsing, in order | modern default |
| `<head>` + `async` | downloads in parallel, runs the moment it arrives, order not guaranteed | independent scripts (analytics) |

```html
<script src="scripts/app.js" defer></script>
```

End-of-body and `defer` both guarantee the DOM exists when the script runs — critical, since a script
in the head that queries an element below it finds nothing. `defer` wins on speed (download overlaps
parsing); end-of-body wins on ubiquity. `async`: parallel download, runs on arrival, no order guarantee.

## Say It in an Interview
- *"HTML is the markup language for a page's structure and content — headings, links, forms. CSS handles
  presentation and JavaScript handles behavior; HTML itself has no logic."*
- *"A document is a doctype, then an html root with two children: head for metadata like the title and
  stylesheet links, and body for everything visible. Skip the doctype and you get quirks mode."*
- *"The browser parses HTML into the DOM, an in-memory tree of nodes. CSS matches against it and
  JavaScript manipulates it, so the live page can differ from the source file."*
- *"Stylesheets go in the head via link rel=stylesheet, so styles are known before first paint and you
  avoid a flash of unstyled content."*
- *"Scripts sit at the end of body or in the head with defer — both guarantee the DOM is built before
  they run, and defer overlaps the download with parsing. Async runs on arrival, order not guaranteed."*

## Check Yourself
1. Which section does `<title>` belong in, and name two places its text shows up.
2. What rendering mode does a missing doctype trigger, and what is one visible symptom?
3. What is the DOM, and why can it differ from the HTML source file?
4. Write the one line that attaches `styles/site.css` to a page, and say where it goes.
5. A `<script>` in the `<head>` without attributes queries an element in the body and gets `null` — why,
   and give two fixes.

**Answers:** (1) `<head>`; the browser tab and search-engine result titles (also bookmarks). (2) Quirks
mode — legacy bug-compatible rendering; e.g., box-model/layout math goes wrong. (3) The in-memory node
tree the browser parses the document into; scripts can add/remove/change nodes after load, so the live
tree drifts from the source. (4) `<link rel="stylesheet" href="styles/site.css">` inside `<head>`.
(5) The script runs before the parser reaches the body, so the element does not exist yet; move the
script to the end of `<body>`, or keep it in the head with `defer`.

## Summary
- HTML = structure and content; CSS = presentation; JavaScript = behavior.
- Skeleton: `<!DOCTYPE html>` (modern-mode switch — omit it and you get quirks mode), `<html>` root,
  `<head>` metadata, `<body>` visible content.
- The browser parses the document into the DOM tree; everything downstream (CSS, JS) targets that tree.
- CSS: `<link rel="stylesheet" href="...">` in the head, before first paint.
- JS: end-of-body or `defer` in the head (DOM guaranteed); `async` = run on arrival, unordered.

## Resources
- [HTML: HyperText Markup Language (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTML)
- [Basic HTML syntax (MDN Learn)](https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/Structuring_content/Basic_HTML_syntax)
- [Document structure (web.dev Learn HTML)](https://web.dev/learn/html/document-structure)
