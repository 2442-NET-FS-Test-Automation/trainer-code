# QC 5 (Front End) Criteria

## HTML/CSS

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Describe what HTML is. | HyperText Markup Language — the standard markup language that defines the structure and content of web pages using elements/tags. |
| Must know | Describe the structure of an HTML document and what is included in the different sections. | `<!DOCTYPE html>` followed by `<html>` wrapping `<head>` (metadata, title, stylesheet/script links) and `<body>` (visible content). |
| Must know | List common HTML tags and describe why they are different from divs. | Semantic tags (`<header>`, `<nav>`, `<main>`, `<footer>`, `<p>`, `<h1>`) convey meaning to browsers and screen readers; `<div>` is a generic container with no semantics. |
| Must know | Describe how/where you link an external CSS sheet into an HTML document. | `<link rel="stylesheet" href="styles.css">` inside the `<head>`. |
| Must know | Describe how/where you link an external JS file into an HTML document. | `<script src="app.js"></script>` — at the end of `<body>`, or in `<head>` with the `defer` attribute. |
| Must know | Describe the structure of a CSS style rule. | A selector plus a declaration block of property/value pairs: `p { color: red; font-size: 16px; }` |
| Must know | Explain the CSS box model. | Every element renders as a box: content, then padding, then border, then margin, from the inside out. |
| Must know | Describe the different ways to add styling to an HTML document. | Inline (`style=""` attribute), internal (`<style>` block in `<head>`), and external (linked stylesheet). |
| Must know | Use the correct syntax for styling different elements such as by tag, class, id, etc. | `p { }` (tag), `.card { }` (class), `#nav { }` (id), `p.card { }` (combined). |
| Must know | Describe CSS priority in regards to inline, internal, and external styles. | Specificity and source order decide: inline styles beat internal/external rules; between rules, the more specific (or later) one wins. |
| Should know | Construct an HTML form. | `<form action="/submit" method="post"><input name="email"><button>Send</button></form>` |
| Should know | Take in user input using a variety of input tags (text, checkbox, etc). | `<input type="text">`, `<input type="checkbox">`, `<input type="radio">`, `<select>`, `<textarea>`. |
| Nice to Have | Describe the benefits of combinators and how to use them. | Target elements by relationship: `div > p` (child), `div p` (descendant), `h1 + p` (adjacent sibling), `h1 ~ p` (general sibling). |
| Nice to Have | Make responsive webpages using CSS. | Media queries plus fluid layout: `@media (max-width: 600px) { .nav { display: none; } }`, flexbox/grid, relative units. |

