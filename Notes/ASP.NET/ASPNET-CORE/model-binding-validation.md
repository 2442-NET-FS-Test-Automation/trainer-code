# Model Binding and Validation Annotations

## Learning Objectives
- Explain where model binding pulls values from (route, query, body, headers, DI) and how it decides.
- Use the `[From*]` attributes when inference is not enough.
- Validate input with Data Annotations and explain the automatic 400 behavior under `[ApiController]`.

## Why This Matters
Model binding is the invisible half of every endpoint you write: `{sku}` in a route template becoming a
`string sku` parameter, a JSON body becoming an `OrderRequest`, `?n=40` becoming `int n`. It works
silently until it does not — a parameter binds from the wrong place, a body deserializes to nulls — and
then you need the rules. Validation is its gatekeeper twin: the same annotation syntax used on entities,
now guarding the API boundary so garbage input dies as a clean 400 before your code runs.

## The Concept

### The binding sources, in lookup order
For each parameter of an action (or Minimal-API handler), the framework asks: where does this come from?

| Source | Wins for | Example |
|---|---|---|
| Route values | simple types matching a `{token}` | `[HttpGet("{sku}")]` -> `string sku` |
| Query string | remaining simple types | `?n=40&expedited=true` -> `int n, bool expedited` |
| Body (JSON) | complex types (one per request) | `InventoryCreateDto dto`, `OrderRequest req` |
| DI container | known service types | `LibraryDbContext db`, `ISeeder seeder` |
| Headers | only when asked | `[FromHeader] string userAgent` |

A burst-style endpoint can exercise three sources in one signature:
`(int n, bool expedited, ISeeder seeder, IServiceScopeFactory scopes, IHostApplicationLifetime lifetime)`
— two from query, three from DI, decided entirely by inference.

### The [From*] attributes
When inference guesses wrong or you want the contract explicit:

```csharp
[HttpGet("{id}")]
public IActionResult Get(
    [FromRoute]  int id,            // explicit, though inference would agree
    [FromQuery]  int page = 1,      // ?page=2
    [FromHeader(Name = "X-Api-Version")] string? version = null)
    => Ok(...);

[HttpPost]
public IActionResult Create([FromBody] InventoryCreateDto dto) => ...;   // explicit body

// [FromServices] pulls DI into a single action instead of the constructor:
[HttpGet("{sku}/supplier-price")]
public async Task<ActionResult<object>> SupplierPrice(
    string sku, [FromServices] ISupplierClient supplier, CancellationToken ct) => ...;
```

Notes that save debugging time: only **one** parameter may bind from the body (JSON is a stream, read
once); `[ApiController]` infers `[FromBody]` for complex types automatically; binding is type-converting
(`"40"` -> `int 40`) and a *failed conversion* on a route/query simple type is itself a 400; and
`CancellationToken` is special-cased — it binds to the request's abort token with no attribute at all.

### Validation annotations
The same `System.ComponentModel.DataAnnotations` attributes used on entities
(`../01-efcore/annotations-fluent-api.md`), now on the *incoming DTO*, where they mean "reject the
request," not "shape the column":

```csharp
public record InventoryCreateDto(
    [Required, MaxLength(20)]  string Sku,
    [Required, MaxLength(200)] string Name,
    [Range(0.01, 100000)]      decimal Price,
    [Range(0, int.MaxValue)]   int CurrentStock);
```

(On record positional parameters the annotations stay on the CONSTRUCTOR PARAMETER — a `[property:]` target
aims them at the generated property, where MVC refuses them and throws at request time.) The working set: `[Required]`, `[MaxLength]`/`[MinLength]`/`[StringLength]`, `[Range]`,
`[EmailAddress]`, `[RegularExpression]`, and `[CustomValidation]`/`IValidatableObject` when a rule spans
properties.

### The automatic 400
After binding, the framework validates the bound model into `ModelState`. Under `[ApiController]`, an
invalid `ModelState` **short-circuits before your action runs**, returning a structured 400:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Sku": ["The Sku field is required."],
              "Price": ["The field Price must be between 0.01 and 100000."] }
}
```

The division of labor worth stating out loud: **annotations validate shape** (present, in range, right
format) and produce automatic 400s; **domain rules validate meaning** (does this SKU exist?) and live in
your code — a factory's `UnknownSkuException` -> 400 path
(`../05-observability-patterns/resilience-patterns.md`). Same status code, two deliberate layers. Without
`[ApiController]` (or in Minimal APIs, which run annotation validation on request DTOs in current .NET but
historically did not) the manual form is `if (!ModelState.IsValid) return BadRequest(ModelState);`.

## Say It in an Interview
- *"Binding resolves each parameter by plausibility: route tokens first, then query string for simple
  types, one complex type from the JSON body, service types from DI, headers only when asked — and
  `[From*]` attributes make it explicit when inference isn't enough."*
- *"Only one parameter can bind from the body — it's a stream, read once. And a failed type conversion on
  a route or query value is already a 400 before my code runs."*
- *"Validation annotations on the incoming DTO guard shape — required, range, length — and under
  `[ApiController]` an invalid model short-circuits into a structured 400 automatically."*
- *"I keep two validation layers on purpose: annotations for shape, domain code for meaning — 'price must
  be positive' is an attribute; 'this SKU exists' is a lookup."*

## Check Yourself
1. `(int id, string? filter, OrderRequest req, AppDb db)` on `[HttpPost("{id}")]` — bind each parameter.
2. Why can't two complex parameters both bind from the body?
3. A client sends `?n=abc` to `(int n, ...)`. What happens, and before or after your handler runs?
4. `[Required]` on a DTO property vs `[Required]` on an entity property — same attribute, different
   effect. State both effects.
5. Where does "SKU must exist in the catalog" belong — annotation or code — and why?
6. What does `[ApiController]` do with an invalid `ModelState`, and what is the manual equivalent?

**Answers:** (1) `id`: route; `filter`: query; `req`: JSON body; `db`: DI. (2) The request body is a
stream consumed once during deserialization — there is nothing left for a second reader. (3) Type
conversion fails and the framework returns 400 — before the handler executes. (4) DTO: reject the request
(validation -> 400); entity: make the column non-nullable (schema shape). (5) Code (factory/service) — it
requires data access and is a domain fact, not an input shape; annotations cannot know the catalog.
(6) Short-circuits before the action with a structured 400 listing per-field errors;
`if (!ModelState.IsValid) return BadRequest(ModelState);`.

## Summary
- Binding sources in order of plausibility: route tokens, query, one JSON body (complex type), DI, headers
  on request; `[From*]` makes it explicit when inference fails.
- One body parameter max; failed simple-type conversion is already a 400; `CancellationToken` binds
  automatically.
- Validation annotations on DTOs guard shape; `[ApiController]` turns violations into structured 400s
  before the action executes.
- Shape validation (annotations) and domain validation (factory/exceptions) are separate deliberate
  layers.

## Resources
- [Model binding in ASP.NET Core (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding)
- [Model validation in ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/#automatic-http-400-responses)
- [System.ComponentModel.DataAnnotations reference](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
