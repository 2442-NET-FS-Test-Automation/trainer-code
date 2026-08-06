# Honest-Gap Talking Sheet — TDD and BDD

Two topics interviewers ask about where your training gave you *reading knowledge, not reps*.
The losing move is bluffing experience you do not have — interviewers probe one level down and
the bluff collapses, taking your credible answers with it. The winning move is a three-part
answer: **what you know cold** (taught material), **the honest boundary** (one sentence, no
apology), and **the bridge** (what you would do next, or what you did that is adjacent). Said
that way, a gap reads as self-awareness — a hiring signal, not a hole.

Practice each script out loud. Then practice the follow-ups, because the follow-up is where
honesty pays off.

---

## TDD (test-driven development)

**Where you actually stand:** the loop and its rationale are covered at interview depth in
`content/01-xunit/xunit-fundamentals.md` ("Test-driven development: writing the test first").
Your demos and P3 deliberately practiced **test-after**: code exists, then xUnit/Moq/
WebApplicationFactory tests prove it. You have never driven a feature test-first.

### What you know cold (say any of this freely)

- The loop: **red** — write a failing test first, and watch it fail so you know it can;
  **green** — write the least code that passes; **refactor** — improve under a green bar.
  Repeat in minutes-long cycles.
- What it buys: **design pressure** (writing the test first forces you to invent the seam —
  the interface, the injected dependency — before the implementation, so testable design falls
  out instead of being retrofitted); an **executable spec** that cannot drift; a **tight
  feedback loop**; and refactoring safety, since the bar is green between every step.
- Where it fits badly: exploratory spikes where you do not yet know the shape, churning UI,
  throwaway scripts — places where the test would be rewritten as fast as the code.

### The honest boundary (one sentence, pick one)

- *"I know the loop well but I have not driven real work test-first yet — my training
  deliberately taught test-after, so my reps are writing tests against existing code."*
- *"I would not claim TDD as a habit yet; I would claim solid test-after discipline and a
  clear understanding of what test-first adds."*

### The bridge (what makes the honest answer strong)

- Point at what you HAVE done that borders it: *"What I did practice is the design half —
  in P3 our test cases were designed before automation, with the expected result written
  before execution, and technique named per case. That is TDD's discipline at the case level,
  just not at the keystroke level."* (True: the P3 spec requires case docs to land before or
  with the automating PR.)
- The forward line: *"It is the first practice I want to build on the job — start with one
  bug fix: reproduce it as a failing test, then fix, which is TDD in miniature and also just
  good regression practice."*

### Follow-ups to expect

- **"Walk me through TDD on a concrete task."** Use the taught fine-calculation shape: red —
  `Fine_For7DaysOverdue_Is3_50` fails because the method returns 0; green — hard-code enough
  logic to pass; add the next row (boundary: 0 days), watch it fail, generalize; refactor
  names and duplication with everything green. You can narrate this honestly *as the loop*,
  because you are describing the method, not claiming a war story.
- **"Is 100% TDD realistic?"** No — spikes and volatile UI resist it; the value concentrates
  in domain logic with clear expected behavior.
- **"TDD vs just writing tests after?"** Same artifacts, different force: test-after verifies
  the design you already committed to; test-first *shapes* the design and guarantees the test
  can fail. The failure-first step is the part test-after silently skips.

---

## BDD (behavior-driven development)

**Where you actually stand:** awareness only, and that is normal at your level. Source:
`content/03-selenium/selenium-ecosystem.md` (framework types — BDD named alongside
data-driven/keyword-driven/hybrid) plus a one-line mention in `xunit-fundamentals.md`. Zero
Gherkin written, zero SpecFlow/Reqnroll code. Do not inflate this one; it is a
recognize-and-place topic.

### What you know cold

- BDD writes specifications as **behavior sentences** readable by non-developers, in
  **Gherkin**: `Given` a starting state, `When` an action, `Then` an outcome. Each line binds
  to a step-definition method in real code; the spec file executes.
- The point is a **shared language**: product, QA, and developers agree on the behavior in
  the artifact that actually runs, instead of a wiki page drifting away from the suite.
- The .NET tooling fact worth volunteering: **SpecFlow was the standard historically;
  Reqnroll is its successor** — knowing the succession signals current awareness.
- Where it sits relative to what you did: BDD is a *layer over* test automation, not a
  different kind of testing — a Gherkin `Then` ultimately calls the same Selenium page-object
  method or API assertion you already write.

### The honest boundary

- *"I know what BDD is for and what Gherkin looks like, but I have not written step
  definitions — my suites are plain xUnit and Cypress/Selenium specs."*

### The bridge

- *"The underlying skills transfer directly: my Selenium tests already read as user journeys
  through page objects — `SignInAs`, `Search`, assert the card — which is exactly what a
  Given/When/Then would bind to. Learning the binding layer is days, not months."* (True:
  journey-shaped specs over page objects are your taught end-state, walkthrough `09`.)
- If asked whether you would use it: the honest trade — BDD earns its overhead when
  non-developers genuinely read and contribute to the specs; when only engineers read them,
  plain well-named tests deliver the same coverage with less machinery.

### Follow-ups to expect

- **"Write me a Gherkin scenario."** Safe, because it is syntax, not experience:

  ```gherkin
  Scenario: Member at the loan limit cannot check out
    Given a member holding 5 of 5 allowed loans
    When they attempt to check out "Clean Code"
    Then the checkout is rejected
    And the response explains the loan limit was reached
  ```

- **"BDD vs TDD?"** Different audiences, same test-first spirit: TDD drives design at the
  unit level in developer language; BDD drives agreement at the behavior level in shared
  language. They compose — BDD scenarios outside, TDD underneath.
- **"What is SpecFlow?"** The historical .NET Gherkin runner; Reqnroll is the maintained
  successor. Step definitions are C# methods bound to Gherkin lines by attribute.

---

## The meta-rule (works for any gap)

1. Answer the *concept* question fully — taught material, spoken confidently.
2. Draw the boundary yourself, in one unapologetic sentence, before probing finds it.
3. Bridge to the adjacent thing you HAVE done, and the first step you would take on the job.

Never say "we didn't cover that" and stop — that ends the conversation at the gap. The
three-part shape ends it at your strengths.
