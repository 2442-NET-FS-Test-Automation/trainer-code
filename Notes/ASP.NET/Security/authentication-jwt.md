# Authentication and Authorization with JWT

## Learning Objectives
- Distinguish authentication from authorization and locate each in the pipeline.
- Describe a JWT: structure, claims, signature, and why it suits stateless APIs.
- Configure JWT bearer auth (`AddAuthentication().AddJwtBearer()`), protect endpoints with
  `[Authorize]`, and walk the 401 -> token -> 200 flow.
- Deliver a token in a cookie with the security flags that matter.

## Why This Matters
The moment your API leaves localhost, "who is calling and what may they do" stops being optional. JWT
bearer auth is the standard answer for stateless REST APIs — it is how SPAs and mobile clients
authenticate against controller APIs everywhere. The authn/authz distinction inside it is a guaranteed
interview question, and the cookie-flag material is the difference between "it works" and "it survives an
XSS writeup."

## The Concept

### Two questions, two middleware
- **Authentication** — *who are you?* Verifies identity: validate the token's signature, populate `User`.
- **Authorization** — *what may you do?* Checks the authenticated identity against requirements:
  `[Authorize]`, roles, policies.

```csharp
app.UseAuthentication();   // read + validate the token -> set HttpContext.User
app.UseAuthorization();    // enforce [Authorize] against that User
```

The order is not stylistic — you cannot authorize an identity you have not established. The failure codes
keep the same separation: **401 Unauthorized** = authentication failed (no/invalid token — "who are you?"),
**403 Forbidden** = authenticated but not permitted ("I know who you are; no").

### What a JWT is
A JSON Web Token is three base64url segments: `header.payload.signature`.

```
header    {"alg":"HS256","typ":"JWT"}
payload   {"name":"ada","iss":"library-fulfillment","aud":"library-fulfillment-clients","exp":1751500000}
signature HMAC-SHA256(header + "." + payload, secret key)
```

The payload carries **claims** — statements about the subject (name, roles, issuer `iss`, audience `aud`,
expiry `exp`). The token is **signed, not encrypted**: anyone can *read* it (never put secrets in claims),
but nobody can *alter* it without the key, because the signature would no longer verify. That signature is
what makes JWTs fit REST's statelessness (`../02-rest-http/rest-principles.md`): the server keeps **no
session** — every request carries its own proof, any instance can verify it with the key alone, and
horizontal scaling needs no shared session store.

### Issuing and validating in ASP.NET Core
Validation is configured once; the middleware does the rest:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,           ValidIssuer   = "library-fulfillment",
        ValidateAudience = true,         ValidAudience = "library-fulfillment-clients",
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true          // reject expired tokens
    });
