# Designing Test Cases: Traceability, Technique Selection, and Structured Exploration

## Learning Objectives
- Write a test case that another person can execute and get the same verdict from: identifier, trace,
  preconditions, steps, expected result.
- Apply the testing principles as design rules — what each one changes about which cases you write and
  which you refuse to write.
- Build and read a requirements traceability matrix (RTM), and name the two questions it answers that a
  test count cannot.
- Select a technique from the *shape* of a documented requirement rather than by habit, and say why the
  chosen one fits.
- Design a case set optimized for coverage per case, and state what the optimization costs you when a
  test fails.
- Choose test data from the test objective, then choose its storage home from the test object.
- Run error guessing as a structured, repeatable, documented activity rather than as intuition.
- Plan, timebox, and document exploratory testing with charters and session notes, map findings back to
  the RTM, and argue both sides of scripted versus exploratory.

## Why This Matters
Anyone can write a test. Writing the *right* twelve tests out of a possible ten thousand, being able to
defend why those twelve, and proving that every requirement has at least one — that is the part teams pay
for, and it is the part that does not come free with a framework. A suite of two hundred tests with no
traceability cannot answer "is this release safe?", because nobody can say what it covers. A suite designed
by intuition tests what its author found interesting and silently omits what they never considered.

This is also where testing interviews separate candidates. "Write a unit test" is a warm-up. "Here is a
requirement — what cases would you write, and why those?" is the real question, and the follow-ups are
always the same three: how do you know you covered it, how did you choose your data, and what do you do
about the defects your cases would never find. This note answers all three.

## The Concept

This note assumes the vocabulary and takes it somewhere. The seven testing principles, the black-box /
white-box distinction, the test pyramid, and the basic idea of a requirement are covered in a
testing-fundamentals note; equivalence partitioning and boundary-value analysis as *mechanics* are
covered where theory rows get chosen in an xUnit note. What follows is about **choosing and defending a
case set** — which technique, how many cases, which data, and what to do about the defects no technique
derived from a requirement can ever find.

### What a test case actually is
A test case is a repeatable experiment with a predicted outcome. Minimum viable shape:

| Field | Purpose |
|---|---|
| Identifier | Something to refer to in a defect report, a matrix, a conversation |
| Trace | The requirement this case proves — the reason the case is allowed to exist |
| Preconditions | The state the world must be in before step 1 |
| Steps | What is done, precisely enough that two testers do the same thing |
| Expected result | The predicted outcome, stated *before* execution |
| Actual result / status | Filled in at execution time |

The two fields that get skipped are the two that matter. **Expected result written in advance** is what
makes it a test rather than an observation — decide the verdict before you run, or you will rationalize
whatever you see. **Trace** is what makes the case auditable: a case with no requirement behind it is
either testing an undocumented rule (find the rule) or testing nothing anyone asked for (delete the case).

An automated test carries the same fields in a different notation: the method name is the identifier, the
arrange block is preconditions, the act is the steps, the assert is the expected result, and the trace
lives in a comment, an attribute, or the naming convention. The discipline does not disappear when the
runner shows up — it changes format.

### The principles as design rules
The principles are commonly recited as a list. Their value is as constraints on the case set you produce:

- **Exhaustive testing is impossible** → your case set is a *sample*, and every sampling decision needs a
  stated basis. This is the principle that makes technique selection mandatory rather than optional.
- **Defects cluster** → weight the sample. The module that has been rewritten twice this quarter earns
  more cases than the one untouched for two years, at equal requirement count.
- **The pesticide paradox** → a case set has a shelf life. Cases that have never failed in two years are
  not proving much; the fix is new cases and new techniques, not more runs of the old ones.
- **Testing is context dependent** → the same requirement deserves different depth in a payments service
  and an internal admin screen. "How many cases?" has no answer without the context.
- **Early testing** → the cheapest case to design is the one designed against the requirement, before the
  code exists, because writing it is what exposes the requirement's ambiguity.
