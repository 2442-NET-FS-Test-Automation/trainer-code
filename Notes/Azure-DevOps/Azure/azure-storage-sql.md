# Azure Blob Storage and Azure SQL Database, Hands-On Depth

## Learning Objectives
- Work with Blob Storage's object hierarchy — storage account, container, blob — and choose
  account redundancy (LRS/ZRS/GRS) and blob access tiers (hot/cool/archive) deliberately.
- Control blob access: private-by-default containers, public access levels, account keys vs
  Entra ID (RBAC data roles) vs SAS tokens.
- Host a static website from a Blob container: what the `$web` container is, what the platform
  does (and does not) provide.
- Provision an Azure SQL Database with the CLI: logical server, database, service objective —
  and explain the DTU-vs-vCore purchasing models and where Basic/serverless tiers fit.
- Open the two standard firewall doors (client IP; allow-Azure-services) and explain why a new
  Azure SQL database refuses everyone by default.
- Point an existing application at Azure SQL by configuration only, apply EF Core migrations to
  it, and name the monitoring surfaces (metrics, Query Performance Insight) you check when it
  slows down.

## Why This Matters
These are the two services a working .NET developer touches in week one of a cloud job: files
go to Blob, data goes to Azure SQL. Interviews probe them concretely — "how would you host a
React build for pennies?", "your app can't reach the new cloud database, what do you check
first?" (firewall, nine times out of ten), "DTU or vCore?". And the migration story — move a
database to the cloud WITHOUT touching application code — is the single best demonstration
that configuration-driven connection strings were worth the discipline.

## The Concept

### Blob Storage: account, container, blob
Three levels:

- **Storage account** — the top-level resource; its name becomes a DNS name
  (`https://<account>.blob.core.windows.net`), so it must be globally unique, 3-24 characters,
  lowercase letters and digits only. Account-level choices: region, kind (StorageV2 is the
  modern general-purpose kind), and **redundancy**:
  - **LRS** — three copies in one datacenter (cheapest; survives drive/rack failure).
  - **ZRS** — three copies across availability zones (survives a datacenter failure).
  - **GRS / GZRS** — LRS/ZRS plus an asynchronous copy in the paired region (survives a
    regional disaster; RA- variants make the secondary readable).
- **Container** — a flat namespace of blobs (folder-LIKE prefixes such as `images/logo.png`
  are naming convention, not real directories).
- **Blob** — the object itself. Block blobs for ordinary files; page blobs back VM disks;
  append blobs for logs.

**Access tiers** trade storage price against access price: **hot** (frequent access), **cool**
(infrequent — cheaper to keep, dearer to read, 30-day minimum), **archive** (offline — hours
to rehydrate before the first byte; compliance archives and old backups). Lifecycle rules
automate demotion ("move blobs untouched for 30 days to cool").

### Who may read a blob
Everything is **private by default**. Three ways in:

1. **Account keys** — two full-power root keys per account. Fine for admin/tooling use
   (`--auth-mode key` in the CLI); never embed them in an application, rotate if exposed.
2. **Entra ID + RBAC data roles** — the production path for people and app identities:
   "Storage Blob Data Reader/Contributor" granted at account or container scope.
   Control-plane roles do NOT imply these: an Owner of the storage account still needs a
   data role (or key fallback) to read blob contents — a distinction interviewers love.
3. **SAS (shared access signature)** — a signed URL granting scoped, time-boxed rights
   ("read this one blob until Friday"). The standard answer for handing a third party or a
   browser temporary access without an identity.

Containers can also be flipped to anonymous public read (blob-level or container-level listing)
— legitimate for public assets, the default for nothing.

### Static website hosting
A storage account can serve a website straight from a special container:

```bash
az storage account create --name catalogweb123 --resource-group rg-web \
    --location centralus --sku Standard_LRS --kind StorageV2
az storage blob service-properties update --account-name catalogweb123 \
    --static-website --index-document index.html --404-document index.html
az storage blob upload-batch --account-name catalogweb123 \
    --destination '$web' --source ./dist --auth-mode key
```

