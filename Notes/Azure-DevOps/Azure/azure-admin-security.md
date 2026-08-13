# Azure Administration and Security

## Learning Objectives
- Choose the right management surface for a job — portal, Azure CLI (`az`), Azure PowerShell —
  and explain why all three are equivalent (one ARM API underneath).
- Explain how Azure organizes resources: management groups, subscriptions, resource groups,
  resources — and why the resource group is the unit of lifecycle.
- Describe identity in Azure: Microsoft Entra ID (tenants, users, service principals, managed
  identities) and how it differs from on-prem Active Directory.
- Apply role-based access control (RBAC): role + principal + scope, inheritance, and the
  directory-roles-vs-resource-roles distinction that trips people up in interviews.
- Read and write network security group (NSG) rules: stateful, priority-ordered,
  first-match-wins.
- Use Azure Key Vault for secrets, keys, and certificates, and explain why a managed identity
  plus Key Vault beats a connection string in a config file.
- Set up cost management: tags, budgets, and the crucial caveat that a budget alert notifies —
  it does not stop spending.

## Why This Matters
Administration and security questions are where cloud interviews separate "deployed a demo
once" from "worked in a real subscription". Real teams live under RBAC (you WILL hit a
"forbidden" error your first week and need to read a role assignment), keep secrets in Key
Vault because a leaked connection string in a git repo is a resume-generating event, and treat
cost management as everyone's job. The distinctions in this note — directory roles vs resource
roles, stateful NSG rules, notify-not-stop budgets — are precisely the ones interviewers use as
depth probes.

## The Concept

### Three doors, one API
Everything in Azure is managed through the **Azure Resource Manager (ARM)** REST API. The three
clients are interchangeable views of it:

- **Azure portal** — the web UI. Best for exploration, one-off inspection, and reading metrics;
  worst for repeatability (clicks do not go in version control).
- **Azure CLI** — `az`, a cross-platform command-line client. Scriptable, greppable,
  copy-pasteable into runbooks; the standard choice for automation in shell scripts and CI
  pipelines.

```bash
az login                          # browser-based sign-in
az account show                   # which subscription am I pointed at?
az account set --subscription "Contoso-Dev"   # explicit beats implicit
az group create --name rg-catalog --location eastus
```

- **Azure PowerShell** — the `Az` module (`Connect-AzAccount`, `New-AzResourceGroup`). Same
  operations, PowerShell-object output; preferred in Windows-heavy ops teams.

Because it is one API, anything you click in the portal exists as a CLI command and vice versa
— a good learning technique is to do a thing in the portal once, then find the `az` command
that does it repeatably. (Further along that same axis is infrastructure-as-code — ARM/Bicep
templates or Terraform — where the desired state itself goes in version control.)

### How Azure organizes resources
Top to bottom:

- **Management group** — groups subscriptions so policy can be applied across them (enterprise
  scale; you may never touch one on a small team).
- **Subscription** — a billing and access boundary. Companies commonly run separate dev/test
  and production subscriptions so a runaway dev experiment cannot spend production money.
- **Resource group (RG)** — a folder for resources that live and die together. Deleting the
  group deletes everything in it, which makes "one exercise, one resource group" the standard
  hygiene for learning and prototyping: `az group delete` is the guaranteed-clean-slate,
  guaranteed-nothing-left-billing move.
- **Resource** — the actual thing: a VM, a database, a storage account.

RBAC and policy assigned at any level **inherit downward** through this hierarchy.

### Identity: Microsoft Entra ID
**Microsoft Entra ID** (renamed from Azure Active Directory / Azure AD in 2023 — interviewers
use both names) is Azure's cloud identity service. A **tenant** is one organization's directory:
its users, groups, and app registrations. Every Azure subscription trusts exactly one tenant to
authenticate its users.

Despite the old name, it is not on-prem Active Directory in the cloud: no domain controllers,
no Group Policy, no LDAP/Kerberos as the primary protocols. It speaks modern web protocols
(OAuth 2.0, OpenID Connect, SAML) — the same token-based world as the JWT authentication in a
typical web API, one level up.

Identities that are not humans matter just as much:
- **Service principal** — an identity for an application or pipeline; it has credentials (a
  secret or certificate) that you must store and rotate.
