# SOAP Message Anatomy: Envelope, Header, Body, Fault, Encoding, Transport

## Learning Objectives
- Draw the structure of a SOAP message: Envelope as the root, optional Header, mandatory Body.
- Explain what belongs in the Header (cross-cutting metadata) vs the Body (the operation payload).
- Read and write a SOAP Fault, and map its parts to the exception you threw on the server.
- Describe how a SOAP message is encoded (XML, document/literal, the serializer's role) and how it
  travels (transport-agnostic, in practice HTTP POST).

## Why This Matters
`../07-soap/soap-vs-rest.md` treats SOAP from the outside: what it is, how to mount it, when to pick it.
This note opens the message itself. When a partner integration breaks, what you actually get is an XML
envelope in a log — and the difference between "I can read this" and "I cannot" is knowing which element
is structure, which is metadata, which is payload, and which is the error. It is also the layer where
SOAP's design makes sense: every rule (everything is POST, faults ride inside the message, security lives
in the Header) follows from one decision — *the message, not the transport, carries everything*.

## The Concept

### Message structure: one shape for every message
Every SOAP message — request, response, or error — is the same three-layer XML document:

```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Header>       <!-- OPTIONAL: cross-cutting metadata -->
    ...
  </soap:Header>
  <soap:Body>         <!-- MANDATORY: the payload (or a Fault) -->
    ...
  </soap:Body>
</soap:Envelope>
```

There is no third shape. A request is an Envelope whose Body holds an operation call; a response is an
Envelope whose Body holds the result; an error is an Envelope whose Body holds a `<Fault>`. That
uniformity is why generic SOAP tooling works: everything on the wire is the same document type, validated
by the same schema.

### Envelope: the root and the version marker
The `<Envelope>` is the root element — nothing exists outside it — and its **namespace declares the SOAP
version**:

| Version | Envelope namespace | Content type |
|---|---|---|
| SOAP 1.1 | `http://schemas.xmlsoap.org/soap/envelope/` | `text/xml` |
| SOAP 1.2 | `http://www.w3.org/2003/05/soap-envelope` | `application/soap+xml` |

The `soap:` prefix is arbitrary (any prefix bound to the right namespace works); the namespace URI is
what tooling checks. Version mismatches are a classic integration failure: a 1.1 client posting to a
1.2-only endpoint gets a version-mismatch fault, and the fix is in the namespace and content type, not
the payload.

### Header: optional, out-of-band, cross-cutting
The `<Header>` carries metadata *about* the message that is not part of the operation: security tokens,
addressing/routing, transaction context, correlation ids. This is where the WS-* standards physically
live — WS-Security puts its signed tokens in header blocks, WS-Addressing its `<To>`/`<Action>` elements:

```xml
<soap:Header>
  <wsse:Security soap:mustUnderstand="1"
      xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">
    <wsse:UsernameToken>
      <wsse:Username>fulfillment-partner</wsse:Username>
      ...
    </wsse:UsernameToken>
  </wsse:Security>
</soap:Header>
```

Two attributes give headers their power. **`mustUnderstand="1"`** says: if the receiver does not know how
to process this block, it must fault rather than silently ignore it — how a client *forces* the server to
honor security instead of skipping what it doesn't recognize. **`actor`** (1.1) / **`role`** (1.2) targets
a block at a specific intermediary, so a message routed through hops can carry instructions addressed to
each. The REST analog: the Header plays the role HTTP headers play (`Authorization`, correlation ids) —
except it is inside the message, so it survives any transport and can be signed along with the payload.

### Body: the payload
The `<Body>` is the one mandatory child, and it carries the operation. By convention (document/literal
style — see Encoding) the Body's child element is named for the operation and its children are the
parameters, all in the service's own namespace:

```xml
<!-- request -->
<soap:Body>
  <GetProductBySku xmlns="http://example.local/fulfillment">
    <sku>WIDGET-001</sku>
  </GetProductBySku>
</soap:Body>

<!-- response -->
<soap:Body>
  <GetProductBySkuResponse xmlns="http://example.local/fulfillment">
    <GetProductBySkuResult>
      <Sku>WIDGET-001</Sku>
      <Name>Widget</Name>
      <Price>9.99</Price>
      <CurrentStock>42</CurrentStock>
    </GetProductBySkuResult>
  </GetProductBySkuResponse>
</soap:Body>
```

