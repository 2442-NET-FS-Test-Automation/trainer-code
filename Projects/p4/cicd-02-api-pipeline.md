# Guide 02 — The back-end pipeline: test gate → image → your registry → your container app (back-end pair, ~3–4 h)

> **Read first:** `cicd-pipeline-README.md` (names table — fill it in before you type a
> stage). **Needs:** guide 01 done on the hosting laptop (agent online, `hello-pipeline` #2
> green). Parts: **B** Jenkinsfile + test gate from SCM · **C** Dockerfile + ACR push · **D**
> Web App for Containers with continuous deployment. Each part ends with something visible on
> the build page or in the portal — **do not start the next part until the current one is
> green.** Raise a hand after 15 minutes stuck; read the first red line of Console Output first.

## The shape you are building

```
job <team>-api  (Pipeline script from SCM -> <api-dir>/Jenkinsfile)
  Build -> Test (GATE: your P3 unit+integration suites; junit -> Test Result tab)
        -> Build image (<api-dir>/Dockerfile, context = repo root, root .dockerignore)
        -> Push to ACR (<acr>.azurecr.io/<team>-api:<BUILD_NUMBER> + :latest, credential acr-admin)
  post { always } -> junit + archive trx
Azure: ACR --webhook--> Web App for Containers <api-app> (continuous deployment ON) -> live API
```

Trainer reference for the same shape on the Library API: the `library-api-build` job on the
trainer's Jenkins — **Pipeline script from SCM**, reading
`Demos/API+Tests/library-api-demo/Jenkinsfile` from the classroom repo (Checkout SCM → Test →
Build image → Push to ACR — ask to see it projected; the file is in the repo you clone) and
the notes (classroom repo `Notes/Azure-DevOps/`) `Jenkins-CICD/jenkins.md` (credentials,
from-SCM) and `Docker/` (Dockerfile, `.dockerignore`, ACR). Same shape, every name yours.
"From SCM" = the job reads the Jenkinsfile from source control (your GitHub repo), not from a
text box in Jenkins.

---

## Part B — Pipeline-as-code: the test gate, read from your repo (~60–75 min)

### B1. The Jenkinsfile

Create **`<api-dir>/Jenkinsfile`** (or at the repo root if you prefer one combined file; then
Script Path is `Jenkinsfile`):

```groovy
pipeline {
    agent { label 'windows' }

    environment {
        API_DIR = '<api-dir>'        // repo-relative folder that CONTAINS the solution/csproj AND the test projects (see below)
    }

    stages {
        stage('Build') {
            steps {
                dir(env.API_DIR) {
                    bat 'dotnet build <api-sln-or-csproj> -c Release'
                }
            }
        }

        stage('Test') {
            // The gate: no green tests, no image.
            steps {
                dir(env.API_DIR) {
                    // The agent workspace persists between builds: clear last build's results first,
                    // or the junit/archive steps publish them again and the counts inflate.
                    powershell 'Remove-Item -Recurse -Force <tests-dir-relative>/*/TestResults -ErrorAction SilentlyContinue'
                    bat 'dotnet test <api-sln-or-csproj> -c Release --no-build --logger "junit;LogFilePath=TestResults/{assembly}.junit.xml" --logger trx'
                }
            }
        }
    }

    post {
        always {
            // Publish results green or red - Jenkins renders the trend and the failing test names.
            // Globs are relative to API_DIR (Ant-style patterns do not match a './' prefix).
            dir(env.API_DIR) {
                junit allowEmptyResults: true, testResults: '<tests-dir-relative>/**/TestResults/*.junit.xml'
                archiveArtifacts allowEmptyArchive: true, artifacts: '<tests-dir-relative>/**/TestResults/*.trx'
            }
        }
    }
}
```

Read it top-down once: `environment` = values every stage sees; `dir()` = cd for the steps
inside it (`-c Release --no-build` on the test: it runs what the Build stage built, so Build
is the compile gate and Test the test gate); `post { always }` = runs green OR red — where
results get published; `junit` =
the JUnit XML result format every CI server reads — Jenkins renders it as the **Test Result**
tab (counts, trend, failing test names).

**`API_DIR` must contain both the solution and the test projects.** The result globs in
`post` are relative to `API_DIR` and cannot climb to `..` — so if your `tests/` folder sits at
the repo root beside `api/`, set `API_DIR = '.'` and make the solution path repo-relative
(`dotnet test api/<sln>`, `tests/**/TestResults/*.junit.xml`).

Three things the shape depends on:

- **`--logger junit` needs the NuGet package `JunitXml.TestLogger` in EACH test project**
  (`dotnet add <test.csproj> package JunitXml.TestLogger`). Without it the logger is silently
  ignored, no `.junit.xml` appears, and the Test Result tab never shows up. The `trx` logger is
  built in — it is the raw artifact you archive.
