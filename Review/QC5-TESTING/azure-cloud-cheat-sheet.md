# Azure and Cloud Fundamentals — Interview Cheat Sheet

These topics sit on the schedule **after** this exam's material window and are **not QC-6 scope** —
no rubric row anywhere touches cloud. They are here for one reason: interviews sometimes range
past the syllabus, and an Azure question should not be the first time you have seen the words.

How to use it: read the **Quick answers** table until each line is yours, then read the block
behind any line you could not say out loud. Every block ends with the trap — the wrong answer a
panelist is listening for. `Say It in an Interview` is the spoken form; practise those aloud.

---

## Quick Answers

| Term | One line |
|---|---|
| Cloud computing | Renting compute, storage, and networking on demand over the internet, billed by use, someone else owning the hardware |
| IaaS | You rent the machine; you own everything from the OS up |
| PaaS | You rent the runtime; you own the app and data only |
| SaaS | You rent finished software; you own your data and your users |
| Major providers | AWS (largest), Azure (enterprise/Microsoft-stack strength), GCP (data and Kubernetes strength) |
| Well-Architected Framework | Five pillars: reliability, security, cost optimization, operational excellence, performance efficiency |
| Azure pricing models | Pay-as-you-go, reservations, savings plans, spot, Hybrid Benefit, free tier |
| Region | A set of datacenters in one geography that you deploy into |
| Availability zone | A physically separate datacenter within a region — protects against a datacenter failure |
| Azure Advisor | Free recommendation engine over your own resources: cost, security, reliability, performance, operational excellence |
| Azure Monitor | The platform-wide telemetry service: metrics, logs, alerts; Application Insights is its APM layer |
| Cost Management | Cost analysis, budgets, alerts, and exports — budgets **notify**, they do not stop spend |
| Portal / CLI / PowerShell | Same ARM API three ways: GUI for discovery, `az` for scripts, `Az` module for object pipelines |
| Network security group | A stateful allow/deny list of rules on a subnet or NIC, evaluated by priority, first match wins |
| Microsoft Entra ID | Azure's cloud identity provider (formerly Azure AD) — authentication, not a domain controller |
| RBAC | Authorization: a role assignment is a security principal plus a role definition plus a scope |
| Key Vault | Managed store for secrets, keys, and certificates, reached with a managed identity instead of a password |
| Azure SQL Database | Fully managed PaaS SQL Server database engine — no OS, no patching, no SQL Agent |
| Blob Storage | Object storage for unstructured data: account, container, blob, with hot/cool/cold/archive tiers |
| Managed Disks | Azure-managed block storage attached to a VM — the VM's actual drives |
| Virtual Machines | IaaS compute: your OS, your patching, maximum control |
| App Services | PaaS web hosting: deploy the app, Azure runs the platform; slots, autoscale, managed identity |

The row that separates a confident candidate from a memorizing one is **IaaS/PaaS/SaaS** —
almost every other Azure question is a specific instance of that boundary.

---

## Cloud Fundamentals

### Cloud, and what it replaced

On-premises means you buy the hardware, house it, power it, and size it for your worst day, which
means it idles most of the year. Cloud converts that **capital expense into operating expense**:
you provision in minutes, pay for what you consume, and release it when you are done.

The five properties usually named: **on-demand self-service**, **broad network access**,
**resource pooling** (multi-tenant), **rapid elasticity**, **measured service** (metered billing).

**Deployment models:** public (shared provider infrastructure), private (dedicated, on-prem or
hosted), hybrid (both, connected), multicloud (more than one provider).

**Trap.** "Cloud is cheaper" is not a safe claim. Cloud is cheaper for variable, elastic, or
short-lived workloads, and often more expensive for a steady 24/7 workload you already own the
hardware for. The real wins are speed, elasticity, and offloaded operational burden.

### IaaS, PaaS, SaaS

| Layer | On-prem | IaaS | PaaS | SaaS |
|---|---|---|---|---|
| Data and access | You | You | You | You |
| Application | You | You | You | Provider |
| Runtime, middleware | You | You | Provider | Provider |
| OS, patching | You | You | Provider | Provider |
| Virtualization, servers, storage, network | You | Provider | Provider | Provider |
| Physical datacenter | You | Provider | Provider | Provider |

- **IaaS** — Azure Virtual Machines, Managed Disks, Virtual Network. You get a machine and a
  network; you install, patch, and secure the OS.
- **PaaS** — App Service, Azure SQL Database, Azure Functions. You get a runtime; you deploy code
  or a schema and never see a server.
- **SaaS** — Microsoft 365, Dynamics 365. You get an application and configure it.

The line to remember: **the boundary moves, the responsibility for your data never does.** In every
model you own your data, your identities, and who can reach them. That is the shared responsibility
model, and it is the single most common cloud-security interview question.

