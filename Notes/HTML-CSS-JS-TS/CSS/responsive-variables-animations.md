# Responsive Design, CSS Variables, and Animations

## Learning Objectives
- Explain responsive web design: fluid layouts, relative units, media queries, and the viewport meta tag.
- Write media queries — syntax, common breakpoints, combined conditions — and choose mobile-first vs
  desktop-first deliberately.
- Declare and consume CSS custom properties (`--brand` / `var()`), and contrast their runtime cascade
  with preprocessor variables.
- Animate with `transition` for state changes and `@keyframes` + `animation` for autonomous motion,
  preferring the cheap-to-animate properties.

## Why This Matters
More than half of web traffic is phones, so a layout that only works at 1440px is broken for most users —
responsive design is table stakes, not a feature. Variables and animations are the polish layer on top:
variables make theming (dark mode, brand swaps) a one-line change instead of a find-and-replace, and
knowing *which* properties animate cheaply is the difference between a butter-smooth UI and one that
stutters on mid-range hardware. All three are frequent interview probes because they reveal whether you
build for real devices or just your own monitor.

## The Concept

### Responsive design: three ingredients plus one tag
Responsive design = **fluid layouts** (grids and flex tracks that stretch, e.g.
`repeat(auto-fit, minmax(220px, 1fr))` — see `flexbox-grid.md`) + **relative units** (`%`, `rem`,
`vw`/`vh` instead of fixed `px` — see `box-model-properties.md`) + **media queries** (breakpoints where
the layout restructures). None of it works without the viewport meta tag in the HTML `<head>`:
```html
<meta name="viewport" content="width=device-width, initial-scale=1.0">
```
Without it, phones render the page at a fake desktop width (~980px) and shrink it, so your media queries
never fire. Every page ships this line (see `../01-html/html-document-structure.md`).

### Media queries
```css
/* Mobile-first: base styles are the phone layout, queries ADD complexity upward */
.cards { display: grid; grid-template-columns: 1fr; gap: 1rem; }

@media (min-width: 600px)  { .cards { grid-template-columns: 1fr 1fr; } }
@media (min-width: 1024px) { .cards { grid-template-columns: repeat(4, 1fr); } }

/* Desktop-first uses max-width and subtracts downward */
@media (max-width: 600px)  { .sidebar { display: none; } }

/* Conditions combine with `and`; media type (screen/print) can lead */
@media screen and (min-width: 600px) and (max-width: 1023px) { /* tablet band */ }
```
There are no official breakpoints; common bands cluster around **~600px** (phone/tablet), **~900-1024px**
(tablet/laptop), **~1200px+** (wide). Let the *content* pick the numbers — add a breakpoint where the
layout visibly breaks, not at a device list. The trade-off: **mobile-first** (base = small screen,
`min-width` queries layer complexity up) keeps the simplest layout as the default and is the modern
norm; **desktop-first** (`max-width`, subtracting features down) suits retrofitting an existing desktop
design but tends to accumulate "undo" rules. Media queries can also probe user preference, not just
width: `@media (prefers-color-scheme: dark)` applies when the OS is in dark mode — the standard hook for
automatic dark themes.

### CSS variables (custom properties)
```css
:root {                      /* :root = <html>, so these are global   */
  --brand: #336;
  --space: 1rem;
}
.button {
  background: var(--brand);
  padding: var(--space) calc(var(--space) * 2);
  border: 1px solid var(--accent, #ccc);   /* second argument = fallback */
}
.theme-dark {                /* theming: override on any subtree      */
  --brand: #88f;
}
```
A custom property is declared with a `--` prefix and read with `var()`. Two rules of the syntax: they are
case-sensitive, and they **cascade and inherit like any other property** — that is the superpower.
Because they are resolved at *runtime* by the browser, overriding `--brand` on `.theme-dark` restyles the
whole subtree instantly, and JavaScript can retune them live
(`element.style.setProperty('--brand', '#f63')`). Preprocessor variables (Sass `$brand`) are the
contrast: compiled away to literal values at build time — zero runtime cost, usable in ways custom
properties are not (e.g. inside selector names), but *gone* in the browser, so no per-subtree theming and
no runtime changes. The modern default: custom properties for anything themable, preprocessors only for
build-time conveniences.

