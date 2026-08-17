# Jenkins: Controller, Agents, Jobs, and Pipelines

## Learning Objectives
- Explain what Jenkins is (an open-source, self-hosted, Java-based automation server with a
  plugin ecosystem) and where it sits next to hosted CI services.
- Draw the controller/agent architecture: what the controller does, what agents do, why builds
  should not run on the built-in node, and how inbound (JNLP) and SSH agents differ.
- Stand up Jenkins in Docker, explain every flag in the `docker run` line, and walk the setup
  wizard from the initial admin password to the instance URL.
- Navigate the UI: Dashboard, New Item, Manage Jenkins (Plugins, Nodes, Credentials, System,
  Tools), Build History, Console Output, Workspace.
- Contrast Freestyle projects with Pipelines, write a declarative Jenkinsfile with stages and a
  `post` block, and explain "Pipeline script from SCM" versus a script typed into the UI.
- Use the Credentials store safely from a pipeline (`withCredentials`), and read Console Output
  to find the first real failure.

## Why This Matters
Jenkins is still the most widely deployed CI server in enterprises, especially .NET shops with
on-premises or hybrid infrastructure. Interviewers use it as a proxy for "have you shipped code
through a pipeline?" — what a controller and an agent are, what a Jenkinsfile looks like, how
you would debug a red build. Explaining why a build ran on the wrong node, or why the log
printed `****` instead of the registry password, is describing real operational experience, and
the vocabulary transfers one-to-one to GitHub Actions and Azure Pipelines (runners, jobs, steps,
secrets).

## The Concept

### What Jenkins is
Jenkins is an **automation server**: it watches for triggers (a push, a schedule, a click),
checks code out, runs the commands you define, and records the result. It is open source, free,
and **self-hosted** — you run it on your own VM or container, which is both its strength (full
control, no per-minute billing, reaches internal systems) and its cost (you patch, back up, and
scale it). It is written in **Java**, so the controller needs a JVM; the official Docker image
ships one. Almost every capability — Git checkout, pipeline syntax, JUnit parsing, credentials
— is a **plugin**, which is why the plugin manager is the first stop after install.

Releases come in two lines: **LTS** (long-term support, a stable line updated every 12 weeks
with security and bug fixes only — what you run in production) and **weekly** (new features,
what you try). The Docker tag `jenkins/jenkins:lts` picks the LTS line.

Trade-off against hosted CI (GitHub Actions, Azure Pipelines): Jenkins gives control and no
vendor lock-in; hosted gives zero maintenance and managed runners. Compliance-bound shops and
existing build farms keep Jenkins; greenfield teams on GitHub usually start hosted.

### Architecture: controller and agents
Jenkins is a **controller + agents** system:

- The **controller** (formerly "master") hosts the web UI, stores job configuration and build
  history under `JENKINS_HOME`, schedules builds, and hosts plugins. It should **not run heavy
  builds** — a compile that pegs the CPU also starves the scheduler and the UI, and any tool a
  build needs would have to be installed on the controller itself.
- **Agents** (also called **nodes**) are the machines where builds actually run. Each agent
  has **labels** (`windows`, `linux`, `docker`, `dotnet10`) that jobs use to say where they can
  run, and a number of **executors** — parallel build slots on that machine. An agent with 2
  executors can run two builds at once; set it to the number of builds the box can really
  handle. Each build gets an **agent workspace** — a folder on the agent
  (`<agent-root>/workspace/<job-name>`) where the checkout and outputs live.
- The **built-in node** is the controller itself acting as an agent. It ships with executors so
  a fresh install can build something, but the recommended production setting is **0 executors
  on the built-in node**: every build runs on a real agent.

Two ways an agent connects:

| Type | Direction | When you use it |
|---|---|---|
| **Inbound (JNLP) agent** | The agent process connects *out* to the controller on the **agent port** (default **50000**) using a secret from the node's page. | Windows agents, agents behind a firewall/NAT, Docker-based agents. Nothing on the controller has to reach the agent. |
| **SSH-launched agent** | The controller connects *in* to the agent over SSH and starts the agent JAR itself. | Linux VMs the controller can reach; the classic setup. |

