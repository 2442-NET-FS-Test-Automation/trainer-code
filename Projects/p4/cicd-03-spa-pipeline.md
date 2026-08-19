# Guide 03 — The front-end pipeline: SPA build → your static site → Cypress against the LIVE site (front-end pair, ~3 h)

> **Read first:** `cicd-pipeline-README.md` (names table). **Needs:** guide 01 done on the
> hosting laptop. **Can start before the back-end pair finishes:** steps E1–E2 and F1 run on
> your own machine against Monday's live API; the pipeline stages need the hosting laptop's
> Jenkins and, for the final form, the back-end pair's container-app URL. Parts: **E** build +
> publish · **F** Cypress against the live site. Raise a hand after 15 minutes stuck; first red
> line of Console Output first.

## The shape you are building

```
job <team>-spa  (Pipeline script from SCM -> <spa-dir>/Jenkinsfile)
  Install + Build SPA (VITE_<API_URL_VAR>=https://<api-app>.azurewebsites.net baked at build time)
  -> Publish SPA (azcopy sync dist -> https://<storage>.blob.core.windows.net/$web  under a SAS credential)
  -> Wait for API (poll <known-get> until 200)
  -> E2E live (npx cypress run --config baseUrl=<static-site-url> ... junit reporter)
  post { always } -> junit cypress/results + archive cypress/screenshots
```

Trainer reference for the same shape on the Library SPA: the trainer's `Jenkinsfile.cd`
(Parts E + F; its stages were proven on the Library stack Wed — ask to see it projected) and
the note
(classroom repo `Notes/Azure-DevOps/Jenkins-CICD/`) `cicd-test-integration-quality.md` (gates
vs smoke, artifacts, debugging). Same shape, every name yours.

---

## Part E — Build → `azcopy` → your `$web` (~60–75 min)

### E1. The build takes the API URL from the environment

Your SPA already reads its API base URL from a Vite variable (Monday's deploy needed it —
`import.meta.env.VITE_<API_URL_VAR>`, with a localhost fallback). Vite **bakes** that value in at
build time, so the pipeline sets it right before `npm run build`. Check locally on your machine:

```powershell
cd <spa-dir>
npm ci
$env:VITE_<API_URL_VAR> = 'https://<api-app>.azurewebsites.net'; npm run build
```

then `findstr /s /c:"azurewebsites.net" dist\assets\*.js` — the URL is inside the bundle. (Until
the back-end pair's container app answers, use Monday's Windows web app URL here; swap once.)

### E2. A SAS (Shared Access Signature) for the static site + the Jenkins credential

A SAS is a signed, time-boxed token that rides in the URL and grants exactly the permissions
it names on your storage account — the way a pipeline writes to `$web` without your login.
(On your own machine first: `winget install Microsoft.Azure.AZCopy.10`, then a NEW terminal
for `azcopy --version`; the hosting laptop already has it from guide 01.)

> Proven on the trainer's stack 2026-08-19 PM: an account SAS (services Blob, resource types
> Container + Object, permissions Read/Write/Delete/List, HTTPS only, 4-day expiry) +
> `azcopy sync dist … --recursive --delete-destination=true` → 3 files up, 2 stale bundles
> deleted, the live site serving the new build against the container app a second later.

Portal → your static-site storage account → **Security + networking → Shared access signature**:
Allowed services **Blob**; Allowed resource types **Container + Object**; Allowed permissions
**Read, Write, Delete, List**; Start/expiry: now → **Saturday**; Allowed protocols HTTPS only →
**Generate SAS and connection string** → copy the **SAS token** (starts with `?sv=`). It is a
password for your storage account until Saturday — it goes ONE place:

Jenkins → **Manage Jenkins → Credentials → (global) → Add Credentials** → **Secret text** →
Secret = the token (WITH the leading `?`), **ID `web-sas`** → Create.

Local proof before any pipeline (the same command the stage will run; `$web` is literal in a
PowerShell **single-quoted** string — in a double-quoted one `$web` would be a variable):

```powershell
$dest = 'https://<storage>.blob.core.windows.net/$web' + '<paste the SAS token>'
azcopy sync dist $dest --recursive --delete-destination=true
```

