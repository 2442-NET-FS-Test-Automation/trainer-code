# .NET and C# Introduction

## Learning Objectives
- Explain what .NET is and how the SDK differs from the runtime.
- Use the .NET CLI to create, build, and run a console application.
- Describe what C# is and where it sits in the .NET ecosystem.
- Read the anatomy of a C# program (`using`, namespace, `Main`, top-level statements).
- Define what a full stack developer is and how this cohort builds toward that role.

## Why This Matters
This is the note that turns "I read about C#" into "I can run C#." Every other note this week shows you snippets; this one shows you *where that code lives and how to execute it*. Before you can practice types, classes, or OOP, you need a project that builds and runs, and you need to recognize the few lines of scaffolding every program starts with. Five minutes here removes the single most common beginner blocker — "I have the code, but where do I put it and how do I run it?"

## The Concept

### What is .NET
**.NET** is the free, cross-platform development *platform* from Microsoft for building applications — console tools, web APIs, desktop apps, and more. It provides:
- A **runtime** (the CLR, Common Language Runtime) that executes your compiled program and manages memory.
- A huge **standard library** (the Base Class Library) of ready-made types like `Console`, `List`, and `File`.
- **Languages** that compile to it — chiefly **C#**.

Your C# code is compiled to an intermediate language, which the runtime turns into machine code as it runs. This is why .NET programs run on Windows, macOS, and Linux without rewriting.

### SDK vs runtime
Two things share the ".NET" name; the difference trips up beginners:
- The **Runtime** is what you need to *run* an existing .NET app.
- The **SDK** (Software Development Kit) is what you need to *build* apps — it includes the runtime **plus** the compiler and the `dotnet` command-line tool.

As a developer, install the **SDK**. Verify it:

```bash
dotnet --version       # prints the installed SDK version, e.g. 8.0.x
```

### The .NET CLI
The `dotnet` command is your control panel. The three you will use constantly:

```bash
dotnet new console -o HelloApp   # scaffold a new console project in folder HelloApp
cd HelloApp
dotnet build                     # compile it
dotnet run                       # compile and run it
```

`dotnet new console` generates a ready-to-run project: a `.csproj` file (project settings and dependencies) and a `Program.cs` (your code).

### VS Code
**Visual Studio Code** is the lightweight, free editor used in this cohort. Install it, then add the **C# Dev Kit** extension so you get syntax highlighting, IntelliSense (autocomplete), and debugging. VS Code edits the files; the `dotnet` CLI builds and runs them — they work together.

### What is C#
**C#** (pronounced "C-sharp") is a modern, statically typed, object-oriented language. "Statically typed" means the type of every variable is fixed and checked at compile time, so many mistakes are caught before the program ever runs. It is the primary language of .NET and the backbone of this entire cohort.

### Anatomy of a C# program
A traditional C# program looks like this:

```csharp
using System;                       // import a namespace of ready-made types

namespace HelloApp                  // groups your own types
{
    class Program
    {
        static void Main(string[] args)   // the entry point — execution starts here
        {
            Console.WriteLine("Hello, world!");
        }
    }
}
```

Line by line:
- `using System;` pulls in the `System` namespace so you can write `Console` instead of `System.Console`.
- `namespace HelloApp` is a container that organizes your code (covered in *Classes and Projects*).
- `class Program` is a class that holds the entry method.
- `static void Main(string[] args)` is the **entry point** — when you `dotnet run`, the runtime calls `Main` first.
- `Console.WriteLine(...)` prints a line to the terminal.

Modern C# offers **top-level statements**, which let small programs skip the ceremony — the compiler supplies the `Main` wrapper for you:

```csharp
Console.WriteLine("Hello, world!");
```

Both compile to the same thing. New `dotnet new console` projects use top-level statements; you should still recognize the full form, because larger programs and most examples use it.

### Reading input
Programs can also read from the terminal:

```csharp
Console.Write("What is your name? ");
string? name = Console.ReadLine();
Console.WriteLine($"Hello, {name}!");
```

### What is a full stack developer
A **full stack developer** can work across the whole application:
- **Front end** — what the user sees in the browser (HTML, CSS, JavaScript/TypeScript, React — Weeks 6–7).
- **Back end** — the server logic and APIs (C#, ASP.NET — Weeks 4–5).
- **Database** — where data is stored (SQL — Weeks 2–3).
- Plus the glue: testing, cloud, and DevOps (Weeks 8–11).

This cohort is sequenced to make you exactly that: comfortable at every layer, with test automation as the throughline. You are starting at the back-end/language layer today.

## Code Example (When Relevant)
A complete, runnable first program with input and output:

```csharp
using System;

Console.Write("Enter your name: ");
string? name = Console.ReadLine();

if (string.IsNullOrWhiteSpace(name))
    name = "stranger";

Console.WriteLine($"Welcome to .NET, {name}!");
```

Save as `Program.cs` in a `dotnet new console` project, then `dotnet run`.

## Summary
- **.NET** is a cross-platform platform: a **runtime** (CLR), a standard library, and languages like **C#**.
- Install the **SDK** to build (it includes the runtime + compiler + `dotnet` CLI); check with `dotnet --version`.
- Core CLI loop: `dotnet new console` -> `dotnet build` -> `dotnet run`. **VS Code** is the editor.
- Every program has an **entry point** (`Main`); modern projects use **top-level statements** that hide the `Main` boilerplate.
- A **full stack developer** works front end, back end, and database — the destination this cohort is built toward.

## Additional Resources
- [What is .NET? — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/introduction)
- [.NET CLI overview — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/tools/)
- [A tour of C# — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/)
