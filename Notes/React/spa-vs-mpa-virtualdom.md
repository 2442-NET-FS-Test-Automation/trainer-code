# SPAs vs MPAs and How the Virtual DOM Works

## Learning Objectives

- Explain the difference between a Single Page Application and a Multi Page Application.
- Weigh the tradeoffs of each architecture honestly.
- Describe what the Virtual DOM is and why React uses one.
- Explain reconciliation and diffing, and how they make updates fast.
- Connect keys and stable identity to how reconciliation performs.

## Why This Matters

"What is an SPA?" and "how does the Virtual DOM work?" are two of the most reliable front-end interview
questions, and they are linked: React exists to make SPAs feel fast, and the Virtual DOM is the mechanism
that delivers it. Beyond interviews, understanding this shapes real decisions — whether a project should be
an SPA at all, why the first load feels heavy but navigation feels instant, and why direct DOM manipulation
is discouraged in React. This is the "how does React actually work under the hood" material, and being able
to explain it clearly separates people who *use* React from people who *understand* it.

## The Concept

### Multi Page Applications (MPA)

The traditional web model. Every navigation — clicking a link, submitting a form — sends a request to the
server, which responds with a **whole new HTML page**. The browser throws away the current document and
renders the new one from scratch. Each URL maps to a distinct document the server produces.

- **Strengths:** simple mental model, excellent default SEO (every page is real HTML the crawler gets
  immediately), fast first paint (the server sends finished markup), and no large JavaScript bundle
  required.
- **Weaknesses:** every navigation is a full round-trip and a full page reload — a visible flash and lost
  client state; shared UI like headers and nav bars are re-sent and re-rendered on every page.

### Single Page Applications (SPA)

An SPA loads **one** HTML shell once, plus a JavaScript bundle. From then on, navigation happens
**client-side**: JavaScript swaps the visible content in place and updates the URL without fetching a new
document. Data still comes from the server, but as JSON over API calls (fetch/Axios), not as new HTML
pages. React apps are SPAs by default.

- **Strengths:** navigation feels instant (no full reload, no flash), client state survives across views,
  and the interface can be highly interactive and app-like.
- **Weaknesses:** a bigger initial download (the bundle) means a slower first load; SEO and first paint
  need extra work (server-side rendering or static generation) because the initial HTML is nearly empty
  until JavaScript runs; and the app must manage routing, history, and state itself.

### Choosing between them

There is no universally correct answer. Content-first sites where SEO and fast first paint dominate — blogs,
marketing pages, documentation — lean MPA (or an SPA framework with server rendering). Highly interactive,
app-like experiences where users stay a while and navigate constantly — dashboards, editors, an internal
catalog management tool — lean SPA. Many modern frameworks blur the line by rendering on the server *and*
hydrating into an SPA, getting the first-paint/SEO benefits of an MPA with the in-app feel of an SPA.

### The problem the Virtual DOM solves

The real DOM — the browser's live tree of page elements — is expensive to change. Touching it triggers
style recalculation, layout, and repaint, and doing many small updates in a loop can be slow and janky. In
an SPA, the UI changes constantly as state updates, so naive "just rewrite the DOM whenever anything
changes" would be far too slow. React's answer is to put a fast, in-memory layer in front of the real DOM.

### What the Virtual DOM is

The **Virtual DOM** is a lightweight JavaScript representation of your UI — a tree of plain objects
describing what the DOM *should* look like. It is cheap to create and cheap to compare because it is just
objects in memory, not real browser nodes. Every time your component renders, React builds a fresh Virtual
DOM tree describing the desired result.

```tsx
// This JSX...
function BookCount({ count }: { count: number }) {
  return <p>You have {count} book(s) checked out</p>;
}

// ...produces a Virtual DOM node, roughly:
// { type: "p", props: {}, children: ["You have ", 3, " book(s) checked out"] }
```

Crucially, building that object tree does **not** touch the real DOM. It is a description, not the page
itself.

### Reconciliation: diffing old tree against new

When state changes, React re-renders the component and produces a **new** Virtual DOM tree. It then compares
this new tree against the **previous** one — this comparison is called **reconciliation**, and the
comparison itself is the **diff**. React walks both trees and works out the smallest set of real-DOM
operations needed to make the actual page match the new description: this text changed, that attribute
changed, this node was added, that one removed. Only those minimal changes are applied to the real DOM;
everything unchanged is left alone.

