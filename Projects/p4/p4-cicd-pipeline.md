# Project 4 — A CI/CD Pipeline for Your Project-2/3 Application (Same teams, Weeks 10–11)

## Objective

Ship what you built. Your team's monorepo already holds a **working full-stack application**
(Project 2: API + SPA + database) and **the suites that prove it** (Project 3: unit, integration,
Cypress, Selenium), and since Monday the application is **live on Azure** — deployed by hand.
Project 4 replaces the hands: **a Jenkins pipeline, running on one of your laptops, that turns a
push to `main` into a tested, deployed application.** The back end is built, **gated by your
tests**, packaged as a **container image**, pushed to **your Azure Container Registry**, and
pulled automatically by **your Web App for Containers**. The front end is built against the live
API's URL and published to **your Blob static site**. Then **Cypress runs against the live site**
as a post-deploy smoke check, and its results land on the Jenkins build page beside the unit
results. Nothing a human clicks after `git push`.

The headline engineering problem is **not Jenkins syntax — it is plumbing under discipline**: a
pipeline that a teammate can read and trust, secrets that never touch the repo or the log, a
gate that actually stops a bad build, and an honest account of what your pipeline proves and what
it does not. A pipeline that happens to go green once is Wednesday news; **a pipeline your team
would let deploy on a Friday afternoon is Project 4.**

You build this in the **same team**, in the **same repository** as Projects 2 and 3, with the
**Azure resources you created by hand this week** as the target (plus one registry and one
container web app you add now), and you present it at the **terminal showcase Friday Aug 21** —
one sitting, one presentation: the product, its proof, and the pipeline that ships both.

**This is a single final spec — no staged deliverables, no tiers, final at handout.** Everything
in "What you ship" is required. The only optional lines are in "Stretch".

---

## Logistics

| | |
|---|---|
| **Handed out** | Thu Aug 20, 9:00 — with the guide index and three supplemental guides (see below) |
| **Build window** | **Thu Aug 20, all day — the trainer floats** (no lecture; raise a hand early) · Thu evening + Fri AM async |
| **Presented** | **Fri Aug 21, 14:00–17:00 — the terminal showcase, P2 + P3 + P4 in one ~25-min slot per team; every member presents** |
| **Mode** | Same P2/P3 teams (3–4), same monorepo. Work in **pairs inside the team**: one pair owns the back-end pipeline (guide 02), one pair the front-end pipeline (guide 03); both start from guide 01 on the laptop that hosts Jenkins |
| **Target** | **Your** Azure subscription: your Azure SQL (Mon), your static-site storage account (Fri/Mon), **plus** a Container Registry (Basic) and a Web App for Containers (Linux) you create in the portal now. Your Windows web app from Monday retires once the container app answers |
| **Runtime** | Jenkins = `jenkins/jenkins:lts` in Docker on a team laptop; builds run on a **Windows agent on that same laptop** (your `dotnet`/`node`/`docker`/`azcopy`/Chrome) — the laptop IS the build server for this project |
| **Stack** | Jenkins declarative pipeline (`Jenkinsfile`, Pipeline script from SCM) · Docker · Azure Container Registry · Azure App Service (Web App for Containers) · Azure Blob static website · `azcopy` · your P3 suites (xUnit, Cypress; Selenium stretch) |
| **Submission** | The team repo: both `Jenkinsfile`s + `Dockerfile` + `.dockerignore` committed; `docs/ci/` with the build-page screenshots and the README writeup |
| **Scaffold** | **No code scaffold.** No starter Jenkinsfile, no solution key. The supplemental guides show every stage's **shape** against placeholders; writing it for **your** repo, **your** folders, **your** resources is the project |

**The guides (handed out with this spec, `async-lab/` — one index + three guides):**

- `cicd-pipeline-README.md` — the index: what you are building, the names you fill in once,
  the order of work, the definition of done.
