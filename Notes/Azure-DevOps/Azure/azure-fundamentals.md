# Cloud Fundamentals and Azure

## Learning Objectives
- Explain what cloud computing is and why organizations move to it: the CapEx-to-OpEx shift,
  elasticity, and the five essential characteristics of the NIST definition.
- Place any cloud offering on the IaaS / PaaS / SaaS spectrum using the shared-responsibility
  model, and defend the placement ("who patches the OS?").
- Compare the three major providers (AWS, Azure, GCP) at interview depth: market position,
  naming differences for the same primitives, and when a company lands on each.
- Describe Azure's global structure: regions, availability zones, region pairs — and what each
  one protects you from.
- Name Azure's pricing models (pay-as-you-go, reservations, savings plans, spot, hybrid benefit)
  and the tools that watch cost and health: Azure Advisor and Azure Monitor.
- State the five pillars of the Azure Well-Architected Framework and give a one-line example of
  each.

## Why This Matters
Nearly every .NET job posting lists a cloud platform, and for .NET shops that platform is
usually Azure. Interviewers rarely open with a deep service question; they open with vocabulary:
"what's the difference between IaaS and PaaS?", "why would a company move to the cloud?",
"what's an availability zone?". These are filter questions — a candidate who stumbles on them
does not get to the deep ones. The concepts in this note are also load-bearing for real work:
choosing App Service over a VM, or understanding why a region-wide outage did not take your
database down, is this vocabulary applied.

## The Concept

### From the server room to the cloud
Before cloud, running software meant owning hardware: buy servers (capital expenditure — CapEx),
rack them, power them, cool them, replace them every few years, and size them for your PEAK load
— which means they sit mostly idle the rest of the year. Cloud computing replaces that with
renting compute from a provider's datacenter and paying for what you use (operational
expenditure — OpEx). The provider owns the hardware problem; you consume capacity as a metered
utility, like electricity.

The trade is not purely financial. The deeper win is **elasticity**: capacity can follow load.
An online bookstore that triples its traffic every December used to own December-sized hardware
all year; in the cloud it scales out for December and back in for January, paying for the spike
only while the spike exists.

### The NIST definition: five essential characteristics
The standard definition of cloud computing (NIST SP 800-145) names five properties. Interviewers
ask for these surprisingly often:

1. **On-demand self-service** — you provision resources through a portal or API, no purchase
   order, no human on the provider's side.
2. **Broad network access** — everything is reachable over standard networks and protocols.
3. **Resource pooling** — the provider's hardware is shared across many tenants
   (multi-tenancy); you get isolation, not dedicated metal, unless you pay for it.
4. **Rapid elasticity** — capacity scales out and in quickly, ideally automatically, and
   appears unlimited from the consumer's side.
5. **Measured service** — usage is metered, and you pay for what is measured. The meter is also
   why cost management is a discipline: the meter never sleeps.

### Deployment models
- **Public cloud** — resources run in the provider's shared datacenters (the default, and what
  almost all of this note describes).
- **Private cloud** — cloud-style self-service and pooling, but on infrastructure dedicated to
  one organization (on-premises or hosted). Chosen for regulatory or data-sovereignty reasons.
- **Hybrid cloud** — a deliberate mix: for example, a hospital keeps patient records in a
  private datacenter but bursts its public website into the public cloud. Azure leans into this
  with services like Azure Arc (manage on-prem servers from Azure).

### IaaS / PaaS / SaaS: the responsibility spine
The single most-asked cloud interview question. The layers differ in **who manages what** — draw
the responsibility table:

| Layer | You manage | Provider manages | Azure example |
|---|---|---|---|
| **On-prem** | Everything | Nothing | your own server room |
| **IaaS** | OS patching, runtime, app, data | Hardware, network, virtualization | Virtual Machines, Managed Disks |
| **PaaS** | App and data only | Everything below: OS, runtime patching, scaling plumbing | App Service, Azure SQL Database |
| **SaaS** | Your data and users | The whole application | Microsoft 365, Dynamics |

The discriminating question for any service is **"who patches the operating system?"** If you
do, it is IaaS. If the provider does but you deploy your own code, it is PaaS. If you just log
in and use it, SaaS. A useful second probe: "can I RDP/SSH into the box?" — yes means IaaS.