**Serverless / FaaS** sits inside PaaS: Azure Functions bills per execution, scales to zero, and
gives you no instance to think about. The cost is cold starts and execution time limits.

**Trap.** Calling App Service "IaaS because it runs on VMs". What decides the tier is **who is
responsible for the OS**, not what it is implemented with.

### Major providers, and the name mapping

The three that matter: **Amazon Web Services (AWS)**, **Microsoft Azure**, and **Google Cloud
Platform (GCP)**. They offer the same primitives under different names, and an interviewer who
worked on AWS will often ask you to translate.

| Capability | Azure | AWS | GCP |
|---|---|---|---|
| VMs | Virtual Machines | EC2 | Compute Engine |
| PaaS web hosting | App Service | Elastic Beanstalk | App Engine |
| Serverless functions | Functions | Lambda | Cloud Functions |
| Object storage | Blob Storage | S3 | Cloud Storage |
| Block storage | Managed Disks | EBS | Persistent Disk |
| Managed relational DB | Azure SQL Database | RDS | Cloud SQL |
| Managed NoSQL | Cosmos DB | DynamoDB | Firestore |
| Managed Kubernetes | AKS | EKS | GKE |
| Identity | Microsoft Entra ID | IAM | Cloud IAM |
| Private network | Virtual Network (VNet) | VPC | VPC |
| Monitoring | Azure Monitor | CloudWatch | Cloud Monitoring |

Positioning, honestly: **AWS** has the largest share and the widest service catalog; **Azure** wins
where the organization already runs Windows Server, SQL Server, and Active Directory, because
identity and licensing carry over; **GCP** is strongest in data analytics and Kubernetes, which it
originated. The row that matters is **object storage** — Blob/S3/Cloud Storage is the service every
cloud architecture touches.

### Azure, and what it gets used for

Azure is Microsoft's public cloud: hundreds of services under one resource-management API, one
identity system, and one billing hierarchy. Structurally, everything lives in
**management group > subscription > resource group > resource**. A resource group is a lifecycle
and permission boundary — things deployed and deleted together.

Common use cases interviewers accept: lift-and-shift of on-prem VMs, hosting web APIs and SPAs,
managed databases, disaster recovery and backup targets, dev/test environments, identity for SaaS
single sign-on, and data/AI workloads.

### Azure Well-Architected Framework

Five pillars — memorize the list, then one sentence each:

| Pillar | The question it asks |
|---|---|
| Reliability | Does it survive failure and recover to a known state? |
| Security | Are data, identity, and network defended in depth? |
| Cost Optimization | Are we paying only for value we actually get? |
| Operational Excellence | Can we deploy, monitor, and diagnose it repeatably? |
| Performance Efficiency | Does it scale to demand without overprovisioning? |

The pillar that trades against all the others is **cost optimization** — every reliability and
performance decision has a price, and the framework's real content is naming those trade-offs
deliberately rather than accidentally.

### Azure pricing models

| Model | What it is | Typical saving |
|---|---|---|
| Pay-as-you-go | Per-second or per-hour consumption, no commitment | Baseline |
| Reserved instances | 1- or 3-year commitment to a specific resource type/region | Large, in exchange for lock-in |
| Savings plans | 1- or 3-year commitment to an hourly compute spend, flexible across services | Slightly less than reservations, more flexible |
| Spot | Bid on unused capacity, evictable at short notice | The deepest discount |
| Azure Hybrid Benefit | Reuse existing Windows Server / SQL Server licenses with Software Assurance | Removes the license portion of the bill |
| Free tier / credits | 12-month free services, always-free services, a starting credit | Full, while it lasts |

The row that decides real bills is **reservations and savings plans**: steady-state production is
where committed pricing pays, and spot is only correct for interruptible work — batch, rendering,
CI agents, scale-set overflow. Never put a stateful production database on spot.

Free directions to name: **many services are free, their consumption is not** — a resource group,
a VNet, an NSG, Entra ID free tier, and Azure Advisor cost nothing; the VMs and disks inside them do.

### Regions and availability zones

- **Geography** — a market/compliance boundary (for example, United States, Europe) containing
  regions. Data residency is argued at this level.
- **Region** — a set of datacenters within a latency envelope. You pick a region for latency, for
  data residency, and for price, which varies by region.
- **Availability zone** — physically separate datacenters within one region, with independent
  power, cooling, and networking. Zone-redundant deployment survives losing one datacenter.
- **Region pair** — a second region in the same geography that Azure pairs for platform updates
  (rolled sequentially, never simultaneously) and for prioritized recovery order. Some newer
  regions are zone-based rather than paired.

The distinction interviewers probe: **zones protect against a datacenter failure; paired regions
protect against a regional disaster.** Different failure, different cost, different architecture.

**Trap.** Assuming every region has zones. Zone support is per region and per service, and a
service in a region without zones cannot be made zone-redundant by configuration.

