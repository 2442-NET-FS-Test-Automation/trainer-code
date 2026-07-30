# P3 Supplemental — Your First Design Artifacts, Step by Step

This document walks you through producing the Project 3 design artifacts — the RTM, documented test
cases, the data-strategy table, the error-guess log, and an exploratory charter — **before you
automate anything**. It replaces a live walkthrough; work through it with your team in your first
project block — Steps 0–1 are ~25 minutes of setup, Steps 2–5 are the rest of the block — with two
things open beside it: the **P3 spec**
(what is required) and the **test-case-design note** (the reference for every concept used here —
this document applies that note, it does not replace it).

It changes nothing about the spec. Same scope, same deadlines. This is the "how do we
actually start" bridge.

**Why the order matters:** every demo this week typed test code directly — that was the right way to
learn the *tools*. It is the wrong model for the *project*. In P3 the case set is designed first and
the code proves it, and your PR history is the graded evidence that design came first. This
walkthrough exists so the first thing your team produces is a design artifact, not a test file.

---

## Step 0 — Inventory your requirements (15 minutes, whole team)

Your requirements already exist: **your Project 2 user stories** — the required skeleton stories
(register, log in/out, role enforcement, browse/search, create a transaction, own-history-only,
admin CRUD, the report) plus your **2+ team-defined stories**, all written with acceptance criteria
in your P2 README and board. You are not writing requirements this week; you are collecting them.

Do this now:

1. Copy every story into a list. Give each one a short id: `REQ-01`, `REQ-02`, ...
2. Where one story hides several testable rules, split it. "Create a transaction" typically splits
   into the happy path, each validation rule, and the not-found rule — each gets its own `REQ` id,
   because each will need its own row and its own cases.
3. Note beside each requirement where it *lives*: service logic, the HTTP surface, the UI, or
   several of those. This becomes the pyramid decision later.

**Sanity check:** most teams land between 10 and 20 requirements. Fewer than 8 means stories were
not split; more than 30 means you split past behaviors into implementation details.

---

## Step 1 — Build the RTM skeleton (10 minutes)

Create `docs/rtm.md` (or wherever your repo keeps docs) and land it by PR **today**, looking like
this:

| Requirement | Cases | Covered |
|---|---|---|
| REQ-01 register | | no |
| REQ-02 log in / log out | | no |
| REQ-03 roles enforced (API) | | no |
| REQ-04 roles enforced (UI) | | no |
| REQ-05 create transaction — happy path | | no |
| REQ-06 create transaction — validation | | no |
| ... every requirement from Step 0 ... | | no |

An RTM with every row empty is not embarrassing — **it is the correct day-one artifact.** It is your
declared to-do list, and its git history is your proof the matrix was maintained rather than
backfilled. Each time a case is designed, its id lands in a row and the commit shows when. The
matrix that appears fully-formed in the last week fails the "maintained, not backfilled" acceptance
line — visibly.

From here on, the working rule from the note applies: **cover every requirement once before covering
any requirement twice.** When you are unsure what to design next, the next empty row is the answer.

One thing this list form shows less well than the note's grid form (requirements as rows, case ids
as columns): the **reverse read** — does every case trace to a requirement? The spec grades both
directions. In the grid, an untraced case is a visible empty column; in the list, you find it by
sweeping your case ids against the matrix — any `TC` that appears in no row is either an
undocumented rule you should surface or waste you should delete. Either form is acceptable
("format is your choice"), but **run both reads either way**, and say in your README how.

---

## Step 2 — First requirement, designed end to end (the worked example; ~30-40 minutes with Step 3)

We will design one requirement completely, using the P2 spec's neutral vocabulary — a **catalog
item** and a **transaction** — so translate into your own entities as you read. Suppose the
requirement is a validation rule on creating a transaction:

> **REQ-06:** a transaction is created for a quantity of **1 up to 10**; anything else is rejected
> with 400 and a reason.

Your actual rule differs — a booking window, a capacity, a length limit. **Substitute yours; the
method is identical.** If your surface truly has no numeric rule, use the nearest limit-shaped
validation you do have.

**2a. Spot the shape.** Open the technique table in the test-case-design note. "1 up to 10" is an
ordered limit → **boundary-value analysis**. The same input also splits into classes treated alike —
below range / in range / above range / not a number → **equivalence partitioning**. One requirement,
two shapes, both techniques — this is normal, and both get named.

