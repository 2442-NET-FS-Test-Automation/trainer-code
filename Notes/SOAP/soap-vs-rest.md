# SOAP Services and the REST-vs-SOAP Comparison

## Learning Objectives
- Describe SOAP: envelopes, WSDL contracts, and faults.
- Expose a SOAP service in ASP.NET Core with SoapCore: `[ServiceContract]`, `[OperationContract]`,
  `[DataContract]`, `FaultException`.
- Compare RESTful and SOAP-based services on functionality, performance, and scalability.

## Why This Matters
SOAP is the enterprise integration style that predates REST, and it is *not dead*: banking, insurance,
government, and logistics systems speak it daily, and "can you consume/expose a SOAP service?" is a real
work assignment. Mounting a SOAP endpoint beside a REST surface — over the **same EF context** — makes
the deeper point economically: transport style and business logic are separate decisions. The
REST-vs-SOAP contrast is also a stock interview comparison; the working knowledge is the differentiator.

## The Concept

### What SOAP is
SOAP (Simple Object Access Protocol) is an XML messaging protocol: every request and response is an
**envelope** (`<Envelope><Header/><Body>...</Body></Envelope>`), the service publishes a machine-readable
contract — the **WSDL** (Web Services Description Language) document listing every operation and type —
and errors travel as structured **faults** inside the envelope. Where REST is a *style* over plain HTTP,
SOAP is a *protocol* with formal standards stacked on top (WS-Security, WS-Addressing, transactions),
which is exactly why contract-heavy enterprises adopted it.

### Exposing one beside a REST surface
SoapCore is the community library that mounts SOAP endpoints in ASP.NET Core. The contract is an
attributed interface:

```csharp
[ServiceContract(Namespace = "http://example.local/fulfillment")]
public interface ICatalogSoapService
{
    [OperationContract] int GetInventoryCount();
    [OperationContract] ProductContract? GetProductBySku(string sku);
    [OperationContract] ProductContract[] GetLowStock(int threshold);
}

[DataContract]
public class ProductContract
{
    [DataMember] public string Sku  { get; set; } = string.Empty;
    [DataMember] public string Name { get; set; } = string.Empty;
    [DataMember] public decimal Price { get; set; }
    [DataMember] public int CurrentStock { get; set; }
}
```

`[ServiceContract]`/`[OperationContract]` declare the callable surface; `[DataContract]`/`[DataMember]`
declare the serializable types (only marked members cross the wire). The implementation is ordinary DI-fed
code over the same `IDbContextFactory` the REST endpoints use, and faults are thrown, not status-coded:

```csharp
public ProductContract? GetProductBySku(string sku)
{
    if (string.IsNullOrWhiteSpace(sku))
        throw new FaultException("SKU must be supplied.");   // -> a <Fault> element in the envelope
    ...
    return row is null ? null : ToContract(row);             // not-found -> null result, NOT a fault
}
```

(The design call mirrors REST manners: a *bad request* is a fault, like a 400; an *empty result* is data,
like a 404-vs-null-body decision.) Wiring it up:

```csharp
builder.Services.AddSoapCore();
builder.Services.AddScoped<ICatalogSoapService, CatalogSoapService>();

((IApplicationBuilder)app).UseSoapEndpoint<ICatalogSoapService>(
    "/soap/catalog.asmx", new SoapEncoderOptions(), SoapSerializer.DataContractSerializer);
// the cast pins an overload: WebApplication implements both IApplicationBuilder and
// IEndpointRouteBuilder, so UseSoapEndpoint is otherwise ambiguous
```