### Azure Advisor

A free, always-on recommendation engine that reads your actual resource configuration and telemetry
and returns findings in five categories that mirror the Well-Architected pillars: **reliability,
security, cost, operational excellence, performance**. Typical output: an idle VM to resize or
deallocate, a reservation you would benefit from, a subscription without MFA, a database without
geo-redundant backup.

**Trap.** Advisor is advisory. It changes nothing on its own, and dismissing a recommendation is a
legitimate answer with a reason.

### Azure Monitor basics

Two data shapes, one platform:

| Data | Shape | Where it lives | Queried with |
|---|---|---|---|
| Metrics | Numeric time series, near real time, cheap | Metrics store | Metrics Explorer / charts |
| Logs | Structured records, verbose, richer | Log Analytics workspace | KQL |

- **Diagnostic settings** are the wiring: they route platform logs and metrics from a resource to
  a Log Analytics workspace, a storage account, or an Event Hub. With no diagnostic setting, the
  detailed logs are simply not collected.
- **Log Analytics workspace** is the destination and the query surface; **KQL** is the language
  (`AppRequests | where TimeGenerated > ago(1h) | summarize count() by ResultCode`).
- **Alerts** = signal + condition + action group. The action group is the "who gets told and how"
  (email, SMS, webhook, function, ITSM). Alert rules cost money; action groups mostly do not.
- **Application Insights** is the application-facing layer: request and dependency tracking,
  failures, live metrics, distributed tracing, availability tests. It is where you answer "which
  downstream call made this endpoint slow".
- **Log stream** (App Service) is the tail of stdout/stderr for one app, for immediate debugging —
  not a retention or analysis tool.

The distinction to hold: **metrics tell you something is wrong; logs and traces tell you why.**

---

## Administration and Security

### Cost Management and Billing

- **Scopes** follow the hierarchy: billing account, then management group, subscription, resource
  group. You analyze and budget at a scope.
- **Cost analysis** — the interactive breakdown: by service, by resource group, by tag, by
  location, over time, with forecasting.
- **Budgets** — a threshold at a scope with alert rules at percentages of it. **A budget notifies;
  it does not stop spending.** Stopping requires automation you wire yourself (an action group
  calling a function or runbook).
- **Tags** — key/value pairs on resources, the mechanism for chargeback and showback. Untagged
  resources are the reason cost reports cannot answer "which team spent this".
- **Exports** — scheduled cost data to storage for finance tooling.
- **Pricing Calculator vs TCO Calculator** — Pricing Calculator estimates the Azure cost of a
  design you are about to build; TCO Calculator compares an existing on-premises estate against
  its Azure equivalent, including power, cooling, and staff.

The one that reliably comes up is **budgets do not cap spend**. Say it before you are asked.

### Portal vs Azure CLI vs Azure PowerShell

All three call the same **Azure Resource Manager** REST API. The choice is about repeatability.

| Tool | Shape | Best at | Weak at |
|---|---|---|---|
| Portal | Web GUI | Discovery, one-off inspection, learning a service | Repeatability, review, audit |
| Azure CLI (`az`) | Cross-platform command, JSON output | Bash scripts, CI pipelines, quick queries | Object manipulation |
| Azure PowerShell (`Az` module) | Cmdlets returning .NET objects | Windows administration, piping objects between commands | Bash-native environments |

```
az group create --name rg-demo --location eastus
az webapp list --query "[?state=='Running'].name" -o tsv

New-AzResourceGroup -Name rg-demo -Location eastus
Get-AzWebApp | Where-Object { $_.State -eq 'Running' } | Select-Object Name
```

Cloud Shell in the portal gives you both, pre-authenticated. Above all three sits
**infrastructure as code** — ARM templates, Bicep, or Terraform — which is declarative: you
describe the desired state and the deployment converges to it, repeatably, in source control.

**Trap.** Calling the CLI "idempotent". Scripts are imperative and re-running one is not
guaranteed to be safe; that guarantee is what Bicep and Terraform exist to provide.

### Network Security Groups

An NSG is a **stateful** list of allow/deny rules filtering traffic to and from Azure resources.
Stateful matters: allow an inbound flow and its response is allowed out automatically.

A rule is: **priority, direction, source, source port, destination, destination port, protocol,
action**. Source and destination can be an IP or CIDR, a **service tag** (`Internet`,
`VirtualNetwork`, `AzureLoadBalancer`, `Storage`), or an **application security group**.

- **Priority** runs 100 to 4096. **Lower number wins, and evaluation stops at the first match.**
- **Default rules** cannot be deleted and sit at the bottom: inbound allows VNet traffic and the
  Azure load balancer, then **DenyAllInBound** at 65500; outbound allows VNet and internet, then
  **DenyAllOutBound** at 65500. So the effective default is: internal traffic allowed, inbound from
  the internet denied, outbound to the internet allowed.
