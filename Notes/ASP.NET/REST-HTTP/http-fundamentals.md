# HTTP Fundamentals: Messages, Verbs, and Status Codes

## Learning Objectives
- Describe the purpose of HTTP messaging and the request/response lifecycle.
- Name the request methods (verbs) and what each is for.
- Explain the status-code classes (1xx-5xx) and pick effective codes for real outcomes.
- Map an API surface's endpoints to their natural status codes.

## Why This Matters
Every call to a web service — from curl, from Swagger, from a front end — is an HTTP message. The verbs
decide what your endpoints *mean*, and the status codes are the contract your callers program against: a
burst endpoint that answers `200 OK` when it actually queued work is lying. Picking honest codes is a
design skill, and this is also the most reliably-asked interview material in the entire web stack.

## The Concept

### HTTP messaging and the lifecycle
HTTP is a **request/response protocol**: text-based messages carrying data between client and server. The
lifecycle: the client opens a connection (TCP, TLS on top for HTTPS), sends a **request** — start line
(method + URI + version), headers, optional body — the server processes it and returns a **response** —
status line (version + status code + reason), headers, optional body. Then the exchange is over: HTTP is
**stateless**, so nothing about this request is remembered by the protocol for the next one (anything that
must persist rides in headers, tokens, or cookies — see `../08-security/authentication-jwt.md`).

```
> POST /orders/burst?n=40&expedited=true HTTP/1.1     < HTTP/1.1 202 Accepted
> Host: localhost:5000                                < Content-Type: application/json
> Content-Type: application/json                      <
>                                                     < {"submitted":40}
```

### The verbs

| Verb | Meaning | Example |
|---|---|---|
| `GET` | read; no side effects | `GET /inventory` |
| `POST` | create / trigger an operation | `POST /orders`, `POST /orders/burst` |
| `PUT` | replace the whole resource | replace an inventory item |
| `PATCH` | partial update | change just `CurrentStock` |
| `DELETE` | remove | `DELETE /api/inventory/BK-001` |

`GET` must be **safe** (no state change) and `GET`/`PUT`/`DELETE` are **idempotent** (repeating them gives
the same end state) — properties intermediaries and retry logic rely on. `POST` is neither, which is why
"trigger a burst" is a `POST`.

### Status-code classes

| Class | Meaning | You will actually meet |
|---|---|---|
| `1xx` | informational | rarely (protocol upgrades) |
| `2xx` | success | 200, 201, 202, 204 |
| `3xx` | redirection | 301/302, 304 (caching) |
| `4xx` | client error — *your caller* got it wrong | 400, 401, 403, 404, 409 |
| `5xx` | server error — *you* got it wrong | 500 |

The 4xx/5xx boundary is a blame line: a 400 means "fix your request," a 500 means "we crashed" (a global
exception middleware should turn every unhandled exception into exactly one clean 500 — see
`../06-aspnet-core/aspnet-pipeline-middleware.md`).

### Effective codes: a placement map
For an inventory/ordering surface, the common codes have natural homes. Emit the ones your surface
honestly produces — do not invent an endpoint just to emit a code:

| Code | Natural home |
|---|---|
| `200 OK` | every successful read: `GET /inventory`, reports, verification endpoints |
| `201 Created` | `POST /orders` when the order is accepted — plus a `Location` header pointing at the new resource |
| `202 Accepted` | `POST /orders/burst` — "queued, working on it"; the work drains on a background task, so 200 would be false |
| `400 Bad Request` | bad input: an unknown SKU or unknown order kind |
| `404 Not Found` | a lookup of an id/SKU that does not exist: `GET /api/inventory/NOPE` |
| `409 Conflict` | a request that conflicts with current state: resetting stock mid-burst, violating a unique key |

In a Minimal API these come from `Results` (`Results.Ok(...)`, `Results.Created(uri, body)`,
`Results.Accepted(...)`, `Results.BadRequest(...)`, `Results.NotFound()`, `Results.Conflict()`); in
controllers from the `ControllerBase` helpers (`Ok()`, `CreatedAtAction(...)`, `NotFound()`...). A good
API README states which codes the surface produces and why — this table is the shape of that answer.

## Say It in an Interview
- *"HTTP is a stateless request/response protocol: start line, headers, optional body each way — nothing
  persists between exchanges unless you carry it in headers, tokens, or cookies."*
- *"GET reads, POST creates or triggers, PUT replaces, PATCH partially updates, DELETE removes. GET is
  safe, and GET/PUT/DELETE are idempotent — retry logic and proxies depend on those properties."*
- *"Status classes are a blame line: 2xx success, 4xx the caller erred, 5xx we erred. 400 means fix your
  request; 500 means we crashed."*
- *"I pick codes for honesty: 202 when work is queued to a background task, 201 with a `Location` header
  for creation, and 400 vs 404 vs 409 by the *kind* of client mistake — malformed, missing, or conflicting
  with current state."*

## Check Yourself
1. What three parts make up an HTTP request, and what three make up the response?
2. Why is "trigger a batch job" a POST rather than a GET, in terms of safety and idempotency?
3. An endpoint queues work on a background task and returns immediately. Which 2xx code, and why not 200?
4. Caller sends a well-formed order for a SKU that doesn't exist vs a malformed order body vs a reset
   during an active run — assign 400/404/409.
5. Where is the 4xx/5xx boundary in one sentence?

**Answers:** (1) Request: start line (method + URI + version), headers, optional body. Response: status
line (version + code + reason), headers, optional body. (2) GET must be safe (no state change) and
repeatable; triggering a job changes state and repeating it duplicates work — POST is neither safe nor
idempotent, which matches. (3) `202 Accepted` — 200 would claim the work is done when it has only been
accepted. (4) 404 (resource missing), 400 (malformed input), 409 (conflicts with current state).
(5) 4xx: the caller must change the request; 5xx: the server failed and the caller did nothing wrong.

## Summary
- HTTP: stateless request/response messages — start line + headers + body each way.
- Verbs carry meaning: GET reads (safe), POST creates/triggers, PUT replaces, PATCH partially updates,
  DELETE removes; idempotency is part of the contract.
- Classes: 2xx you succeeded, 4xx the caller erred, 5xx you erred.
- Effective codes are honest codes: 202 for queued bursts, 201 + Location for created orders, 400 vs 404
  vs 409 by *what kind* of caller mistake.

## Resources
- [An overview of HTTP (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTTP/Overview)
- [HTTP response status codes (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)
- [RFC 9110 — HTTP Semantics](https://www.rfc-editor.org/rfc/rfc9110.html)