The **tools-on-agent principle**: the agent machine must have every SDK a build needs — the
.NET SDK, Node, the Docker CLI, browsers for UI tests. The controller container has **none of
these**; it has Java and Jenkins. So "the build cannot find `dotnet`" is almost always "the job
ran on a node without the SDK". The agent's OS also dictates step syntax: a Windows agent runs
`bat` or `powershell` steps, a Linux agent runs `sh`.

Recognize on sight: `Running on built-in node in /var/jenkins_home/workspace/...` in a log
means the build landed on the controller — usually not what you meant.

### Running Jenkins in Docker
```
docker run -d --name jenkins ^
  -p 8080:8080 -p 50000:50000 ^
  -v jenkins_home:/var/jenkins_home ^
  jenkins/jenkins:lts
```
- `-p 8080:8080` — the web UI. `-p 50000:50000` — the classic inbound-agent TCP port
  (disabled by default on a fresh install — enable it under Manage Jenkins → Security →
  Agents if you use it). Inbound agents launched with `-webSocket` tunnel over the web port
  instead and need neither the TCP port nor its publish line; publishing 50000 is the
  conventional, harmless default.
- `-v jenkins_home:/var/jenkins_home` — a **named volume** for `JENKINS_HOME`: jobs, build
  history, plugins, credentials, users. Without it, `docker rm` deletes your entire Jenkins.
  This volume is also your **backup**: archive it and you can restore Jenkins on any host.
- `jenkins/jenkins:lts` — the official image, LTS line.

First run: the log (and the file) hold a one-time unlock key:
```
docker exec jenkins cat /var/jenkins_home/secrets/initialAdminPassword
```
Browse to `http://localhost:8080` and the **setup wizard** goes: unlock (paste the password) ->
**Install suggested plugins** (Git, Pipeline, Credentials, JUnit, and about 20 others) -> create
the **first admin user** -> confirm the **instance URL** (what links in emails and webhooks
point at). After that you land on the Dashboard.

### The UI map
- **Dashboard** — jobs with weather icons (recent stability) and last-build status.
- **New Item** — create a job: Freestyle project, Pipeline, Multibranch Pipeline, Folder.
- **Manage Jenkins** — the admin hub: **Plugins** (install/update), **Nodes** (add agents,
  executors, offline), **Credentials** (the secret store), **System** (instance URL, global
  env vars, mail), **Tools** (registered JDK/Git/Maven installs to auto-install on agents).
- On a job: **Build Now**, **Configure**, **Build History** (`#1`, `#2`...); on a build:
  **Console Output** (the full log), **Workspace** (browse the agent folder), test results and
  archived artifacts once plugins produce them.

### Job types: Freestyle vs Pipeline
A **Freestyle project** is configured entirely through UI forms: a **Source Code Management**
section (Git URL, branch, credentials), **Build Triggers** (poll SCM, webhook, periodic), **Build
Steps** ("Execute Windows batch command", "Execute shell"), and **Post-build Actions** ("Publish
JUnit test result report", "Archive the artifacts", email). Quick for one-off utility jobs;
its cost is that the definition lives in Jenkins' XML, not your repo — no review, no history,
no per-branch behaviour, and multi-stage flows model poorly.

A **Pipeline** job is defined by a **Jenkinsfile** — Groovy-based code. Two dialects:
**Declarative** (structured `pipeline { }` block, opinionated, what you write today) and
**Scripted** (`node { }` with free-form Groovy, older, more flexible, harder to read).

```groovy
pipeline {
  agent { label 'windows' }
  stages {
    stage('Build') {
      steps {
        bat 'dotnet build --configuration Release'
      }
    }
    stage('Test') {
      steps {
        bat 'dotnet test --no-build --configuration Release --logger "trx;LogFileName=results.trx" --results-directory TestResults'
      }
    }
  }
  post {
    always {
      junit '**/TestResults/*.xml'
      archiveArtifacts artifacts: '**/*.trx', allowEmptyArchive: true
    }
  }
}
```
Read aloud: run on an agent labelled `windows`; two stages in order; whatever happens, publish
JUnit XML and keep the `.trx` files. `post { always }` is why reports show up on red builds too;
other conditions: `success`, `failure`, `unstable`, `changed`.

Where the script lives: **Pipeline script** typed into the job's Configure page (fine for
experiments) versus **Pipeline script from SCM** — point the job at a Git repo, set the **Script
Path** (default `Jenkinsfile`), optionally tick **Lightweight checkout** (fetch just the
Jenkinsfile to start, full checkout inside the pipeline). This is **pipeline-as-code**: the
build definition is versioned with the app, reviewed in the same pull requests, and differs
per branch when it should. **Multibranch Pipeline** (name depth): one job that scans a repo and
creates a sub-job per branch or PR that contains a Jenkinsfile.