So a state update follows three steps:

1. **Render** — React re-runs the component and builds a new Virtual DOM tree.
2. **Diff (reconcile)** — React compares the new tree to the previous one to find what actually changed.
3. **Commit** — React applies just those differences to the real DOM.

This is why React feels fast even though "re-render on every state change" sounds wasteful. The re-render
produces cheap in-memory objects; only the genuine differences ever reach the costly real DOM. A comparison
across the whole tree could in theory be slow, so React uses fast heuristics: elements of different types
are assumed to produce different subtrees (so it replaces rather than deep-compares), and lists are matched
by **key**.

### Why keys matter to reconciliation

This is where keys connect. When React diffs a list, it needs to decide which new items correspond to which
old items. If items carry stable `key` props, React matches them by identity: it can tell that an item
moved, or that one was inserted in the middle, and reuse the existing DOM nodes for the rest. Without stable
keys — or with keys based on array position — React matches by index and can mistake "an item was inserted
at the top" for "every item's content changed," producing extra DOM work and, worse, attaching per-item DOM
state (input values, focus, checkbox state) to the wrong element. Good keys make the diff both correct and
minimal.

### A practical consequence: let React own the DOM

Because React is diffing its own model of the UI against the real DOM, you should let React make the DOM
changes. Reaching in and manually editing DOM nodes that React manages puts the real page out of sync with
React's Virtual DOM model, and the next reconciliation can overwrite or fight your change. The React way is
to change **state**; React re-renders, diffs, and updates the DOM for you. Describe *what* the UI should be
for a given state, and let reconciliation figure out *how* to get there.

## Say It in an Interview

- *"An MPA fetches a whole new HTML page from the server on every navigation; an SPA loads one shell once
  and swaps views client-side with JavaScript, pulling data as JSON. SPAs feel instant after load but cost
  more upfront and need extra work for SEO."*
- *"The Virtual DOM is a lightweight in-memory JavaScript tree describing the UI. It's cheap to build and
  compare, unlike the real DOM."*
- *"On a state change React builds a new Virtual DOM tree, diffs it against the previous one —
  reconciliation — and commits only the minimal set of changes to the real DOM."*
- *"Keys give list items stable identity so the diff can match items across renders and reuse DOM nodes
  instead of rebuilding the list."*

## Check Yourself

1. In one sentence each, contrast how an MPA and an SPA handle navigation.
2. Give one strength and one weakness of an SPA compared to an MPA.
3. What is the Virtual DOM, and why is it cheaper to work with than the real DOM?
4. Walk through the three steps React takes when a component's state changes.
5. How do keys influence reconciliation, and what goes wrong without stable ones?

**Answers:** (1) An MPA requests a brand-new HTML page from the server on every navigation and reloads the
whole document; an SPA loads one HTML shell once and swaps views client-side in JavaScript without a full
reload. (2) Strength: navigation feels instant and client state persists; weakness: larger initial download
and extra effort for SEO/first paint. (3) A lightweight in-memory JavaScript tree of plain objects
describing the desired UI; it is cheap because comparing and building objects avoids the layout/repaint cost
of touching real browser nodes. (4) Render a new Virtual DOM tree, diff/reconcile it against the previous
tree to find changes, then commit only those changes to the real DOM. (5) Keys let the diff match list
items by identity so React can reuse and reorder existing DOM nodes; without stable keys React matches by
position and can do extra work and attach per-item DOM state to the wrong element.

## Summary

- **MPA:** server sends a new HTML page per navigation; great SEO/first paint, but full reloads.
- **SPA:** one shell loads once, JavaScript swaps views client-side and fetches JSON; instant navigation,
  but heavier first load and SEO needs extra work. React apps are SPAs by default.
- The **Virtual DOM** is a cheap in-memory JS tree describing the UI.
- On state change React **renders** a new tree, **diffs/reconciles** it against the old one, and
  **commits** only the minimal real-DOM changes.
- **Keys** give list items stable identity so reconciliation stays correct and minimal — change state and
  let React own the DOM rather than editing it by hand.

## Resources

- [Preserving and Resetting State — react.dev](https://react.dev/learn/preserving-and-resetting-state)
- [Render and Commit — react.dev](https://react.dev/learn/render-and-commit)
- [Single-page application (SPA) — MDN Glossary](https://developer.mozilla.org/en-US/docs/Glossary/SPA)
