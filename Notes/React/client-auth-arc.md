# The Client-Side Auth Arc: Login, Token, Guarded Routes, End to End

## Learning Objectives
- Trace the full arc: a controlled login form posts credentials, the API returns a JWT, and the client
  shapes the UI around it.
- Weigh where to keep the token — `localStorage` vs an HttpOnly cookie — and state the XSS/CSRF trade-off
  honestly.
- Attach `Authorization: Bearer` on every request with a single Axios interceptor.
- Decode (never verify) the JWT payload client-side to shape the UI, and internalize that the server
  re-validates every call.
- Model auth as a state machine in Context with `useReducer`, and gate both UI and routes on it.
- Explain why the `Authorization` header triggers a CORS preflight the server must allow.

## Why This Matters
"Auth" on the front end is not one feature — it is an arc of small decisions that only work if they line up:
how you collect credentials, where you put the token, how you attach it, how you decide what to show, and
how you keep unauthenticated users out of protected screens. Each link has a wrong answer that looks fine in
a demo and fails in production: a token in the wrong place is an XSS payout, a UI that trusts a decoded
token is a security hole, a route guard that reads stale state flickers the dashboard at a logged-out user.
This is the capstone because it forces every earlier React idea — controlled forms, Context, reducers,
Router, Axios — to cooperate around a single source of truth.

## The Concept

### (a) The login form: controlled input in, JWT out
Start with a controlled form (state owns the fields) that posts credentials and receives a token. The
server checks the password and, on success, returns a signed JWT — a compact string of three dot-separated
base64url segments: header, payload (the claims), and signature.

```tsx
import { useState, FormEvent } from "react";
import axios from "axios";

export function LoginForm({ onToken }: { onToken: (token: string) => void }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      const res = await axios.post<{ token: string }>("/auth/login", { email, password });
      onToken(res.data.token); // hand the JWT up to auth state
    } catch {
      setError("Invalid email or password.");
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" />
      <input
        type="password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        placeholder="Password"
      />
      <button type="submit">Log in</button>
      {error && <p role="alert">{error}</p>}
    </form>
  );
}
```

### (b) Where to keep the token: `localStorage` vs HttpOnly cookie
Once you hold a JWT you must store it somewhere so a refresh does not log the user out. There are two real
options, and the trade-off is a genuine one — say it honestly rather than pretending one is strictly safe.

- **`localStorage`** — simple: read/write from JS, attach it yourself. But it is **readable by any
  JavaScript on the page**, so a single successful **XSS** injection can exfiltrate the token. It is not
  sent automatically, which sidesteps CSRF.
- **HttpOnly cookie** — set by the server with `HttpOnly`, so **JavaScript cannot read it**, which defangs
  token theft via XSS. But the browser attaches it to requests automatically, which reopens **CSRF** as the
  concern, mitigated with the `SameSite` attribute and/or anti-forgery tokens.

The honest summary: `localStorage` trades CSRF-immunity for XSS exposure; HttpOnly cookies trade the
reverse. Cookies are generally the more defensible default for real applications; `localStorage` is common
and acceptable when your XSS surface is tightly controlled. The examples here read from `localStorage`
because it makes the client-side flow explicit, but the storage decision is independent of the rest of the
arc. The deep version of this trade-off — cookie attributes, XSS and CSRF mechanics, the defenses, and
what a cookie switch costs on both ends of our stack — is its own note:
[token-storage-cookies-xss-csrf.md](token-storage-cookies-xss-csrf.md).

### (c) Attach the token once: a single Axios request interceptor
Do **not** hand-write an `Authorization` header at each call site — you will miss one. Register a single
Axios **request interceptor** that runs before every request and adds the header if a token exists. One
place to change, impossible to forget.

```ts
import axios from "axios";

export const api = axios.create({ baseURL: "https://localhost:5001" });

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

Every `api.get`/`api.post` now carries `Authorization: Bearer <token>` automatically. (With an HttpOnly
cookie you would skip this entirely and set `withCredentials: true` so the browser sends the cookie.)

### (d) Decode, do not verify: reading claims client-side to shape UI
To show a name or hide an admin button you need the claims *inside* the token. The client can **decode** the
payload — but it can never **verify** it. Verification requires the server's secret; the client does not
have it and must not. So the client base64url-decodes the middle segment purely to shape the UI, and treats
the result as a hint, never as proof.

```ts
interface Identity {
  name: string;
  role: string;
}

