# The Box Model and Everyday CSS Properties

## Learning Objectives
- Describe the CSS box model: content, padding, border, margin, from the inside out.
- Explain `box-sizing: content-box` vs `border-box` and why `border-box` is the standard reset.
- State what margin collapsing is in one line.
- Recognize the everyday property families on sight: text, box, background, display, position, overflow,
  and stacking (`z-index`).
- Choose sensible units (`px`, `%`, `em`, `rem`, `vh`/`vw`) and explain the `rem` accessibility argument.

## Why This Matters
Every element the browser renders is a rectangular box, and every layout bug you will ever debug — "why is
this 20px wider than I asked?", "why did my margins disappear?", "why does this overlap?" — is a box-model
question. The property families below are the vocabulary of day-to-day CSS work: you do not memorize every
value, but you must *recognize* what each property controls when reading someone else's stylesheet.
"Explain the box model" is a near-guaranteed interview question.

## The Concept

### The box model, inside out
Content sits at the center; **padding** is space inside the border; the **border** is the drawn edge;
**margin** is transparent space outside, separating the box from its neighbors.

```
+-----------------------------------------------+
|                   margin (transparent)        |
|   +---------------------------------------+   |
|   |            border                     |   |
|   |   +-------------------------------+   |   |
|   |   |        padding                |   |   |
|   |   |   +-----------------------+   |   |   |
|   |   |   |       content         |   |   |   |
|   |   |   +-----------------------+   |   |   |
|   |   +-------------------------------+   |   |
|   +---------------------------------------+   |
+-----------------------------------------------+
```

Padding takes the element's background; margin never does. Rule of thumb: padding for breathing room
*inside* a card, margin for spacing *between* cards.

### box-sizing: what does width mean?
```css
.card { width: 300px; padding: 20px; border: 5px solid; }
/* content-box (default): rendered width = 300 + 40 + 10 = 350px  */
/* border-box:            rendered width = 300px, content shrinks */
```
Under the default **`content-box`**, `width` sets only the content area, so padding and border are added
*on top* — the source of the classic "it's wider than I said" surprise. **`border-box`** makes `width`
mean the full visible box (content + padding + border), which is how humans reason about size. That is why
nearly every codebase starts with the reset:
```css
*, *::before, *::after { box-sizing: border-box; }
```
Trade-off: essentially none today — the reset costs one rule and eliminates a whole bug class, which is
why it is near-universal.

### Margin collapsing (one line, know the name)
Vertical margins of adjacent block elements do not add — they **collapse** to the larger of the two (a
`24px` bottom margin meeting a `16px` top margin yields `24px`, not `40px`); horizontal margins and
flex/grid items never collapse.

### The everyday properties, on sight

| Family | Properties | What they control |
|---|---|---|
| Text | `color`, `font-size`, `font-family`, `font-weight`, `text-align`, `line-height` | ink: color, size, typeface, boldness, alignment, vertical rhythm |
| Box | `width`/`height`, `padding`, `margin`, `border`, `border-radius`, `box-shadow` | the box itself: size, inner space, outer space, edge, rounded corners, drop shadow |
| Background | `background-color`, `background-image` | what paints behind the content and padding |
| Display | `display: block \| inline \| inline-block \| none` | participation in layout (see below) |
| Position | `position: static \| relative \| absolute \| fixed \| sticky` | how the box is placed (see below) |
| Overflow | `overflow: visible \| hidden \| scroll \| auto` | what happens when content outgrows the box |
| Stacking | `z-index` | who is on top when positioned boxes overlap (only on non-static elements) |

**Display in one pass:** `block` fills the available width and stacks vertically (`div`, `p`); `inline`
flows within text and ignores width/height (`span`, `a`); `inline-block` flows inline *but* accepts box
dimensions; `none` removes the element from layout entirely. Flex and grid are display values too — they
get their own note: `flexbox-grid.md`.

