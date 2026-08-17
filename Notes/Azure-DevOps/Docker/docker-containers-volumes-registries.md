# Docker Containers, Volumes, and Registries: the Working Commands

## Learning Objectives
- Walk a container through its lifecycle states (created, running, paused, exited, removed) and
  name the command behind each transition — including what `docker stop` actually sends.
- Use the day-to-day commands (`run`, `ps`, `logs`, `exec`, `inspect`, `top`, `stats`, `cp`) to
  create, observe, and debug a container.
- Explain where a process's data lives (the writable layer), why it survives stop/start but dies
  on `docker rm`, and what that means for a database container.
- Distinguish named volumes, anonymous volumes, and bind mounts, and pick the right one.
- Describe what an image registry is, name the common ones, and run the login / tag / push / pull
  cycle against a private registry such as Azure Container Registry.
- Read the command cheatsheet as a reference, recognizing the `docker <noun> <verb>` management
  form next to its legacy short form.

## Why This Matters
Once a team ships in containers, the container CLI is the operations vocabulary: "is it running?",
"what did it log?", "get me a shell in it", "why did the data vanish when we recreated it?".
Interviewers test it with small concrete prompts — "your SQL container was recreated and the
database is gone, what happened?", "volume versus bind mount?" — because they separate people who
have run containers from people who have read about them. Registries close the loop: `docker
login`, `tag`, `push`, and how a private registry authenticates are the hand-off between "works on
my machine" and "deployed".

## The Concept

### The container lifecycle
A container is a process tree started from an image with its own filesystem view, network
namespace, and resource limits. Docker tracks it through five states, and each arrow is a command:
**created** (`docker create`) -> **running** (`start`; or `docker run` does both) -> **paused**
(`pause` freezes every process without a signal; `unpause` resumes) -> **exited** (`stop`, `kill`,
or the main process ended; `start` runs it again) -> **removed** (`docker rm`, or `--rm` at exit;
gone, writable layer included). The flags you type daily:

```bash
docker run -d \                       # detached: print the ID, do not attach
  --name catalog-api \                # stable name instead of a random one
  -p 8080:8080 \                      # host:container port publish
  -e ASPNETCORE_ENVIRONMENT=Production \
  -v catalogdata:/app/data \          # named volume at /app/data
  --restart unless-stopped \          # daemon restarts it after a crash or reboot
  ghcr.io/example/catalog-api:1.4.2
```

`--rm` deletes the container the moment it exits (one-shot tools, never stateful services).
`--restart` takes `no` (default), `on-failure[:N]`, `always`, `unless-stopped`.

**Stopping is a two-step signal.** `docker stop` sends SIGTERM to PID 1, waits 10 seconds (`-t`
changes it), then SIGKILL. A .NET host treats SIGTERM as graceful shutdown; a process that ignores
it is killed at the deadline. `docker kill` is SIGKILL now; `docker restart` is stop then start.

`docker rm` refuses a running container — `cannot remove container "catalog-api": container is
running: stop the container before removing or force remove`. Stop first, or `docker rm -f`
(kill then remove). The refusal is the last guard before the writable layer is destroyed.

### Observing and debugging
```bash
docker ps                                  # running containers; -a includes exited/created
docker logs --tail 50 -f catalog-api       # last 50 lines of stdout/stderr, then follow
docker exec -it catalog-api sh             # interactive shell inside (bash if the image has it)
docker top catalog-api                     # processes inside, as the host sees them
docker stats catalog-api                   # live CPU / memory / net / block IO
docker port catalog-api                    # 8080/tcp -> 0.0.0.0:8080
docker cp catalog-api:/app/logs/app.log .  # copy a file out (or in); works on stopped containers
docker inspect --format '{{.State.Status}} {{.NetworkSettings.IPAddress}}' catalog-api
# running 172.17.0.3
```

`docker inspect` alone dumps the full JSON; `--format` takes a Go template to pull one field.
Recognize on sight: `docker logs` shows only what the process wrote to stdout/stderr — an app that
logs to a file inside the container shows nothing here, which is why container-native apps log to
the console.

### Stopped vs removed: the writable layer
An image is a stack of read-only layers. A started container adds one thin **writable layer** on
top; every file the process creates or changes lands there (copy-on-write). Two facts follow:

- **Stop/start keeps it.** An exited container still owns its writable layer; `docker start` brings
  the same filesystem back — database files, log files, uploads included.
