# Integration Testing ASP.NET Core APIs with WebApplicationFactory

## Learning Objectives
- Explain what an integration test exercises that a unit test cannot: the real HTTP pipeline — routing,
  model binding, filters, middleware, and serialization.
- Boot the app in-memory with `WebApplicationFactory<Program>` and send requests via `CreateClient()`.
- Handle the `Program` accessibility wrinkle (`public partial class Program {}`) for minimal-hosting apps.
- Share one application boot per test class with `IClassFixture<>` and explain why.
- Override service registrations with `WithWebHostBuilder` + `ConfigureTestServices` — canonically,
  swapping the real database for a test one.
- Assert through HTTP: status codes, `ReadFromJsonAsync<T>`, `PostAsJsonAsync`, validation problem details.
- Say which events in a running application are vital enough to log, at which severity and in structured
  form, and why an injected `ILogger<T>` is a test seam while a static logger is not.

## Why This Matters
A unit test of a controller calls the action method directly: `controller.GetBook(42)`. That proves the
method's logic but silently skips everything ASP.NET Core does around it — routing, binding, validation,
middleware, serialization — and production bugs live disproportionately in exactly that glue. An
integration test sends a genuine HTTP request through the genuine pipeline and asserts on the genuine HTTP
response, so a green test means the whole request path works. The trade-off is cost: integration tests boot
the app and run slower, so teams keep a fast unit-test core and add integration tests for the pipeline
behavior units cannot see.

## The Concept

### What an integration test sees that a unit test cannot
When a unit test invokes an action method, there is no HTTP request at all. An integration test instead
issues `GET /api/books/42` and lets the framework do its job, which puts all of this under test:

- **Routing** — does the URL actually reach the action you think it does?
- **Model binding** — do route values, query strings, and JSON bodies bind to parameters correctly?
- **Validation** — do data annotations and `[ApiController]` produce the 400 you expect?
- **Filters and middleware** — auth, exception handling, CORS, logging: every request rides through them.
- **Serialization** — does the response JSON have the property names and shape clients depend on?

If any of those is misconfigured, unit tests stay green while the deployed API is broken.

### Booting the real app in memory: WebApplicationFactory
One package on the test project supplies the whole apparatus:

```bash
dotnet add YourProject.Tests package Microsoft.AspNetCore.Mvc.Testing
```

The test project also needs a project reference to the web application under test. It stays an ordinary
`Microsoft.NET.Sdk` project — the testing package brings the ASP.NET Core framework reference with it, so
no SDK change is required.

`Microsoft.AspNetCore.Mvc.Testing` provides `WebApplicationFactory<TEntryPoint>`. Given your
app's `Program` class, it runs the application's actual startup — real DI container, real middleware
pipeline, real routing — hosted on an in-memory `TestServer`: no network, no port, no launched process.
`CreateClient()` returns a standard `HttpClient` whose requests are delivered directly to that server.

```csharp
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class BooksApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BooksApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task GetBooks_ReturnsOkWithCatalog()
    {
        var response = await _client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        Assert.NotNull(books);
        Assert.Contains(books!, b => b.Title == "The Pragmatic Programmer");
    }
}
```

One wrinkle: minimal-hosting apps use top-level statements, and older SDKs generated an *internal*
`Program` class from them — invisible to the test project. The classic fix, which you will still see in
most codebases, is one line at the bottom of `Program.cs`:

```csharp
// Program.cs, after app.Run();
public partial class Program { }
```

Newer SDKs (including .NET 10) generate a public `Program`, so the line is no longer strictly required —
but it is harmless, and knowing why it exists explains a lot of existing test projects.

The idiomatic way to share the factory is `IClassFixture<WebApplicationFactory<Program>>`, as above:
xUnit builds it once, injects it into every test's constructor, and disposes it after the class finishes.
Booting the app is the expensive part, so one boot per class — not per test — keeps the suite fast. The
cost of sharing: every test hits the same app instance, so anything stateful (a shared database, a cached
value) must be managed deliberately.

### Swapping services for tests: WithWebHostBuilder + ConfigureTestServices
The factory boots the app with its production service registrations — including, typically, a `DbContext`
pointed at a real connection string. Tests usually want to swap that for a test database.
`WithWebHostBuilder` returns a derived factory, and `ConfigureTestServices` runs *after* the app's own
`Program` registrations, so anything you register there wins:

```csharp
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();

var testFactory = factory.WithWebHostBuilder(builder =>
{
    builder.ConfigureTestServices(services =>
    {
        // Remove the app's real database registration...
        services.RemoveAll<DbContextOptions<LibraryContext>>();
        // ...and swap in a test database.
        services.AddDbContext<LibraryContext>(o => o.UseSqlite(connection));
    });
});

var client = testFactory.CreateClient();
```

The same pattern replaces any dependency: an outbound e-mail sender, a payment gateway client, a clock.
Which test database to swap in — the InMemory provider, SQLite, or a containerized real engine — is its own
decision with real trade-offs; the comparison is covered in the EF Core testing strategies note.

