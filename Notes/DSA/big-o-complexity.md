# Big-O Notation and Algorithm Complexity

## Learning Objectives
- Describe algorithm complexity using Big-O notation.
- Rank the common growth classes and recognize each in code.
- Distinguish worst, average, best, and amortized cost.
- Read existing code, identify its Big-O cost, and debug it with efficiency in mind.

## Why This Matters
Big-O is the shared language for "will this survive real data." Picture an order service that resolves
SKUs on every line of every incoming order — do that with a linear scan and a 10,000-order burst does
10,000 scans of the catalog; do it with a dictionary and each lookup is constant. Same feature, orders of
magnitude apart, and the *only* tool that predicts it before you ship is complexity analysis. Interviews
and code reviews both ask you to analyze code you did not write — so this note practices both directions:
naming the class and spotting it in the wild.

## The Concept

### What Big-O says (and does not)
Big-O describes how an algorithm's cost **grows as input size n grows**, ignoring constant factors.
`O(n)` means: double the data, roughly double the work. It does not say which of two algorithms is faster
at n = 10 — constants win small races; growth wins big ones.

### The growth classes

| Class | Name | Canonical example |
|---|---|---|
| `O(1)` | constant | `list[i]`, `dict[key]` (average) |
| `O(log n)` | logarithmic | binary search — halve the space each step |
| `O(n)` | linear | linear search, one pass to sum an array |
| `O(n log n)` | linearithmic | merge sort, any good general sort |
| `O(n^2)` | quadratic | bubble/insertion/selection sort, nested loops over the same data |
| `O(2^n)` | exponential | naive recursive Fibonacci |

A simple timing harness makes the gap physical — time linear vs binary search for the last element as n
grows:

```
n           linear(find-last)     binary(find-last)
100,000          ~5,700 ticks           ~10 ticks
1,000,000       ~57,000 ticks           ~12 ticks
8,000,000      ~460,000 ticks           ~14 ticks
```

Linear grows with n; binary barely moves (`log2` of 8 million is ~23). That flat second column is what
`O(log n)` *feels* like. Build one of these harnesses yourself once (`Stopwatch` around each call) — the
numbers persuade in a way the table cannot.

### Worst, average, best — and amortized
Unqualified Big-O usually means **worst case**, but interviewers probe the distinctions:

- **Worst case** — the guarantee. Linear search when the target is absent: O(n).
- **Best case** — the lucky path. Linear search when the target is first: O(1). Rarely worth quoting.
- **Average case** — expected cost over typical inputs. Hash-table lookup is O(1) *average* but O(n)
  *worst* (all keys colliding); quicksort is O(n log n) *average* but O(n^2) *worst* (bad pivots on
  sorted-ish input).
- **Amortized** — the per-operation average across a sequence, when occasional operations are expensive.
  `List<T>.Add` is O(1) *amortized*: most adds are cheap, and the occasional capacity-doubling copy (O(n))
  is spread across the adds that preceded it.

The spoken form matters: "dictionary lookup is O(1) average, O(n) worst under pathological collisions" is
a complete answer; "O(1)" alone invites the follow-up.

### Reading code for its cost
The mechanical rules cover most code you will ever read:

- A loop over n items: `O(n)`. Two sequential loops: still `O(n)` (constants drop).
- **Nested** loops over the same data: multiply — `O(n^2)`:

```csharp
for (int i = 0; i < a.Length - 1; i++)          // n outer
    for (int j = 0; j < a.Length - 1 - i; j++)  // ~n inner
        if (a[j] > a[j + 1]) Swap(ref a[j], ref a[j + 1]);   // bubble sort: O(n^2)
```

- A loop that halves its range each iteration: `O(log n)` (binary search's `while (low <= high)`).
- Recursion that branches twice per call without caching: exponential (`FibNaive(n-1) + FibNaive(n-2)`).
- A dictionary/`HashSet` operation inside a loop: the loop is `O(n)` *total*, because each lookup is
  `O(1)` average — this is the single most common optimization in working code.

Debugging for efficiency is the same reading applied to a symptom: "the endpoint is fine with 100 rows and
times out with 100,000" almost always means an accidental `O(n^2)` — a `Contains` on a `List` inside a
loop, a query in a loop (the ORM N+1 problem — see `../01-efcore/loading-strategies-raw-sql.md`), a
re-sort per iteration. Find the loop-in-a-loop that does not need to be one; replace the inner scan with a
hash lookup or a presorted structure.

### Space complexity, briefly
The same notation applies to memory. Bubble sort sorts in place (`O(1)` extra space); merge sort allocates
merge buffers (`O(n)`); memoized Fibonacci trades `O(n)` space for the exponential-to-linear time win. Time
and space are frequently a trade — say which one you are buying.

## Say It in an Interview
- *"Big-O describes how an algorithm's cost grows with input size, ignoring constants — O(n) means double
  the data, double the work. Unqualified, it usually means worst case."*
- *"The ladder I keep in my head: O(1) dictionary lookup, O(log n) binary search, O(n) a single pass,
  O(n log n) a good sort, O(n^2) nested loops, O(2^n) uncached branching recursion."*
- *"To analyze code I read its loop structure: nested loops multiply, a halving loop is logarithmic, and a
  hash lookup inside a loop keeps it linear overall."*
- *"Amortized means averaged over a sequence — `List<T>.Add` is O(1) amortized because the occasional
  resize copy is spread across many cheap adds."*

## Check Yourself
1. A method has two sequential `for` loops over the same array, then a third loop nested inside a fourth.
   What is its overall complexity?
2. Why is hash-table lookup quoted as O(1) *average* rather than just O(1)?
3. An endpoint is fast at 100 rows and times out at 100,000. What class of bug do you look for first, and
   what is the usual fix?
4. `List<T>.Add` sometimes copies the whole backing array. Why do we still call it O(1)?
5. Which costs more memory, bubble sort or merge sort — and what does each buy for it?

**Answers:** (1) O(n^2) — sequential O(n) loops drop against the nested pair, which multiplies.
(2) Pathological collisions can put every key in one bucket, degrading lookup to O(n) worst case.
(3) An accidental quadratic — a linear scan (`Contains`, a query, a re-sort) inside a loop; replace the
inner scan with a dictionary/`HashSet` lookup or sort once outside the loop. (4) The O(n) resize happens
so rarely (capacity doubles) that its cost amortizes to O(1) per add. (5) Merge sort — O(n) merge buffers
versus bubble's O(1) in-place swaps; the memory buys guaranteed O(n log n) time.

## Summary
- Big-O = growth rate vs input size, constants ignored; unqualified usually means worst case.
- Know the ladder: 1, log n, n, n log n, n^2, 2^n — with one concrete example each.
- Worst vs average vs amortized: the qualifier is part of the answer (hash tables, quicksort, `List.Add`).
- Read code by its loop structure: nested = multiply, halving = log, branching recursion without cache =
  exponential, hash lookups = constant.
- Slow-at-scale bugs are usually hidden quadratics; the fix is usually a hash table or a sort you do once.

## Resources
- [Big-O Cheat Sheet](https://www.bigocheatsheet.com/)
- [Big O notation (Khan Academy, Algorithms course)](https://www.khanacademy.org/computing/computer-science/algorithms/asymptotic-notation/a/big-o-notation)
- [Time complexity of .NET collections (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/standard/collections/)
