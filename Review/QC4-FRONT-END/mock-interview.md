# QC-5 (Front End) — Mock Interview Bank

Say your answer out loud **before** reading the model. Recognition is not recall, and the exam asks you
to produce these, not to pick them from a list.

Each entry carries a tier badge, a model answer, the QC objective it proves, and the source file the
answer comes from.

---

## HTML

**[Must] What is HTML, and what is it not?**

> HyperText Markup Language — the markup language that defines the structure and content of a web page:
> headings, paragraphs, links, images, forms. It is not a programming language; it has no logic or loops.
> It also does not control presentation, which is CSS's job, or behavior, which is JavaScript's.
> "HyperText" is the defining idea — documents that link to other documents.

`Proves QC: Describe what HTML is.`
`Source: content/01-html/html-document-structure.md`

**[Must] Walk me through the structure of an HTML document.**

> Doctype first — `<!DOCTYPE html>` — which is a rendering-mode switch; leave it out and browsers drop
> into quirks mode and emulate old layout bugs. Then the `<html>` root with a `lang` attribute, wrapping
> two children: `<head>` for metadata that never renders — the title, charset, viewport meta, stylesheet
> links, script tags — and `<body>` for everything the user sees. The browser parses that document into
> the DOM, an in-memory tree of nodes, which is what CSS matches against and JavaScript rewrites.

`Proves QC: Describe the structure of an HTML document and what is included in the different sections.`
`Source: content/01-html/html-document-structure.md`; `demo/frontend-demo/index.html`

**[Must] Name the common tags, and tell me why a semantic tag beats a div.**

> The vocabulary splits into semantic structure — header, nav, main, section, article, footer — text
> content like headings, paragraphs and lists, links and images, and generics like div and span. A div
> carries no meaning at all. Semantic elements carry meaning machines act on: screen readers expose nav
> and main as landmarks users jump between, headings build the outline blind users navigate by, and
> search engines weight article and h1 content. Divs are not banned — they are correct when you need a
> styling or scripting hook and no semantic element fits.

`Proves QC: List common HTML tags and describe why they are different from divs.`
`Source: content/01-html/tags-elements-attributes.md`

**[Must] Where do you link a stylesheet, and where do you link a script? Why?**

> The stylesheet goes in the head: `<link rel="stylesheet" href="css/styles.css">`. That way styles are
> known before first paint and users do not see a flash of unstyled content. A script goes at the end of
> the body, or in the head with `defer` — both guarantee the DOM exists when it runs. `defer` also
> overlaps the download with parsing, so it is the modern default. `async` downloads in parallel but runs
> the moment it arrives, with no order guarantee, so it is for independent scripts like analytics.

`Proves QC: Describe how/where you link an external CSS sheet into an HTML document.` /
`Describe how/where you link an external JS file into an HTML document.`
`Source: content/01-html/html-document-structure.md`

**[Should] Build me a form. What decides where the data goes and how?**

> `<form action="/members/register" method="post">` — action is the destination, method is the verb. On
> submit the browser collects every control that has a `name` attribute and sends name-value pairs. No
> name, no submission — `id` is only for labels and scripts and never reaches the server. GET puts the
> pairs in the URL, which is right for bookmarkable searches; POST puts them in the body, which is right
> for state changes and anything sensitive. A password in a GET URL lands in history and server logs.

`Proves QC: Construct an HTML form.`
`Source: content/01-html/forms-inputs.md`

**[Should] Give me the input vocabulary, and tell me how radio buttons become mutually exclusive.**

> One input element with many types: text, password, email, number, checkbox, radio, date, file — plus
> select with options for a dropdown, textarea for multi-line, and button, whose default type inside a
> form is submit. Radios become one choice by **sharing a `name`**; that shared name is the group.
> Checkboxes each get their own name and only submit when checked. Every control gets a label via `for`
> and `id`, or by wrapping — screen readers announce it and clicking it activates the control.

`Proves QC: Take in user input using a variety of input tags (text, checkbox, etc).`
`Source: content/01-html/forms-inputs.md`

**[Should] Your form has `required` and a `pattern` on every field. Does the server still need to
validate?**

> Yes, always. Browser validation is user experience only — instant, friendly feedback. Anyone can delete
> the attributes in DevTools or skip the browser entirely and send the request with curl. The server is
> the trust boundary and must independently revalidate everything it receives. They are complements, not
> alternatives.

`Proves QC: Construct an HTML form.` (validation-attribute depth)
`Source: content/01-html/forms-inputs.md`

---

## CSS

**[Must] Name the parts of a CSS rule.**

> `p { color: red; font-size: 16px; }` — the whole thing is a rule or ruleset. `p` is the selector, the
> braces enclose the declaration block, and `color: red;` is a declaration made of a property and a
> value. A missing semicolon silently kills the *next* declaration, which is a classic debugging trap.

`Proves QC: Describe the structure of a CSS style rule.`
`Source: content/02-css/css-fundamentals-selectors.md`

**[Must] What are the three ways to add styling, and which do you reach for?**

> Inline via the style attribute, internal via a style block in the head, and external via a linked
> stylesheet. External is the production default: one maintainable file across every page, cached by the
> browser after one download, and no presentation mixed into the markup. Inline is for one-off overrides
> or values a script computes; internal suits single-page prototypes.

`Proves QC: Describe the different ways to add styling to an HTML document.`
`Source: content/02-css/css-fundamentals-selectors.md`

**[Must] Show me selectors for a tag, a class, an id, and a combination.**

> `p { }` targets every paragraph, `.card { }` targets anything carrying that class, `#nav { }` targets
> the one element with that id, and `p.card { }` is a compound — paragraphs that *also* have the card
> class. A comma groups selectors onto one block. Classes are the workhorse because they are reusable and
> low specificity; ids are unique so they cannot be reused, and most teams reserve them for anchors and
> JavaScript hooks.

`Proves QC: Use the correct syntax for styling different elements such as by tag, class, id, etc.`
`Source: content/02-css/css-fundamentals-selectors.md`

**[Must] Two rules both set colour on the same element. Which one wins?**

> Inline styles beat both internal and external rules. Among stylesheet rules, specificity decides — it
> is counted as ids, then classes, then elements, compared left to right, so one id beats any number of
> classes. Pseudo-classes and attribute selectors count as classes; the universal selector and
> combinators count as nothing. If specificity ties, source order wins and the later rule applies.
> `!important` overrides all of it, which is exactly why I avoid it — the only way to beat an important
> is another important.

`Proves QC: Describe CSS priority in regards to inline, internal, and external styles.`
`Source: content/02-css/css-fundamentals-selectors.md`

**[Must] Explain the box model. Then: an element is `width: 200px; padding: 10px; border: 2px` — how
wide does it render?**

> Every element renders as a box: content in the middle, then padding inside the border, the border
> itself, then margin outside separating it from neighbours. Padding takes the element's background;
> margin is always transparent. Under the default `content-box`, width sizes only the content, so that
> element renders 224 pixels wide — 200 plus 20 of padding plus 4 of border. With
> `box-sizing: border-box` it renders exactly 200 and the content area shrinks, which is how humans
> reason about size. That is why nearly every codebase opens with a global border-box reset.