- **Association** — an NSG attaches to a **subnet**, a **NIC**, or both. Inbound is evaluated
  subnet first, then NIC; outbound is NIC first, then subnet. Traffic must be allowed at both.
- **Application security groups** let you write rules against a logical group ("web servers") rather
  than IP addresses, so the rule survives the addressing changing.

NSG versus the alternatives: an NSG is a **layer 3/4 packet filter**, free, and attached to your
own subnets. **Azure Firewall** is a managed, stateful, layer 3-7 network appliance with FQDN
filtering, threat intelligence, and centralized logging — it costs real money and protects a whole
network. **Web Application Firewall** (on Application Gateway or Front Door) inspects HTTP for
injection and OWASP-class attacks. They are layers, not substitutes.

**Trap.** "We have NSGs, so we have a firewall." An NSG cannot inspect payloads, cannot filter by
domain name, and will not stop SQL injection over an allowed port 443.

### Microsoft Entra ID and RBAC

**Entra ID** (formerly Azure Active Directory) is Azure's cloud identity provider: it authenticates
users, groups, and applications and issues tokens over OAuth 2.0, OpenID Connect, and SAML.

The single most-missed fact: **Entra ID is not Active Directory in the cloud.** It has no domain
join, no group policy, no LDAP, no Kerberos by default. It is an identity provider for cloud and
SaaS applications. The service that does offer domain-join and LDAP is **Entra Domain Services**,
and hybrid setups sync on-prem AD into Entra with **Entra Connect**.

Identity objects: **users**, **groups**, **service principals** (an application's identity in a
tenant), and **managed identities** (a service principal Azure creates and rotates for you).

- A **tenant** is one directory instance, one organization. A **subscription** is a billing and
  deployment container that **trusts exactly one tenant**. One tenant can hold many subscriptions.

**RBAC is authorization**, and it answers a different question than Entra ID. A **role assignment**
is exactly three things:

**security principal (who) + role definition (what) + scope (where)**

- **Scope** inherits downward: management group > subscription > resource group > resource. Grant at
  the narrowest scope that works — that is least privilege in practice.
- **Built-in roles** to know: **Owner** (everything, including granting access), **Contributor**
  (everything except granting access), **Reader** (view only), **User Access Administrator**
  (manage access, not resources). Then the data-plane roles, for example
  **Storage Blob Data Contributor** and **Key Vault Secrets User**.
- RBAC is **additive** — assignments accumulate, there is no "deny" role. The exception is an
  explicit **deny assignment**, which overrides allows and is created by the platform, not by you.
- **Azure RBAC is not Entra roles.** Global Administrator is an Entra directory role and grants no
  rights over resources; Owner is an Azure resource role and grants no directory rights. Interviewers
  love this one.
- **Azure Policy** is the other half of governance: RBAC says *who may act*, Policy says *what
  configuration is allowed* — deny a VM SKU, require a tag, audit unencrypted storage.

**Trap.** Answering "authentication" when asked about RBAC. Entra ID proves who you are; RBAC
decides what you may do. Say both words.

### Key Vault

A managed service for three object types: **secrets** (connection strings, passwords, API keys),
**keys** (cryptographic keys, optionally HSM-backed, used without ever leaving the vault), and
**certificates** (TLS certs with lifecycle and auto-renewal).

- **Two permission models.** Legacy **access policies** are vault-wide and not granular per object;
  **Azure RBAC** is the current recommendation and uses roles like *Key Vault Secrets User*.
  Access is also gated by network rules — firewall, service endpoints, private endpoint.
- **Access from an app** should use a **managed identity**: the app asks the platform for a token,
  Azure issues it, no credential exists in your code or config. The whole point is removing the
  bootstrap secret.
- **App Service Key Vault references** put `@Microsoft.KeyVault(SecretUri=...)` into an app setting;
  the platform resolves it at startup and the app just reads configuration.
- **Soft delete** is on by default: deleted objects are recoverable for a retention window.
  **Purge protection** additionally blocks permanent deletion during that window — the defense
  against a malicious or mistaken wipe.
- Every operation can be logged to Azure Monitor, which is what makes the vault auditable.

**Trap.** "We keep secrets in `appsettings.json` and gitignore it." That leaks through builds,
logs, and developer machines, has no rotation and no audit trail, and the file eventually gets
committed. The honest architecture: no secret in source at all — Key Vault plus a managed identity.

---

## Core Services

### Azure SQL Database

Three ways to run SQL Server on Azure, and the interview question is which one and why:

| Option | Model | You manage | Pick it when |
|---|---|---|---|
| SQL Server on a VM | IaaS | OS, patching, backups, HA | You need full instance control, an old feature, or a lift-and-shift |
| SQL Managed Instance | PaaS | Almost nothing | You need near-full SQL Server surface (SQL Agent, cross-database queries) inside a VNet |
| Azure SQL Database | PaaS | Schema and data | Greenfield apps, single databases, maximum managed-ness |

