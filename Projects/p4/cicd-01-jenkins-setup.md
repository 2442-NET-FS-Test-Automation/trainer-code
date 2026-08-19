# Guide 01 — Jenkins + the Windows agent, from zero (~60–90 min, one laptop per team; everyone reads)

> **Read first:** `cicd-pipeline-README.md` (the index — names, order of work, definition of
> done). This guide gets **one laptop** in your team to a Jenkins controller, a Windows agent,
> and a green pipeline that can see `dotnet`, `node`, `docker`, and `azcopy`. Guides 02 and 03
> assume that laptop. Teammates may also run their own Jenkins to experiment — the showcase
> needs one.
>
> **Prerequisites on the hosting laptop:** Docker Desktop running; your monorepo cloned; the
> same `dotnet` / `node` / `docker` / Chrome that run your P2 app and P3 suites locally; ~2 GB
> free disk; about 90 minutes.

## 0. The ten-minute theory (read it, do not skip it)

- **CI — Continuous Integration:** every push to `main` triggers an automated build + tests; a
  red build is everyone's first job. *CI is a practice; Jenkins is a tool that enforces it.*
- **Continuous Delivery vs Continuous Deployment:** build → tests → package → deploy → smoke →
  **[gate]** → production. *Delivery:* every green build is deployable, a human presses the
  button. *Deployment:* nobody presses it. One manual gate is the entire difference.
- **A CI server** (1) notices a change, (2) checks it out, (3) runs build + tests **on some
  machine**, (4) publishes results, (5) optionally deploys. GitHub Actions, GitLab CI, Azure
  Pipelines, **Jenkins** — the same loop, different UI. Jenkins = self-hosted, Java, plugins for
  everything, the one you meet in enterprises.
- **Jenkins architecture — today's whole lesson:** a **controller** (the web UI, job
  definitions, credentials, scheduling, build history — the brain) and **agents** (machines with
  the tools where stages actually execute — the hands). The controller hands a build to an
  agent whose **label** matches; the agent streams the log back. You will FEEL this in step 4
  when the controller tries to run `dotnet` and cannot.

**Where you end up by Friday** (the two pipelines guides 02 and 03 build):

```
 your laptop                                              GitHub (your monorepo)
 ┌──────────────────────────────────────────────┐          ┌─────────────────────────┐
 │ Docker: jenkins/jenkins:lts  :8080           │ Pipeline │ api/ + spa/ + tests/    │
 │   controller - UI, jobs, credentials         │◄─────────│ + Jenkinsfile x2        │
 │          │ WebSocket (inbound)               │ from SCM └─────────────────────────┘
 │ Windows agent  label 'windows'               │
 │   C:\jenkins-agent  (dotnet/node/docker/     │
 │   azcopy/Chrome live HERE - every stage)     │
 └───────────┬──────────────────────────────────┘
             │ API job: checkout -> build -> dotnet test (THE GATE, junit) -> docker build -> push <your ACR>
             │                                                  (Web App for Containers pulls it by itself)
             │ SPA job: npm run build (live API URL) -> azcopy sync dist -> $web (your static site)
             ▼          -> wait for the API -> npx cypress run vs the LIVE site (smoke) -> junit + screenshots
 Azure (yours): ACR -> Web App for Containers (API)   |   Blob $web (SPA)   |   Azure SQL
```

## 1. Installs (run these first — they download while you read)

PowerShell, on the hosting laptop:

```powershell
docker pull jenkins/jenkins:lts
```

Leave it running (~480 MB). A second terminal:

```powershell
java -version
```

If that prints a version 17 or newer, you are done with Java. If it says *not recognized*:

```powershell
winget install EclipseAdoptium.Temurin.21.JRE
```