`Proves QC: Explain the CSS box model.`
`Source: content/02-css/box-model-properties.md`

**[Nice] What are combinators for?**

> They select by position in the document rather than by inventing a class for every element. A space is
> any descendant at any depth, `>` is a direct child only, `+` is the single immediately-following
> sibling, and `~` is every later sibling under the same parent. So "the first paragraph after any
> heading is the lede" is just `h2 + p` — zero extra markup.

`Proves QC: Describe the benefits of combinators and how to use them.`
`Source: content/02-css/css-fundamentals-selectors.md`

**[Nice] How do you make a page responsive?**

> Fluid layouts with flex or grid, relative units like rem and percentages instead of fixed pixels, and
> media queries where the layout restructures. And the viewport meta tag —
> `width=device-width, initial-scale=1.0` — without which a phone renders at a fake ~980px desktop width
> and shrinks it, so the breakpoints never fire. I write mobile-first: base styles are the small-screen
> layout and `min-width` queries layer complexity upward, with breakpoints where the content breaks
> rather than at a device list.

`Proves QC: Make responsive webpages using CSS.`
`Source: content/02-css/responsive-variables-animations.md`

---

## JavaScript language

**[Must] What is JavaScript?**

> A dynamically typed scripting language — variables carry no declared type and a value's type is checked
> at runtime. Modern engines like V8 JIT-compile it rather than purely interpret it. It runs
> single-threaded with an event loop, in the browser to make pages interactive and in Node.js everywhere
> else. ECMAScript is the written standard; JavaScript is the everyday name for implementations of it.

`Proves QC: Describe what JS is.`
`Source: content/03-javascript/js-language-runtime.md`

**[Must] `let` versus `const` versus `var`.**

> I default to `const` and use `let` only when I will reassign — const documents intent and turns an
> accidental reassignment into an error, though it freezes the binding, not the value, so a const array
> can still be pushed to. `var` is legacy: function-scoped and hoisted, so it leaks out of blocks. The
> proof is a loop with async callbacks — with `var` there is one shared variable already at its final
> value when the callbacks run, so you get 3, 3, 3; `let` gives each iteration its own binding and you
> get 0, 1, 2.

`Proves QC: Create variables in JS.` / `Describe the different variable scopes in JS.`
`Source: content/03-javascript/variables-scope-types-coercion.md`

**[Must] What are the data types, and what does `typeof null` return?**

> Seven primitives — string, number, boolean, null, undefined, symbol, bigint — and everything else is an
> object, including arrays and functions. `typeof null` returns `"object"`, which is a historic bug kept
> for compatibility; you just memorize it. `typeof` on an array also says object, so `Array.isArray()` is
> the real check. Undefined means never assigned; null means deliberately empty.

`Proves QC: Explain the different data types in JS.`
`Source: content/03-javascript/variables-scope-types-coercion.md`

**[Must] What is type coercion? Predict `"5" + 1` and `"5" - 1`.**

> Coercion is JavaScript converting types implicitly during an operation. `+` is the trap: if either
> operand is a string it concatenates, so `"5" + 1` is `"51"`. Every other arithmetic operator converts
> to number, so `"5" - 1` is `4`. The same thing happens in comparisons — `==` coerces before comparing,
> so `"5" == 5` is true, while `===` compares type and value with no coercion and gives false. I use
> `===` by default because its result is predictable from the operands alone.

`Proves QC: Describe what type coercion is.`
`Source: content/03-javascript/variables-scope-types-coercion.md`

**[Must] What is truthy and falsy? List the falsy values.**

> It is how a value coerces to boolean in a condition. Exactly six values are falsy: `false`, `0`, the
> empty string, `null`, `undefined`, and `NaN`. Everything else is truthy — including the string `"0"`,
> an empty array, and an empty object. That matters for defaults: `input || "guest"` replaces a
> legitimate `0` or empty string, which is exactly why `??` exists — it falls back only on null or
> undefined.

`Proves QC: Describe what truthy/falsy is.`
`Source: content/03-javascript/variables-scope-types-coercion.md`

**[Must] Create an object and an array, and tell me how you read a dynamic key.**

> `const book = { title: "Dune", pages: 412 };` and `const skus = ["BK-001", "BK-002"];`. Dot access for
> keys known at write time — `book.title` — and bracket access when the key is dynamic or not a valid
> identifier, `book[key]`. I also lean on destructuring to pull properties into variables and spread to
> copy with overrides, remembering spread is a **shallow** copy, so nested objects are shared.

`Proves QC: Create objects in JS.` / `Create arrays in JS.`
`Source: content/03-javascript/objects-arrays-loops.md`

**[Must] Talk me through the array methods, and which ones mutate.**

> Map transforms and returns a new array, filter keeps matching items into a new array, reduce folds
> everything to one value, find grabs the first match or undefined, and forEach is side effects only.
> Those all leave the original alone. The mutators are push and pop, shift and unshift, splice, sort, and
> reverse. Two traps worth naming: slice copies out while splice edits in place, and sort both mutates
> *and* compares as strings by default, so `[10, 2].sort()` gives `[10, 2]` — always pass
> `(a, b) => a - b` for numbers.

`Proves QC: Describe the different array methods and how to use them.`
`Source: content/03-javascript/objects-arrays-loops.md`

**[Must] How do you loop through an array, and when would you pick each?**

> `for...of` gives the values and is my default for arrays. A classic `for` loop when I need index
> arithmetic, an early `break`, or raw speed in a hot path. `forEach` for a callback per item — but it
> cannot break out. And `for...in` gives *keys*, so it belongs on objects, not arrays; on an array it
> yields string indexes plus any inherited enumerable keys.

`Proves QC: Loop through arrays.`
`Source: content/03-javascript/objects-arrays-loops.md`

**[Must] What are the different types of functions?**

> Declarations, which hoist fully so they are callable before their line; function expressions assigned
> to a variable; arrow functions; anonymous functions passed inline as arguments; and methods stored on
> an object property. The form changes hoisting and `this`, not passability — all of them are values. On
> top of that, default parameters, rest parameters, and the IIFE shape are worth recognizing.

`Proves QC: Describe the different types of functions in JS.`
`Source: content/03-javascript/functions-this-closures.md`

**[Must] How do you handle errors in JavaScript?**

> `try` around the risky call, `catch (e)` to handle a throw, and `finally` for cleanup — finally runs
> whether the try succeeded, threw, or returned, so spinners and resources always get released. I throw
> `new Error("message")`, never a string, because an Error instance carries `message`, `name`, and
> `stack`; a thrown string has no stack trace and code reading `e.message` gets undefined. A custom class
> extending Error gives me typed catches via `instanceof`. And I catch only where I can act — retry,
> substitute a fallback, or tell the user something useful; otherwise I let it propagate, because a catch
> that cannot act is a silencer.

`Proves QC: Handle errors in JS.`
`Source: content/03-javascript/error-handling.md`

**[Should] What are template literals for?**

> Backtick strings that interpolate expressions with `${}` and span multiple lines. They are the standard
> way to build any string from data — messages, URLs, markup. One caution: when the built string goes
> into `innerHTML`, every interpolated value is a potential injection point, so untrusted input needs
> escaping or should be assigned separately with `textContent`.