- **Testing shows presence, not absence** → design so that a failure is *informative*. One behavior per
  case, so a red result names one thing. A case asserting six unrelated outcomes tells you something broke
  and makes you go find out what.
- **Absence-of-defects fallacy** → some of your effort belongs on whether the requirement itself is right,
  which no case traced to that requirement can ever tell you. This is the gap exploratory testing fills.

### The requirements traceability matrix
An RTM is a grid: requirements down one axis, test cases across the other, a mark where a case proves a
requirement. It can live in a spreadsheet, a test-management tool, or a markdown table — the format is
uninteresting, the discipline is not.

Take four requirements for a library loan feature:

| Req | Statement |
|---|---|
| R-1 | A member may hold at most 5 loans at a time |
| R-2 | A checkout request is for 1 to 3 books |
| R-3 | A book with zero available stock cannot be checked out |
| R-4 | A checkout by an unknown member is rejected with a "member not found" error |

| Requirement | TC-01 | TC-02 | TC-03 | TC-04 | TC-05 | Covered |
|---|---|---|---|---|---|---|
| R-1 max loans | X | X | | | | yes |
| R-2 quantity range | | | X | | | yes |
| R-3 zero stock | | | | X | | yes |
| R-4 unknown member | | | | | | **no** |

Read it in both directions, because each direction answers a different question:

- **Along a requirement row** — which cases prove this? An empty row is a **coverage gap**: R-4 above is
  a stated requirement nobody is testing, and it is invisible in a report that says "5 test cases, all
  passing." This is the finding an RTM exists to produce.
- **Down a case column** — what does this case prove? An empty column is **waste**: TC-05 traces to
  nothing. Either it is testing an undocumented rule — in which case the rule needs documenting, and that
  is a genuine finding — or it is testing nothing anyone asked for and should be deleted rather than
  maintained forever.

The second use is **impact analysis**. When R-1 changes from five loans to ten, the row tells you exactly
which cases must be revisited. Without the matrix that question is answered by grepping and hoping.

The honest caveat: an RTM proves *linkage*, not *adequacy*. A single weak case in R-1's row makes the row
non-empty and the requirement no better tested than before. Traceability is a floor, not a ceiling — it
tells you what is uncovered, never that what is covered is covered well.

### Selecting a technique from the requirement's shape
Technique selection is a lookup, not an inspiration. Read the requirement, identify its shape, choose
accordingly:

| Requirement shape | Recognize it by | Technique | Cases it produces |
|---|---|---|---|
| A numeric or ordered limit | "at most", "between", "over", "before" | Boundary-value analysis | The edge, one below, one above |
| Inputs grouped into classes treated alike | "valid members", "any supported format" | Equivalence partitioning | One representative per class, valid and invalid |
| Several conditions combining | "if A **and** B, then C, unless D" | Decision table | One case per rule of the combination |
| An entity moving between states | "pending", "approved", "cancelled" | State transition testing | Legal transitions, plus attempts at illegal ones |
| A sequence a user follows | "the member searches, selects, then checks out" | Use-case / scenario testing | The path end to end, plus its realistic deviations |
| Internal logic you can read | Source available, branches to reach | Statement / branch / path coverage | Cases chosen to reach unexecuted branches |
| Anything, after the above | The requirement is fully covered on paper | Error guessing, exploratory | Cases about what the requirement failed to say |

Two rules for using this table. First, requirements usually have **more than one shape**: R-1 above is a
numeric limit (boundary values) whose input also partitions into valid, negative, and above-limit classes
(equivalence partitioning) — apply both, and the techniques overlap on purpose. Second, the last row is
not optional. The first six techniques all derive cases from what the requirement *says*, so none of them
can find a defect the requirement never anticipated. That whole category of defect is invisible to
technique-driven design, which is why the last two sections of this note exist.