export function decodeToken(token: string): Identity | null {
  try {
    const segment = token.split(".")[1];
    if (!segment) return null;
    const base64 = segment.replace(/-/g, "+").replace(/_/g, "/"); // base64url -> base64
    const payload = JSON.parse(atob(base64)) as { [claim: string]: string | number | undefined };
    const name = payload["name"];
    const role = payload["role"];
    if (typeof name !== "string" || typeof role !== "string") return null;
    return { name, role };
  } catch {
    return null;
  }
}
```

Two real-world wrinkles. First, **.NET APIs often write claims under full XML-schema URI keys**, not short
names — a role can arrive as `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` and a name as
`http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name`. Production code reads those exact keys (or
whatever the server emits), so confirm the claim names against a decoded sample rather than assuming
`"role"`:

```ts
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
const role = payload[ROLE_CLAIM] ?? payload["role"];
```

Second, and non-negotiable: **the server re-validates every request.** It checks the signature and
expiry on every protected call, so a token decoded on the client is *never trusted on the client* for
anything that matters. Client-side decoding decides what to *display*; the server decides what is
*allowed*. A user who forges `"role": "Admin"` in devtools can flip a button on — and gets a 403 the instant
they touch a protected endpoint, because the signature no longer matches. Never enforce security in the
browser.

### (e) Auth as a state machine: Context + useReducer
Auth has more than two states. Anonymous, authenticating (request in flight), authenticated, and error are
four distinct conditions the UI must render differently. A reducer centralizes the transitions; Context
distributes the result to the whole tree so any component can read it without prop-drilling.

```tsx
import { createContext, useContext, useReducer, ReactNode } from "react";

type Status = "anonymous" | "authenticating" | "authenticated" | "error";
interface Identity { name: string; role: string; }

interface AuthState {
  status: Status;
  identity: Identity | null;
  error: string | null;
}

type AuthAction =
  | { type: "LOGIN_START" }
  | { type: "LOGIN_SUCCESS"; identity: Identity }
  | { type: "LOGIN_ERROR"; error: string }
  | { type: "LOGOUT" };

const initialState: AuthState = { status: "anonymous", identity: null, error: null };

function authReducer(state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case "LOGIN_START":
      return { status: "authenticating", identity: null, error: null };
    case "LOGIN_SUCCESS":
      return { status: "authenticated", identity: action.identity, error: null };
    case "LOGIN_ERROR":
      return { status: "error", identity: null, error: action.error };
    case "LOGOUT":
      return initialState;
    default:
      return state;
  }
}

interface AuthContextValue extends AuthState {
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(authReducer, initialState);

  async function login(email: string, password: string) {
    dispatch({ type: "LOGIN_START" });
    try {
      const res = await api.post<{ token: string }>("/auth/login", { email, password });
      const identity = decodeToken(res.data.token);
      if (!identity) throw new Error("Malformed token");
      localStorage.setItem("token", res.data.token);
      dispatch({ type: "LOGIN_SUCCESS", identity });
    } catch {
      dispatch({ type: "LOGIN_ERROR", error: "Login failed." });
    }
  }

  function logout() {
    localStorage.removeItem("token");
    dispatch({ type: "LOGOUT" });
  }