`Proves QC: Use template literals.`
`Source: content/03-javascript/dom-selection-manipulation.md`

**[Should] What is `this`?**

> It is not "the function's object" — it is bound per call, by how the function is invoked. In a method
> call it is the object before the dot. Under `new` it is the new instance. In a plain call it is
> undefined in strict mode — that is the classic detached-method bug, where a method passed as a callback
> loses its object. And an arrow function has no `this` of its own; it inherits lexically from the
> surrounding scope. `call` and `apply` invoke with an explicit `this`, and `bind` returns a permanently
> bound copy.

`Proves QC: Describe what the this keyword is.`
`Source: content/03-javascript/functions-this-closures.md`

**[Should] What is a callback, and what problem do promises solve?**

> A callback is a function you hand to other code to be invoked later — later in time like a timer,
> per event like a click handler, or once per item like an array method. Callbacks are the primitive
> under every async and iteration API in the language. Nesting them for sequential async work produces
> callback hell: pyramids of indentation with error handling smeared everywhere. Promises exist to fix
> exactly that.

`Proves QC: Explain the role of callbacks in JavaScript programming.`
`Source: content/03-javascript/functions-this-closures.md`

**[Should] Define an arrow function and give me the benefits — and when not to use one.**

> `const add = (a, b) => a + b;` — a single parameter can drop the parens, a body without braces is an
> implicit return, and returning an object literal needs wrapping parens. Two benefits: shorter syntax
> for inline callbacks, and lexical `this` — an arrow has no `this` of its own, so a callback inside a
> method keeps the object. That same property is why I do not use them as object methods or as
> constructors, or where the handler needs `this` to be the element.

`Proves QC: Define arrow functions and explain the benefits of using arrow functions.` /
`Create arrow functions.` / `Create anonymous functions.`
`Source: content/03-javascript/functions-this-closures.md`

**[Nice] Explain a closure.**

> A function that retains access to the variables of the scope where it was defined, even after that
> scope has returned. The canonical example is a counter factory: a local `count` plus returned functions
> that close over it, so the state is private and each call to the factory gets an independent copy.
> That is how JavaScript had private state before classes. The cost: whatever a closure holds cannot be
> garbage-collected while the closure lives.

`Proves QC: Describe and explain a closure.`
`Source: content/03-javascript/functions-this-closures.md`

**[Nice] What is hoisting?**

> Declarations are processed before the code runs, but the three forms behave differently. `var` hoists
> initialized to undefined, so reading it early gives undefined rather than an error. Function
> declarations hoist fully, name and body, so they are callable before their line. `let` and `const`
> hoist too but stay uninitialized in the temporal dead zone until their declaration executes — reading
> them earlier throws a ReferenceError. That is a feature: a loud error beats undefined silently
> propagating through your math.

`Proves QC: Describe what function and variable hoisting is.`
`Source: content/03-javascript/variables-scope-types-coercion.md`

**[Nice] How does inheritance work in JavaScript?**

> Through delegation up the prototype chain. Objects do not copy behaviour from a parent; a failed
> property lookup walks the object's prototype, then that prototype's prototype, until null. `class Book
> extends Media` is syntax sugar over exactly that mechanism — methods land on `Book.prototype` and
> instances delegate to it, and an override works by sitting earlier in the chain. `prototype` lives on
> the constructor; the instance's own link is `__proto__`, which you read with
> `Object.getPrototypeOf`.

`Proves QC: Describe how inheritance works in JS.`
`Source: content/03-javascript/objects-arrays-loops.md`

---

## Browser JavaScript

**[Must] What is the DOM?**

> The Document Object Model — the browser's in-memory tree of the page, one node per element, attribute,
> and text run, built when the HTML is parsed. The rendered page is a projection of that tree, so
> changing a node re-renders that part of the screen. JavaScript reaches it through the global `document`
> object. It is a browser API standard, not part of the JavaScript language, which is why it does not
> exist in Node.

`Proves QC: Describe what the DOM is.`
`Source: content/03-javascript/dom-selection-manipulation.md`

**[Must] How do you query the DOM?**

> `document.querySelector` for the first match of any CSS selector, `querySelectorAll` for all matches as
> a static NodeList — a snapshot that supports forEach — and `getElementById` as the fast path when I
> have an id. The older `getElementsByClassName` family returns a live HTMLCollection that updates itself
> as the document changes, which surprises people who mutate the DOM while iterating. A missed match
> returns null, so I check before dereferencing.

`Proves QC: Query the DOM for elements.`
`Source: content/03-javascript/dom-selection-manipulation.md`

**[Must] How do you insert a new element? And when would you use `innerHTML` instead?**

> `document.createElement("li")`, set its `textContent` and classes, then `parent.append(li)` — that is
> the granular insertion API, and looping it over an array is the render-a-collection pattern behind
> every list you will build. `innerHTML` is the bulk alternative: it parses its input as markup, so it is
> for markup *I* wrote. Untrusted data goes in through `textContent`, because writing it with `innerHTML`
> lets an attacker inject something like an `img` with an `onerror` handler — that is a cross-site
> scripting hole.

`Proves QC: Insert new elements into the DOM.`
`Source: content/03-javascript/dom-selection-manipulation.md`; `demo/frontend-demo/js/app.js`

**[Must] What is an event listener?**

> A callback registered to run when a given event fires on an element:
> `btn.addEventListener("click", handleSave)`. The handler receives an event object describing what
> happened. `addEventListener` stacks any number of handlers and takes an options object; the older
> `btn.onclick =` property style allows only one, since assigning again overwrites. To remove a listener
> I must pass the identical function reference, which is why an inline arrow is effectively unremovable.

`Proves QC: Describe what event listeners are.`
`Source: content/03-javascript/events.md`

**[Must] Synchronous versus asynchronous — and why does it matter that JavaScript is single-threaded?**

> Synchronous code blocks until each line finishes. JavaScript runs on one thread, so a long synchronous
> computation freezes everything — no clicks, no rendering. Asynchronous operations like timers and HTTP
> are therefore scheduled: the work starts, the function returns immediately, and a callback runs later
> with the result. Promises are the standard way to write that "later" part.

`Proves QC: Explain the difference between synchronous and asynchronous programming.`
`Source: content/03-javascript/promises-async.md`

**[Must] What is a Promise?**

> An object representing the eventual result of an asynchronous operation. It is always in one of three
> states — pending, fulfilled with a value, or rejected with a reason — and once settled it never
> changes. I consume it by attaching callbacks with `.then`, `.catch`, and `.finally`, and each `.then`
> returns a new promise, which is what makes chaining work: return a value and the next then sees it,
> return a promise and the chain waits for it.

`Proves QC: Explain what a JavaScript Promise is and when it is used to handle asynchronous
operations.`
`Source: content/03-javascript/promises-async.md`

**[Must] Name the promise methods.**

> On an instance: `.then` for fulfilment, `.catch` for a rejection anywhere above it, `.finally` for
> either. The static combinators are `Promise.all`, which resolves with every value in order but fails
> fast on the first rejection; `Promise.allSettled`, which never rejects and reports a status per item;
> `Promise.race`, which settles with the first to settle either way — good for timeouts; and
> `Promise.any`, which takes the first to fulfil and rejects only if all reject.