(UAC prompt, a one-time `Y` for winget's source agreement, ~1 min.) **Java is for the agent** —
the Jenkins agent is a small Java program that will run on this laptop. Jenkins itself runs in
the container, which brings its own Java. Then the tool guide 03 needs for the blob push —
install it NOW so the agent you launch in step 5 inherits it:

```powershell
winget install Microsoft.Azure.AZCopy.10
```

(user scope, no UAC, ~20 s.) **Re-open PowerShell after installs** — PATH changes do not reach
an already-open window; `java -version` and `azcopy --version` must both answer in the NEW
window. Last check: `netstat -ano | findstr :8080` — if something already listens on 8080,
stop it; do not remap the port, every URL in these guides says 8080.

## 2. Run Jenkins: one `docker run` + four screens

```powershell
docker run -d --name jenkins -p 8080:8080 -p 50000:50000 -v jenkins_home:/var/jenkins_home jenkins/jenkins:lts
docker ps
docker logs jenkins -f
```

Read the flags against Tuesday: `lts` = long-term-support tag, what production runs; `8080` =
the web UI; `50000` = the classic agent port (published by convention — our agent calls home
over the web port instead); **`-v jenkins_home:/var/jenkins_home` = EVERYTHING Jenkins knows —
jobs, plugins, users, credentials, build history — in a NAMED VOLUME**, so `docker rm jenkins`
costs nothing but a restart with the same line.

Watch the log (~15 s) until the banner *Jenkins initial setup is required… Please use the
following password…* — a 32-hex string between the asterisk bars. `Ctrl+C` out of `-f`. Read
it the way the docs do, too:

```powershell
docker exec jenkins cat /var/jenkins_home/secrets/initialAdminPassword
```

(`exec` into the container to read a file the container wrote — the setup's first screen is a
proof of ownership: whoever can read that file owns the box.)

Browser → **`http://localhost:8080`**:

1. **Unlock Jenkins** — paste the password → **Continue**.
2. **Customize Jenkins** → **Install suggested plugins** (Git, Pipeline, Credentials, JUnit,
   folders, timestamps… ~90 plugins, ~1–2 min; a plugin per capability is Jenkins' whole
   design; "suggested" is the sane baseline; add nothing else this week).
3. **Create First Admin User** — username `admin` (any), a password you will remember, name,
   email → **Save and Continue**.
4. **Instance Configuration** — Jenkins URL `http://localhost:8080/` → **Save and Finish** →
   **Start using Jenkins**. The dashboard.

Doubt the volume claim? `docker volume inspect jenkins_home`.

## 3. Four places you will live

Left nav / top bar: **New Item** (a job) · **Manage Jenkins → Nodes** (one node: *Built-In
Node* — the controller itself, running builds unless told not to; step 5 changes that) ·
**Manage Jenkins → Credentials** (where passwords live so pipelines never print them — the ACR
password, the SAS, the JWT key, tomorrow) · inside any job: **Build Now** / **Configure** / a
build's **Console Output** — *Console Output is 80% of Jenkins: when a build is red, read it
top-down for the first red line.*

## 4. A pipeline job typed in the UI — and the red you WANT to see

**New Item** → name `hello-pipeline` → **Pipeline** → **OK**. Scroll to **Pipeline** →
Definition **Pipeline script** → type:

```groovy
pipeline {
    agent any
    stages {
        stage('Hello') {
            steps {
                sh 'echo Hello from a pipeline'
                sh 'hostname'
            }
        }
        stage('Tools') {
            steps {
                sh 'dotnet --version'
            }
        }
    }
}
```

Declarative pipeline: `pipeline` → `agent` (WHERE it runs — `any` = whatever is free, i.e. the
built-in node right now) → `stages` → `stage` → `steps`. `sh` = a shell command on a Linux
agent (`bat` / `powershell` on Windows — remember that). This text is what a Jenkinsfile IS;
guides 02/03 put it in your repo, diffed and reviewed like code — pipeline-as-code — and the
job reads it from there ("Pipeline script from SCM": SCM = source control = your Git repo).

**Save** → **Build Now** → click **#1** → the stage view: `Hello` green, `Tools` **red**. Open
**Console Output**: `+ hostname` prints a 12-hex container id (you are INSIDE the Jenkins
container), then `+ dotnet --version` / `dotnet: not found` / `ERROR: script returned exit code
127` / `Finished: FAILURE`.

**This failure is the lesson.** Exit code 127 = the shell could not find the command. This
Jenkins is a Linux container that contains JAVA and JENKINS and nothing else — no .NET SDK, no
Node, no Docker CLI, no Chrome. It cannot build your API, run your tests, build your image, or
run your SPA. It was never meant to. Two bad fixes people reach for: (1) install everything INTO
the Jenkins container — now your CI server is a snowflake nobody can rebuild; (2) mount the
host's Docker socket into it — now anything that runs a pipeline is root on your laptop. **The
real fix is architectural: the controller ORCHESTRATES; an AGENT with the tools BUILDS.** Your
laptop already has every tool your project needs — it becomes the agent.

(Self-check: `sh 'docker version'` in this pipeline — pass or fail, and why? Fail, `docker: not
found`, exit 127, same reason.)

## 5. The Windows agent: a node in the UI, one launch line

Two ways an agent joins: the controller SSHes out to it, or the agent **connects IN** (inbound —
a small `agent.jar` on the agent machine phones home). Inbound is what a laptop behind a NAT
does, and what we do. In a real shop the agent is a separate box or a fresh container per
build; a laptop agent is the teaching shape — same architecture, one machine.

**Manage Jenkins → Nodes → + New node**: name `windows-agent`, type **Permanent Agent** →
**Create**. Configure: **Remote root directory** `C:\jenkins-agent`; **Labels** `windows`;
**Usage** *Use this node as much as possible*; **Launch method** *Launch agent by connecting it
to the controller* (already selected — the inbound shape); leave the rest → **Save**. The node
page shows **offline** with the exact commands to run on the agent machine in four flavours
(Unix / Windows, secret inline / secret in a file) and the note *PowerShell users must use
curl.exe*. Use the **Windows, secret INLINE** block. The secret is a per-node token — a
password for this node. (The "secret in a file" flavour does NOT work from PowerShell 5.1 — its
`echo x> file` writes no file — so: inline.)

In a **NEW PowerShell opened AFTER step 1's installs** (the agent inherits the PATH of the
terminal that launches it — an older window means `azcopy`, maybe `java`, red in step 6):