## JS Language

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Describe what JS is. | A dynamically typed, interpreted scripting language that runs in the browser (and in Node.js) to make pages interactive. |
| Must know | Describe what type coercion is. | Automatic conversion between types during operations: `"5" + 1` is `"51"`, but `"5" - 1` is `4`. |
| Must know | Describe what truthy/falsy is. | How values coerce to boolean in conditions; falsy values are `false`, `0`, `""`, `null`, `undefined`, `NaN` — everything else is truthy. |
| Must know | Describe the different variable scopes in JS. | Global, function, and block scope; `let`/`const` are block-scoped, `var` is function-scoped. |
| Must know | Explain the different data types in JS. | Primitives (string, number, boolean, null, undefined, symbol, bigint) plus objects (including arrays and functions). |
| Must know | Create variables in JS. | `let count = 0; const name = "Alice";` |
| Must know | Create objects in JS. | `const user = { name: "Alice", age: 30 };` — or via classes, `new Object()`, `Object.create()`. |
| Must know | Handle errors in JS. | `try { risky(); } catch (e) { console.error(e); } finally { cleanup(); }` |
| Must know | Create arrays in JS. | `const nums = [1, 2, 3];` |
| Must know | Describe the different array methods and how to use them. | `map`, `filter`, `reduce`, `find`, `forEach`, `push`/`pop`, `slice`/`splice`: `nums.filter(n => n > 1).map(n => n * 2)` |
| Must know | Loop through arrays. | `for`, `for...of`, or `forEach`: `for (const n of nums) { console.log(n); }` |
| Must know | Describe the different types of functions in JS. | Function declarations, function expressions, arrow functions, anonymous functions, and methods on objects. |
| Should know | Use template literals. | `` `Hello, ${name}!` `` |
| Should know | Describe what the this keyword is. | A reference to the calling context: the object before the dot, the new instance in a constructor, and lexically inherited inside arrow functions. |
| Should know | Explain the role of callbacks in JavaScript programming. | A function passed into another function to be invoked later: `setTimeout(() => console.log("done"), 1000);` |
| Should know | Define arrow functions and explain the benefits of using arrow functions. | `(a, b) => a + b` — shorter syntax and lexical `this` (no rebinding). |
| Should know | Create arrow functions. | `const square = x => x * x;` |
| Should know | Create anonymous functions. | `arr.map(function (x) { return x * 2; });` |
| Nice to Have | Describe and explain a closure. | A function that retains access to the variables of its defining scope after that scope has returned — e.g. a counter factory with private state. |
| Nice to Have | Describe what function and variable hoisting is. | Declarations move to the top of their scope before execution: `var` hoists as `undefined`, function declarations hoist fully, `let`/`const` sit in the temporal dead zone. |
| Nice to Have | Describe how inheritance works in JS. | Through the prototype chain: objects delegate lookups to their prototype; `class Dog extends Animal` is syntax sugar over prototypal inheritance. |

## Browser Based JS

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Describe what the DOM is. | The Document Object Model — the in-memory tree representation of the page that JS can read and modify. |
| Must know | Query the DOM for elements. | `document.querySelector(".card")`, `document.getElementById("nav")`, `document.querySelectorAll("li")`. |
| Must know | Describe what event listeners are. | Callbacks registered to run when an event fires on an element: `btn.addEventListener("click", handler);` |
| Must know | Insert new elements into the DOM. | `const li = document.createElement("li"); li.textContent = "New"; list.appendChild(li);` |
| Must know | Explain what a JavaScript Promise is and when it is used to handle asynchronous operations. | An object representing the eventual result of an async operation (pending, fulfilled, or rejected), consumed via `.then`/`.catch` or `await`. |
| Must know | Describe what type of object the Fetch API returns. | A `Promise` that resolves to a `Response` object; `response.json()` returns another Promise for the parsed body. |
| Must know | Explain what JSON is. | JavaScript Object Notation — a language-independent text format for data exchange: `{ "name": "Alice", "age": 30 }` |
| Must know | Handle a failed request when using the Fetch API. | Check `response.ok` (fetch only rejects on network failure) and chain `.catch`: `if (!res.ok) throw new Error(res.status);` |
| Must know | Describe the different promise methods. | Instance: `.then`, `.catch`, `.finally`; static combinators: `Promise.all`, `Promise.race`, `Promise.allSettled`, `Promise.any`. |
| Must know | Explain the difference between synchronous and asynchronous programming. | Synchronous code blocks until each step completes; asynchronous code schedules work (timers, I/O) and handles results later via callbacks or Promises. |
| Should know | List the steps to sending an HTTP request using the Fetch API. | `fetch(url, { method: "POST", headers, body }).then(res => res.json()).then(data => render(data));` |
| Should know | Describe what async/await is and how they compare to using .then(). | Syntax sugar over Promises that lets async code read sequentially: `const res = await fetch(url);` instead of chained `.then()` calls. |
| Should know | Explain what JSON.stringify() and JSON.parse() are. | `JSON.stringify(obj)` serializes an object to a JSON string; `JSON.parse(str)` turns a JSON string back into an object. |
| Nice to Have | Describe what bubbling and capturing are and their difference. | Events travel down the tree to the target (capturing) then back up (bubbling); listeners fire in the bubbling phase by default. |
| Nice to Have | Describe some methods on the event object and what they do. | `preventDefault()` stops the default action, `stopPropagation()` halts bubbling, `event.target` identifies the originating element. |
| Nice to Have | Explain how to chain multiple asynchronous operations using Promises or async/await. | `const user = await getUser(); const orders = await getOrders(user.id);` — or `.then()` chains that return the next Promise. |
| Nice to Have | Implement error handling using try-catch blocks with async/await. | `try { const res = await fetch(url); } catch (e) { console.error("Request failed", e); }` |
| Nice to Have | Describe and explain the event loop. | JS runs a single call stack; when it empties, the loop pulls queued async callbacks (microtasks before macrotasks) onto the stack. |
| Nice to Have | Describe the difference between Fetch and XHR. | Fetch is Promise-based with a cleaner API and does not reject on HTTP error statuses; XHR is the older event/callback-based object. |