**Azure SQL Database** gives you a database, not a server: no OS, no patching, automatic backups,
built-in HA. What you lose is the instance surface — **no SQL Server Agent**, no cross-database
queries, no `master`-level operations.

- **Purchasing models.** **DTU** bundles compute, memory, and I/O into one blended unit — simple,
  single databases only. **vCore** prices compute and storage separately, is the current default,
  and is the only model where **Azure Hybrid Benefit** applies.
- **Service tiers** (vCore): **General Purpose** (balanced), **Business Critical** (local SSD, low
  latency, a readable replica), **Hyperscale** (very large databases, fast restore).
- **Serverless** auto-scales compute and **auto-pauses** when idle, billing per second of use —
  correct for dev/test and intermittent workloads, wrong for anything latency-sensitive, because a
  resumed database has a cold start.
- **Elastic pools** share one budget of resources across many databases with uneven peaks — the
  multi-tenant SaaS answer.
- **Security.** Server-level and database-level firewall rules, the "allow Azure services" toggle
  (convenient and broad — prefer explicit rules), **private endpoint** for VNet-only access,
  Entra authentication instead of SQL logins, Transparent Data Encryption on by default, Always
  Encrypted for column-level protection, and auditing to a Log Analytics workspace.
- **Continuity.** Automatic backups with point-in-time restore over a 1-35 day window,
  **active geo-replication** for readable secondaries, and **failover groups** for a stable
  listener endpoint that survives a regional failover.

The row that decides the answer is **the instance surface**: if the application needs SQL Agent or
cross-database joins, Azure SQL Database is the wrong service no matter how attractive its pricing.

### Blob Storage

Object storage for unstructured data. The hierarchy is
**storage account > container > blob**, and the account name is globally unique because it becomes
a DNS name (`https://<account>.blob.core.windows.net`).

- **Blob types.** **Block** blobs for the ordinary case — files, images, backups, logs.
  **Append** blobs, optimized for appending, so logging. **Page** blobs, for random-access reads
  and writes, which is what VHDs and managed disks are built on.
- **Access tiers** trade storage price against retrieval price and latency:

| Tier | Storage cost | Access cost | Minimum retention | Retrieval |
|---|---|---|---|---|
| Hot | Highest | Lowest | None | Immediate |
| Cool | Lower | Higher | 30 days | Immediate |
| Cold | Lower still | Higher still | 90 days | Immediate |
| Archive | Lowest | Highest | 180 days | Offline — rehydration takes hours |

- **Redundancy.** **LRS** three copies in one datacenter; **ZRS** three copies across availability
  zones; **GRS** LRS plus asynchronous replication to a paired region; **GZRS** the combination;
  **RA-GRS/RA-GZRS** adds read access to the secondary. Cost rises left to right, and asynchronous
  replication means a regional failure can lose the most recent writes.
- **Access control**, worst to best: **account keys** (two, full control, all-or-nothing, rotate
  them), **shared access signatures** (a scoped, time-limited, revocable URL — a
  *user delegation SAS* signed with Entra credentials is the good kind), and **Entra RBAC data
  roles** (*Storage Blob Data Reader/Contributor*) with a managed identity, which is the target
  state. Anonymous public access can and usually should be disabled at the account level.
- **Lifecycle management** policies move or delete blobs by age automatically — the practical way
  to stop paying hot prices for cold data. **Versioning**, **soft delete**, and **immutable
  (WORM) policies** cover recovery and compliance.
- **Static website hosting** serves a SPA straight from the `$web` container with an index and
  error document. Custom domains with TLS need a CDN or Front Door in front.

**Trap.** Treating archive as "just cheaper storage". It is offline; a read requires rehydration
measured in hours, and early deletion incurs the minimum-retention charge.

### Managed Disks

Block storage attached to a VM, where Azure manages the underlying storage account, replication,
and placement. You choose a type and a size; performance follows from both.

| Type | Media | Use |
|---|---|---|
| Ultra Disk | NVMe, tunable IOPS/throughput independent of size | Top-tier databases, extreme I/O |
| Premium SSD v2 | SSD, flexible sizing and performance | Modern production default |
| Premium SSD | SSD, performance tied to size tier | Production workloads, single-VM SLA |
| Standard SSD | SSD, entry level | Light production, dev/test |
| Standard HDD | Spinning | Backup, infrequent access |

- **Disk roles.** The **OS disk** holds the operating system. **Data disks** are attached for
  application data, with a per-VM-size limit on how many. The **temporary disk** (often `D:` on
  Windows) is local to the host, is **not persistent**, and is lost on deallocation or host
  maintenance — never put data there.