`Proves QC: Describe the different promise methods.`
`Source: content/03-javascript/promises-async.md`

**[Must] What type of object does the Fetch API return?**

> `fetch` returns a Promise that resolves to a **Response** object — headers and status. The body is a
> second asynchronous step: `response.json()` itself returns a Promise for the parsed body. That is why
> reading JSON takes two awaits — the first for the Response, the second for the parsed body.

`Proves QC: Describe what type of object the Fetch API returns.`
`Source: content/03-javascript/fetch-json-http.md`

**[Must] A fetch to `/api/books/999` comes back 404. Does the promise reject?**

> No. Fetch rejects only on network failure — DNS down, connection refused, a CORS block. A 4xx or 5xx
> is a successful conversation whose answer was "no," so it resolves normally. Status checking is my
> job: `if (!response.ok) throw new Error(...)`, using `response.ok` for 200-299 or reading
> `response.status`. So there are two failure lanes — the catch for unreachability and the ok check for
> refusal — and callers get one thrown error either way.

`Proves QC: Handle a failed request when using the Fetch API.`
`Source: content/03-javascript/fetch-json-http.md`; `demo/frontend-demo/js/app.js`

**[Must] What is JSON?**

> JavaScript Object Notation — a language-independent **text** format for exchanging data, so a .NET API,
> a Python script, and a browser all read the same bytes. Its rules are stricter than JS object literals:
> double-quoted keys, no trailing commas, no comments, and values limited to strings, numbers, booleans,
> null, arrays, and objects. No functions and no undefined; dates travel as strings.

`Proves QC: Explain what JSON is.`
`Source: content/03-javascript/fetch-json-http.md`

**[Should] List the steps to send an HTTP request with fetch.**

> Build the URL, including any encoded query parameters. Build the options object — method, headers, and
> a stringified body for a POST. Await the fetch call. Check `response.ok` and throw or branch on an HTTP
> error. Parse the body with `await response.json()`. Then use it — render it, update state. For JSON I
> set `Content-Type: application/json` and stringify the body myself; fetch does not serialize objects
> for me.

`Proves QC: List the steps to sending an HTTP request using the Fetch API.`
`Source: content/03-javascript/fetch-json-http.md`

**[Should] What is `async`/`await`, and how does it compare to `.then()`?**

> It is syntax over promises — no new machinery. An `async` function always returns a Promise, and
> `await` unwraps a value and pauses only that function; the thread moves on, so awaiting never blocks
> the page. Compared with a `.then` chain, the await version reads top to bottom like synchronous code
> and rejections surface as ordinary exceptions I handle with `try`/`catch` instead of a `.catch`
> callback. One caution: await sequentially only when the second call needs the first's result;
> independent calls go into `Promise.all`, which turns time A plus time B into the max of the two.

`Proves QC: Describe what async/await is and how they compare to using .then().`
`Source: content/03-javascript/promises-async.md`

**[Should] What do `JSON.stringify` and `JSON.parse` do?**

> `stringify` serializes an object to a JSON string for the wire; `parse` turns a JSON string back into
> an object. The detail worth knowing: stringify silently drops functions and `undefined` values, so the
> round trip is a lossy clone for anything richer than plain data — methods never survive.

`Proves QC: Explain what JSON.stringify() and JSON.parse() are.`
`Source: content/03-javascript/fetch-json-http.md`

**[Nice] Explain bubbling and capturing.**

> When you click a button inside a list item inside a list, the event travels in phases: capturing from
> the document down through the ancestors to the target, then bubbling from the target back up. Listeners
> fire during the bubbling phase by default; passing `{ capture: true }` opts into the way down. The
> practical payoff of bubbling is event delegation — one listener on the container plus
> `event.target.closest(...)` handles every row, which means fewer listeners and rows added later just
> work.

`Proves QC: Describe what bubbling and capturing are and their difference.`
`Source: content/03-javascript/events.md`

**[Nice] Name some methods and properties on the event object.**

> `event.target` is the element the event originated on; `event.currentTarget` is the element my listener
> is attached to — they differ whenever the event bubbled from a descendant. `preventDefault()` cancels
> the browser's default action, like a form's page reload or a link navigating, while the event keeps
> propagating. `stopPropagation()` halts the journey through other listeners but does *not* cancel the
> default. The two are independent and orthogonal.

`Proves QC: Describe some methods on the event object and what they do.`
`Source: content/03-javascript/events.md`

**[Nice] Explain the event loop. What does this log: sync log, `setTimeout(..., 0)`, a resolved
`.then`, sync log?**

> JavaScript has one call stack; async callbacks wait in queues and the event loop moves them onto the
> stack only when it is empty. There are two priorities: microtasks — promise callbacks — drain
> completely before the next macrotask like a timer or an event. So the order is both synchronous logs
> first, then the promise callback, then the timer, even at zero milliseconds. `setTimeout(fn, 0)` does
> not mean "now"; it means "queue as a macrotask after the current stack and all pending microtasks."

`Proves QC: Describe and explain the event loop.`
`Source: content/03-javascript/promises-async.md`

**[Nice] Fetch versus XHR.**

> XHR — `XMLHttpRequest` — is the older event and callback-based request object: you `open`, assign an
> `onload` handler, and `send`. Fetch replaced it with a promise-based API that composes with
> async/await, a cleaner Request and Response model, and streaming support. The shared quirk is worth
> naming: neither one treats an HTTP error status as a failure, so you check the status either way.

`Proves QC: Describe the difference between Fetch and XHR.`
`Source: content/03-javascript/fetch-json-http.md`

**[Nice] How do you handle errors around `await`?**

> Wrap the awaited calls in `try`/`catch` — await unwraps a rejection into a thrown exception, so one
> catch covers every await above it, including a network failure, an HTTP error I threw myself after
> checking `response.ok`, and bad JSON. A `.then` chain needs a terminal `.catch` instead. If neither is
> present the rejection becomes an unhandled rejection: nothing visible happens at first, then the
> runtime reports it far from the cause.

`Proves QC: Implement error handling using try-catch blocks with async/await.` /
`Explain how to chain multiple asynchronous operations using Promises or async/await.`
`Source: content/03-javascript/error-handling.md`, `promises-async.md`

---

## TypeScript

**[Must] Compare TypeScript to JavaScript.**

> TypeScript is a superset of JavaScript that adds a static type layer checked at compile time. Every
> valid `.js` file is already valid `.ts`, so you can adopt it gradually. The types exist only for the
> compiler — when `tsc` transpiles, all type information is **erased** and plain JavaScript comes out.
> There is no TypeScript runtime; browsers and node execute the emitted JS. The consequence worth saying
> out loud is that types cannot check anything at runtime, so data arriving from fetch or user input
> still has to be validated with real code.

`Proves QC: Compare/contrast TypeScript to JavaScript.`
`Source: content/04-typescript/why-typescript-tooling.md`

**[Must] Show me basic types, and when do you bother annotating?**

