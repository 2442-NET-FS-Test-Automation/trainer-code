# Controllers and Action Methods

## Learning Objectives
- Implement a controller with action methods and explain each annotation on it.
- Describe the function of HTTP method annotations (`[HttpGet]`, `[HttpPost]`, ...) and attribute routing.
- Return effective response codes with the `ControllerBase` helpers.
- Compare Minimal APIs and controllers and choose between them.

## Why This Matters
Controllers are the organizational style of most production ASP.NET Core APIs: classes grouping related
actions, attribute routing, the full filter pipeline. If you learned the platform through Minimal APIs,
learn controllers as a *rewrite*, not a new framework — same DI, same EF context, same middleware; both
styles can serve side by side in one process. Every concept maps one-to-one onto what you already know.

## The Concept

### The anatomy
An inventory controller, annotated line by line:

```csharp
[ApiController]                          // API behaviors: auto-400 on invalid model, body inference
[Route("api/[controller]")]              // token -> class name minus "Controller": /api/inventory
public class InventoryController : ControllerBase        // ControllerBase = API base (no views)
{
    private readonly IInventoryService _service;             // constructor DI, same container as before
    private readonly IMapper _mapper;
    public InventoryController(IInventoryService query, IMapper mapper)
    { _service = query; _mapper = mapper; }

    [HttpGet]                                            // GET /api/inventory
    public async Task<ActionResult<IEnumerable<InventoryDto>>> Get()
        => Ok(_mapper.Map<List<InventoryDto>>(await _service.AllAsync()));

    [HttpGet("{sku}")]                                   // GET /api/inventory/BK-001
    public async Task<ActionResult<InventoryDto>> GetBySku(string sku)
    {
        var item = await _service.BySkuAsync(sku);
        return item is null ? NotFound() : Ok(_mapper.Map<InventoryDto>(item));   // 404 vs 200
    }

    [HttpPost]                                           // POST /api/inventory
    public async Task<ActionResult<InventoryDto>> Create(InventoryCreateDto dto)
    {
        var created = await _service.AddAsync(dto);
        var read = _mapper.Map<InventoryDto>(created);
        return CreatedAtAction(nameof(GetBySku), new { sku = read.Sku }, read);   // 201 + Location
    }

    [HttpDelete("{sku}")]                                // DELETE /api/inventory/BK-001
    public async Task<IActionResult> Delete(string sku)
        => await _service.RemoveAsync(sku) ? NoContent() : NotFound();              // 204 vs 404
}
```

An **action method** is a public method on a controller that handles requests. `ControllerBase` (not
`Controller`, which adds view support you do not want in an API) supplies the helpers; `[ApiController]`
turns on API conventions — most visibly automatic 400s from validation (`model-binding-validation.md`).

### HTTP method annotations
In one sentence: **the attribute maps an action to a verb and a route template**, combining with the
controller's `[Route]`:

| Annotation | Route | Meaning |
|---|---|---|
| `[HttpGet]` | `/api/inventory` | list |
| `[HttpGet("{sku}")]` | `/api/inventory/{sku}` | read one; `sku` binds from the route |
| `[HttpPost]` | `/api/inventory` | create |
| `[HttpPut("{sku}")]` / `[HttpPatch("{sku}")]` | `/api/inventory/{sku}` | replace / partial update |
| `[HttpDelete("{sku}")]` | `/api/inventory/{sku}` | remove |

This is *attribute routing* — the route lives on the code it routes to. The `[controller]` token keeps the
prefix in one place; constraints work like Minimal APIs (`{id:int}`).

### Effective response codes
`ControllerBase` helpers make status codes read like intent — the controller counterpart of `Results.*`:

- `Ok(body)` 200 · `CreatedAtAction(actionName, routeValues, body)` 201 with a `Location` header pointing
  at the resource you can now GET · `NoContent()` 204 for successful deletes · `NotFound()` 404 ·
  `BadRequest(body)` 400 · `Conflict()` 409.
