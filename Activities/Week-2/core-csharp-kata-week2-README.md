# Async Lab — Ship a Production-Shaped Domain CLI (Week 2 Capstone — Teams of 3)

## Objective

Take the **interactive console app you built in Week 1** (the `core-csharp-kata` domain manager) and
**extend it into a finished, production-shaped app**: add real collection types, code that fails loudly and
logs, fast indexed internals, and a live call to the outside world — all in one shared domain. The trainer
demos *showcase* each feature in a **Library** domain; here your team *applies* them and brings the kata to
feature-complete.

This is a **single multi-day deliverable** — one project, one team. You work in a **team of 3** in **one
shared repo**, building the kata together: it cements the
intro C# from weeks 1 + 2 and gives a low-stakes first taste of working on a shared repository (feature
branches, pull requests, a quick teammate review). **Keep the process light** — the real team project
(4-person teams, full backend + DB + Azure hosting, multi-week sprint) comes later, after EF Core and
ASP.NET. This week is a console warmup.

## What You're Building

Still the **same interactive, menu-driven app** from your Week 1 `core-csharp-kata` — a **CRUD manager**
over your domain entities (add / list / update / remove, plus a domain action or two), seeded with 2-3
entities at startup, driven by the `while` + `switch` menu loop. **Everything your Week 1 lab required —
the menu loop, seeded entities, 3+ commands, the OOP pillars — still holds.** You are *extending* that app,
not replacing it.

What's new is **under the hood, plus one new command**: the entities move into the right collections, bad
input and missing lookups fail loudly and get logged, lookups get fast, and a new command pulls **live data
from a public API**. You are not writing four feature demos — you are growing
**one coherent app** whose menu commands now stand on production-grade internals.

The next section says it as **user stories** — what a person running your app can do. Each story points at a
C# tool from this week, but **choosing the right tool is your job** (that's the lesson). The
*Techniques You Must Demonstrate* checklist near the end lists the full toolbox your finished app has to use
somewhere — you decide which story each one serves.

## Logistics

| | |
|---|---|
| **Handed out** | Tue Jun 16 (after the collections + exceptions demos) |
| **Due** | **Mon Jun 22**, start of day |
| **Mode** | **Teams of 3** — one shared domain, one shared repo, light branch/PR/review |
| **Submission** | Feature branches → small reviewed PRs into `main` in the team repo (see Submission) |

## Your Team Repo

Everyone already has a `FirstName-Lastname` repo on the training GitHub org. **Pick one existing member's
repo as the team repo** and **add the other two as collaborators** so everyone can push and open PRs — do
not create a new repo. Then choose your starting point:

- **Continue the strongest Week-1 kata** among the three of you (its domain becomes the team domain), or
- **Reroll from scratch** (`dotnet new console`, quickly re-lay the Week-1 classes + OOP) if that gets you
  to a clean start faster.

Either is fine — pick whatever ships. No scaffold is provided and there is no solution key; designing the
types, patterns, API, and validation is the exercise.

## Getting Everyone Up to Speed

If a teammate joined late and missed the Week-1 demos (basics, classes, OOP), bring them in by **building
together, not lecturing**: walk them through the base kata (or co-build the reroll), then have them write
some of the **Week-1 fundamentals** themselves — an entity class (field/property, constructor, method) and
one OOP pillar (inheritance or polymorphism). That hands-on pass is how weeks 1 + 2 C# lands. Everyone
ends the week having written real C#.

## Working Together (keep it light)

- Split the work among yourselves however you like — **no fixed roles or owners.**
- Keep PRs small; have a teammate glance at each PR and approve before it merges. That quick review **is**
  the shared-repo practice for this week.
- Don't build heavier process (branching strategies, role rotation, conflict-resolution drills) — that's
  for the later multi-week sprint, not this console warmup.

## Your Team Domain