Worked, on R-1 and R-3 together. R-1 is a limit: boundaries at 4, 5, 6. Its input also partitions:
negative (invalid), 0-5 (valid), above 5 (invalid) — representatives -1, 3, 9. R-3 is a class rule about
stock: zero versus positive, with zero itself being the boundary. The combination of "member under the
limit" and "stock available" is a two-condition rule, so a small decision table catches the interaction
that per-input testing misses:

| Under loan limit | Stock available | Expected |
|---|---|---|
| yes | yes | checkout succeeds |
| yes | no | rejected, out of stock |
| no | yes | rejected, loan limit reached |
| no | no | rejected — and *which* message? |

That fourth row is the payoff. Testing each input separately never asks it, and the requirements as
written do not answer it. That is not a test failure; it is a **requirement defect found during design**,
which is the cheapest possible place to find one, and it goes back to whoever owns R-1 and R-3 as a
question before anyone writes code.

### Designing for coverage per case
Given limited time, you want the case set that proves the most per case written. Four moves, in order:

1. **Cover every requirement once before covering any requirement twice.** A second boundary case on R-1
   is worth less than the first case on R-4, always. The RTM makes this ordering visible; without it,
   teams reliably over-test what they find interesting.
2. **Combine independent inputs into one case where a failure would still be unambiguous.** One negative
   case can use an invalid quantity *and* an unknown member only if you accept that a failure requires a
   second look to attribute. Combine setup and preconditions freely; keep the *asserted behavior*
   singular.
3. **Prefer cases that are cheap to run repeatedly.** A case at the unit level costs milliseconds and runs
   on every commit; the same rule proven end to end costs seconds and runs nightly. Push each case as far
   down as it can go while still proving the requirement — the pyramid, applied at design time.
4. **Delete redundancy deliberately.** Two cases in the same equivalence class prove the same thing at
   twice the maintenance cost. If you cannot say what a case proves that its neighbor does not, that is
   your answer.

The cost, stated plainly because interviewers ask for it: **optimized case sets diagnose worse.** A lean
combined case tells you something is broken with less precision than three narrow ones would have. You are
trading debugging time for design and execution time, which is the right trade when the suite is mostly
green and the wrong trade in a module that fails constantly. Where defects cluster, un-optimize.

### Test data: the objective picks the data, the object picks its home
Two separate decisions, routinely collapsed into one and then argued about at cross purposes.

**What data? Ask the test objective.** The objective is the one thing the case proves, and the data is the
smallest set that could prove it. To prove the out-of-stock rejection you need exactly one title with zero
stock. Not a realistic catalog, not a hundred rows — one row with the property under test, plus whatever
else the system needs to function. Every extra row is a thing that can drift, break, and cost you a
morning. State the objective first and the data almost falls out; skip that and you get "some test data"
that nobody can later justify or safely change.

**Where does it live? Ask the test object.** The object is what is actually under test, and it decides
which storage home is honest:

| Home | What it is | Buys | Costs |
|---|---|---|---|
| Inline in the case | Literal values in the test itself | Maximum locality — the data is visible where it is used | Does not scale past a handful of values; duplicates across cases |
| A data file | JSON/CSV read by the case or the runner | One edit updates many cases; non-programmers can review it | Drifts from the real schema silently; the file and the code disagree at run time |
| A seeded store | The real database put into a known state before the run | Proves the real system accepts and returns this data | Requires reset discipline; a failed run leaves residue that poisons the next |
| Created by the case | The case creates what it needs, then removes it | Self-contained; no shared-state coupling | Slow; and cleanup that runs only on success leaks state on failure |
| A stub or mock | The dependency is replaced with a scripted response | Total determinism, including states the real system cannot produce on demand | Proves nothing about whether the real dependency agrees |

The trade is one axis: **determinism versus proof**. Files and stubs are fast and repeatable and stop
proving the real system agrees. Seeded and created data prove it and charge you cleanup discipline. Neither
is correct in general; what is incorrect is not knowing which one you chose. A practical default: stub what
the case is *not* testing, use real data for what it *is* testing, and keep one small unstubbed suite whose
whole job is to notice when the stubs have drifted from reality.

