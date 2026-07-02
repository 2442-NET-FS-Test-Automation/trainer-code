# Trees and Graphs: Structure

## Learning Objectives
- Explain the structure of a tree: root, parent/child, leaves, no cycles.
- Explain the structure of a graph: vertices, edges, directed/undirected, cyclic/acyclic.
- Name the two traversal families (BFS, DFS) and what drives each.
- Recognize where each shows up in systems you already use.

## Why This Matters
Trees and graphs are the shape of half the systems you touch daily: file systems, JSON documents, and DOM
trees are trees; service dependencies, foreign-key schemas, and git histories are graphs. At this level
the objective is *structure* — describe the parts correctly and place each shape in a real system;
implementing traversals and shortest-path algorithms comes later. Naming the parts precisely is what the
first interview question checks.

## The Concept

### Trees
A **tree** is hierarchical: nodes connected so that there is exactly **one root** (no parent), every other
node has **exactly one parent**, and there are **no cycles** — exactly one path exists between any two
nodes:

```
            Catalog            <- root
           /       \
      Fiction    Technical     <- internal nodes (children of root)
                 /        \
          "Clean Code"  "Refactoring"   <- leaves (no children)
```

Vocabulary: **root** (top), **parent/child** (the edge relationship), **leaf** (no children), **depth**
(distance from root), **height** (longest root-to-leaf path), **subtree** (any node plus its descendants
is itself a tree). A **binary tree** limits children to two; a **binary search tree** additionally keeps
left < node < right, which is what makes O(log n) lookup possible when balanced — the same halving idea as
binary search, stored as a shape. (The heap behind `PriorityQueue` is also a binary tree, kept *complete*
rather than ordered left-to-right.)

You use trees constantly: the file system, every nested JSON body an API returns, the HTML DOM, an ORM
aggregate like `Order -> OrderLines` — a two-level tree.

### Graphs
A **graph** is the general case: **vertices** (nodes) connected by **edges**, with none of the tree's
rules — any node may connect to any others, cycles are allowed, and the graph need not even be connected.

```
   A ---- B          A -> B -> C
   |    / |               ^    |
   |   /  |               |    v
   C ---- D               +--- D        (directed, cyclic)
```

The distinctions that matter:

- **Directed vs undirected** — edges are one-way arrows (follows-on-social-media, foreign keys) or
  mutual links (roads).
- **Cyclic vs acyclic** — can you return to where you started? A **DAG** (directed acyclic graph) is the
  workhorse: build dependencies, task scheduling, git commit history.
- **Weighted** — edges carry a cost (route planning).

Every tree is a graph (connected, acyclic, one-root directed); not every graph is a tree. When someone
says "the services form a dependency graph, and we need it to stay a DAG," they are saying: arrows, no
cycles, or deploys break.

Where they meet your work: a database schema's foreign-key diagram is a directed graph (a typical order
schema: `Order -> Customer`, `OrderLine -> Order`, `OrderLine -> Product` — acyclic); a microservice call
diagram (the SOA sketch in `../02-rest-http/rest-principles.md`) is a directed graph you *want* acyclic.

### Traversal: the two family names
Visiting every node has exactly two standard strategies — know the names and the one-line mechanics even
before you ever implement them:

- **BFS (breadth-first search)** — level by level, driven by a **queue**: visit a node, enqueue its
  neighbors, dequeue the next. Finds the shortest path (by edge count) first.
- **DFS (depth-first search)** — as deep as possible before backtracking, driven by a **stack** (or
  recursion — the call stack *is* the stack). Natural for "does a path exist," cycle detection, and
  tree walks.

Note the callback to the ADTs: the queue and the stack are not just interview trivia — they are what
*select* the traversal order.

### Saying it in an interview (the structural answer)
*Tree: hierarchical nodes with one root, one parent per node, and no cycles. Graph: vertices connected by
edges, possibly directed and possibly cyclic; a tree is the special case that is connected and acyclic
with a single root.* Follow with one example of each from real systems and you have answered the
structural question.

## Say It in an Interview
- *"A tree has one root, one parent per node, no cycles — exactly one path between any two nodes. A graph
  is the general case: vertices and edges, possibly directed, possibly cyclic."*
- *"Every tree is a graph; a DAG is the middle ground — directed, no cycles — and it's what build systems,
  schedulers, and git history are."*
- *"A BST keeps left < node < right so a balanced one searches in O(log n) — binary search stored as a
  shape."*
- *"BFS walks level by level with a queue and finds shortest paths first; DFS dives deep with a stack or
  recursion and suits path-existence and cycle detection."*

## Check Yourself
1. What three structural rules make a tree a tree, and what single consequence do they have for paths?
2. A dependency diagram has arrows and must never loop. What is this called, and name two real systems
   shaped like it.
3. What extra invariant turns a binary tree into a binary search tree, and what does it buy?
4. BFS vs DFS: which data structure drives each, and which one finds the shortest path by edge count?
5. Classify: the file system; a social "follows" network; a git commit history.

**Answers:** (1) One root, exactly one parent per node, no cycles — so exactly one path exists between any
two nodes. (2) A DAG (directed acyclic graph): build dependencies, task schedulers, git commit history.
(3) Left subtree < node < right subtree — O(log n) search when balanced. (4) BFS: queue; DFS:
stack/recursion; BFS finds shortest paths first. (5) Tree; directed graph (cycles allowed); DAG.

## Summary
- Tree = one root, one parent per node, no cycles, one path between any two nodes; know root/leaf/
  parent/child/depth/height.
- BSTs order the shape for O(log n) search; heaps (priority queues) are complete binary trees.
- Graph = vertices + edges; classify by directed/undirected, cyclic/acyclic (DAG), weighted.
- BFS = queue, level-by-level, shortest path; DFS = stack/recursion, deep-first.
- Trees: file systems, JSON, DOM, aggregates. Graphs: schemas, service dependencies, git history.

## Resources
- [Trees (Khan Academy / CS)](https://www.khanacademy.org/computing/computer-science/algorithms)
- [Graph theory basics (GeeksforGeeks)](https://www.geeksforgeeks.org/graph-data-structure-and-algorithms/)
- [Binary trees and BSTs (VisuAlgo)](https://visualgo.net/en/bst)
