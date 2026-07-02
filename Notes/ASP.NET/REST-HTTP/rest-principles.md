# REST, URL Conventions, SOA, and Calling APIs

## Learning Objectives
- Describe the purpose of REST and the six REST principles.
- Apply RESTful URL conventions to resource design.
- Describe Service-Oriented Architecture and diagram a sample system.
- Distinguish authentication from authorization.
- Send GET and POST requests to a REST API with curl or Postman.

## Why This Matters
REST is the architectural style most APIs you will build or consume follow: resources named by URIs,
manipulated with HTTP verbs, no server-side session. The conventions are what make an API guessable — a
developer who has never seen your service can predict that inventory lives at `/inventory` and a specific
item at `/inventory/{sku}`. And because poking your own service from the command line is daily
development practice, curl fluency is a working tool, not exam trivia.

## The Concept

### What REST is
REST (Representational State Transfer) is an **architectural style for stateless, resource-based web
services**: things (resources) are identified by URIs, and you operate on them with the uniform HTTP verb
set, exchanging *representations* (usually JSON) of their state. It is a set of constraints, not a
protocol — HTTP is the protocol; REST is the discipline in how you use it.

### The six principles

| Principle | Meaning | Concrete form |
|---|---|---|
| Client-server | UI concerns separated from data concerns | any client (curl, Swagger, an SPA) against one API |
| Stateless | every request carries all context; no server session | a JWT rides on each request instead of a login session |
| Cacheable | responses declare their cacheability | `GET /api/inventory` sends `Cache-Control: max-age=30` |
| Uniform interface | same verbs + URIs + representations everywhere | `GET/POST/DELETE` on `/api/inventory/{sku}` |
| Layered system | intermediaries (proxies, gateways) can sit between | works unchanged behind a reverse proxy |
| Code on demand (optional) | server may ship executable code | not used (typical) |

Statelessness is the one with teeth: it is why any instance of your API can serve any request, which is
why REST services scale horizontally so well.

### URL conventions
Resources are **plural nouns**; nesting expresses ownership; verbs stay out of the path (the HTTP method
is the verb):

```
/products                 all products
/products/42              one product
/customers/1/orders       orders belonging to customer 1
/orders?status=backorder  filtering via query string
```

Avoid `/getProducts` or `/products/delete/42` — the method already says that. One honest reality: most
APIs keep a small RPC-style corner for operational actions that are not resources (`/orders/burst`,
`/seed/reset`, `/verify/no-oversell`) — name those as actions deliberately, and keep the corner small and
obvious.

### SOA: the wider frame
Service-Oriented Architecture is the style where a system is composed of **loosely coupled services
communicating over a network**, each owning one capability. REST APIs are the most common way those
services talk. Be able to sketch this from memory:

```
                      +----------------+
   Client (SPA/curl)  |  API Gateway   |
        ------------> | (routing/auth) |
                      +-------+--------+
              +---------------+----------------+
              v                                v
      +---------------+               +------------------+
      | Order Service  | --------->   | Supplier Service |   (3rd-party HTTP API)
      +-------+-------+               +------------------+
              v
        +-----------+
        | SQL Server |
        +-----------+
```

Note the order service plays both roles: it *is* a service, and it *consumes* one (a supplier-price
lookup over `HttpClient`) — most real services do both.

### Authentication vs authorization
Two words that are not synonyms, and a favorite interview screen:

- **Authentication** — *who are you?* Verifying identity (login, validating a JWT's signature).
- **Authorization** — *what may you do?* Checking permissions after identity is known (`[Authorize]`,
  roles, policies).

The order is fixed: authenticate first, then authorize. In ASP.NET Core the middleware literally runs in
that order (`UseAuthentication()` then `UseAuthorization()`); the full JWT mechanics are in
`../08-security/authentication-jwt.md`.

### curl and Postman
GET and POST-with-body from the command line, against real endpoints:

```bash
# GET - read the inventory
curl http://localhost:5000/inventory

# POST with a JSON body - create an order
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"kind":"expedited","customerId":1,"lines":[{"sku":"BK-001","qty":2}]}'

# POST with query parameters - trigger a burst, see the 202 and headers
curl -i -X POST "http://localhost:5000/orders/burst?n=40&expedited=true"
```

`-X` sets the verb, `-H` a header, `-d` the body (which also implies POST), `-i` prints response headers so
you can *see* the status code. Postman is the same requests with a UI — build the request, set the
`Content-Type` header, put JSON in the Body tab, read the status code in the response pane. Use whichever
you like; be able to do both.

## Say It in an Interview
- *"REST is an architectural style for stateless, resource-based services: URIs name resources, the
  uniform HTTP verb set operates on them, and you exchange representations — usually JSON."*
- *"The six principles: client-server, stateless, cacheable, uniform interface, layered system, and
  optional code on demand. Statelessness is the scalability lever — any instance can serve any request."*
- *"URL conventions: plural nouns, nest for ownership, filter with query strings, keep verbs out of paths
  — the HTTP method is the verb."*
- *"SOA composes a system from loosely coupled services over a network; I'd diagram client, gateway,
  services, and data stores, and note a service usually consumes other services too."*
- *"Authentication proves who you are; authorization decides what you may do — and it has to run in that
  order."*

## Check Yourself
1. Recite the six REST principles and name the one that most directly enables horizontal scaling.
2. Design URLs for: all customers; customer 7; customer 7's orders; open orders only.
3. What is wrong with `POST /products/delete/42` as a RESTful route?
4. Sketch (verbally) an SOA diagram for an ordering system with a gateway and one external dependency.
5. A request carries a valid token but hits a 403. Which of authentication/authorization failed?
6. Write the curl for a POST of `{"name":"Ada"}` to `/customers` showing response headers.

**Answers:** (1) Client-server, stateless, cacheable, uniform interface, layered system, code on demand
(optional); stateless. (2) `/customers`, `/customers/7`, `/customers/7/orders`,
`/orders?status=open` (or `/customers/7/orders?status=open`). (3) The verb is in the path; the method
already says it: `DELETE /products/42`. (4) Client -> API gateway (routing/auth) -> order service ->
database, with the order service also calling a supplier service over HTTP. (5) Authorization —
authentication succeeded (identity known), but the identity lacks permission. (6)
`curl -i -X POST http://localhost:5000/customers -H "Content-Type: application/json" -d '{"name":"Ada"}'`.

## Summary
- REST = stateless, resource-based services: URIs name resources, HTTP verbs operate on them.
- Six principles: client-server, stateless, cacheable, uniform interface, layered, (optional) code on
  demand — stateless is the scalability lever.
- URLs: plural nouns, nest for ownership, filter with query strings, no verbs in paths.
- SOA: loosely coupled services over a network; be able to diagram client -> gateway -> services -> data.
- Authentication proves who; authorization decides what; in that order.
- curl: `-X`, `-H`, `-d`, `-i` cover everyday API work.

## Resources
- [What is REST? (restfulapi.net)](https://restfulapi.net/)
- [Web API design best practices (Microsoft Learn)](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [Everything curl — HTTP basics](https://everything.curl.dev/http/index.html)