- **If an integration test needs your SQL container:** `bat 'docker start <your-sql-container>'`
  as the first Test step (idempotent — green if it already runs).
- **Secrets your tests need — the trainer's first Jenkins build of the Library API went 8 × 500
  on exactly this:** the workspace is a **CLEAN clone**. `appsettings.Development.json` is gitignored, so a
  `WebApplicationFactory` test that boots your real `Program.cs` sees `Jwt:Key` (or a
  connection string) as NULL → every request 500. Put those values in **Manage Jenkins →
  Credentials → (global) → Add Credentials → Secret text** (e.g. ID `jwt-key`) and surface them
  as environment variables in the `environment` block:

  ```groovy
  environment {
      API_DIR = '<api-dir>'
      Jwt__Key = credentials('jwt-key')                       // YOUR key's config path with '__' for ':' (Jwt:Key here; JwtSettings:Secret -> JwtSettings__Secret); masked in the log
      ConnectionStrings__<Name> = credentials('test-conn')    // only if your tests hit a real DB
  }
  ```

  Same rule as Monday's App Service settings and Tuesday's `docker run -e`: **the code ships,
  the secrets do not — the environment supplies them; in Jenkins the environment is a
  credential.**

### B2. The job, from SCM

Commit + **push** (Jenkins reads GitHub, not your disk). Jenkins → **New Item** → `<team>-api`
→ **Pipeline** → **OK** → scroll to **Pipeline** → Definition **Pipeline script from SCM** →
SCM **Git** → Repository URL `<repo-url>` → (private repo: **Credentials → Add → Username with
password**, username = your GitHub user, password = a **personal access token** — GitHub →
Settings → Developer settings → Personal access tokens → Tokens (classic) → Generate, scope
`repo`; paste it ONCE here, it is a password) → **Branch Specifier `*/main`** (the default `*/master` is the classic first red) →
**Script Path** `<api-dir>/Jenkinsfile` → **Save** → **Build Now**.

First build: **Declarative: Checkout SCM** clones your repo onto the agent
(`C:\jenkins-agent\workspace\<team>-api`), then your stages run. Console Output top-down.

### B3. Prove the gate

- Green build → **Test Result** tab shows your counts; **Artifacts** → the `.trx` files.
- **Break one assertion**, commit, push, Build Now → **Test** red, the build stops there, `post`
  still publishes the failing test's name under Test Result. **Revert**, push, green. Keep that
  red build — the spec wants it in the history and the README.

**Part B DoD:** `<team>-api` reads your repo, the Test Result tab shows your real P3 counts, one
deliberate red build in the history.

**Reds you will hit:**

| Symptom | Cause → fix |
|---|---|
| `Couldn't find any revision to build` | Branch Specifier `*/master` → `*/main` |
| `Authentication failed` on checkout | Private repo without a credential → PAT as Username with password on the SCM |
| `Jenkinsfile not found` | Script Path wrong (it is repo-relative, forward slashes) |
| Test Result tab never appears | `JunitXml.TestLogger` missing from a test project, or the glob does not match (globs are relative to `API_DIR`; no `./`) |
| `dotnet test` runs 0 tests | Wrong solution/csproj path inside `dir(env.API_DIR)` |
| Integration tests 500 in Jenkins, green locally | A gitignored settings file your API reads at startup (JWT key, connection string) → Secret text credential + `environment` line |
| SQL connection refused in tests | The SQL container was not started on the agent machine → `docker start` step |
| Results count doubles on rebuild | Missing the `Remove-Item … TestResults` clean |

---

## Part C — The API as an image: Dockerfile → your ACR (~45–60 min)

### C1. Dockerfile + the root `.dockerignore`

**`<api-dir>/Dockerfile`** in Tuesday's shape: SDK build stage (`restore` → `publish -c
Release -o /app`) → `aspnet` runtime stage (`WORKDIR /app`, `COPY --from=build /app .`,
`EXPOSE 8080`, `ENTRYPOINT ["dotnet", "<Api>.dll"]`). The **build context** is the folder you
hand `docker build` as its last argument: use **the repo root** if your projects reference
each other across folders (every `COPY` path is then repo-root-relative), or keep `<api-dir>`
if Tuesday's build already worked from there (then `docker build … -f Dockerfile <api-dir>`
and the ignore file lives in `<api-dir>`).

**`.dockerignore` at the CONTEXT ROOT** — the repo root in the shape below:

```
**/bin/
**/obj/
.git/
**/appsettings.Development.json
**/node_modules/
```

**TRAP (Tuesday's finding on the trainer's own tree):** a `.dockerignore` next to the Dockerfile
is IGNORED when the context is the repo root — the file is read ONLY at the build-context root.
The image then carries your dev settings (JWT key!) and your local `bin/`/`obj/`. (`docker
history` / `docker run --rm <img> ls` shows what went in.)

### C2. Local proof first

```powershell
docker build -t <team>-api:test -f <api-dir>/Dockerfile .
docker run --rm -p 5199:8080 -e "ConnectionStrings__<Name>=<your Azure SQL string>" -e "Jwt__Key=<your key>" <team>-api:test
```

`http://localhost:5199<known-get>` → 200. (Cloud SQL string = Monday's; the venue wifi blocks
outbound 1433 → hotspot, as Monday. `docker history <team>-api:test` or `docker run --rm
<team>-api:test ls` if you want to SEE that no `appsettings.Development.json` is inside.)