- `cicd-01-jenkins-setup.md` — Jenkins + the Windows agent from zero (the CI/CD vocabulary, the
  controller, the agent, a green hello). **Everyone reads it; one laptop does it.**
- `cicd-02-api-pipeline.md` — the back-end pipeline: Jenkinsfile from your repo → build → test
  gate with published results → image → your registry → your container web app pulling it.
- `cicd-03-spa-pipeline.md` — the front-end pipeline: SPA build against the live API → push to
  your static site → Cypress against the live site → results and screenshots on the build page.

**Where the time comes from:** Thursday is yours — no content is scheduled, the trainer walks
the room all day. Thu PM is the protected P4 sprint. Friday morning is async prep. The scope is
sized to that: every part of it you have done by hand already this week; the work is wiring.

---

## The stakeholder blurb (your acceptance spec)

> *The product works and the tests prove it — now make releases boring. We want every push to
> `main` to build, run the tests, and — only if they pass — ship the API and the site to the
> environment we already have. We want to see the results of every build in one place, we want
> the secrets out of the code, and we want one honest sentence about what the pipeline does and
> does not guarantee before we trust it with a Friday release.*

Translate that into acceptance criteria yourselves; the checklist below is the minimum.

**Reference:** the trainer's Library API pipeline (`library-api-build`: checkout → test gate →
image → ACR push; green on the trainer's Jenkins Wed; Pipeline script from SCM — its
`Jenkinsfile` is in the classroom repo at `Demos/API+Tests/library-api-demo/`) and Library SPA pipeline
(`Jenkinsfile.cd`: build → `azcopy` → `$web` → Cypress against the live site; its stages were
proven on the Library stack Wed) — ask to see either projected while you build. Their shape is what the guides show against placeholders; the names are
yours. (The Library pipelines are the trainer's answer key, not a template to copy.)

---

## What You're Building

```
 your laptop (Jenkins controller in Docker + Windows agent)        GitHub: your monorepo
 ┌───────────────────────────────────────────────────────────┐      ┌─────────────────────────┐
 │ job <team>-api   (Pipeline from SCM, <api-dir>/Jenkinsfile)│◄─────│ api/  spa/  tests/      │
 │   checkout → build → TEST (gate, junit) → image → push ACR │      │ + Jenkinsfile ×2        │
 │ job <team>-spa   (Pipeline from SCM, <spa-dir>/Jenkinsfile)│◄─────│ + Dockerfile            │
 │   checkout → build SPA (live API URL) → azcopy → $web      │      │ + .dockerignore (root)  │
 │   → wait for API → Cypress vs LIVE site → junit+screenshots│      └─────────────────────────┘
 └───────────────────────────────────────────────────────────┘
 Azure (yours): ACR ──webhook──► Web App for Containers (API)   |   Blob $web (SPA)   |   Azure SQL
```

Two jobs, two Jenkinsfiles, one repo — the back-end pair and the front-end pair can work without
stepping on each other, and it mirrors the reference: the trainer's Library API and Library SPA
each carry their own pipeline. (One combined Jenkinsfile with every stage is equally acceptable
if your team prefers it; the acceptance criteria are the same.)

---

## What you ship (one spec, one set — everything below is required)

### 1. Jenkins + agent (guide 01)

- Jenkins running as a container on a team laptop with its state in a **named volume**; the setup
  wizard done; a **Windows agent** on that laptop, label `windows`, **online**.
- A pipeline that runs on that agent and prints `dotnet`, `node`, `docker`, `azcopy`, and `git`
  versions, **green**. (Optional but recommended: keep the first pipeline that ran on the built-in node and
  went red with exit 127 — it is the best explanation of controller-vs-agent you will ever give.)

### 2. The back-end pipeline (guide 02)

- **`<api-dir>/Jenkinsfile` committed**; job `<team>-api` configured **Pipeline script from SCM**
  against your GitHub repo (branch `main`, script path pointing at that file).
- Stages, in order: **Build** → **Test** (your P3 unit + integration suites; **the gate** — a red
  test stops everything after it) → **Build image** (your `Dockerfile`; a **root
  `.dockerignore`** that keeps `bin/`, `obj/`, `.git/`, and every development settings file OUT
  of the image) → **Push** to **your ACR** (Basic) tagged with the Jenkins build number **and**
  `latest`, via a Jenkins credential.
- **Test results published**: the build page has a **Test Result** tab with your counts; raw
  results archived as artifacts.
- **Every configuration value your API needs at test time or run time comes from the
  environment** — Jenkins credentials surfaced as environment variables for the test stage; App
  Service settings for the container. Nothing secret in the repo, the Jenkinsfile, or the log
  (credentials are masked — prove it on the console).
- **A deliberate red build in the history**: one broken assertion pushed, the Test stage red, no
  image built, no push; then the fix, green.

### 3. The back-end deploy (guide 02)

- A **Web App for Containers (Linux)** in your resource group pulling `<team>-api:latest` from
  **your** registry, **continuous deployment ON** (the registry webhook restarts the app on a new
  push — there is no deploy stage in the Jenkinsfile; the registry IS the deploy).
- App settings carry the connection string (or a Key Vault reference, as Monday), the JWT key,
  the CORS origin of your static site, and the port your image listens on.
- Proof: **a visible API change, pushed, reaches the live container app with no portal click.**
  Your Monday Windows web app may be stopped or deleted once this answers.

### 4. The front-end pipeline (guide 03)

- **`<spa-dir>/Jenkinsfile` committed**; job `<team>-spa`, Pipeline script from SCM.
- Stages: **Install + Build** (the API base URL baked at build time from the pipeline, pointing at
  the container app) → **Publish** to your static site's `$web` container with `azcopy` under a
  **SAS** stored as a Jenkins credential → **Wait for the API** (poll a known GET until 200) →
  **E2E** — **Cypress against the LIVE site** (`baseUrl` from the command line, no hard-coded
  localhost left), results published to the Test Result tab, **screenshots archived on failure**.
- Proof: **a visible SPA change, pushed, is on the live site** — and the E2E stage is red with a
  screenshot artifact when you break one assertion, green when you fix it.

### 5. The honest account (`docs/ci/README.md` + the showcase)

- What each stage does, in your words; **which stage is the gate and which is the smoke** — and
  why a red E2E means the bad build is already live (one environment, treated as "dev"; say
  what production would add: a slot and a swap, a staging environment, approval gates).
- What the pipeline does **not** guarantee (e.g., Selenium not in the pipeline; no rollback;
  no push trigger — Build Now, not a GitHub webhook, unless you add poll-SCM).
- Screenshots: the green build page of each job with its Test Result tab; the registry tags; the
  container app's Deployment Center log showing an automatic pull.

### 6. Workflow evidence

- The pipeline work went through **pull requests** like the rest of the repo; the Jenkinsfiles
  were reviewed by a teammate; the board moved.

---

## Engineering Definition of Done

- **Pipeline-as-code**: both Jenkinsfiles live in the repo; the jobs read them from GitHub. A job
  whose script is pasted in the Jenkins editor does not count.
- **Runs on the agent**: every stage says `agent { label 'windows' }` (or the job-level
  equivalent) and uses `bat`/`powershell`, never `sh`.
- **Test gate is real**: `dotnet test` runs your actual P3 suites (not a placeholder project);
  the junit logger is wired so the Test Result tab shows real counts; a failing test fails the
  build before any image is built.
- **Images are hermetic**: `.dockerignore` at the **build-context root** (the folder you hand
  `docker build` — the repo root if your Dockerfile copies across folders, `<api-dir>` if
  Tuesday's build already worked from there); the image carries no development settings;
  configuration arrives through environment variables (`__` = `:`).
- **Secrets live in two places only**: Jenkins credentials (ACR admin password, SAS, JWT key,
  connection string for tests) and App Service settings. `git log -p` shows none of them.
- **Idempotent builds**: re-running a build does not double-publish results or fail on leftover
  state (clean result folders first; `docker start` is idempotent; `azcopy sync` deletes stale
  files).
- **The console is readable**: stage names say what they do; when a build is red, the first red
  line in Console Output is the cause, and your README says how you read it.

---

## Stretch (only after everything above is green)

- **Selenium against the live site** (base URL from an environment variable; headless Chrome on
  the agent) as a second E2E stage.
- **A boolean parameter** that skips the publish/deploy stages for a CI-only build
  (Continuous Delivery vs Deployment — the one difference is a gate).
- **Poll-SCM trigger** (`pollSCM('H/5 * * * *')`) so a push starts the build without Build Now
  (a GitHub webhook needs a public endpoint your laptop does not have).
- **The local e2e stack** pipeline: stand your stack up on the agent from your ACR image, run
  Cypress + Selenium against localhost, tear it down.
- **Static analysis** as a stage (SonarQube, notes `content/03-jenkins-cicd/` — not required).

---

## Submission & Presentation (the terminal showcase)

**In the repo at the showcase:** the P2 app runnable + the P3 suites runnable + both Jenkinsfiles +
`Dockerfile` + `.dockerignore` + `docs/ci/`.

**The `docs/ci/README.md` writeup — one checklist:**

- [ ] **Topology**: where Jenkins runs, where the agent runs, which laptop, how to bring both
      up (the two commands).
- [ ] **Each job**: name, Jenkinsfile path, stages in order, what each stage does, which one is
      the gate and which the smoke.
- [ ] **Configuration & secrets table**: every value the pipeline or the app needs, where it
      lives (Jenkins credential ID / App Service setting / build-time env), and what breaks
      without it.
- [ ] **Azure resources**: registry, container app, static site, SQL — names and URLs; what
      continuous deployment is wired to.
- [ ] **The gate proof**: the build number of the deliberate red, and what the console said.
- [ ] **The trade-off**: one paragraph on post-deploy smoke vs pre-deploy gate, what you would
      change with a second environment.
- [ ] **What it does not do** (honest residual list) and what you would add with a week.
- [ ] **Who built what** — per-member, consistent with PR history.

**Live (inside the team's ~25-min showcase slot; P4 is the closing ~7 min):**

1. **Open on the build page** — both jobs green, the Test Result tabs.
2. **One push, live** — a visible change committed and pushed on stage; Build Now; narrate the
   stages as they run; when the pipeline finishes, show the change on the live site. **Timing:**
   the SPA job lands in ~3 min; the API job plus the container re-pull is ~8–10 min — push it
   at the START of your team's 25-minute slot and come back to it, or use the SPA push (or show
   the registry tag + Deployment Center pull if the clock is short).
3. **The gate proof** — open the red build in the history; read the first red line aloud.
4. **The honest sentence** — what this pipeline guarantees and what it does not.

Pitch it as release engineering, not homework: *"here is the product, here is the proof, and
here is the machine that ships both — and here is exactly where we would still not trust it."*

> **Stuck?** The riskiest step is the one nobody has done: the first green **Pipeline script from
> SCM** build. Get there first — guide 01 then guide 02 through the Test stage — before anyone
> touches a registry or a storage account. Once the job reads your repo and publishes one test
> result, every remaining stage is a variation of "one more stage, one more credential."
>
> **If Thursday ends before D or F is green:** the minimum that is presentable Friday is both
> jobs reading your repo from SCM with the test gate proven on the Test Result tab (the red
> build + the green one) — tell the trainer at the ~15:00 checkpoint, finish the cloud parts
> Thursday evening / Friday morning, and say in the showcase exactly which stage is not yet
> wired and why. Honest partial beats silent partial; the spec's required set does not change.

> **Not examined.** There is no Project 4 exam and no DevOps rubric issued; the showcase is the
> record. Build the pipeline your team would actually keep.