**2b. Derive the values.** Boundaries: 0, 1, 2 at the bottom edge; 9, 10, 11 at the top.
Partitions: one representative per class — say -3 (invalid low), 5 (valid), 40 (invalid high), plus
the degenerate non-numeric input if your API can receive one. Then collapse: **a boundary value
already sitting inside a class can serve as that class's representative** — 0 stands in for
invalid-low (dropping -3), 2 or 5 for valid, 11 for invalid-high (dropping 40) — so the ten
candidates collapse to about seven cases, and each kept value carries both technique names where
both apply.

**2c. Write the case docs — before any code.** Create `docs/test-cases.md` (or one file per area)
and write each case in the note's minimum shape. Two shown; your set for this requirement will be
4-7 cases:

```
TC-01
Trace:        REQ-06 (create transaction - quantity validation)
Technique:    Boundary-value analysis (limit 1..10, upper edge)
Precondition: an authenticated consumer; a catalog item the transaction can be created against
Steps:        create a transaction for that item with quantity 11
Expected:     rejected - 400, response names the quantity rule; no transaction persisted
Status:       (at execution)

TC-02
Trace:        REQ-06
Technique:    Equivalence partitioning (valid class representative)
Precondition: same as TC-01
Steps:        create a transaction with quantity 5
Expected:     created - 201; the transaction is persisted
Status:       (at execution)
```

Notice what makes these *designed*: the expected result is written before execution, the technique
is named with its reasoning visible (which edge, which class), and the trace ties every case to the
requirement that permits it to exist.

**2d. Mark the RTM.** REQ-06's row now reads `TC-01 .. TC-07 | yes`. Land the case docs and the
RTM update together in one PR. **This PR — design with no automation in it — is the evidence the
spec grades.**

**2e. Now decide the layer, then automate.** Where does each case live? **First find where the rule
actually runs — it decides the seam.** If your quantity rule lives in **service code**, most of
these cases are **unit tests** (the shape from `01-xunit-fundamentals`: the values become
`[Theory]` rows, and the case doc is what makes those rows a design product instead of arbitrary
numbers). If it lives in **validation annotations on your DTO** — the P2 default — it has **no unit
seam**: the pipeline enforces it, and unit-testing annotations directly is the false-green trap the
`02-xunit-webapi` demo pinned. Then its lowest provable layer is integration, and the value set
becomes integration cases. Either way, the 400-with-reason contract is an HTTP behavior → at least
**one** integration case (the `02-xunit-webapi` shape) proves the pipeline translates the rule to
the right status code, and you do not re-prove at a higher layer what a lower one already proved —
the pyramid rule: each case at the lowest layer that proves it. The automation PR references the TC
ids; each test method carries its id in the name or a comment.

That is the whole loop: **shape → technique → values → case docs → RTM mark → layer → automate.**
Every requirement in your matrix goes through the same loop.

---

## Step 3 — Second requirement, different shape (combining conditions)

> **REQ-03:** consumers cannot reach admin capability, even crafting the HTTP request by hand.

No numbers here, so boundary values buy nothing. Two conditions combine — *who is calling* (anonymous
/ consumer / admin) and *what they call* (a consumer endpoint / an admin endpoint) — and "if A and B
then C" combinations are the **decision table** shape:

| Caller | Endpoint class | Expected |
|---|---|---|
| anonymous | protected (any) | 401 — declared collapse: endpoint class cannot matter before authentication, so two cells are one rule |
| consumer | consumer endpoint | 200-family |
| consumer | admin endpoint | **403** |
| admin | admin endpoint | 200-family |
| admin | consumer endpoint | **your call** — the specs never say; decide it and document it |

The full space is 3 callers x 2 endpoint classes = 6 combinations; the table accounts for all six —
two by a **declared** collapse (the anonymous row), and one by surfacing a cell the requirements
never answered. That last row is the payoff of the technique: can your admin *act as* a consumer
(create transactions), or is admin curate-only? Neither spec decides it — which makes it a
**requirements gap found during design** (Step 5's rarest finding type, caught here for free).
Decide as a team, write the decision down, and test what you decided. What fails the discipline is
not collapsing or deciding — it is dropping a combination *silently*.

The kept rules are HTTP-pipeline behaviors (authentication middleware, role claims), so they live at
the **integration layer** — this is exactly the auth matrix the spec's integration deliverable
requires, and now it is a *designed* auth matrix with a table behind it. Write the case docs, mark
REQ-03's row, automate against your API. (For the 403 case you need a consumer token: register a
consumer through your own API, log in as it, use that token — the seeded admin the demos leaned on
cannot produce a 403 on an admin endpoint.)