### C3. Your registry + the credential

Portal → **Create a resource → Container Registry** → your RG, registry name `<acr>` (globally
unique, lowercase, letters+digits), **Pricing plan: Basic** (the wizard defaults higher) →
Create → open it → **Settings → Access keys → Admin user: Enabled** → username + password on
screen (read once). Login server = `<acr>.azurecr.io`.

Jenkins → **Manage Jenkins → Credentials → (global) → Add Credentials** → **Username with
password** → Next → Username `<acr>`, Password pasted, **ID `acr-admin`** → Create. *A
credential has an ID; pipelines name the ID, never the value — the log masks it.*

### C4. Two stages after `Test`

Add to `environment`:

```groovy
REGISTRY = '<acr>.azurecr.io'
IMAGE    = "${REGISTRY}/<team>-api"
```

and after the `Test` stage:

```groovy
stage('Build image') {
    steps {
        bat 'docker build -t %IMAGE%:%BUILD_NUMBER% -t %IMAGE%:latest -f "%API_DIR%/Dockerfile" .'
    }
}

stage('Push to ACR') {
    steps {
        // The credential never appears in the log: Jenkins injects it as env vars and masks them.
        withCredentials([usernamePassword(credentialsId: 'acr-admin', usernameVariable: 'ACR_USER', passwordVariable: 'ACR_PASS')]) {
            bat 'echo %ACR_PASS%| docker login %REGISTRY% -u %ACR_USER% --password-stdin'
            bat 'docker push %IMAGE%:%BUILD_NUMBER%'
            bat 'docker push %IMAGE%:latest'
            bat 'docker logout %REGISTRY%'
        }
    }
}
```

`%X%` = batch reading an environment variable; `BUILD_NUMBER` = Jenkins' own counter — it
becomes the image tag (two tags go up: the number and `latest`). **`echo %ACR_PASS%|` — NO space
before the pipe**, or the password gets a trailing space and login fails. Note the `Build image`
stage is NOT inside `dir(env.API_DIR)`: the context is the repo root, which is the workspace.

Commit, push, Build Now. Point at the `****` in the console where the password would be.

**Part C DoD:** portal → your registry → **Repositories → `<team>-api`** → tags `<n>` + `latest`.
**And the gate still holds:** a red test → `Build image` and `Push to ACR` show as skipped.

**Reds you will hit:**

| Symptom | Cause → fix |
|---|---|
| `unauthorized: authentication required` | Wrong password / admin user off / the trailing-space pipe |
| `COPY failed: … no such file or directory` | Context is the repo root: Dockerfile paths are repo-root-relative |
| Image builds locally but not in Jenkins | The workspace is a fresh clone — anything gitignored is not there (good, that is the point; fix the Dockerfile, not the ignore) |
| `denied: requested access to the resource is denied` | Image name does not start with `<acr>.azurecr.io/` |
| Push slow / every layer re-sent | First push only; later pushes reuse layers |

---

## Part D — The API runs from the image: Web App for Containers, continuous deployment (~45–60 min)

> Proven on the trainer's stack 2026-08-19 PM: `library-api-ctr` (Linux, **Free F1 — accepted
> for containers**) pulling `libraryapi0818.azurecr.io/library-api:latest`, settings as Key
> Vault references, `/api/Inventory` 200 over HTTPS; a push of a new `latest` fired the
> registry webhook and the app re-pulled and restarted by itself (Deployment Center log:
> `Pulling image … is pulled from registry … Starting container`). The portal click-path was
> not walked end-to-end by the trainer — blade names below are the portal's; if a label on
> your screen differs, the setting is the same, find it by meaning and tell the trainer.

### D1. The web app