  return (
    <AuthContext.Provider value={{ ...state, login, logout }}>{children}</AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside <AuthProvider>");
  return ctx;
}
```

Now `status === "authenticating"` disables the login button, `"error"` shows the message, and
`"authenticated"` unlocks the app — all from one source of truth.

### (f) Role-gated UI and role-gated routes
Two levels of gating, and you need both. **UI gating** hides controls the user cannot use — a courtesy and a
declutter, not a security boundary. **Route gating** (the route guard) redirects unauthenticated users away
from protected screens entirely.

UI gating is a plain conditional on the identity's role:

```tsx
function Toolbar() {
  const { identity } = useAuth();
  return (
    <div>
      <button>Search catalog</button>
      {identity?.role === "Admin" && <button>Add book</button>} {/* only admins see this */}
    </div>
  );
}
```

The route guard wraps protected routes and redirects to `/login` with React Router's declarative
`<Navigate>` when the user is not authenticated (and optionally lacks a required role). Rendering
`<Navigate>` performs the redirect:

```tsx
import { Navigate } from "react-router-dom";
import { ReactNode } from "react";

function RequireAuth({ children, role }: { children: ReactNode; role?: string }) {
  const { status, identity } = useAuth();

  if (status !== "authenticated") {
    return <Navigate to="/login" replace />; // not logged in -> bounce to login
  }
  if (role && identity?.role !== role) {
    return <Navigate to="/" replace />;       // logged in but wrong role -> home
  }
  return <>{children}</>;
}
```

Wire it into the route table so protected elements only ever render behind the guard:

```tsx
import { BrowserRouter, Routes, Route } from "react-router-dom";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/" element={<Catalog />} />
        <Route
          path="/dashboard"
          element={
            <RequireAuth>
              <Dashboard />
            </RequireAuth>
          }
        />
        <Route
          path="/admin"
          element={
            <RequireAuth role="Admin">
              <AdminPanel />
            </RequireAuth>
          }
        />
      </Routes>
    </BrowserRouter>
  );
}
```

Say the same thing UI gating said, at the routing layer: hiding the admin *button* is not security, and
neither is redirecting away from the admin *route*. Both run in the browser and both are for user
experience. The server still authorizes every call behind those screens — the guard just spares an
unauthenticated user a screen full of failed requests. `replace` swaps the history entry so the guarded URL
does not linger in the back button.

### (g) The header that triggers a CORS preflight
There is one operational gotcha that turns a working local setup into 401s and console errors the moment
auth is added. When your app and API are on different origins (different host, port, or scheme — a dev
front end on one port calling an API on another counts), adding a custom `Authorization` header makes the
request **non-simple**. The browser first sends an automatic **preflight**: an `OPTIONS` request asking the
server whether this origin may send that header and method.

If the server's CORS policy does not explicitly allow the `Authorization` header (and the origin, and the
method), the browser blocks the real request before it is ever sent — you see a CORS error, not a 401. The
fix lives on the server: its CORS configuration must permit the calling origin and include `Authorization`
among the allowed headers so the preflight passes. This is not a React bug; it is the browser enforcing the
same-origin policy, and it appears exactly when you add the Bearer header from step (c).

## Say It in an Interview
- *"The arc: a controlled login form posts credentials, the API returns a signed JWT, I store it, attach it
  on every request with one Axios interceptor, and shape the UI from its claims — while the server
  re-validates every call."*
- *"`localStorage` is XSS-exposed but CSRF-free; an HttpOnly cookie is XSS-safe but reopens CSRF via
  SameSite. It's a real trade-off, not one right answer — cookies are the more defensible default."*
- *"I decode the JWT payload client-side to display a name or hide an admin button, but I never verify it —
  verification needs the server's secret. Client decoding shapes the UI; the server decides what's allowed."*
- *"Route guards render `<Navigate to='/login'>` when the user isn't authenticated. That, and hiding admin
  buttons, is UX — the API still authorizes every request. And the Bearer header triggers a CORS preflight
  the server must allow."*

## Check Yourself
1. Walk the arc from typed credentials to a rendered, personalized dashboard. What are the stages?
2. State the `localStorage`-vs-HttpOnly-cookie trade-off in terms of XSS and CSRF.
3. Why does the client decode but never verify the JWT, and what stops a user who forges a role in devtools?
4. Why put the `Authorization` header in an Axios interceptor instead of on each request?
5. What are the four auth states in the reducer, and what should each drive in the UI?
6. A route guard hides `/admin` and you also hide the admin button. Is the app now secure? Why or why not?
7. You add the Bearer header and suddenly see a CORS error instead of data. What happened, and where is the
   fix?

**Answers:** (1) Controlled form collects credentials -> `POST /auth/login` -> server returns a signed JWT
-> store it -> interceptor attaches `Authorization: Bearer` on future calls -> decode claims to set auth
state -> UI/routes render from that state. (2) `localStorage` is readable by any JS so XSS can steal the
token, but it is not auto-sent so CSRF is moot; an HttpOnly cookie is unreadable by JS (XSS-safe) but
auto-sent, so CSRF becomes the risk (mitigate with SameSite/anti-forgery). (3) Verification requires the
server's signing secret, which the client neither has nor should; decoding only shapes the UI. A forged role
flips a button on but hits a 403/401 at the server because the signature no longer validates. (4) One place
to attach it means you can never forget it on a call site, and one place to change when storage or format
changes. (5) `anonymous`, `authenticating`, `authenticated`, `error`: anonymous shows login, authenticating
disables the submit/spinner, authenticated unlocks the app, error shows the message. (6) No — both are
browser-side UX; the server must authorize every protected request, since anyone can bypass client checks.
(7) The custom `Authorization` header made the cross-origin request non-simple, so the browser sent an
`OPTIONS` preflight the server's CORS policy rejected; fix it on the server by allowing the origin and the
`Authorization` header.

## Summary
- The arc is one connected chain: controlled login -> JWT -> storage -> interceptor -> decode -> auth state
  -> gated UI and routes; break any link and the rest misbehaves.
- Token storage is a real trade-off: `localStorage` (XSS-exposed, CSRF-free) vs HttpOnly cookie (XSS-safe,
  CSRF-exposed) — cookies are the more defensible default.
- Attach `Authorization: Bearer` with a single Axios request interceptor; decode the JWT client-side to
  shape UI but never verify it — the server re-validates every call, so a client-decoded token is never
  trusted for enforcement.
- .NET APIs may emit claims under full XML-schema URI keys; read the exact keys the server sends.
- Model auth as a `useReducer` state machine (anonymous/authenticating/authenticated/error) in Context; gate
  both UI (hide admin buttons) and routes (`<Navigate to="/login">`), knowing both are UX, not security.
- Adding the Bearer header triggers a CORS preflight `OPTIONS` the server must explicitly allow.

## Resources
- [`<Navigate>` and route protection (reactrouter.com)](https://reactrouter.com/en/main/components/navigate)
- [Introduction to JSON Web Tokens (jwt.io)](https://jwt.io/introduction)
- [Cross-Origin Resource Sharing (CORS) — MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS)
