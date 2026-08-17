# DevOps and CI/CD Fundamentals

## Learning Objectives
- Define DevOps as culture + practices + tooling that shortens the loop between a code change and
  reliable production, name the CALMS pillars, and walk the infinity-loop stages.
- Contrast DevOps with Agile: what each one optimizes, why Agile without DevOps "ships to a
  queue", and how Scrum ceremonies relate to pipelines.
- State the DORA four key metrics and what an elite team's numbers look like.
- Explain Continuous Integration: daily merges to a shared mainline, every merge builds and tests
  automatically, and what a CI server actually does.
- Distinguish Continuous Delivery from Continuous Deployment (one manual gate) and describe a
  staged pipeline with fail-fast ordering, artifacts, environments, and rollback strategies.
- Name the major CI/CD tools, say how Jenkins differs, and read a small declarative Jenkinsfile.

## Why This Matters
"Describe your CI/CD pipeline" is one of the most common non-coding interview questions for a
.NET developer, and "what is the difference between continuous delivery and continuous
deployment?" is the follow-up that sorts people who have lived it from people who have heard the
words. On the job, the pipeline is where your tests earn their keep: a suite nobody runs on every
merge is a suggestion; a pipeline that runs it is a contract. The vocabulary — mainline, artifact,
gate, environment, rollback — is what lets a developer own a failing build instead of waiting for
someone else, and answer "how do you know it works in production?" with a mechanism, not a hope.

## The Concept

### What DevOps is
DevOps is the combination of **culture, practices, and tooling** that shortens the time between
committing a change and running it reliably in production. Before DevOps, development and
operations were separate departments with opposing incentives — developers rewarded for shipping
change, operators for preventing outages — so change went "over the wall" in large, infrequent,
risky releases. DevOps removes the wall: the people who build the software share responsibility
for running it, and the release path is automated so small changes flow constantly instead of
accumulating.

A common way to summarize the movement is **CALMS**: **C**ulture (shared ownership, blameless
post-mortems), **A**utomation (build, test, deploy, infrastructure — anything repeated by hand is
a defect), **L**ean (small batches, limit work in progress, remove waste), **M**easurement (you
can only improve what you observe — deployment counts, failure rates, latency), **S**haring
(knowledge, tools, and successes cross team lines).

The **infinity loop** names the stages a change passes through, over and over: **plan** ->
**code** -> **build** -> **test** -> **release** -> **deploy** -> **operate** -> **monitor** ->
back to plan. The point is that monitoring feeds planning — production behavior drives the next change.

### DevOps vs Agile
Agile answers "how does a team plan and build software in short iterations with feedback?" DevOps
answers "how does each finished increment reach production reliably and quickly?" They are
complementary, and both are about shortening feedback loops; a team can be very Agile and still
release quarterly because the release process is manual — that is Agile "shipping to a queue".

| | Agile | DevOps |
|---|---|---|
| Question it answers | How do we build the right thing in small steps? | How does each step reach production safely? |
| Loop it shortens | Idea -> working increment (sprint) | Commit -> running in production |
| Unit of work | User story / backlog item | Change / build / deployment |
| Rituals | Sprint planning, daily stand-up, review, retro | Pipeline runs, deployment reviews, post-mortems |
| Who | Product + development team | Development + operations (ideally one team) |
| Failure mode without the other | Fast increments pile up undeployed | Fast deployments of the wrong features |

Adjacent question: "Scrum ceremonies vs pipelines?" — ceremonies coordinate people; pipelines
coordinate machines. The sprint review shows an increment; the pipeline makes it deployable.

### Measuring it: the DORA metrics
The DevOps Research and Assessment program (DORA) measures delivery performance with four key
metrics (a fifth, rework rate, was added recently). Interviewers expect the four:

| Metric | Question | Elite (order of magnitude) |
|---|---|---|
| Deployment frequency | How often does code reach production? | On demand — multiple times a day |
| Lead time for changes | Commit -> running in production? | Under a day |
| Change failure rate | What share of deployments cause an incident or rollback? | Roughly 0-15% |
| Time to restore service | Outage -> recovered? | Under an hour |

Two are speed, two are stability; the research finding is that elite teams are better at *both*.

### Continuous Integration
**Continuous Integration** is the practice of every developer merging to a shared mainline at
least daily, with every merge triggering an automated build and test run. The rules that make it
work:

- **The build is the source of truth.** If it is not green on the CI server, it is not done — "it
  works on my machine" is not a status.
- **Fail fast, fix immediately.** A red mainline blocks everyone; fixing it (or reverting) is the
  team's top priority.