```powershell
mkdir C:\jenkins-agent
cd C:\jenkins-agent
java -version
curl.exe -fsSO http://localhost:8080/jnlpJars/agent.jar
dir
java -jar agent.jar -url http://localhost:8080/ -secret <the secret from YOUR node page> -name "windows-agent" -webSocket -workDir "C:\jenkins-agent"
```

That is the node page's own block with two edits — `curl.exe` **with `-fsSO`** (fails loudly
instead of saving an error page) and a `dir` so you SEE `agent.jar` (~1.4 MB) before
launching. **Copy the launch line from YOUR node page — the page is the authority.** **`curl.exe`,
with the `.exe`:** in PowerShell bare `curl` is an alias for `Invoke-WebRequest` and dies with
`A parameter cannot be found that matches parameter name 'sO'`; then `java -jar agent.jar` says
`Unable to access jarfile agent.jar` because nothing was downloaded. `-webSocket` uses the web
port, so `:50000` is not even needed.

Leave that terminal running — the log ends `INFO: WebSocket connection open` / `INFO:
Connected`. Refresh the node page: **online**, with the machine's name, OS = Windows, the Java
version. **That was the whole agent: a jar, a URL, a secret.** It runs as YOU on YOUR machine —
so it has your PATH: `dotnet`, `node`, `docker`, `azcopy`, Chrome. On a build server it runs as
a service under a build account; the ideas are identical. **Close that terminal and your agent
is offline** — keep it open whenever you want builds to run; if it closes, re-run the last line.

**Labels are how pipelines pick a machine:** `agent { label 'windows' }` — every Jenkinsfile you
write this week says it.

## 6. Green on the agent

`hello-pipeline` → **Configure** → change the script — **TWO edits: the agent line AND every
`sh` → `bat`** (change only the first and you get `Cannot run program "sh" … The system cannot
find the file specified` — the Windows mirror of step 4's red: a Linux box has no `dotnet`, a
Windows box has no `sh`):

```groovy
pipeline {
    agent { label 'windows' }
    stages {
        stage('Hello') {
            steps {
                bat 'echo Hello from a pipeline'
                bat 'hostname'
            }
        }
        stage('Tools') {
            steps {
                bat 'dotnet --version'
                bat 'node -v'
                bat 'docker version'
                bat 'azcopy --version'
                bat 'git --version'
            }
        }
    }
}
```

**Save → Build Now → #2**: both stages green; Console Output: `Running on windows-agent in
C:\jenkins-agent\workspace\hello-pipeline`, `hostname` = this laptop, `10.0.x`, `v22.x`, Docker
Client AND Server, `azcopy version 10.x`, `git version 2.x` (guide 02's first step — *Checkout
SCM* — needs `git` on this agent's PATH). Same pipeline, two changes — WHERE it runs, and the
shell that machine speaks. Every stage in guides 02/03 starts with that agent line and says
`bat` / `powershell`.

**Guide 01 DoD:** `hello-pipeline` #2 green on `windows-agent` with all four tools printed.
Keep the red #1 in the history — it is the best controller-vs-agent explanation you will ever
give at the showcase.

## Reds you will hit (read before you raise a hand)

| Symptom | Cause → fix |
|---|---|
| `java` is not recognized | Step 1 skipped, or the terminal predates the install → re-open PowerShell |
| `Unable to access jarfile agent.jar` | The fetch never landed: bare `curl` (the `'sO'` parameter error) or it ran in another folder → `cd C:\jenkins-agent`, `curl.exe -fsSO …`, `dir`, relaunch |
| `Invalid or corrupt jarfile` | `curl.exe` saved an error page as `agent.jar` (Jenkins down / URL mistyped) → open `http://localhost:8080/jnlpJars/agent.jar` in the browser, then re-fetch |
| Handshake failure / `Unauthorized` on connect | Pasted secret has a trailing space or a missed character → re-copy from the node page |
| Node online but `azcopy` red in the pipeline | Agent launched from a terminal older than the install → close the agent terminal, open a new one, relaunch the `java -jar` line |
| `Cannot run program "sh"` | You changed the agent line but not `sh` → `bat` |
| Port 8080 in use | Something else listens → stop it; do not remap |
| The image pull crawls on the venue wifi | Hotspot, or let it run under the theory reading; everything else waits on it |

## What this guide did NOT cover (so you know the names)

Freestyle jobs (click-configured — everything real is a Pipeline); triggers beyond Build Now —
poll-SCM cron and GitHub webhooks (the "noticing" half; no public endpoint in this room, so we
press the button; poll-SCM is a stretch line in the spec); Multibranch pipelines; SSH-launched
agents and agents as containers; executors and the built-in node's zero-executor setting;
Jenkins security beyond the first admin; Scripted pipeline syntax; SonarQube/SonarLint (notes).

**Next:** guide 02 (back-end pair) and guide 03 (front-end pair) — both start on this laptop's
Jenkins; the front-end pair can start guide 03 E1–E2 and F1 on their own machines (install
`azcopy` there too) while the back-end pair gets the first from-SCM build green.
