# Azure App Service and Observability

## Learning Objectives
- Deploy an ASP.NET Core API to App Service: plan vs app, tiers, zip deploy, and what the
  platform now owns.
- Configure a deployed app without code changes: app settings as environment variables, the
  `__` hierarchy mapping, connection strings, and Key Vault references resolved through a
  managed identity.
- Explain the reverse-proxy consequences of PaaS hosting: TLS termination at the front end,
  forwarded headers, and why an app that redirects to HTTPS can loop without them.
- Describe scale up vs scale out, what autoscale is, and which plan tiers unlock it.
- Instrument an app with Application Insights and name the telemetry it collects with zero
  code (requests, dependencies, exceptions, live metrics).
- Triage a misbehaving deployed app with the observability ladder: log stream first, then
  App Insights failures, then metrics and alerts.

## Why This Matters
Deploying to App Service is the single most common way .NET code reaches production on Azure —
"have you deployed anything?" is a make-or-break screen question, and the credible answer
names the concrete moves: publish, deploy, app settings, connection string, logs. The
observability half is what separates "it deployed" from "I operate it": when the deployed app
500s (it will), the developer who says "I'd open the log stream, then the App Insights
failures blade" has done this before. Configuration-over-code is the through-line: the same
build must run everywhere, with every environmental difference injected from outside.

## The Concept

### Plan and app
Two resources cooperate: the **App Service plan** is the rented compute (a tier and a size —
the machine-shaped thing that costs money); the **web app** is your application, riding on a
plan. Several small apps commonly share one plan — they share its compute and its bill.

Tiers, briefly: **Free (F1)** — sleeps when idle, a daily CPU quota, no always-on; fine for
exercises and nothing else. **Basic** — dedicated compute, manual scale-out. **Standard** —
autoscale and deployment **slots** (a staging copy you deploy to, warm up, then SWAP with
production — near-zero-downtime deploys and instant rollback; know the word). **Premium** —
more of everything plus network integration.

```bash
az appservice plan create --name catalog-plan --resource-group rg-app --sku F1
az webapp create --name catalog-api-123 --resource-group rg-app \
    --plan catalog-plan --runtime "dotnet:10"
```

The app's name becomes its DNS name — `https://catalog-api-123.azurewebsites.net` — so it is
globally unique, and HTTPS exists from minute one on the platform's certificate.

### Deploying code
The lowest-common-denominator deploy, and the one every pipeline ultimately performs: publish,
zip, push.

```bash
dotnet publish -c Release -o ./publish
# zip the CONTENTS of ./publish (the app dll at the zip root, not nested)
az webapp deploy --name catalog-api-123 --resource-group rg-app \
    --src-path app.zip --type zip
```

Alternatives to recognize: `az webapp up` (convenience wrapper), CI/CD pushing the same zip,
container deploys (the app ships as an image instead). After deploy, the platform owns: web
server, TLS, OS and runtime patches, restarts, scale mechanics. You own: your code, your
configuration, your telemetry.

### Configuration: the same build everywhere
An App Service **app setting** is an environment variable injected into your process — and
ASP.NET Core's default configuration stack reads environment variables ABOVE
`appsettings.json`, with **double underscore `__` standing in for the `:` hierarchy
separator**:

| App setting name | Reaches .NET config as |
|---|---|
| `Jwt__Key` | `Jwt:Key` |
| `Cors__AllowedOrigins__0` | `Cors:AllowedOrigins[0]` (arrays = indexed entries) |
| `ConnectionStrings__Catalog` | `GetConnectionString("Catalog")` |

```bash
az webapp config appsettings set --name catalog-api-123 --resource-group rg-app \
    --settings "Cors__AllowedOrigins__0=https://catalogweb123.z19.web.core.windows.net"
```

So a well-factored app — connection strings, keys, allowed origins all read from
configuration — deploys to a new environment with ZERO code edits: the environment differs,
the build does not. Changing an app setting restarts the app (settings are process
environment; the process must be born again to see them).

**Secrets do not belong in app settings directly** — anyone with portal read access sees
them. The production pattern is a **Key Vault reference**: give the app a managed identity,
grant that identity "Key Vault Secrets User" on the vault, and set the app setting's VALUE to
a reference:

```text
ConnectionStrings__Catalog = @Microsoft.KeyVault(SecretUri=https://catalog-kv.vault.azure.net/secrets/CatalogConnectionString/)
```

The platform resolves the reference at startup using the app's identity and injects the
secret as the environment variable — the app cannot tell the difference, the portal shows a
reference instead of a password, and secret access is audited at the vault. No SDK, no code.

### Life behind a reverse proxy
App Service terminates TLS at its front end and forwards requests to your process over plain
HTTP. Your app therefore *sees* HTTP — and middleware that redirects HTTP to HTTPS
(`UseHttpsRedirection`) will redirect every request, including the ones that were already
HTTPS at the front door, looping forever. The front end tells the truth in headers
(`X-Forwarded-Proto`, `X-Forwarded-For`); ASP.NET Core consumes them natively when the
environment variable **`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`** is set (or the app adds
forwarded-headers middleware). Symptoms of forgetting: redirect loops, "too many redirects",
scheme-dependent URL generation pointing at http. This is the classic first-deploy gotcha of
any proxied PaaS.

### Scale up, scale out, autoscale
- **Scale up** — a bigger tier/size for the plan (more CPU/RAM per instance). One knob, no
  app changes, has a ceiling.
