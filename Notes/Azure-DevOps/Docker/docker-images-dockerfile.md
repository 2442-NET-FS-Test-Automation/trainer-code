# Docker Images and the Dockerfile

## Learning Objectives
- Describe an image as a content-addressed stack of read-only layers; explain tags vs digests,
  why `latest` is only a tag, and how image names encode the registry.
- Read and write a Dockerfile: FROM (with multi-stage `AS`), WORKDIR, COPY vs ADD, RUN, ENV,
  ARG, EXPOSE, CMD vs ENTRYPOINT, VOLUME, USER, LABEL, HEALTHCHECK, and `.dockerignore`.
- Build an image with `docker build`, explain the build context and top-down layer caching, and
  recognize classic-builder and BuildKit output.
- Write a multi-stage Dockerfile for an ASP.NET Core Web API from memory, including the
  8080 / non-root defaults of current .NET images.
- List and justify image best practices (small bases, cache ordering, multi-stage,
  `.dockerignore`, one process, pinned tags, no baked secrets, non-root).
- Separate what an image declares from what a run injects, and explain how ASP.NET Core reads
  configuration from environment variables in a container.

## Why This Matters
The Dockerfile turns "my code" into "a thing that runs anywhere," and every backend engineer is
expected to write and review one. Interviewers ask "CMD vs ENTRYPOINT?", "why multi-stage?",
and "why did your image get bigger after you deleted a file?" because the answers reveal
whether the candidate understands layers or copies a template. The second half of this note —
image configuration versus run configuration — is where real container bugs live: an API that
works with `dotnet run` and returns 500s in a container almost always has a
configuration-source problem (`appsettings.Development.json` not loaded, a connection string
pointing at `localhost`, a port nobody published). Diagnosing that in five minutes is a job
skill.

## The Concept

