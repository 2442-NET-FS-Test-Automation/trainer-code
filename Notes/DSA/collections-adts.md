# Abstract Data Types: Lists, Stacks, Queues, Hash Tables, and Priority Queues

## Learning Objectives
- Explain the list, stack, queue, and hash-table ADTs and their common operations.
- Explain when to choose a stack, queue, or priority queue from program requirements.
- Compare arrays, linked lists, and hash tables on insertion, deletion, and lookup cost.
- Analyze a problem and pick the right structure, and defend the choice in Big-O terms.

## Why This Matters
"Analyze a given problem and determine the appropriate data structures" is the skill everything else here
serves — it is also the most common practical interview question there is. Consider the choices a typical
order-processing service makes: a dictionary for SKU-to-product lookups (hash table — O(1) per order line
instead of a scan), a priority queue so expedited orders drain first, and plain `List<T>`s where order and
index access are all that matter. Being able to say *why* each is right is the skill.

## The Concept

### ADT vs implementation
An **abstract data type** is a contract of operations (what you can do); a data structure is an
implementation (how). "Queue" is the ADT; `Queue<T>` (a circular array) is one implementation, a linked
list is another. Interviews test the contracts and their costs.

### The list ADT
Ordered, index-accessible collection: `Add`, `Insert(i, x)`, `RemoveAt(i)`, `list[i]` get/set. .NET's
`List<T>` is a **dynamic array**: contiguous storage that doubles capacity when full.

- `list[i]`: O(1). `Add` at the end: O(1) amortized (the occasional doubling is O(n) but rare).
- `Insert`/`RemoveAt` in the middle: O(n) — everything after shifts.

### The stack and queue ADTs
- **Stack — LIFO** (last in, first out): `Push`, `Pop`, `Peek`. Choose it when the *most recent* thing
  matters next: undo history, backtracking, matching brackets, the call stack itself.
- **Queue — FIFO** (first in, first out): `Enqueue`, `Dequeue`, `Peek`. Choose it when *arrival order*
  must be preserved: work items, message processing, breadth-first traversal.

```csharp
var stack = new Stack<string>();  stack.Push("a"); stack.Push("b");  stack.Pop();   // "b"
var queue = new Queue<string>();  queue.Enqueue("a"); queue.Enqueue("b"); queue.Dequeue(); // "a"
```

All core operations on both: O(1).

### The hash-table ADT
Key-value store with **average O(1)** `Add`, `Remove`, and lookup: a hash function maps the key to a
bucket. .NET: `Dictionary<K,V>` (and `HashSet<T>` for keys-only membership).

```csharp
var skuToId = new Dictionary<string, int> { ["BK-001"] = 1, ["BK-002"] = 2 };
if (skuToId.TryGetValue("BK-001", out int id)) ...      // O(1) average
```

