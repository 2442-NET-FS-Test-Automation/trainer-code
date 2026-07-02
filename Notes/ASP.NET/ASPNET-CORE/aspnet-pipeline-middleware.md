# ASP.NET Core: The HTTP Pipeline, Middleware, and Filters

## Learning Objectives
- Describe the HTTP pipeline and why middleware order matters.
- Use native middleware (routing, authentication/authorization, request logging, caching, CORS).
- Write custom middleware (a global exception handler) and a custom action filter (a timing filter).
- Place ASP.NET Core in the frame: one platform, two API styles.

## Why This Matters
ASP.NET Core is the platform under both API styles — a Minimal API was never a different framework, just
the lean face of this one. The pipeline is its central idea: every request flows through an ordered chain
of middleware, and *where* a component sits decides what it sees. Get the order wrong and authentication
runs after the endpoint (useless) or caching never sees the response. A well-ordered `Program.cs` is a
complete worked example, from outermost exception handling to the endpoints.

## The Concept

### ASP.NET Core, oriented
One platform: Kestrel at the bottom (see `../02-rest-http/minimal-api-hosting.md`), the DI container, the
middleware pipeline, and at the end an **endpoint** — which may be a Minimal-API lambda or a controller
action (`controllers-actions.md`). Everything in this note applies to both styles identically.

### The pipeline
Each middleware receives the request, may act, and calls the next — then sees the **response on the way
back out**, in reverse order:

```
request ->  Exception  ->  Swagger  ->  RequestLog  ->  Caching/CORS  ->  AuthN  ->  AuthZ  ->  endpoint
response <- Exception  <-  Swagger  <-  RequestLog  <-  Caching/CORS  <-  AuthN  <-  AuthZ  <-
```

A production-shaped order, with the reasoning:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();  // OUTERMOST: catches everything below
app.UseSwagger(); app.UseSwaggerUI();
app.UseSerilogRequestLogging();                    // one structured line per request
app.UseResponseCaching();                          // before endpoints, so it can serve/store
app.UseCors(SpaCors);
app.UseAuthentication();                           // WHO are you  - sets User from the token
app.UseAuthorization();                            // MAY you      - enforces [Authorize]
app.MapControllers();                              // endpoints last
```

Three rules cover most ordering questions: exception handling **first** (it can only catch what is inside
it), authentication **before** authorization (you cannot check permissions on an unknown identity), and
everything that inspects or shapes responses **before** the endpoints that produce them.

### Native middleware
"Native" = ships with the framework; you compose it with `Use*` calls. The ones above plus the usual
suspects: `UseStaticFiles`, `UseHttpsRedirection`, `UseRouting`/`UseEndpoints` (implicit in minimal
hosting). The pair everyone asks about is `UseAuthentication()`/`UseAuthorization()` — know what each does
and their order (details in `../08-security/authentication-jwt.md`).

### Custom middleware: the global exception handler
The pattern: a class with `InvokeAsync(HttpContext, ...)` that wraps `await _next(ctx)` — this one turns
any escape into a clean JSON 500:

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _log;
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> log)
    { _next = next; _log = log; }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await _next(ctx); }                       // run the REST of the pipeline
        catch (Exception ex)
        {
            _log.LogError(ex, "Unhandled exception on {Path}", ctx.Request.Path);
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
            { error = "An unexpected error occurred.", traceId = ctx.TraceIdentifier }));
        }
    }
}
```

This is the base-case half of the exception strategy (`../05-observability-patterns/resilience-patterns.md`
is the specific half): *expected* failures are caught near their meaning and mapped to 4xx; everything else
lands here exactly once — logged with the stack, returned as a 500 that leaks nothing but a correlation id.
For quick inline middleware there is also the lambda form: `app.Use(async (ctx, next) => { /* pre */ await
next(); /* post */ });`.

### Filters: the MVC-side cousin
Middleware sees every request; **filters** run only around *controller actions*, with access to
MVC-specific context (the action, its arguments, its result). A timing filter:

```csharp
public class TimingFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sw = Stopwatch.StartNew();
        var executed = await next();          // run the action
        sw.Stop();
        executed.HttpContext.Response.Headers["X-Elapsed-ms"] = sw.ElapsedMilliseconds.ToString();
    }
}

builder.Services.AddControllers(o => o.Filters.Add<TimingFilter>());   // global: every action
```

Same wrap-the-next shape as middleware, different altitude. Filters can also be applied per-controller or
per-action as attributes, and come in flavors (action, result, exception, authorization filters). Choosing:
cross-cutting for *all* traffic (auth, logging, exceptions) -> middleware; cross-cutting for *actions
specifically*, needing model-binding or result context -> filter.

## Say It in an Interview
- *"ASP.NET Core routes every request through an ordered middleware chain; each component acts, calls the
  next, and sees the response on the way back out in reverse order — so position decides visibility."*
- *"My three ordering rules: exception handling outermost, authentication before authorization, and
  anything that shapes responses before the endpoints that produce them."*
- *"Custom middleware is a class whose `InvokeAsync` wraps `await _next(ctx)` — the canonical one is a
  global exception handler that logs the stack and returns one clean 500 with a trace id, leaking
  nothing."*
- *"Filters are action-scoped middleware with MVC context — model binding, the action result. Cross-cutting
  for all traffic goes in middleware; cross-cutting for controller actions specifically goes in a
  filter."*

## Check Yourself
1. Why must the exception-handling middleware be registered first?
2. What breaks if `UseAuthorization()` runs before `UseAuthentication()`?
3. In `InvokeAsync`, what does `await _next(ctx)` represent, and what does code after it see?
4. Expected failures (bad SKU) vs unexpected exceptions — where is each handled, and to what status code?
5. You need `X-Elapsed-ms` on every controller response but not on static files. Middleware or filter,
   and why?
6. What two pieces of information should a global 500 response include and exclude?

**Answers:** (1) Middleware can only catch exceptions thrown *inside* its `await _next` — outermost
position wraps the whole pipeline. (2) Authorization would evaluate permissions before any identity was
established — `User` is unset, so everything protected fails (or worse, passes unchecked). (3) The rest of
the pipeline; code after it runs on the response's way back out. (4) Expected: caught near their meaning,
mapped to 4xx (e.g. 400 with the offending value); unexpected: the global middleware, logged with stack,
returned as a 500. (5) A global action filter — it scopes to controller actions and has MVC context;
middleware would see all traffic. (6) Include: a generic message and a correlation/trace id. Exclude:
stack traces and internal details.

## Summary
- One platform; requests traverse ordered middleware in, endpoints at the center, responses traverse back
  out.
- Order rules: exceptions outermost, authN before authZ, response-shaping before endpoints.
- Custom middleware = constructor takes `RequestDelegate`, `InvokeAsync` wraps `await _next(ctx)`; the
  global 500 handler is the canonical one.
- Filters are action-scoped middleware with MVC context; a global timing filter (via
  `AddControllers(o => o.Filters.Add<...>)`) stamps `X-Elapsed-ms` on every controller response.

## Resources
- [ASP.NET Core middleware (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Write custom ASP.NET Core middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write)
- [Filters in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters)