Enabling the feature creates the **`$web`** container; files in it are served at
`https://<account>.z##.web.core.windows.net/` — the `--404-document index.html` trick doubles
as the classic single-page-app fallback, so client-side routes deep-link correctly. What you
get: HTTPS, massive scale, storage-priced hosting for any static build (a bundled React app is
just files). What you do NOT get: server-side code — and a static frontend calling an API on
another origin is subject to CORS, so the API must allow the site's origin. For custom
domains and edge caching, the production pairing is a CDN (or its successor, Azure Front
Door) in front of the account; Azure Static Web Apps is the newer purpose-built alternative
worth knowing by name.

### Azure SQL: server, database, tier
Provisioning has two layers: a **logical server** — not a machine, just an administrative
front door with a DNS name (`<server>.database.windows.net`, globally unique), an admin login,
and the firewall — and the **database** on it, where the money is: the **service objective**
(tier) sets the performance envelope and the bill.

```bash
az sql server create --name catalog-sql-123 --resource-group rg-data \
    --location centralus --admin-user sqladmin --admin-password '<strong-password>'
az sql db create --name CatalogDb --server catalog-sql-123 \
    --resource-group rg-data --service-objective Basic
```

Two **purchasing models**:
- **DTU model** — blended compute+IO+memory units. Tiers: **Basic** (5 DTU — tiny, ~five
  dollars a month, perfect for dev/learning), **Standard** S0+, **Premium**. Simple, capped,
  predictable.
- **vCore model** — explicit cores and memory; aligns with on-prem licensing (Hybrid
  Benefit); required for the fancier shapes. **Serverless** is a vCore variant that
  auto-scales and **auto-pauses** when idle (billing drops to storage only; the first
  request after a pause eats a resume delay — fine for dev, a surprise in a demo).

Interview line: DTU for simple/predictable small workloads, vCore for control, license reuse,
and serverless elasticity.

### The firewall: deny by default
A new Azure SQL server refuses every connection. Two standard doors:

```bash
az sql server firewall-rule create --name AllowMyIp --server catalog-sql-123 \
    --resource-group rg-data --start-ip-address 203.0.113.7 --end-ip-address 203.0.113.7
az sql server firewall-rule create --name AllowAzureServices --server catalog-sql-123 \
    --resource-group rg-data --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

The first admits one client IP (yours — for SSMS, migrations, local runs). The second is
magic, not a range: **0.0.0.0-0.0.0.0 means "allow connections originating inside Azure"** —
the standard way a web app reaches the database (coarse — it admits anyone's Azure-hosted
code as far as the network layer; credentials still gate login. Production tightens this
with VNet rules or private endpoints — name them, use the simple doors while learning).
"App can't reach the database" is a firewall rule missing until proven otherwise; error
messages even say which IP was refused.

### Connecting an application: configuration, not code
The engine speaks the same TDS protocol as any SQL Server, so an app moves by connection
string alone:

```text
Server=tcp:catalog-sql-123.database.windows.net,1433;Database=CatalogDb;
User Id=sqladmin;Password=<password>;Encrypt=true;
```

`Encrypt=true` is mandatory (Azure SQL requires TLS). An ASP.NET Core app that reads its
connection string from configuration (`GetConnectionString`) swaps databases via an
environment variable — the provider pattern maps `ConnectionStrings__Catalog` onto
`ConnectionStrings:Catalog`, so:

```powershell
$env:ConnectionStrings__Catalog = "Server=tcp:catalog-sql-123.database.windows.net,1433;..."
dotnet run
```

runs the same code against the cloud. Schema travels the same way — EF Core migrations apply
to whatever the connection string names:

```bash
dotnet ef database update --project Catalog.Data \
    --startup-project Catalog.Api --connection "<the cloud connection string>"