Two habits that prevent the common failures. **Determinism**: no `DateTime.Now`, no random values, no
"whatever is in the database today" — a case whose data changes underneath it fails on Tuesdays and
teaches the team to ignore red. Inject a fixed clock, seed random generators, pin the rows. And **cleanup
that runs regardless of outcome**: teardown placed after the assertions only executes when the assertions
pass, so the one run that fails is the one that leaves residue behind — put it in the framework's
guaranteed-teardown hook instead.

### Error guessing, made repeatable
Error guessing is designing cases from expectations about where defects are, rather than from the
requirement text. Everyone does it informally; the difference between a technique and a hunch is whether
it is **sourced, documented, and repeatable by someone else.**

Source the guesses rather than inventing them:

- **Past defect history.** What has actually broken in this codebase or product before? Defects cluster,
  and the cluster is on record in the bug tracker.
- **Known-fragile input classes.** Empty string, whitespace-only, `null`, zero, negative, maximum length,
  Unicode and emoji in a name field, a leading `0` on a numeric string, apostrophes in surnames, dates at
  month and year ends, values at the exact limit.
- **Domain knowledge.** A librarian knows members return books in bulk after a holiday; a developer knows
  the bulk path was written last and tested least.
- **Structural suspicion.** Recently changed code, the module with the highest churn, anything one person
  wrote alone under deadline, and every boundary where two systems meet.

Document each guess so it survives you. A one-line format is enough, and the point is that a second tester
can rerun the exercise and get comparable results rather than a different person's intuitions:

```
GUESS-07  Hypothesis: checkout with 3 books where only 2 are in stock partially succeeds
          Source:     similar partial-write defect in returns, ticket 4412
          Case:       TC-19  Result: confirmed - 2 loans created, request reported as failed
```

What it buys: defects that requirement-derived cases structurally cannot find, at very low cost, because
each guess is a single case. What it costs: coverage is entirely a function of the guesser's experience,
which is why sourcing from defect history matters more than seniority, and why a confirmed guess should
immediately become a permanent regression case with a real requirement trace behind it.

### Exploratory testing: planned, timeboxed, documented
Exploratory testing is simultaneous learning, design, and execution — you decide the next action based on
what the last one revealed. It is *not* unstructured clicking, and the difference is entirely in the
scaffolding around it.

**The charter** states the mission before the session starts, narrow enough to be answerable in one
sitting: *"Explore the checkout flow with members at or near their loan limit, using the mobile viewport,
to discover defects in limit enforcement and its error messaging."* A charter naming an area, a technique
or condition, and a purpose keeps the session from becoming a tour.

**The timebox** is the second constraint — typically 60 to 120 minutes. It forces a stopping point and
makes sessions comparable and schedulable, which is what lets exploratory work appear in a test plan
alongside scripted execution instead of being the thing done "if there's time."

**The session notes** are the deliverable. Record what you covered, what you observed, defects found,
questions raised, and how the time actually split between testing, investigating a specific bug, and
fighting the environment. That last number is the one that surprises managers and justifies fixing test
environments.

**Mapping findings back** is the step that gets skipped, and it is where the real value lands. Each finding
resolves to one of three things:

1. A **defect against an existing requirement** — file it, and add a scripted regression case, which puts
   a new mark in that requirement's RTM row.
2. A **gap in the requirements** — the behavior is undefined, and neither "works" nor "broken" applies.
   This becomes a question for the requirement's owner, and it is the finding type no scripted case could
   ever have produced.
3. **Correct behavior that surprised you** — worth a note; a surprised tester today is a confused user
   tomorrow.

Benefits and drawbacks, both directions, because the interview question is always comparative. Exploratory
testing finds what scripted cases cannot — the unstated requirement, the interaction nobody modeled, the
workflow that is technically correct and unusable — and it needs no upfront case-writing, so it is the
fastest way to learn a new build. Its drawbacks are equally real: coverage depends on the tester, results
are hard to reproduce without disciplined notes, it is not automatable, and it can look unaccountable to a
manager reading a report. Scripted cases are the mirror image — repeatable, automatable, measurable,
auditable, and structurally blind to anything their requirement failed to mention.