Your team stories go through the same shape-spotting: an entity with a lifecycle (pending →
confirmed → cancelled) is the state-transition shape; a multi-step consumer journey is the scenario
shape; and any requirement where you can read the code and see an unexecuted branch is where your
**white-box, coverage-driven case** comes from: run the coverage collection from `01` Step 10, then
the ReportGenerator one-liner that step named to turn the raw XML into a readable per-branch report,
find a branch no case reaches, design the case that reaches it, and name it white-box in its doc.

---

## Step 4 — The data decisions you just implicitly made (10 minutes)

Steps 2-3 quietly used test data. The spec requires those choices to be **explicit**. For each test
object, answer two questions from the note — *what data?* (the objective decides: the smallest set
proving the one thing) and *where does it live?* (the object decides) — and record the answers as a
table in your README:

| Test object | Data home | Why / the trade accepted |
|---|---|---|
| Service rules (unit) | Inline + mocked repository | Total determinism; proves nothing about the real DB — that is the integration suite's job |
| API (integration) | Created/seeded per run, reset in setup | Proves the real pipeline + real persistence; costs reset discipline |
| SPA (Cypress/Selenium) | Seeded demo accounts + a known catalog state | The browser needs a real running system; costs a documented reset step |

Your table will differ — the requirement is that it **exists, is per-object, and names the trade**
(determinism versus proof). Two rules are non-negotiable regardless of choices: no wall-clock time,
no unseeded randomness, no "whatever is in the DB"; and cleanup lives where it **runs on failure
too** — the framework's guaranteed teardown, not after the last assertion.

---

## Step 5 — The hunting record (do not leave this for the last week)

Requirement-derived cases — everything above — structurally cannot find defects the requirements
never mentioned. The spec requires two artifacts that hunt for those, and both are cheap:

**The error-guess log.** Three or more one-liners in the note's format, each **sourced** — from your
own P2 defect history (what actually broke during the P2 sprint? your PR history remembers), the
known-fragile input classes (empty, whitespace, null, zero, negative, max length, apostrophes and
unicode in names, month-end dates), or churn (the code one person wrote alone at 2 AM before the P2
demo). Example of the format — source yours from *your* history, not this line:

```
GUESS-01  Hypothesis: registering a username differing only by case creates a duplicate account
          Source:     fragile-input class (case sensitivity); no P2 story ever mentioned case
          Case:       TC-31  Result: (filled in after running it)
```

A confirmed guess is a defect your requirements missed — fix it via a finding-traced PR, then
graduate the guess into a permanent regression case with a real RTM row (the log and the
graduation are both required deliverables).

**Exploratory sessions — the spec requires two; here is the first.** Pick the riskiest area your
guesses point at, write a one-sentence
charter *before starting* ("Explore <area> under <condition> to discover <kind of defect>"), set a
60-120 minute timebox, and take notes as you go: what you covered, what you saw, what surprised
you. Afterward, resolve every finding one of three ways — a defect against a requirement (file it,
regression case, RTM mark), a **requirements gap** (behavior nobody specified — write the question
down; this is the finding type nothing else in your suite can produce), or surprising-but-correct
(note it). The charter, notes, and map-backs go in the repo with everything else.

---

## The loop, and the failure modes we will look for

Per requirement, the whole discipline is six questions: *What shape is it? So which technique? So
which values/combinations? Case docs written? RTM marked? Lowest layer that proves it?* Run the loop
on the next empty row until no row is empty — or until the remaining empties are **declared residual
risk** in your README with a reason, which is an honest and acceptable answer.

Four failure modes, all visible in the evidence the spec grades:

1. **The backfilled RTM** — a matrix that appears complete in one late commit. The git history is
   the tell.
2. **Doc-after-code** — case docs that trail their automation PRs. Same tell.
3. **The untraced case** — a test that maps to no requirement. Either it found an undocumented rule
   (document it — genuine finding) or it is waste (delete it). In the grid RTM form this shows as an
   empty case column; in the list form, the Step 1 reverse-read sweep finds it.
4. **The assert-everything case** — one case checking six behaviors. A red run should name one
   thing; one behavior per case.

**End of your first project block, you should have:** the requirement inventory, the committed RTM
skeleton, and one requirement fully through the loop — designed, marked, and automated at every
layer taught so far (unit and integration). That is the spec's "Stuck?" ramp with its first two
legs green on day one; the E2E leg of this same requirement is your first Cypress spec when that
work starts, and every remaining requirement is a variation of the one you finished.
