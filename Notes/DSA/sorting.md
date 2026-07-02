# Sorting: Bubble, Insertion, Selection, Merge (and Quicksort)

## Learning Objectives
- Implement bubble, insertion, and selection sort and identify each by its syntax.
- Implement merge sort and explain its divide-and-conquer structure.
- Recognize quicksort and state its average/worst-case behavior.
- Compare the algorithms: complexity, behavior on nearly-sorted data, and when each matters.

## Why This Matters
Nobody hand-sorts in production — `OrderBy` and `Array.Sort` exist. You learn these algorithms because
they are the standard vocabulary for *reasoning about cost* (every "why is this O(n^2)?" conversation uses
them), because identifying them on sight is a stock interview exercise, and because the difference between
the simple sorts and the divide-and-conquer sorts is the cleanest demonstration of why algorithm choice
beats micro-optimization. Sorted data is also a prerequisite you produce on purpose: sort a report once
and ranking becomes a binary search.

## The Concept

All four implementations below take and return `int[]`. One hygiene habit: clone the input before sorting
when the caller still needs the original — these sorts mutate the array they are given, and "the data was
already sorted by the previous run" is a classic way to fool yourself when timing or testing.

### Bubble sort — swap adjacent pairs, O(n^2)
The largest value "bubbles" to the end on each pass; the sorted tail grows from the right:

```csharp
for (int i = 0; i < a.Length - 1; i++)
    for (int j = 0; j < a.Length - 1 - i; j++)      // note the -i: skip the sorted tail
        if (a[j] > a[j + 1])
            (a[j], a[j + 1]) = (a[j + 1], a[j]);    // tuple swap
```

Recognize it: nested loops, compares **adjacent** elements `a[j]` vs `a[j+1]`, swaps in place. It is also
the easiest sort to get subtly wrong — the classic bugs live in the loop bounds (an inner condition
testing the wrong variable, or a missing `- i`), and they throw `IndexOutOfRangeException` or quietly
half-sort. When reviewing one, check the bounds first.

### Insertion sort — grow a sorted prefix, O(n^2)
Take the next element ("the key"), shift larger sorted elements right, drop the key in its slot:

```csharp
for (int i = 1; i < a.Length; i++)
{
    int key = a[i], j = i - 1;
    while (j >= 0 && a[j] > key) { a[j + 1] = a[j]; j--; }   // shift right
    a[j + 1] = key;                                          // insert
}
```

Recognize it: starts at index 1, a `key` variable, a backward-shifting `while`. Its superpower: on
**nearly-sorted** data the while loop barely runs, making it effectively O(n) — which is why real
libraries use it for small or almost-ordered partitions.

### Selection sort — select the minimum, O(n^2)
Find the minimum of the unsorted region, swap it into the next sorted position:

```csharp
for (int i = 0; i < a.Length - 1; i++)
{
    int min = i;
    for (int j = i + 1; j < a.Length; j++)
        if (a[j] < a[min]) min = j;
    if (min != i) (a[i], a[min]) = (a[min], a[i]);
}
```

Recognize it: a `min` index tracked through the inner loop, exactly **one swap per outer pass**. That
minimal-swaps property is its only practical niche (expensive writes); it is O(n^2) even on sorted input.

### Merge sort — divide and conquer, O(n log n)
Split in half, recursively sort each half, merge two sorted halves:

```csharp
public static int[] Merge(int[] a)
{
    if (a.Length <= 1) return a;              // base case
    int mid = a.Length / 2;
    int[] left  = Merge(a[..mid]);            // recursive case
    int[] right = Merge(a[mid..]);
    return MergeTwo(left, right);             // two-pointer merge of sorted halves
}
```

`MergeTwo` walks both halves with two indexes, always taking the smaller head — O(n) per level, log n
levels of splitting = O(n log n), **guaranteed, any input**. The costs: O(n) extra space for the merge
buffers, and recursion. It is also *stable* (equal elements keep their relative order), which matters when
sorting by one field while preserving an earlier ordering.

### Quicksort — the other divide-and-conquer
The interviewer's favorite sibling. Pick a **pivot**, *partition* the array so everything smaller sits
left of it and everything larger right (the pivot is now in its final position), recurse on the two sides:

