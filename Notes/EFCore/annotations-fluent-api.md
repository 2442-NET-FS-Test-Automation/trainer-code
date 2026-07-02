# Data Annotations vs the Fluent API

## Learning Objectives
- Configure a model with Data Annotations in the entity class.
- Configure the same concerns (and more) with the Fluent API in `OnModelCreating`.
- State the precedence order: convention < annotations < Fluent API.
- Recognize the cases where only the Fluent API will do.

## Why This Matters
Conventions get a typical model 80% of the way, but the load-bearing 20% — the unique index on `Sku` that
backs SKU lookups, a `RowVersion` concurrency token an oversell guard rides on, the decimal precision that
keeps prices exact — all live in explicit configuration. EF gives you three layers for it, and interviews
expect you to say which layer does what and why the Fluent API exists at all.

## The Concept

### Three layers, one precedence rule
EF builds the model by applying, in order: **conventions**, then **Data Annotations**, then the **Fluent
API**. Later layers override earlier ones. Same rule stated as advice: let convention do everything it can,
annotate what is naturally part of the class's contract, and reserve `OnModelCreating` for mappings only it
can express.

### Data Annotations: attributes on the class
Annotations sit directly on the entity, so the constraint is visible where the property is declared:

```csharp
[Table("Customers")]                      // explicit table name
public class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(100)]            // stack or comma-combine; applies to the property below
    public string Name { get; set; } = default!;

    [Required]
    public string Email { get; set; } = default!;

    public List<Order> Orders { get; set; } = new();
}

public class Product
{
    ...
    [Precision(10, 2)]                    // decimal(10,2) in SQL Server
    public decimal Price { get; set; }
}
```

Common annotations: `[Key]`, `[Required]`, `[MaxLength(n)]`/`[StringLength(n)]`, `[Table]`, `[Column]`,
`[Precision]`, `[NotMapped]`. Note annotations apply only to the property *directly below* them — there is
no fall-through.

(The same attribute family shows up again on DTOs, where it drives HTTP 400 *validation* rather than
schema — same syntax, different job; see `../06-aspnet-core/model-binding-validation.md`.)

### Fluent API: configuration in OnModelCreating
The Fluent API lives in your context's `OnModelCreating(ModelBuilder b)` and can express everything
annotations can, plus the things they cannot:

```csharp
protected override void OnModelCreating(ModelBuilder b)
{
    b.Entity<Product>(e =>
    {
        e.HasIndex(p => p.Sku).IsUnique();                    // non-key unique index
        e.Property(p => p.Price).HasColumnType("decimal(10,2)");
        e.HasOne(p => p.Inventory)                            // explicit 1:1 with FK side chosen
         .WithOne(i => i.Product)
         .HasForeignKey<InventoryItem>(i => i.ProductId);
    });

    // a concurrency token - the backbone of optimistic-concurrency guards
    b.Entity<InventoryItem>().Property(i => i.RowVersion).IsRowVersion();

    // string length FIRST, then the unique index (SQL Server: nvarchar(max) can't be indexed)
    b.Entity<Customer>().Property(c => c.Email).HasMaxLength(256);
    b.Entity<Customer>().HasIndex(c => c.Email).IsUnique();
}
```

### When only the Fluent API will do
These have no annotation equivalent — if you need one, you are in `OnModelCreating`:

- **Indexes** (`HasIndex`, unique or not) in most usable forms.
- **Composite keys**: `b.Entity<OrderLine>().HasKey(l => new { l.OrderId, l.ProductId });`
- **Choosing the FK side of a one-to-one** (`HasForeignKey<TDependent>`), configuring cascade behavior,
  many-to-many join shaping.
- **Concurrency tokens** the `IsRowVersion()` way, value conversions, `HasData` seeding.

There is a judgment call hiding in `Customer.Email` above: the `HasMaxLength(256)` *before*
`HasIndex(...).IsUnique()` is required because SQL Server will not index an `nvarchar(max)` column. Fluent
configuration is ordered code — you can express dependencies annotations cannot even state.

### Which should you use?
Both, deliberately. Class-level facts (required, lengths, precision) read best as annotations — the
constraint travels with the property. Relational topology (indexes, key shape, relationship endpoints,
concurrency tokens) belongs in `OnModelCreating`. Keep all Fluent config in one place; a model whose truth
is scattered is a model nobody can review.

## Say It in an Interview
- *"EF configures the model in three layers — conventions, then Data Annotations, then the Fluent API —
  and later layers win."*
- *"Annotations carry class-level facts like `[Required]`, `[MaxLength]`, and `[Precision]` right on the
  property; the Fluent API in `OnModelCreating` covers what annotations can't express."*
- *"Fluent-only territory: indexes, composite keys, picking the FK side of a one-to-one, cascade rules,
  `IsRowVersion()` concurrency tokens, and `HasData` seeding."*
- *"My split: annotations for the class's own contract, Fluent for relational topology — and all Fluent
  config in one place so the model stays reviewable."*

## Check Yourself
1. Convention says a property is nullable; an annotation says required; Fluent says optional. What wins?
2. Name three configurations that simply cannot be written as annotations.
3. Why does `HasIndex(c => c.Email).IsUnique()` fail on SQL Server without a length configured first?
4. `OrderLine` should be keyed by `(OrderId, ProductId)` — write the configuration in one line.
5. Where would you put "price is decimal(10,2)" vs "Sku has a unique index," and why?

**Answers:** (1) Fluent API — precedence is convention < annotations < Fluent. (2) Indexes, composite keys,
one-to-one FK-side selection (also cascade behavior, value conversions, `IsRowVersion`, `HasData`).
(3) The column defaults to `nvarchar(max)`, which SQL Server cannot index; set `HasMaxLength` first.
(4) `b.Entity<OrderLine>().HasKey(l => new { l.OrderId, l.ProductId });` (5) Precision as an annotation —
a fact of the property's own contract; the index in `OnModelCreating` — relational topology with no
annotation form.

## Summary
- Precedence: convention < Data Annotations < Fluent API; later wins.
- Annotations decorate the entity: `[Required]`, `[MaxLength]`, `[Table]`, `[Precision]` — good for facts
  that belong to the class.
- Fluent API in `OnModelCreating` handles what annotations cannot: indexes, composite keys, 1:1 FK
  selection, cascade rules, `IsRowVersion()`, `HasData`.
- Canonical examples: a unique `Sku` index, a `RowVersion` token, and the email length-then-index ordering.

## Resources
- [Creating and configuring a model (Microsoft Learn)](https://learn.microsoft.com/en-us/ef/core/modeling/)
- [Data Annotations reference](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
- [Indexes in EF Core](https://learn.microsoft.com/en-us/ef/core/modeling/indexes)
