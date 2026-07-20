# Client-Side Routing with React Router

## Learning Objectives
- Explain how a single-page application routes in the browser without full-page reloads.
- Wire up React Router with `BrowserRouter`, `Routes`, `Route`, and navigate with `Link`.
- Read URL parameters with `useParams` and navigate programmatically with `useNavigate`.
- Compose nested routes that render children through an `Outlet` shared layout.

## Why This Matters
A single-page application loads one HTML shell and then swaps views in the browser. Without a router, that means
one giant component with a pile of conditionals; with React Router it means clean, bookmarkable URLs that map to
components, a working back button, and shareable deep links. "How does routing work in an SPA" and "how do you
read a route parameter" are standard interview questions, and every non-trivial React app needs this. React
Router is the de facto standard, so it is worth knowing its core pieces cold.

## The Concept

### SPA routing vs multi-page navigation
In a traditional multi-page app, every link asks the server for a whole new HTML document and the browser
repaints from scratch. An SPA loads once, then **JavaScript intercepts navigation**, updates the URL with the
History API, and re-renders the matching view in place — no round trip, no flash. React Router is the library
that maps URL paths to components and keeps the URL and the UI in sync. Install it as a normal dependency:

```
npm install react-router-dom
```

### BrowserRouter, Routes, and Route
Wrap your app once in `BrowserRouter`. Inside it, declare a `Routes` block containing one `Route` per URL path;
each `Route` names a `path` and the `element` to render when the URL matches.

```tsx
import { BrowserRouter, Routes, Route } from "react-router-dom";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/books" element={<BookList />} />
        <Route path="/about" element={<About />} />
        <Route path="*" element={<NotFound />} />
      </Routes>
    </BrowserRouter>
  );
}
```

`path="*"` is the catch-all: it matches anything no other route did, which is how you render a 404 view. Only the
element whose path matches the current URL is rendered; the rest are not.

### Navigating with Link
Never use a plain `<a href>` for in-app navigation — that triggers a full page reload and throws away your app's
state. Use `Link` (or `NavLink`, which adds an active style), which changes the URL and lets the router swap the
view client-side.

```tsx
import { Link, NavLink } from "react-router-dom";

function Nav() {
  return (
    <nav>
      <Link to="/">Home</Link>
      <Link to="/books">Catalog</Link>
      {/* NavLink exposes isActive for styling the current route */}
      <NavLink
        to="/about"
        style={({ isActive }) => ({ fontWeight: isActive ? "bold" : "normal" })}
      >
        About
      </NavLink>
    </nav>
  );
}
```

### Route parameters with useParams
A dynamic segment in the path is written with a colon: `path="/books/:id"`. Inside the rendered component, read
that segment with the `useParams` hook. The value is always a string, so parse it if you need a number.

```tsx
import { Routes, Route, useParams } from "react-router-dom";

function AppRoutes() {
  return (
    <Routes>
      <Route path="/books" element={<BookList />} />
      <Route path="/books/:id" element={<BookDetail />} />
    </Routes>
  );
}

function BookDetail() {
  const { id } = useParams();          // e.g. "/books/42" -> id === "42"
  const bookId = Number(id);
  return <h2>Showing book #{bookId}</h2>;
}
```

Now `/books/42` and `/books/7` render the same component with different data — one route definition serves every
book.

### Programmatic navigation with useNavigate
Sometimes you navigate in response to logic rather than a click on a link — after a form submits, say. The
`useNavigate` hook returns a function you call with a path.

```tsx
import { useNavigate } from "react-router-dom";

function AddBook() {
  const navigate = useNavigate();

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    // ...save the book...
    navigate("/books");          // go to the list
    // navigate(-1) would go back one entry, like the browser back button
  }

  return <form onSubmit={handleSubmit}>{/* fields */}</form>;
}
```

### Nested routes and Outlet
When several views share a layout — a sidebar, a header, a tab strip — nest their routes under a parent and let
the parent render an `Outlet` where the matched child should appear. The parent's chrome stays mounted while only
the inner view changes.