- **Scale out** — more instances of the same size behind the platform's load balancer. Needs
  the app to be effectively stateless (no per-instance session state — the reason session
  affinity and external caches exist).
- **Autoscale** — rules that move the instance count automatically ("CPU > 70% for 10
  minutes: +1 instance, max 5; scale back below 30%"). **Standard tier and above** — on Free
  and Basic the scale-out blade is the lesson: scaling is a tier feature, not a birthright.

### Application Insights
Azure's application performance monitoring (APM) service, an arm of Azure Monitor. For .NET
on App Service it attaches **codeless** — enable it (portal toggle or an
`APPLICATIONINSIGHTS_CONNECTION_STRING` app setting) and the runtime is instrumented without
touching the project; richer control comes from adding the SDK, but zero-code is the point on
day one.

What it collects, unprompted: **requests** (rate, duration, status), **dependencies**
(outbound SQL/HTTP calls with timing — this is where "the API is slow because the database
call is slow" becomes visible), **exceptions** (stack traces, grouped), plus **Live Metrics**
(a real-time firehose view) and the **application map** (services and dependencies drawn as a
graph with failure rates on the edges). The query surface underneath is KQL against the
collected telemetry; the failures and performance blades are prebuilt views of it.

### The observability ladder
When the deployed app misbehaves, climb in this order:

1. **Log stream** — live stdout/stderr of the running process:

```bash
az webapp log config --name catalog-api-123 --resource-group rg-app \
    --application-logging filesystem --level information
az webapp log tail --name catalog-api-123 --resource-group rg-app
```

   A structured logger writing to console (Serilog, the built-in logger) makes this a live
   view of the app's own narrative — startup config errors (an unresolvable Key Vault
   reference, a bad connection string) surface HERE first.
2. **App Insights failures blade** — which operations fail, at what rate, with which
   exceptions and dependency errors — after the fact, aggregated.
3. **Metrics + alerts** — Azure Monitor trends on requests, response time, CPU/memory; alert
   rules so the threshold breach pages a human before users report it.

Interview shorthand: logs answer "what did my code say", App Insights answers "what is
failing and why", metrics answer "how is it trending".

## Say It in an Interview
- **"Walk me through deploying an ASP.NET Core API to Azure."** — "App Service: create a plan
  and a web app on the runtime I target, `dotnet publish`, zip, `az webapp deploy`. Then
  configuration, not code: app settings for environment differences, the connection string
  and any keys as Key Vault references resolved by the app's managed identity, forwarded
  headers enabled because TLS terminates at the platform's proxy. Same build, injected
  environment."
- **"How does your deployed app get configuration and secrets?"** — "App settings surface as
  environment variables — double underscore maps the config hierarchy, and ASP.NET Core
  reads environment above appsettings.json. Secrets are Key Vault references: the platform
  resolves them at startup through the app's managed identity, so no secret sits in portal
  config, code, or the repo."
- **"Why did your app redirect-loop after the first deploy?"** — "TLS terminates at App
  Service's front end, so the app sees plain HTTP and https-redirect middleware loops. The
  fix is honoring X-Forwarded-Proto — on App Service, setting
  ASPNETCORE_FORWARDEDHEADERS_ENABLED=true."
- **"Scale up vs scale out?"** — "Up is a bigger box — simple, has a ceiling. Out is more
  boxes behind the load balancer — needs stateless instances, and it's what autoscale drives
  from metric rules. Autoscale starts at Standard tier; slots for zero-downtime swap deploys
  live there too."
- **"Production is 500ing. What do you actually do?"** — "Log stream first — live console of
  the process, where startup and config failures announce themselves. Then App Insights:
  the failures blade for the failing operation and its exception, dependencies for whether
  it's really the database or a downstream call. Metrics and alerts close the loop so the
  next one pages us instead of surprising us."

## Check Yourself
1. Setting `Cors__AllowedOrigins__0` as an app setting changed nothing until the app
   restarted. Why — and what does the platform actually do when you save a setting?
2. Write the app-setting name that populates `builder.Configuration["Jwt:Key"]`, and describe
   its value if the key must live in Key Vault.
3. Your F1-tier exercise app has no autoscale blade options. Feature gap or your mistake?
4. The app works over HTTP locally, redirect-loops on App Service. Diagnose.
5. Which telemetry surface shows that 40% of request time is one SQL query — and which shows
   the exception your app just threw on startup?

**Answers:** (1) Settings are environment variables — the process reads them at birth; saving
a setting triggers the platform to restart the app (which is also why it appeared to "change
nothing" only if the restart hadn't completed). (2) `Jwt__Key`; value
`@Microsoft.KeyVault(SecretUri=<secret URI>)`, resolvable because the app's managed identity
holds Key Vault Secrets User on that vault. (3) Neither gap nor mistake: autoscale is
Standard+; Free teaches the tier lesson. (4) TLS terminates at the front end, app sees HTTP,
`UseHttpsRedirection` loops — enable forwarded headers
(`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`). (5) App Insights dependencies (via the
performance blade / application map); the log stream — startup exceptions print to console
before App Insights has anything to aggregate.

## Resources
- [App Service overview](https://learn.microsoft.com/en-us/azure/app-service/overview)
- [Configure apps: app settings](https://learn.microsoft.com/en-us/azure/app-service/configure-common)
- [Key Vault references for App Service](https://learn.microsoft.com/en-us/azure/app-service/app-service-key-vault-references)
- [Application Insights overview](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
