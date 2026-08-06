# QC-5 (Front End) — Cheat Sheet

Dense quick reference. Skim the night before and the morning of. Everything here is drawn from
`weeklytechrepo/Frontend-React/content/**` and the demo threads; the study guide has the explanations.

---

## HTML

**Document skeleton**

```html
<!DOCTYPE html>          <!-- rendering-mode switch; omit it and you get quirks mode -->
<html lang="en">
  <head>                 <!-- metadata: nothing renders -->
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Library Catalog</title>
    <link rel="stylesheet" href="css/styles.css">
  </head>
  <body>                 <!-- everything visible -->
    <h1>Open Orders</h1>
    <script src="js/app.js" defer></script>
  </body>
</html>
```

**Tag families**

| Group | Tags |
|---|---|
| Semantic structure | `header` `nav` `main` `section` `article` `footer` |
| Text content | `h1`-`h6` `p` `ul` `ol` `li` |
| Hypertext / media | `a href` `img src alt` |
| Generic | `div` (block) `span` (inline) — no meaning; the fallback |

**Void elements (no content, no closing tag):** `img` `br` `hr` `input` `link` `meta`

**Element anatomy:** tag = the bracketed marker; attribute = `name="value"` inside the opening tag;
element = opening tag + attributes + content + closing tag.

**Global attributes:** `id` (unique per page) - `class` (reusable) - `style` (inline one-off) -
`data-*` (read via `element.dataset`)

**Inline vs block**

| | Block | Inline |
|---|---|---|
| Flow | new line, stacks | within the line |
| Width | fills parent | hugs content |
| width/height | accepted | **ignored** |
| Examples | `div` `p` `h1` `ul` `form` | `span` `a` `img` `strong` `label` |

`inline-block` = flows inline, accepts dimensions.

**Script placement**

| Placement | Behavior |
|---|---|
| head, plain | blocks parsing — almost never |
| end of body | runs after the HTML above it |
| head + `defer` | parallel download, runs after parsing, in order — modern default |
| head + `async` | parallel download, runs on arrival, unordered |

**Forms**

```html
<form action="/members/register" method="post">
  <label for="email">Email</label>
  <input type="email" id="email" name="email" required>
  <button type="submit">Register</button>
</form>
```

- Only controls with a **`name`** are submitted. `id` is for labels and scripts.
- GET = pairs in the URL (bookmarkable reads). POST = body (state changes, secrets).
- Types: `text` `password` `email` `number` `checkbox` `radio` `date` `file`; plus `select`/`option`,
  `textarea`, `button` (default type in a form is `submit`).
- **Radios group by a shared `name`.** Checkboxes each need their own.
- Validation attributes `required` `min` `max` `minlength` `maxlength` `pattern` = **UX only**; the
  server revalidates.

---

## CSS

**Rule anatomy**

```css
p {                  /* selector          */
  color: red;        /* declaration       */
  font-size: 16px;   /* property: value;  */
}                    /* declaration block */
```

**Three ways to attach:** inline `style=""` - internal `<style>` in head - external `<link
rel="stylesheet">` (production default: maintainable, cached, separated).

**Selectors**

```css
p        { }   /* tag      */      p.card  { }   /* compound  */
.card    { }   /* class    */      h1, h2  { }   /* grouping  */
#nav     { }   /* id       */      input[type="email"] { }   /* attribute */
a:hover  { }   /* pseudo-class: state or position (:focus, :first-child, :nth-child(even)) */
```

**Combinators**

| Syntax | Selects |
|---|---|
| `div p` | any `p` descendant, any depth |
| `div > p` | direct children only |
| `h1 + p` | the single immediately-next sibling |
| `h1 ~ p` | all later siblings sharing the parent |

**Priority**

1. Inline `style=""` beats stylesheet rules.
2. Specificity `(ids, classes, elements)`, compared left to right — `(1,0,0)` beats `(0,99,0)`.
   Pseudo-classes and attribute selectors count as classes; `*` and combinators count as nothing.
