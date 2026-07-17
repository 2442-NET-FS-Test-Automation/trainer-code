# CSS Fundamentals: Rules, Selectors, and the Cascade

## Learning Objectives
- Explain what CSS is and why presentation is separated from structure.
- Read and write the anatomy of a style rule: selector plus declaration block.
- Compare the three ways to attach styling (inline, internal, external) and pick the right one.
- Select elements by tag, class, id, and combinations; recognize attribute selectors and pseudo-classes.
- Use combinators (child, descendant, adjacent sibling, general sibling) to target structure.
- Predict which rule wins: inline vs stylesheet, specificity, source order, and `!important`.

## Why This Matters
HTML says what a page *is*; CSS says what it *looks like*. Keeping them separate means one stylesheet can
reskin a thousand pages, and a markup change does not require touching presentation. In practice the skill
that separates juniors from everyone else is not writing declarations — it is predicting **which** of
several competing rules actually applies. "Why is my style not showing up?" is a cascade question, and the
cascade (specificity, source order, origin) is also one of the most common front-end interview topics.

## The Concept

### What CSS is
CSS — **Cascading Style Sheets** — is a declarative language that maps *selectors* (which elements) to
*declarations* (what visual properties they get). "Cascading" is the conflict-resolution algorithm: when
multiple rules target the same element, origin, specificity, and source order decide the winner. The
payoff of the separation: structure stays semantic (see `../01-html/tags-elements-attributes.md`), styles
are reusable across pages, and browsers can cache a stylesheet once for the whole site.

### Anatomy of a style rule
```css
p {                    /* selector: which elements this rule targets            */
  color: red;          /* declaration: property, colon, value, semicolon        */
  font-size: 16px;     /* a declaration block holds any number of declarations  */
}
```
Terminology to recognize on sight: the whole thing is a **rule (ruleset)**; `p` is the **selector**; the
braces enclose the **declaration block**; `color: red;` is a **declaration** made of a **property** and a
**value**. A missing semicolon silently kills the *next* declaration — a classic debugging trap.

### Three ways to attach CSS

| Method | Syntax | When appropriate |
|---|---|---|
| Inline | `<p style="color: red;">` | One-off overrides, email HTML, values computed by script |
| Internal | `<style>` block in `<head>` | Single-page prototypes, page-unique critical styles |
| External | `<link rel="stylesheet" href="site.css">` | Everything else — the production default |

External wins for maintainability (one file, every page), caching (the browser downloads it once), and
separation of concerns. Inline styles scatter presentation through markup, cannot be reused, and sit at
the top of the priority ladder, making them painful to override later.

### Selectors: tag, class, id, and combinations
```css
p        { line-height: 1.5; }   /* type (tag): every <p>                        */
.card    { padding: 1rem; }      /* class: every element with class="card"       */
#nav     { position: sticky; }   /* id: the one element with id="nav"            */
p.card   { color: navy; }        /* compound: <p> elements that ALSO have .card  */
h1, h2   { font-family: serif; } /* grouping: comma = same block for both        */
```
Classes are the workhorse — reusable, composable (`class="card featured"`), low specificity. Ids are
unique per page, so id selectors do not reuse; most teams style with classes and reserve ids for anchors
and JavaScript hooks. Recognize on sight: **attribute selectors** — `input[type="email"]`,
`a[href^="https"]` (starts with), `img[alt]` (has the attribute) — and **pseudo-classes**, which select by
*state or position*: `a:hover`, `input:focus`, `li:first-child`, `tr:nth-child(even)`.

### Combinators: selecting by structure

| Combinator | Example | Selects |
|---|---|---|
| Descendant (space) | `div p` | every `<p>` anywhere inside a `<div>`, any depth |
| Child `>` | `div > p` | only `<p>` elements that are *direct* children of a `<div>` |
| Adjacent sibling `+` | `h1 + p` | the single `<p>` immediately following an `<h1>` |
| General sibling `~` | `h1 ~ p` | every later `<p>` sharing the `<h1>`'s parent |