> `let age: number = 34;`, `let title: string = "Dune";`, `let done: boolean = false;` — but TypeScript
> infers from initializers, so on a local variable the annotation adds nothing. My working rule is: let
> inference carry local variables and annotate function boundaries. Parameters have no call site to infer
> from yet, and an explicit return type turns a function into a checked contract instead of whatever the
> body happens to produce.

`Proves QC: Describe and implement basic types in TypeScript.`
`Source: content/04-typescript/basic-special-object-types.md`

**[Must] How do you define your own types?**

> Two ways: `interface User { id: number; name: string }` or `type Point = { x: number; y: number }`.
> For a plain object shape they are interchangeable — same checking, same erasure, zero runtime cost. The
> differences are at the edges: an alias can name *any* type, including unions, primitives, and function
> types, while an interface only describes object or function shapes; interfaces get `extends` and
> declaration merging, aliases get mapped and conditional forms. My default is interface for public
> object contracts and type for everything else.

`Proves QC: Implement user defined types in TypeScript.` / `Describe and implement type aliasing.`
`Source: content/04-typescript/aliases-interfaces-unions.md`

**[Must] What is casting in TypeScript, and what does it actually do?**

> A type assertion — `const input = document.getElementById("email") as HTMLInputElement;` — overrules
> the compiler's belief about a value's type. The critical part: it converts **nothing** at runtime. It
> is erased like every other type, so if I assert wrongly the code compiles clean and crashes later when
> I touch a property that never existed. Recognize the older angle-bracket spelling but write `as`,
> because `<T>` collides with JSX syntax. And `value as unknown as T` is the escape hatch that silences
> even the compiler's sanity check — a smell every time.

`Proves QC: Describe and implement casting in TypeScript.`
`Source: content/04-typescript/casting-guards-asconst.md`

**[Must] Walk me through transpiling and running TypeScript.**

> Install the compiler — globally, or as a devDependency and run it through npx. `tsc app.ts`
> type-checks and emits `app.js` next to it, then `node app.js` runs the emitted JavaScript. In a real
> project I generate a tsconfig with `npx tsc --init` and then run a bare `npx tsc`, which compiles the
> whole project by the config. `npx ts-node app.ts` collapses compile-and-run into one command. One
> default that surprises people: type errors do not stop emit — `tsc` reports the error and still writes
> the JS, unless you set `noEmitOnError`.

`Proves QC: Describe and demonstrate the process to transpile and run TypeScript.`
`Source: content/04-typescript/why-typescript-tooling.md`

**[Should] How would you use TypeScript with no framework at all?**

> From an empty folder: `npm init -y`, `npm install --save-dev typescript`, `npx tsc --init` to generate
> the config, then write plain `.ts` files, compile the project with a bare `npx tsc`, and run the output
> with node. No bundler, no JSX — just the compiler and node. That is the shape I reach for when
> practising the language itself, and it is exactly how the Week 6 typed API client was built.

`Proves QC: Implement TypeScript outside of Angular/React environments using plain .ts files.`
`Source: content/04-typescript/why-typescript-tooling.md`; `demo/frontend-demo/ts/`

**[Should] What does the `strict` flag do?**

> It is an umbrella that turns on the whole strict-checking family. The two to name are
> **strictNullChecks** — null and undefined stop being assignable to everything, so a maybe-absent value
> must be typed `string | undefined` and handled, which kills the biggest class of JavaScript runtime
> crashes — and **noImplicitAny**, which stops untyped parameters silently becoming `any`. Friends
> include strictFunctionTypes and strictPropertyInitialization. New projects turn it on day one, because
> the cost is near zero with no code yet; retrofitting it onto a mature codebase surfaces hundreds of
> loose spots at once.

`Proves QC: Describe the purpose of the "strict" flag in the tsconfig.json file.`
`Source: content/04-typescript/tsconfig.md`

**[Should] What is a union type, and what can you do with one before narrowing?**

> A union says a value is one of several types: `let id: string | number`. Before narrowing you may only
> use members present on **every** arm — `id.toString()` is fine because both have it, but
> `id.toUpperCase()` errors while `id` might be a number. Unions of string literals like
> `"pending" | "shipped" | "delivered"` are the idiomatic way to model a closed set of values: plain
> strings at runtime, exact values checked at compile time.

`Proves QC: Describe and implement union types.`
`Source: content/04-typescript/aliases-interfaces-unions.md`

**[Should] What is a type guard? Contrast it with an assertion.**

> A guard is a runtime check the compiler recognizes and uses to narrow a union inside the guarded block:
> `typeof` for primitives, `instanceof` for class instances, `in` for property presence, truthiness for
> stripping null and undefined. My go-to is a discriminated union — every arm carries a literal `kind`,
> a switch on it narrows perfectly, and a `never` default means adding an arm turns every stale switch
> into a compile error. A reusable guard is written as a type predicate: `function isBook(x: unknown): x
> is Book`. The contrast: an assertion says "trust me now" and shifts the risk to runtime; a guard proves
> it at runtime and the compiler rewards the proof with narrowing. I default to guards at data
> boundaries.

`Proves QC: Describe and implement type guards.`
`Source: content/04-typescript/casting-guards-asconst.md`; `demo/frontend-demo/ts/ts-client.ts`

**[Nice] Which tsconfig options would you set for a node tool versus a browser app?**

> Both get `strict: true`, `rootDir` and `outDir` to keep emit out of the source tree, and `sourceMap`
> for debugging. For a browser app I target what shipping browsers run — say es2017 — with
> `module: esnext`, and let a bundler own module wiring. For a node tool I can target modern JS because
> I control the runtime, emit commonjs or ESM to match how node will load it, and add `esModuleInterop`
> so imports from CommonJS packages work. Also recognize error TS5011: setting `outDir` requires an
> explicit `rootDir`.

`Proves QC: Configure the TypeScript compiler using options in the tsconfig.json based on project
needs.`
`Source: content/04-typescript/tsconfig.md`

**[Nice] Why a generic instead of `any`?**

> Because a generic preserves the connection between input and output. `function first<T>(arr: T[]): T`
> called with an array of strings returns a string, and everything downstream stays checked. With
> `any[]` the type information dies at the boundary and the compiler goes silent for the rest of the call
> chain. Constraints — `<T extends { id: number }>` — let the body use members by demanding callers
> supply them. And I read nested signatures inside-out: `Promise<Map<string, Order[]>>` is a promise
> resolving to a map from strings to arrays of Orders.

`Proves QC: Describe and leverage generic types.`
`Source: content/04-typescript/classes-generics-functions.md`

---

## React

**[Must] What is a functional component?**

> A JavaScript function that returns a React element. Two things make it a component rather than an
> ordinary function: a capitalized name — React treats lowercase tags as raw HTML and capitalized tags as
> components — and returning something renderable, JSX or a string or null. Props go in, UI comes out. No
> class, no render method, no boilerplate.

`Proves QC: Describe and implement functional components in React.`
`Source: content/05-react/react-jsx-components.md`

**[Must] What actually is JSX?**