### Asserting through HTTP: status codes, JSON, and validation problems
Because the client is a plain `HttpClient`, assertions are HTTP-shaped: status code first, then the body
via `System.Net.Http.Json` (`PostAsJsonAsync` to send, `ReadFromJsonAsync<T>` to read). A particularly
valuable target is validation: post an invalid body and assert the `[ApiController]`-generated 400 with
its errors dictionary — behavior no unit test of the action can see, because binding and validation happen
before the action runs.

```csharp
using Microsoft.AspNetCore.Mvc;

[Fact]
public async Task CreateBook_MissingTitle_Returns400WithTitleError()
{
    var response = await _client.PostAsJsonAsync("/api/books",
        new { Title = "", Author = "Anonymous" });

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
    Assert.NotNull(problem);
    Assert.Contains("Title", problem!.Errors.Keys);
}
```

### Middleware and filters ride along — and how to target them
Every request the test client sends traverses the full pipeline, so global filters and middleware are
implicitly exercised by every test: if your exception middleware were broken, the catalog test above would
fail too. For a *targeted* proof, write a request whose expected outcome only that component can produce —
classically, an unauthenticated request to a protected endpoint asserting 401 proves the authentication
middleware is wired and ordered correctly, with no mocking and no reaching into internals.

```csharp
[Fact]
public async Task DeleteSupplier_NoToken_Returns401()
{
    var response = await _client.DeleteAsync("/api/suppliers/1");
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}
```

The standard follow-up question is where in-memory testing stops: `TestServer` skips real sockets, TLS,
and HTTP.SYS/Kestrel behavior, so for true end-to-end coverage (real network, real browser) teams run
Kestrel on a real port or drive the deployed app with an E2E tool — a small top layer above a large
integration-test middle.

### Logging vital events, and why the logger is a test seam
An application under test is also an application someone has to operate, and the two concerns meet at the
logger. **Vital events** are the ones needed to explain the system's behavior after the fact: startup and
its configuration, authentication failures, rejected or short-circuited requests, unhandled exceptions,
failed calls to external services, and state changes that money or compliance depends on. Noise is
everything else — per-iteration debug spam, whole request bodies, and above all secrets: passwords,
tokens, card numbers. A log that echoes request bodies leaks credentials into a file by accident.

Severity is the filter that decides what production keeps and what wakes someone: `ILogger` exposes
Trace, Debug, Information, Warning, Error and Critical (Serilog names the same ladder Verbose, Debug,
Information, Warning, Error, Fatal). Routine request accounting is Information; a request the system
deliberately refused is Warning; an unhandled exception is Error. Structure matters as much as level —
`_log.LogWarning("Rejected {Method} {Path} because {Reason}", method, path, reason)` keeps the arguments
as queryable properties, whereas the interpolated `$"Rejected {method} {path}"` flattens to one string
nobody can search on.

For testing, the shape of the dependency is what counts:

```csharp
public class MaintenanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MaintenanceMiddleware> _log; // injected: a test can substitute this
    // ...
}
```

An injected `ILogger<T>` is a **seam** — the same kind `ConfigureTestServices` exploits. A test can pass
`NullLogger<T>.Instance` to silence it, or a capturing fake to assert that the right event fired at the
right level (a common requirement for audit-relevant events). A static, process-global logger is not a
seam: even when the static property is assignable, it is shared across parallel tests and reassigned by
application startup, so no test can control it per-case. The rule that follows is short — inject
`ILogger<T>` into anything you intend to test, and keep static loggers to composition-root code. Depth on
sinks, message templates and enrichment belongs to the structured-logging note; what matters here is that
observability and testability are the same design decision seen from two sides.

### Common failures and what they mean
Integration tests fail in ways unit tests never do, because a real startup pipeline is running.

**`'Program' is inaccessible due to its protection level`.** This is the top-level-statements wrinkle
described earlier, seen from the compiler's side: add `public partial class Program { }` to `Program.cs`.

**The factory throws during construction, somewhere deep in startup.** `WebApplicationFactory` runs your
*real* `Program`, so any startup requirement the app has, the test now has too: a configuration key that
only exists in a development settings file, a connection string, a certificate. A `NullReferenceException`
or "value cannot be null" from inside DI registration almost always means missing configuration, not
broken code. The factory defaults to the Development environment; if you call `UseEnvironment` with
anything else, you must supply what that environment expects through test configuration.

**Tests hang, or fail with connection errors, before any assertion runs.** The in-memory server is only
in-memory at the *HTTP* boundary — everything behind it is real. If startup seeds data or the app resolves
a database on boot, that database must be reachable before the first test. Hangs here are usually a
connection timeout, not a deadlock.

**A `401` where you expected a `400`.** Pipeline order. Authorization runs before model validation, so an
unauthenticated request with a deliberately invalid body is rejected for the wrong reason and your
validation assertion never gets its chance. Authenticate first, then assert on validation.

**One failing test poisons later ones.** A test that creates a row and deletes it at the end only deletes
when every assertion before the delete passed; a mid-test failure leaks the row, and the next run fails on
a unique constraint instead of the original defect. Clean up in `Dispose` (which runs regardless), give
each test its own keys, or reset state between tests rather than after.