## TypeScript

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Compare/contrast TypeScript to JavaScript. | TS is a superset of JS that adds static types checked at compile time; the types are erased when transpiled to plain JS. |
| Must know | Describe and implement basic types in TypeScript. | `let age: number = 30; let name: string = "Alice"; let done: boolean = false;` |
| Must know | Implement user defined types in TypeScript. | `interface User { id: number; name: string; }` or `type Point = { x: number; y: number };` |
| Must know | Describe and implement casting in TypeScript. | Type assertions: `const input = document.getElementById("email") as HTMLInputElement;` |
| Must know | Describe and demonstrate the process to transpile and run TypeScript. | `tsc app.ts` emits `app.js`, then `node app.js` (or `npx ts-node app.ts` in one step). |
| Should know | Implement TypeScript outside of Angular/React environments using plain .ts files. | `tsc --init`, write `.ts` files, compile with `tsc`, run the emitted JS with node. |
| Should know | Describe the purpose of the "strict" flag in the tsconfig.json file. | Enables the strict family of checks (`strictNullChecks`, `noImplicitAny`, etc.) for maximum type safety. |
| Should know | Describe and implement union types. | `let id: string \| number;` |
| Should know | Describe and implement type guards. | Narrow a union before use: `if (typeof id === "string") { id.toUpperCase(); }` |
| Should know | Describe and implement type aliasing. | `type ID = string \| number;` |
| Nice to Have | Configure the TypeScript compiler using options in the tsconfig.json based on project needs. | Set `target`, `module`, `outDir`, `rootDir`, `strict`, etc. in `tsconfig.json`. |
| Nice to Have | Describe and leverage generic types. | `function first<T>(arr: T[]): T { return arr[0]; }` |