- **Snapshots** are point-in-time copies of a disk (full or incremental); **images** are the basis
  for creating new VMs.
- **Encryption at rest is on by default** — server-side encryption with platform-managed keys.
  Options: customer-managed keys in Key Vault, encryption at host, and Azure Disk Encryption
  (BitLocker or dm-crypt inside the guest).
- **Shared disks** allow attaching one disk to several VMs for clustered workloads.
- **Ephemeral OS disks** live on the host: free, very fast to reimage, and lost on reboot — correct
  for stateless scale-set instances.
- **Billing.** Disks are provisioned capacity, so **a stopped VM still pays for its disks**.

**Trap.** Sizing the disk and forgetting the VM. The VM size has its own IOPS and throughput cap,
so a Premium disk on an undersized VM delivers the VM's ceiling, not the disk's.

### Azure Virtual Machines

IaaS compute: you pick a size, an image, and a network, and from the OS up it is yours to patch,
secure, and monitor.

- **Size families** by workload: **B** burstable (cheap, credit-based), **D** general purpose,
  **E** memory-optimized, **F** compute-optimized, **L** storage-optimized, **M** very large
  memory, **N** GPU.
- **Availability**, in increasing order of protection: a single VM with premium disks;
  an **availability set**, which spreads instances across **fault domains** (separate racks, power,
  network) and **update domains** (patched at different times); **availability zones**, which spread
  across datacenters; and **virtual machine scale sets**, which add automatic scaling and a uniform
  instance model.
- **Spot VMs** take surplus capacity at a deep discount and can be evicted with short notice —
  batch, CI agents, rendering, never a stateful production tier.
- **Images** come from the Marketplace, from your own Azure Compute Gallery, or from a captured VM.
  **Extensions** (custom script, monitoring agents, DSC) run configuration at provisioning time.
- **Access.** RDP or SSH should not be exposed to the internet. **Azure Bastion** brokers browser
  based RDP/SSH with no public IP on the VM; **just-in-time access** opens the port only on request.
- **Diagnostics.** Boot diagnostics gives you a screenshot and serial log for a VM that will not
  start — the first stop when a VM is unreachable.
- **Billing.** Compute is charged while the VM is running. **Stopping the OS from inside the guest
  keeps billing; "Stop (deallocate)" in Azure releases the compute and stops the compute charge.**
  Disks, public IPs, and reserved addresses are charged either way.

**Trap.** "We shut the VM down over the weekend" — from inside the guest, that saves nothing.
Deallocate, or schedule automation to do it.

### Azure App Services

PaaS for web applications, REST APIs, and background jobs. You deploy code or a container; Azure
provides the OS, the runtime, patching, load balancing, and TLS termination.

- **App Service Plan is the unit you pay for** — the set of VMs the apps run on. Many apps can
  share a plan, and they then share its CPU and memory. Tiers: **Free/Shared** (quotas, no custom
  domain TLS, no Always On), **Basic** (dedicated compute, manual scale), **Standard** (autoscale,
  deployment slots, daily backups), **Premium v3** (more instances and slots, better hardware, zone
  redundancy), **Isolated / App Service Environment** (single-tenant, deployed into your VNet).
- **Deployment slots** are live, addressable instances of the app (`staging`, `qa`) on the same
  plan. Deploy there, warm it up, test it, then **swap** — the swap exchanges the running instances,
  so rollback is a second swap. Settings marked **slot settings** stay with the slot instead of
  travelling with the swap, which is how per-environment configuration survives.
- **Scaling.** **Scale up** changes the plan tier (bigger machines). **Scale out** adds instances,
  either manually or by **autoscale rules** driven by a metric (CPU, queue length) or a schedule.
  Rules need both a scale-out and a scale-in condition, with a cooldown, or the app flaps.
- **Configuration.** Application settings are environment variables that **override** matching
  values in `appsettings.json`, so no secret needs to ship in the build. Combine with a **managed
  identity** and **Key Vault references** for a config surface with no stored credentials.
- **Networking.** Public by default; VNet integration for outbound access to private resources,
  private endpoints for inbound, and access restrictions for IP rules.
- **Custom domains and TLS**, including free App Service managed certificates.
- **Diagnostics.** Log stream for live stdout/stderr, App Service logs, the Kudu/SCM console for the
  file system and deployment history, and Application Insights for real telemetry.
- **Always On** keeps the app warm so it is not unloaded when idle — required for background and
  timer work, and unavailable on Free/Shared.

**Trap.** "We have three apps so we pay three times." You pay for the **plan**, not the app — and
that is also the risk, because three apps on one plan compete for the same CPU.

---

## Choosing Between Services

### Compute

| Need | Service |
|---|---|
| Full OS control, legacy software, custom drivers | Virtual Machines |
| A web app or API, minimal operations | App Service |
| Containers without running Kubernetes | Container Apps |
| Event-driven, short-lived, scale to zero | Functions |
| Full container orchestration, many services, a platform team | AKS |