### Animations: transition vs @keyframes
**`transition`** animates a property *between two states* — you define start and end via normal rules,
and the browser tweens when the state changes (hover, focus, a class toggle):
```css
.card {
  transform: translateY(0);
  box-shadow: 0 1px 3px rgba(0,0,0,.2);
  transition: transform 200ms ease-out, box-shadow 200ms ease-out;
} /*            property  duration  timing-function                  */
.card:hover { transform: translateY(-4px); box-shadow: 0 8px 20px rgba(0,0,0,.25); }
```
**`@keyframes` + `animation`** runs *autonomously* — no state change needed, with multiple steps and
looping:
```css
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50%      { opacity: .4; }
}
.loading-dot { animation: pulse 1.2s ease-in-out infinite; }
/*             name  duration  timing-function  iteration-count      */
```
Rule of thumb: state change = transition; ongoing/multi-step motion (spinners, entrance sequences) =
keyframes.

**The performance trade-off:** animate **`transform`** and **`opacity`** — the browser can composite them
on the GPU without recomputing layout. Animating geometry properties (`width`, `height`, `top`/`left`,
`margin`) forces layout recalculation *every frame*, which is what makes animations stutter. So: slide
with `transform: translateX(...)`, never by animating `left`; scale with `transform: scale(...)`, never
by animating `width`. Adjacent one-liner: respect `@media (prefers-reduced-motion: reduce)` — users with
vestibular disorders opt out of motion at the OS level, and your stylesheet should disable or shrink
animations when it matches. Name-only: **container queries** (`@container`) let a component respond to
its *container's* width instead of the viewport's — know the name as the next step beyond media queries.

## Say It in an Interview
- *"Responsive design is fluid layouts plus relative units plus media queries — and the viewport meta
  tag, without which a phone pretends to be a desktop and none of the breakpoints fire."*
- *"I write mobile-first: the base styles are the small-screen layout and `min-width` queries layer
  complexity upward, so the simplest layout is the default. I put breakpoints where the content breaks,
  not at a device list."*
- *"Custom properties are declared like `--brand` on `:root` and read with `var()`. Unlike Sass
  variables, which compile away at build time, they cascade at runtime — so I can theme a whole subtree
  or flip dark mode by overriding one property."*
- *"Transitions tween between two states on a trigger like hover; keyframes run autonomously with
  multiple steps. Either way I animate `transform` and `opacity` because they composite on the GPU —
  animating layout properties like width or top recalculates layout every frame and stutters."*

## Check Yourself
1. A page's media queries work on desktop dev tools but the real phone shows a shrunken desktop site.
   What is missing?
2. Write a media query that applies only between 600px and 1023px wide.
3. Why can a CSS custom property drive a dark theme when a Sass variable cannot?
4. A menu should slide in from the left. Which property do you animate, and which do you avoid — and why?
5. Transition or keyframes: a button's hover color change vs an infinite loading spinner?

**Answers:** (1) The viewport meta tag — `<meta name="viewport" content="width=device-width,
initial-scale=1.0">`; without it the phone lays out at a fake desktop width. (2) `@media (min-width:
600px) and (max-width: 1023px) { ... }`. (3) Custom properties are resolved by the browser at runtime and
cascade, so overriding `--brand` under a `.theme-dark` class (or a `prefers-color-scheme` query) restyles
live; Sass variables are replaced with literal values at build time and no longer exist in the browser.
(4) Animate `transform: translateX(...)`; avoid animating `left`/`margin-left`, because geometry changes
force layout recalculation every frame while transforms composite cheaply. (5) Hover color = transition
(two states, a trigger); spinner = `@keyframes` + `animation` (autonomous, looping).

## Summary
- Responsive = fluid layouts + relative units + media queries; the viewport meta tag makes phones report
  their real width.
- `@media (min-width: ...)` mobile-first is the norm; combine conditions with `and`; breakpoints come
  from content, commonly near 600 / 1024 / 1200px; `prefers-color-scheme` detects dark mode.
- Custom properties: `--name` on `:root`, consumed with `var(--name, fallback)`; runtime cascade enables
  theming and JS control — preprocessor variables vanish at build time.
- `transition: property duration timing-function` for state changes; `@keyframes` + `animation` for
  autonomous motion; animate `transform`/`opacity`, not layout properties; honor
  `prefers-reduced-motion`.

## Resources
- [Using media queries (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_media_queries/Using_media_queries)
- [Using CSS custom properties (MDN)](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)
- [How to create high-performance CSS animations (web.dev)](https://web.dev/articles/animations-guide)
