# Microservices Infrastructure Patterns: Registry, Discovery, Gateway, Load Balancing, Circuit Breaker

## Learning Objectives
- Explain the service registry and how service discovery works — client-side vs server-side.
- Describe the API gateway pattern: a single entry point handling routing, cross-cutting concerns, and aggregation.
- Distinguish client-side from server-side load balancing and name common strategies (round-robin, least-connections).
- Trace the circuit breaker through its closed / open / half-open states and explain how it prevents cascading failure.
- Recognize the real-world tools that implement each pattern.

## Why This Matters
Splitting a system into services solves some problems and creates new ones: *Where is the Orders service right now? Which of its five copies should I call? What happens when one of them is down?* In a monolith these questions never existed — everything was an in-process call. A microservices architecture only works because a small set of well-known infrastructure patterns answers them. Knowing these patterns by name, and being able to sketch how a request flows through them, is exactly what a system-design interview probes when it asks "how do your services find and call each other reliably?"

## The Concept

### Service registry and service discovery
Service instances are ephemeral: they scale up and down, move between hosts, and get new IP addresses on every deploy. Hard-coding addresses is therefore impossible. A **service registry** is the solution — a live directory (a specialized database) of every healthy instance: its name, address, port, and health status. Instances **register** on startup, send periodic heartbeats, and are **deregistered** when they stop or fail their health check. Netflix Eureka and HashiCorp Consul are canonical examples; Kubernetes has an equivalent built into its DNS and Service objects.

**Service discovery** is the act of *querying* that registry to turn a logical name ("orders-service") into a concrete address to call. There are two shapes:

- **Client-side discovery.** The caller asks the registry for the list of healthy instances and picks one itself (so the client also does the load balancing). Fewer network hops and full control, but every client needs discovery logic, usually via a library. Eureka with a smart client is the classic example.
- **Server-side discovery.** The caller sends the request to a fixed endpoint — a load balancer or gateway — and *that* component queries the registry and forwards the request. Clients stay simple and dumb; the routing tier does the work. A cloud load balancer, or Kubernetes routing a request through a Service, is server-side discovery.

```
Client-side discovery                 Server-side discovery
 client --ask--> [registry]            client --> [ LB / gateway ] --ask--> [registry]
 client --call-> chosen instance                     |----call----> chosen instance
```

The trade-off in one line: client-side puts intelligence (and coupling to the registry) in every caller; server-side centralizes it in a routing tier at the cost of an extra hop.

### API gateway
If every client had to discover and call each service directly, the client would need to know the whole internal topology, and every service would have to re-implement authentication, rate limiting, and logging. The **API gateway** pattern solves this by putting a **single entry point** in front of the services. External callers talk only to the gateway; the gateway routes each request to the right internal service.

Because it sits on the path of every request, the gateway is the natural home for **cross-cutting concerns** that would otherwise be duplicated in every service:
- **Routing** — map an external path (`/api/orders/...`) to the correct internal service.
- **Authentication and authorization** — validate the token once at the edge so services can trust incoming requests.
- **Rate limiting and throttling** — protect the system from abuse and overload.
- **TLS termination, logging, tracing, and metrics** — handled uniformly in one place.
- **Response aggregation** — fan one client request out to several services and compose a single response (closely related to the Backend-for-Frontend pattern, where a tailored gateway serves a specific client such as a web or mobile app).