Which is why every serious test strategy runs both: scripted cases prove the requirements are met, and
exploratory sessions probe whether the requirements were right.

## Say It in an Interview
- *"A test case needs an identifier, the requirement it traces to, preconditions, steps, and an expected
  result written before execution. The expected result written in advance is what makes it a test instead
  of an observation, and the trace is what makes it auditable — a case that traces to nothing is either
  finding an undocumented rule or is waste."*
- *"The principles aren't slogans, they're design rules. Exhaustive testing is impossible, so my case set
  is a sample and every sampling decision needs a stated basis. Defects cluster, so I weight the sample
  toward recently changed and complex code. The pesticide paradox means a fixed case set has a shelf
  life. And 'testing shows presence, not absence' is why I keep one behavior per case — so a red result
  names one thing."*
- *"An RTM is requirements against test cases. I read it both ways: an empty requirement row is a coverage
  gap, an empty case column is waste, and when a requirement changes the row tells me exactly what to
  revisit. What it does not prove is adequacy — a weak case fills the row just as well as a strong one."*
- *"I pick the technique from the requirement's shape. A limit gets boundary values, inputs that group
  into classes get equivalence partitioning, combining conditions get a decision table, a lifecycle gets
  state transitions, and then error guessing and exploratory work cover what the requirement never said —
  because no requirement-derived technique can find a defect the requirement failed to anticipate."*
- *"To optimize a case set I cover every requirement once before covering any twice, combine setup but
  never the asserted behavior, and push each case as far down the pyramid as it can go. The cost is
  diagnosis: leaner cases localize failures less precisely, so where defects cluster I deliberately
  un-optimize."*
- *"Test data is two decisions. What data comes from the test objective — the smallest set that could
  prove the one thing. Where it lives comes from the test object, and the axis is determinism versus
  proof: fixtures and stubs are repeatable and stop proving the real system agrees, seeded and created
  data prove it and cost cleanup discipline."*
- *"Error guessing is only a technique if it's sourced and written down — I drive it off past defect
  history, known-fragile inputs like null, empty, boundary and max-length values, and recent churn, and I
  log each hypothesis with its source and result so someone else can rerun it. Confirmed guesses become
  permanent regression cases."*
- *"Exploratory testing is charter, timebox, session notes. It's simultaneous design and execution, not
  unstructured clicking. Findings map back three ways: a defect against a requirement, a gap in the
  requirements, or surprising-but-correct behavior. It finds what scripted cases can't and can't be
  automated or reproduced as reliably — so you run both."*

## Check Yourself
1. A colleague shows you a suite of 200 passing automated tests and says the release is safe. What single
   artifact would you ask for, and what two questions would you use it to answer?
2. Requirement: "A promotional discount applies to orders of 10 or more items, except for members whose
   account is suspended." Name the two techniques this requirement's shape calls for, and give the case
   set each produces.
3. You have time for eight cases and twelve requirements, three of which are in a module rewritten twice
   this quarter. How do you allocate, and which principle justifies it?
4. A test needs a member holding exactly five loans. Give two different homes for that data, and say what
   each one stops proving.
5. What class of defect can no amount of equivalence partitioning, boundary analysis, or decision-table
   design ever find, and what do you do about it?
6. Your teammate says exploratory testing is "just clicking around." Give the three-part structure that
   distinguishes it, and name the finding type it produces that a scripted case cannot.
7. A cleanup step at the end of a test case deletes the record the case created. Why is this the wrong
   place for it, and when specifically does it bite?

