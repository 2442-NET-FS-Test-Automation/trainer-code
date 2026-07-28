# QC-5 (Front End) — Self-Assessment Checklist

Every objective below is reproduced **verbatim** from `qc-criteria/QC-5-Front-End.md`, grouped by
priority tier and rubric section. Read each as a self-question — *"Can I confidently do / explain this
without notes?"* Tick only what you can do unaided; every unticked box points you at the matching cluster
in `study-guide.md`, task in `drills.md`, and question in `mock-interview.md`.

The rubric is **99 objectives**: 54 Must know, 26 Should know, 19 Nice to Have. **Must + Should are
mandatory** (74 rows in the exam's mandatory band, once Nice is set aside). Nice-to-have rows are all
covered in the written notes too — they are listed here in full so this checklist mirrors the real rubric.

---

## Must know

### HTML/CSS
- [ ] Describe what HTML is.
- [ ] Describe the structure of an HTML document and what is included in the different sections.
- [ ] List common HTML tags and describe why they are different from divs.
- [ ] Describe how/where you link an external CSS sheet into an HTML document.
- [ ] Describe how/where you link an external JS file into an HTML document.
- [ ] Describe the structure of a CSS style rule.
- [ ] Explain the CSS box model.
- [ ] Describe the different ways to add styling to an HTML document.
- [ ] Use the correct syntax for styling different elements such as by tag, class, id, etc.
- [ ] Describe CSS priority in regards to inline, internal, and external styles.

### JS Language
- [ ] Describe what JS is.
- [ ] Describe what type coercion is.
- [ ] Describe what truthy/falsy is.
- [ ] Describe the different variable scopes in JS.
- [ ] Explain the different data types in JS.
- [ ] Create variables in JS.
- [ ] Create objects in JS.
- [ ] Handle errors in JS.
- [ ] Create arrays in JS.
- [ ] Describe the different array methods and how to use them.
- [ ] Loop through arrays.
- [ ] Describe the different types of functions in JS.

### Browser Based JS
- [ ] Describe what the DOM is.
- [ ] Query the DOM for elements.
- [ ] Describe what event listeners are.
- [ ] Insert new elements into the DOM.
- [ ] Explain what a JavaScript Promise is and when it is used to handle asynchronous operations.
- [ ] Describe what type of object the Fetch API returns.
- [ ] Explain what JSON is.
- [ ] Handle a failed request when using the Fetch API.
- [ ] Describe the different promise methods.
- [ ] Explain the difference between synchronous and asynchronous programming.

### TypeScript
- [ ] Compare/contrast TypeScript to JavaScript.
- [ ] Describe and implement basic types in TypeScript.
- [ ] Implement user defined types in TypeScript.
- [ ] Describe and implement casting in TypeScript.
- [ ] Describe and demonstrate the process to transpile and run TypeScript.

### React
- [ ] Describe and implement functional components in React.
- [ ] Explain the difference between Single Page Applications and Multi Page Applications.
- [ ] Utilize and explain common React hooks: useState, useEffect, and useContext.
- [ ] Pass props to components and manage local component state.
- [ ] Create and run a React application using Vite CLI.
- [ ] Explain the lifecycle of a React component.
- [ ] Describe how the React Virtual DOM works and how it improves performance.
- [ ] Make HTTP requests using Axios or Fetch and handle the response.
- [ ] Write and explain JSX syntax and how it integrates with JavaScript.
- [ ] Use useReducer for complex state management scenarios.
- [ ] Explain and apply the principles of state immutability in React.
- [ ] Handle user input through form elements and manage form state.
- [ ] Implement component communication through props and callbacks (Parent to Child & vice versa).
- [ ] Build and use nested component structures to model UI architecture.
- [ ] Use React Router to implement navigation in a single-page application.
- [ ] Apply styling to components using inline styles, CSS modules, or external stylesheets.
- [ ] Use Lists and Keys correctly to render dynamic components efficiently.

---

## Should know

### HTML/CSS
- [ ] Construct an HTML form.
- [ ] Take in user input using a variety of input tags (text, checkbox, etc).

### JS Language
- [ ] Use template literals.
- [ ] Describe what the this keyword is.
- [ ] Explain the role of callbacks in JavaScript programming.
- [ ] Define arrow functions and explain the benefits of using arrow functions.
- [ ] Create arrow functions.
- [ ] Create anonymous functions.

### Browser Based JS
- [ ] List the steps to sending an HTTP request using the Fetch API.
- [ ] Describe what async/await is and how they compare to using .then().
- [ ] Explain what JSON.stringify() and JSON.parse() are.

### TypeScript
- [ ] Implement TypeScript outside of Angular/React environments using plain .ts files.
- [ ] Describe the purpose of the "strict" flag in the tsconfig.json file.
- [ ] Describe and implement union types.
- [ ] Describe and implement type guards.
- [ ] Describe and implement type aliasing.

### React
- [ ] Use Context.Provider tags to wrap components and distribute application state.
- [ ] Route users between components through the use of BrowserRouter.
- [ ] Leverage route guards to change the routing behavior based on the given state.
- [ ] Use a Reducer to manage a set of complex known states.
- [ ] Conditionally render a component based on user interaction and/or state.
- [ ] Describe the benefits of TypeScript in React development.
- [ ] Leverage NPM libraries in a React project to add functionality.
- [ ] Lift state up to a parent component to share data between child components.
- [ ] Describe how one-way data flow works in React.
- [ ] Build a reusable component using TSX with type-checked props.

---

## Nice to Have

Not required by the coverage standard (R-002) — but every row below has written coverage in the concept
notes, and most were demoed live. They are classic interview questions, so they are worth a pass once the
Must/Should bands are solid.

### HTML/CSS
- [ ] Describe the benefits of combinators and how to use them.
- [ ] Make responsive webpages using CSS.

### JS Language
- [ ] Describe and explain a closure.
- [ ] Describe what function and variable hoisting is.
- [ ] Describe how inheritance works in JS.

### Browser Based JS
- [ ] Describe what bubbling and capturing are and their difference.
- [ ] Describe some methods on the event object and what they do.
- [ ] Explain how to chain multiple asynchronous operations using Promises or async/await.
- [ ] Implement error handling using try-catch blocks with async/await.
- [ ] Describe and explain the event loop.
- [ ] Describe the difference between Fetch and XHR.

### TypeScript
- [ ] Configure the TypeScript compiler using options in the tsconfig.json based on project needs.
- [ ] Describe and leverage generic types.

### React
- [ ] Use createContext and Context.Provider to manage global state.
- [ ] Use refs to store information without triggering a re-render.
- [ ] Use Jest and a React testing library to test components.
- [ ] Leverage advanced routing techniques to create parent-child routing, or through passing variables into routes.
- [ ] Explain and implement higher-order and container components for reusable logic.
- [ ] Compare and implement controlled vs uncontrolled components in form handling.

---

## Not yet covered

**Nothing.** Every objective on this rubric maps to material delivered in Weeks 6-7 — concept notes for
all 99 rows, and a live demo beat for every Must and Should row. See `out-of-scope-register.md` for the
detail (including the five Nice rows that are written-coverage-only, and the rows that were cut from the
rubric before it reached this form).
