# Docker Fundamentals: Containers, Images, and the Engine

## Learning Objectives
- Define a container — an isolated process on a shared kernel, built from namespaces and
  cgroups — and explain why "a small VM" is the wrong mental model.
- Compare containers with VMs on kernel sharing, size, startup, isolation, and density.
- Explain containerization: why shipping the environment with the app fixes "works on my
  machine" and gives dev / CI / prod parity.
- Define Docker, place it in history (2013, OCI), and name Podman, containerd, and runc.
- Draw Docker's architecture: `docker` CLI → REST API → `dockerd` → containerd/runc, managing
  images, containers, volumes, networks, and registries.
- Install Docker on Windows/macOS (Docker Desktop over WSL2) and Linux (Docker Engine), verify
  with `docker version` and `docker run hello-world`, and state the Desktop licensing rule.
- Describe the daemon: what `dockerd` does, why `/var/run/docker.sock` is root-equivalent, and
  what restart policies are for.

## Why This Matters
Containers are the deployment unit of modern backend work: a .NET Web API today ships as an
image to a registry and runs under Kubernetes, Azure Container Apps, or App Service far more
often than it is copied to a server as a folder of DLLs. Interviewers use Docker questions as a
proxy for "has this person shipped software" — "container vs VM?" and "what happens when you
type `docker run`?" are near-universal openers, and "a container is a lightweight VM" is a
wrong answer the interviewer will notice. Understanding the daemon and its socket is also a
security fundamental: the most common container misconfiguration is handing a build agent the
Docker socket without realizing that is handing it root on the host.

## The Concept

### What a container actually is
A container is an ordinary process (or small process tree) running on the host's kernel with a
restricted view of the world. Two kernel features make that view possible:

- **Namespaces** control what the process can *see*: its own process IDs (its main process is
  PID 1), its own network stack, its own mount table and root filesystem, its own hostname,
  users, and IPC.
- **Control groups (cgroups)** control what it can *use*: CPU shares, memory limits, I/O, PID
  counts.

Add a root filesystem unpacked from an image and the process believes it is alone on a fresh
machine — but there is exactly one kernel, the host's, and `ps` on the host shows the
container's processes as plain processes. Nothing boots, no second OS runs; that is why
containers start in milliseconds and cost megabytes. Everything else in this note — the
container dying with its main process, `localhost` meaning the container, the disposable
filesystem — follows from "it is just a process" (see the mental model at the end).

### Containers vs virtual machines
A VM virtualizes hardware: a hypervisor presents virtual CPUs and disks, and a full guest OS
with its own kernel boots on top. A container virtualizes the OS: many containers share one
kernel and differ only in what the kernel lets them see and use.

| Dimension | Virtual machine | Container |
|---|---|---|
| Kernel | Own guest kernel per VM | Shares the host kernel |
| Size | GBs (full OS) | MBs to a few hundred MB (app + libs) |
| Startup | Tens of seconds to minutes (OS boot) | Milliseconds to seconds (process start) |
| Isolation | Strong — hardware boundary | Weaker — kernel boundary; a kernel exploit escapes to the host |
| Density | Tens per host | Hundreds to thousands per host |

They are complementary, not rivals: cloud providers run your containers *inside* VMs, and
Docker Desktop runs a Linux VM to host Linux containers. "Weaker isolation" is a real
trade-off: untrusted multi-tenant workloads belong in VMs or in sandboxed runtimes such as
gVisor or Kata (a VM-grade boundary under a container interface — name-and-one-line depth).

