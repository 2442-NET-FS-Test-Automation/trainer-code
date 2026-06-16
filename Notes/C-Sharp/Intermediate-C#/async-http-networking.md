# Networking, Async & Language Round-out

## Learning Objectives
- Call an HTTP API with a shared `HttpClient` and `async`/`await`, and explain why I/O work goes async.
- Deserialize a JSON response and build your domain object from the fields you read.
- Run independent awaits concurrently with `Task.WhenAll`.
- Validate input shape with `Regex`, and use `out` parameters, nullable value types with lifted operators, and recognize boxing.

## Why This Matters
This is the T2 finale: today the `core-csharp-kata` console app becomes feature-complete. Everything so far ran on one thread, top to bottom — fine until you call the network, where a request takes hundreds of milliseconds and a blocked thread sits there doing nothing. Real applications pull data from somewhere, and doing that without freezing is the difference between a responsive program and a stalled one. Pulling real book data over HTTP, deserializing it, validating it, and overlapping the waits is exactly the shape every API client, integration test, and microservice you will write later takes. The repository, factory, and HTTP seams you finish today are precisely where a real database plugs in next week — so this lesson is both an ending and a hinge into persistence.

## The Concept

### Sync vs async: don't block on waiting
There are two kinds of work, and they want different tools:

```
CPU-bound work       busy computing            threads help (parallelism)
I/O-bound work       waiting on network/disk   async helps (don't block the thread)

sync  : call --[ thread BLOCKED waiting ]-- result
async : call --await--> (thread freed) ... (resumes) --> result
```

`async`/`await` is about not blocking while waiting, not about computing faster. While a network request is in flight, `await` hands the thread back so other work runs. Because `HttpClient` is async-first (its methods return `Task<...>`), `Main` itself becomes `async Task`.

### `HttpClient` + `async`/`await`
Use **one** `HttpClient` for the whole process — a new one per call leaks OS sockets and is the single most common `HttpClient` bug:

```csharp
public class OpenLibraryClient
{
    private static readonly HttpClient Http = new();   // shared, not per-call

    public async Task<LibraryItem?> FetchByIsbnAsync(string isbn)
    {
        string url = $"https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&jscmd=data&format=json";
        try
        {
            string json = await Http.GetStringAsync(url);   // await unwraps Task<string>
            return Parse(json);
        }
        catch (HttpRequestException ex)
        {
            Log.Warning("Network fetch failed for {Isbn}: {Message}", isbn, ex.Message);
            return null;     // a foreseeable failure, handled near the call
        }
    }
}
```

Read `await` like blocking code; it simply does not block. An `async` method returns a `Task`, and `Task<T>` carries a result. The `Async` suffix is the naming convention for awaitable methods. A few hard rules: never call `.Result` or `.Wait()` (they deadlock and hide errors) — `await` all the way up to `Main`; and never use `async void` except for event handlers, because its exceptions vanish.

### Deserialize JSON, then build your domain object
Deserialize the response into a built-in shape, read the fields you need, and build your own type from them:

```csharp
var map = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);  // keyed by "ISBN:xxxx"
foreach (JsonElement book in map!.Values)
{
    string title  = book.GetProperty("title").GetString() ?? "Untitled";
    string author = book.GetProperty("authors")[0].GetProperty("name").GetString() ?? "Unknown";
    return LibraryItemFactory.Create(ItemKind.Book, title, author);           // build through the factory
}
```

You read each field straight off the JSON by name (`GetProperty("title")`), so the names must match the payload — verify against the real response. Build *your* `Book` through the one place that makes items (the factory from Tuesday).

### Concurrency with `Task.WhenAll`
Sequential awaits are still serial. To overlap independent waits, launch the tasks first, then await them together:

```csharp
Task<LibraryItem?>[] fetches = new Task<LibraryItem?>[isbns.Length];
for (int i = 0; i < isbns.Length; i++)
    fetches[i] = client.FetchByIsbnAsync(isbns[i]);   // each starts immediately

LibraryItem?[] fetched = await Task.WhenAll(fetches); // one await, both overlap
```