Browse `/soap/catalog.asmx?wsdl` and the generated WSDL lists every operation and type — hand that URL to
any SOAP tooling (SoapUI, WCF's `svcutil`, most enterprise platforms) and it generates a working client.
That contract-first tooling story is SOAP's enduring superpower.

### REST vs SOAP (the comparison)

| | REST | SOAP |
|---|---|---|
| Nature | architectural style over HTTP | protocol with formal standards |
| Payload | usually JSON — lightweight, human-readable | XML envelopes — verbose, schema-validated |
| Contract | informal (OpenAPI/Swagger by convention) | **WSDL** — formal, machine-enforced, tooling-generated clients |
| Errors | HTTP status codes | `FaultException` -> structured `<Fault>` |
| Verbs/transport | full HTTP verb semantics; HTTP only by convention | everything is POST; can ride other transports |
| Caching/scale | HTTP caching + statelessness scale horizontally cheaply | little transport caching; heavier per-message |
| Security/rigor | TLS + tokens (JWT/OAuth) | WS-Security: message-level signing/encryption, formal policy |
| Fits | public web APIs, mobile/SPA backends, microservices | formal enterprise integration, regulated B2B, legacy estates |

One-sentence version: *REST is lightweight (JSON over HTTP, cacheable, scales horizontally); SOAP is
heavier (XML, strict WSDL contracts, built-in standards) and suits formal enterprise transactions.* And
the architectural point worth repeating in your own vocabulary: the business logic does not change when
you add a protocol — protocols are skins over a service, which is why the service-layer seam
(`../06-aspnet-core/dtos-service-layer-automapper.md`) matters more than any one transport.

## Say It in an Interview
- *"SOAP is an XML messaging protocol: envelopes on the wire, a WSDL as the formal machine-readable
  contract, and structured faults for errors. REST is a style over HTTP; SOAP is a protocol with
  standards stacked on top — WS-Security, transactions."*
- *"In ASP.NET Core I'd expose it with SoapCore: a `[ServiceContract]` interface with
  `[OperationContract]` methods, `[DataContract]` types, thrown `FaultException`s, one
  `UseSoapEndpoint` call — over the same DI and data layer the REST endpoints use."*
- *"SOAP's enduring superpower is contract-first tooling: point SoapUI or `svcutil` at `?wsdl` and get a
  generated client."*
- *"The comparison in one line: REST is lightweight JSON over HTTP, cacheable and horizontally scalable;
  SOAP is heavier XML with strict WSDL contracts and built-in standards, suited to formal enterprise
  transactions."*

## Check Yourself
1. Name the three structural pillars of SOAP and what each does.
2. In a SOAP service, when do you throw a `FaultException` vs return `null` — and what are the REST
   analogs?
3. What do `[DataContract]`/`[DataMember]` control, and what happens to unmarked members?
4. Why can adding a SOAP endpoint to an existing REST service require zero changes to business logic?
5. A partner bank requires message-level signing and a formal machine-validated contract. REST or SOAP,
   and which two features decide it?
6. What does `?wsdl` give an integration partner, concretely?

**Answers:** (1) The envelope (XML message structure), the WSDL (formal contract of operations and
types), and faults (structured errors inside the envelope). (2) Fault for a bad request (the caller
erred — REST's 400); `null`/empty result for no-match (a data outcome — REST's 404-vs-null decision).
(3) Which types and members serialize across the wire; unmarked members never leave the process.
(4) Protocols are skins: the SOAP service is another consumer of the same service/data layer via DI —
transport and logic are separate decisions. (5) SOAP — WS-Security (message-level signing/encryption) and
the WSDL contract. (6) A machine-readable description of every operation and type, from which their
tooling generates a working, typed client.

## Summary
- SOAP = XML envelopes + WSDL contracts + faults; a protocol, not a style — still the lingua franca of
  formal enterprise integration.
- SoapCore mounts it in ASP.NET Core: `[ServiceContract]`/`[OperationContract]` interface,
  `[DataContract]` types, `FaultException` for errors, one `UseSoapEndpoint` call.
- `?wsdl` gives any SOAP toolchain enough to generate a client — contract-first tooling is the selling
  point.
- Know the comparison table cold; know that both protocols can serve the same data layer unchanged.

## Resources
- [SoapCore (GitHub)](https://github.com/DigDes/SoapCore)
- [WSDL specification primer (W3C)](https://www.w3.org/TR/wsdl20-primer/)
- [SOAP vs REST (AWS comparison)](https://aws.amazon.com/compare/the-difference-between-soap-rest/)
