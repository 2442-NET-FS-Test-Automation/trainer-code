# Fetch, JSON, and Talking to HTTP APIs

## Learning Objectives
- Describe JSON's syntax rules and round-trip data with `JSON.stringify` / `JSON.parse`.
- Make GET and POST requests with the Fetch API, including headers and a JSON body.
- Walk the steps of a fetch request from URL to rendered data.
- Handle failures correctly: `response.ok` for HTTP errors, catch for network errors.
- Recognize XHR on sight and say why fetch replaced it.
- Compare localStorage vs HttpOnly cookies for storing an auth token, with the real trade-off.

## Why This Matters
The front end's entire relationship with the back end runs through this note: serialize data to JSON,
send it with fetch, check the status, parse the reply. The single most common bug in beginner API code —
treating a 404 as a success because fetch did not reject — lives here, as does the security decision
every authenticated app makes about where the token goes. Promise mechanics: `promises-async.md`.

## The Concept

### JSON: the wire format
**JSON** (JavaScript Object Notation) is a language-independent *text* format for exchanging data — a
.NET API, a Python script, and a browser all read the same bytes. Its rules are stricter than JS object
literals: **double-quoted keys**, no trailing commas, no comments, and values limited to strings,
numbers, booleans, `null`, arrays, objects — no functions, no `undefined` (dates travel as strings):
```js
const book = { title: "Dune", pages: 412, format: undefined, describe() {} };
const text = JSON.stringify(book);   // '{"title":"Dune","pages":412}'
const clone = JSON.parse(text);      // a plain object again — but describe() and format are GONE
```
`stringify` silently **drops functions and `undefined`** — fine for data transfer, but it makes
stringify/parse a *lossy* clone for anything richer than plain data; methods never survive the wire.

### The Fetch API
`fetch(url)` returns a Promise resolving to a **Response**; the body is read by a second async step,
`response.json()`, which itself returns a Promise:
```js
const response = await fetch("https://api.example.com/api/books");
const books = await response.json();          // two awaits: headers first, then the parsed body

await fetch("https://api.example.com/api/books", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ title: "Emma", author: "Austen" }),
});
```
The options object carries `method` (GET is the default), `headers`, and `body`. For JSON you set
`Content-Type: application/json` and stringify yourself — fetch does not serialize objects for you.

### Anatomy of a request

| Step | Code |
|---|---|
| 1. Build the URL | `` const url = `/api/books?author=${encodeURIComponent(name)}` `` |
| 2. Build options | method, headers, `JSON.stringify` body (GET: none) |
| 3. Send | `const response = await fetch(url, options)` |
| 4. Check status | `if (!response.ok) throw new Error(\`HTTP ${response.status}\`)` |
| 5. Parse body | `const data = await response.json()` |
| 6. Use it | render into the DOM, update state |

### Handling failure: the part everyone gets wrong
`fetch` rejects **only on network failure** — DNS down, connection refused, CORS block. An HTTP
**4xx or 5xx resolves normally**: a 404 is a successful *conversation* whose answer was "not found." So
status checking is your job, via `response.ok` (true for 200-299) or `response.status`:
```js
async function getJson(url) {
  let response;
  try {
    response = await fetch(url);
  } catch (err) {
    throw new Error(`Network failure calling ${url}: ${err.message}`);  // fetch itself rejected
  }
  if (!response.ok) {
    throw new Error(`HTTP ${response.status} from ${url}`);             // server said no
  }
  return response.json();
}
```
Two failure lanes — reachability (catch) and refusal (ok check) — and callers get a single thrown
error either way, handled with the patterns in `error-handling.md`.

### Fetch vs XHR
Before fetch, requests used `XMLHttpRequest` — an event/callback-based object. Recognize it on sight:
```js
const xhr = new XMLHttpRequest();
xhr.open("GET", "/api/books");
xhr.onload = () => console.log(JSON.parse(xhr.responseText));
xhr.send();
```
Fetch wins on ergonomics: promise-based (composes with `async`/`await`), a cleaner Request/Response
model, streaming support. The shared quirk: **neither** treats an HTTP error status as failure.

