# SDLC, Waterfall, and Agile

## Learning Objectives
- Explain what the Software Development Life Cycle (SDLC) is and name its phases.
- Describe the Waterfall model and where it fits.
- Describe Agile and why most modern teams work this way.
- Compare Waterfall and Agile and choose appropriately.
- Identify the core Agile roles, ceremonies, and artifacts you will use in this cohort.

## Why This Matters
Before you write a line of C#, you should know *how software gets built by a team*. This week's epic stands you up as a working developer, and every employer runs some process for turning an idea into shipped, maintained software. The vocabulary here — sprint, backlog, standup, pull request — is the language your future team speaks daily. It is also the most common "tell me about your process" interview question. You do not need to memorize a textbook; you need a mental map so the ceremonies you join feel familiar rather than foreign.

## The Concept

### What is the SDLC
The **Software Development Life Cycle** is the sequence of phases software moves through from idea to retirement. Almost every methodology is some arrangement of these same phases:

1. **Requirements** — figure out what to build and why (gather needs from users/stakeholders).
2. **Design** — decide how to build it (architecture, data models, interfaces).
3. **Implementation** — write the code.
4. **Testing** — verify it works and meets the requirements.
5. **Deployment** — release it to users.
6. **Maintenance** — fix bugs, add features, keep it running.

The methodologies below differ mainly in *how often* and *in what order* you pass through these phases.

### Waterfall
**Waterfall** runs the phases **once, in strict sequence** — each phase completes and is signed off before the next begins, like water flowing down a series of steps.

```
Requirements -> Design -> Implementation -> Testing -> Deployment -> Maintenance
```

Strengths: simple to understand, heavy documentation, predictable for fixed-scope projects (e.g. regulated or contractual work). Weakness: it assumes you know all requirements up front and they will not change. If testing reveals a flawed requirement, you have already built on top of it — change is expensive and late.

### Agile
**Agile** is an iterative, incremental approach: instead of one long pass, you deliver working software in short cycles (typically **1–2 week sprints**), gather feedback, and adapt. Each sprint touches *all* the SDLC phases in miniature — a little design, code, test, and review — producing a small, usable increment.

The **Agile Manifesto** prizes four things:
- **Individuals and interactions** over processes and tools.
- **Working software** over comprehensive documentation.
- **Customer collaboration** over contract negotiation.
- **Responding to change** over following a plan.

This does not mean "no process" or "no docs" — it means favoring the left when they conflict. Agile wins when requirements are uncertain or evolving, which is most real software.

> **Scrum** is the most common Agile *framework*. When people say "we do Agile," they usually mean Scrum or a close variant.

### Waterfall vs Agile

| | Waterfall | Agile |
|---|-----------|-------|
| Cadence | one long pass | repeated short sprints |
| Requirements | fixed up front | evolve each sprint |
| Delivery | one release at the end | working increment every sprint |
| Feedback | late (after testing) | early and continuous |
| Best for | stable, well-known scope | uncertain or changing scope |
| Risk | discovered late | surfaced early |

### Agile roles, ceremonies, and artifacts
You will participate in these throughout the cohort:

**Roles**
- **Product Owner** — owns the backlog and priorities; represents the customer's voice.
- **Scrum Master** — facilitates the process, removes blockers; not a "boss."
- **Development Team** — the people who build the increment (you).

**Artifacts**
- **Product Backlog** — the ordered master list of everything that might be built.
- **Sprint Backlog** — the slice the team commits to this sprint.
- **Increment** — the working, shippable output of a sprint.
- **User Story** — a backlog item in the form *"As a [user], I want [goal] so that [benefit]."*

**Ceremonies**
- **Sprint Planning** — pick and size the work for the sprint.
- **Daily Standup** — a short (~15 min) sync: *what I did, what I will do, what is blocking me.*
- **Sprint Review** — demo the increment to stakeholders.
- **Sprint Retrospective** — the team reflects on how to improve next sprint.

A **board** (To Do / In Progress / Done) visualizes the sprint backlog. You will set one up in GitHub Projects for your deliverable.

## Code Example (When Relevant)
This is a process topic, not a coding topic. The closest "artifact" you will write is a user story and a board column. A user story on your board might read:

```
As a librarian, I want to search books by title
so that I can find a copy quickly.

Acceptance criteria:
- Entering a title returns matching books.
- An empty search shows a friendly message.
```

## Summary
- The **SDLC** is the universal phase sequence: requirements, design, implementation, testing, deployment, maintenance.
- **Waterfall** runs those phases once, in order — predictable but slow to absorb change.
- **Agile** runs them iteratively in short sprints — early feedback, embraces change; **Scrum** is its dominant framework.
- Core Scrum: roles (Product Owner, Scrum Master, Dev Team), artifacts (product/sprint backlog, increment, user story), ceremonies (planning, standup, review, retro).
- Most modern teams — and this cohort — work in sprints with a board and daily standups.

## Additional Resources
- [What is the SDLC? — AWS](https://aws.amazon.com/what-is/sdlc/)
- [The Agile Manifesto](https://agilemanifesto.org/)
- [The Scrum Guide](https://scrumguides.org/scrum-guide.html)