## React

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Describe and implement functional components in React. | `function Greeting({ name }) { return <h1>Hello, {name}</h1>; }` |
| Must know | Explain the difference between Single Page Applications and Multi Page Applications. | An SPA loads one HTML shell and swaps views client-side; an MPA requests a new page from the server on every navigation. |
| Must know | Utilize and explain common React hooks: useState, useEffect, and useContext. | `const [count, setCount] = useState(0); useEffect(() => { fetchData(); }, []); const theme = useContext(ThemeContext);` |
| Must know | Pass props to components and manage local component state. | Parent renders `<Card title="Hi" />`; child reads `props.title`; local state lives in `useState`. |
| Must know | Create and run a React application using Vite CLI. | `npm create vite@latest my-app -- --template react`, then `npm install` and `npm run dev`. |
| Must know | Explain the lifecycle of a React component. | Mount, update (on state/prop change), unmount — modeled in function components with `useEffect` and its cleanup return. |
| Must know | Describe how the React Virtual DOM works and how it improves performance. | React diffs an in-memory tree against the previous render and applies only the minimal set of real-DOM updates. |
| Must know | Make HTTP requests using Axios or Fetch and handle the response. | `useEffect(() => { axios.get("/api/users").then(res => setUsers(res.data)); }, []);` |
| Must know | Write and explain JSX syntax and how it integrates with JavaScript. | HTML-like syntax compiled to `React.createElement` calls; embed JS expressions with `{ }`: `<li>{user.name}</li>` |
| Must know | Use useReducer for complex state management scenarios. | `const [state, dispatch] = useReducer(reducer, initialState); dispatch({ type: "add", payload: item });` |
| Must know | Explain and apply the principles of state immutability in React. | Never mutate state in place; produce new objects/arrays so React detects the change: `setItems([...items, newItem]);` |
| Must know | Handle user input through form elements and manage form state. | Controlled inputs: `<input value={name} onChange={e => setName(e.target.value)} />` |
| Must know | Implement component communication through props and callbacks (Parent to Child & vice versa). | Parent passes data down as props and a callback (`onSave`) that the child invokes to send data back up. |
| Must know | Build and use nested component structures to model UI architecture. | Compose the UI as a tree: `<App><Nav /><Main><Card /></Main></App>` |
| Must know | Use React Router to implement navigation in a single-page application. | `<BrowserRouter><Routes><Route path="/users" element={<Users />} /></Routes></BrowserRouter>` |
| Must know | Apply styling to components using inline styles, CSS modules, or external stylesheets. | `style={{ color: "red" }}`, `import styles from "./Card.module.css"`, or a stylesheet applied via `className`. |
| Must know | Use Lists and Keys correctly to render dynamic components efficiently. | `items.map(item => <li key={item.id}>{item.name}</li>)` — stable keys let React reconcile without re-rendering every row. |
| Should know | Use Context.Provider tags to wrap components and distribute application state. | `<ThemeContext.Provider value={theme}><App /></ThemeContext.Provider>` |
| Should know | Route users between components through the use of BrowserRouter. | `<Link to="/about">About</Link>` navigating between routes declared under `<BrowserRouter>`. |
| Should know | Leverage route guards to change the routing behavior based on the given state. | Wrap protected routes: `element={token ? <Dashboard /> : <Navigate to="/login" />}` |
| Should know | Use a Reducer to manage a set of complex known states. | Centralize transitions in a reducer switch: `case "loading": ... case "success": ... case "error": ...` |
| Should know | Conditionally render a component based on user interaction and/or state. | `{isLoggedIn ? <Dashboard /> : <Login />}` or `{error && <Alert message={error} />}` |
| Should know | Describe the benefits of TypeScript in React development. | Type-checked props and state catch wiring mistakes at compile time and improve editor autocompletion/refactoring. |
| Should know | Leverage NPM libraries in a React project to add functionality. | `npm install axios` then `import axios from "axios";` |
| Should know | Lift state up to a parent component to share data between child components. | Move shared state to the closest common ancestor and pass it (plus setters) down to both children as props. |
| Should know | Describe how one-way data flow works in React. | Data flows parent-to-child via props; children request changes through callbacks — they never mutate parent state directly. |
| Should know | Build a reusable component using TSX with type-checked props. | `function Card({ title }: { title: string }) { return <h2>{title}</h2>; }` |
| Nice to Have | Use createContext and Context.Provider to manage global state. | `const AuthContext = createContext(null);` — wrap the app in its Provider and consume with `useContext(AuthContext)`. |
| Nice to Have | Use refs to store information without triggering a re-render. | `const idRef = useRef(0); idRef.current++; // updating .current does not re-render` |
| Nice to Have | Use Jest and a React testing library to test components. | `render(<Button />); fireEvent.click(screen.getByRole("button")); expect(handler).toHaveBeenCalled();` |
| Nice to Have | Leverage advanced routing techniques to create parent-child routing, or through passing variables into routes. | Nested routes render through `<Outlet />`; route params like `path="users/:id"` are read with `useParams()`. |
| Nice to Have | Explain and implement higher-order and container components for reusable logic. | An HOC wraps a component to add behavior (`withAuth(Profile)`); container components hold logic while presentational children render. |
| Nice to Have | Compare and implement controlled vs uncontrolled components in form handling. | Controlled: value driven by state via `onChange`; uncontrolled: the DOM holds the value, read with a ref and `defaultValue`. |