- **rm destroys it.** `docker rm` deletes the container *and* the layer. Recreating from the same
  image gives a fresh, empty layer. That is the "we recreated the container and the database
  vanished" story, and the reason volumes exist.

Trade-off: the writable layer needs no setup, but it is tied to one container's life, not
shareable, and slower than a volume for write-heavy work.

### The three kinds of mounts
Anything that must outlive the container, be shared, or be edited from the host is mounted from
outside the writable layer.

- **Named volume** — `-v name:/path`. Docker creates and manages the storage (under
  `/var/lib/docker/volumes/` on Linux; inside the WSL2 VM on Docker Desktop for Windows). Survives
  `docker rm`; managed with `docker volume create / ls / inspect / rm / prune` (`rm` refuses while
  a container references it).
- **Anonymous volume** — from a Dockerfile `VOLUME /var/lib/data` with nothing mounted there, or
  `-v /path` with no name. Same mechanics, but named by a 64-hex hash and easy to orphan: each
  recreation makes another. `docker volume ls` full of hashes is the symptom.
- **Bind mount** — `-v <host-path>:<container-path>[:ro]`. A real host directory; both sides see
  the same files instantly — the dev-loop tool. On Docker Desktop for Windows the path
  (`C:\sites\catalog`) is translated into the WSL2 VM's view of the drive: it works, but file
  watching and I/O are slower than on native Linux.

```bash
# bind mount: serve a folder of static HTML with nginx, read-only, on port 8081
docker run -d --name site -p 8081:80 -v ${PWD}/dist:/usr/share/nginx/html:ro nginx:alpine
```

