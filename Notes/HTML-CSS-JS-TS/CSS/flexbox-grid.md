# Layout with Flexbox and Grid

## Learning Objectives
- Build one-dimensional layouts with Flexbox: axes, container properties, and item properties.
- Build two-dimensional layouts with CSS Grid: track definitions, `fr`, `repeat()`, `minmax()`, spanning.
- Choose between Flexbox and Grid for a given layout, and compose them in one page.

## Why This Matters
Flexbox and Grid *are* modern CSS layout. Nav bars, card rows, sidebars, page shells, form alignment,
"center this thing" — every one of these was once a hack (floats, tables, absolute positioning) and is now
a few declarations. Reading real stylesheets means recognizing these properties instantly, and "flexbox vs
grid" is a staple front-end interview question because it tests whether you understand layout *models*,
not just property names.

## The Concept

### Flexbox: one dimension
Flexbox lays out children of a container along **one axis** — a row *or* a column. Setting
`display: flex` on the container turns its direct children into flex items. Everything is described
relative to two axes: the **main axis** (the direction of `flex-direction`) and the **cross axis**
(perpendicular to it). `justify-content` distributes items along the main axis; `align-items` aligns them
on the cross axis — the pair people confuse most.

```css
/* A nav bar: logo left, links right, everything vertically centered */
.nav {
  display: flex;                   /* children become flex items                  */
  flex-direction: row;             /* main axis = horizontal (the default)        */
  justify-content: space-between;  /* main axis: push first/last to the edges     */
  align-items: center;             /* cross axis: vertical centering, one line    */
  gap: 1rem;                       /* space BETWEEN items, no margin hacks        */
  flex-wrap: wrap;                 /* allow items onto a second line when tight   */
}
```
```html
<nav class="nav">
  <span class="logo">Library</span>
  <a href="/catalog">Catalog</a> <a href="/loans">Loans</a> <a href="/account">Account</a>
</nav>
```

Container properties to recognize: `flex-direction` (`row`, `column`, and their `-reverse` forms),
`justify-content` (`flex-start`, `center`, `space-between`, `space-around`, `space-evenly`),
`align-items` (`stretch` default, `center`, `flex-start`, `flex-end`, `baseline`), `flex-wrap`, `gap`.

**Item properties** control how each child claims space: `flex-grow` (share of leftover space),
`flex-shrink` (how readily it gives space up), `flex-basis` (starting size before grow/shrink). The
shorthand `flex: grow shrink basis` is what you will actually see — and `flex: 1` (= `1 1 0`) is the
idiom for "make these items share the space equally":
```css
.card-row { display: flex; gap: 1rem; }
.card     { flex: 1; }          /* three cards -> three equal columns             */
.card.featured { flex: 2; }     /* featured card takes twice the share            */
```
Trade-off: flex sizes come from the *content out* — items negotiate for space, so columns across separate
rows will not line up. The moment you need rows *and* columns to agree, you have outgrown Flexbox.

### Grid: two dimensions
Grid lets the **container** define a full track structure — columns *and* rows — and places children into
it. That inversion (layout-in, not content-out) is the core difference from Flexbox.

```css
/* A page shell: header across the top, sidebar + main, footer across the bottom */
.shell {
  display: grid;
  grid-template-columns: 220px 1fr;   /* fixed sidebar, main takes the rest       */
  grid-template-rows: auto 1fr auto;  /* header/footer size to content            */
  gap: 1rem;
  min-height: 100vh;
}
header, footer { grid-column: 1 / -1; }   /* span from first line to last         */
```

The units and functions to recognize on sight:

| Syntax | Meaning |
|---|---|
| `1fr` | one *fraction* of the leftover space; `2fr 1fr` = a 2:1 split |
| `repeat(3, 1fr)` | shorthand for `1fr 1fr 1fr` |
| `minmax(200px, 1fr)` | a track at least 200px, growing to a fair share |
| `grid-column: 1 / -1` | span from the first grid *line* to the last (full width) |
| `grid-column: span 2` | occupy two column tracks from wherever the item lands |

