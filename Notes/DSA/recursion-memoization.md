# Recursion, Memoization, and Tabulation

## Learning Objectives
- Explain recursion and identify the base case and recursive case in a method.
- Solve simple problems recursively (factorial, sum, Fibonacci).
- Optimize exponential recursion with memoization (top-down) and tabulation (bottom-up).

## Why This Matters
Recursion is how divide-and-conquer algorithms are written — you already met it inside merge sort — and
"find the base case" is an on-sight reading skill interviewers test directly. Memoization and tabulation
matter because they are the standard rescue when a natural recursive definition turns out to be
exponentially wasteful — and the underlying instinct (compute once, cache, reuse) shows up everywhere in
working systems, from Fibonacci to the lookup dictionary a service builds once at startup instead of
re-querying per request.

## The Concept

### The two cases every recursive method has
A recursive method calls itself on a smaller version of the problem. It terminates only because of its
**base case** — the input small enough to answer directly. Everything else is the **recursive case**:

```csharp
// BASE: n <= 1 -> 1.       RECURSIVE: n * Factorial(n - 1)
public static long Factorial(int n) => n <= 1 ? 1 : n * Factorial(n - 1);

// BASE: n == 0 -> 0.       RECURSIVE: n + Sum(n - 1)
public static long Sum(int n) => n == 0 ? 0 : n + Sum(n - 1);
```

Reading drill (the interview form): given a method, point at the condition that does *not* recurse — that
is the base case; confirm the recursive call moves toward it (`n - 1` shrinks n). A recursion whose
argument does not provably approach the base case is a `StackOverflowException` with extra steps: each
pending call holds a stack frame, so even correct recursion has an O(depth) space cost — the reason
`Sum(10)` is fine and `Sum(10_000_000)` is not, and the reason iterative rewrites exist.

### When recursion explodes
Fibonacci's natural definition branches twice per call:

```csharp
public static long FibNaive(int n) => n < 2 ? n : FibNaive(n - 1) + FibNaive(n - 2);
```

`FibNaive(50)` recomputes `FibNaive(48)` twice, `FibNaive(47)` three times... the call tree doubles per
level: **O(2^n)**. The waste is *recomputing identical subproblems* — which is precisely the thing caching
fixes.

### Memoization: top-down, cache on the way
Keep the recursive shape; cache each result the first time you compute it:

```csharp
public static long FibMemo(int n, Dictionary<int, long>? cache = null)
{
    cache ??= new();
    if (n < 2) return n;                                  // base case unchanged
    if (cache.TryGetValue(n, out long hit)) return hit;   // seen it? O(1) return
    return cache[n] = FibMemo(n - 1, cache) + FibMemo(n - 2, cache);
}
```

Each distinct n is computed once: **O(n) time**, O(n) space for the cache (plus recursion depth).
Memoization is the minimal change when you already have a recursive solution — bolt a dictionary on.

### Tabulation: bottom-up, build a table
Flip the direction: start from the base cases and iterate up, no recursion at all:

```csharp
public static long FibTab(int n)
{
    if (n < 2) return n;
    long prev = 0, curr = 1;                       // the base cases
    for (int i = 2; i <= n; i++)
        (prev, curr) = (curr, prev + curr);        // each value from the previous two
    return curr;
}
```

Also **O(n) time** — and because Fibonacci only ever needs the last two values, the "table" shrinks to two
variables: O(1) space, no stack risk. Tabulation generally wins when you can see the iteration order;
memoization wins when the dependency structure is irregular and recursion finds it for you.

| | Memoization | Tabulation |
|---|---|---|
| Direction | top-down (recurse, then cache) | bottom-up (iterate from base cases) |
| Code change | add a cache to existing recursion | rewrite as a loop + table |
| Space | cache + call stack | table (sometimes O(1)) |
| Use when | irregular subproblem graph, quick fix | clear order, want stack safety |

Both are the entry door to *dynamic programming* — the general technique of solving overlapping
subproblems once.

## Say It in an Interview
- *"Every recursive method has a base case that answers directly and a recursive case that calls itself
  on smaller input; I check that the argument provably moves toward the base case."*
- *"Recursion costs a stack frame per pending call — O(depth) space — so deep recursion risks stack
  overflow even when it's logically correct."*
- *"Naive Fibonacci is O(2^n) because it recomputes the same subproblems; memoization caches results
  top-down for O(n), and tabulation builds the answer bottom-up in a loop — often with O(1) space."*
- *"I'd memoize when I already have a recursive solution and want a minimal change; I'd tabulate when the
  iteration order is clear and I want stack safety."*

## Check Yourself
1. In `long F(int n) => n <= 1 ? 1 : n * F(n - 1);`, point at the base case and the recursive case.
2. Why does correct, terminating recursion still fail on very large inputs, and with what exception?
3. What exactly makes `FibNaive` exponential — recursion itself, or something else?
4. Memoization vs tabulation: which direction does each work in, and which one eliminates stack risk?
5. Fibonacci tabulation runs in O(1) space. Why can the "table" shrink to two variables?

**Answers:** (1) Base: `n <= 1 ? 1`; recursive: `n * F(n - 1)`. (2) Each pending call holds a stack frame
— O(depth) space — so depth eventually exhausts the stack: `StackOverflowException`. (3) Not recursion —
the *re-computation of identical subproblems* from the double branch; caching removes it. (4) Memoization:
top-down, keeps recursion (stack risk stays); tabulation: bottom-up loop, no recursion, no stack risk.
(5) Each Fibonacci value depends only on the previous two, so only those two need to be retained.

## Summary
- Recursion = base case (answers directly, stops) + recursive case (self-call on smaller input); identify
  both on sight and check the argument approaches the base.
- Each call costs a stack frame — recursion has an O(depth) space bill and a stack-overflow failure mode.
- Naive branching recursion recomputes subproblems (Fibonacci: O(2^n)).
- Memoization caches top-down (O(n), minimal diff); tabulation iterates bottom-up (O(n), stack-free,
  sometimes O(1) space).

## Resources
- [Recursion (Khan Academy)](https://www.khanacademy.org/computing/computer-science/algorithms/recursive-algorithms/a/recursion)
- [Memoization and dynamic programming (GeeksforGeeks)](https://www.geeksforgeeks.org/tabulation-vs-memoization/)
- [Recursion in C# (Microsoft Learn, methods)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/methods)