> Syntax sugar. It looks like HTML inside my code, but it is neither HTML nor a string — a compiler
> rewrites it into ordinary function calls, so `<h1 className="title">Hello</h1>` becomes
> `React.createElement("h1", { className: "title" }, "Hello")`. Because it *is* JavaScript, attributes
> use camelCase and JS names — className, htmlFor, onClick — every element must close, and a component
> returns one root, so siblings go in a parent or a Fragment. Curly braces embed any JavaScript
> **expression**; a statement like `if` is illegal, so I use a ternary, a logical and, or compute the
> value above the return.

`Proves QC: Write and explain JSX syntax and how it integrates with JavaScript.`
`Source: content/05-react/react-jsx-components.md`

**[Must] How do you create and run a React app with Vite?**

> `npm create vite@latest my-app -- --template react`, then `cd my-app`, `npm install`, and
> `npm run dev`, which serves on localhost:5173 with hot reloading. The lone double dash separates npm's
> own arguments from the ones passed through to the Vite scaffolder, so `--template react` actually
> reaches Vite; `react-ts` gives you the TypeScript template. For shipping, `npm run build` produces an
> optimized bundle in `dist/` and `npm run preview` serves it locally.

`Proves QC: Create and run a React application using Vite CLI.`
`Source: content/05-react/react-jsx-components.md`

**[Must] SPA versus MPA.**

> A multi-page app requests a whole new HTML document from the server on every navigation and repaints
> from scratch — simple, great default SEO, fast first paint, but a visible reload and lost client state
> every time. A single-page app loads one HTML shell plus a JavaScript bundle once, then swaps views
> client-side and pulls data as JSON from an API. Navigation feels instant and client state survives, at
> the cost of a heavier first load and extra work for SEO and first paint, plus the app owning routing
> and history itself. React apps are SPAs by default.

`Proves QC: Explain the difference between Single Page Applications and Multi Page Applications.`
`Source: content/05-react/spa-vs-mpa-virtualdom.md`

**[Must] How does the Virtual DOM work, and why is it faster?**

> The Virtual DOM is a lightweight in-memory JavaScript tree of plain objects describing what the UI
> should look like. It is cheap to build and cheap to compare, unlike real DOM nodes, where every change
> can trigger style recalculation, layout, and repaint. When state changes React does three things:
> renders a new Virtual DOM tree, diffs it against the previous one — that is reconciliation — and
> commits only the minimal set of differences to the real DOM. So "re-render on every state change" is
> cheap, because the re-render produces objects and only genuine differences reach the expensive part.
> Keys give list items stable identity so the diff can match items across renders instead of rebuilding.

`Proves QC: Describe how the React Virtual DOM works and how it improves performance.`
`Source: content/05-react/spa-vs-mpa-virtualdom.md`

**[Must] Props versus state.**

> Props are read-only inputs a parent passes down; a component must never reassign its own props. State
> is private memory a component owns, created with `useState`, and changing it re-renders the component.
> `const [count, setCount] = useState(0)` gives me the current value and a setter, and the argument is
> the initial value used only on the first render. I never assign to the variable — React would never
> learn it changed — and when the next value derives from the previous I pass a function,
> `setCount(prev => prev + 1)`.

`Proves QC: Pass props to components and manage local component state.`
`Source: content/05-react/props-state-hooks-intro.md`

**[Must] What is a hook, and what do useState, useEffect, and useContext do?**

> A hook is a `use`-prefixed function that lets a plain function component tap into React features that
> used to require classes. Two hard rules: call them only at the top level, never inside loops,
> conditions, or nested functions, and only from components or other hooks — React relies on the same
> call order every render. `useState` is local memory. `useEffect` runs side effects after render, with
> a dependency array controlling re-runs and an optional cleanup return. `useContext` reads shared data
> from a Provider without threading props through every level.

`Proves QC: Utilize and explain common React hooks: useState, useEffect, and useContext.`
`Source: content/05-react/props-state-hooks-intro.md`, `component-lifecycle.md`,
`context-global-state.md`

**[Must] Explain the React component lifecycle.**

> A component mounts — created and inserted into the DOM — updates when its state or props change, and
> unmounts when it is removed. Class components had named methods for those; function components collapse
> all three into `useEffect`. The dependency array is what selects the phase: an empty array runs once
> after mount, `[dep]` runs on mount and whenever that value changes, and no array at all runs after
> every render. The function I return from the effect is the cleanup — React calls it before the next run
> and on unmount, which is where I stop timers, subscriptions, and listeners so there are no leaks or
> stale updates.

`Proves QC: Explain the lifecycle of a React component.`
`Source: content/05-react/component-lifecycle.md`

**[Must] How do you make an HTTP request from React and handle the response?**

> I put the call in a `useEffect` with an empty dependency array so it runs on mount, and I model three
> states: loading, error, and data. With Axios I create one configured instance —
> `axios.create({ baseURL })` — import it everywhere, and read the parsed body off `res.data`; Axios
> rejects on HTTP error statuses so failures land in `catch`. With fetch I have to check `res.ok` myself
> because a 404 resolves normally, and call `.json()` explicitly. Either way I clear the loading flag in
> `finally` so it clears on both paths, and I keep an `active` flag in the effect's cleanup so a late
> response cannot set state on an unmounted component.

`Proves QC: Make HTTP requests using Axios or Fetch and handle the response.` /
`Leverage NPM libraries in a React project to add functionality.`
`Source: content/05-react/axios-fetch-data.md`; `demo/react-spa-demo/src/components/CatalogPage.tsx`

**[Must] Explain state immutability in React. Why does `books.push(x); setBooks(books)` fail?**

> Because React decides whether to re-render by comparing the new state to the old **by reference**. Push
> mutated the existing array and I handed back the very same reference, so React's `Object.is` check saw
> no change and skipped the render. The rule is never mutate state in place — always produce a new value:
> `[...books, newBook]` to add, `filter` to remove, `map` returning `{ ...b, available: false }` to
> update one item, and spread every nested level you touch. The traps are push, splice, and sort, and
> `Object.assign(state, patch)`, which writes into the existing object and returns it.

`Proves QC: Explain and apply the principles of state immutability in React.`
`Source: content/05-react/state-immutability-lifting.md`

**[Must] How do you render a list, and what is the `key` for?**

> Call `.map()` on the data array and return one element per item — there is no loop directive, it is
> just JavaScript. The key gives each item a stable identity so that when the list re-renders React can
> match new elements to the ones it drew last time and compute the minimal DOM update. A key must be
> unique among siblings and stable across renders, so I use a real ID from the data. The array index is a
> poor key because it describes a position, not an item: delete the first row and every index shifts, so
> React mis-matches elements and per-row DOM state like input text or checkbox state sticks to the wrong
> row.

`Proves QC: Use Lists and Keys correctly to render dynamic components efficiently.`
`Source: content/05-react/rendering-lists-keys.md`

**[Must] How do you handle user input and manage form state?**

> With controlled inputs: bind `value` to state and update that state in `onChange`, so React state is
> the single source of truth and the DOM never holds a value React does not know about. For several
> fields I keep one state object and use a single handler keyed by the input's `name` —
> `setForm(prev => ({ ...prev, [name]: value }))` — branching on `type` for checkboxes, which use
> `checked` rather than `value`. On submit I put `onSubmit` on the form, not `onClick` on the button, so
> the Enter key works, and I call `e.preventDefault()` to stop the browser's full-page reload. Resetting
> the form is just resetting the state object.

