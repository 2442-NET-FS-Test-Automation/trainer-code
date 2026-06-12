# Async Lab — Core C# Kata: Build an Interactive Domain Manager

## Objective

Build a small **interactive console application** — a *domain manager* in your own domain — that applies this week's C# basics, class fundamentals, and the four pillars of OOP. The trainer demo *showcased* these features in a **Library** domain; here you *apply* the same classes and OOP design inside a real little app. This is the Week 1 capstone: it proves you can structure a small program with object-oriented design and make it do something.

The work lives in your existing **`FirstName-Lastname`** repo on the training GitHub org. Add the kata as a project there (e.g. `core-csharp-kata/`), then open a Pull Request.

## What You're Building

A menu-driven manager over the entities of your domain. A loop prints a menu; the user picks a command; the program adds, lists, or acts on entities until the user quits. Seed **2-3 entities at startup** so the app does something the moment it runs.

Pick your own domain and verbs — examples:

| Domain | Commands |
|--------|----------|
| Inventory | add product / restock / sell / list |
| Bank | open account / deposit / withdraw / list |
| Garage | add vehicle / log service / list / search |
| Playlist | add track / play / list / total time |

The `switch` dispatches commands, the `while` keeps the app running, each command is a method, the entities are your classes, and the OOP pillars show up as a list of mixed entity types you drive through a shared base reference.

## The Demo This Mirrors

The `LibraryKata` demo committed the syntax basics, class basics, and oop (inheritance, polymorphism, abstraction, encapsulation, interfaces & abstract classes, access modifiers, `static`, override vs new, overloading) in the **Library** domain. That demo prints fixed output to teach the features in isolation. You mirror it at the **concept level** — same classes and OOP pillars, your own domain — but wrapped in an interactive app instead of a print-only `Main`.

## Choose Your Domain

Pick a domain that is **not** Library so your kata is your own:

- Inventory (Product, quantity, restock)
- Bank (Account, deposit, balance)
- Garage (Vehicle, mileage, service)
- Playlist (Track, duration, play)

## Acceptance Criteria (Grading Rubric)

This checklist is how your work is graded. Your PR is reviewed against it — each item must be true in your repo and PR:

**Application**

- [ ] A runnable console app exists (`dotnet run` works) that prints a **menu** and loops until the user quits.
- [ ] At least **3 commands** that operate on your domain objects (e.g. add, list, one domain action).
- [ ] **2-3 entities are seeded at startup** so a `list` command shows data immediately.
- [ ] The menu loop uses a **`while`** (run-until-quit) and a **`switch`** (command dispatch).

**Basics** (satisfied through the app, not a separate demo)

- [ ] Uses at least three different **data types** appropriately.
- [ ] Uses **operators** (arithmetic and comparison or logical) in real command logic.
- [ ] Each command is a **method**; at least one takes a parameter and returns a value.
- [ ] Uses an **array** (or list) of entities and a **string** operation (interpolation or formatting) in your output.

**Classes**

- [ ] Defines at least one **class** in your domain (not `Book` — pick your own).
- [ ] The class has a **field or property**, a **constructor**, and a **method**.
- [ ] The class lives in a sensible **namespace**.
- [ ] Commands create objects with `new` and call their methods.

**OOP pillars**

- [ ] **Encapsulation** — private state exposed through properties/methods that enforce a rule.
- [ ] **Inheritance** — a derived class extends a base class in your domain ("is-a").
- [ ] **Polymorphism** — a `virtual` method `override`n in a subclass, called through a base reference (e.g. your `list` command loops a base-typed collection of mixed entity types).
- [ ] **Abstraction** — an **abstract class** or an **interface** defines a contract your classes implement.
- [ ] At least one **access modifier** beyond the default is used deliberately (`protected`/`internal`).
- [ ] At least one **`static`** member (helper or shared value).
- [ ] At least one example of **overloading** (same name, different parameters).

**Pull Request**

- [ ] Work is on a feature branch (e.g., `feature/core-csharp-kata`).
- [ ] A **Pull Request** is opened against `main` in your `FirstName-Lastname` repo.
- [ ] The PR description lists which file/class demonstrates each pillar.
- [ ] Commit messages are clear and scoped.

## Structuring Program.cs

Keep `Main` thin — a menu loop that reads a choice and dispatches. The command logic lives in handler methods, and the domain logic lives in your **classes**, not in `Main`:

```csharp
static void Main()
{
    var running = true;
    while (running)
    {
        PrintMenu();
        int choice = int.Parse(Console.ReadLine());   // naive: may throw on bad input — fine for now
        switch (choice)
        {
            case 1: AddItem(); break;
            case 2: ListItems(); break;
            case 0: running = false; break;
        }
    }
}
```

`Main` orchestrates; the handlers (`AddItem`, `ListItems`, ...) operate on your domain objects.

## Build From Scratch

No scaffold is provided. Create the project yourself: `dotnet new console -n core-csharp-kata`. Designing the structure — namespaces, classes, the menu, handler methods — is part of the exercise. There is no solution key; the logic is yours.

## Stretch (optional, not required.)

Once **every** criterion above passes, harden the input: replace `int.Parse` with `int.TryParse` and re-prompt on bad input instead of crashing. Do this **last** — defensive parsing is a later topic and is not what this lab tests.

## Submission

1. Create a feature branch in your `FirstName-Lastname` repo.
2. Commit with clear messages (one per logical step is good practice).
3. Push the branch and open a **Pull Request** against `main`.
4. In the PR description, map each OOP pillar to the file/class that demonstrates it, and paste a short sample session transcript.

*Graded deliverable — complete start of day 6/16. This closes out Week 1 topics.*
