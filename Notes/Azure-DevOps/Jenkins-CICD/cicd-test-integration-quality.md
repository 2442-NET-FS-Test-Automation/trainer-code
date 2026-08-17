# CI/CD Test Integration, Pipeline Debugging, and Code Quality (Sonar)

## Learning Objectives
- Arrange the test pyramid (unit -> integration -> e2e/UI) as gated, fail-fast pipeline stages
  and explain why a broken build must never become an image or a deployment.
- Run each layer headless in CI for a .NET + JavaScript stack: xUnit through `dotnet test`
  with TRX and JUnit-format loggers, Cypress through `npx cypress run`, and Selenium .NET
  tests with a headless browser — with the application under test started by the pipeline.
- Solve the environment problem: services the tests need, configurable base URLs, test-data
  reset before e2e, and flakiness controls (deterministic waits, retries, quarantine).
- Publish results and artifacts (`junit`, `archiveArtifacts`, TRX, screenshots) so they appear
  even when a stage fails, and write the full declarative Jenkinsfile that does it idempotently.
- Debug a red pipeline from Console Output: tool not on PATH, wrong working directory,
  credentials ID typo, port in use, background processes killed by the ProcessTreeKiller,
  bat/sh quoting, exit codes.
- Explain static analysis in the pipeline with SonarQube, SonarCloud, and SonarLint: the
  scanner begin/build/end flow, what the dashboard shows, and the quality gate as a pipeline gate.

## Why This Matters
"We have tests" and "our tests run on every push and block a bad merge" are different
statements, and interviewers for test-automation and .NET roles probe exactly that gap: how a
UI test runs without a screen, how the API is up when Cypress starts, why the build is red on
the agent but green on your laptop, how a flaky test stops blocking the team. Code quality is
the natural follow-up — most enterprise .NET teams run Sonar and expect you to know what a
quality gate is. Describing a pipeline that builds, unit-tests, boots the app, runs e2e,
publishes reports, and fails on a Sonar gate is the difference between "wrote automated tests"
and "owns quality in the delivery process".

## The Concept

### The pyramid becomes gated stages
The test pyramid orders tests by cost: many fast **unit** tests, fewer **integration** tests
(real database, real HTTP through the in-process host), fewest **end-to-end/UI** tests (a real
browser against a running app). In a pipeline that ordering becomes **stage order**: cheap
first. Each stage is a **gate** — the pipeline stops at the first failing stage, so a compile
error fails in seconds rather than after ten minutes of browser tests, and nothing downstream
(image, push, deploy) runs on a broken build. The artifact you ship is one that passed every
gate. Trade-off: fail-fast means a flaky e2e test blocks a deploy — so flakiness is a defect.

Adjacent question — *CI vs CD?* Continuous Integration = every commit builds and tests on a
shared server; Continuous Delivery = every green build produces a deployable artifact;
Continuous Deployment = it also deploys automatically.

### Running each layer headless
**xUnit / integration tests.** `dotnet test` returns non-zero if any test fails, so it gates by
itself. For reports:
```
dotnet test --configuration Release --no-build ^
  --logger "trx;LogFileName=unit.trx" ^
  --logger "junit;LogFilePath=TestResults/unit-results.xml" ^
  --results-directory TestResults
```
TRX is Visual Studio's format — archive it, but Jenkins' JUnit plugin does not read it. Add
the **JunitXml.TestLogger** NuGet package to the test project and the `junit` logger writes
JUnit XML that the `junit` step renders as per-test pass/fail with trend graphs. Output shape:
```
Passed!  - Failed:     0, Passed:    42, Skipped:     0, Total:    42, Duration: 1 s
```
A failing run prints `Failed!` with the failing test names and exits 1.

**Cypress.** `npx cypress run` is headless by default (Electron; `--browser chrome` for
Chrome). Its exit code is the **number of failing tests** (0 = green; `--posix-exit-codes`
makes it a plain 1). Two things must be true first: the **app under test is running** —
Cypress does not start your API or dev server, the pipeline must — and `baseUrl` points at
it. Screenshots on failure and videos (if enabled) land in `cypress/screenshots` and
`cypress/videos` — archive them. Component tests (`--component`) mount components directly and
need no running app.

**Selenium (.NET).** Selenium tests are ordinary xUnit/NUnit tests, so `dotnet test` runs them;
the difference is a browser. Use headless Chrome and a matching driver (Selenium Manager
downloads it automatically in Selenium 4.6+):
```csharp
var options = new ChromeOptions();
options.AddArgument("--headless=new");
options.AddArgument("--window-size=1920,1080");
using var driver = new ChromeDriver(options);
```
Capture a screenshot on failure (`((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(...)`
in a fixture dispose) into a folder you archive — a failing UI test with no picture is nearly
undebuggable from a log.

