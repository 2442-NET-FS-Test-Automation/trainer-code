# Reflection: Inspecting Types at Runtime

## Learning Objectives
- Explain what reflection is and what `Type` metadata exposes.
- Inspect a type's properties and methods, create an instance, and invoke a method at runtime.
- Name where reflection powers the frameworks you already use, and its costs.

## Why This Matters
Reflection is the .NET API for examining and using types *at runtime*, by name, without compile-time
knowledge of them. You will rarely write it — but you run on it constantly: EF Core reads your entity
classes' properties to build its model, ASP.NET Core binds JSON to your parameter types, AutoMapper
matches property names, and xUnit discovers your `[Fact]` methods — all reflection. Knowing the mechanism
demystifies the frameworks and equips you for the occasional legitimate direct use.

## The Concept

### Type: the entry point
Every object carries metadata about its class, exposed through `System.Type`:

```csharp
Type t = typeof(Catalogue);        // compile-time known type
Type t2 = someObject.GetType();    // runtime type of any instance
```

From a `Type` you can enumerate everything the compiler knows:

```csharp
Console.WriteLine(t.Name);                                   // "Catalogue"
Console.WriteLine(string.Join(", ",
    t.GetProperties().Select(p => $"{p.PropertyType.Name} {p.Name}")));
// "String Name, Int32 ItemCount"
```

`GetProperties()`, `GetMethods()`, `GetFields()`, `GetConstructors()` return info objects
(`PropertyInfo`, `MethodInfo`, ...) that describe members — and can *operate* on them.

### Creating and invoking without naming
A minimal end-to-end example:

```csharp
object instance = Activator.CreateInstance(t)!;              // new Catalogue(), without writing it
MethodInfo describe = t.GetMethod(nameof(Catalogue.Describe))!;
var result = describe.Invoke(instance, null);                // call it, args as object[]
// -> "Library holds 42 items"
```

`Activator.CreateInstance` builds an object from a `Type`; `MethodInfo.Invoke` calls a method found by
name; `PropertyInfo.GetValue`/`SetValue` read and write properties the same way. Strings in, objects out —
the compiler is no longer checking any of it.

### Where it powers your stack
- **EF Core**: walks `DbSet<T>` properties and entity members to build the model and materialize rows into
  objects.
- **ASP.NET Core**: model binding matches JSON fields and route values to parameter and property names;
  DI constructs your services by reflecting over constructors.
- **AutoMapper**: maps `InventoryItem` to `InventoryDto` by matching member names at configuration time.
- **Serialization**: `System.Text.Json` (source-generation aside) discovers properties reflectively.
- **Test frameworks**: xUnit finds `[Fact]`-attributed methods by scanning assemblies — attributes are
  metadata made for reflection to read.

### The costs, honestly
Reflection trades away the two things the compiler normally gives you:

- **Speed**: an `Invoke` is many times slower than a direct call (frameworks mitigate by reflecting once
  and caching — the same compute-once instinct as any startup-built lookup table).
- **Safety**: typos in member names become runtime exceptions, not build errors; refactoring tools cannot
  see string-based access (prefer `nameof(...)`, as the example does, so renames stay checked).

Rule of thumb: reach for reflection when the types genuinely are not known at compile time (plugins,
generic frameworks, tooling); otherwise write the direct code.

## Say It in an Interview
- *"Reflection is runtime access to type metadata — from a `Type` I can enumerate properties and methods,
  create instances with `Activator.CreateInstance`, and invoke members found by name."*
- *"It's the machinery under EF Core's model building, ASP.NET Core's model binding and DI, AutoMapper,
  JSON serializers, and test discovery — attributes exist to be read by reflection."*
- *"Its costs are speed and safety: invocations are much slower than direct calls and typos surface at
  runtime, so frameworks reflect once and cache, and I use `nameof` to keep renames checked."*
- *"I'd only reach for it directly when types genuinely aren't known at compile time — plugins, tooling,
  generic frameworks."*

## Check Yourself
1. Two ways to get a `Type` object — one when you know the type at compile time, one from any instance?
2. How does xUnit find your test methods without you registering them anywhere?
3. Create an instance of a type you only hold as a `Type` variable, then call its `Describe` method —
   which two APIs?
4. Name the two costs of reflection and one mitigation for each.
5. Why does model binding "just work" matching JSON fields to your parameter properties?

**Answers:** (1) `typeof(SomeType)` and `instance.GetType()`. (2) It scans assemblies with reflection for
methods carrying the `[Fact]`/`[Theory]` attributes — attributes are metadata designed for reflective
reading. (3) `Activator.CreateInstance(type)` then `type.GetMethod("Describe")` (better:
`nameof`) `.Invoke(instance, null)`. (4) Speed — reflect once and cache; safety — use `nameof(...)` so
member-name strings survive renames. (5) ASP.NET Core reflects over the target type's properties and
matches them to JSON field names at runtime.

## Summary
- Reflection = runtime access to type metadata: `Type`, `GetProperties`/`GetMethods`,
  `Activator.CreateInstance`, `MethodInfo.Invoke`.
- It is the machinery under EF, model binding, DI, AutoMapper, serializers, and test discovery.
- Costs: slower than direct calls and unchecked by the compiler — cache what you reflect, use `nameof`,
  and prefer direct code when types are known.

## Resources
- [Reflection in .NET (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/fundamentals/reflection/reflection)
- [System.Type documentation](https://learn.microsoft.com/en-us/dotnet/api/system.type)
- [Attributes and reflection](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/)