Share **one** domain across the team (the base kata's, or one you pick at reroll). Use a domain that is
**not** Library:

| Domain | Entity | Sample public API |
|--------|--------|-------------------|
| Inventory | Product, supplier, stock | Open Library / a products API |
| Bank | Account, owner, balance | exchangerate.host (FX rates) |
| Garage | Vehicle, make, VIN | a public VIN-decode API |
| Playlist / Weather-station | Track / Reading, location | open-meteo.com |

---

## What the App Must Do (User Stories)

Keep the **`while` + `switch` menu loop** from Week 1 — every capability below is a **menu command** a user
runs, not a block of demo output. Mirror the trainer demos at the **concept level** in your own domain (same
ideas, **never a copy of the Library code**). Phrasing is domain-neutral — swap in your own entities. Each
story lists **acceptance criteria** you can see in a menu session. **How you split these across days,
commits, and branches is your team's call.**

### Collections & ordering

- **Add and list.** *As a user, I can add an entity and list everything currently held; the list grows and
  shrinks as I add or remove — no fixed cap.*
  - Accept: after N adds the listing shows N; after a remove it shows N-1.
- **Undo my last action.** *As a user, I can undo my most recent change (un-sell a product, reverse the last
  deposit, drop the last-queued track), and the one before it is untouched.*
  - Accept: undo reverses the **most recent** action first.
- **Serve in arrival order.** *As a user, I can process pending requests (restock requests, a teller line, a
  play queue) in the exact order they arrived.*
  - Accept: the **first** request added is served first.
- **Reorder my working list.** *As a user, I can move an urgent entity to the front of a working list without
  rebuilding the list.*
  - Accept: an item sent to the front prints first on the next listing.
- **See a grid view.** *As a user, I can view a fixed rows×columns layout (aisle×shelf, lot×row, week×day).*
  - Accept: the app reports the grid's two dimensions and places items at `[row, col]`.

### Failing safely & a log trail

- **Clear errors, no crash.** *As a user, when I ask for an entity that doesn't exist, I get a clear message
  naming what was missing, and the app keeps running.*
  - Accept: a bad lookup does not crash; the error names the missing id; the menu returns.
- **A trail of what happened.** *As an operator, I can read a running log of what the app did, tagged by
  severity (routine / warning / failure).*
  - Accept: a session shows at least one info, one warning, and one error line tied to real actions.

### Fast lookups & flexible browsing

- **Instant lookup by key.** *As a user, I can pull up any entity immediately by its natural key (SKU,
  account number, VIN) without scanning the whole list.*
  - Accept: looking up a missing key reports "not found" cleanly rather than crashing.
- **Distinct values, no duplicates.** *As a user, I can see each distinct attribute once (each supplier, each
  owner, each artist), however many entities share it.*
  - Accept: the distinct count is **less than** the entity count when duplicates exist.
- **Browse everything in one pass.** *As a user, I can browse the whole collection with a single list
  command.*
  - Accept: one command walks every entity in order.
- **Search by my own condition.** *As a user, I can search for entities matching a condition I supply (price
  over X, balance under Y, genre = Z).*
  - Accept: the same search command returns different results for different conditions.

### Live data & input it can trust

- **Enrich from a live source.** *As a user, I can add or enrich an entity using real data fetched from a
  public online source, and the app stays responsive while it fetches.*
  - Accept: the fetched data shows up on the entity; the app does not freeze during the call.
- **Survive a network error.** *As a user, if the fetch fails the app reports it and keeps running instead
  of crashing.*
  - Accept: a failed fetch is logged and the command reports "nothing fetched"; the app does not crash.
- **Reject bad input.** *As a user, the app refuses a malformed identifier before it saves anything.*
  - Accept: a wrongly-shaped id is rejected with a message; a well-shaped one is accepted.

---

## Engineering Definition of Done (how you build it)

User stories say *what*; these are the *how* your code must implement.

- Your collections are **private**; callers reach them only through methods/indexers you expose.
- An **`enum`** names fixed choices (no magic strings); a **`readonly struct`** bundles small identity-less
  data, and your output shows **value-vs-reference** (a copied struct is independent).
- You wrote your **own generic type** (`Bin<T>`/`Slot<T>`/…) and used it with at least one element type.
- A broken assumption **throws a custom exception that carries data** (the missing id) instead of returning
  `null`; a caller handles it with **`try`/`catch`/`finally`**, catching the **specific** type **before** any
  base type, and `finally` runs even on the throw path.
- **One** design pattern centralizes creation/fetch: a **repository behind an interface** (callers depend on
  the interface) **or** a **factory** over your kind that rejects an unknown kind in its `default` arm.
  (Unit-of-Work / Singleton not required.)
- **Serilog** configured **once** (`CreateLogger()` at startup, `CloseAndFlush()` on exit), using the
  **structured-template** form (`Log.Information("... {Id}", id)`), **never string concatenation**.
- Changing the data structure behind a lookup **does not change the public contract** callers use.
- Your manager is **`foreach`-able** (`IEnumerable<T>` + `yield return`) and filtering takes a
  **lambda / `Predicate<T>`** — **hand-rolled, No LINQ**, so the mechanism stays visible.
- At least one **expression-bodied member** (`=>`), and **either** `partial` (split the manager across two
  files) **or** a **`sealed`** leaf type.
- One **shared `HttpClient`** (not one per call); `async Task` all the way to `Main`; **no** `.Result`/
  `.Wait()`, **no** `async void`.
- The fetched JSON is **deserialized** and your domain object is **built from the fields you read** (via your
  constructor or factory) — **no separate wire type** mirroring the API.
- A foreseeable **`HttpRequestException`** is caught and handled (logged, returns nothing) so a dead network
  doesn't crash the run.
- Validation uses a **`Regex`** (verbatim `@"..."`, anchored with `^` and `$`), plus at least one of: an
  **`out`** parameter, a **nullable** value type with `??`/lifted operator, or a **pattern-matching `switch`**.

---

## Techniques You Must Demonstrate

The finished app must use **each** of these **somewhere** — you decide which story each one serves.

- [ ] `List<T>` · `Stack<T>` (LIFO) · `Queue<T>` (FIFO) · `LinkedList<T>` · multi-dimensional array `T[,]`
- [ ] `enum` · `readonly struct` · a generic type you wrote yourself
- [ ] custom exception (carries data) · `try`/`catch`/`finally`
- [ ] one pattern: repository behind an interface **or** factory
- [ ] Serilog structured logging (info / warning / error)
- [ ] `Dictionary<K,V>` + `TryGetValue` · `HashSet<T>`
- [ ] `IEnumerable<T>` + `yield return` · lambda / `Predicate<T>` filter
- [ ] expression-bodied member · `partial` **or** `sealed`
- [ ] shared `HttpClient` + `async`/`await` · `HttpRequestException` handling
- [ ] JSON deserialization (read the fields, build via your factory/constructor)
- [ ] `Regex` validation · one of: `out` param / nullable + lifted operator / pattern-matching `switch`

---

## Stretch (each group is required to pick 2 to implement.)

Pick atleast two (or come up with your own!). With three pairs of hands you have room to pick a couple — but the core (every ship criterion) comes first. Once every ship criterion passes you can:

- Add a **generic constraint** to your custom generic type (e.g. `where T : class`) and note in the PR why it
  fits or does not fit your element type.
- Add a **second `yield` method** that filters during iteration and confirm nothing runs until you `foreach`
  it (deferred execution).
- Add a **second Serilog sink** (e.g. `.WriteTo.File(...)`) and confirm the call sites did not change.
- Issue **two** HTTP requests with **`Task.WhenAll`** and confirm they overlap rather than run back-to-back.
- Add a **second design pattern** (e.g. ship both a repository *and* a factory) and name both in the PR.
- Add a short **`CONTRIBUTING.md`** noting your team's branch/PR/review convention.

## Submission

1. Work on **feature branches** in the team repo; how you slice commits and branches is your team's call.
2. Open **small PRs** against `main`; a teammate gives each a quick review/approval before it merges. One
   small PR per chunk of work is a good rhythm, but it's a suggestion, not a hard rule. The deliverable is
   **the finished kata** in the team repo.
3. In the final PR (or a short README section): map each **user story** (and each technique) to the file/type
   that proves it, name the SOLID principle behind your pattern and the API you called, paste a short
   **sample session transcript** (a menu run that exercises the stories), and add a light
   **who-worked-on-what** line.

Keep it light — the history should show **everyone has real commits** (including anyone who joined late).