### Triggers
**Build Now** (manual), **Poll SCM** (cron such as `H/5 * * * *` — Jenkins asks Git for changes
every ~5 minutes; simple, adds load and latency), **webhooks** (the Git host POSTs to
`http://<jenkins>/github-webhook/` — instant, needs the controller reachable), and **upstream**
("build after other projects are built" — chain deploy after build). Declarative:
`triggers { pollSCM('H/5 * * * *') }`.

### Credentials
Secrets never go in the Jenkinsfile. **Manage Jenkins -> Credentials** stores them by **kind**:
**Username with password** (registry or Git login), **Secret text** (a token), **Secret file**
(a certificate or kubeconfig), SSH key. Each has an **ID** you reference from jobs, and a
**scope** — global (any job) or restricted to a folder/domain. In a pipeline:

```groovy
withCredentials([usernamePassword(credentialsId: 'acr',
                                  usernameVariable: 'U',
                                  passwordVariable: 'P')]) {
  bat 'docker login myregistry.azurecr.io -u %U% -p %P%'
}
```
Inside the block, `U` and `P` are environment variables on the agent; the log **masks** their
values as `****`. Two rules: never `echo` a secret (masking is best-effort, and a
transformation such as base64 defeats it), and never bake one into an image or the Jenkinsfile.
Adjacent question — *what is a Secret text used for?* An API token such as a Sonar token or a
GitHub PAT, bound with `string(credentialsId: 'sonar', variable: 'SONAR_TOKEN')`.

### A Docker build-and-push pipeline
```groovy
pipeline {
  agent { label 'docker' }           // an agent with the Docker CLI and daemon access
  environment {
    IMAGE = "myregistry.azurecr.io/order-api:${env.BUILD_NUMBER}"
  }
  stages {
    stage('Checkout') { steps { checkout scm } }
    stage('Build image') {
      steps { bat 'docker build -t %IMAGE% .' }
    }
    stage('Push') {
      steps {
        withCredentials([usernamePassword(credentialsId: 'acr',
            usernameVariable: 'U', passwordVariable: 'P')]) {
          bat 'docker login myregistry.azurecr.io -u %U% -p %P%'
          bat 'docker push %IMAGE%'
        }
      }
    }
  }
}
```
`checkout scm` re-uses the job's configured SCM. Tagging with `BUILD_NUMBER` gives every image
a traceable, increasing tag; `latest` alone tells you nothing about which build is running.

### Environment variables
Jenkins injects: `BUILD_NUMBER`, `BUILD_ID`, `JOB_NAME`, `WORKSPACE` (the agent folder for this
build), `BRANCH_NAME` (multibranch), `NODE_NAME`, `GIT_COMMIT` (with the Git plugin). In Groovy
they are `env.BUILD_NUMBER`; in `bat` steps `%BUILD_NUMBER%`; in `sh` steps `$BUILD_NUMBER`.
Define your own in an `environment { }` block at pipeline or stage level. Debug tip: `bat 'set'`
(or `sh 'env'`) dumps everything the step can see.

### Reading Console Output
Read **top-down to the first red line**. Jenkins prints each step's command (`+ dotnet build`
on Linux; the batch echo on Windows), then its output. The classic first failure on a new
agent is a **missing tool**: `'dotnet' is not recognized as an internal or external command`
(Windows) or `dotnet: not found` (Linux). Then comes the generic
`ERROR: script returned exit code 1` — that is Jenkins reporting the step failed; the *cause*
is above it. Other shapes: `Running on <node>` (confirms which agent), `[Pipeline] { (Build) }`
(stage boundaries), `Finished: FAILURE | UNSTABLE | SUCCESS` (UNSTABLE = build ran but tests
failed or a publisher marked it).