→ `Number of Copy Transfers Completed: <n>` / `Number of Deletions at Destination: <m>` /
`Final Job Status: Completed` → hard-refresh your static-site URL (read it from
the storage account's **Data management → Static website** blade — the `z##` in the host is
per account). Revoke-and-retry if you mistyped: a SAS cannot be "edited", you generate another.

### E3. The Jenkinsfile

**`<spa-dir>/Jenkinsfile`**:

```groovy
pipeline {
    agent { label 'windows' }

    environment {
        SPA_DIR  = '<spa-dir>'                                       // folder holding package.json (repo-relative)
        API_URL  = 'https://<api-app>.azurewebsites.net'             // the container app from guide 02
        SITE_URL = 'https://<storage>.z##.web.core.windows.net'      // Static website blade, no trailing slash
    }

    stages {
        stage('Build SPA') {
            steps {
                dir(env.SPA_DIR) {
                    bat 'npm ci'
                    // Vite bakes VITE_* at build time: set it ON THE SAME LINE as the build.
                    bat 'set VITE_<API_URL_VAR>=%API_URL%&& npm run build'
                }
            }
        }

        stage('Publish SPA') {
            steps {
                // withCredentials: Jenkins reads the credential by ID, hands it to the steps inside as
                // an environment variable (SAS) and MASKS it in the log - the pipeline text never holds it.
                withCredentials([string(credentialsId: 'web-sas', variable: 'SAS')]) {
                    dir(env.SPA_DIR) {
                        // PowerShell, single-quoted PS string: no batch %-expansion games with the SAS
                        // (a SAS contains %3A and &), and $web stays literal.
                        powershell '''
                            $dest = 'https://<storage>.blob.core.windows.net/$web' + $env:SAS
                            azcopy sync dist $dest --recursive --delete-destination=true
                        '''
                    }
                }
            }
        }
    }
}
```

(`API_URL` = Monday's Windows web app until the back-end pair's container app answers; swap
the one line and rebuild — the site follows.) `set VITE_X=%API_URL%&& npm run build` — **no
space before `&&`**, or the variable gets a
trailing space and the SPA calls `https://…azurewebsites.net ` (with a space) — broken in a way
that looks like CORS. `--delete-destination=true` removes files from `$web` that are no longer
in `dist/` (old hashed bundles) — the site always equals the last build.

Commit + push → **New Item** → `<team>-spa` → **Pipeline** → Definition **Pipeline script from
SCM** → Git → `<repo-url>` (+ the PAT credential if private) → Branch Specifier `*/main` →
Script Path `<spa-dir>/Jenkinsfile` → Save → **Build Now**.