- **Managed identity** — a service principal that Azure creates for a resource (a web app, a
  VM) and whose credentials **Azure itself manages — no secret ever exists for you to store,
  leak, or rotate**. When a web app with a managed identity calls Key Vault, it authenticates
  as itself. Managed identity is the modern answer to "how does my app log into other Azure
  services"; reach for it before any solution that involves storing a credential.

### RBAC: role, principal, scope
Every access grant in Azure is a triple:

> **principal** (who: user, group, service principal, managed identity)
> gets **role** (a named bundle of allowed actions)
> at **scope** (where: management group, subscription, resource group, or one resource).

```bash
az role assignment create \
  --assignee dev@contoso.com \
  --role "Contributor" \
  --scope /subscriptions/<id>/resourceGroups/rg-catalog
```

The four generic roles to know cold: **Owner** (everything, including granting access),
**Contributor** (create/modify/delete resources, but NOT grant access), **Reader** (look, don't
touch), **User Access Administrator** (manage access only). Beyond those, hundreds of built-in
service-specific roles exist (for example "Key Vault Secrets User" — read secrets, nothing
else); prefer the narrowest role that works — least privilege.

Assignments inherit down: Contributor on the subscription is Contributor on every resource
group and resource inside it. Grant at the narrowest scope that does the job.