**The suite got slower after you introduced a shared fixture.** Sharing a server or a database across
classes via `ICollectionFixture` also serializes those classes — xUnit will not parallelize within a
collection. That is the deliberate price of sharing, not a regression; if the throughput matters more than
the sharing, give each class its own factory and pay the boot cost instead.

## Say It in an Interview
- *"Logging and testing meet at the seam: I inject `ILogger<T>` so a test can pass a null logger or a
  capturing fake and assert the vital event fired at the right level. Vital events are the ones you need
  to explain the system afterwards — auth failures, refused requests, unhandled exceptions, external-call
  failures — logged with structured properties, never secrets, and never as an interpolated string."*
- *"A unit test calls the action method directly, so it never sees routing, model binding, validation,
  filters, or middleware. An integration test sends a real HTTP request through the real pipeline, so it
  catches the glue bugs unit tests can't."*
- *"I use `WebApplicationFactory<Program>` — it boots the actual app on an in-memory TestServer, no port
  or process, and `CreateClient()` gives me an `HttpClient` wired to it. I share the factory per class
  with `IClassFixture` because booting the app is the expensive part."*
- *"To point tests at a test database I call `WithWebHostBuilder` with `ConfigureTestServices`, remove the
  real `DbContextOptions` registration, and add a test one — it runs after the app's own registrations, so
  it wins."*
- *"Assertions are HTTP-shaped: status code, then `ReadFromJsonAsync<T>` on the body. Posting an invalid
  body and asserting the 400 `ValidationProblemDetails` tests validation itself; an unauthenticated
  request asserting 401 proves the auth middleware."*

## Check Yourself
1. Name three things an integration test through `WebApplicationFactory` exercises that a direct unit
   test of the same controller action does not.
2. Why did test projects historically need `public partial class Program { }`, and why is it still common?
3. Why share the factory via `IClassFixture` instead of newing one up per test, and what caution comes
   with sharing?
4. How do you replace the app's real database registration for tests, and why does your registration win?
5. How would you write a targeted test proving the authentication middleware is in place?
6. A middleware refuses a request and returns 503 without logging anything. Say what you would add, at
   which level, in what form — and what makes that logger testable.

**Answers:** (1) Any three of: routing, model binding, `[ApiController]` validation, filters, middleware,
and response serialization — none run when you call the action method directly. (2) Top-level statements
generated an *internal* `Program` on older SDKs, so the test project could not reference it; the
partial-class line made it public. .NET 10 generates a public `Program`, but the idiom persists and is
harmless. (3) Booting the app is the slow part; `IClassFixture` gives one boot per class instead of per
test — at the cost that tests share one app instance, so shared state must be managed deliberately.
(4) `factory.WithWebHostBuilder(b => b.ConfigureTestServices(services => ...))` — `RemoveAll` the real
`DbContextOptions<T>` and add the test registration; `ConfigureTestServices` runs after the app's own
startup registrations, so the last registration wins. (5) Send a request to a protected endpoint with no
credentials and assert `HttpStatusCode.Unauthorized` — only the auth middleware can produce that outcome.
(6) A `LogWarning` naming the reason alongside the method and path as structured properties
(`"Refused {Method} {Path}: {Reason}"`), because a deliberate refusal is not routine traffic but is not
an error either; it is testable when the middleware takes `ILogger<T>` by constructor injection, so a
test can supply `NullLogger<T>.Instance` or a capturing fake and assert on the event.

## Summary
- Integration tests cover the HTTP pipeline — routing, model binding, validation, filters, middleware,
  serialization — which unit tests of action methods never touch.
- `WebApplicationFactory<Program>` boots the real app in-memory on a `TestServer` — no network, no port,
  no launched process; `CreateClient()` returns an `HttpClient` wired to it.
- `public partial class Program { }` fixed the internal-`Program` wrinkle of top-level statements; .NET 10
  generates a public `Program`, but the idiom remains common and harmless.
- `IClassFixture<WebApplicationFactory<Program>>` shares one app boot per class — fast, but shared state
  then needs deliberate handling.
- `WithWebHostBuilder` + `ConfigureTestServices` overrides production registrations (canonically: swap the
  real `DbContextOptions` for a test database); it runs after app startup, so it wins.
- Assert HTTP outcomes: `HttpStatusCode`, `ReadFromJsonAsync<T>`, `PostAsJsonAsync`, and
  `ValidationProblemDetails` for the 400-with-errors shape.
- Middleware and filters ride along in every request; targeted tests (401 on a protected route) prove a
  specific component. `TestServer` stops short of real sockets/TLS — that is E2E territory.
- Log vital events (startup config, auth failures, refused requests, unhandled exceptions, external-call
  failures) at a meaningful severity with structured properties and no secrets; inject `ILogger<T>` so
  the logger is a seam a test can substitute, and keep static loggers out of testable classes.

## Resources
- [Integration tests in ASP.NET Core (learn.microsoft.com)](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Test ASP.NET Core middleware (learn.microsoft.com)](https://learn.microsoft.com/en-us/aspnet/core/test/middleware)
- [Shared Context between Tests — xUnit fixtures (xunit.net)](https://xunit.net/docs/shared-context)