The costs are *average*: hash collisions (two keys landing in one bucket) are resolved by **chaining**
(each bucket holds a small list — .NET's approach) or **open addressing** (probe the next slot);
pathological collisions degrade lookup toward O(n), which is why key types need good `GetHashCode`
implementations. Classic problem-solving move: "detect duplicates" = add everything to a `HashSet<T>` and
watch the `bool` that `Add` returns — O(n) total instead of the O(n^2) nested scan.

### The priority queue
Serve by **priority**, not arrival: `Enqueue(item, priority)`, `Dequeue()` returns the smallest-priority
item. .NET's `PriorityQueue<TElement, TPriority>` is a binary heap: enqueue and dequeue are **O(log n)**,
peek O(1). Lower priority value dequeues first — so "expedited beats normal" is encoded by *inverting*:

```csharp
var pq = new PriorityQueue<int, int>();                   // element = order id
foreach (var o in orders)
    pq.Enqueue(o.Id, o.Priority == Priority.Expedited ? 0 : 1);   // 0 beats 1
while (pq.TryDequeue(out var id, out _)) ordered.Add(id);         // expedited ids drain first
```

Choose stack/queue/priority queue by requirement: most-recent-first -> stack; fairness/arrival order ->
queue; urgency classes or "always the smallest/largest next" -> priority queue.

### Arrays vs linked lists vs hash tables (the classic comparison)

| Operation | Array / `List<T>` | Linked list | Hash table |
|---|---|---|---|
| Lookup by index/key | **O(1)** by index | O(n) walk | **O(1)** avg by key |
| Search by value | O(n) (O(log n) if sorted) | O(n) | **O(1)** avg |
| Insert/delete at a known node | O(n) shift | **O(1)** relink | **O(1)** avg |
| Memory layout | contiguous, cache-friendly | node + pointers per element | buckets + overhead |

A **linked list** chains nodes (`value` + `next` reference; doubly-linked adds `prev` — .NET
`LinkedList<T>`). Its O(1) insert/delete *at a node you already hold* is real, but getting to that node is
O(n), and pointer-chasing is cache-hostile — in practice `List<T>` wins most workloads, and the linked
list's honest niche is splicing within a sequence you are already iterating. No index access: "give me
element 500" walks 500 nodes.

### Choosing, out loud
The form interviewers want: *requirement -> structure -> cost*. Worked examples from an order service:

- "Resolve SKU to product id on every order line" -> hash table -> O(1) per line instead of an O(n)
  catalog scan per line.
- "Expedited orders complete first" -> priority queue -> O(log n) per order, independent of arrival order.
- "Rank a product in a report" -> sort once + binary search -> O(n log n) + O(log n) per query.

## Say It in an Interview
- *"An ADT is the contract of operations; the data structure is the implementation — `Queue<T>` and a
  linked list both implement the queue ADT."*
- *"Stack is LIFO for most-recent-first work like undo and backtracking; queue is FIFO for arrival order;
  priority queue serves by urgency — .NET's is a binary heap, O(log n) enqueue/dequeue."*
- *"Hash tables give O(1) average lookup by hashing keys into buckets; collisions are resolved by chaining
  or open addressing, and bad hashing degrades toward O(n)."*
- *"Arrays: O(1) index, O(n) mid-list insert. Linked lists: O(1) insert at a node you hold, O(n) to reach
  it. Hash tables: O(1) average everything, no ordering. I justify a choice as requirement, structure,
  then cost."*

## Check Yourself
1. You need "undo" in an editor. Which ADT, and why?
2. Why is `List<T>.Add` O(1) *amortized* rather than plain O(1)?
3. Two keys hash to the same bucket. Name the two standard resolution strategies and .NET's choice.
4. Expedited orders must always process before normal ones, whatever order they arrive in. Structure,
   .NET type, and per-operation cost?
5. When does a linked list genuinely beat `List<T>`, and what kills it the rest of the time?
6. Detect duplicates in a list of a million strings — what is the O(n) approach?

**Answers:** (1) Stack — undo needs the *most recent* action first (LIFO). (2) The backing array
occasionally doubles (an O(n) copy), and that cost spreads across the many cheap adds. (3) Chaining
(bucket holds a list; .NET's approach) and open addressing (probe for the next free slot). (4) Priority
queue, `PriorityQueue<TElement, TPriority>`, O(log n) enqueue/dequeue (peek O(1)). (5) Splicing at a node
you already hold while iterating — O(1) relink; killed elsewhere by O(n) traversal to find the node and
cache-hostile pointer chasing. (6) Add each to a `HashSet<T>` and watch `Add`'s boolean — first `false` is
your duplicate.

## Summary
- ADTs are operation contracts: list (indexed order), stack (LIFO), queue (FIFO), hash table (keyed O(1)
  average), priority queue (serve by priority, heap-backed O(log n)).
- Choose stack for most-recent-first, queue for arrival order, priority queue for urgency.
- The array/linked-list/hash-table trade-off table is core interview material — memorize it *with* the
  caveats (amortized, average, known-node), and know chaining vs open addressing for collisions.
- Justify choices as requirement -> structure -> Big-O.

## Resources
- [Collections and data structures (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/standard/collections/)
- [PriorityQueue&lt;TElement,TPriority&gt; documentation](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.priorityqueue-2)
- [Data structures overview (GeeksforGeeks)](https://www.geeksforgeeks.org/data-structures/)