```tsx
import { Routes, Route, Outlet, Link } from "react-router-dom";

function BooksLayout() {
  return (
    <div>
      <aside>
        <Link to="/books">All</Link>
        <Link to="/books/new">Add</Link>
      </aside>
      <main>
        <Outlet />           {/* the matched child route renders here */}
      </main>
    </div>
  );
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/books" element={<BooksLayout />}>
        <Route index element={<BookList />} />     {/* /books */}
        <Route path=":id" element={<BookDetail />} /> {/* /books/42 */}
        <Route path="new" element={<AddBook />} />    {/* /books/new */}
      </Route>
    </Routes>
  );
}
```

Child paths are **relative** to the parent, and the `index` route is what shows at the parent's own path
(`/books`). This is how you model real UI architecture: shared layout once, swappable content inside.

### A note on route guards
Restricting a route based on whether the user is authenticated — redirecting an unauthenticated visitor to a
login page, for example — is called a *route guard* or protected route. That pattern is covered separately; this
note stays on the navigation mechanics themselves.

### The data-router alternative
Newer React Router also offers `createBrowserRouter` with a route array and a `RouterProvider`, which unlocks
data loading and actions per route. The routing concepts are identical — the same paths, params, and outlets —
just declared as objects instead of JSX.

```tsx
import { createBrowserRouter, RouterProvider } from "react-router-dom";

const router = createBrowserRouter([
  {
    path: "/books",
    element: <BooksLayout />,
    children: [
      { index: true, element: <BookList /> },
      { path: ":id", element: <BookDetail /> },
    ],
  },
]);

function App() {
  return <RouterProvider router={router} />;
}
```

## Say It in an Interview
- *"An SPA loads one shell and swaps views client-side. React Router maps URL paths to components: wrap the app
  in BrowserRouter, declare Routes with a Route per path, and navigate with Link instead of an anchor so there is
  no full reload."*
- *"A dynamic segment like `/books/:id` is read with `useParams`; it comes back as a string. For navigation in
  code — after a submit — I use `useNavigate`."*
- *"Nested routes share a layout: the parent renders an `Outlet` where the matched child appears, and an `index`
  route is the default child at the parent's path."*

## Check Yourself
1. What is the difference between how an SPA and a multi-page app handle a navigation click?
2. Why use `Link` instead of `<a href>` for in-app navigation?
3. Given `path="/books/:id"`, how do you read the id inside the component, and what type is it?
4. You need to navigate to `/books` after saving a form — no link is clicked. Which hook, and how?
5. In a nested-route layout, what does `<Outlet />` do, and what does an `index` route render?

**Answers:** (1) An SPA intercepts the click, updates the URL via the History API, and re-renders the matching
view in place with no server round trip; a multi-page app fetches a whole new HTML document from the server and
repaints. (2) `Link` changes the URL client-side and lets the router swap the view, preserving app state; `<a
href>` triggers a full page reload. (3) `const { id } = useParams();` — it is a string, so `Number(id)` if you
need a number. (4) `const navigate = useNavigate();` then `navigate("/books")`. (5) `<Outlet />` marks where the
matched child route renders inside the parent layout; the `index` route is the default child shown at the
parent's own path.

## Summary
- An SPA swaps views client-side; React Router maps URL paths to components and keeps URL and UI in sync.
- Wrap once in `BrowserRouter`, declare a `Route` per `path` inside `Routes`, and navigate with `Link`/`NavLink`
  (never a plain anchor for in-app links).
- Read dynamic segments (`:id`) with `useParams` (always a string); navigate from code with `useNavigate`.
- Nest routes under a parent that renders an `Outlet`; child paths are relative and `index` is the default child.
  Route guards for auth are covered separately.

## Resources
- [React Router — Route (reactrouter.com)](https://reactrouter.com/en/main/route/route)
- [React Router — useParams (reactrouter.com)](https://reactrouter.com/en/main/hooks/use-params)
- [React Router — Nested Routes tutorial (reactrouter.com)](https://reactrouter.com/en/main/start/tutorial)