- **Average O(n log n)**, and *in place* — no merge buffers, which is why library sorts build on it.
- **Worst case O(n^2)**: a naive pivot (first/last element) against already-sorted or reverse-sorted input
  makes every partition lopsided. Real implementations pick pivots defensively (median-of-three, random).
- Not stable, unlike merge sort.

Recognize it: a `Partition` method, a pivot, two recursive calls on index ranges (no merge step). .NET's
`Array.Sort` uses *introsort* — quicksort that switches to heapsort when recursion gets deep (dodging the
O(n^2) worst case) and to insertion sort on tiny partitions.

### The comparison interviews ask for

| Sort | Time | Space | Identify by | Note |
|---|---|---|---|---|
| Bubble | O(n^2) | O(1) | adjacent compare + swap | teaching tool; check its bounds |
| Insertion | O(n^2), ~O(n) nearly-sorted | O(1) | key + backward shift | best simple sort in practice |
| Selection | O(n^2) always | O(1) | tracked min, one swap/pass | fewest swaps |
| Merge | O(n log n) always | O(n) | recursion + two-pointer merge | stable; divide and conquer |
| Quick | O(n log n) avg, O(n^2) worst | O(log n) stack | pivot + partition | in-place; library workhorse |

In application code, call `OrderBy`/`Array.Sort` and spend your judgment on *when* to sort — once, before
the repeated binary searches, not inside the loop.

## Say It in an Interview
- *"Bubble, insertion, and selection are the O(n^2) simple sorts; merge and quick sort are O(n log n)
  divide-and-conquer."*
- *"I recognize them by shape: adjacent swaps = bubble, a key shifted backward into a sorted prefix =
  insertion, a tracked minimum with one swap per pass = selection, split-recurse-merge = merge sort,
  pivot-and-partition = quicksort."*
- *"Insertion sort is effectively O(n) on nearly-sorted data, which is why libraries use it for small
  partitions."*
- *"Quicksort averages O(n log n) in place but degrades to O(n^2) with naive pivots on sorted input;
  merge sort guarantees O(n log n) and stability at the price of O(n) extra space. .NET's `Array.Sort`
  is introsort — quicksort with escape hatches."*

## Check Yourself
1. You see nested loops comparing `a[j]` with `a[j+1]` and swapping. Which sort, and what should you
   double-check first in a code review?
2. Which simple sort would you run on a list that is already 95% sorted, and what happens to its cost?
3. Merge sort vs quicksort: name the two prices merge sort pays and the one guarantee it buys.
4. Why can quicksort hit O(n^2), and how do real implementations avoid it?
5. What does *stable* mean for a sort, and which of the two divide-and-conquer sorts has it?

**Answers:** (1) Bubble sort; check the loop bounds — the classic bugs are a wrong variable in the inner
condition or a missing `- i`. (2) Insertion — the backward shift barely runs, so it approaches O(n).
(3) Prices: O(n) merge-buffer space and recursion; guarantee: O(n log n) on *any* input (plus stability).
(4) Naive pivot choice (first/last) makes partitions lopsided on sorted/reverse-sorted input; defenses:
median-of-three or random pivots, or introsort's switch to heapsort. (5) Equal elements keep their
relative order — merge sort is stable, quicksort is not.

## Summary
- Five shapes to recognize on sight: adjacent-swap (bubble), key-and-shift (insertion), min-and-swap
  (selection), split-and-merge (merge), pivot-and-partition (quick).
- Simple sorts are O(n^2); insertion wins among them on nearly-sorted data; selection minimizes swaps.
- Merge sort: O(n log n) guaranteed + stable, at O(n) space. Quicksort: O(n log n) average in place,
  O(n^2) worst without pivot defense; introsort is the production compromise.
- Production code uses the library sort; the vocabulary is for reasoning and review.

## Resources
- [Sorting algorithms overview (Khan Academy)](https://www.khanacademy.org/computing/computer-science/algorithms/sorting-algorithms/a/sorting)
- [Sorting algorithm visualizations (VisuAlgo)](https://visualgo.net/en/sorting)
- [Array.Sort documentation (introspective sort)](https://learn.microsoft.com/en-us/dotnet/api/system.array.sort)