### Why containerize
The problem containers solve is **environment drift**. An app depends on a runtime version,
native libraries, environment variables, and OS packages; when those live on the machine
rather than with the app, every machine (each laptop, the CI agent, staging, production) drifts
and bugs appear that reproduce nowhere else — "works on my machine." Containerization moves the
environment into the artifact: the image carries the exact runtime and libraries, so the bytes
that passed CI are the bytes that run in production. The wins: **parity** across dev/CI/prod
(one image promoted through environments), **reproducibility** (the Dockerfile is a
code-reviewed recipe), **density** (many services per host, started in seconds — the basis of
autoscaling and affordable microservices), and **dependency isolation** (an order service on
.NET 10 beside a legacy job on .NET 6, no fights over global installs). The cost: another layer
to operate (builds, registries, orchestration), disk for images, and deliberate handling for
state, because a container's own disk is disposable.

### Docker: definition, history, standards, alternatives
**Docker** is a platform for building, shipping, and running containers: a daemon that runs
them, a CLI to drive it, an image format and Dockerfile language to build them, and a registry
(Docker Hub) to distribute them. Docker did not invent containers — the kernel features and
FreeBSD jails predate it — but Docker (open-sourced by dotCloud in 2013) made them usable by
putting the pieces behind one command and one portable image format. In 2015 Docker and others
founded the **Open Container Initiative (OCI)**, which standardized the image, runtime, and
distribution specs; because of OCI, a Docker-built image runs on any compliant runtime, and
Docker's own stack is open components:

- **runc** — the reference OCI runtime; the small binary that creates the namespaces and
  cgroups and starts the process.
- **containerd** — the supervisor above runc: pulls images, manages lifecycle, exposes an API.
  Docker uses it; Kubernetes talks to it directly (Kubernetes dropped its Docker shim in 2022 —
  Docker-built images still run there, because they are OCI images).
- **Podman** — Red Hat's daemonless, rootless-by-default drop-in for the `docker` CLI; popular
  where daemon-as-root is a policy problem.
- **BuildKit / Buildah / kaniko** — image builders; BuildKit is Docker's own modern builder,
  the other two need no privileged daemon.

### Architecture: client, daemon, objects
```text
docker CLI --REST API (unix socket / TCP)--> dockerd --> containerd --> runc --> container process
                                               |
                                    images, containers, volumes, networks
                                               |
                             registries (Docker Hub, ACR, GHCR, ...) via pull/push
```

- **Client** (`docker`): a thin CLI; every command is an HTTP request to the Engine API
  (`docker -H ssh://user@host ps` drives a remote daemon with the same CLI).
- **Daemon** (`dockerd`): the long-running service that owns everything — builds images,
  creates and supervises containers (via containerd and runc), manages storage, networks,
  volumes, and registry traffic.
- **Objects**: **image** (read-only layer stack + metadata; immutable; named by tag or
  digest), **container** (image + thin writable layer + one main process in its own
  namespaces/cgroups), **volume** (daemon-managed storage that survives `docker rm`; bind
  mounts `-v ./src:/app` map a host path instead), **network** (containers on the same
  user-defined bridge resolve each other by name over DNS), **registry** (stores and serves
  images — Docker Hub by default, Azure Container Registry, GitHub Container Registry;
  `docker pull` / `docker push` move them).

The lifecycle to narrate: `docker run nginx` → CLI asks daemon → daemon pulls `nginx:latest`
from Docker Hub if not cached → creates a container (writable layer, namespaces, cgroups,
network endpoint) → containerd/runc start the image's default command as PID 1 → daemon
streams logs and the exit code back.