### Images: layers, tags, digests, names
An image is a stack of **read-only filesystem layers** plus a small JSON config (default
command, environment, exposed ports, working directory, user). Each layer is a tarball of
filesystem *changes* relative to the layer below, identified by the SHA-256 of its content —
**content-addressed**, so identical layers are stored once and shared (every image built
`FROM mcr.microsoft.com/dotnet/aspnet:10.0` on a machine shares that base's layers). Layers are
immutable: deleting a file in a later layer adds a "whiteout" marker but the bytes stay in the
earlier layer, which is why `RUN apt-get install ... && rm -rf /var/lib/apt/lists/*` must be
*one* instruction to save space.

```console
$ docker image ls
REPOSITORY                        TAG       IMAGE ID       CREATED        SIZE
orders-api                        1.4.2     3f9c1a7d2b1e   2 hours ago    221MB
orders-api                        latest    3f9c1a7d2b1e   2 hours ago    221MB
mcr.microsoft.com/dotnet/aspnet   10.0      8a1b2c3d4e5f   3 weeks ago    218MB

$ docker history orders-api:1.4.2   # one row per layer: size + the instruction that made it
$ docker inspect orders-api:1.4.2   # the config JSON: Env, ExposedPorts, Entrypoint, Cmd, User
```

**Names.** Full form `<registry-host>[:port]/<repository>[:<tag>|@<digest>]`: `orders-api:1.4.2`
has no host, so Docker Hub is implied (and single-segment Hub names like `nginx` mean
`docker.io/library/nginx`); `myacr.azurecr.io/orders-api:1.4.2` is Azure Container Registry;
`mcr.microsoft.com/dotnet/aspnet:10.0` is Microsoft's registry.

**Tags vs digests.** A tag is a *mutable pointer* — `aspnet:10.0` moves every time Microsoft
publishes a patch. A digest (`aspnet@sha256:3fcf6f...`) is the *immutable* content hash and
names exactly one image forever. **`latest` is just a tag**: it is what `docker build -t name`
applies when you give no tag and what `docker pull name` fetches — not "newest," but "whatever
was last pushed as `latest`." Production pins a version tag at minimum and a digest where
supply-chain integrity matters.

### The Dockerfile: infrastructure as code for the image
A Dockerfile is a text file of instructions the builder executes top to bottom, each producing a
layer (or metadata). It is checked in next to the code and reviewed like code. The keywords:

| Keyword | What it does | Example |
|---|---|---|
| `FROM` | Base image; starts a stage. `AS name` names the stage. | `FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build` |
| `WORKDIR` | Working directory for following instructions (created if absent); prefer over `RUN cd`. | `WORKDIR /app` |
| `COPY` | Copies files from the build context (or `--from=stage`) into the image. Plain, predictable. | `COPY --from=build /app/publish .` |
| `ADD` | COPY plus auto-extract of local tars and URL fetch. Use COPY unless you need those. | `ADD vendor.tar.gz /opt/vendor/` |
| `RUN` | Executes a command at build time in a new layer. | `RUN dotnet restore` |
| `ENV` | Environment variable baked into the image; visible at build and run time. | `ENV ASPNETCORE_HTTP_PORTS=8080` |
| `ARG` | Build-time variable set with `--build-arg`; absent in the running container. | `ARG VERSION=1.0.0` |
| `EXPOSE` | Documents the port the process listens on. Does not publish — `-p` does. | `EXPOSE 8080` |
| `CMD` | Default command or default args to ENTRYPOINT; overridden by args after the image name. | `CMD ["--urls","http://+:8080"]` |
| `ENTRYPOINT` | The executable the container always runs; run args are appended. Override with `--entrypoint`. | `ENTRYPOINT ["dotnet","Orders.Api.dll"]` |
| `VOLUME` | Declares a persistent mount point; an anonymous volume is created if none is supplied. | `VOLUME /var/lib/postgresql/data` |
| `USER` | Switches user for following RUN/CMD/ENTRYPOINT — the non-root switch. | `USER app` |
| `LABEL` | Key/value metadata, read by `docker inspect`. | `LABEL org.opencontainers.image.source="https://github.com/acme/orders"` |
| `HEALTHCHECK` | Command the daemon runs periodically; sets the health status. | `HEALTHCHECK CMD curl -f http://localhost:8080/health \|\| exit 1` |

**CMD vs ENTRYPOINT, exec vs shell form.** Both accept two forms. *Exec form*
(`["dotnet","App.dll"]`, a JSON array) runs the binary directly as PID 1 — SIGTERM from
`docker stop` reaches it, and there is no shell to expand `$VAR`. *Shell form* (`dotnet
App.dll`) runs `/bin/sh -c "..."` — the shell is PID 1, your process is its child, and
`docker stop` waits the full timeout then SIGKILLs. Use exec form for the main process. The
two instructions combine: ENTRYPOINT is the fixed executable, CMD supplies default arguments
that `docker run image <args>` replaces — `ENTRYPOINT ["dotnet","App.dll"]` + `CMD
["--verbose"]` runs `dotnet App.dll --verbose` by default and `dotnet App.dll --quiet` on
`docker run image --quiet`. Only the last CMD and last ENTRYPOINT in a file count.

**`.dockerignore`** uses `.gitignore` syntax at the context root and keeps `bin/`, `obj/`,
`.git/`, `node_modules/`, and secrets out of the build context — smaller uploads to the daemon,
fewer cache busts, and no accidental `COPY . .` of a local secrets file:

```gitignore
bin/
obj/
.git/
**/*.user
```

### Building an image
```console
$ docker build -t orders-api:1.4.2 -f src/Orders.Api/Dockerfile .
```

- `-t name:tag` tags the result (repeatable). `-f path` names the Dockerfile (default
  `./Dockerfile`).
- The final `.` is the **build context**: the directory tree sent to the daemon. `COPY` paths
  are relative to it and cannot reach outside it — `COPY ../shared .` fails. For a solution
  with project references, the context is the solution root and `-f` points into the project.

**Layer caching.** The builder walks instructions top-down and reuses the cached layer when the
instruction text is unchanged *and* (for COPY/ADD) the copied files' content hashes are
unchanged. **The first instruction whose inputs changed invalidates every layer after it.**
That one rule drives ordering: rare-change things (base image, package restore) above
frequent-change things (source). Copying `*.csproj` and running `dotnet restore` before
copying the rest of the source means a code edit re-runs publish but not restore.

**Two output formats exist in the wild.** The classic builder prints `Step 4/10 : RUN dotnet
restore` / `---> Using cache`; **BuildKit** (default since Docker 23) prints numbered
`[build 4/6]` steps with `CACHED` markers and runs independent stages in parallel. Same
Dockerfile; recognize both.

```console
 => [build 2/6] WORKDIR /src                                                    0.0s
 => CACHED [build 3/6] COPY [Orders.Api.csproj, ./]                             0.0s
 => CACHED [build 4/6] RUN dotnet restore                                        0.0s
 => [build 5/6] COPY . .                                                        0.1s
 => [build 6/6] RUN dotnet publish -c Release -o /app/publish --no-restore     14.2s
 => [runtime 3/3] COPY --from=build /app/publish .                              0.2s
 => => naming to docker.io/library/orders-api:1.4.2
```

### A complete multi-stage Dockerfile for an ASP.NET Core Web API
The SDK image (~800 MB, compilers and NuGet) is what you *build* with; the aspnet image
(~220 MB, runtime only) is what you *run*. Multi-stage builds use both in one file and ship only
the second.

```dockerfile
# ---------- build stage: has the SDK, never shipped ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. copy only the project file and restore -> this layer caches until the csproj changes
COPY Orders.Api.csproj ./
RUN dotnet restore

# 2. now copy the source and publish -> re-runs on every code change, restore does not
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---------- runtime stage: runtime only, this is the image you ship ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# the aspnet image already listens on 8080 (ASPNETCORE_HTTP_PORTS=8080); EXPOSE documents it
EXPOSE 8080

# opt into the non-root user that ships in the .NET 8+ Linux images
USER app

ENTRYPOINT ["dotnet", "Orders.Api.dll"]
```

```console
$ docker build -t orders-api:1.4.2 .
$ docker run --rm -p 5000:8080 -e ConnectionStrings__Default="Server=host.docker.internal,1433;..." orders-api:1.4.2
$ curl -s http://localhost:5000/api/orders | head -c 80
```

The .NET-specific facts:

- **8080, not 80.** Since .NET 8 the `aspnet` image sets `ASPNETCORE_HTTP_PORTS=8080`, so the
  app listens on 8080 with no code change. `-p 5000:80` therefore connects to nothing — the
  most common "it built but I can't reach it" mistake. Override with `-e
  ASPNETCORE_HTTP_PORTS=80` (or `ASPNETCORE_URLS`) if a platform demands another port.
- **Non-root `app` user.** The .NET 8+ Linux images ship a non-root user named `app`; the port
  moved to 8080 precisely so a non-root process can bind it (ports below 1024 are privileged).
  Images still start as root unless you add `USER app` — the switch is yours, and once made the
  process cannot write outside directories `app` owns.
- **`--no-restore`** on publish trusts the earlier restore layer.
- The SDK can also produce an image with no Dockerfile (`dotnet publish /t:PublishContainer`)
  — awareness depth; the Dockerfile remains what most teams and CI systems use.

### Best practices
1. **Small base images.** Runtime, not SDK, in the final stage; `-alpine` variants
   (musl-based, roughly half the size) where native dependencies allow; and, at awareness
   depth, Ubuntu **chiseled** images (`aspnet:10.0-noble-chiseled`, no shell or package manager
   — Microsoft's distroless equivalent) for the smallest attack surface.
2. **Order for the cache**: rare-change instructions first (FROM, restore); source copy and
   publish last.
3. **Multi-stage always** for compiled languages: build tools never ship.
4. **`.dockerignore`** every time — `bin/`, `obj/`, `.git/`, secrets.
5. **One process per container.** An API and its database are two containers on a shared
   network, not one image with a supervisor.
6. **Pin tags** (`aspnet:10.0`, never bare `aspnet` or `latest`), pin digests for production,
   and rebuild regularly to pick up base-image patches.
7. **Never bake secrets or environment config** into the image — no connection strings in
   `ENV`, no production settings file with passwords copied in. Layers are permanent and
   `docker history` shows them to anyone who pulls the image.
8. **Run as non-root** (`USER app`); add `--read-only` where the app allows.
9. **Inspect size** with `docker history <image>` — it shows which instruction produced which
   MB and finds the layer that copied `bin/Debug` by mistake.
10. **Health checks and labels** so orchestrators and humans can tell what an image is and
    whether it is up.

### Image configuration vs run configuration
An image *declares* defaults; a run *injects* the environment. Keep the split straight:

| Image declares (Dockerfile) | Run injects (`docker run` / compose / orchestrator) |
|---|---|
| `EXPOSE 8080` — the port the process listens on | `-p 5000:8080` — publish host 5000 to container 8080 |
| `ENV ASPNETCORE_HTTP_PORTS=8080` — a default env var | `-e ConnectionStrings__Default=...`, `--env-file .env` — environment-specific values |
| `ENTRYPOINT` / `CMD` — the process and default args | trailing args or `--entrypoint` — override for one run |
| `VOLUME /data` — "this path should persist" | `-v orders-data:/data` — which volume actually mounts there |
| (nothing) | `--name orders-api`, `--restart unless-stopped`, `--network backend`, `--memory 512m` |

The principle behind the split is the twelve-factor rule **configuration comes from the
environment**: the same image is promoted from dev to staging to production, and only the
injected environment differs. Anything that varies per environment (connection strings, API
keys, external URLs, log levels) is *never* in the image.

**How ASP.NET Core reads it.** The default host builder loads configuration in order —
`appsettings.json`, `appsettings.{Environment}.json`, user secrets (Development only),
**environment variables**, command-line args — later sources overriding earlier. Environment
variables map onto hierarchical keys with a **double underscore `__` as the section separator**
(`:` is not legal in variable names on Linux):

```console
$ docker run -e ConnectionStrings__Default="Server=db,1433;Database=Orders;User Id=sa;Password=...;TrustServerCertificate=True" \
             -e Logging__LogLevel__Default=Warning \
             -p 5000:8080 orders-api:1.4.2
```

Inside the app those arrive as `Configuration["ConnectionStrings:Default"]` and
`Logging:LogLevel:Default` — the same keys the JSON files use, so code does not change between
`dotnet run` and the container.

**Why `appsettings.Development.json` is not loaded in a container.** `dotnet run` sets
`ASPNETCORE_ENVIRONMENT=Development` from `launchSettings.json`; a container never reads that
file, so **`ASPNETCORE_ENVIRONMENT` defaults to `Production`** — the Development JSON and user
secrets are skipped, the developer exception page is off, Swagger UI is usually off. Symptoms:
a 500 with a missing-connection-string error, or a Swagger URL that 404s. The fix is not to
force Development in production but to supply values through the environment
(`-e ConnectionStrings__Default=...`), and, for local container runs only,
`-e ASPNETCORE_ENVIRONMENT=Development` when you genuinely want the dev profile.

Recognize-on-sight: `-e` for one value, `--env-file` for a file of `KEY=value` lines kept out
of source control, and in Kubernetes the same values arrive through ConfigMaps and Secrets — the mechanism
changes, the `Section__Key` mapping does not.

## Say It in an Interview
- **"What is a Docker image; tag vs digest?"** — "An immutable stack of content-addressed,
  read-only layers plus a config JSON. A tag is a mutable pointer like `10.0` or `latest`; a
  digest is the SHA-256 of the content and names exactly one image forever. `latest` is just a
  tag with no special meaning."
- **"CMD vs ENTRYPOINT?"** — "ENTRYPOINT is the executable the container always runs; CMD is
  the default arguments, replaced by anything after the image name in `docker run`. Use exec
  form so the process is PID 1 and receives SIGTERM from `docker stop`."
- **"How does the build cache work?"** — "Layers are reused top-down until the first
  instruction whose inputs changed; everything after that rebuilds. So I copy the csproj and
  restore before copying the source, and keep a `.dockerignore` so `bin/` and `obj/` don't bust
  the cache."
- **"Why multi-stage, and what does your .NET Dockerfile look like?"** — "Build with the large
  SDK image and copy only the publish output into the small aspnet runtime image: restore from
  the csproj first for caching, publish in Release, `COPY --from=build`, `EXPOSE 8080` because
  the aspnet image listens on 8080 since .NET 8, `USER app` for non-root, and an exec-form
  `ENTRYPOINT ["dotnet","App.dll"]`."
- **"Name some image best practices."** — "Small runtime bases — alpine or chiseled —
  multi-stage, cache-friendly ordering, `.dockerignore`, one process per container, pinned tags
  or digests, no secrets or environment config baked in, non-root, and `docker history` to see
  where the size went."
- **"How do you configure a containerized ASP.NET Core app per environment?"** — "The image
  declares defaults — EXPOSE, ENV, ENTRYPOINT — and the run injects the environment: `-p`,
  `-e`, `--env-file`, volumes. ASP.NET Core reads env vars with double underscore as the section
  separator, so `ConnectionStrings__Default` becomes `ConnectionStrings:Default`. In a container
  `ASPNETCORE_ENVIRONMENT` defaults to Production, so `appsettings.Development.json` is not
  loaded — values must come from the environment."

## Check Yourself
1. You delete a 300 MB SDK folder in a later `RUN` instruction, but the image is still 300 MB
   larger. Why?
   *Layers are immutable — the delete adds a whiteout marker in a new layer, but the bytes stay
   in the earlier layer. Delete in the same RUN that created it, or use a multi-stage build.*
2. Your Dockerfile has `EXPOSE 8080` but `curl localhost:8080` on the host fails. What is
   missing?
   *`EXPOSE` only documents; the run must publish with `-p 8080:8080` (host:container).*
3. `docker run myapi` starts, but the app logs "connection string 'Default' not found" and you
   know it is in `appsettings.Development.json`. What happened, and what is the right fix?
   *`ASPNETCORE_ENVIRONMENT` defaults to Production in the container, so the Development file
   is not loaded. Supply it through the environment: `-e ConnectionStrings__Default=...`.*
4. You changed one line of C# and `docker build` re-ran `dotnet restore`. What is wrong with
   the Dockerfile ordering?
   *The source is copied before restore, so a source change invalidates the restore layer.
   Copy the csproj, restore, then copy the rest of the source.*
5. Two `docker run` commands: `-p 5000:80 orders-api` and `-p 5000:8080 orders-api`, both
   built from `aspnet:10.0`. Which works, and why?
   *The second — the .NET 8+ aspnet image sets `ASPNETCORE_HTTP_PORTS=8080`, so the process
   listens on 8080; nothing listens on 80.*

## Resources
- [Dockerfile reference: every instruction, exec vs shell form (Docker Docs)](https://docs.docker.com/reference/dockerfile/)
- [Multi-stage builds (Docker Docs)](https://docs.docker.com/build/building/multi-stage/)
- [Building best practices (Docker Docs)](https://docs.docker.com/build/building/best-practices/)
- [Run an ASP.NET Core app in Docker containers: the sdk/aspnet multi-stage Dockerfile (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images)
- [Breaking change: default ASP.NET Core container port changed from 80 to 8080 (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/core/compatibility/containers/8.0/aspnet-port)
- [Configuration in ASP.NET Core: providers, environment variables, `__` separator (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