`Proves QC: Handle user input through form elements and manage form state.`
`Source: content/05-react/events-controlled-forms.md`

**[Must] How does a child tell its parent something happened?**

> Data flows down as props; a child cannot reach up and change the parent's state. So the parent passes a
> **function** down as a prop and the child calls it, optionally with data. That call runs the parent's
> setter, the parent re-renders, and the new value flows back down as props. Every "child tells parent"
> interaction works that way: form submits, button clicks, list selections, delete buttons.

`Proves QC: Implement component communication through props and callbacks (Parent to Child & vice
versa).` / `Describe how one-way data flow works in React.`
`Source: content/05-react/props-state-hooks-intro.md`

**[Must] Why build nested component trees?**

> Because real interfaces are not one giant component. I break a screen into the smallest single-purpose
> pieces and compose them exactly the way I compose functions, so the JSX reads like an outline of the
> page — an App containing a Nav and a Main, the Main containing Cards. Small components are easier to
> name, test, reuse, and reason about than one 400-line function, and composition is the core design
> skill in React.

`Proves QC: Build and use nested component structures to model UI architecture.`
`Source: content/05-react/react-jsx-components.md`

**[Must] Three ways to style a component.**

> Inline styles — a JavaScript object on the `style` prop with camelCase properties, note the double
> braces because the outer pair enters JavaScript and the inner is the object literal; good for one-off
> dynamic values, useless for reuse since there are no pseudo-selectors or media queries. CSS Modules —
> a `Something.module.css` file imported as an object, where the build tool renames every class to be
> globally unique so two components can both define `.card`. And a plain external stylesheet imported
> once, with global class names, which is simplest and right for resets and design tokens.

`Proves QC: Apply styling to components using inline styles, CSS modules, or external stylesheets.`
`Source: content/05-react/react-jsx-components.md`

**[Must] Set up routing in an SPA.**

> `npm install react-router-dom`, wrap the app once in `BrowserRouter`, then declare a `Routes` block
> with one `Route` per path, each naming a `path` and the `element` to render. `path="*"` is the
> catch-all for a 404 view. I navigate with `Link` or `NavLink`, never a plain anchor — an anchor
> triggers a full page reload and throws away the app's state, while Link updates the URL through the
> History API and lets the router swap the view client-side.

`Proves QC: Use React Router to implement navigation in a single-page application.` /
`Route users between components through the use of BrowserRouter.`
`Source: content/05-react/routing-react-router.md`

**[Must] When would you reach for `useReducer` over `useState`?**

> When several pieces of state have to change together consistently, or when the state is really a
> machine with named transitions like idle, loading, success, error. A reducer is a pure function
> `(state, action) => newState`; `useReducer` returns the state and a `dispatch`, and every legal
> transition lives in one switch instead of five scattered setters. In TypeScript I type the actions as a
> discriminated union on a literal `type` field, so the switch narrows each case to exactly its payload
> and the compiler flags an action I forgot to handle. The reducer must return new immutable state, same
> as `useState`. A useful tell: if I am calling several setters together to keep them in sync, that is a
> reducer waiting to be written.

`Proves QC: Use useReducer for complex state management scenarios.` /
`Use a Reducer to manage a set of complex known states.`
`Source: content/05-react/useReducer-complex-state.md`; `demo/react-spa-demo/src/auth/authReducer.ts`

**[Should] What problem does Context solve, and what are its three pieces?**

> Prop-drilling — threading a prop through components that do not use it just to reach a deep consumer.
> Context lets me publish a value once at the top and read it anywhere below. Three pieces:
> `createContext` for the Context object, whose argument is the default used only when there is no
> Provider above; a Provider component that owns the state with useState or useReducer and exposes both
> data and updaters through `value`; and `useContext` in the consumers. I wrap `useContext` in a custom
> hook like `useAuth` that throws when the context is null, so using it outside its Provider is a loud
> error instead of a silent default.

`Proves QC: Use Context.Provider tags to wrap components and distribute application state.` /
`Use createContext and Context.Provider to manage global state.`
`Source: content/05-react/context-global-state.md`; `demo/react-spa-demo/src/auth/AuthContext.tsx`

**[Should] When should you *not* use Context?**

> For state that changes constantly, because every consumer re-renders on every value change — a mouse
> position or a text field would re-render half the tree. And it is not a substitute for props: if only
> one child needs a value, pass a prop. Context earns its keep for stable cross-cutting values like auth,
> theme, or locale. For large apps with heavy, frequently-updated shared state, libraries like Redux
> Toolkit or Zustand add the selective subscriptions Context lacks.

`Proves QC: Use Context.Provider tags to wrap components and distribute application state.` (judgment
depth)
`Source: content/05-react/context-global-state.md`

**[Should] What is a route guard, and does it make the app secure?**

> A guard is a wrapper around protected routes that checks auth state and renders `<Navigate to="/login"
> replace />` instead of the protected element when the user is not authenticated — or redirects home
> when they are signed in but lack the required role. `replace` swaps the history entry so the guarded
> URL does not linger in the back button. And no, it does not make the app secure: the guard and any
> hidden admin buttons run in the browser, so they are user experience, not a security boundary. The
> server authorizes every protected request; the guard just spares an unauthenticated user a screen full
> of failed calls.

`Proves QC: Leverage route guards to change the routing behavior based on the given state.`
`Source: content/05-react/client-auth-arc.md`; `demo/react-spa-demo/src/components/RequireAuth.tsx`

**[Should] How do you conditionally render?**

> A ternary when I am choosing between two pieces of UI — `{books.length > 0 ? <List /> : <p>No
> results.</p>}` — and the logical `&&` when it is show-something-or-nothing, like
> `{isNew && <span className="badge">New</span>}`. One sharp edge: `&&` with a number on the left renders
> the number, so `{count && <Badge/>}` puts a stray `0` on screen. I guard with a real boolean:
> `{count > 0 && <Badge/>}`. Anything more complex than that, I compute the element in a variable above
> the return.

`Proves QC: Conditionally render a component based on user interaction and/or state.`
`Source: content/05-react/rendering-lists-keys.md`

**[Should] What does TypeScript buy you in React?**

> Props become a compile-time-checked contract: pass the wrong type, misspell a prop, or omit a required
> one and it is an error at the call site rather than a blank screen at runtime. I get autocomplete on
> every prop and data field, and safe renames across the project. And the prop type doubles as the
> component's documentation — a new teammate reads the interface and knows how to call it without opening
> the implementation. The cost is small: `.tsx` files, annotated props, and occasionally a community
> types package. Vite's `react-ts` template sets it all up.

`Proves QC: Describe the benefits of TypeScript in React development.`
`Source: content/05-react/react-with-typescript.md`

**[Should] Write me a reusable component with type-checked props.**

