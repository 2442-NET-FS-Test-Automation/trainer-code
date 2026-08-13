# Azure Core Services: VMs, Disks, Blob, SQL, App Service

## Learning Objectives
- Place the five workhorse services — Virtual Machines, Managed Disks, Blob Storage, Azure SQL
  Database, App Service — on the IaaS/PaaS boundary and justify each placement.
- Describe when a team reaches for each service, and the standard pairings (VM + disks;
  App Service + Azure SQL; static frontend + Blob).
- Explain VM sizing, the difference between stopping and deallocating, and what an ephemeral OS
  disk actually loses (and when).
- Distinguish the storage duty of Managed Disks (block storage for one VM) from Blob Storage
  (shared object storage over HTTP).
- Recognize the service-selection question — "VM or App Service?" — as the IaaS/PaaS
  responsibility question in disguise.

## Why This Matters
"Tell me about the Azure services you've used" is the most common cloud screen question, and it
is really a taxonomy question: interviewers listen for whether you know WHAT KIND of thing each
service is and when you'd choose it, not for feature trivia. These five services cover the
majority of real workloads — most line-of-business systems are some arrangement of "compute +
files + database + web hosting" — so being able to sketch that arrangement, and defend why the
compute is PaaS but the legacy piece is on a VM, is exactly working-engineer conversation.

## The Concept

### The map, up front

| Service | Kind | Layer | One-line duty |
|---|---|---|---|
| Virtual Machines | Compute | IaaS | A rented computer; you own the OS up |
| Managed Disks | Storage | IaaS | The VM's virtual hard drives (block storage) |
| Blob Storage | Storage | PaaS | Objects/files over HTTP at any scale |
| Azure SQL Database | Data | PaaS | Managed SQL Server engine, no server to run |
| App Service | Compute | PaaS | Managed web hosting: push code, platform runs it |

The pattern: the IaaS pair gives you maximum control and the patching burden; the PaaS trio is
"bring your data/code, the platform does the rest". Most modern designs are PaaS-first and drop
to IaaS only for what genuinely needs an OS of its own.

### Virtual Machines
An Azure VM is a computer you rent by the second: pick a **size** (a named bundle of
vCPU/RAM/disk throughput — families exist for general purpose, compute-heavy, memory-heavy,
GPU) and an **image** (Windows Server, Ubuntu, or your own captured image), and you get a box
you can RDP/SSH into. It is the definition of IaaS: total control, and every patch, agent, and
hardening step from the OS up is yours.

When a VM is right: lift-and-shift of legacy systems that cannot be re-platformed, software
needing OS-level installs or custom drivers, build agents, anything the PaaS platforms refuse
to run.

Billing detail with interview mileage: **stopped is not free**. A VM stopped from inside the
OS still reserves its host capacity and bills for compute; only a **deallocated** VM (stopped
via Azure — portal or `az vm deallocate`) releases the hardware and stops the compute meter.
Disks bill either way, because they persist either way.

Related vocabulary worth recognizing: **scale sets** (identical VMs behind autoscale rules —
the IaaS way to scale out), **spot VMs** (spare capacity at a discount, evictable at any time —
covered under pricing in the fundamentals note).

### Managed Disks
The virtual hard drives behind VMs — **block storage**, attachable to one VM (OS disk + data
disks). "Managed" means Azure handles the underlying storage placement, replication, and
resilience; you pick a size and a performance tier: **Standard HDD** (dev/test), **Standard
SSD** (light production), **Premium SSD** (production databases and anything
latency-sensitive), **Ultra** (extreme IOPS). You can snapshot a disk, create disks from
snapshots, and resize.

One precise fact that gets garbled often — **ephemeral OS disks**: an option where the OS disk
lives on the VM host's local storage (fast, free) instead of durable storage. Its contents
survive a plain restart, but are **lost on deallocation or reimage** — which is why the option
fits only stateless, image-rebuilt workloads (scale-set web tiers, build agents), never
anything that keeps state on the OS disk.

Disks vs Blob in one line: a disk is one VM's private drive (block storage, mounted); Blob is
shared object storage reached over HTTP by anything with a URL and credentials.

### Blob Storage
Azure's **object store** (the S3 equivalent): you store immutable objects ("blobs") in
**containers** inside a **storage account**, and address every object by URL. It scales to
petabytes, replicates per your redundancy choice (LRS/ZRS/GRS — detailed in the storage note),
and prices per GB-month plus operations, with **access tiers** (hot / cool / archive) trading
storage price against retrieval price and latency.

What teams put in it: user uploads, images and media, log and backup archives, data-lake raw
files, and **static websites** — a container of HTML/CSS/JS served directly over HTTP, the
cheapest possible hosting for a frontend that is just files (the deep dive and the hands-on
walk live in the storage-and-SQL note).

What it is NOT: a file system for a running OS (that is a disk) and not a database (no queries
over content — you fetch objects by name). The moment you want to SELECT rows, you want the
next service.