In the .NET ecosystem the gateway pattern is implemented by **YARP** (Microsoft's reverse-proxy toolkit) and **Ocelot**; in the Java/Spring world by **Spring Cloud Gateway**; and infrastructure proxies like **NGINX** or **Envoy** fill the same role. One caution: a gateway is a single point of failure and a potential bottleneck, so it is run redundantly and kept deliberately thin — routing and cross-cutting concerns only, never business logic.

```
                 +---------------------+        +-----------+
   clients  -->  |     API Gateway     | -----> |  Orders   |
 (web/mobile)    |  auth, rate-limit,  | -----> | Payments  |
                 |  routing, aggregate | -----> | Inventory |
                 +---------------------+        +-----------+
```

### Load balancing
When a service runs as several identical instances, something must spread requests across them so no single instance is overwhelmed while others idle. That is **load balancing**, and it comes in the same two shapes as discovery:

- **Server-side load balancing.** A dedicated component (NGINX, a cloud load balancer, a Kubernetes Service) receives all traffic and distributes it. Clients know only the one address. Simple clients, centralized control, one more hop.
- **Client-side load balancing.** The caller already holds the list of instances from discovery and chooses one itself, cutting out the middle hop. It requires a capable client library and pairs naturally with client-side discovery.

Either way, a **strategy** decides which instance gets the next request:
- **Round-robin** — hand requests out in rotation, one after another. Simple and fair when instances and requests are roughly equal.
- **Least-connections** — send the next request to the instance currently handling the fewest, which adapts better when some requests are far heavier than others.
- Others include weighted round-robin (bias toward more powerful instances) and hashing on a key like client IP for session stickiness.

Health checks are essential: the load balancer must stop routing to an instance that fails its health probe, and resume when it recovers. Load balancing gives you both horizontal scale and a first layer of resilience — but it only handles an instance being *down*, not one that is *up but failing slowly*. That last problem needs the circuit breaker.

### Circuit breaker
Imagine Orders calls Payments synchronously, and Payments becomes slow or unresponsive. Every Orders request now blocks waiting for a timeout, tying up threads and connections. Orders itself becomes slow, its callers back up, and the failure **cascades** across the system until everything is exhausted — all because one downstream dependency was sick. The **circuit breaker**, borrowed from electrical engineering, stops this by failing fast instead of waiting on a call that is likely to fail. It wraps a remote call and tracks failures, moving through three states:

```
        failures exceed threshold
 CLOSED ---------------------------> OPEN
   ^                                   |
   |                                   | after a cooldown timer
   | trial call                        v
   +----------- succeeds ------- HALF-OPEN
   |                                   |
   +------ trial call fails -----------+   (back to OPEN)
```

- **Closed** — normal operation. Calls pass through to the dependency; the breaker counts failures. If the failure rate crosses a threshold, it trips to **open**.
- **Open** — the breaker short-circuits: calls fail *immediately* without touching the dependency, usually returning an error or a **fallback** (a cached value, a default, a graceful degradation). This gives the struggling service room to recover and keeps the caller's threads free. After a cooldown timer it moves to **half-open**.
- **Half-open** — a limited number of trial calls are allowed through. If they succeed, the dependency is healthy again and the breaker returns to **closed**; if they fail, it snaps back to **open** and waits again.

The point is that failing fast is healthier than failing slow: an immediate error (or a sensible fallback) is far better than a pile-up of blocked threads, and it prevents one sick service from dragging down the whole system. In the .NET ecosystem this is provided by **Polly** (integrated into `Microsoft.Extensions.Http.Resilience`), and in the JVM/Spring world by **Resilience4j**; **Steeltoe** brings several of these patterns to .NET. Circuit breakers are almost always used alongside **timeouts** (never wait forever), **retries** (with backoff, for transient blips), and **bulkheads** (isolate resource pools so one failing dependency cannot consume every thread).

A minimal illustration of the wrapping idea (conceptual, not tied to any one library):

```
call PaymentsService with a circuit breaker:
  if breaker is OPEN:            return fallback immediately   // fail fast
  else:                          // CLOSED or HALF-OPEN
    try   result = paymentsService.charge(order)
          breaker.recordSuccess()
          return result
    catch breaker.recordFailure()   // may trip breaker to OPEN
          return fallback
```

## Say It in an Interview
- *"A service registry is a live directory of healthy instances; discovery is looking a service up in it. Client-side discovery has the caller query the registry and pick an instance; server-side puts that behind a load balancer or gateway so clients stay simple."*
- *"An API gateway is the single entry point in front of the services — it handles routing plus cross-cutting concerns like auth, rate limiting, and request aggregation, so every service doesn't reimplement them. YARP, Ocelot, and Spring Cloud Gateway are examples."*
- *"Load balancing spreads traffic across instances, client-side or server-side, using strategies like round-robin or least-connections, with health checks to skip dead instances."*
- *"A circuit breaker wraps a remote call and trips from closed to open when failures spike, failing fast with a fallback instead of piling up on a slow dependency; after a cooldown it goes half-open to test recovery. That's what stops cascading failure — Polly does this in .NET."*

## Check Yourself
1. What does a service registry store, and what keeps its entries current?
2. In client-side vs server-side discovery, who queries the registry and who picks the instance?
3. Name three cross-cutting concerns an API gateway handles that services would otherwise each implement, and one risk of the gateway itself.
4. What problem does least-connections solve that plain round-robin does not?
5. Walk the circuit breaker through closed → open → half-open, and explain what each transition prevents.

**Answers:** (1) A live list of every healthy service instance — name, address, port, health status — kept current by instances registering on startup, sending heartbeats, and being deregistered when they stop or fail a health check. (2) Client-side: the calling client queries the registry and picks the instance itself (and load-balances). Server-side: a load balancer or gateway queries the registry and forwards; the client just hits a fixed endpoint. (3) Any three of routing, authentication/authorization, rate limiting, TLS termination, logging/tracing, response aggregation — the risk is that the gateway is a single point of failure and potential bottleneck, so it must be run redundantly and kept thin. (4) Least-connections adapts when request costs are uneven, routing to the least-busy instance; round-robin assumes every request is roughly equal and can overload an instance stuck with heavy work. (5) Closed: calls pass through, failures counted — trips to open when they exceed a threshold (prevents ignoring a failing dependency). Open: calls fail fast with a fallback, sparing the caller's threads and giving the dependency room to recover (prevents cascading failure); after a cooldown it goes half-open. Half-open: a few trial calls test recovery — success returns to closed, failure snaps back to open (prevents hammering a still-broken service).

## Summary
- A **service registry** is a live directory of healthy instances; **service discovery** looks names up in it — **client-side** (caller queries and picks) vs **server-side** (a router/gateway does it, clients stay simple).
- An **API gateway** is the single entry point handling routing plus cross-cutting concerns (auth, rate limiting, aggregation); implemented by YARP, Ocelot, Spring Cloud Gateway. Keep it thin and redundant.
- **Load balancing** spreads traffic across instances, **client-side** or **server-side**, via strategies like **round-robin** and **least-connections**, backed by health checks.
- A **circuit breaker** cycles **closed → open → half-open**, failing fast with a fallback when a dependency is sick to prevent **cascading failure**; Polly (.NET) and Resilience4j (JVM) implement it, alongside timeouts, retries, and bulkheads.

## Resources
- [Pattern: API Gateway / Backends for Frontends — microservices.io](https://microservices.io/patterns/apigateway.html)
- [Pattern: Circuit Breaker — microservices.io](https://microservices.io/patterns/reliability/circuit-breaker.html)
- [Implement resilient applications (Microsoft .NET microservices docs)](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/)