### Plugins to know
**Git** (SCM checkout), **Pipeline** (the `pipeline {}` DSL — actually a suite), **Credentials**
and **Credentials Binding** (`withCredentials`), **JUnit** (`junit` step, test trend graphs),
**Docker Pipeline** (name depth: `docker.build()` / `docker.withRegistry()` and `agent {
docker { image '...' } }`), **Pipeline Graph View** / **Blue Ocean** (visual stage view).
Fewer plugins = fewer upgrade breaks; install what you use.

### Security basics
Jenkins runs commands on your machines, so treat it as production. **Authorization**: the
wizard default is "logged-in users can do anything"; **Matrix-based security** (or the
Role-based plugin) grants per-user/group permissions such as Job/Build vs Job/Configure. **API
tokens**: per-user tokens for scripts and webhooks instead of passwords. **CSRF crumb**: POSTs
to the API need a crumb header (an API token exempts the call) — why a plain `curl -X POST
.../build` returns 403. Keep the controller off the public internet or behind TLS.

## Say It in an Interview
- **"What is Jenkins?"** — "An open-source, self-hosted automation server written in Java. It
  runs your build, test, and deploy steps on triggers, and almost all its features are
  plugins. You run the LTS line in production and you own the hosting, which is both the
  appeal and the maintenance cost compared to hosted CI."
- **"Controller vs agent?"** — "The controller is the brain: web UI, job configs, scheduling,
  plugins. Agents are where builds run; they carry the SDKs and are chosen by label. Builds
  should not run on the built-in node because they starve the controller and force tools onto
  it. Inbound agents connect out on port 50000; SSH agents are launched by the controller."
- **"How do you run Jenkins in Docker?"** — "`docker run` the `jenkins/jenkins:lts` image
  publishing 8080 for the UI and 50000 for agents, with a named volume on
  `/var/jenkins_home` so nothing is lost when the container is recreated — that volume is also
  the backup. Read the initial admin password with `docker exec`, then the wizard installs
  suggested plugins and creates the admin user."
- **"Freestyle vs Pipeline?"** — "Freestyle is UI-configured steps stored in Jenkins; Pipeline
  is a Jenkinsfile in the repo — versioned, reviewed, multi-stage, with `post` blocks. I write
  declarative pipelines loaded from SCM."
- **"How do you handle secrets in Jenkins?"** — "Store them in the Credentials store by kind
  and reference them by ID with `withCredentials`, which exposes them as environment
  variables inside the block and masks them in the log. Never echo them or hard-code them in
  the Jenkinsfile."
- **"A build went red — what do you do?"** — "Open Console Output and read top-down for the
  first error, not the last line. `script returned exit code 1` is the symptom; the cause is
  above it, and the classic one is a tool missing on the agent that ran the job."

## Check Yourself
1. A build fails with `'dotnet' is not recognized as an internal or external command`. What is
   the most likely cause and where do you fix it?
2. Why publish port 50000 in addition to 8080?
3. Your Jenkins container was removed and recreated and all jobs vanished. What was missing?
4. Which block guarantees `junit` runs even when the Test stage fails?
5. What is the difference between "Pipeline script" and "Pipeline script from SCM"?

**Answers:** (1) The job ran on an agent (or the built-in node) without the .NET SDK — fix by
installing the SDK on the agent or targeting an agent labelled with it; the controller
container never has it. (2) 50000 is the classic inbound (JNLP) agent TCP port; agents using
that transport cannot connect without it — WebSocket-launched inbound agents use the web
port instead. (3) The named volume on `/var/jenkins_home` — all state lives there.
(4) `post { always { ... } }`. (5) The first is typed into the job in the UI; the second reads a
Jenkinsfile from the repo at a script path — pipeline-as-code, versioned with the app.

## Resources
- [Installing Jenkins with Docker (jenkins.io)](https://www.jenkins.io/doc/book/installing/docker/)
- [Managing nodes: controller, agents, executors (jenkins.io)](https://www.jenkins.io/doc/book/managing/nodes/)
- [Pipeline syntax reference (jenkins.io)](https://www.jenkins.io/doc/book/pipeline/syntax/)
- [Using a Jenkinsfile (jenkins.io)](https://www.jenkins.io/doc/book/pipeline/jenkinsfile/)
- [Using credentials (jenkins.io)](https://www.jenkins.io/doc/book/using/using-credentials/)
- [Jenkins glossary (jenkins.io)](https://www.jenkins.io/doc/book/glossary/)