### Azure SQL Database
The SQL Server engine as a managed service: you get a database with a connection string;
Microsoft runs the underlying servers, patches the engine, takes continuous backups with
point-in-time restore, and offers built-in high availability. Your application code — ADO.NET,
Entity Framework Core, any SQL Server client — connects exactly as it would to a self-hosted
SQL Server; from the code's perspective the swap is a connection-string change.

What you give up vs SQL Server on a VM: OS access, cross-database features, SQL Agent, full
instance control. That trade has its own middle option — **SQL Managed Instance**, a managed
near-full SQL Server instance for migrations that need instance-level features. The decision
ladder in an interview: SQL on a VM (full control, full burden) → Managed Instance
(instance-compatible, managed) → SQL Database (single database, most managed, the default for
new applications). Provisioning, firewalls, tiers, and monitoring are the storage-and-SQL
note's territory.

### App Service
Managed hosting for web applications and HTTP APIs: you deploy code (or a container), the
platform provides the web server, TLS, OS and runtime patching, scaling, custom domains, and
deployment plumbing. Apps run on an **App Service plan** — the actual rented compute, with
tiers from Free (great for exercises; sleeps when idle, minutes-per-day CPU quota) through
Basic/Standard/Premium (always-on, scale-out, deployment slots at Standard+).

It is the natural home for a .NET (or Node, Python, Java) web API: push a published build, get
`https://yourapp.azurewebsites.net`, wire configuration through app settings (which surface to
the app as environment variables), and turn on autoscale at the tiers that support it. The
full deployment story — configuration, managed identity to Key Vault, Application Insights,
log streaming — is the App-Services-and-observability note.

"VM or App Service?" is the IaaS/PaaS question wearing a costume: if the workload is a web app
whose requirements fit the platform (supported runtime, no OS-level installs), App Service
means never patching an OS again; the VM is the fallback for everything the platform cannot
express.

### The standard arrangement
A very large share of real systems is exactly this sketch — worth being able to draw:

> **Browser** → static frontend from **Blob Storage** (or the frontend served by the app) →
> HTTP API on **App Service** → **Azure SQL Database**; secrets in Key Vault via managed
> identity; telemetry to Application Insights; the odd legacy service on a **VM** with
> **Premium SSD disks**, reached over the virtual network.

Each arrow is a service-selection decision you can now defend layer by layer.

## Say It in an Interview
- **"Which Azure services have you worked with?"** — "The core web stack: App Service hosting
  an ASP.NET API, Azure SQL Database behind it, Blob Storage for files and for static frontend
  hosting, and VMs with managed disks where something genuinely needs OS control. I default to
  the PaaS side and treat a VM as the exception that must justify its patching burden."
- **"VM or App Service for a new web API?"** — "App Service, unless the app needs something
  the platform can't give — OS installs, custom runtimes, exotic networking. That's the
  IaaS/PaaS trade: the VM gives control and costs you every patch window; App Service takes
  the OS off my plate."
- **"Blob Storage vs a managed disk?"** — "A disk is block storage — one VM's private drive.
  Blob is object storage — files at HTTP URLs, shared, effectively unlimited, tiered by access
  frequency. Backups and uploads go to Blob; a database's data files live on a disk."
- **"Is a stopped VM free?"** — "Only if it's deallocated — stopped through Azure so the
  hardware is released. Shut down from inside the OS it still bills compute. Disks bill in
  both cases."
- **"Why Azure SQL Database over SQL Server on a VM?"** — "Same engine, no server to run:
  managed patching, continuous backups with point-in-time restore, built-in HA, and to the app
  it's just a connection string. I'd only take the VM for instance-level features — and I'd
  consider Managed Instance before that."

## Check Yourself
1. For each, IaaS or PaaS, and the one-word reason: Managed Disks, Azure SQL Database,
   App Service.
2. A VM with an ephemeral OS disk is restarted; then later deallocated. What happened to the
   OS disk contents at each step?
3. Where do these live: (a) nightly database backups kept 7 years, (b) the running database's
   data files, (c) a React build output served to browsers?
4. Your team shut down a reporting VM Friday from inside Windows. Monday's bill still shows
   compute charges. Why?
5. Sketch the standard PaaS web arrangement for an order-management system, naming a service
   per layer.

**Answers:** (1) IaaS (it's the VM's hardware layer), PaaS (managed engine), PaaS (managed
platform — you bring only code). (2) Restart: contents intact. Deallocation: contents gone —
ephemeral disks live on the host's local storage, and deallocation or reimage releases it.
(3) (a) Blob, archive/cool tier; (b) a managed disk (Premium SSD in production); (c) Blob
static website (or the app's own hosting). (4) OS shutdown is not deallocation — the host is
still reserved; deallocate via Azure to stop the compute meter. (5) Browser → Blob-hosted
frontend → App Service API → Azure SQL Database, Key Vault + managed identity for secrets,
Application Insights for telemetry.

## Resources
- [Azure Virtual Machines overview](https://learn.microsoft.com/en-us/azure/virtual-machines/overview)
- [Introduction to Azure Blob Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-introduction)
- [What is Azure SQL Database?](https://learn.microsoft.com/en-us/azure/azure-sql/database/sql-database-paas-overview)
- [App Service overview](https://learn.microsoft.com/en-us/azure/app-service/overview)