A card gallery in one rule — the single most reused Grid recipe:
```css
.gallery {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem;      /* as many 220px+ columns as fit; they stretch to fill the row */
}
```
Recognize on sight: **`grid-template-areas`** names regions in ASCII-art strings and assigns children with
`grid-area` — `"header header" "sidebar main" "footer footer"` reads like a diagram of the page. Adjacent
one-liner: in `repeat(auto-fit|auto-fill, ...)`, **auto-fill** keeps empty tracks when items run out while
**auto-fit** collapses them so the real items stretch — with few items, auto-fit looks fuller.

### Choosing, and composing

| Question | Reach for |
|---|---|
| One axis — a row of buttons, a nav, a vertical stack? | Flexbox |
| Two axes — page shell, dashboard, photo grid, aligned columns across rows? | Grid |
| Content should decide the sizes? | Flexbox (content-out) |
| The layout should impose the sizes? | Grid (layout-in) |

They compose, and real pages use both: the Grid shell above places the regions, and inside it the nav is
a flex row, each card's innards are a flex column pushing its button to the bottom. It is layers, not a
rivalry. Adjacent name to know: before these existed, layouts were built with **floats** (and clearfix
hacks) — you will still meet float-based layout in legacy code, so know the name, but never start a new
layout with it.

## Say It in an Interview
- *"Flexbox is one-dimensional layout: `display: flex` on the container, `justify-content` distributes
  items along the main axis, `align-items` handles the cross axis, and item-level `flex-grow`, `shrink`,
  and `basis` — usually via `flex: 1` — control how children share space."*
- *"Grid is two-dimensional: the container declares column and row tracks with `grid-template-columns`
  using `fr`, `repeat()`, and `minmax()`, and items can span tracks with `grid-column`. The
  `repeat(auto-fit, minmax(...))` pattern gives a responsive gallery without media queries."*
- *"I choose by dimensions and direction of control: flex for one axis where content sizes itself, grid
  when I need rows and columns to agree and the layout imposes the structure — and they compose, grid for
  the page shell, flex inside each region."*

## Check Yourself
1. In a `flex-direction: column` container, which axis does `justify-content` distribute along, and what
   does `align-items: center` do?
2. Three flex items have `flex: 1`, `flex: 1`, and `flex: 2`. How is leftover space divided?
3. Write the one declaration that makes a grid of equal columns, each at least 220px, filling the row.
4. How does a grid item stretch across every column of its grid?
5. Cards in three flex rows refuse to align into clean vertical columns. What is the actual fix?

**Answers:** (1) Vertically — the main axis follows `flex-direction`, so `justify-content` distributes
top-to-bottom and `align-items: center` centers items horizontally (the cross axis). (2) 1:1:2 — the third
item gets half the leftover space, the others a quarter each. (3) `grid-template-columns:
repeat(auto-fit, minmax(220px, 1fr));`. (4) `grid-column: 1 / -1;` — from the first grid line to the last.
(5) Switch the container to Grid — column alignment across rows is two-dimensional agreement, which
Flexbox by design does not provide.

## Summary
- Flexbox = one axis: `display: flex`, main vs cross axis, `justify-content` / `align-items`,
  `flex-wrap`, `gap`; items share space via `flex-grow/shrink/basis` (`flex: 1`).
- Grid = two axes: `display: grid`, tracks via `grid-template-columns/rows` with `fr`, `repeat()`,
  `minmax()`; span with `grid-column`; `grid-template-areas` names regions readably.
- `repeat(auto-fit, minmax(Npx, 1fr))` = responsive gallery in one line; auto-fill keeps empty tracks,
  auto-fit collapses them.
- Choose: flex is content-out on one axis; grid is layout-in on two. Compose them — grid shell, flex
  interiors. Floats are legacy; recognize, do not write.

## Resources
- [A Complete Guide to Flexbox (CSS-Tricks)](https://css-tricks.com/snippets/css/a-guide-to-flexbox/)
- [A Complete Guide to CSS Grid (CSS-Tricks)](https://css-tricks.com/snippets/css/complete-guide-grid/)
- [Learn CSS: Flexbox (web.dev)](https://web.dev/learn/css/flexbox)
