# C# Basics Overview

## Learning Objectives
- Identify C#'s core value types and reference types, and pick the right one for a job.
- Use the arithmetic, comparison, equality, and logical operators correctly.
- Direct program flow with `if`/`else`, `switch`, and the loop family.
- Convert between types safely with casts, `Convert`, and `int.Parse` / `TryParse`.
- Write and call methods, passing arguments and returning values, including a method that calls itself (recursion).
- Explain how strings, the stack, the heap, and arrays store data in memory.

## Why This Matters
This week's epic takes the cohort "from zero to a runnable, OOP-structured C# console app." None of the object-oriented design later in the week stands up without these primitives. Every class you build, every method you call, and every bug you troubleshoot bottoms out in the rules covered here: which type holds your data, whether two values are *equal* or merely *the same object*, and where that data actually lives. Get these right and the OOP pillars on Friday feel like organization rather than magic.

## The Concept

### Data Types
C# is **statically typed**: every variable's type is known at compile time. Types split into two families.

**Value types** hold their data directly. Assigning one copies the value.

| Type | Holds | Example |
|------|-------|---------|
| `int` | 32-bit whole number | `42` |
| `long` | 64-bit whole number | `42L` |
| `double` | 64-bit floating point | `3.14` |
| `decimal` | high-precision decimal (money) | `19.99m` |
| `bool` | true / false | `true` |
| `char` | single character | `'A'` |

**Reference types** hold a *reference* to data stored elsewhere. `string`, arrays, and every class are reference types. More on the memory implications below.

Use `var` when the type is obvious from the right-hand side; the compiler still infers a concrete static type — `var` is not "any."

```csharp
int count = 10;          // explicit
var name = "Ada";        // inferred as string
decimal price = 19.99m;  // m suffix = decimal literal
```

A value you never want reassigned is a **constant**. `const` is a compile-time fixed value; `readonly` (seen later on fields) is set once at construction:

```csharp
const double Pi = 3.14159;   // can never change
```

**Comments** document intent — `//` for a line, `/* ... */` for a block. Comment *why*, not *what the code obviously does*.

#### Type conversion
Sometimes you have one type and need another. Three common routes:

```csharp
double d = 9.7;
int floored = (int)d;          // explicit cast — truncates to 9 (no rounding)

int n = Convert.ToInt32("42"); // Convert helpers parse and convert

bool ok = int.TryParse("42", out int parsed); // safe parse: ok is true, parsed is 42
bool bad = int.TryParse("oops", out int zero); // bad is false, zero is 0 — no crash
```

Prefer `int.TryParse` over `int.Parse` for user input: `Parse` throws on bad text, while `TryParse` returns `false` and lets you handle it gracefully.

### Operators

**Arithmetic:** `+  -  *  /  %`. Integer division truncates: `7 / 2` is `3`, not `3.5`. The modulo `%` returns the remainder: `7 % 2` is `1`.

**Comparison:** `<  >  <=  >=` return a `bool`.

**Equality:** `==` and `!=`. For value types these compare the values. For reference types `==` compares references by default (do the two variables point at the *same* object?) — except `string`, which overloads `==` to compare text. This distinction is a classic source of bugs; keep it in mind.

**Logical:** `&&` (and), `||` (or), `!` (not). `&&` and `||` **short-circuit** — the right side is skipped when the left already decides the result, which is why `obj != null && obj.IsReady` is safe to write in that order.

**Assignment shortcuts:** compound operators combine arithmetic with assignment, and `++`/`--` step by one:

```csharp
total += 5;    // same as total = total + 5
count++;        // increment by 1
price *= 1.1;   // 10% increase
```

**Ternary:** a compact `if`/`else` that produces a value — `condition ? whenTrue : whenFalse`:

```csharp
string label = age >= 18 ? "adult" : "minor";
```

```csharp
bool eligible = age >= 18 && hasId;
int remainder = total % groupSize;
```

### Control Flow

`if` / `else if` / `else` branch on boolean conditions:

```csharp
if (score >= 90)       grade = "A";
else if (score >= 80)  grade = "B";
else                   grade = "C";
```

`switch` is cleaner when matching one value against many cases:

```csharp
switch (day)
{
    case "Sat":
    case "Sun":
        Console.WriteLine("Weekend");
        break;
    default:
        Console.WriteLine("Weekday");
        break;
}
```

**Loops:**
- `for` — when you know the count: `for (int i = 0; i < 5; i++)`.
- `while` — repeat while a condition holds, checked first.
- `do/while` — same, but the body runs at least once.
- `foreach` — iterate every item in a collection without an index.

```csharp
foreach (string title in titles)
    Console.WriteLine(title);
```