The deciding question is **how much of the platform you want to own**. Interviewers are testing
whether you reach for a VM by default; the defensible answer starts at PaaS and moves down only for
a named requirement.

### Storage

| Need | Service |
|---|---|
| Unstructured files, images, backups, static site | Blob Storage |
| A drive attached to one VM | Managed Disks |
| An SMB/NFS file share several machines mount | Azure Files |
| Relational data with queries and transactions | Azure SQL Database |
| Schemaless documents, global distribution, low latency | Cosmos DB |

The row people get wrong is **Blob versus Files**: Blob is reached over HTTP by an application,
Files is mounted as a drive letter by an operating system.

### Getting access to a resource

| Mechanism | Secret to manage | Use when |
|---|---|---|
| Account key / connection string | Yes, and it is all-powerful | Legacy only; rotate and plan to remove |
| Shared access signature | Yes, but scoped and time-limited | Granting a client or partner narrow, temporary access |
| Service principal + client secret | Yes | Automation outside Azure (CI runners, on-prem jobs) |
| Managed identity | **No** | Anything running **in** Azure — the default answer |

The row that is almost always correct is **managed identity**: if the workload runs in Azure, there
is no reason for a stored credential to exist.

---

## Say It in an Interview

- *"Cloud computing is on-demand, metered access to someone else's infrastructure. The real wins
  are elasticity and speed of provisioning, not automatically a lower bill."*
- *"IaaS gives me a machine and I own the OS upward; PaaS gives me a runtime and I own the app and
  data; SaaS gives me finished software and I own only my data and identities. The responsibility
  boundary moves, but ownership of the data never leaves me."*
- *"AWS, Azure, and GCP map service for service — EC2 and Virtual Machines, S3 and Blob Storage,
  RDS and Azure SQL. Azure tends to win where the organization is already on Windows Server, SQL
  Server, and Active Directory, because identity and licensing carry across."*
- *"The Well-Architected Framework is five pillars: reliability, security, cost optimization,
  operational excellence, and performance efficiency. It is useful because it forces you to name
  which pillar you are trading away."*
- *"Pay-as-you-go for variable load, reservations or a savings plan for steady production, spot
  only for interruptible work, and Hybrid Benefit if we already own the licenses."*
- *"A region is where I deploy; availability zones are separate datacenters inside that region, so
  zone-redundancy survives losing one building; paired regions are for a regional disaster."*
- *"Azure Advisor reads my actual resources and recommends across cost, security, reliability,
  performance, and operational excellence. It recommends, it does not change anything."*
- *"Azure Monitor collects metrics and logs; metrics tell me something is wrong, logs and
  Application Insights traces tell me why. Alerts are a signal plus a condition plus an action
  group."*
- *"Cost Management gives me cost analysis, budgets, and alerts, and tags are what make the report
  attributable to a team. A budget notifies at a threshold — it does not stop the spend."*
- *"The portal, the CLI, and the Az PowerShell module all call the same Resource Manager API. I use
  the portal to explore and the CLI in pipelines, but anything that has to be repeatable belongs in
  Bicep or Terraform."*
- *"A network security group is a stateful layer 3/4 rule list on a subnet or a NIC. Rules are
  evaluated by priority, lowest number first, first match wins, and the platform's default rules
  deny inbound from the internet and allow outbound. It is not a firewall — it cannot inspect
  payloads or filter by domain."*
- *"Microsoft Entra ID is a cloud identity provider, not Active Directory in the cloud. There is no
  domain join, no group policy, no Kerberos — it issues tokens over OpenID Connect and OAuth."*
- *"An RBAC role assignment is a security principal, a role definition, and a scope. Scope inherits
  from management group down to the resource, permissions are additive, and I assign at the
  narrowest scope that still works."*
- *"Owner, Contributor, Reader, and User Access Administrator are the four built-ins to know.
  Contributor can build anything but cannot grant anyone else access — that separation is the
  point."*
- *"Key Vault stores secrets, keys, and certificates, and the application reaches it with a managed
  identity, so no credential is ever stored in configuration or source. Soft delete and purge
  protection are what make a deletion recoverable."*
- *"Azure SQL Database is the managed database engine: no OS, no patching, automatic backups with
  point-in-time restore. What I give up is the instance surface — no SQL Agent, no cross-database
  queries — so if the app needs those, I go to Managed Instance."*
- *"Blob Storage is object storage: account, container, blob, with hot, cool, cold, and archive
  tiers and LRS through geo-zone-redundant replication. Access should be an Entra data role with a
  managed identity, a scoped SAS for a third party, and account keys only for legacy."*