### Docker Desktop on Windows and macOS is a Linux VM
Linux containers need a Linux kernel. Docker Desktop provides one by running a lightweight
Linux VM — on Windows a WSL2 distribution (`docker-desktop`), on macOS an Apple Virtualization
VM — with `dockerd` inside it; the host `docker` CLI talks to it through a forwarded socket.
Consequences: bind mounts cross a VM boundary (fast from the WSL2 filesystem, slow from `C:\`),
published ports are forwarded to the host's `localhost`, and a container that must reach a
service on the host machine (SQL Server, an API under `dotnet run`) uses the DNS name
**`host.docker.internal`** — because inside the container, `localhost` is the container.
Desktop can switch to Windows containers, but nearly all cloud and open-source images are
Linux, so Linux mode is assumed here.

### Installing Docker
**Windows / macOS — Docker Desktop.** Windows prerequisites: 64-bit Windows 10/11, hardware
virtualization enabled in firmware, **WSL2** installed (`wsl --install` from an elevated
terminal, reboot). Install Desktop, start it, wait for the whale icon to go steady, then verify:

```console
$ docker version
Client:
 Version:           28.x.x
 API version:       1.5x
 OS/Arch:           windows/amd64

Server: Docker Desktop
 Engine:
  Version:          28.x.x
  OS/Arch:          linux/amd64
```

The two halves matter: **Client** is the CLI on your OS; **Server** is the engine inside the
Linux VM (`linux/amd64`). If the Server half is missing or the command says "cannot connect to
the Docker daemon", the daemon is not running — start Docker Desktop. Then:

```console
$ docker run hello-world
Unable to find image 'hello-world:latest' locally
latest: Pulling from library/hello-world
...
Hello from Docker!
This message shows that your installation appears to be working correctly.
```

That one command exercises the whole architecture: CLI → daemon → registry pull → create →
run → exit; `docker ps -a` afterwards shows the exited container.

**Linux — Docker Engine.** Servers have no Desktop; install the engine packages from Docker's
apt repository (repository setup lines are on the docs page):

```console
$ sudo apt-get install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
$ sudo docker run hello-world
$ sudo usermod -aG docker $USER   # optional: docker without sudo -- see the daemon section for what this grants
```

**Licensing.** Docker Engine is open source (Apache 2.0) and free everywhere. **Docker
Desktop** is free for personal use, education, non-commercial open source, and small businesses
(fewer than 250 employees *and* under 10 million USD annual revenue); larger organizations need
a paid subscription. This is why some corporate laptops run Podman Desktop or Rancher Desktop
instead — expect "why not Docker Desktop at your company?".

### The daemon
`dockerd` runs as root (unless rootless mode is configured) and listens by default on a Unix
socket, **`/var/run/docker.sock`**. Anyone who can write to that socket can ask the daemon to
do anything it can do — including `docker run --privileged -v /:/host ...`, which mounts the
host's root filesystem into a container. Socket access is therefore **root-equivalent**: adding
a user to the `docker` group grants root, and mounting the socket into a container (common for
CI agents and tools like Portainer) gives that container the host. Never expose the daemon on
TCP without TLS client certificates. **Rootless mode** (daemon and containers in a user
namespace as an unprivileged user) is the mitigation to know by name. Daemon settings live in
`/etc/docker/daemon.json` (Desktop: Settings > Docker Engine).

Two operational facts: **restart policies** are per container and enforced by the daemon —
`--restart no` (default), `on-failure[:n]`, `always`, `unless-stopped`; `docker run -d
--restart unless-stopped --name orders-db postgres` comes back after a reboot, and without a
policy a reboot leaves it stopped (in production an orchestrator usually owns this). And
`docker logs <container>` reads what PID 1 wrote to stdout/stderr — containers log to stdout,
not files — while `docker system prune` reclaims the disk that images and stopped containers
silently accumulate.

### The mental model, in five lines
1. **Image** = read-only stack of layers + metadata. Immutable; lives in the daemon's cache
   and in registries.
2. **Container** = image + one thin writable layer + one main process, inside namespaces and
   cgroups.
3. The container **exits when its main process exits** — `dotnet App.dll` stays up because
   Kestrel keeps PID 1 alive; a script that finishes exits at once. Restart policies decide
   what happens next.
4. **`localhost` inside the container is the container** — a connection string to
   `localhost:1433` looks for SQL Server inside the same container. Reach the host with
   `host.docker.internal`; reach sibling containers by name on a shared network.
5. **State that must survive goes in a volume**; the writable layer vanishes with `docker rm`.

## Say It in an Interview
- **"What is a container?"** — "An isolated process on the host's kernel: namespaces restrict
  what it can see — PIDs, network, filesystem — and cgroups restrict what it can use. Not a
  small VM: no guest OS, no second kernel, so it starts in milliseconds and dies when its main
  process exits."
- **"Container vs VM?"** — "A VM virtualizes hardware and boots a guest OS with its own kernel
  — strong isolation, gigabytes, minutes. A container virtualizes the OS and shares the host
  kernel — weaker isolation, megabytes, milliseconds, far higher density. They stack: cloud
  containers run inside VMs, and Docker Desktop runs a Linux VM underneath."
- **"Why containerize?"** — "The environment ships with the app: the image carries the runtime
  and libraries, so dev, CI, and production run the same bytes and 'works on my machine' goes
  away. You also get density and fast start, which autoscaling and microservices are built on."
- **"What is Docker, and what are the alternatives?"** — "The platform — daemon, CLI,
  Dockerfile, image format, Docker Hub — that made containers mainstream in 2013. The formats
  are OCI standards now, so Docker-built images run on containerd, which Kubernetes uses
  directly, or on Podman, a daemonless, rootless drop-in for the CLI."
- **"Walk me through Docker's architecture."** — "The `docker` CLI calls a REST API on the
  `dockerd` daemon over a Unix socket. The daemon manages images, containers, volumes, and
  networks, pulls from and pushes to registries, and delegates running containers to containerd
  and runc."
- **"How do you install and verify Docker?"** — "Docker Desktop on Windows or macOS — WSL2 on
  Windows, a Linux VM underneath; the Docker Engine apt packages on Linux. `docker version`
  should show a Client and a Server half, and `docker run hello-world` proves the full
  pull-create-run path. Desktop is free for small businesses, personal use, and education, paid
  for larger companies."
- **"What is the Docker daemon, and why is its socket sensitive?"** — "`dockerd` is the
  root-running service that does the work; the CLI talks to it over `/var/run/docker.sock`.
  Whoever can write to that socket can start a privileged container with the host filesystem
  mounted, so socket access is root access — treat the docker group and socket-mounted CI
  agents that way. Rootless mode is the mitigation."

## Check Yourself
1. A teammate says "a container is basically a tiny VM." Which two kernel features correct
   them, and what does each do?
   *Namespaces (what the process sees: PIDs, network, mounts, hostname, users) and cgroups
   (what it uses: CPU, memory, I/O). No guest kernel is involved.*
2. Your API container starts and immediately exits with code 0. Most likely reason?
   *Its main process (PID 1) finished — a container lives only as long as its main process, so
   the command it runs is not a long-running server.*
3. An API in a Linux container on Docker Desktop for Windows needs SQL Server on the Windows
   host at 1433. Why does `localhost:1433` fail, and what do you use?
   *Inside the container `localhost` is the container's own network namespace (and the
   container is inside a Linux VM); use `host.docker.internal:1433`.*
4. A CI job mounts `/var/run/docker.sock` into its build container "so it can build images."
   What has it effectively been granted?
   *Root on the host — anyone with the socket can run a privileged container with the host
   filesystem mounted.*
5. `docker version` prints the Client block and then "Cannot connect to the Docker daemon."
   Which half is missing, and what does that mean?
   *The Server half — the CLI is installed but the engine (`dockerd`, on Windows inside the
   WSL2 VM) is not running; start Docker Desktop or the docker service.*

## Resources
- [Docker overview: architecture, daemon, client, registries (Docker Docs)](https://docs.docker.com/get-started/docker-overview/)
- [Install Docker Desktop on Windows: WSL2 prerequisites and licensing (Docker Docs)](https://docs.docker.com/desktop/setup/install/windows-install/)
- [Install Docker Engine on Ubuntu (Docker Docs)](https://docs.docker.com/engine/install/ubuntu/)
- [Docker Engine security: daemon attack surface and rootless mode (Docker Docs)](https://docs.docker.com/engine/security/)
- [Run an ASP.NET Core app in Docker containers (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images)