- Return type `ActionResult<T>` means "either a `T` (implicitly 200) or any status result" — the
  `GetBySku` pattern above is the everyday shape: null-check, `NotFound()` or `Ok(dto)`.

`CreatedAtAction(nameof(GetBySku), new { sku = read.Sku }, read)` is worth memorizing whole: it emits 201,
sets `Location: /api/inventory/BK-004`, and returns the created representation — the honest create
response (`../02-rest-http/http-fundamentals.md` has the full code map).

### Minimal API vs controllers — when each
Same platform, same DI, same middleware, same binding engine. The differences are organizational:

| | Minimal API | Controllers |
|---|---|---|
| Shape | routes + lambdas in `Program.cs` (or grouped) | classes grouping related actions |
| Ceremony | least | more, but self-organizing at scale |
| Filters | endpoint filters | the full action-filter pipeline (e.g. a timing filter) |
| Fit | small services, few endpoints | larger API surfaces, team codebases |

Rules of thumb: a handful of endpoints or a focused microservice — Minimal API stays clearer; a growing
surface with cross-cutting per-action behavior and a front-end team consuming it — controllers pay off.
They coexist in one app (`MapControllers()` beside `MapGet`s), so the rewrite is a choice of
*organization*, not a migration of platform. The wiring that does not change when you rewrite: DI
registrations, the EF context, middleware, configuration — which is exactly why such a rewrite fits in a
single working session.

## Say It in an Interview
- *"An API controller is `[ApiController]` + `[Route("api/[controller]")]` on a class deriving from
  `ControllerBase` — not `Controller`, which drags in view support. Actions are its public handler
  methods, and dependencies arrive by constructor injection."*
- *"HTTP method annotations map an action to a verb plus a route template that composes with the class
  route — that's attribute routing: the route lives on the code it routes to."*
- *"For status codes I use the helpers as intent: `Ok`, `CreatedAtAction` for 201 with a `Location`
  header, `NoContent` for deletes, `NotFound`, `BadRequest`. `ActionResult<T>` lets an action return
  either the payload or any status."*
- *"Minimal APIs and controllers are one platform, two organizations — same DI, middleware, and binding.
  Few endpoints: minimal stays clearer; a growing surface with per-action cross-cutting behavior:
  controllers pay off. They coexist in one process."*

## Check Yourself
1. Why `ControllerBase` rather than `Controller` for an API, and what two behaviors does
   `[ApiController]` add?
2. With `[Route("api/[controller]")]` on `OrdersController`, what URL does `[HttpGet("{id:int}")]` serve?
3. Write the create-action return that emits 201, a `Location` header, and the created body.
4. What does `ActionResult<InventoryDto>` allow an action to return that `InventoryDto` alone would not?
5. Name three things that do *not* change when you rewrite a Minimal-API surface as controllers.
6. Delete succeeded / delete target missing — which two codes?

**Answers:** (1) `Controller` adds view rendering an API never uses; `[ApiController]` adds automatic 400
on model-validation failure and body-binding inference (among other conventions). (2)
`GET /api/orders/{id}` with an integer constraint — e.g. `/api/orders/42`. (3)
`return CreatedAtAction(nameof(GetById), new { id = read.Id }, read);` (4) Any status result
(`NotFound()`, `BadRequest()`...) *or* the typed payload (implicit 200). (5) DI registrations, the EF
context/data layer, middleware pipeline, configuration (also the binding engine). (6) 204 `NoContent` /
404 `NotFound`.

## Summary
- `[ApiController]` + `Route("api/[controller]")` + `ControllerBase` + constructor DI = the API controller
  skeleton; actions are its public handler methods.
- Method annotations map verb + route template onto actions; attribute routing composes with the class
  route.
- Status helpers say intent: `Ok`, `CreatedAtAction` (201 + Location), `NoContent`, `NotFound`,
  `BadRequest`; `ActionResult<T>` carries either.
- Minimal APIs and controllers are one platform, two organizations; both run side by side in one process.

## Resources
- [Create web APIs with ASP.NET Core (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [Routing to controller actions](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing)
- [Minimal APIs vs controller-based APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis)