**When to use which:** named volume = data the container owns (a database's files, uploads);
bind mount = host files the container should see (source in development, config, a static site);
anonymous = usually an accident to clean up.

### Worked example: SQL Server with and without a volume
```bash
docker run -d --name sql -p 1433:1433 \
  -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='Str0ng!Passw0rd' \
  -v sqldata:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2022-latest
```

`/var/opt/mssql` is where the engine writes its `.mdf`/`.ldf` files. With `-v sqldata:...` they
live in the volume: `docker rm -f sql` followed by the same `docker run` brings every database
back. **Without** `-v`, they live in the writable layer — `stop`/`start` keeps them, `docker rm`
destroys them, and re-running the image does not get them back.

No client install is needed to check: `sqlcmd` ships inside the image at
`/opt/mssql-tools18/bin/sqlcmd` (the 18 tools default to encrypted connections, so `-C` trusts the
container's self-signed certificate).

```bash
docker exec -it sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Str0ng!Passw0rd' -C \
  -Q "SELECT name FROM sys.databases"
# name
# ------------
# master  tempdb  model  msdb  LibraryCatalog   (one per row)
```

**Backup pattern (awareness):** a volume is just files, so a throwaway container that mounts the
volume plus a host folder can tar it out; restore is the mirror. For a live database prefer the
engine's own `BACKUP DATABASE` — file copies of open data files can be inconsistent.

```bash
docker run --rm -v sqldata:/data -v ${PWD}:/backup alpine tar czf /backup/sqldata.tgz -C /data .
```

### Command cheatsheet
Modern Docker groups commands as `docker <noun> <verb>` (management commands); the legacy short
forms still work and are what most people type. Both are shown where they differ.

| Command | What it does | Note |
|---|---|---|
| **Images** | | |
| `docker pull IMAGE[:TAG]` (`image pull`) | Download an image and its layers | Default tag `latest`; default registry Docker Hub |
| `docker build -t NAME:TAG .` (`image build`) | Build from the Dockerfile in `.` | `.` is the build context; `-f` picks a file |
| `docker tag SRC TARGET` (`image tag`) | Add another name/tag to the same image ID | Free; how you add a registry host before push |
| `docker push NAME:TAG` (`image push`) | Upload to the registry in the name | Only missing layers are sent |
| `docker images` (`image ls`) | List local images | `-a` includes intermediate layers |
| `docker rmi IMAGE` (`image rm`) | Delete a local image | Fails if a container uses it; `-f` overrides |
| `docker history IMAGE` (`image history`) | Layers and the instruction that made each | Spot bloated layers |
| `docker image prune` | Remove dangling (untagged) images | `-a` removes all unused images |
| **Containers** | | |
| `docker run [FLAGS] IMAGE [CMD]` (`container run`) | Create and start | `-d --name -p -e -v --rm --restart -it` |
| `docker create IMAGE` | Create without starting | State: created |
| `docker start / stop / restart NAME` | Start; SIGTERM then SIGKILL after 10 s; stop+start | `stop -t 30` lengthens the grace period |
| `docker kill NAME` | SIGKILL now | `--signal SIGHUP` etc. |
| `docker pause / unpause NAME` | Freeze / thaw all processes | No signal reaches the app |
| `docker rm NAME` (`container rm`) | Delete a stopped container and its writable layer | Refuses running; `-f` kills first; `-v` also drops anonymous volumes |
| `docker ps` / `docker ps -a` (`container ls`) | List running / all containers | `-q` for IDs only |
| `docker logs NAME` | Show stdout/stderr | `--tail N`, `-f`, `--since 10m` |
| `docker exec [-it] NAME CMD` | Run a command in a running container | `-it sh` for a shell |
| `docker inspect NAME` | Full JSON of config, mounts, network, state | `--format` Go template |
| `docker top NAME` | Processes inside | Host-side PIDs |
| `docker stats [NAME]` | Live resource usage | `--no-stream` for one shot |
| `docker cp SRC DEST` | Copy files in or out (`name:/path`) | Works on stopped containers |
| `docker port NAME` | Published port mappings | Same as the `ps` PORTS column |
| **Volumes** | | |
| `docker volume create NAME` | Create a named volume | Also implicit from `-v NAME:/path` |
| `docker volume ls` | List volumes | Hash names = anonymous |
| `docker volume inspect NAME` | Driver, mountpoint, labels | |
| `docker volume rm NAME` | Delete a volume | Refuses while referenced |
| `docker volume prune` | Delete all unreferenced volumes | Data loss for orphaned-but-wanted volumes |
| **Networks** | | |
| `docker network ls` | List networks (`bridge`, `host`, `none` + yours) | |
| `docker network create NAME` | Create a user-defined bridge | Members resolve each other by name |
| `docker network inspect NAME` | Subnet, connected containers | |
| **System** | | |
| `docker version` | Client and server versions | Server missing = daemon not running |
| `docker info` | Daemon-wide facts: storage driver, counts | |
| `docker system df` | Disk used by images, containers, volumes, build cache | `-v` per item |
| `docker system prune` | Remove stopped containers, unused networks, dangling images, build cache | `-a` all unused images; `--volumes` too |
| **Registry** | | |
| `docker login [HOST]` | Authenticate (Docker Hub if no host) | `-u USER --password-stdin`; never `-p` |
| `docker logout [HOST]` | Forget stored credentials | |
| `docker search TERM` | Search Docker Hub | Hub only |

### Registries: where images live between build and run
A **registry** is an HTTP service that stores image layers by content digest (`sha256:...`) and
maps tags (`catalog-api:1.4.2`) to manifests listing those layers. Because layers are addressed
by digest, two images sharing a base layer store it once, and a push transfers only what the
registry lacks.

The registry is part of the image name: `[HOST[:PORT]/][NAMESPACE/]NAME[:TAG]`. No host means
Docker Hub (`docker.io`); no namespace on Hub means `library/` (official images); no tag means
`latest`. `nginx:alpine` is really `docker.io/library/nginx:alpine`.

Registries you will meet: **Docker Hub** (`docker.io`, the default; public and official images,
anonymous pulls rate-limited); **Microsoft Container Registry** (`mcr.microsoft.com`; .NET
SDK/runtime, SQL Server, Windows base images; pull-only); **GitHub Container Registry**
(`ghcr.io`; images next to a repo); and the cloud-vendor private registries — **AWS ECR**
(`<acct>.dkr.ecr.<region>.amazonaws.com`), **Google Artifact Registry**
(`<region>-docker.pkg.dev`), **Azure Container Registry** (`<name>.azurecr.io`).

The push cycle against any private registry:

```bash
docker login catalogreg.azurecr.io -u catalogreg        # interactive prompt; or --password-stdin
# Password: ********
# Login Succeeded
docker tag catalog-api:1.4.2 catalogreg.azurecr.io/catalog-api:1.4.2   # name carries the host
docker push catalogreg.azurecr.io/catalog-api:1.4.2
# 5f70bf18a086: Layer already exists
# 9c3d2b1e4f00: Mounted from dotnet/aspnet
# 1.4.2: digest: sha256:8a1f... size: 1571
docker pull catalogreg.azurecr.io/catalog-api@sha256:8a1f...          # by tag, or by immutable digest
docker logout catalogreg.azurecr.io
```

In scripts use `echo "$PASSWORD" | docker login HOST -u USER --password-stdin`; `-p` lands the
secret in the process list and shell history.

**Azure Container Registry at concept depth.** A managed private registry, login server
`<name>.azurecr.io`, three SKUs: Basic (about 10 GiB included, roughly $0.167 per day — a team or
a lab), Standard (100 GiB, more throughput), Premium (500 GiB, geo-replication, private
endpoints). Two authentication doors: the **admin user** (Access keys blade; one shared username
with two regenerable passwords; disabled by default; everyone using it is the same identity — a
teaching/dev door, not production) versus a **service principal or managed identity** holding
`AcrPush` or `AcrPull` — the production door: per-workload, revocable, audited. The
**Repositories** blade lists what was pushed, per tag, with its digest.

**Registry in CI/CD.** The registry is the hand-off between build and every environment: build
once, tag with the version (often the commit SHA too), push, and test, staging, and production
all pull the *same digest*. Rebuilding per environment silently produces different images; the
registry is what makes "what we tested is what we ship" true.

## Say It in an Interview
- **"What happens when you run `docker stop`?"** — "Docker sends SIGTERM to the container's main
  process, waits ten seconds by default, then SIGKILL. `docker kill` skips straight to SIGKILL. A
  well-behaved app treats SIGTERM as graceful shutdown."
- **"How do you debug a misbehaving container?"** — "`docker ps -a` for state, `docker logs -f`
  for stdout, `docker exec -it name sh` for a shell, `docker inspect --format` for one fact,
  `docker stats` if I suspect memory."
- **"You recreated a database container and the data is gone. Why?"** — "It was in the writable
  layer, which `docker rm` deletes. Stop/start keeps it; remove destroys it. The fix is a named
  volume at the engine's data path — the volume outlives the container."
- **"Volume vs bind mount?"** — "A named volume is Docker-managed storage the container owns —
  database files. A bind mount is a real host directory the container sees — source in a dev loop,
  a config folder. Anonymous volumes come from a Dockerfile `VOLUME` and are the ones you find
  orphaned."
- **"How do you push to a private registry?"** — "`docker login` to the host, `docker tag` the
  image so its name carries the host, `docker push` — only missing layers upload. On Azure the
  host is `name.azurecr.io`, and production should authenticate with a managed identity holding
  AcrPull rather than the shared admin user."
- **"Where does the registry sit in CI/CD?"** — "It is the hand-off: build once, push a digest,
  and every environment pulls that same digest, so what was tested is what ships."

## Check Yourself
1. A container is `exited`. Which two commands move it out of that state, and where does each
   leave the writable layer?
2. `docker volume ls` shows twelve 64-character hex names. What made them, and what is the risk
   of `docker volume prune`?
3. Write the flag that puts SQL Server's data on a named volume, and say what changes without it.
4. `docker rm sql` fails. What is the message telling you, and what are the two ways forward?
5. A push prints "Layer already exists" for four of five layers. Why is that expected and good?

**Answers:** (1) `docker start` (running; same layer comes back) or `docker rm` (removed; layer
deleted). (2) Anonymous volumes — a `VOLUME` instruction or `-v /path` with no name — one per
recreation; `prune` deletes every unreferenced volume, including one you meant to keep.
(3) `-v sqldata:/var/opt/mssql`; without it the `.mdf`/`.ldf` files sit in the writable layer and
vanish on `docker rm`. (4) The container is running; `docker stop sql` then `docker rm sql`, or
`docker rm -f sql`. (5) Layers are stored by digest; the base runtime layers were pushed by an
earlier build, so only the changed application layer travels.

## Resources
- [docker container run (Docker CLI reference)](https://docs.docker.com/reference/cli/docker/container/run/)
- [Docker CLI base command and management commands](https://docs.docker.com/reference/cli/docker/)
- [Volumes (Docker storage)](https://docs.docker.com/engine/storage/volumes/)
- [Bind mounts (Docker storage)](https://docs.docker.com/engine/storage/bind-mounts/)
- [Run SQL Server Linux container images with Docker (Microsoft Learn)](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/quickstart-install-docker)
- [Azure Container Registry authentication options (Microsoft Learn)](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-authentication)