Portal → **Create a resource → Web App** → **Basics**: your RG, name `<api-app>` (globally
unique), **Unique default hostname: Off** (so the host is exactly `<api-app>.azurewebsites.net`
— if you leave it On the portal appends a hash and you must read the **Default domain** from
**Overview** and use THAT everywhere these guides say `<api-app>.azurewebsites.net`),
**Publish: Container**, **Operating System: Linux**, region = your SQL's region, **Linux Plan:
create new** (a Linux plan is separate from Monday's Windows plan), **Pricing plan: Free F1**
(accepted for containers; B1 if you want no cold start at the showcase) → **Container** tab:
**Sidecar support: leave it OFF / "Docker container"** (Part D is written for the classic
single-container mode — sidecar mode changes where the port and registry settings live),
**Image Source: Azure Container Registry**, Registry `<acr>`, Image `<team>-api`, Tag `latest`
→ **Review + create → Create**.

(Behind the scenes: App Service logs into your registry with the admin credentials and pulls
`<team>-api:latest` on first start — 1–2 min; **Deployment Center → Logs** shows the pull.)

### D2. Continuous deployment ON — two switches

1. **Settings → Configuration → General settings → Platform settings → SCM Basic Auth
   Publishing Credentials: On → Save.** New web apps default this to Off, and the registry
   webhook calls the app's SCM site with those credentials — with it Off every webhook call
   is a **401** and nothing ever redeploys (the trainer hit exactly this).
2. **Deployment → Deployment Center** → the container tab (the trainer's app shows it as
   **Containers** next to *Logs* and *FTPS Credentials*; older portal builds label it
   *Settings*): **Source: Container Registry**, Registry source *Azure Container Registry*,
   registry / image / tag as above, **Continuous deployment: On** → **Save** (top of the blade). That creates a **webhook in your registry**
   (registry → **Services → Webhooks**: one row, pointing at `https://<api-app>.scm.
   azurewebsites.net/docker/hook`, scope `<team>-api:latest`) — every push of the `latest` tag
   now tells the app to pull and restart. **There is no deploy stage in the Jenkinsfile — the
   registry IS the deploy.**

### D3. Settings — the environment supplies everything

**Settings → Environment variables** (App settings) → **+ Add** each, then **Apply** (the app
restarts):

- `ConnectionStrings__<Name>` — your Azure SQL string (or a Key Vault reference exactly as
  Monday: first **Settings → Identity → System assigned: On → Save** on this NEW app, grant
  that identity **Key Vault Secrets User** on the vault, then the value
  `@Microsoft.KeyVault(VaultName=<kv>;SecretName=<name>)`);
- `Jwt__Key`;
- your CORS-origin setting with the static-site URL (the SPA's origin — guide 03 Part E;
  exact scheme + host, no trailing slash);
- **`WEBSITES_PORT` = `8080`** — the port your image EXPOSEs; App Service assumes 80 otherwise
  and the container never answers.

### D4. Prove it

- `https://<api-app>.azurewebsites.net<known-get>` → 200 (first answer after a cold pull can
  take a minute; **Log stream** if not).
- **Continuous deployment proof:** change something visible in the API (a string in the
  `<known-get>` payload, a header), commit, push, Build Now → after `Push to ACR` goes green,
  watch **Deployment Center → Logs**: a new pull + restart appears by itself; the change is live
  a minute later. No portal click in between. (Registry → **Webhooks → your hook → events**:
  a `push` row with response **202**; a **401** there is switch 1 above.)
- Tell the front-end pair the live API URL: `https://<api-app>.azurewebsites.net` — guide 03
  bakes it into the SPA build. Your Monday Windows web app can be stopped (or deleted) now.

**Part D DoD:** a push to `main` ends with the new API live without you touching the portal;
Deployment Center shows the automatic pull.

**Reds you will hit:**

| Symptom | Cause → fix |
|---|---|
| `Application Error` / 503 on first hit | Container still pulling (wait) or a startup exception → **Log stream** (missing env var, as Monday's 500.30) |
| Container starts, site never answers | `WEBSITES_PORT` missing (image listens on 8080, App Service expects 80) |
| `Login failed` / SQL timeout in the log | SQL server firewall: **Allow Azure services** ON (Monday's door) |
| Push happened, app did not restart; webhook events show **401** | SCM Basic Auth Publishing Credentials Off → switch 1, then Deployment Center Save again (the webhook credential is re-issued) |
| Push happened, app did not restart; no webhook event | Continuous deployment off, or the webhook watches a different tag than the one pushed (`latest`) |
| SPA gets CORS errors against the new API | The CORS-origin setting lacks the static-site origin (exact scheme + host, no trailing slash) |

---

## What the back-end pair hands the front-end pair

- The live API base URL `https://<api-app>.azurewebsites.net`.
- An unauthenticated `<known-get>` the wait loop can poll.
- Confirmation that the static-site origin is in the API's CORS setting.

**Then:** `docs/ci/README.md` sections for the API job (spec §5), screenshots (green build +
Test Result tab + registry tags + Deployment Center pull log), and the showcase's "one push,
live" rehearsal — run it once end-to-end before Friday.