**The interview trap: directory roles vs resource roles.** Entra ID has its own role system
(Global Administrator, User Administrator, ...) governing the *directory* — users, groups, app
registrations. Azure RBAC governs *resources* — VMs, databases, storage. They are separate
systems, and neither implies the other: a Global Administrator does **not** automatically have
access to any subscription's resources (there is a break-glass "elevate access" toggle for
emergencies, which is audited and expected to be exceptional), and an Owner of a subscription
may have no directory rights at all. Some data-plane authorization ALSO piggybacks on RBAC
(storage data roles like "Storage Blob Data Contributor", Key Vault's RBAC mode below) — being
able to MANAGE a resource (control plane) is distinct from being able to read its DATA (data
plane), and several services enforce the two with different role sets.

### Network security groups
An **NSG** is a stateful packet filter attached to a subnet or a VM's network interface. Rules
have: priority (a number — **lower number = evaluated first**), direction (inbound/outbound),
source/destination, port, protocol, and allow/deny. Evaluation is **first match wins**: the
lowest-priority-number rule that matches the packet decides, and nothing later is consulted.

**Stateful** means return traffic is automatic: if an inbound rule allows a request in, the
response flows out without any outbound rule — the NSG tracks the connection. You never write
"allow responses" rules.

Every NSG ends with default rules (priority 65000+): allow traffic inside the virtual network,
allow Azure's load balancer probes, and **deny all other inbound**. So the security posture is
deny-by-default, and your rules are exceptions carved into it. A classic real-world review
finding is an "allow 22/3389 from Internet" rule someone added while debugging and never
removed.

(Azure SQL's server firewall, covered in the storage-and-SQL note, is the same allow-listing
idea applied at a PaaS service's front door rather than at a network interface.)

### Key Vault
**Azure Key Vault** stores three kinds of things, each with a different job:
- **Secrets** — arbitrary sensitive strings (connection strings, API keys). Read/write like a
  tiny key-value store; every secret is versioned.
- **Keys** — cryptographic keys that **never leave the vault**: you send data in, the vault
  signs/encrypts/decrypts inside itself (optionally inside an HSM — a hardware security
  module). You can use the key; you cannot exfiltrate it.
- **Certificates** — TLS certificate lifecycle: storage, and automated renewal.

Access control has two modes: legacy **access policies** (per-vault lists) and **Azure RBAC
mode** (the modern default — grant "Key Vault Secrets User" at vault scope like any other role
assignment; data-plane roles must be granted explicitly even to the vault's creator, a classic
first-hour surprise). Vaults also have **soft delete**: a deleted vault (or secret) is
recoverable for a retention window, and its NAME stays reserved until purged — the second
first-hour surprise, when a delete-and-recreate hits "name already exists".

The pattern that matters — **no secret in code, config, or repo**:

> An order-service web app needs a database password. The password lives in Key Vault as a
> secret. The app has a managed identity; that identity is granted "Key Vault Secrets User" on
> the vault; at startup the app (or the hosting platform, via a Key Vault *reference* in its
> configuration) fetches the secret over an authenticated call. Nothing sensitive is in
> `appsettings.json`, nothing in the repo, nothing to rotate when a laptop is stolen — and
> secret access is itself audited in the vault's logs.

### Cost management and billing
Cost discipline is administrative hygiene, and Azure gives you three tools:

- **Tags** — key/value labels on resources and groups (`env=dev`, `team=fulfillment`,
  `costcenter=1234`). Cost analysis can then slice spend by tag — which is how "whose money is
  this?" gets answered in a shared subscription.
- **Cost analysis** — the portal's spend explorer: actual and forecast cost by service, region,
  RG, tag, over time. Check it the morning after any provisioning session.
- **Budgets** — a threshold on a scope (subscription or RG) with alert rules ("email at 80%,
  100%, forecast-to-exceed"). **A budget notifies; it does not stop or cap anything.** Azure
  has no global "spend limiter" for pay-as-you-go accounts — the guard rails are alerts plus
  your own tear-down discipline (and RBAC keeping provisioning rights narrow). Say this
  precisely in interviews; "the budget will stop the spend" is a known wrong answer.

## Say It in an Interview
- **"Portal, CLI, or PowerShell — which do you use?"** — "They all drive the same ARM API, so
  it's fitness-for-purpose: portal to explore and read metrics, `az` CLI for anything I want
  repeatable or in a pipeline, PowerShell in Windows-centric ops shops. If I click something
  twice I go find the CLI command for it."
- **"Explain RBAC."** — "Every grant is a principal, a role, and a scope — who, what actions,
  where. Assignments inherit down from management group through subscription and resource group
  to resource, so you grant the narrowest role at the narrowest scope. Owner adds access
  management on top of Contributor; Reader is look-don't-touch."
- **"Is a Global Administrator all-powerful in Azure?"** — "No — that's a directory role.
  Entra ID roles govern users and groups; Azure RBAC governs resources; neither implies the
  other. A Global Admin has no resource access until someone assigns it — there's only an
  audited emergency elevate-access path."
- **"How does your app get its database password?"** — "It doesn't store one. The app runs
  with a managed identity, the password sits in Key Vault, and the identity is granted just
  Key-Vault-Secrets-User on that vault. No credential in code, config, or the repo, nothing to
  rotate by hand, and access is audited."
- **"What happens when an NSG rule conflicts with another?"** — "Rules evaluate by priority,
  lowest number first, first match wins. And NSGs are stateful — allowed inbound traffic gets
  its responses out automatically; you never write return-traffic rules."
- **"Will an Azure budget stop you from overspending?"** — "No — budgets alert, they never
  stop spend. Real cost control is tags for ownership, alerts for early warning, least
  privilege on who can provision, and tearing down what you're done with."

## Check Yourself
1. A pipeline needs to deploy into one resource group and must not be able to grant anyone
   access. What identity type and which role, at what scope?
2. Your NSG has "deny all inbound from Internet" at priority 200 and "allow 443 from Internet"
   at priority 100. Is the web reachable?
3. Why is a managed identity safer than a service principal with a client secret?
4. You delete a Key Vault to recreate it with the same name and the create fails. Why?
5. Which pair is a real distinction: (a) Entra roles vs RBAC roles, (b) budgets that alert vs
   budgets that cap? Explain the false one.

**Answers:** (1) A service principal (or the agent's managed identity where supported) with
**Contributor** scoped to that one resource group — Contributor cannot create role assignments;
Owner could. (2) Yes — 100 evaluates before 200, first match wins, and statefulness returns the
responses. (3) There is no credential to store, leak, expire, or rotate — Azure issues and
rotates it internally. (4) Soft delete keeps the deleted vault (and its name) reserved for the
retention window; recover it or purge it first. (5) (a) is real. (b) is false: Azure budgets
only ever alert — no pay-as-you-go spend cap exists.

## Resources
- [What is Azure RBAC?](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview)
- [Network security groups](https://learn.microsoft.com/en-us/azure/virtual-network/network-security-groups-overview)
- [About Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/overview)
- [Managed identities for Azure resources](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview)