> A named props interface above the component: a required field, an optional one marked with `?`, a
> union type to restrict values, and a typed callback. Something like
> `interface BookCardProps { item: InventoryItem; compact?: boolean; onSelect: (id: number) => void }`,
> then `function BookCard({ item, compact = false, onSelect }: BookCardProps)`. The default value goes in
> the destructuring. Now every call site is verified: forget `onSelect` and it will not compile, pass
> `size="medium"` when the union says compact or full and TypeScript rejects it. Children get typed as
> `React.ReactNode`.

`Proves QC: Build a reusable component using TSX with type-checked props.`
`Source: content/05-react/react-with-typescript.md`; `demo/react-spa-demo/src/components/BookCard.tsx`

**[Should] Two sibling components need the same data. What do you do?**

> Lift the state up to their closest common ancestor — no higher, so the fewest components re-render and
> the data stays as local as it can be. The parent owns the state and passes the value down to both
> children as props, plus a callback so a child can request a change. That keeps a single source of truth
> and both siblings consistent, and it is one-way data flow applied one level up: data down through
> props, change requests up through callbacks.

`Proves QC: Lift state up to a parent component to share data between child components.`
`Source: content/05-react/state-immutability-lifting.md`

**[Nice] What is `useRef` for, and how is it different from state?**

> `useRef` returns an object with a single mutable `.current` property that React preserves across every
> render — but reassigning `.current` does **not** cause a re-render. That makes it a box for values that
> must persist without driving the UI: a timer handle, a previous value, a click count I do not display.
> The other half is DOM refs: pass the ref to an element's `ref` attribute and after mount `.current` is
> the live node, for genuinely imperative jobs like focusing an input, measuring a box, or scrolling. If
> a value determines what the UI looks like, that is state, not a ref.

`Proves QC: Use refs to store information without triggering a re-render.`
`Source: content/05-react/hoc-refs.md`

**[Nice] What is a higher-order component, and would you still write one?**

> A function that takes a component and returns a new component wrapping it with extra behaviour — it is
> a pattern, not an API. `withAuth(Dashboard)` produces a component that renders Dashboard only when a
> user is signed in, and the same wrapper works on any page. The related split is container versus
> presentational: the container holds logic, state, and fetching; the presentational component just
> receives props and renders, which makes it trivially reusable and testable. For new code I would
> usually reach for a custom hook instead — no wrapper hell and no ambiguity about where injected props
> came from — but I still need to recognize HOCs, because plenty of existing code and libraries use them.

`Proves QC: Explain and implement higher-order and container components for reusable logic.`
`Source: content/05-react/hoc-refs.md`

**[Nice] Controlled versus uncontrolled components.**

> The question is who owns the input's value. Controlled means React state owns it: bind `value` and
> update state in `onChange`, so the value is always available for live validation, a disabled submit
> button, or as-you-type formatting. Uncontrolled means the DOM owns it: seed it with `defaultValue` and
> read it with a ref when you need it, usually at submit. Controlled costs a re-render per keystroke and
> a state variable per field; uncontrolled avoids both but hides the value from React between reads. The
> switch is literally `value` versus `defaultValue`, never both, and never flip a live input between the
> two. Controlled is the honest default — and a file input is always uncontrolled, because its value is
> read-only.

`Proves QC: Compare and implement controlled vs uncontrolled components in form handling.`
`Source: content/05-react/controlled-uncontrolled.md`

**[Nice] How would you test a React component?**

> With React Testing Library, on the principle of testing the software the way a user uses it. The loop
> is four beats: render the component, query with `screen` the way a person or screen reader would —
> `getByRole` first, then `getByLabelText`, then `getByText` — interact with `userEvent`, then assert
> with `expect` on what is on screen. To check a callback I render with a mock, click, and assert
> `toHaveBeenCalledTimes(1)`. Query families matter: `getBy` throws if missing so it asserts presence,
> `queryBy` returns null so it asserts absence, and `findBy` returns a promise for something async. What
> I never assert on is internal state, class names, or DOM structure — that is what makes a test survive
> a refactor. The API is identical under Jest and Vitest; only the mock factory and the globals import
> differ.

`Proves QC: Use Jest and a React testing library to test components.`
`Source: content/05-react/testing-jest-rtl.md`

**[Nice] Show me nested routing and a route parameter.**

> A dynamic segment is a colon in the path — `path="/books/:id"` — and inside the component I read it
> with `useParams`, remembering the value is always a string, so `Number(id)` if I need a number. For
> nesting, I put child routes inside a parent `Route` whose element renders an `<Outlet />` where the
> matched child appears, so shared chrome like a sidebar stays mounted while only the inner view changes.
> Child paths are relative to the parent, and an `index` route is what renders at the parent's own path.
> For navigating from code rather than a click, `useNavigate` gives me a function — `navigate("/books")`,
> or `navigate(-1)` for back.

`Proves QC: Leverage advanced routing techniques to create parent-child routing, or through passing
variables into routes.`
`Source: content/05-react/routing-react-router.md`

---

## Cross-cutting: the questions that span sections

**[Must+] Where should an auth token live on the client?**

> There are two real options and it is a genuine trade-off. `localStorage` is simple, survives a
> refresh, and my JavaScript can read it to set the header — but *any* script on the page can read it
> too, so a single XSS hole hands an attacker the token. An HttpOnly cookie cannot be read by script at
> all, which defangs token theft via XSS — but the browser attaches it automatically to cross-site
> requests, so CSRF becomes the concern and needs SameSite or anti-forgery tokens, plus server
> cooperation. Cookies are the more defensible default; localStorage is common and acceptable when the
> XSS surface is tightly controlled. I present it as a trade-off, not a rule.

`Proves QC: Handle a failed request when using the Fetch API.` (auth-adjacent depth; the row's note
home is `fetch-json-http.md`)
`Source: content/03-javascript/fetch-json-http.md`, `content/05-react/client-auth-arc.md`

**[Must+] You add a Bearer token and suddenly get a CORS error instead of data. What happened?**

> The custom `Authorization` header made the cross-origin request non-simple, so the browser sent an
> automatic `OPTIONS` preflight first, asking the server whether this origin may send that header and
> method. The server's CORS policy did not allow it, so the browser blocked the real request before it
> was ever sent — which is why I see a CORS error rather than a 401. The fix is on the server: its CORS
> configuration has to permit the calling origin and include `Authorization` among the allowed headers.

`Proves QC: Handle a failed request when using the Fetch API.` (CORS depth)
`Source: content/05-react/client-auth-arc.md`; `demo/react-spa-demo/src/api/client.ts`

**[Must+] Can the client verify a JWT? What is 401 versus 403?**

> The client can **decode** the payload — it is base64url — to shape the UI, like showing a name or
> hiding an admin button. It cannot **verify** it, because verification needs the server's signing
> secret, which the browser neither has nor should. So a decoded token is a hint for display, never
> proof for enforcement: someone can forge a role in devtools and flip a button on, and they get a 403
> the instant they touch a protected endpoint because the signature no longer matches. On the status
> pair: 401 means not authenticated — missing or invalid credentials; 403 means authenticated but not
> allowed — a valid token with insufficient role.

`Proves QC: Handle a failed request when using the Fetch API.` (status-code depth) /
`Leverage route guards to change the routing behavior based on the given state.`
`Source: content/05-react/client-auth-arc.md`, `content/03-javascript/fetch-json-http.md`