Those element names are not guessed — the **WSDL defines them**, which is why generated clients never
misspell an operation. On the server side this is the `[OperationContract]` method and its
`[DataContract]` return type from `soap-vs-rest.md`, serialized: `GetProductBySku` -> the method,
`<Sku>`/`<CurrentStock>` -> the `[DataMember]` properties. Unmarked members simply never appear here.

### Fault: the structured error
Errors do not travel as HTTP status codes — they travel as a `<Fault>` element inside the Body, with a
fixed schema. SOAP 1.1:

```xml
<soap:Body>
  <soap:Fault>
    <faultcode>soap:Client</faultcode>          <!-- WHO erred: Client | Server (1.2: Sender | Receiver) -->
    <faultstring>SKU must be supplied.</faultstring>   <!-- human-readable message -->
    <faultactor>...</faultactor>                <!-- optional: which node faulted -->
    <detail>...</detail>                        <!-- optional: structured, app-specific data -->
  </soap:Fault>
</soap:Body>
```

The `faultcode` carries the blame semantics REST puts in status classes: **`Client`** = the caller's
request was wrong (the 4xx analog), **`Server`** = the service failed processing a valid request (the
5xx analog). SOAP 1.2 renames them `Sender`/`Receiver` and restructures the element (`<Code>`,
`<Reason>`, `<Node>`, `<Role>`, `<Detail>`), but the semantics are identical. The `<detail>` element is
the SOAP analog of a problem-details body: machine-readable, app-defined error data — where a typed
fault contract would serialize.

Server-side, this is what `throw new FaultException("SKU must be supplied.")` *becomes*: SoapCore
catches it and writes the `<Fault>`. And note the transport wrinkle: over HTTP, SOAP 1.1 faults
conventionally ride an **HTTP 500** even for client errors — the real classification is the `faultcode`
*inside* the envelope, another instance of "the message, not the transport, carries everything."

### Encoding: how objects become XML
"Encoding" in SOAP covers two related questions. First, **style**: how the Body is organized. The modern
answer — and what SoapCore's default produces — is **document/literal**: the Body is a document whose
schema the WSDL states *literally*, validatable by plain XML Schema. The legacy alternative, RPC/encoded,
embedded type-encoding rules in the message itself; it interoperated poorly and is effectively dead —
recognize the term, don't build with it.

Second, **serialization**: which component turns your objects into that XML. In the SoapCore setup that
is the `SoapSerializer.DataContractSerializer` argument — it maps `[DataContract]`/`[DataMember]` types
to elements, and it is exactly the seam where `SoapEncoderOptions` sits (text encoding, message version,
buffer sizes). The practical consequences of XML encoding: messages are verbose (angle brackets around
every field — the size/performance cost in the REST-vs-SOAP table), schema-validated (a malformed request
is rejected before your code runs), and typed end-to-end (the WSDL's XSD types match the serializer's
output). For large binary payloads there is **MTOM**, an optimization that ships binary parts outside the
XML instead of base64-bloating the Body — awareness-level: know it exists and why.

### Transport: agnostic in principle, HTTP POST in practice
SOAP is deliberately **transport-agnostic**: the envelope is self-contained (addressing, security, and
errors all live inside it), so the same message can ride HTTP, SMTP, or a message queue like JMS/MSMQ —
and in enterprise estates, sometimes does. That is the design reason SOAP ignores HTTP's verb semantics.

In practice, transport means **HTTP POST to one endpoint URL**. Every operation — reads included — is a
POST to (in our demo) `/soap/catalog.asmx`; *which* operation is inside the Body, not in the URL or verb.
The operation is also named redundantly for routing/filtering: SOAP 1.1 puts it in a **`SOAPAction`**
HTTP header, 1.2 folds it into the Content-Type's `action` parameter. Consequences worth saying aloud:
intermediaries cannot cache SOAP responses the way they cache REST GETs (every message is an opaque
POST), and URL-level tooling (browser address bar, curl one-liners) needs a full envelope in hand. A raw
request, end to end:

```
POST /soap/catalog.asmx HTTP/1.1
Content-Type: text/xml; charset=utf-8
SOAPAction: "http://example.local/fulfillment/ICatalogSoapService/GetProductBySku"

<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetProductBySku xmlns="http://example.local/fulfillment">
      <sku>WIDGET-001</sku>
    </GetProductBySku>
  </soap:Body>
</soap:Envelope>
```