### Methods
A method is a named, reusable block of logic. It declares a return type (`void` if it returns nothing), a name, and a parameter list.

```csharp
int Add(int a, int b)
{
    return a + b;
}

int sum = Add(3, 4); // 7
```

Methods keep code DRY and testable — the seam your test-automation track later targets directly. Name them as verbs (`Calculate`, `Print`, `Validate`).

A method may also call **itself** — **recursion**. Each call solves a smaller piece of the problem until it hits a **base case** that returns without recursing. Omit the base case and the calls never stop, overflowing the stack (`StackOverflowException`).

```csharp
int Factorial(int n)
{
    if (n <= 1) return 1;         // base case — stops the recursion
    return n * Factorial(n - 1);  // recursive case — same method, smaller n
}

int result = Factorial(5); // 120
```

Recursion fits naturally nested problems (factorials, walking a folder tree); for plain counting a `for` loop is usually clearer and cheaper.

### Strings, Stack & Heap, Arrays

**The stack and the heap** are two regions of memory.
- The **stack** stores local variables and value types; it is fast and automatically cleaned up when a method returns.
- The **heap** stores objects (reference types); a variable on the stack holds a *reference* (address) pointing into the heap.

So `int x = 5;` puts `5` on the stack, while `string s = "hi";` puts the reference on the stack and the string data on the heap.

**Strings** are reference types but **immutable** — any "change" produces a new string. Building text in a loop with `+` creates many throwaway objects; prefer interpolation for readability:

```csharp
string greeting = $"Hello, {name}! You have {count} items.";
```

Strings carry a rich set of methods (each returns a *new* string, since the original is immutable):

```csharp
string raw = "  Clean Code  ";
raw.Length;              // 13 (includes spaces)
raw.Trim();               // "Clean Code"
raw.ToUpper();            // "  CLEAN CODE  "
raw.Contains("Code");    // true
raw.Replace("Code", "Architecture");
"a,b,c".Split(',');      // string[] { "a", "b", "c" }
```

When you must build a string across many steps (e.g. inside a loop), use `StringBuilder` to avoid creating a throwaway string each time:

```csharp
using System.Text;
var sb = new StringBuilder();
for (int i = 0; i < 3; i++) sb.Append($"line {i}\n");
string result = sb.ToString();
```

To read text from the user, pair `Console.Write` with `Console.ReadLine` (which returns a nullable string):

```csharp
Console.Write("Enter a number: ");
string? input = Console.ReadLine();
int value = int.TryParse(input, out int n) ? n : 0;
```

**Arrays** are fixed-size, indexed collections of a single type, stored on the heap:

```csharp
int[] scores = new int[3];   // {0, 0, 0}
string[] days = { "Mon", "Tue", "Wed" };
Console.WriteLine(days[0]);   // Mon — zero-based
Console.WriteLine(days.Length); // 3
```

Indexing past the end throws `IndexOutOfRangeException`. Arrays are the foundation for the richer collections you will meet in later weeks.

### Troubleshooting Basics
- **Read the compiler error first** — it names the file, line, and cause. `CS0103: The name 'x' does not exist` means a typo or scope problem.
- **`NullReferenceException`** at runtime means you used a reference that points at nothing; check what was never assigned.
- Use `Console.WriteLine` to print intermediate values when behavior surprises you.

## Code Example (Putting It Together)
A complete, runnable program (top-level statements) that pulls these pieces together — types, an array, a loop, a method, a cast, and interpolation:

```csharp
using System;

int[] checkouts = { 3, 1, 4, 1, 5 };

Console.WriteLine($"Total: {Sum(checkouts)}, Average: {Average(checkouts):0.00}");

int Sum(int[] values)
{
    int total = 0;
    foreach (int n in values)
        total += n;
    return total;
}

double Average(int[] values) => (double)Sum(values) / values.Length;
```

Save this as `Program.cs` in a `dotnet new console` project and `dotnet run` to see `Total: 14, Average: 2.80`.

## Summary
- C# is statically typed; **value types** copy data, **reference types** copy a reference.
- `==` compares values for value types and references for objects — `string` is the friendly exception.
- Convert types with casts (`(int)`), `Convert`, or `int.TryParse` (the safe choice for user input).
- `&&` / `||` short-circuit; integer `/` truncates and `%` gives the remainder; `+=`/`++` and the ternary `?:` are everyday shortcuts.
- Choose the loop that matches what you know: count (`for`), condition (`while`), collection (`foreach`).
- Methods make logic reusable and testable (and may call themselves — recursion, which needs a base case); strings are immutable; arrays are fixed-size and zero-indexed; the stack holds locals, the heap holds objects.

## Additional Resources
- [C# data types — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types)
- [Statements and control flow — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/)
- [Stack vs heap and memory in .NET — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals)