Each `FetchByIsbnAsync` returns a `Task` right away and begins running; `Task.WhenAll` completes when all of them do and returns the results in order. Awaiting *inside* the loop instead would run them one after another — that is the "optimizing with async" point.

### Regex validation and pattern matching
Validate input *shape* before spending a network call on garbage:

```csharp
bool validIsbn  = Regex.IsMatch(isbn,  @"^\d{13}$");                    // exactly 13 digits
bool validEmail = Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");  // shape of an email
```

Use a verbatim string `@"..."` so backslashes stay literal (`\d` not `\\d`). Anchor with `^` and `$` so the *whole* string must match — without them a partial match passes, a common bug. A pattern-matching `switch` branches on the runtime type and binds a typed variable in one move:

```csharp
string shelf = item switch
{
    ReferenceBook r => $"Reference: {r.Section}",
    Book b          => $"Lending ({b.CopiesAvailable} copies)",
    Magazine        => "Periodicals",
    _               => "Unsorted"
};
```

When behavior belongs on the type, prefer a `virtual` method; when the decision is external (which shelf a sorter assigns), a type switch is clean.

### `out`, nullable value types, and boxing
- **`out` returns extra values.** A method can return `bool` (found?) and hand a value back through `out` — the idiom for "parse or find without throwing":

```csharp
if (int.TryParse("42", out int parsed))
    Console.WriteLine(parsed);   // 42
```

- **Nullable value types `int?`** are value types that can also be null. **Lifted operators** propagate null through arithmetic, and `??` supplies a default:

```csharp
int? maybe = null;
int copies = maybe ?? 0;     // null-coalescing -> 0
int? doubled = maybe * 2;    // lifted: null * 2 == null
```

`?.` and `??` are your null-safe toolkit.

- **Boxing** copies a value type onto the heap when you assign it to `object`; **unboxing** copies it back, and you must unbox to the *exact* boxed type:

```csharp
object boxed = 5;            // boxing (hidden heap allocation)
int unboxed = (int)boxed;    // unbox to int, not long
```

Boxing is exactly why generics exist: `List<int>` never boxes, while the pre-generics `ArrayList` boxed every `int` (the callback to Monday).

## Code Example
A concurrent fetch with an offline fallback, then the type switch:

```csharp
LibraryItem?[] fetched = await Task.WhenAll(fetches);
LibraryItem? first = fetched.Length > 0 ? fetched[0] : null;

if (first is null)   // network gave nothing — fall back to a local fixture, same parser
{
    string path = Path.Combine(AppContext.BaseDirectory, "sample-book.json");
    first = OpenLibraryClient.Parse(File.ReadAllText(path));
}

if (first is not null)
    Console.WriteLine($"Fetched: {first.Describe()}");
```

With a live network it prints real, deserialized data built through the factory; with the network down it reads the canned `sample-book.json` and runs it through the *same* `Parse` — the only difference is where the bytes came from. That completes the kata: `core-csharp-kata` is feature-complete (T2 final).

> Heads up: `IHttpClientFactory`, cancellation tokens, and retry policies are production concerns for Week 5 (DI and the TPL); a real persistent data store is Friday's SQL kickoff and Week 3. Today's HTTP layer is the shape a database layer mirrors.

## Summary
- **`async`/`await` frees the thread while waiting on I/O** — `HttpClient` is async-first, so `Main` becomes `async Task`; never `.Result`/`.Wait()` or `async void`.
- **Share one `HttpClient`**; a new one per call exhausts sockets.
- **Deserialize the JSON into a built-in shape, read the fields you need, and build your domain object through the factory.**
- **`Task.WhenAll` overlaps independent awaits** — launch the tasks, then await once; awaiting in a loop is serial.
- **Regex validates input shape** — verbatim `@"..."`, anchored with `^`/`$`; a type-`switch` branches on runtime type.
- **`out` returns extra values, `int?` + lifted operators handle null, boxing is a hidden heap allocation** that generics avoid.

## Additional Resources
- [Make HTTP requests with HttpClient — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient)
- [Asynchronous programming with async and await — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)
- [How to serialize and deserialize JSON — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to)