**Part E DoD:** the static-site URL serves the build the pipeline made — prove it with a
visible change (a heading, a footer string): commit, push, Build Now, hard refresh, there it is.
The SPA talks to the live API (Network tab: requests go to your `azurewebsites.net` host —
Monday's app now, the container app once guide 02 Part D is done — 200s, no CORS errors).

**Reds you will hit:**

| Symptom | Cause → fix |
|---|---|
| `403 … AuthorizationPermissionMismatch` / `AuthenticationFailed` | SAS lacks Write/Delete/List or Container resource type, expired, or pasted without the `?` |
| `azcopy` is not recognized in the pipeline | Agent launched before the install → relaunch the agent from a NEW terminal (guide 01 step 5) |
| Site still shows the old build | Browser cache → hard refresh; or `dist/` was built but `sync` pointed at the wrong container (`$web` exactly) |
| The SPA calls `localhost:<port>` in prod | The Vite variable was not set at BUILD time (name typo, wrong stage, or the space-before-`&&`) |
| CORS error in the browser against the container app | The API's CORS-origin setting lacks your static-site origin → back-end pair, guide 02 D3 |
| `$web` became `web` / empty in the URL | A double-quoted PowerShell string expanded `$web` → single-quoted PS string as written above |

---

## Part F — Cypress against the LIVE site: the post-deploy smoke (~60–75 min)

> Proven on the trainer's stack 2026-08-19 PM: the Library SPA's Cypress suite against the
> live static site + the container app — **14/14 in 17 s** across six specs (smoke,
> navigation, catalog, login through the real form against the live API, intercept, POM);
> the two excluded specs = the admin form (needs the local seed-reset endpoint) and the
> visual baseline (viewport-bound). The one live-only failure mode found: **deep links**
> (`/about`, `/login`) come back from the Blob static website with `index.html` as the body
> but a **404 status** (it has no rewrite rule — the "error document" trick), and
> `cy.visit()` fails on any non-2xx → the `live` switch below. Reference: the trainer's
> `react-spa-demo` rung `20-cd-blob-e2e` (`cypress/support/commands.js` + `e2e.js`,
> `Jenkinsfile.cd`).

### F1. Make the suite point-able (your machine, before any pipeline)

Your P3 Cypress suite was written against `localhost`. Two changes make it run anywhere:

- **`baseUrl` comes from the command line** — `cy.visit('/')`, `cy.visit('/login')` stay as they
  are; `npx cypress run --config baseUrl=https://<static-site-url>` overrides
  `cypress.config.js` for the run.
- **Any hard-coded API URL** (in `cypress/support/*.js` commands, `cy.request`, `cy.intercept`
  patterns) becomes `Cypress.env('apiUrl') ?? 'http://localhost:<port>'`, passed with `--env
  apiUrl=https://<api-app>.azurewebsites.net`. Intercept patterns that matched
  `**/api/**` keep working; ones that matched `localhost:<port>` do not.
- **Deep links on a Blob static website are 404s with the right body.** Every
  `cy.visit('/some-route')` fails with `cy.visit() failed trying to load … 404` until you tell
  Cypress to tolerate it — once, in `cypress/support/e2e.js`, only in live mode:

  ```js
  // Live mode (--env live=true): the static site answers deep links with index.html
  // as the body but a 404 status; cy.visit() fails on non-2xx by default.
  if (Cypress.env("live")) {
    Cypress.Commands.overwrite("visit", (originalVisit, url, options = {}) =>
      originalVisit(url, { failOnStatusCode: false, ...options })
    );
  }
  ```

  Locally nothing changes (`live` is unset).
- **Specs that depend on local-only state** — a seed-reset endpoint your live API does not
  expose, a visual-regression baseline taken on a local viewport, component tests (`cy:ct`),
  anything that writes data you cannot reset in the cloud — **exclude or guard them** for the
  live run: `--spec "cypress/e2e/a.cy.js,cypress/e2e/b.cy.js"` (the live-safe list) or, inside
  a local-only `describe`, `before(function () { if (Cypress.env('live')) this.skip(); })`
  (a `function`, not an arrow — `this.skip()` exists only inside hooks/tests). The live run is a
  **smoke**, not the full regression: a login, the main read flow, one protected-route check.
  Say which specs are in it and why in the README.

Run it locally against the live site first:

```powershell
npx cypress run --config baseUrl=https://<static-site-url> --env apiUrl=https://<api-app>.azurewebsites.net,live=true --spec "cypress/e2e/<a>.cy.js,cypress/e2e/<b>.cy.js"
```

Green here = the pipeline stage will be green. Red here = fix here, not in Jenkins.

### F2. Two stages after `Publish SPA`, plus `post`

```groovy
        stage('Wait for API') {
            steps {
                // The container app may be cold after a new pull: poll a known GET until it answers.
                powershell '''
                    foreach ($i in 1..30) {
                        try {
                            Invoke-WebRequest -UseBasicParsing "$env:API_URL<known-get>" | Out-Null
                            "api up after $i tries"; exit 0
                        } catch { Start-Sleep 10 }
                    }
                    throw "api never answered"
                '''
            }
        }

        stage('E2E live') {
            steps {
                dir(env.SPA_DIR) {
                    // Workspace persists: clear last run's results and screenshots first.
                    powershell 'Remove-Item -Recurse -Force cypress/results, cypress/screenshots -ErrorAction SilentlyContinue'
                    bat 'npx cypress run --config baseUrl=%SITE_URL% --env apiUrl=%API_URL%,live=true --spec "cypress/e2e/<a>.cy.js,cypress/e2e/<b>.cy.js" --reporter junit --reporter-options "mochaFile=cypress/results/[hash].xml,toConsole=false"'
                }
            }
        }
    }

    post {
        always {
            dir(env.SPA_DIR) {
                junit allowEmptyResults: true, testResults: 'cypress/results/*.xml'
                archiveArtifacts allowEmptyArchive: true, artifacts: 'cypress/screenshots/**'
            }
        }
    }
```

(`[hash].xml` — the junit reporter writes one file per spec; the `post` glob collects them.
The `junit` reporter is built into Cypress' bundled Mocha — no install.)

Commit, push, Build Now → the stage view: Build → Publish → Wait → E2E, all green → **Test
Result** tab lists your specs beside nothing else (this job is the SPA's); **Artifacts** empty
on green.

### F3. Prove the smoke

Break one assertion in a smoke spec, push, Build Now → **E2E live** red; **Artifacts** → the
failure screenshot (`cypress/screenshots/<spec>/<test> (failed).png`); Test Result → the failing
test's name and message. Fix, push, green. Keep the red build for the README.

**Part F DoD:** `<team>-spa` green end-to-end with the Test Result tab showing the smoke specs;
one red build in the history with a screenshot artifact.

**Say it in the showcase (the trade-off):** the API job's Test stage is the **GATE** — nothing
ships red. This E2E stage is a **SMOKE** on the real environment — it runs AFTER deploy, so a
red E2E means the bad build is already live. We have one environment and we treat it as "dev";
production would be a deployment slot + swap (smoke the slot, swap on green) or a staging
environment in front of prod. What you gain by smoking the real thing: you test the real HTTPS,
the real CORS, the real config, the real cold start — things no local run ever sees.

**Reds you will hit:**

| Symptom | Cause → fix |
|---|---|
| Cypress hits `localhost` | An API URL override missed one spot (a support command, an intercept) |
| `cy.visit() failed trying to load … 404` on a deep link | The static site's 404-status deep links → the `live` visit overwrite above (passed `--env live=true`?) |
| `cy.visit()` fails on `/` itself | `baseUrl` has a trailing slash + a leading slash in `visit` → double slash; or the site has no `index.html` at the root |
| CORS in the Cypress browser | The API's CORS-origin setting lacks the static-site origin (back-end pair) |
| First spec times out, the rest pass | Cold start — the wait stage polls the API but the SPA's first call still waited: widen `defaultCommandTimeout` for the smoke config or visit once in a `before` |
| Visual-regression spec fails on a different viewport / anti-aliasing | Exclude it from the smoke set |
| Seed/reset spec 404s | The live API does not expose the reset endpoint — guard with `Cypress.env('live')` or exclude |
| Test Result tab empty after a green run | Reporter options typo (`mochaFile=cypress/results/[hash].xml`) or the `post` glob not under `dir(env.SPA_DIR)` |

---

## Stretch (only after F is green — spec §Stretch)

- **Selenium against the live site:** your P3 Selenium project reads its base URL from an
  environment variable (a one-line change in the test base class); a stage `bat 'set
  E2E_BASE_URL=%SITE_URL%&& dotnet test <selenium-csproj> --logger
  "junit;LogFilePath=TestResults/selenium.junit.xml"'` + the junit glob; headless Chrome on
  the agent (the `--headless=new` option you used in Week 9).
- **A `booleanParam` `PUBLISH`** (default `true`) gating Publish/Wait/E2E — a CI-only build of
  the SPA; Jenkins learns the parameter on the first build, so the first run shows no checkbox.
- **The local e2e-stack pipeline** (stand the stack up on the agent from your ACR image, run
  Cypress + Selenium against localhost, tear down) — the trainer's reference is the
  `react-spa-demo` e2e pipeline named in the notes; it is the full-regression complement to
  this smoke.

**Then:** `docs/ci/README.md` sections for the SPA job (spec §5) — which specs are the smoke
and why, the trade-off paragraph — + screenshots (green build + Test Result tab + the red
build's screenshot artifact), and rehearse "one push, live" once with the back-end pair before
Friday.
