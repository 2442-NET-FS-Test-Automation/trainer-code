# Searching: Linear and Binary

## Learning Objectives
- Implement linear search and identify its syntax on sight.
- Implement binary search and identify its syntax on sight.
- Evaluate the trade-offs: time complexity vs the sorted-data precondition.
- Use `Array.BinarySearch`, including against descending data with a custom comparer.

## Why This Matters
Search is the atom of data work — every "find the row where" is one of these two shapes. The trade-off
between them (any data but O(n), versus O(log n) but *sorted only*) is the first real algorithmic decision
you learn to make, and it pays off concretely in real endpoints: a ranking feature can binary search a
report that is already sorted instead of scanning it, precisely because the sort happened once up front.

## The Concept

### Linear search: O(n), any data
Walk every element until you find the target. Works on sorted, unsorted, anything enumerable:

```csharp
public static int LinearSearch(int[] data, int target)
{
    for (int i = 0; i < data.Length; i++)
        if (data[i] == target) return i;
    return -1;                                // convention: -1 = not found
}
```

Recognize it by shape: one loop, one equality check, early return. Worst case (target absent or last) it
touches all n elements. For small n or one-off searches on unsorted data, it is the right tool — sorting
first just to search once costs more than the scan.

### Binary search: O(log n), sorted data ONLY
Compare against the middle; discard the half that cannot contain the target; repeat:

```csharp
public static int BinarySearch(int[] sorted, int target)
{
    int low = 0, high = sorted.Length - 1;
    while (low <= high)
    {
        int mid = low + (high - low) / 2;        // not (low+high)/2: avoids int overflow
        if (sorted[mid] == target) return mid;
        if (sorted[mid] < target) low = mid + 1; // target is in the upper half
        else high = mid - 1;                     // target is in the lower half
    }
    return -1;
}
```

Recognize it by shape: `low`/`high`/`mid`, a halving loop, no full scan. Each iteration halves the space:
8 million elements need at most ~23 comparisons. Two classic implementation bugs to check whenever you
review a hand-rolled version: the loop condition must be `<=` (a one-element range is still searchable),
and `mid` must move past the compared element (`mid + 1`/`mid - 1`) or a two-element range loops forever.
Hand-rolled binary searches are wrong often enough that spotting these two on sight is a genuine skill.

### The trade-off, stated the way interviews ask

| | Linear | Binary |
|---|---|---|
| Time | O(n) | O(log n) |
| Data precondition | none | **must be sorted** |
| Best when | unsorted data, small n, search once | sorted (or worth sorting) data, search many times |

The break-even intuition: sorting costs O(n log n). One search does not repay it; many searches (or data
that is *already* sorted, like an ordered report) repay it immediately.

### The library call, and descending data
In real code you rarely hand-roll it:

```csharp
int idx = Array.BinarySearch(sortedArr, target);       // >= 0 hit; negative = not found
```

`Array.BinarySearch` assumes **ascending** order. To search data sorted *descending* (a
biggest-first report, say), supply a comparer that inverts the comparison — the search must agree with
the sort:

```csharp
var idx = Array.BinarySearch(unitsDesc, units,
    Comparer<int>.Create((a, b) => b.CompareTo(a)));   // descending comparer
return new { units, rank = idx >= 0 ? idx + 1 : -1 };
```

Mismatch the order and you get garbage indexes with no exception — a favorite debugging exercise.

## Say It in an Interview
- *"Linear search walks every element — O(n), works on any data. Binary search halves a sorted range each
  step — O(log n), but the data must be sorted."*
- *"I'd pick linear for unsorted or tiny data searched once; binary when the data is sorted already or
  searched often enough to repay an O(n log n) sort."*
- *"A sound binary search has three tells: `low <= high` as the loop condition, an overflow-safe midpoint
  (`low + (high - low) / 2`), and `mid + 1`/`mid - 1` moves so the range always shrinks."*
- *"In .NET I'd call `Array.BinarySearch` — negative result means not found — and pass a custom comparer
  if the data isn't ascending."*

## Check Yourself
1. Binary search on 8 million elements: roughly how many comparisons worst case, and why?
2. A hand-rolled binary search hangs forever on a two-element array. Which of the two classic bugs is it?
3. You need to find one value in an unsorted 50-element array, once. Which search, and why?
4. `Array.BinarySearch` returns nonsense indexes (no exception) against your data. What is the likely
   cause?
5. Why is `low + (high - low) / 2` preferred over `(low + high) / 2`?

**Answers:** (1) ~23 — each comparison halves the range, and log2(8,000,000) ≈ 23. (2) `mid` not moving:
the update must be `low = mid + 1`/`high = mid - 1`, not `low = mid`/`high = mid`. (3) Linear — sorting
first (O(n log n)) to search once costs more than one O(n) scan. (4) The array's sort order disagrees with
the comparer (e.g. descending data, default ascending comparer) — the search must agree with the sort.
(5) `low + high` can overflow `int` when both are large; the subtraction form cannot.

## Summary
- Linear: one loop, any data, O(n) — right for unsorted or tiny inputs.
- Binary: halve the sorted space, O(log n) — right when data is sorted or searched repeatedly.
- Sound binary search: `low <= high`, overflow-safe mid, `mid +/- 1` moves.
- `Array.BinarySearch` in practice; a custom comparer when the data is not ascending.

## Resources
- [Binary search algorithm (Khan Academy)](https://www.khanacademy.org/computing/computer-science/algorithms/binary-search/a/binary-search)
- [Array.BinarySearch documentation](https://learn.microsoft.com/en-us/dotnet/api/system.array.binarysearch)
- [Sequential vs binary search comparison (GeeksforGeeks)](https://www.geeksforgeeks.org/linear-search-vs-binary-search/)