### The environment problem
Tests need things: a **database** (a SQL Server container the pipeline starts, or one already
on the agent), the **API** process, the **front-end dev server**. Either the pipeline starts
them or the agent already has them — decide explicitly. On a single-machine agent `localhost`
works for everything. On distributed agents (API on one node, tests on another) hard-coded
`localhost` breaks, so **base URLs come from environment variables**: `CYPRESS_BASE_URL`
overrides Cypress' `baseUrl`; a Selenium base class reads
`Environment.GetEnvironmentVariable("APP_BASE_URL")`; the API reads its connection string from
configuration, never a literal.

**Test data**: e2e tests that create orders or move stock drift the database. Reset or seed
before the e2e stage — a reset endpoint guarded to non-production, a SQL script, or drop and
recreate a container — so a rerun starts from a known state.

**Flakiness**: a test that passes on retry is a defect in the test or the environment. Prefer
**deterministic waits** (Cypress retries assertions automatically; Selenium `WebDriverWait`
until a condition) over `Thread.Sleep`. Use **retries** sparingly and at name depth (Cypress
`retries: { runMode: 2 }`; xUnit via a retry attribute) — they hide problems. **Quarantine**
persistently flaky tests (a `[Trait("Category","Quarantine")]` filter excluded from the gate)
and fix them, rather than letting the whole gate be ignored.

### Reports, artifacts, timeouts, parallelism
- `junit '**/TestResults/*.xml'` publishes results; `archiveArtifacts artifacts: '**/*.trx,
  cypress/screenshots/**, cypress/videos/**'` keeps files on the controller, browseable from
  the build page. HTML reports (Cypress mochawesome, ReportGenerator coverage) can be archived
  or shown with the HTML Publisher plugin.
- Publish in **`post { always { } }`** — otherwise a failing Test stage skips the report and
  you have red with no detail.
- `options { timeout(time: 30, unit: 'MINUTES') }` (pipeline or stage) stops a hung browser
  holding an executor forever.
- **Parallel stages** (`parallel { stage('Cypress') {...} stage('Selenium') {...} }`) cut wall
  time when both share one running app — awareness: they need independent data or ports.
- **Container vs agent**: `agent { docker { image 'cypress/included:14.0.0' } }` gives a pinned
  toolchain with nothing installed on the agent; for Windows agents and .NET + browsers
  together, a prepared agent is simpler.

### A full example Jenkinsfile
Windows agent with the .NET SDK, Node, and Chrome installed; API on 5000, Vite dev server on
5173. Everything is idempotent — a re-run kills leftovers first.
```groovy
pipeline {
  agent { label 'windows' }
  options { timeout(time: 40, unit: 'MINUTES') }
  environment {
    APP_BASE_URL     = 'http://localhost:5173'
    CYPRESS_BASE_URL = 'http://localhost:5173'
  }
  stages {
    stage('Restore & Build') {
      steps {
        bat 'dotnet restore'
        bat 'dotnet build --configuration Release --no-restore'
        dir('client') { bat 'npm ci' }
      }
    }
    stage('Unit + Integration tests') {
      steps {
        bat 'dotnet test tests/Orders.UnitTests --no-build -c Release --logger trx --logger "junit;LogFilePath=TestResults/unit.xml"'
        bat 'dotnet test tests/Orders.IntegrationTests --no-build -c Release --logger trx --logger "junit;LogFilePath=TestResults/integration.xml"'
      }
    }
    stage('Start services') {
      steps {
        powershell '''
          Get-NetTCPConnection -LocalPort 5000,5173 -ErrorAction SilentlyContinue |
            ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
          $env:JENKINS_NODE_COOKIE = "dontKillMe"
          Start-Process dotnet -ArgumentList "run --project src/Orders.Api --no-build -c Release --urls http://localhost:5000" -RedirectStandardOutput api.log
          Start-Process npm -ArgumentList "run dev -- --port 5173" -WorkingDirectory client -RedirectStandardOutput web.log
          foreach ($port in 5000, 5173) {
            $deadline = (Get-Date).AddSeconds(90)
            while (-not (Test-NetConnection localhost -Port $port -InformationLevel Quiet)) {
              if ((Get-Date) -gt $deadline) { throw "port $port never opened" }
              Start-Sleep -Seconds 2
            }
          }
          Invoke-RestMethod http://localhost:5000/api/seed/reset -Method Post
        '''
      }
    }
    stage('Cypress e2e') {
      steps { dir('client') { bat 'npx cypress run --browser chrome' } }
    }
    stage('Selenium e2e') {
      steps {
        bat 'dotnet test tests/Orders.UiTests --no-build -c Release --logger "junit;LogFilePath=TestResults/ui.xml"'
      }
    }
  }
  post {
    always {
      junit testResults: '**/TestResults/*.xml', allowEmptyResults: true
      archiveArtifacts artifacts: '**/*.trx, client/cypress/screenshots/**, client/cypress/videos/**, **/TestResults/screenshots/**, *.log', allowEmptyArchive: true
      powershell '''
        Get-NetTCPConnection -LocalPort 5000,5173 -ErrorAction SilentlyContinue |
          ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
      '''
    }
  }
}
```
`Start-Process` returns immediately, so the **port-wait loop** is what makes the next stage
safe; `JENKINS_NODE_COOKIE = "dontKillMe"` stops Jenkins killing the background processes at
step end (see below); the cleanup in `post { always }` means the next run never hits "port
already in use". A Linux agent uses `sh` with `nohup ... &` and a `curl --retry` loop.

