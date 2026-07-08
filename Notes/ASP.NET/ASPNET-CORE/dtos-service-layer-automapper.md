# DTOs, the Service Layer, and AutoMapper

## Learning Objectives
- Explain what Data Transfer Objects are for and the over-posting problem they prevent.
- Implement an API service behind an interface and defend the separation of concerns.
- Map entities to DTOs automatically with AutoMapper.

## Why This Matters
Returning EF entities directly from an API welds your public contract to your database schema: rename a
column and every client breaks; expose `Order` and you expose `RowVersion` and whatever else the entity
grows. The mature boundary is a three-piece discipline — DTOs shape what crosses, a service owns the
operations, AutoMapper removes the copying boilerplate. Every front end that consumes your API consumes it
through this boundary, and "why DTOs?" is a bread-and-butter interview question.

## The Concept

### DTOs: the public shape
A **Data Transfer Object** is a type that exists only to carry data across the API boundary. Define two,
and the asymmetry is the design:

```csharp
// OUT: what readers get - flattened (Sku/Name live on Product, the entity needs a join)
public record InventoryDto(string Sku, string Name, int CurrentStock);

// IN: what creators send - only what a client MAY set, with validation attached
public record InventoryCreateDto(
    [Required, MaxLength(20)]  string Sku,
    [Required, MaxLength(200)] string Name,
    [Range(0.01, 100000)]      decimal Price,
    [Range(0, int.MaxValue)]   int CurrentStock);
```

What DTOs buy:

- **Decoupling.** The entity can change shape (new columns, renamed properties) without breaking the
  public contract — and vice versa.
- **No leaked internals.** `RowVersion`, FKs, navigation cycles (`InventoryItem.Product.Inventory...`
  would not even serialize) stay home.
- **Over-posting protection.** If the create endpoint bound the *entity*, a malicious client could POST
  `{"id": 7, "rowVersion": ...}` and set fields you never offered. The DTO makes the settable surface
  explicit — a client can only send what `InventoryCreateDto` declares.
- **Read/write asymmetry.** What you return (`InventoryDto` has no Price here) and what you accept
  (`InventoryCreateDto` has no Id) are different shapes because they are different concerns.

`record` is the natural DTO vehicle: immutable, value-equal, one line.

### The API service behind an interface
"Implement an API service" means: the controller does not do the work — it delegates to a service
registered behind an interface:

```csharp
public interface IInventoryService
{
    Task<IReadOnlyList<InventoryItem>> AllAsync();
    Task<InventoryItem?> BySkuAsync(string sku);
    Task<InventoryItem> AddAsync(InventoryCreateDto dto);
    Task<bool> RemoveAsync(string sku);
}

builder.Services.AddScoped<IInventoryService, InventoryService>();   // DI: interface -> implementation
```

The layering is deliberately legible: **controller** (HTTP concerns: routes, status codes, DTO mapping) ->
**`IInventoryService`** (the API's operations) -> **`IInventoryRepository`** (persistence,
`../05-observability-patterns/resilience-patterns.md`) -> EF. Each layer has one reason to change — that
is **separation of concerns** as an architecture answer, not a slogan: HTTP changes touch the controller,
operation changes touch the service, query changes touch the repository. And because the controller
depends on an *interface*, tests can hand it a fake service and exercise HTTP behavior without a database.

### AutoMapper: the copying, automated
Entity-to-DTO copying is mechanical (`new InventoryDto(item.Product.Sku, item.Product.Name,
item.CurrentStock)` — times every endpoint, times every shape change). AutoMapper does it by convention,
configured once in a `Profile`:

```csharp
public class MappingProfile : Profile
{
    public MappingProfile() =>
        CreateMap<InventoryItem, InventoryDto>()
            .ForCtorParam("Sku",  o => o.MapFrom(s => s.Product.Sku))    // flatten the nav property
            .ForCtorParam("Name", o => o.MapFrom(s => s.Product.Name));
}

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
```

```csharp
// in the controller (IMapper injected):
Ok(_mapper.Map<List<InventoryDto>>(await _service.AllAsync()));
```

Same-named members map automatically; you configure only the exceptions (here: the flattening from
`Product`). Two pieces of craft: keep profiles *thin* — mapping config, not business logic; and know that
convention-magic has a failure mode (a renamed property silently maps nothing), so
`MapperConfiguration.AssertConfigurationIsValid()` in a test is standard insurance. For small services,
hand-written mapping methods are a legitimate alternative — AutoMapper earns its keep as shapes and
endpoints multiply.

## Say It in an Interview
- *"A DTO carries data across the API boundary so the public contract and the database schema can evolve
  independently — internals like concurrency tokens and navigation cycles stay home."*
- *"Binding an entity on create invites over-posting: a client could set fields you never offered, like
  the id or role. A create DTO makes the settable surface explicit."*
- *"I keep read and write DTOs asymmetric on purpose — what you return and what you accept are different
  concerns."*
- *"The controller handles HTTP, a service behind an interface owns the operations, a repository owns
  persistence — one reason to change per layer, and the interface is the seam tests fake."*
- *"AutoMapper maps same-named members by convention; I configure only exceptions like flattening,
  keep profiles free of logic, and run `AssertConfigurationIsValid` in a test because a silent unmapped
  property is its failure mode."*

## Check Yourself
1. What breaks, concretely, when an API returns EF entities directly? Name three problems.
2. Explain over-posting with an example, and the mechanism that prevents it.
3. Why are the read DTO and the create DTO different shapes?
4. In controller -> service -> repository, which layer changes when you rename a route? Add a business
   rule? Optimize a query?
5. What silent failure does AutoMapper introduce, and what standard test guards it?

**Answers:** (1) Contract welded to schema (renames break clients), leaked internals (`RowVersion`, FKs),
navigation cycles that fail serialization — plus over-posting on the write side. (2) POSTing
`{"id":7,"isAdmin":true}` against an endpoint that binds the entity sets fields never offered; a create
DTO declares exactly what is settable, so unbindable fields cannot arrive. (3) Different concerns: readers
get a flattened view (no settable fields); creators may only send permitted inputs (no id, validation
attached). (4) Controller; service; repository — one reason to change per layer. (5) A renamed property
maps nothing without error; `MapperConfiguration.AssertConfigurationIsValid()` in a unit test.

## Summary
- DTOs define the public contract: flattened reads, minimal validated writes, no leaked internals, no
  over-posting.
- The API service behind an interface separates HTTP from operations from persistence — one reason to
  change per layer, and a fakeable seam for tests.
- AutoMapper maps by matching names, configured once per profile; configure exceptions, keep logic out,
  validate the config in tests.
- Together they are the boundary discipline every consumed API is built on.

## Resources
- [Create Data Transfer Objects (Microsoft Learn Web API tutorial)](https://learn.microsoft.com/en-us/aspnet/web-api/overview/data/using-web-api-with-entity-framework/part-5)
- [AutoMapper documentation](https://docs.automapper.org/en/stable/)
- [Common web application architectures (Microsoft architecture docs)](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