Costs and control move in opposite directions: IaaS gives maximum control and maximum
operational burden; SaaS gives none of either. Modern application teams default to PaaS and
step down to IaaS only when they need something PaaS will not allow (a custom OS dependency, an
exotic runtime, lift-and-shift of a legacy system).

### The big three providers
- **AWS (Amazon Web Services)** — first mover (2006), largest market share, broadest service
  catalog. Names to recognize: EC2 (VMs), S3 (object storage), RDS (managed databases), Lambda
  (serverless functions).
- **Microsoft Azure** — strong second, dominant in enterprises already on the Microsoft stack
  (Windows Server, Active Directory, Office). Its integration story — Entra ID single sign-on,
  hybrid tooling, .NET-first developer experience — is why .NET shops overwhelmingly choose it.
  Names: Virtual Machines, Blob Storage, Azure SQL Database, Azure Functions.
- **GCP (Google Cloud Platform)** — third in share, strongest reputation in data analytics,
  machine learning, and Kubernetes (which Google originated). Names: Compute Engine, Cloud
  Storage, BigQuery, GKE.

The primitives are equivalent across all three — VMs, object storage, managed SQL, serverless
functions, managed Kubernetes — so provider skills transfer at the concept level. In an
interview, mapping one provider's service names onto another's ("S3 is Azure Blob Storage")
signals you understand the concepts rather than one vendor's menu.

### Azure's global structure: regions, zones, pairs
- A **region** is a geographic area containing one or more datacenters (East US, West Europe,
  ...). You pick a region for every resource; latency and data-residency law drive the choice.
- An **availability zone (AZ)** is a physically separate datacenter (own power, cooling,
  network) *within* a region. Spreading instances across zones protects against a
  datacenter-level failure while keeping single-digit-millisecond latency between them. Not
  every region has zones; zone-redundant SKUs of a service cost more.
- **Region pairs**: most Azure regions have a designated partner region in the same geography
  (East US pairs with West US). Platform updates roll out to one of the pair at a time, and
  geo-redundant storage replicates to the pair. Pairing is what a *region*-level disaster plan
  builds on.

The ladder of blast radius is worth saying aloud in an interview: a rack failure is survived by
the provider's basic redundancy, a datacenter failure by availability zones, a regional disaster
by region pairs and geo-replication — each step up costs more and is a business decision, not a
default.

### Pricing models
Azure's meter can be paid several ways; knowing the names is Should-know interview currency:

- **Pay-as-you-go** — the default hourly/secondly meter. Most flexible, highest unit price.
- **Reservations (reserved instances)** — commit to 1 or 3 years of a specific resource shape
  for discounts up to ~70%. For steady, predictable workloads.
- **Savings plans** — commit to an hourly SPEND rather than a specific resource; more flexible
  than reservations, slightly smaller discount.
- **Spot pricing** — buy the provider's spare capacity at a steep discount; the catch is your
  VM can be **evicted when Azure needs the capacity back**. Only for interruptible work (batch
  jobs, test agents) — never for anything a user is waiting on.
- **Azure Hybrid Benefit** — bring existing on-prem Windows Server / SQL Server licenses to
  Azure and stop paying for the license portion of the meter. An enterprise-migration lever.
- **Free tier and dev/test offers** — many services have permanently free low tiers (they
  appear throughout the storage and App Service notes as the tiers a learning exercise should
  land on).

### Azure Advisor
Advisor is Azure's built-in recommendation engine. It continuously inspects your actual
resources and emits recommendations in five buckets — **cost** (that VM has been idle for two
weeks — resize or delete it), **security**, **reliability**, **performance**, and **operational
excellence**. The recommendations map onto the Well-Architected pillars below, which makes
Advisor essentially "Well-Architected as a service" for an existing deployment. It is free, and
in a cost-conscious shop, checking Advisor is a weekly ritual.