- **Small, frequent merges.** Integration pain grows with the size of the diff; long-lived feature
  branches are the opposite of CI, which is why CI teams tend to **trunk-based development**:
  short-lived branches (hours to a couple of days) merged behind a passing build, with feature
  flags hiding unfinished work.

What a CI server actually does on each trigger:

```text
1. Trigger   - webhook from the repository (push / pull request) or polling on a schedule
2. Checkout  - clone the exact commit into a clean workspace (or a fresh container)
3. Build     - dotnet restore; dotnet build -c Release --no-restore
4. Test      - dotnet test --no-build --logger trx   (unit first; slower suites later)
5. Publish   - test reports, coverage, the built package or image
6. Report    - status back to the commit / PR (green check or red X), notify on failure
```

Trade-off: CI costs build minutes and discipline (tests must be fast and deterministic — a flaky
test destroys trust in the red X). The payoff: integration problems surface minutes after the
commit that caused them, while the author still remembers the change.

### Continuous Delivery vs Continuous Deployment
Both extend CI past the build. The difference is one manual gate:

| | Continuous Delivery | Continuous Deployment |
|---|---|---|
| What every green build is | **Deployable** to production | **Deployed** to production |
| Trigger for prod | A human presses the button / approves | Automatic on green |
| Who decides timing | The business (release when ready) | The pipeline |
| Typical for | Regulated releases, coordinated launches | SaaS with strong automated tests and monitoring |
| Prerequisite | Automated deploy to staging, releasable mainline | Everything Delivery needs plus enough test/monitor confidence to remove the human |

Continuous Delivery says "we *could* ship any build"; Continuous Deployment says "we *do* ship
every build". Most teams live in Delivery with a one-click promotion; Deployment is what
Delivery becomes when the last approval is trusted to automation.

### The pipeline: stages, gates, fail-fast ordering
A pipeline is an ordered set of **stages**, each a **gate**: a stage that fails stops the run.
A representative shape for a Web API:

```text
build -> unit tests -> integration tests -> package / containerize -> push artifact or image
      -> deploy to staging -> end-to-end / smoke tests -> (approval) -> deploy to production
```

Order by cost: cheap fast checks first (compile, unit tests in seconds), expensive slow ones later
(integration tests needing a database, end-to-end tests driving a browser). A compile error
should never wait behind a ten-minute Selenium run.

**Artifacts** are the outputs a stage keeps: test reports (`.trx`, JUnit XML), coverage,
published binaries or a zip, and — in a container world — the image pushed to a registry. They
are kept so later stages deploy *the same bits* that were tested (never rebuild per environment)
and so a failure can be diagnosed after the workspace is gone.

**Environments** — dev, test, staging, production — are progressively more production-like
targets. Configuration and secrets (connection strings, API keys) belong to the environment, not
the code: the twelve-factor rule. The same artifact reads different environment variables or a
key vault in each; baking a production connection string into the build makes the artifact
unpromotable and leaks a secret into version control.

Adjacent vocabulary at name depth:

- **Infrastructure as code** — environments described in files (Terraform, Bicep, ARM), versioned
  and applied by the pipeline, so "staging" is reproducible rather than hand-built.
- **Rollback** — redeploy the previous known-good artifact; cheap precisely because artifacts are
  kept and deployments are automated.
- **Blue-green** — two identical production environments; deploy to the idle one, switch traffic,
  switch back to roll back.
- **Canary** — send a small percentage of traffic to the new version, watch metrics, widen or
  abort.
- **Pipeline as code** — the pipeline definition (Jenkinsfile, GitHub Actions YAML, Azure
  Pipelines YAML) lives in the repository next to the app: versioned, reviewed, branch-aware.

### The tools, and where Jenkins sits
- **Jenkins** — open source, self-hosted, plugin-driven; a controller schedules work onto agents.
- **GitHub Actions** — YAML workflows in `.github/workflows/`, hosted runners, tightest GitHub fit.
- **GitLab CI** — `.gitlab-ci.yml`, built into GitLab, hosted or self-managed runners.
- **Azure DevOps Pipelines** — YAML or classic UI, hosted or self-hosted agents, Azure deploy tasks.
- **CircleCI** (hosted-first) and **TeamCity** (JetBrains, self-hosted or cloud).

How Jenkins differs: it is the one you run yourself. A **controller** (web UI and scheduler) farms
jobs out to **agents** (machines or containers carrying the toolchain — .NET SDK, Docker,
browsers), and nearly every capability, from Git checkout to test reports, is a **plugin**. That
is why it fits on-premises and air-gapped shops, and why it needs an owner.