```

(SQL authentication with an admin login is the learning shape. The production ladder:
per-app least-privilege logins, then Entra-authenticated connections and managed identity —
no password in the string at all.)

### Monitoring the database
Where to look when "the app is slow" points at the database:

- **Metrics** (Azure Monitor): DTU/CPU percentage, storage used, deadlocks, failed
  connections. Sustained DTU at 100% = the tier is the bottleneck — scale up, or find the
  query burning it.
- **Query Performance Insight** (portal, per-database): top queries by CPU/duration/count
  over time — the "which query is eating the database" view, backed by Query Store.
- **Alerts**: metric thresholds (DTU > 80% for 15 min) notifying before users do.
- Automatic **backups** with point-in-time restore are on by default (PITR window per tier)
  — restore creates a NEW database; there is no in-place overwrite.

## Say It in an Interview
- **"How would you host a React build cheaply on Azure?"** — "Blob static website hosting:
  enable the feature on a StorageV2 account, upload the build to the `$web` container, and
  it's served over HTTPS at the account's web endpoint for storage prices. 404-to-index
  handles SPA routing; a CDN or Front Door goes in front for custom domains; and the API it
  calls must include the site's origin in its CORS policy."
- **"LRS vs GRS?"** — "Copies in one datacenter versus copies plus an async replica in the
  paired region. LRS survives hardware failure; GRS survives a regional disaster. ZRS is the
  middle: three availability zones, one region."
- **"How do apps authenticate to Blob?"** — "Never with embedded account keys. People and
  app identities get RBAC data roles like Storage Blob Data Reader — which even an account
  Owner needs, since control plane and data plane are separate — and time-boxed SAS URLs
  cover external or anonymous-ish sharing."
- **"Walk me through standing up Azure SQL for an existing app."** — "Create the logical
  server and a database at a small service objective, open two firewall doors — my IP for
  tooling and migrations, allow-Azure-services for the hosted app — run the EF migrations
  with the cloud connection string, then point the app at it purely through configuration.
  No code change if connection strings were config-driven, which is exactly why they should
  be."
- **"DTU vs vCore?"** — "DTU is a blended unit — simple, cheap, capped; Basic and Standard
  cover small predictable loads. vCore prices cores explicitly — needed for license reuse
  and for serverless, which auto-pauses idle databases at the cost of a resume delay."
- **"The new cloud DB refuses connections. First check?"** — "The server firewall — deny by
  default. Is my client IP in the rules, and if it's the deployed app failing, is
  allow-Azure-services (0.0.0.0) on? The refusal error even names the caller's IP."

## Check Yourself
1. A compliance archive must survive a regional disaster and is read maybe once a year.
   Account redundancy and blob tier?
2. Why does `az storage blob upload --auth-mode login` fail for the account's Owner, and what
   are the two fixes?
3. What exactly does the 0.0.0.0-0.0.0.0 firewall rule allow, and what does it NOT bypass?
4. Your serverless dev database "hangs" for several seconds on the first morning request.
   What is happening?
5. An app reads `GetConnectionString("Catalog")`. List the moves to point it at a new Azure
   SQL database without editing code.

**Answers:** (1) GRS (or GZRS); archive tier — hours-long rehydration is acceptable at that
read frequency. (2) Owner is control plane; blob reads are data plane — grant a Storage Blob
Data role, or fall back to `--auth-mode key`. (3) Connections originating from Azure IP space
— any tenant's; it does not bypass authentication, and it is not a substitute for VNet rules
or private endpoints in production. (4) Serverless auto-paused overnight; the first request
pays the resume. (5) Provision server+db, firewall doors, run EF migrations with
`--connection`, set `ConnectionStrings__Catalog` as an environment variable (locally or as an
app setting / Key Vault reference in the host), restart.

## Resources
- [Static website hosting in Azure Storage](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-static-website)
- [Azure Storage redundancy](https://learn.microsoft.com/en-us/azure/storage/common/storage-redundancy)
- [Azure SQL Database purchasing models](https://learn.microsoft.com/en-us/azure/azure-sql/database/purchasing-models)
- [Azure SQL Database firewall rules](https://learn.microsoft.com/en-us/azure/azure-sql/database/firewall-configure)
