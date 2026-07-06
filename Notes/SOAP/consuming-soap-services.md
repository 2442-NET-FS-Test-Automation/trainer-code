# Consuming a SOAP Service: WSDL-First Clients

## Learning Objectives
- Describe the standard workflow for consuming any SOAP service: obtain the WSDL, generate a typed
  client, call methods, catch faults.
- Generate a .NET client with `dotnet-svcutil` (or Visual Studio's Connected Services) and read what it
  produced.
- Explore an unfamiliar SOAP service with tooling (SoapUI, Postman) before writing code.
- Explain the fallback — hand-posting an envelope with `HttpClient` — and when it is (rarely) the right
  call.

## Why This Matters
You will meet SOAP as a **consumer**, not an author: the payment processor, the shipping-rate service,
the government filing system, the twenty-year-old internal inventory feed — they publish a WSDL and you
integrate with it. That is the realistic assignment, and it is friendlier than it looks: SOAP was
*designed* for machine-generated clients, so the workflow is more mechanical than a typical REST
integration — no docs-reading-and-guessing, no hand-built request models. Knowing the four-step routine
is the familiarity employers actually mean when a posting says "experience with SOAP/web services."

## The Concept

### The universal workflow
Consuming any SOAP service, in any language, is the same four steps:

1. **Get the WSDL** — usually the endpoint URL plus `?wsdl`. This is the machine-readable contract:
   every operation, every parameter, every type (`soap-vs-rest.md`).
2. **Generate a typed client** from it — every SOAP-era platform ships a generator (.NET:
   `dotnet-svcutil`; Java: `wsimport`; etc.). The generator writes the proxy classes, the request/response
   types, and all the envelope plumbing.
3. **Call methods on the proxy** — it looks like an ordinary local class; the envelope, encoding, and
   HTTP POST from `soap-message-anatomy.md` happen inside the generated code.
4. **Catch `FaultException`** — the `<Fault>` in a response Body surfaces as a typed exception, not a
   status code you inspect.

Contrast with REST: there you read documentation, hand-write DTOs, and hope they match. Here the contract
generates the code — SOAP's tooling story is the whole reason enterprises accepted its verbosity.

### Step 0: explore before you code
Given an unfamiliar service, do not start in C#. Point **SoapUI** (the classic SOAP workbench) at the
WSDL URL — it lists every operation and pre-builds a skeleton request envelope for each; fill in values,
send, read the response. **Postman** imports WSDLs too. Ten minutes of this answers the questions code
cannot ask yet: Which operation do I actually need? What does a real response look like? What faults come
back for bad input? (This is the same reconnaissance habit as hitting a REST endpoint with curl before
writing the client.)

### Generating the .NET client
`dotnet-svcutil` is the .NET generator (successor to WCF's `svcutil.exe`):

```bash
dotnet tool install --global dotnet-svcutil
dotnet-svcutil http://partner.example.com/soap/catalog.asmx?wsdl --outputDir GeneratedClients
```

It reads the WSDL and emits a `Reference.cs` containing, for the catalog service from `soap-vs-rest.md`:

- an **interface** mirroring the service contract (`ICatalogSoapService` with `GetProductBySkuAsync`...),
- a **client class** (`CatalogSoapServiceClient`) that implements it over a channel,
- **data classes** for every WSDL type (`ProductContract` with `Sku`, `Name`, `Price`, `CurrentStock`).

Visual Studio's *Connected Services > WCF Web Service Reference* is the same generator behind a dialog.
Either way, add the `System.ServiceModel.*` NuGet packages the generated code needs (the generator's
output tells you which). Regenerate when the partner's contract changes — the WSDL is the source of
truth; never hand-edit `Reference.cs`.

### Calling it
The generated client is ordinary C#:

```csharp
var client = new CatalogSoapServiceClient(
    CatalogSoapServiceClient.EndpointConfiguration.BasicHttpBinding_ICatalogSoapService);

try
{
    ProductContract? product = await client.GetProductBySkuAsync("WIDGET-001");
    if (product is null)
        Console.WriteLine("No such SKU.");                    // empty result = data, not an error
    else
        Console.WriteLine($"{product.Name}: {product.CurrentStock} in stock");
}
catch (FaultException ex)         // the <Fault> element, surfaced as an exception
{
    Console.WriteLine($"Service fault: {ex.Message}");        // e.g. "SKU must be supplied."
}
catch (CommunicationException ex) // transport-level failure - unreachable, timeout, bad envelope
{
    Console.WriteLine($"Transport failure: {ex.Message}");
}
finally
{
    await client.CloseAsync();    // channels want an explicit close
}
```

Three things to notice. The **binding** (`BasicHttpBinding`) is the generated client's bundle of
transport decisions — SOAP version, HTTP vs HTTPS, timeouts; it comes from the WSDL, and it is where you
tweak settings if needed (larger message size, credentials). The **two exception types** mirror the
anatomy note's layers: `FaultException` = the *service* said no (a fault inside a well-formed envelope);
`CommunicationException` = the *transport* failed (nothing meaningful came back). And the proxy is
**IDisposable-ish but not quite** — WCF-lineage clients should be closed explicitly rather than trusted
to a bare `using`, because `Dispose` on a faulted channel can throw and mask the original error.

### The fallback: raw HttpClient
No generator, or a one-off call in a constrained environment? A SOAP call is ultimately just an HTTP
POST (`soap-message-anatomy.md`), so `HttpClient` can do it by hand:

```csharp
const string envelope = """
    <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
      <soap:Body>
        <GetProductBySku xmlns="http://example.local/fulfillment">
          <sku>WIDGET-001</sku>
        </GetProductBySku>
      </soap:Body>
    </soap:Envelope>
    """;

var content = new StringContent(envelope, Encoding.UTF8, "text/xml");
content.Headers.Add("SOAPAction",
    "\"http://example.local/fulfillment/ICatalogSoapService/GetProductBySku\"");

var response = await http.PostAsync("http://partner.example.com/soap/catalog.asmx", content);
var xml = XDocument.Parse(await response.Content.ReadAsStringAsync());
// walk xml for <GetProductBySkuResult> - or for <Fault>, since a fault may arrive as HTTP 500
```

Now *you* own everything the generator handled: envelope correctness, namespaces, the `SOAPAction`
header, parsing responses **and faults** by hand, and keeping up when the contract changes. Legitimate
for a single hardwired call or for debugging (it shows exactly what crosses the wire); the wrong default
for a real integration. Know it exists, know why the generated client wins.

### Consumer's mental checklist
Meeting a SOAP integration at work, in order: Where is the WSDL? Explore it in SoapUI first. Generate the
client — never hand-write contract types. Which binding/security does the partner require (plain HTTPS?
WS-Security header?) — that is binding configuration, not your business code. Handle `FaultException`
(their "no") separately from `CommunicationException` (couldn't ask). Regenerate on contract change.

## Say It in an Interview
- *"Consuming SOAP is a standard routine: grab the WSDL, generate a typed client — `dotnet-svcutil` in
  .NET — call the proxy like a local class, catch `FaultException`. The contract generates the code;
  that's SOAP's tooling advantage over hand-writing REST DTOs."*
- *"Before coding I'd point SoapUI at the WSDL — it builds skeleton requests for every operation, so I
  can see real responses and faults in minutes."*
- *"Two failure layers when calling: `FaultException` means the service processed my envelope and said
  no; `CommunicationException` means transport failed and nothing meaningful came back. Different
  handling."*
- *"You *can* hand-post an envelope with HttpClient — it's just an HTTP POST with a SOAPAction header —
  and it's useful for debugging, but for a real integration the generated client owns the plumbing and
  tracks contract changes."*

## Check Yourself
1. The four steps for consuming any SOAP service, in order?
2. A partner hands you a WSDL URL. What do you do before writing any C#?
3. What does `dotnet-svcutil` generate, and why must you never hand-edit its output?
4. `FaultException` vs `CommunicationException` — what does each mean, and why handle them separately?
5. When is raw `HttpClient` + hand-built envelope the right choice, and what four responsibilities do
   you take on?
6. The partner adds a required parameter to an operation. What is the consumer-side fix?

**Answers:** (1) Get the WSDL; generate a typed client; call the proxy's methods; catch faults.
(2) Explore it in SoapUI/Postman — list operations, send skeleton requests, see real responses and
faults. (3) A proxy interface, a client class, and data classes for every contract type — all derived
from the WSDL; hand-edits are lost on regeneration and drift from the contract. (4) `FaultException`:
the service received a valid envelope and returned a structured `<Fault>` — a business/validation "no".
`CommunicationException`: the transport failed — unreachable, timeout, malformed exchange — you learned
nothing about the operation. One is the partner's answer, the other is no answer. (5) One-off hardwired
calls or wire-level debugging; you own envelope correctness, namespaces/SOAPAction, fault parsing, and
contract drift. (6) Regenerate the client from the updated WSDL — the compiler then points at every call
site that must change.

## Summary
- Consumers, not authors: the training's SOAP posture is *integrate with what exists*.
- Universal routine: WSDL -> generate typed client -> call proxy -> catch `FaultException`.
- Explore first: SoapUI/Postman turn a WSDL into ready-to-send skeleton requests.
- .NET generator: `dotnet-svcutil` (CLI) or VS Connected Services — proxy, client, and types from the
  contract; regenerate on change, never hand-edit.
- Two failure layers: `FaultException` (service said no) vs `CommunicationException` (transport failed);
  close WCF-lineage clients explicitly.
- Raw `HttpClient` POST works — envelope + `SOAPAction` — right for debugging and one-offs, wrong default
  for real integrations.

## Resources
- [dotnet-svcutil (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/core/additional-tools/dotnet-svcutil-guide)
- [WCF Web Service Reference provider (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/core/additional-tools/wcf-web-service-reference-guide)
- [SoapUI — getting started with SOAP testing](https://www.soapui.org/docs/soap-and-wsdl/)