Transport, encoding, structure — one story: HTTP delivers an XML document; the namespace says which SOAP;
the Header carries the cross-cutting blocks; the Body carries the operation or its Fault.

## Say It in an Interview
- *"Every SOAP message is the same shape: an Envelope root, an optional Header for cross-cutting metadata,
  a mandatory Body for the payload. Requests, responses, and errors are all that one document type."*
- *"The Header is where WS-Security and addressing physically live — inside the message, so they survive
  any transport and can be signed. `mustUnderstand` forces a receiver to fault rather than ignore a block
  it can't process."*
- *"A Fault is a structured error inside the Body: `faultcode` says who erred — Client or Server, the
  4xx/5xx analog — plus a message and an app-defined `detail` block. Over HTTP it typically rides a 500;
  the real classification is inside the envelope."*
- *"Modern SOAP is document/literal: the Body is plain schema-validated XML described literally by the
  WSDL. In SoapCore the DataContractSerializer does the object-to-XML mapping."*
- *"SOAP is transport-agnostic by design — the envelope is self-contained — but in practice it's HTTP
  POST to one URL, operation named in the Body and the SOAPAction header. That's why there's no verb
  semantics and no HTTP-level caching."*

## Check Yourself
1. Which envelope child is optional, which is mandatory, and what belongs in each?
2. How do you tell SOAP 1.1 from 1.2 by looking at a captured message?
3. What does `mustUnderstand="1"` on a header block change, and why would a client set it on a security
   header?
4. A fault arrives with `faultcode` `soap:Client`. Whose bug is it, what is the REST analog, and what
   HTTP status probably carried it?
5. Document/literal vs RPC/encoded — which do you build today and what does "literal" buy you?
6. Why can the same SOAP envelope ride HTTP or a message queue unchanged, and what did SOAP give up in
   exchange over HTTP?
7. In the SoapCore demo, trace `throw new FaultException("SKU must be supplied.")` to what the client
   receives on the wire.

**Answers:** (1) Header optional — cross-cutting metadata (security, addressing, correlation); Body
mandatory — the operation payload or a Fault. (2) The Envelope namespace (`schemas.xmlsoap.org/...` = 1.1,
`www.w3.org/2003/05/...` = 1.2) and the content type (`text/xml` vs `application/soap+xml`). (3) The
receiver must fault if it cannot process the block instead of silently ignoring it; the client sets it so
the server cannot skip security handling. (4) The caller's — the request was invalid; REST's 4xx; over
HTTP likely a 500 anyway, because SOAP classifies inside the envelope. (5) Document/literal; the Body is
plain XML whose schema the WSDL states literally, so standard XML Schema validation and tooling work.
(6) Everything the message needs — addressing, security, error structure — is inside the envelope, so no
transport feature is required; the price over HTTP is verb semantics and transport caching (everything is
POST). (7) SoapCore catches the exception and serializes a `<Fault>` into the response Body:
`faultcode` `soap:Client`-style blame, `faultstring` "SKU must be supplied.", HTTP 500 carrying it.

## Summary
- One shape for every message: Envelope (root, namespace = version) > optional Header > mandatory Body.
- Header = cross-cutting metadata inside the message — WS-Security, addressing; `mustUnderstand` makes a
  block non-ignorable; it is the in-message analog of HTTP headers.
- Body = the operation payload, element names dictated by the WSDL, produced by the DataContract
  serializer from your `[DataContract]` types.
- Fault = structured error in the Body: `faultcode` Client/Server (the 4xx/5xx analog), `faultstring`,
  optional `detail`; what a thrown `FaultException` becomes; usually rides HTTP 500.
- Encoding: document/literal XML via `DataContractSerializer` (the `SoapEncoderOptions` seam); verbose but
  schema-validated; MTOM exists for binary.
- Transport: agnostic by design, HTTP POST in practice — one URL, operation in the Body + `SOAPAction`,
  no verb semantics, no HTTP caching.

## Resources
- [SOAP 1.2 Primer (W3C)](https://www.w3.org/TR/soap12-part0/)
- [SOAP 1.1 specification (W3C Note)](https://www.w3.org/TR/2000/NOTE-SOAP-20000508/)
- [WS-Security UsernameToken Profile (OASIS)](https://docs.oasis-open.org/wss/v1.1/wss-v1.1-spec-os-UsernameTokenProfile.pdf)
- [SoapCore (GitHub)](https://github.com/DigDes/SoapCore)