### Azure Monitor, in one paragraph
Azure Monitor is the platform's umbrella observability service: every resource emits **metrics**
(numeric time series — CPU percent, request count) and most emit **logs** (structured records
queryable with KQL, the Kusto Query Language) into it; on top of those, Monitor provides
dashboards and **alert rules** (notify or auto-act when a metric crosses a threshold).
Application-level telemetry (requests, dependencies, exceptions inside YOUR code) comes from
Application Insights, which feeds the same pipeline — the application-side story lives in the
App Services and observability note.

### The Well-Architected Framework: five pillars
Azure's published standard for judging a workload's architecture. Learn the five pillars with
one concrete example each:

1. **Cost optimization** — right-size resources; delete what is idle; use reservations for
   steady load. ("Why is this dev database on a production tier?")
2. **Security** — least privilege, encrypt in transit and at rest, keep secrets out of code.
3. **Reliability** — design for failure: health probes, retries, zone redundancy, backups
   that have been *restored* at least once.
4. **Performance efficiency** — scale horizontally, cache what is hot, measure before tuning.
5. **Operational excellence** — automate deployment, monitor everything, practice incident
   response.

The framework's use in real teams is as a review checklist ("Well-Architected review") and as a
shared vocabulary for trade-offs — every pillar can be traded against another, and naming the
trade ("we accepted single-region reliability to hit the cost target") is the mark of a
deliberate design.

## Say It in an Interview
- **"What is cloud computing and why do companies adopt it?"** — "Renting compute, storage, and
  services from a provider's datacenters over the internet, metered like a utility. Companies
  adopt it to trade upfront hardware spend for pay-as-you-go cost, and — more importantly — for
  elasticity: capacity can follow demand instead of being sized for the annual peak."
- **"IaaS vs PaaS vs SaaS?"** — "It's a question of who manages what. IaaS rents me the
  virtualized hardware and I own everything from the OS up — Azure VMs. PaaS runs my code on a
  managed platform — the provider patches the OS and runtime; App Service and Azure SQL are the
  ones I use. SaaS is a finished application I just consume, like Microsoft 365. The quick test
  is 'who patches the OS?'"
- **"Region vs availability zone?"** — "A region is a geographic area of datacenters; an
  availability zone is one isolated datacenter within a region. Zones protect against a
  datacenter failure with no meaningful latency cost; surviving a whole-region disaster needs
  geo-replication to a paired region."
- **"How do you keep cloud costs under control?"** — "Tag resources so cost has an owner,
  set budgets with alerts, check Azure Advisor's cost recommendations, use reservations for
  steady load and spot for interruptible work, and tear down what is not in use — the meter
  runs on provisioned resources whether or not they do anything."
- **"What is the Well-Architected Framework?"** — "Azure's five-pillar standard for judging a
  workload: cost optimization, security, reliability, performance efficiency, and operational
  excellence. In practice it's a review checklist and a vocabulary for trade-offs."

## Check Yourself
1. Your company's e-commerce site runs on servers sized for Black Friday. What cloud property
   fixes this, and what billing change comes with it?
2. Who patches the OS for: an Azure VM, Azure App Service, Azure SQL Database?
3. A datacenter loses power. Which Azure construct keeps your app up, and what did you have to
   do at design time to benefit from it?
4. Name a workload that belongs on spot pricing and one that must never be on it.
5. Which free Azure service tells you a VM has been idle for two weeks and suggests resizing it?

**Answers:** (1) Rapid elasticity — scale out for the peak, back in after; billing shifts from
CapEx (owned peak-sized hardware) to OpEx (pay while scaled out). (2) You; Microsoft; Microsoft
— that is exactly the IaaS/PaaS line. (3) Availability zones; you had to deploy a
zone-redundant configuration (multiple instances spread across zones, or a zone-redundant SKU).
(4) Spot: batch processing, CI build agents, anything restartable. Never: a customer-facing web
server or a database. (5) Azure Advisor (cost recommendations).

## Resources
- [What is cloud computing? (Microsoft Learn)](https://learn.microsoft.com/en-us/training/modules/describe-cloud-compute/)
- [Azure regions and availability zones](https://learn.microsoft.com/en-us/azure/reliability/availability-zones-overview)
- [Azure Well-Architected Framework](https://learn.microsoft.com/en-us/azure/well-architected/)