A declarative **Jenkinsfile** to make the shape concrete — three stages, fail-fast by order,
reports archived even when a stage fails:

```groovy
pipeline {
    agent any                                // run on any available agent
    stages {
        stage('Build') {
            steps {
                sh 'dotnet restore'
                sh 'dotnet build -c Release --no-restore'
            }
        }
        stage('Test') {
            steps {
                sh 'dotnet test -c Release --no-build --logger "trx;LogFileName=results.trx"'
            }
        }
        stage('Package') {
            steps {
                sh 'dotnet publish src/Orders.Api -c Release -o out'
                sh 'docker build -t orders-api:${BUILD_NUMBER} .'
            }
        }
    }
    post {
        always  { archiveArtifacts artifacts: '**/TestResults/*.trx', allowEmptyArchive: true }
    }
}
```

Read it as: `agent` says where it runs; each `stage` is a gate whose `steps` are shell commands or
plugin steps; `post` runs after the stages regardless of outcome. Push, deploy, and approval
stages extend the same list.

## Say It in an Interview
- **"What is DevOps?"** — "The culture, practices, and tooling that shorten the loop from a code
  change to reliable production — developers and operations owning the outcome together, with the
  build, test, and deploy path automated. CALMS is the usual summary: culture, automation, lean,
  measurement, sharing."
- **"DevOps vs Agile?"** — "Agile is how a team plans and builds in short iterations; DevOps is
  how each increment gets to production reliably. Both shorten feedback loops. Agile without
  DevOps means finished work waits in a release queue."
- **"How do you measure delivery performance?"** — "The DORA four keys: deployment frequency,
  lead time for changes, change failure rate, and time to restore. Elite teams deploy on demand
  with lead time under a day, low failure rate, and recovery under an hour — fast *and* stable."
- **"What is Continuous Integration?"** — "Everyone merges to a shared mainline at least daily,
  and every merge triggers an automated build and test run. The CI server's green or red status
  is the source of truth, and a red mainline is fixed before anything else."
- **"Continuous Delivery vs Continuous Deployment?"** — "Delivery: every green build is
  deployable and production is a button press or approval. Deployment: every green build goes to
  production automatically. The difference is one manual gate."
- **"Describe a CI/CD pipeline."** — "Stages as gates: build, unit tests, integration tests,
  package or containerize, push the artifact, deploy to staging, smoke or end-to-end tests, then
  promote the same artifact to production. Cheap checks first, artifacts kept, config and secrets
  from the environment, rollback is redeploying the previous artifact."
- **"Jenkins vs GitHub Actions?"** — "Both run pipeline-as-code. Jenkins is self-hosted, a
  controller with agents, everything via plugins — full control, you maintain it. Actions is
  hosted YAML tightly bound to GitHub — less to run, less to control."

## Check Yourself
1. A team runs two-week sprints with a sprint review every Friday and releases to customers every quarter.
   Are they Agile? Are they doing DevOps? What is missing?
2. Which two DORA metrics measure stability, and what would you look at first if change failure
   rate spiked after a pipeline change?
3. Your pipeline runs a 12-minute browser test suite before `dotnet build`. What principle does
   that violate and how would you reorder it?
4. Every green build is deployed to staging automatically; production needs a release manager's
   click. Delivery or Deployment?
5. Why should the image deployed to production be the one that was tested in staging, rather than
   rebuilt from the same commit?

**Answers:** (1) Agile yes (iterations, reviews); DevOps no — increments queue for a quarterly
release; missing is an automated build/test/deploy path that makes each increment shippable.
(2) Change failure rate and time to restore; check what the pipeline change skipped — a test stage
removed or made non-blocking, a gate now auto-approving. (3) Fail-fast ordering: compile and unit
tests first, integration next, the browser suite last. (4) Continuous Delivery — one manual gate
remains. (5) Rebuilds are not guaranteed identical (dependency resolution, base-image drift);
promoting the same digest is what makes "tested = shipped" true.

## Resources
- [Continuous Integration (Martin Fowler)](https://martinfowler.com/articles/continuousIntegration.html)
- [Continuous Delivery (Martin Fowler, bliki)](https://martinfowler.com/bliki/ContinuousDelivery.html)
- [DORA's software delivery performance metrics](https://dora.dev/guides/dora-metrics-four-keys/)
- [What is DevOps? (Microsoft Learn)](https://learn.microsoft.com/en-us/devops/what-is-devops)
- [Jenkins Pipeline syntax (declarative)](https://www.jenkins.io/doc/book/pipeline/syntax/)