### Storing auth tokens client-side
After a login endpoint returns a token (commonly a JWT), the client keeps it and attaches it to every
later request as `Authorization: Bearer <token>`:
```js
localStorage.setItem("token", token);          // after login returns { token }
await fetch("/api/loans", {
  headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
});
```
Two mainstream homes, and the trade-off is genuine:
- **localStorage** — simple, survives reload, and your JS can read it to set the header. But *any*
  script running in the page can read it too, so a single XSS hole hands the attacker the token.
- **HttpOnly cookie** — the browser attaches it automatically and script *cannot* read it, so XSS
  cannot exfiltrate the token. But cookies ride along on cross-site requests, so you now need CSRF
  defenses, plus server cooperation to set the cookie and matching CORS credentials settings.

There is no universal winner: localStorage trades XSS exposure for simplicity; HttpOnly cookies trade
CSRF surface and server coupling for XSS protection. Pick per threat model.

### Adjacent: CORS, and 401 vs 403
**CORS** (Cross-Origin Resource Sharing): the browser blocks your JS from reading responses from another
origin unless the server opts in with `Access-Control-Allow-Origin` headers — the classic "works in curl,
fails in the browser" symptom. Status pair: **401** = not authenticated (missing/invalid token);
**403** = authenticated but not allowed (valid token, insufficient role).

## Say It in an Interview
- *"JSON is a language-independent text format — double-quoted keys, no trailing commas, no functions or
  `undefined`. I go to text with `JSON.stringify` and back with `JSON.parse`, remembering stringify
  silently drops functions and undefined values."*
- *"`fetch` returns a promise for a Response, and reading the body is a second promise —
  `response.json()`. For a POST I set the method, a `Content-Type: application/json` header, and a
  stringified body myself."*
- *"My flow is: build the URL, build the options, await fetch, check `response.ok`, then parse and use
  the body."*
- *"Fetch only rejects on network failure — a 404 or 500 resolves normally — so I check `response.ok`
  and throw on HTTP errors, and catch separately for network problems."*
- *"XHR is the older callback-based request object; fetch replaced it with a promise-based, cleaner
  API — though both leave HTTP error statuses for you to check."*
- *"For tokens: localStorage is simple but readable by any script, so XSS steals it; an HttpOnly cookie
  is invisible to script but reintroduces CSRF concerns and needs server cooperation. I present it as a
  trade-off, not a rule."*

## Check Yourself
1. Which of these survive `JSON.stringify`: a string, a date object, a method, `undefined`, an array?
2. Why are there two `await`s in reading a JSON response, and what does each one wait for?
3. A fetch to `/api/books/999` returns 404. Does the promise reject? What must your code do?
4. Write the header entry that attaches a bearer token to a request.
5. An attacker achieves XSS on your page. Which token store do they read, and why does the other resist?

**Answers:** (1) The string and the array survive; the date is converted to an ISO string (survives, but
as text); the method and `undefined` are dropped. (2) The first awaits the Response — status and headers
have arrived; the second awaits `response.json()`, reading and parsing the body stream. (3) No — 4xx/5xx
resolve normally; the code must check `response.ok` (or `.status`) and throw or branch on it. (4)
`Authorization: \`Bearer ${token}\`` inside the `headers` object. (5) localStorage — any script in the
page can call `localStorage.getItem`; an HttpOnly cookie is not exposed to JavaScript at all, so script
cannot exfiltrate it (though CSRF becomes the concern instead).

## Summary
- JSON: strict text format (double-quoted keys, no trailing commas, no functions/`undefined`);
  `stringify`/`parse` round-trips plain data and silently drops the rest.
- `fetch(url, options)` → Promise of Response → `response.json()` → Promise of data; POST needs
  method + `Content-Type` header + stringified body.
- Steps: URL → options → await fetch → check `ok`/status → parse → use. XHR is the callback-era
  predecessor — recognize it; prefer fetch.
- Fetch rejects only on network failure; HTTP 4xx/5xx must be caught by checking `response.ok`.
- Tokens: localStorage (simple, XSS-readable) vs HttpOnly cookie (XSS-proof reads, CSRF + server
  coupling); attach as `Authorization: Bearer`. CORS blocks cross-origin reads without server opt-in;
  401 = unauthenticated, 403 = unauthorized.

## Resources
- [Using the Fetch API (MDN)](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch)
- [JSON (MDN)](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/JSON)
- [Fetch (javascript.info)](https://javascript.info/fetch)