### Debugging pipelines
Read **Console Output top-down to the first failure** — the last lines are consequences.
Common causes and their message shapes:

| Message shape | Cause | Fix |
|---|---|---|
| `'npx' is not recognized as an internal or external command` / `sh: dotnet: not found` | Tool not on the agent's PATH (or not installed) | Install on the agent; or run under an agent label that has it; check with `bat 'where dotnet'` |
| `MSB1003: Specify a project or solution file` / `ENOENT package.json` | Wrong working directory | Wrap in `dir('client') { ... }`; print `bat 'cd'` |
| `ERROR: Could not find credentials entry with ID 'acr-cred'` | Credentials ID typo or wrong scope | Copy the ID from Manage Jenkins -> Credentials |
| `address already in use` / `EADDRINUSE :5173` | Previous run left the process | Kill by port before starting; clean up in `post` |
| API works in one stage, "connection refused" in the next | **ProcessTreeKiller** killed the background process at step end | Set `JENKINS_NODE_COOKIE=dontKillMe` (or `BUILD_ID=dontKillMe` for Freestyle) when launching, or start and test in the same step |
| Works locally, fails on the agent with a quoting error | `bat` vs `sh` differences (`%VAR%` vs `$VAR`, `^` vs `\` continuations, single vs double quotes) | Match the agent OS; keep one-line commands |
| `script returned exit code 3` after Cypress | Three tests failed (Cypress exit code = failures) | Read the Cypress summary above it |

Tools: `bat(script: 'dotnet test ...', returnStatus: true)` returns the exit code instead of
failing, when you want to decide later; `echo "${env.WORKSPACE}"` and `bat 'set'` show what the
step sees; **Replay** lets you edit the Jenkinsfile and rerun without pushing (diagnosis only —
the fix still goes into the repo); **Pipeline Syntax** (the snippet generator on every pipeline
job) writes correct step syntax; the stage view / Blue Ocean / Pipeline Graph View shows which
stage went red at a glance.

### Code quality: SonarQube, SonarCloud, SonarLint
Tests check behaviour; **static analysis** checks the code itself for bugs, vulnerabilities,
and maintainability without running it. The Sonar family:

- **SonarQube Server** — self-hosted; the free **Community Build**. Run it in Docker:
  `docker run -d --name sonarqube -p 9000:9000 sonarqube:community` (add volumes for
  `/opt/sonarqube/data` and `extensions` to keep state), browse `http://localhost:9000`, log in
  `admin`/`admin` and **change the password** (forced on first login). Create a **project** (its
  **key** identifies it) and generate a **token** for the scanner.
- **SonarCloud** (SonarQube Cloud) — the hosted SaaS, same engine and rules, free for public
  repos, billed by lines of code for private ones; nothing to run.
- **SonarLint** (SonarQube for IDE) — the extension for Visual Studio, VS Code, and Rider that
  shows the same rules while you type; **connected mode** syncs the server's quality profile so
  IDE and pipeline agree. This is **shift-left**: catch it before it is pushed.

The **.NET scanner flow** wraps the build (it must see MSBuild run):
```
dotnet tool install --global dotnet-sonarscanner
dotnet sonarscanner begin /k:"orders-api" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="%SONAR_TOKEN%"
dotnet build --configuration Release
dotnet sonarscanner end /d:sonar.token="%SONAR_TOKEN%"
```
`begin` hooks analyzers into the build, `build` produces the analysis, `end` uploads it and
prints `EXECUTION SUCCESS` with a dashboard link. Add
`/d:sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml` (from `dotnet test
--collect:"XPlat Code Coverage"` in OpenCover format) to see coverage.

**The dashboard** shows: **Bugs** (code that will misbehave — a null dereference),
**Vulnerabilities** (security-relevant — SQL injection), **Security Hotspots** (needs a human
look — hard-coded credential), **Code Smells** (maintainability — a 300-line method),
**Coverage** (from your test run), **Duplications** (copy-pasted blocks), plus ratings A-E,
with a **New Code** view separated from overall so old debt does not block today's change.

**Quality gate**: a set of conditions — for example "coverage on new code >= 80%, no new bugs,
duplications on new code <= 3%" — evaluated after each analysis to a **Passed/Failed** status.
Two ways to use it: **read it in the UI** (informational), or make it a **pipeline gate** with
the **SonarQube Scanner** Jenkins plugin: wrap the scan in `withSonarQubeEnv('local')` and add
a stage with `waitForQualityGate abortPipeline: true`, which waits for the server's webhook and
fails the build on a red gate (awareness: the server needs a webhook pointing at Jenkins).
Trade-offs: a self-hosted server is one more thing to run and wants a real database and 4 GB+
RAM; SonarCloud is zero-ops but hosted; gates too strict on legacy code get bypassed, so gate
**new code**.

## Say It in an Interview
- **"How do tests fit into a CI pipeline?"** — "As ordered, gated stages: unit and integration
  first because they are cheap and need no browser, then end-to-end against a running app. A
  failing stage stops the pipeline, so a build that fails tests never becomes an image or a
  deployment."
- **"How do you run Cypress or Selenium in CI with no screen?"** — "Both run headless: `npx
  cypress run` is headless by default and its exit code is the failure count; Selenium gets
  Chrome with `--headless=new`. The pipeline starts the API and dev server first and waits for
  their ports, and archives screenshots and videos so a failure is debuggable."
- **"What about the environment the tests need?"** — "Either the pipeline starts it — DB
  container, API, dev server — or the agent already has it, and base URLs come from environment
  variables. I reset test data before e2e so reruns are deterministic, and I treat flaky tests
  as defects: real waits, minimal retries, quarantine and fix."
- **"How do you get reports out even when the build fails?"** — "Publish in `post { always }`
  — `junit` for JUnit XML, `archiveArtifacts` for TRX, screenshots and logs — and I clean up
  background processes there too so the next run is idempotent."
- **"A pipeline is red — how do you debug it?"** — "Console Output top-down to the first error.
  Usual suspects: tool missing on the agent, wrong working directory, credentials ID typo,
  port already in use, or a background process the ProcessTreeKiller stopped between steps.
  Replay and the snippet generator speed up the fix."
- **"What is SonarQube and a quality gate?"** — "Static analysis reporting bugs,
  vulnerabilities, hotspots, smells, coverage, and duplication. For .NET it wraps the build:
  `sonarscanner begin`, `dotnet build`, `sonarscanner end`. A quality gate is pass/fail
  conditions on that analysis, ideally on new code; with the Jenkins plugin
  `waitForQualityGate` turns a red gate into a failed build. SonarCloud is the hosted version,
  SonarLint the same rules in the IDE."

## Check Yourself
1. Why do unit tests run before end-to-end tests in the pipeline, and what does "gated" mean?
2. Cypress passes locally but the pipeline logs "connection refused" on the base URL. Name the
   two most likely causes.
3. Which two flags make `dotnet test` produce both a TRX file and something the Jenkins JUnit
   plugin can render?
4. Your API starts fine in a "Start services" step but is gone when the next stage runs. What
   killed it and how do you keep it alive?
5. What are the three commands of a SonarScanner for .NET analysis, and where does the build go?

**Answers:** (1) Cheap-first fail-fast: a failing stage stops the pipeline, so nothing
downstream (image, deploy) runs on a broken build. (2) The pipeline never started the app (or
skipped the port-wait), or the base URL is hard-coded to a host the agent cannot reach — set
`CYPRESS_BASE_URL`. (3) `--logger trx` and `--logger "junit;LogFilePath=..."`
(JunitXml.TestLogger). (4) The ProcessTreeKiller ends processes a step spawned when the step
finishes; set `JENKINS_NODE_COOKIE=dontKillMe` on launch, or start and test in one step. (5)
`dotnet sonarscanner begin`, `dotnet build`, `dotnet sonarscanner end` — the build sits between.

## Resources
- [dotnet test command (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test)
- [Cypress command line: `cypress run` and exit codes](https://docs.cypress.io/app/references/command-line)
- [Continuous Integration with Cypress](https://docs.cypress.io/app/continuous-integration/overview)
- [JUnit plugin (plugins.jenkins.io)](https://plugins.jenkins.io/junit/)
- [SonarScanner for .NET (docs.sonarsource.com)](https://docs.sonarsource.com/sonarqube-server/analyzing-source-code/scanners/dotnet/introduction/)
- [SonarQube Scanner Jenkins plugin (waitForQualityGate)](https://plugins.jenkins.io/sonar/)
