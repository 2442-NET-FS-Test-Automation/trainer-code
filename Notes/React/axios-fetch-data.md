# Fetching Data with Axios and Fetch

## Learning Objectives

- Make HTTP requests from React with Axios and with the native Fetch API, and read the response.
- Add an NPM library (Axios) to a React project and configure a reusable client instance.
- Model the loading / error / data states of a request and render each conditionally.
- Attach an `Authorization: Bearer` token in one place using an Axios request interceptor.

## Why This Matters

A front end is only as useful as the data it shows, and that data comes over HTTP. Interviewers expect you to
make a request, handle both success and failure, and reflect the in-flight state in the UI — the "spinner, then
data or error" pattern users take for granted. Doing this in a scattered, ad hoc way leads to duplicated base
URLs, forgotten error handling, and auth tokens copy-pasted onto every call. A single configured Axios instance
with an interceptor fixes all three, and it is the seam every authenticated app hangs its token on.

## The Concept

### Fetch: the built-in baseline

`fetch` ships with the browser — no install needed. It returns a promise of a `Response`; you call `.json()`
(itself async) to get the body. Two gotchas: `fetch` only rejects on **network** failure, not on HTTP error
statuses, so you must check `res.ok` yourself; and you assemble URLs and headers by hand.

```tsx
async function loadBooks(): Promise<Book[]> {
  const res = await fetch("https://api.example.com/books");
  if (!res.ok) {
    throw new Error(`Request failed: ${res.status}`);   // fetch won't throw on 404/500
  }
  return res.json();
}
```

### Adding Axios as an NPM library

Axios is a small HTTP client you add to the project like any dependency. This is the everyday act of leveraging
an NPM library: install it, then import it where needed.

```bash
npm install axios
```

```tsx
import axios from "axios";
```

Axios improves on `fetch` in three ways that matter here: it **parses JSON automatically** (the body is on
`res.data`), it **rejects on HTTP error statuses** (a 404 or 500 lands in `catch`), and it supports a
**configured instance** plus **interceptors** so you set base URL and auth once.

### A reusable Axios instance

Rather than typing the base URL and headers on every call, create one configured client and import it everywhere.

```tsx
// api.ts
import axios from "axios";

export const api = axios.create({
  baseURL: "https://api.example.com",
  headers: { "Content-Type": "application/json" },
});
```

Now every call is relative to that base:

```tsx
import { api } from "./api";

// GET: the parsed body is on res.data
const res = await api.get<Book[]>("/books");
const books = res.data;

// POST: send a body, read what comes back
const created = await api.post<Book>("/books", { title: "Refactoring", author: "Fowler" });
const newBook = created.data;
```

### The request interceptor: attach the token in one place

An **interceptor** is a function that runs on every request (or response) passing through the instance. The
canonical use is authentication: read the stored token and set the `Authorization: Bearer` header on outgoing
requests, so no individual call has to remember it. This interceptor is the single seam where auth attaches to
every request.

```tsx
// api.ts (continued)
api.interceptors.request.use(config => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;   // must return the (possibly modified) config
});
```

Every request through `api` now carries the token automatically. Log in once, store the token, and the interceptor
does the rest; log out by clearing the token and the header simply stops appearing. A response interceptor is the
matching seam for handling a global `401` (for example, redirecting to login).

### The loading / error / data pattern

A request has three visible states: in flight, failed, and succeeded. Model each with state and render
conditionally. Kick the request off in an effect so it runs when the component mounts.

```tsx
import { useEffect, useState } from "react";
import { api } from "./api";

interface Book { id: number; title: string; }

function BookList() {
  const [books, setBooks] = useState<Book[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;                 // guard against setting state after unmount

    async function load() {
      setIsLoading(true);
      setError(null);
      try {
        const res = await api.get<Book[]>("/books");
        if (active) setBooks(res.data);
      } catch (err) {
        if (active) setError("Could not load books.");
      } finally {
        if (active) setIsLoading(false);
      }
    }

    load();
    return () => { active = false; };  // cleanup: ignore a late response
  }, []);                              // empty deps: run once on mount

  if (isLoading) return <p>Loading...</p>;
  if (error) return <p role="alert">{error}</p>;

  return (
    <ul>
      {books.map(b => <li key={b.id}>{b.title}</li>)}
    </ul>
  );
}
```

The shape is always the same: set loading true, `try` the request, set data on success, set an error message on
failure, and set loading false in `finally` so it clears on both paths. The `active` flag prevents a "set state
on an unmounted component" warning if the user navigates away before the response lands.

### Typed error handling with Axios

Because Axios rejects on HTTP errors, `catch` receives an error you can narrow. Use `axios.isAxiosError` to read
the status and server message safely.

```tsx
import axios from "axios";

try {
  await api.post("/books", newBook);
} catch (err) {
  if (axios.isAxiosError(err)) {
    if (err.response?.status === 409) {
      setError("That book already exists.");
    } else {
      setError(`Server error (${err.response?.status ?? "network"}).`);
    }
  } else {
    setError("Unexpected error.");
  }
}
```

## Say It in an Interview

- *"I create one configured Axios instance with `axios.create({ baseURL })` and import it everywhere, so the base
  URL lives in one place. Axios parses JSON onto `res.data` and rejects on HTTP errors, unlike fetch which only
  rejects on network failure and needs a manual `res.ok` check."*
- *"Auth goes in a request interceptor: it reads the token and sets `Authorization: Bearer <token>` on every
  outgoing request, so no individual call has to remember it."*
- *"For any request I track three states — loading, error, and data — and render conditionally: a spinner while
  loading, the error message on failure, the data on success, clearing loading in `finally`."*

## Check Yourself

1. Name two behaviors of Axios that differ from `fetch` when a server returns a 500.
2. What does `axios.create` give you over calling `axios.get` directly each time?
3. Where does a request interceptor run, and what is the standard thing to do in it for authentication?
4. What are the three UI states of a data request, and where do you clear the loading flag so it clears on both
   success and failure?
5. Why keep an `active`/`ignore` flag in the effect that fetches data?

**Answers:** (1) Axios rejects the promise on a 500 (it lands in `catch`) and parses the JSON body onto
`res.data` automatically; `fetch` resolves normally on a 500 (you must check `res.ok`) and needs an explicit
`.json()` call. (2) A reusable instance with a shared `baseURL`, default headers, and interceptors, so
configuration lives in one place instead of on every call. (3) On every outgoing request through the instance;
read the stored token and set `config.headers.Authorization = \`Bearer ${token}\``, then return`config`. (4)
Loading, error, and data (success); clear the loading flag in the`finally` block. (5) So a response that arrives
after the component unmounted (or after a newer request) does not call `setState` on an unmounted component or
overwrite fresh data.

## Summary

- Fetch is built in but only rejects on network failure (check `res.ok`) and needs manual `.json()`; Axios
  (`npm install axios`) parses JSON to `res.data` and rejects on HTTP error statuses.
- Create one instance with `axios.create({ baseURL })` and import it everywhere so configuration lives in one
  place.
- Attach auth in a **request interceptor** that sets `Authorization: Bearer <token>` on every request — the
  single seam auth hangs on.
- Model every request as loading / error / data and render conditionally; clear loading in `finally` and guard
  against setting state after unmount.

## Resources

- [Axios — Creating an instance & interceptors (axios-http.com)](https://axios-http.com/docs/instance)
- [Using the Fetch API (MDN)](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch)
- [Synchronizing with Effects (react.dev)](https://react.dev/learn/synchronizing-with-effects)