- *"Managed disks are the VM's drives. Encryption at rest is on by default, snapshots handle
  point-in-time recovery, the temporary disk is not persistent, and a stopped VM still pays for its
  disks."*
- *"A VM is IaaS, so I own patching and hardening. Availability sets spread across racks, zones
  spread across datacenters, and scale sets add autoscaling. Stopping the OS from inside the guest
  does not stop the bill — you have to deallocate."*
- *"App Service is PaaS web hosting. I pay for the App Service Plan, not the app; deployment slots
  let me warm up a release and swap it in with a one-step rollback; and app settings plus a managed
  identity plus Key Vault references mean no secret in the build."*

---

## Traps Panelists Listen For

| The wrong answer | What to say instead |
|---|---|
| "The cloud is always cheaper" | Cheaper for elastic and short-lived workloads; steady 24/7 load can favor owned hardware. The wins are speed and elasticity |
| "App Service is IaaS, it runs on VMs" | The tier is decided by who owns the OS, not the implementation |
| "Entra ID is Active Directory in the cloud" | It is an identity provider — no domain join, group policy, LDAP, or Kerberos by default |
| "Global Administrator can manage the resources" | Entra directory roles and Azure RBAC roles are separate systems; Owner is the resource-side role |
| "We have NSGs, so we are firewalled" | An NSG is a layer 3/4 packet filter; payload inspection is Azure Firewall or a WAF |
| "The budget will stop the overspend" | Budgets alert; stopping requires automation you build |
| "Secrets are safe in gitignored config" | They leak through builds, logs, and machines, with no rotation or audit — use Key Vault with a managed identity |
| "We shut the VM down at night" | Only "Stop (deallocate)" stops compute billing; disks bill regardless |
| "Archive is just cheaper storage" | It is offline — rehydration takes hours and early deletion is penalized |
| "Premium disk, so it will be fast" | The VM size caps disk IOPS and throughput independently |
| "Three apps, three bills" | You pay for the App Service Plan, and co-located apps contend for the same compute |

---

## Check Yourself

1. Name the three responsibility tiers and say who patches the OS in each.
2. An NSG has an allow rule at priority 300 and a deny rule at priority 200 for the same traffic.
   What happens, and why?
3. What are the three parts of an RBAC role assignment, and which built-in role can create
   resources but not grant access?
4. How does an application in App Service read a Key Vault secret without storing a credential?
5. What is the difference between an availability zone and a region pair?
6. You need SQL Server Agent jobs. Which Azure SQL option, and why not the others?
7. Distinguish an account key, a SAS, and an Entra data role for Blob access. Which is the default
   answer for a workload running in Azure?
8. A VM was "shut down" and the bill did not change. What happened?
9. What does swapping a deployment slot actually do, and how do you roll back?
10. Metrics or logs: which tells you an endpoint is slow, and which tells you why?

**Answers:** (1) IaaS — you patch; PaaS — provider patches the OS and runtime, you own the app and
data; SaaS — provider patches everything, you own data and identities. (2) The deny wins: rules are
evaluated lowest priority number first and evaluation stops at the first match. (3) Security
principal, role definition, scope; Contributor. (4) The app has a managed identity with a Key Vault
role assignment, and the platform resolves a Key Vault reference in the app setting at startup — no
secret is stored. (5) A zone is a physically separate datacenter inside one region and protects
against a datacenter failure; a region pair is a second region in the same geography for regional
disaster and sequenced platform updates. (6) SQL Managed Instance — Azure SQL Database has no SQL
Agent, and SQL Server on a VM would work but hands back all the OS and patching burden. (7) Account
key = all-powerful shared secret, legacy only; SAS = scoped, time-limited, revocable URL for a
specific client; Entra data role with a managed identity = no stored secret and the default for
in-Azure workloads. (8) It was stopped from inside the guest; only "Stop (deallocate)" releases the
compute and stops the compute charge, and disks bill either way. (9) It exchanges the running
instances of the two slots after warm-up, carrying non-slot settings across; roll back by swapping
again. (10) Metrics show the latency, logs and Application Insights dependency traces show which
call caused it.

---

## Resources

- [Describe cloud computing (shared responsibility, cloud models, pricing) — Microsoft Learn](https://learn.microsoft.com/en-us/training/modules/describe-cloud-compute/)
- [Azure Well-Architected Framework pillars — Microsoft Learn](https://learn.microsoft.com/en-us/azure/well-architected/pillars)
- [What is Azure role-based access control (RBAC)? — Microsoft Learn](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview)
- [Network security groups — Microsoft Learn](https://learn.microsoft.com/en-us/azure/virtual-network/network-security-groups-overview)
- [About Azure Key Vault — Microsoft Learn](https://learn.microsoft.com/en-us/azure/key-vault/general/overview)
- [App Service overview — Microsoft Learn](https://learn.microsoft.com/en-us/azure/app-service/overview)