3. Tie -> source order, later wins.
4. `!important` trumps everything — avoid.

**Box model** (inside out): content -> padding -> border -> margin. Background paints content + padding;
margin is always transparent.

```css
.card { width: 300px; padding: 20px; border: 5px solid; }
/* content-box (default) -> renders 350px | border-box -> renders 300px */
*, *::before, *::after { box-sizing: border-box; }   /* the universal reset */
```

**Margin collapsing:** adjacent vertical margins merge to the larger, not the sum.

**Property families:** text (`color` `font-size` `font-family` `font-weight` `text-align`
`line-height`) - box (`width` `height` `padding` `margin` `border` `border-radius` `box-shadow`) -
background - `display` - `position` - `overflow` - `z-index`.

**Position:** `static` (default) - `relative` (nudge + anchor for absolute children) - `absolute` (out of
flow, pinned to nearest non-static ancestor) - `fixed` (viewport) - `sticky` (in flow, then pins).

**`display: none`** removes from layout; **`visibility: hidden`** keeps the space.

**Units:** `px` (hairlines) - `%` (parent) - `em` (own font size) - `rem` (root — default for type and
spacing, respects the user's font-size preference) - `vh`/`vw` (viewport).

**Responsive**

```css
/* mobile-first: base = phone, min-width queries layer complexity upward */
.cards { display: grid; grid-template-columns: 1fr; }
@media (min-width: 600px)  { .cards { grid-template-columns: 1fr 1fr; } }
@media (min-width: 1024px) { .cards { grid-template-columns: repeat(4, 1fr); } }
@media screen and (min-width: 600px) and (max-width: 1023px) { }
@media (prefers-color-scheme: dark) { }
```

Requires the viewport meta tag or phones lay out at a fake ~980px width.

**Variables and motion**

```css
:root { --brand: #336; }
.button { background: var(--brand, #ccc); }     /* second arg = fallback */
.card { transition: transform 200ms ease-out; }  /* state change */
@keyframes pulse { 0%,100% { opacity: 1 } 50% { opacity: .4 } }   /* autonomous */
```

Animate `transform` and `opacity` (GPU composited), not `width`/`top`/`margin` (layout every frame).

---

## JavaScript language

**Declarations**

```js
const taxRate = 0.07;   // default choice; freezes the BINDING, not the value
let subtotal = 0;       // reassignable
var legacy = "avoid";   // function-scoped, hoisted - recognize, do not write
```

**Scope:** global - function (`var`, parameters) - block (`let`/`const`, nearest `{ }`).

```js
for (var i = 0; i < 3; i++) setTimeout(() => console.log(i));  // 3, 3, 3
for (let j = 0; j < 3; j++) setTimeout(() => console.log(j));  // 0, 1, 2
```

**Types:** 7 primitives — `string` `number` `boolean` `null` `undefined` `symbol` `bigint`; everything
else is an object (arrays, functions included).

| Expression | Result |
|---|---|
| `typeof null` | `"object"` (historic bug — memorize) |
| `typeof [1,2]` | `"object"` — use `Array.isArray()` |
| `typeof function(){}` | `"function"` |

**Coercion**

```js
"5" + 1    // "51"   string wins with +
"5" - 1    // 4      other operators force numbers
"5" == 5   // true   == coerces
"5" === 5  // false  === compares type and value
NaN === NaN // false — use Number.isNaN(x)
```

**Falsy (exactly six):** `false` `0` `""` `null` `undefined` `NaN`. Everything else truthy — including
`"0"`, `[]`, `{}`.

```js
const name = input || "guest";   // replaces a legitimate 0 or ""
const qty  = input ?? 0;         // ?? falls back only on null/undefined
```

**Hoisting:** `var` -> `undefined`; function declarations -> fully usable; `let`/`const` -> temporal dead
zone, access throws `ReferenceError`.

**Objects**

```js
const book = { title: "Dune", author: { name: "Herbert" }, [key]: "978-0441172719" };
book.title;  book["isbn"];                      // dot = known key, bracket = dynamic
const { title: t, author } = book;              // destructuring (with rename)
const copy = { ...book, title: "New" };         // spread = SHALLOW copy + override
```

**Array methods**

| Method | Returns | Mutates |
|---|---|---|
| `map` `filter` `reduce` `find` `slice` `concat` | new value | no |
| `forEach` | `undefined` | no |
| `some` `every` `includes` | boolean | no |
| `push` `pop` `shift` `unshift` `splice` `sort` `reverse` | varies | **yes** |

`slice` copies out, `splice` edits in place. `[10,2].sort()` -> `[10,2]` (string compare) — pass
`(a,b) => a-b`.

**Loops**

```js
for (let i = 0; i < a.length; i++) {}   // index math, break, fastest
for (const v of a) {}                   // VALUES - default for arrays
for (const k in obj) {}                 // KEYS - objects, not arrays
a.forEach((v, i) => {});                // cannot break
while (queue.length) {}                 // do-while runs at least once
```

**Function forms**

| Form | Syntax | Note |
|---|---|---|
| Declaration | `function add(a,b){}` | hoists fully |
| Expression | `const add = function(a,b){}` | not usable before its line |
| Arrow | `const add = (a,b) => a+b` | lexical `this` |
| Anonymous | `setTimeout(function(){}, 500)` | inline argument |
| Method | `const o = { total(){} }` | on an object |

```js
const double = n => n * 2;          // one param: parens optional, implicit return
const make = () => ({ id: 1 });     // object literal needs wrapping parens
function f(x = 10, ...rest) {}      // default + rest parameters
(function(){ })();                  // IIFE
```

**`this` by call site:** method call -> object before the dot | `new` -> the instance | plain call ->
`undefined` (strict) | arrow -> lexical, from the surrounding scope. `call`/`apply`/`bind` set it.

**Closure**

```js
function makeCounter() {
  let count = 0;                     // survives the return...
  return { increment: () => ++count, current: () => count };  // ...via these closures
}
```

**Errors**

```js
try { risky(); }
catch (e) { console.error(e.message); }   // message, name, stack
finally { cleanup(); }                    // runs on BOTH paths, even past return

throw new Error("not a price");           // never throw a string - no stack, no shape
class NotFoundError extends Error {}      // instanceof for typed catching
```

Built-ins: `TypeError` (wrong type) - `RangeError` (out of range) - `SyntaxError` (unparseable) -
`ReferenceError` (undeclared).

**Prototype chain:** failed lookup delegates up the prototype until `null`. `class B extends A` is sugar;
methods live on `B.prototype`, instances delegate to it. `prototype` on constructors, `__proto__`
(legacy) on instances.

**Node and npm**

```
node app.js                         # V8 outside the browser: fs, process; NO document/window
npm init -y                         # create package.json
npm install axios                   # dependencies (runtime)
npm install --save-dev typescript   # devDependencies (build/test)
npx tsc                             # run a package CLI without a global install
```

`^18.3.1` = minor + patch | `~5.4.0` = patch only | `18.3.1` = exact. `node_modules` is generated and
git-ignored.

---

## Browser JavaScript

**Query**

```js
document.getElementById("nav");            // one element, fastest
document.querySelector(".card");           // first CSS-selector match
document.querySelectorAll("ul.books li");  // ALL - static NodeList (supports forEach)
```

`getElementsByClassName`/`TagName` return a **live** HTMLCollection.

**Read / modify / insert**

```js
el.textContent = data;                   // inert text - SAFE default
el.innerHTML = "<b>markup</b>";          // parsed as HTML - XSS risk with untrusted data
el.setAttribute("href", url);
el.classList.add("done"); el.classList.remove("done"); el.classList.toggle("done");
el.style.backgroundColor = "gold";       // camelCase

const li = document.createElement("li");
li.textContent = `${book.title} - ${book.author}`;   // template literal
list.append(li);                         // appendChild takes exactly one node
list.firstElementChild.remove();
```

**Events**

```js
btn.addEventListener("click", handleSave);        // stacks; options: { once, passive, capture }
btn.removeEventListener("click", handleSave);     // needs the SAME reference
```

| Event | Fires when |
|---|---|
| `click` | pointer press+release |
| `submit` | form submitted (listen on the **form**) |
| `input` | every keystroke |
| `change` | value committed (blur / selection) |
| `keydown` | key down, repeats while held |
| `DOMContentLoaded` | HTML parsed, DOM queryable |
| `load` | page **and** all assets |

- `event.target` = origin | `event.currentTarget` = where the listener sits
- `preventDefault()` cancels the default action | `stopPropagation()` halts travel — independent
- Capture down, **bubble up**; listeners default to bubbling
- **Delegation:** one container listener + `event.target.closest(".row")` — covers future rows

**Promises**

```js
fetchCatalog()
  .then(books => render(books))
  .catch(err => showError(err))
  .finally(() => hideSpinner());
```

States: pending -> fulfilled / rejected; **settled once, never changes**.

| Combinator | Resolves with | Rejects |
|---|---|---|
| `Promise.all` | all values, in order | on the first rejection |
| `Promise.allSettled` | `{status, value/reason}[]` | never |
| `Promise.race` | first to settle | if that one rejected |
| `Promise.any` | first to fulfill | only if all reject |

**async/await**

```js
async function load() {
  try {
    const book = await fetchBook(1);                  // sequential ONLY if dependent
    const [a, b] = await Promise.all([f1(), f2()]);   // independent -> max, not sum
  } catch (err) { showError(err); }
}
```

An `async` function always returns a Promise; `await` pauses only that function.

**Event loop order:** sync code -> **microtasks** (promise callbacks) -> **macrotasks**
(`setTimeout`, events).

```js
console.log("one");
setTimeout(() => console.log("four"), 0);
Promise.resolve().then(() => console.log("three"));
console.log("two");
// one, two, three, four
```

**JSON**

```js
JSON.stringify(obj)   // object -> text; DROPS functions and undefined
JSON.parse(text)      // text -> object
```

Rules: double-quoted keys, no trailing commas, no comments, no functions/`undefined`.

**Fetch**

```js
const res = await fetch(url);                       // Promise -> Response
if (!res.ok) throw new Error(`HTTP ${res.status}`); // 4xx/5xx RESOLVE - check yourself
const data = await res.json();                      // Promise -> parsed body

await fetch(url, {
  method: "POST",
  headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
  body: JSON.stringify({ title: "Emma" }),
});
```

**Six steps:** build URL -> build options -> `await fetch` -> check `ok` -> parse -> use.
**Two failure lanes:** `catch` = unreachable (network/CORS); `!res.ok` = refused.

**Fetch vs XHR:** XHR is the older callback object (`open`/`onload`/`send`); fetch is promise-based and
cleaner. Neither treats an HTTP error status as a failure.

**CORS:** the browser blocks cross-origin reads unless the server sends
`Access-Control-Allow-Origin`. **401** = not authenticated; **403** = authenticated, not allowed.

**Token storage:** `localStorage` = simple, readable by any script (XSS steals it), not auto-sent (CSRF
moot). **HttpOnly cookie** = invisible to JS (XSS-safe), auto-sent (CSRF concern, mitigate with
`SameSite`). A real trade-off, not one right answer.

---

## TypeScript

**The one-liner:** JavaScript + a static type layer checked at compile time and **erased** on transpile.
No TypeScript runtime; types cannot check anything at runtime.

**Transpile and run**

```
npm install --save-dev typescript
npx tsc --init          # generates tsconfig.json
npx tsc                 # config-driven whole-project compile
node dist/app.js        # run the emitted JS
npx ts-node app.ts      # one-step alternative
```

Type errors **do not block emit** by default (`noEmitOnError` changes that).

**Basic and special types**

```ts
let age: number = 34;  let title: string = "Dune";  let done: boolean = false;
let nums: number[] = [1, 2];   // same as Array<number>
```

| Type | Meaning |
|---|---|
| `any` | opts out of checking — and it **spreads** |
| `unknown` | accepts anything, blocks every use until narrowed |
| `void` | returns nothing useful |
| `never` | cannot exist — powers exhaustiveness checks |

**Object types**

```ts
let member: { id: number; email: string; nickname?: string; readonly joined: string };
```

`?` = optional (read type gains `| undefined`); `readonly` = assign at declaration/constructor only.

**User-defined types**

```ts
interface User { id: number; name: string; }     // object contracts, extends, declaration merging
type Point = { x: number; y: number };           // ...and ANY type:
type ID = string | number;                       // union
type Comparator = (a: ID, b: ID) => number;      // function type
type Employee = Person & Payroll;                // intersection
```

Default: **interface for public object contracts, type for everything else.**
**Structural typing:** shape is identity — identical shapes are interchangeable (unlike nominal C#/Java).

**Unions:** before narrowing, only members common to **every** arm are legal.

```ts
type OrderStatus = "pending" | "shipped" | "delivered";   // literal union = closed value set
```

**Casting (assertions)**

```ts
const input = document.getElementById("email") as HTMLInputElement;
// <HTMLInputElement>x is the older spelling - collides with JSX, use `as`
// value as unknown as T = the escape hatch, and a smell
```

Converts **nothing** at runtime — a wrong assertion compiles clean and crashes later.

**Type guards**

```ts
if (typeof id === "string") { }         // primitives
if (when instanceof Date) { }           // class instances
if ("message" in err) { }               // property presence
if (value) { }                          // strips null/undefined

function isBook(x: unknown): x is Book { ... }   // type predicate - reusable guard

type Shape = { kind: "circle"; radius: number } | { kind: "rect"; w: number; h: number };
switch (s.kind) {
  case "circle": return Math.PI * s.radius ** 2;
  default: const _exhaustive: never = s; return _exhaustive;   // new arm -> compile error
}
```

**Assertion = "trust me now" (risk moves to runtime). Guard = "prove it" (compiler rewards you with
narrowing).**

**as const**

```ts
const ROLES = ["admin", "user", "guest"] as const;
type Role = typeof ROLES[number];    // "admin" | "user" | "guest" - derived, cannot drift
```

**tsconfig.json**

```json
{
  "compilerOptions": {
    "target": "es2020", "module": "commonjs",
    "rootDir": "./src", "outDir": "./dist",
    "strict": true, "sourceMap": true, "esModuleInterop": true
  },
  "include": ["src/**/*"], "exclude": ["node_modules"]
}
```

`strict` is an umbrella: **`strictNullChecks`** (null/undefined not assignable to everything) +
**`noImplicitAny`** (no silently-`any` parameters), plus `strictFunctionTypes`,
`strictPropertyInitialization`, `alwaysStrict`. **error TS5011** = `outDir` needs an explicit `rootDir`.

**Generics**

```ts
function first<T>(arr: T[]): T { return arr[0]; }          // preserves the caller's type
function byId<T extends { id: number }>(items: T[], id: number) { }  // constraint
interface Repository<T> { getById(id: number): T | undefined; }
// Read inside-out: Promise<Map<string, Order[]>> = promise of a map from strings to Order arrays
```

**Classes**

```ts
class Order implements Shippable {
  readonly id: number;
  protected status = "pending";
  constructor(id: number, private items: string[]) { this.id = id; }   // parameter property
}
```

TS `private` is compile-time only (erased); JS `#field` is runtime-enforced.

---

## React

**Component + JSX**

```tsx
function Greeting({ name }: { name: string }) {   // capitalized = component
  return <h1>Hello, {name}</h1>;                  // { } holds an EXPRESSION, not a statement
}
// compiles to React.createElement("h1", null, "Hello, ", name)
```

`className` not `class`, `htmlFor` not `for`, every tag closes, one root (or a Fragment `<>...</>`).

**Vite**

```
npm create vite@latest my-app -- --template react      # react-ts for TypeScript
cd my-app && npm install && npm run dev                # http://localhost:5173
npm run build   # dist/     npm run preview   # serve the build
```

**SPA vs MPA**

| | MPA | SPA |
|---|---|---|
| Navigation | new HTML document per click | JS swaps views in place |
| Data | server-rendered pages | JSON over API calls |
| Strength | SEO, fast first paint | instant navigation, state persists |
| Weakness | full reloads, lost state | heavy first load, SEO needs work |

**Virtual DOM:** cheap in-memory JS tree -> on state change React **renders** a new tree, **diffs
(reconciles)** it against the old, and **commits** only the differences to the real DOM. Keys give list
items stable identity for the diff.

**State and props**

```tsx
const [count, setCount] = useState(0);      // initial value used on first render only
setCount(prev => prev + 1);                 // functional updater when derived from previous
```

Props are read-only inputs; state is the component's own memory. Never assign — always call the setter.

**Hooks rules:** top level only (never in loops, conditions, nested functions); only from components or
other hooks.

**Lifecycle with useEffect**

```tsx
useEffect(() => { /* every render */ });
useEffect(() => { /* mount only */ }, []);
useEffect(() => { /* mount + when dep changes */ }, [dep]);
useEffect(() => {
  const id = setInterval(tick, 1000);
  return () => clearInterval(id);    // cleanup: before next run AND on unmount
}, []);
```

Mount -> update -> unmount. No array + setting state = infinite loop.

**Immutability**

```tsx
setBooks([...books, newBook]);                                   // add
setBooks(books.filter(b => b.id !== id));                        // remove
setBooks(books.map(b => b.id === id ? { ...b, available: false } : b));  // update
setProfile({ ...profile, address: { ...profile.address, city: "Oxford" } }); // nested
```

React compares **references**. `push`/`splice`/`sort` and `Object.assign(state, patch)` break re-rendering.

**Lists, keys, conditional rendering**

```tsx
{books.map(b => <BookRow key={b.id} book={b} />)}     // stable ID, never the index
{books.length > 0 ? <List /> : <p>No results.</p>}    // two branches
{isNew && <span className="badge">New</span>}         // show or nothing
{count > 0 && <Badge />}                              // guard numbers - `count &&` renders 0
```

**Events and forms**

```tsx
<button onClick={handleClick}>Like</button>          {/* pass the function, not handle() */}
<button onClick={() => remove(id)}>Remove</button>   {/* arrow to pass arguments */}

<input value={title} onChange={e => setTitle(e.target.value)} />   {/* CONTROLLED */}
<input ref={inputRef} defaultValue="" />                           {/* UNCONTROLLED */}

function handleChange(e) {                            // one handler, many fields
  const { name, value, type, checked } = e.target;
  setForm(prev => ({ ...prev, [name]: type === "checkbox" ? checked : value }));
}

<form onSubmit={e => { e.preventDefault(); /* ... */ }}>   {/* onSubmit on the FORM */}
```

| | Controlled | Uncontrolled |
|---|---|---|
| Source of truth | React state | the DOM node |
| Initial value | `value` | `defaultValue` |
| Read it | already in state | `ref.current.value` |
| Re-render per keystroke | yes | no |
| Live validation | easy | awkward |

`<input type="file">` is always uncontrolled.

**Styling**

```tsx
<span style={{ color: "green", fontWeight: 600 }} />     // inline: object, camelCase, double braces
import styles from "./BookCard.module.css";              // CSS Modules: scoped, collision-free
<div className={styles.card} />
import "./index.css";                                    // global stylesheet
```

**Axios / Fetch**

```ts
export const api = axios.create({ baseURL: "http://localhost:5137" });
api.interceptors.request.use(config => {
  const token = getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
const res = await api.get<Book[]>("/books");   // parsed body on res.data
```

Axios rejects on HTTP error statuses and parses JSON; fetch resolves on 4xx/5xx and needs `res.ok` +
`.json()`. Model every request as **loading / error / data**, clear loading in `finally`, guard with an
`active` flag against setting state after unmount.

**React Router**

```tsx
<BrowserRouter>
  <Routes>
    <Route path="/" element={<Home />} />
    <Route path="/books/:id" element={<BookDetail />} />
    <Route path="/books" element={<BooksLayout />}>
      <Route index element={<BookList />} />      {/* default child at /books */}
      <Route path="new" element={<AddBook />} />  {/* relative path */}
    </Route>
    <Route path="*" element={<NotFound />} />
  </Routes>
</BrowserRouter>

<Link to="/books">Catalog</Link>            {/* never a plain <a href> in-app */}
const { id } = useParams();                  // always a string
const navigate = useNavigate(); navigate("/books"); navigate(-1);
<Outlet />                                   // where the matched child renders
```

**Context**

```tsx
const AuthContext = createContext<AuthContextValue | null>(null);   // arg = default, no-Provider case

function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<Identity | null>(null);
  return <AuthContext.Provider value={{ user, login, logout }}>{children}</AuthContext.Provider>;
}

function useAuth() {                          // custom hook with a null-guard
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}
```

Every consumer re-renders on every value change — keep fast-changing state out.

**useReducer**

```tsx
type Action = { type: "login_start" } | { type: "login_success"; user: Identity }
            | { type: "login_failure"; error: string } | { type: "logout" };

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case "login_success": return { status: "authenticated", user: action.user, error: null };
    // ...every case returns a NEW object
  }
}
const [state, dispatch] = useReducer(reducer, initialState);
dispatch({ type: "login_start" });
```

Reach for it when several fields must change together or the state is a machine with named transitions.

**Route guard**

```tsx
function RequireAuth({ children, role }: { children: ReactNode; role?: string }) {
  const { status, user } = useAuth();
  if (status !== "authenticated") return <Navigate to="/login" replace />;
  if (role && user?.role !== role) return <Navigate to="/" replace />;
  return <>{children}</>;
}
```

Guards and hidden buttons are **UX, not security** — the server authorizes every request.

**TypeScript in React**

```tsx
interface BookCardProps {
  book: Book;
  compact?: boolean;                    // optional
  size?: "compact" | "full";            // restricted values
  onSelect: (id: number) => void;       // typed callback
  children?: React.ReactNode;           // anything renderable
}
function BookCard({ book, compact = false, onSelect }: BookCardProps) { }
```

**Refs and HOCs (Nice tier)**

```tsx
const clicks = useRef(0);          // { current: 0 }; changing .current does NOT re-render
const inputRef = useRef<HTMLInputElement>(null);
inputRef.current?.focus();         // DOM ref: imperative jobs only

const ProtectedDashboard = withAuth(Dashboard);   // HOC: component in, wrapped component out
```

**Testing (Nice tier)**

```tsx
render(<CheckoutButton onCheckout={handler} />);
await userEvent.click(screen.getByRole("button", { name: "Borrow" }));
expect(handler).toHaveBeenCalledTimes(1);
expect(screen.getByText("Hunt & Thomas")).toBeInTheDocument();
```

`getBy*` throws (presence) - `queryBy*` returns null (absence) - `findBy*` awaits (async). Prefer
`getByRole`, then `getByLabelText`, then `getByText`. RTL is identical under Jest and Vitest; only
`jest.fn()` vs `vi.fn()` and the globals import differ. Test behavior, never internal state or class
names.