builder.Services.AddAuthorization();
```

Issuing, reduced to its skeleton (a real system authenticates credentials first — that step is a login
flow of its own). Issuance belongs in an injected **service** — the demo's `TokenService : ITokenService`,
registered `AddSingleton`, the same DI shape as any other dependency:

```csharp
public string Issue(string user)   // TokenService - validation's mirror: same key, issuer, audience
{
    var creds = new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)), SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken("library-fulfillment", "library-fulfillment-clients",
        new[] { new Claim(ClaimTypes.Name, user) },
        expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

Issuer and validator **must agree** on key, issuer, and audience — a mismatch means tokens you just minted
come back 401. Protecting an endpoint is one attribute (the demo guards a controller action; on a
minimal-API endpoint the same gate is spelled `.RequireAuthorization()` — one gate, same 401), and the
round trip is observable with curl:

```csharp
// Controller action (the demo's shape - InventoryController):
[HttpGet("{sku}/supplier-price")]
[Authorize]                               // no valid token -> 401; the action never runs
public async Task<ActionResult> GetSupplierPrice(string sku, CancellationToken ct) => ...;

// Minimal-API spelling of the identical gate:
app.MapGet("/api/inventory/{sku}/supplier-price", ...).RequireAuthorization();
```

```bash
curl -i http://localhost:5000/api/inventory/BK-001/supplier-price          # 401
TOKEN=$(curl -s -X POST "http://localhost:5000/auth/token?user=ada" | jq -r .token)
curl -i -H "Authorization: Bearer $TOKEN" \
     http://localhost:5000/api/inventory/BK-001/supplier-price             # 200
```

`Bearer` in the `Authorization` header is the delivery convention — "the bearer of this token is
authorized." **Key handling:** in any real deployment the signing key lives in configuration/secrets
management, is rotated, and is never committed to source. A constant fallback key in a sample exists only
so it runs without setup — label it loudly and treat it as the first thing to fix.

### The cookie carrier and its flags
A browser front end can carry the same token in a **cookie** instead of a header (the demo's
`AuthController` serves both carriers from the same `ITokenService`) — and then the flags are
the entire lesson:

```csharp
Response.Cookies.Append("auth", _tokens.Issue(user), new CookieOptions
{
    HttpOnly = true,                 // JavaScript cannot read it -> XSS cannot steal it
    Secure   = true,                 // HTTPS only -> never sent in cleartext
    SameSite = SameSiteMode.Strict,  // not sent on cross-site requests -> blunts CSRF
    MaxAge   = TimeSpan.FromHours(1) // the "session" lifetime
});
```

Trade-off to be able to state: header tokens are immune to CSRF but must be stored somewhere JS can reach
(XSS exposure); `HttpOnly` cookies are immune to XSS theft but need `SameSite`/anti-forgery for CSRF.
Flags are how you buy back most of both.

## Say It in an Interview
- *"Authentication verifies who you are — validate the token, populate `User`; authorization decides what
  you may do — `[Authorize]`, roles, policies. The middleware must run in that order, and the status codes
  mirror it: 401 for 'who are you,' 403 for 'I know who you are; no.'"*
- *"A JWT is header, payload, signature — base64url, signed not encrypted: readable by anyone, alterable
  by no one without the key. Its claims carry identity, issuer, audience, and expiry."*
- *"JWTs fit stateless APIs because every request carries its own proof — no server session, any instance
  can validate with the key, and horizontal scaling needs no shared session store."*
- *"For browser delivery I set the three cookie flags: `HttpOnly` so XSS can't steal it, `Secure` so it
  never travels in cleartext, `SameSite` to blunt CSRF. Header tokens and cookies trade XSS exposure
  against CSRF exposure — flags buy most of both back."*
- *"Signing keys live in secret stores, rotated, never in source."*

## Check Yourself
1. A request with a valid token gets 403. Which middleware rejected it, and what does 403 mean vs 401?
2. Why is it safe that anyone can decode a JWT's payload — and what must therefore never go in it?
3. What four things does the `TokenValidationParameters` block above validate?
4. Why do JWTs eliminate the need for a shared session store when scaling horizontally?
5. Name the three cookie flags for a token cookie and the attack each blunts.
6. State the header-vs-cookie trade-off in one sentence.

**Answers:** (1) Authorization — identity was established (authentication passed) but lacks permission;
401 means authentication itself failed. (2) The token is signed, not encrypted — tampering breaks the
signature, but the contents are readable, so no secrets (passwords, keys, PII you would not show the
user) belong in claims. (3) Issuer, audience, signing key/signature, and lifetime (expiry). (4) The
token itself is the session: every request carries verifiable proof, so any instance with the key can
authenticate it — no server-side session lookup. (5) `HttpOnly` (XSS theft), `Secure` (cleartext
interception), `SameSite` (CSRF). (6) Header tokens dodge CSRF but sit where JavaScript can reach them
(XSS risk); `HttpOnly` cookies dodge XSS theft but need `SameSite`/anti-forgery against CSRF.

## Summary
- Authentication = who (401); authorization = what (403); middleware in that order.
- JWT = signed header.payload.signature; claims readable, tamper-evident, self-contained — the stateless
  session.
- `AddJwtBearer` validates issuer/audience/signature/lifetime once; `[Authorize]` gates endpoints; curl
  shows the 401 -> Bearer -> 200 arc.
- Cookies carry tokens for browsers; `HttpOnly` + `Secure` + `SameSite` are non-negotiable flags.
- Signing keys live in secret stores — a constant key is a loudly-labeled sample device only.

## Resources
- [JWT introduction (jwt.io)](https://jwt.io/introduction)
- [JWT bearer authentication in ASP.NET Core (Microsoft Learn)](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
- [OWASP: Session management and cookie security cheat sheet](https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html)