**Answers:** (1) The requirements traceability matrix. Which requirements have no case at all (an empty
row is a coverage gap that "200 passing" actively conceals), and which cases trace to no requirement
(waste, or an undocumented rule worth surfacing). Add the caveat that the RTM proves linkage, not
adequacy. (2) It has two shapes. The "10 or more" limit is a boundary — cases at 9, 10, and 11. The
account status is a condition combining with the quantity, so a decision table: suspended/not-suspended
crossed with above/below the threshold, four rules. The table's payoff is proving the exception
*overrides* the quantity rule in both directions — a suspended member at 25 items gets no discount, and a
non-suspended member at 9 gets none either — which testing each input on its own never asks. (3) You
cannot cover twelve requirements with eight cases, and pretending otherwise is the trap in the question.
Pick the eight by risk: the three in the twice-rewritten module first (*defects cluster*), then the five
highest-risk of the remaining nine, one case each — cover every requirement you are going to cover once
before covering any of them twice. Then report the four uncovered requirements explicitly as empty RTM
rows and declared residual risk, so the gap is a decision on record rather than a silence.
(4) Seeded into the real database: proves the system can
actually reach that state and that the query reads it correctly, but requires reset discipline and leaks
residue when a run fails. Stubbed at the repository: total determinism and instant, but proves nothing
about whether the real data layer would ever return that shape. (Inline and created-by-the-case are also
valid answers with their own trade.) (5) Defects arising from what the requirement *failed to say* — the
unstated rule, the unmodeled interaction, the undefined state. All requirement-derived techniques inherit
the requirement's blind spots. The answers are error guessing (structured, sourced from defect history and
fragile-input classes) and exploratory testing, plus feeding the resulting requirement gaps back to their
owner. (6) Charter (a narrow mission stated before you start), timebox (a fixed session length that makes
the work schedulable and comparable), and session notes (coverage, observations, defects, questions, and
how the time actually split). The unique finding type is a *requirements gap* — behavior nobody specified,
where neither "works" nor "broken" applies — which no case traced to a requirement could produce. (7)
Because it only runs when every assertion before it passed: the one run that fails is precisely the run
that leaves the record behind, and the next run then fails on a duplicate instead of the original defect —
a failure that masks its own cause. Put cleanup in the framework's guaranteed-teardown hook, or give each
case its own unique keys so leftovers cannot collide.

## Summary
- A test case is identifier, trace, preconditions, steps, and an expected result written *before*
  execution; automated tests carry the same fields as name, arrange, act, and assert.
- The principles are design constraints: sample deliberately (exhaustive testing is impossible), weight
  the sample (defects cluster), refresh the set over time (pesticide paradox), and set depth by context.
- An RTM maps requirements to cases. Empty row = coverage gap; empty column = waste; a changed
  requirement's row = the impact analysis. It proves linkage, never adequacy.
- Select technique by requirement shape: limits → boundary values; classes → equivalence partitioning;
  combining conditions → decision table; lifecycles → state transitions; sequences → scenarios; readable
  internals → branch coverage. Then error guessing and exploratory for what the requirement never said.
- Optimize by covering every requirement once before any twice, combining setup but not asserted behavior,
  and pushing cases down the pyramid — at the cost of weaker failure localization.
- Test data: the *objective* decides what data (smallest set that proves the one thing); the *object*
  decides where it lives (inline, file, seeded store, created-in-test, stub). The axis is determinism
  versus proof. Keep it deterministic, and put cleanup where it runs even on failure.
- Error guessing is a technique only when sourced (defect history, fragile-input classes, churn) and
  documented as hypothesis / source / case / result; confirmed guesses graduate to regression cases.
- Exploratory testing = charter + timebox + session notes, with findings mapped back as defects,
  requirement gaps, or surprises. It finds what scripted cases cannot; scripted cases are repeatable and
  automatable where it is not. Run both.

## Resources
- [Requirements traceability (Wikipedia)](https://en.wikipedia.org/wiki/Requirements_traceability)
- [Equivalence partitioning (Wikipedia)](https://en.wikipedia.org/wiki/Equivalence_partitioning)
- [Exploratory Testing — James Bach (satisfice.com)](https://www.satisfice.com/exploratory-testing)
- [SBTM Session Report Checklist (satisfice.com)](https://www.satisfice.com/sbtm)