**Position in one pass**, with the shape to recognize:
```css
.badge  { position: absolute; top: 0; right: 0; }   /* pinned inside nearest positioned ancestor */
.parent { position: relative; }                      /* becomes that anchor without moving        */
.header { position: sticky; top: 0; }                /* scrolls, then pins at the offset          */
```
`static` is the default (offsets ignored); `relative` nudges the box from its normal spot and, crucially,
anchors absolute children; `absolute` removes the box from flow and pins it to the nearest non-static
ancestor; `fixed` pins to the viewport (classic floating button); `sticky` is the hybrid — in flow until
its scroll threshold, then pinned. Adjacent one-liner: `z-index` only compares within a **stacking
context**, which is why a huge `z-index` sometimes still loses — a new context (from an ancestor's
positioning, opacity, or transform) traps it.

### Hidden vs gone, and which unit to reach for
`display: none` removes the element from layout — it takes no space and is skipped by screen readers.
`visibility: hidden` keeps the element's box (a blank hole) and merely stops painting it; the layout does
not shift when it toggles.

| Unit | Relative to | Reach for it when |
|---|---|---|
| `px` | nothing (absolute) | borders, shadows, fine details |
| `%` | the parent's corresponding dimension | fluid widths |
| `em` | the element's own font size | padding that scales with the text inside it |
| `rem` | the root (`html`) font size | font sizes, spacing scales — the default choice |
| `vh`/`vw` | 1% of viewport height/width | full-screen sections, hero banners |

The accessibility trade-off: users who raise their browser's base font size get larger text only if you
sized in `rem`/`em`; `px` font sizes ignore that preference. Hence the common practice: `rem` for type and
spacing, `px` only for hairlines and shadows.

## Say It in an Interview
- *"Every element is a box: content in the middle, then padding inside the border, the border itself, and
  margin outside separating it from neighbors."*
- *"By default `width` only sizes the content, so padding and border make the box bigger than declared.
  `box-sizing: border-box` makes width mean the whole visible box, which is why virtually every codebase
  applies it globally as a reset."*
- *"Margin collapsing means adjacent vertical margins merge to the larger one instead of adding."*
- *"I group the everyday properties by family — text like `font-size` and `line-height`, box like padding
  and `border-radius`, display values like block and inline-block, and the five position values from
  static through sticky."*
- *"I default to `rem` for font sizes and spacing because it respects the user's browser font-size
  preference — `px` text ignores it, which is an accessibility problem."*

## Check Yourself
1. Order the four box-model layers from the inside out. Which two can carry the background color?
2. A `content-box` element has `width: 200px; padding: 10px; border: 2px solid;` — how wide does it
   render? And under `border-box`?
3. Two stacked paragraphs have `margin-bottom: 30px` and `margin-top: 20px`. What is the gap?
4. You need a "Sale" ribbon pinned to a card's corner. Which position values go on the ribbon and on the
   card, and why does the card need one at all?
5. When would you pick `visibility: hidden` over `display: none`?

**Answers:** (1) Content, padding, border, margin; background paints the content and padding (margin is
always transparent). (2) 200 + 20 + 4 = 224px rendered; under `border-box` it renders exactly 200px and
the content area shrinks. (3) 30px — vertical margins collapse to the larger. (4) Ribbon:
`position: absolute` with `top`/`right` offsets; card: `position: relative`, because absolute pins to the
nearest *non-static* ancestor — without it the ribbon pins to the page. (5) When the element must stay
hidden but keep occupying its space so the layout does not jump when it reappears.

## Summary
- Box model inside out: content, padding, border, margin; padding is inside the background, margin is
  transparent space between boxes.
- `box-sizing: border-box` makes `width` mean the full visible box — apply it globally.
- Adjacent vertical margins collapse to the larger value.
- Property families to recognize: text (`font-*`, `line-height`), box (`padding`, `border-radius`,
  `box-shadow`), background, `display` (block/inline/inline-block/none), `position` (static/relative/
  absolute/fixed/sticky), `overflow`, `z-index`.
- `display: none` removes; `visibility: hidden` hides but reserves space.
- Units: `rem` for type and spacing (accessibility), `%`/`vh`/`vw` for fluid and viewport sizing, `px`
  for hairline details.

## Resources
- [The box model (MDN)](https://developer.mozilla.org/en-US/docs/Learn_web_development/Core/Styling_basics/Box_model)
- [box-sizing (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/box-sizing)
- [Box Sizing (CSS-Tricks)](https://css-tricks.com/box-sizing/)