The benefit: you style by *position in the document* without inventing a class for every element. A
catalog page can say "the first paragraph after any heading is the lede" with `h2 + p` — zero extra markup.

### Priority: who wins
1. **Inline `style=""` beats internal and external rules** (only `!important` outranks it).
2. Among stylesheet rules, higher **specificity** wins: id beats class beats element.
3. On a specificity tie, **source order** wins — the rule declared *last* applies.

Specificity is calculated as a three-part count **(ids, classes, elements)** compared left to right:
`#nav .card a:hover` scores (1, 2, 1) — `:hover` and attribute selectors count as classes; the universal
selector `*` and combinators count as nothing. So (1, 0, 0) beats (0, 99, 0): one id outranks any number
of classes. `!important` appended to a declaration overrides all of this — and should be avoided, because
the only way to beat an `!important` is *another* `!important`, starting an arms race that makes the sheet
unmaintainable. Over-specific selectors hurt for the same reason: `div#main ul.menu li a` can only be
overridden by something even longer, so keep selectors as flat as the design allows (a single class is the
sweet spot). Adjacent fact worth one line: newer specificity-management tools exist — `:where()` wraps
selectors at zero specificity, and `@layer` lets you rank whole stylesheets — know the names.

## Say It in an Interview
- *"CSS is the styling language of the web — it separates presentation from HTML structure, so the markup
  stays semantic and one cached stylesheet can restyle an entire site."*
- *"A rule is a selector plus a declaration block of property-value pairs — like `p { color: red;
  font-size: 16px; }`."*
- *"You can style inline, in a `<style>` block, or from an external stylesheet. External is the default in
  production: one maintainable file, cached by the browser, no presentation mixed into markup."*
- *"Tag selectors hit every element of that type, `.class` hits anything carrying the class, `#id` hits
  the unique element, and you can compound them — `p.card` is paragraphs that also have the card class.
  Pseudo-classes like `:hover` select by state."*
- *"Combinators select by structure: space is any descendant, `>` is direct child, `+` is the immediately
  next sibling, `~` is all later siblings — so I can style by position without adding classes."*
- *"Inline styles beat stylesheet rules; among rules, specificity wins — counted as ids, then classes,
  then elements — and source order breaks ties. `!important` overrides everything, which is exactly why I
  avoid it."*

## Check Yourself
1. Name the parts of `p { color: red; }` using the correct vocabulary.
2. Why does an external stylesheet beat inline styles for a production site, in two words per reason?
3. What is the difference between `div p`, `div > p`, and `div + p`?
4. Compute the specificity of `#sidebar .widget h3` and of `.a .b .c .d` — which wins?
5. Two rules have identical specificity and both set `color`. Which applies?

**Answers:** (1) `p` is the selector; the braces hold the declaration block; `color: red;` is a
declaration — `color` the property, `red` the value. (2) Maintainability (one file), caching (downloaded
once), separation (markup stays clean). (3) `div p` = any `<p>` descendant at any depth; `div > p` = only
direct children; `div + p` = the one `<p>` immediately *after* the `<div>`, as a sibling, not inside it.
(4) (1, 1, 1) vs (0, 4, 0) — the id rule wins; one id outranks any number of classes. (5) The one that
appears later in source order.

## Summary
- CSS separates presentation from structure; "cascading" names the conflict-resolution algorithm.
- Rule = selector + declaration block; declaration = `property: value;`.
- Inline, internal, external — external is the production default (maintainability, caching).
- Selectors: `tag`, `.class`, `#id`, compounds like `p.card`; attribute selectors and pseudo-classes
  (`:hover`, `:first-child`) select by attribute and state.
- Combinators: descendant (space), child `>`, adjacent sibling `+`, general sibling `~`.
- Priority: inline > specificity (ids, classes, elements) > source order; `!important` trumps all and
  should be a last resort; keep selectors flat.

## Resources
- [CSS selectors (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_selectors)
- [Specificity (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/Specificity)
- [Specifics on CSS Specificity (CSS-Tricks)](https://css-tricks.com/specifics-on-css-specificity/)
