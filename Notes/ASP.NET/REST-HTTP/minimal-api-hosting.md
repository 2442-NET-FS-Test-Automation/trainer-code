# Minimal APIs, Kestrel, and Configuration

## Learning Objectives
- Stand up a Minimal-API host with `dotnet new web` and explain the builder/app split in `Program.cs`.
- Describe what Kestrel is and where it sits.
- Implement `MapGet`/`MapPost` endpoints whose parameters bind from route, query, body, and DI.
- Read settings and connection strings from configuration across environments.

## Why This Matters
Minimal APIs are ASP.NET Core with the ceremony removed: routes map straight to lambdas, DI injects
straight into parameters, and the whole host fits in one readable `Program.cs`. They are the fastest way
to stand up a real RESTful service in .NET, and everything you learn here — the builder/app split,
Kestrel, parameter binding, configuration layering — carries over unchanged when you graduate to the
controller style for larger surfaces (see `../06-aspnet-core/controllers-actions.md`).

## The Concept

### The host: builder area, app area
`dotnet new web` generates a `Program.cs` with no `Main`, no `Startup` class — top-level statements in two
sections:

```csharp
// 1. BUILDER area - register services (DI container recipes)
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LibraryDbContext>(o => o.UseSqlServer(conn));
builder.Services.AddScoped<IFulfillmentService, FulfillmentService>();

// 2. APP area - configure the pipeline + map endpoints
var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.Run();                                   // blocks; serves until shutdown
```

Everything before `Build()` teaches the container how to make things; everything after wires middleware
and endpoints. The file always ends with `app.Run()`.

### Kestrel
**Kestrel is the web server** — the cross-platform HTTP server embedded in every ASP.NET Core app. When
`app.Run()` executes, Kestrel opens the port (the familiar `Now listening on: http://localhost:5000`),
parses raw HTTP into `HttpContext` objects, and feeds them to your pipeline. You did not install IIS or
Apache; the server ships inside your process. In production Kestrel commonly sits behind a reverse proxy
(nginx, IIS, a cloud load balancer), but it is a fully capable server on its own — locally it *is* the
whole stack.

### Endpoints: MapGet / MapPost
A Minimal-API endpoint is a route pattern plus a handler. Parameters bind from wherever they plausibly
come from:

```csharp
// DI binding: LibraryDbContext comes from the container
app.MapGet("/inventory", async (LibraryDbContext db) =>
    await db.Inventory.ToListAsync());

// ROUTE binding with a constraint: /reports/rank-of/12
app.MapGet("/reports/rank-of/{units:int}", (int units, LibraryDbContext db) => ...);

// QUERY binding: /orders/burst?n=40&expedited=true  (+ services alongside)
app.MapPost("/orders/burst", (int n, bool expedited, ISeeder seeder, ...) => ...);

// BODY binding: a complex type parameter deserializes the JSON body
app.MapPost("/orders", async (OrderRequest req, OrderFactory factory, ...) => ...);
record OrderRequest(string Kind, int CustomerId, List<OrderLineRequest> Lines);
```

The rules: route parameters match `{name}` in the pattern; simple types not in the route bind from the
query string; one complex type binds from the JSON body; known service types come from DI. Return values:
a plain object serializes as `200` JSON, or use `Results.*` to choose the code explicitly
(`Results.Created`, `Results.Accepted`, `Results.NotFound`, `Results.BadRequest` — the honest-status-code
map is in `http-fundamentals.md`).

### Environment and configuration
Settings never belong in source. ASP.NET Core layers configuration providers, later overriding earlier:

```
appsettings.json  ->  appsettings.{Environment}.json  ->  environment variables  ->  command line
```

The environment name comes from `ASPNETCORE_ENVIRONMENT` (`Development` on your machine — it is what turns
on the developer exception page and Swagger by convention). Connection strings have first-class support:

```jsonc
// appsettings.json
{ "ConnectionStrings": { "Library": "Server=localhost,1433;Database=LibraryDb;..." } }
```

```csharp
var conn = builder.Configuration.GetConnectionString("Library")
    ?? "Server=localhost,1433;Database=LibraryDb;User Id=sa;Password=<local-dev-only>;TrustServerCertificate=true";
```

The environment-variable form uses `__` as the section separator: `ConnectionStrings__Library=...` — the
standard way to supply a per-machine string without editing files. The in-code fallback above is a
**local-development device** (a bare `dotnet run` works with zero setup); a production service would fail
fast instead of embedding credentials — if you keep such a fallback in a sample, label it loudly.

### Swagger while you build
The template wires `AddEndpointsApiExplorer()` + `AddSwaggerGen()` and `UseSwagger()/UseSwaggerUI()`:
browse `/swagger` and every mapped endpoint is listed and invokable — the fastest feedback loop you have
while no front end exists yet.

## Say It in an Interview
- *"A Minimal API is ASP.NET Core with routes mapped directly to handlers: `Program.cs` splits into a
  builder area that registers services and an app area that wires the pipeline and endpoints, ending in
  `app.Run()`."*
- *"Kestrel is the in-process, cross-platform HTTP server in every ASP.NET Core app — locally it's the
  whole stack; in production it usually sits behind a reverse proxy."*
- *"Handler parameters bind by plausibility: `{name}` from the route, simple types from the query string,
  one complex type from the JSON body, and registered service types from DI."*
- *"Configuration layers appsettings, per-environment appsettings, environment variables (`Section__Key`),
  then command line — later wins; secrets stay out of source and connection strings come from
  `GetConnectionString`."*

## Check Yourself
1. In `Program.cs`, what belongs before `builder.Build()` and what after?
2. What exactly is Kestrel, and why does your API serve HTTP without IIS/Apache installed?
3. For `app.MapPost("/orders/{id:int}/notes", (int id, string? tag, NoteBody body, AppDb db) => ...)` —
   where does each parameter bind from?
4. `appsettings.json` says `"Retries": 3`; the environment sets `Retries=5`. What value wins and why?
5. How do you supply `ConnectionStrings:Library` through an environment variable?
6. Why is an in-code fallback connection string acceptable in a local sample but not production?

**Answers:** (1) Before: service registrations (DI recipes); after: middleware, endpoint mappings, and
`app.Run()`. (2) The embedded cross-platform HTTP server — it ships in-process, opens the port, and parses
HTTP into `HttpContext`. (3) `id`: route; `tag`: query string (simple, not in route); `body`: JSON body
(the one complex type); `db`: DI. (4) 5 — environment variables layer after the json providers; later
providers override. (5) `ConnectionStrings__Library=...` — `__` is the section separator. (6) Production
must not embed credentials in source and should fail fast on missing config; a labeled dev fallback only
buys a zero-setup local run.

## Summary
- Minimal API = ASP.NET Core with routes mapped directly to handlers; `Program.cs` splits into builder
  (register) and app (pipeline + endpoints + `Run`).
- Kestrel is the in-process HTTP server; locally it is the entire web stack.
- Parameter binding by position of plausibility: route `{name}`, simple types from query, one complex type
  from body, services from DI; `Results.*` picks status codes.
- Configuration layers json -> per-environment json -> env vars (`Section__Key`) -> CLI;
  `GetConnectionString` reads the standard section; secrets stay out of source.

## Resources
- [Minimal APIs quick reference (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Kestrel web server](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
