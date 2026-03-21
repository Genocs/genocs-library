# Genocs.Auth — Agent Reference Documentation

> **Format**: AI-optimized agent reference. Structured for rapid decision-making. All capability claims are
> linked to actual source files. Do not infer capabilities not listed here.

---

## 1. Purpose

`Genocs.Auth` provides JWT authentication and token management for ASP.NET Core services in the Genocs
ecosystem. It covers:

- **JWT registration** with symmetric keys, X.509 certificates, or OpenIdConnect discovery (`AddJwt`,
  `AddOpenIdJwt`, `AddPrivateKeyJwt`).
- **Token issuance** (`IJwtHandler.CreateToken`) with full claims, roles, audience, and configurable expiry.
- **Token validation** (`IJwtHandler.GetTokenPayload`) for decoding tokens without full auth pipeline.
- **Real-time token revocation** via an in-memory blacklist (`IAccessTokenService`,
  `InMemoryAccessTokenService`, `AccessTokenValidatorMiddleware`).
- **Auth bypass mode** for development/testing: when `jwt.enabled: false`, registers
  `DisabledAuthenticationPolicyEvaluator` so all requests pass as authenticated.
- **`[Auth]` attribute** — thin wrapper over `[Authorize]` with configurable scheme.

**Package ID**: `Genocs.Auth`  
**NuGet config section**: `jwt` (= `JwtOptions.Position`)

---

## 2. Quick Facts

| Property | Value |
|---|---|
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Config section key | `jwt` (= `JwtOptions.Position`) |
| Registration guard | `builder.TryRegister("auth")` — idempotent |
| Default signing algorithm | `HS256` (symmetric); auto-switches to `RS256` for X.509 certs |
| Token storage for revocation | `IMemoryCache` (in-process) |
| Expiry default | 60 minutes (`ExpiryMinutes`), overridable with `Expiry` (TimeSpan) |
| Requires | `Genocs.Security` (project/package ref), `Microsoft.AspNetCore.Authorization` |
| Source file | [`src/Genocs.Auth/Extensions.cs`](../src/Genocs.Auth/Extensions.cs) |

---

## 3. Use When

- You need JWT Bearer authentication in any Genocs ASP.NET Core service.
- You need to issue tokens from within the service (e.g. identity/auth endpoint) — inject `IJwtHandler` and
  call `CreateToken(userId, roles, audience, claims)`.
- You need real-time token revocation without a distributed store — use `IAccessTokenService.Deactivate(token)`
  and `UseAccessTokenValidator` middleware.
- You need OpenID Connect / Firebase-compatible JWT validation — use `AddOpenIdJwt`.
- You need RSA private-key signed JWT validation — use `AddPrivateKeyJwt`.
- You need to completely disable auth in a development or testing environment — set `jwt.enabled: false`.

---

## 4. Do Not Assume

- **`AddJwt` is idempotent** — `TryRegister("auth")` blocks all subsequent calls; calling it twice is safe.
- **`AddOpenIdJwt` and `AddPrivateKeyJwt` are NOT guarded** by `TryRegister`. Do not call them together with
  `AddJwt` — they each independently call `AddAuthentication(...)` / `AddJwtBearer(...)`.
- **`DisabledAuthenticationPolicyEvaluator`** is registered when `jwt.enabled = false`. All requests will
  appear as authenticated in this mode — **never use in production**.
- **`IAccessTokenService` (InMemoryAccessTokenService) is in-memory only.** Revoked tokens are NOT shared
  across service replicas. For distributed revocation, replace the `IAccessTokenService` registration.
- **`AccessTokenValidatorMiddleware` must be explicitly added to the pipeline** via `UseAccessTokenValidator`.
  `AddJwt` registers it as a transient middleware but does NOT add it to the pipeline automatically.
- **`UseAccessTokenValidator` checks the in-memory blacklist only.** JWT signature and expiry validation is
  still performed by the standard `JwtBearer` middleware; the access token validator provides an *additional*
  real-time revocation check on top.
- **X.509 certificate for signing**: if the certificate has a private key, it is used for both issuing and
  validating. If public key only, it is used for validation only. Both cases log to `Console.WriteLine`.
- **`ClockSkew` is set to `TimeSpan.Zero`** — there is no tolerance for clock drift between issuer and
  validator. Tokens expire exactly at the `exp` claim.
- **`AllowAnonymousEndpoints`** (in `JwtOptions`) — `AccessTokenValidatorMiddleware` bypasses the blacklist
  check for these exact path strings; standard JWT Bearer middleware still applies to them.

---

## 5. High-Value Entry Points

```
Extensions.cs → AddJwt(IGenocsBuilder, string sectionName, Action<JwtBearerOptions>?)
Extensions.cs → AddOpenIdJwt(IGenocsBuilder, string sectionName)
Extensions.cs → AddPrivateKeyJwt(IGenocsBuilder, string sectionName)
Extensions.cs → UseAccessTokenValidator(IApplicationBuilder)
IJwtHandler.cs → IJwtHandler                              ← create and decode tokens
IAccessTokenService.cs → IAccessTokenService              ← revoke tokens
AccessTokenValidatorMiddleware.cs                          ← pipeline hook for revocation
JsonWebToken.cs → JsonWebToken                             ← token issuance output model
Configurations/JwtOptions.cs → JwtOptions                 ← full config model
Attributes/AuthAttribute.cs → [Auth]                      ← endpoint authorization shorthand
```

---

## 6. Decision Matrix

| Goal | API to use |
|---|---|
| Register standard symmetric-key JWT auth | `builder.AddJwt()` with `jwt.issuerSigningKey` set |
| Register X.509 certificate JWT auth | `builder.AddJwt()` with `jwt.certificate.location` or `jwt.certificate.rawData` set |
| Register OpenIdConnect / Firebase JWT | `builder.AddOpenIdJwt()` with `jwt.issuer` + `jwt.metadataAddress` |
| Register RSA private-key JWT validation | `builder.AddPrivateKeyJwt()` with `jwt.issuerSigningKey` (RSA key) |
| Issue a new JWT | Inject `IJwtHandler`, call `CreateToken(userId, roles, audience, claims)` |
| Decode a JWT without auth pipeline | Inject `IJwtHandler`, call `GetTokenPayload(accessToken)` |
| Revoke the currently active token | Inject `IAccessTokenService`, call `DeactivateCurrent()` |
| Check if a specific token is revoked | Inject `IAccessTokenService`, call `IsActive(token)` |
| Enforce revocation check in pipeline | `app.UseAccessTokenValidator()` (after `UseAuthentication`) |
| Disable auth in dev/test | Set `jwt.enabled: false` in config |
| Customize `JwtBearerOptions` at registration | Pass `Action<JwtBearerOptions>` to `AddJwt(...)` |
| Add auth to an endpoint | Decorate with `[Auth("Bearer")]` or use `auth: true` in endpoint DSL |

---

## 7. Minimal Integration Recipe

### 7.1 appsettings.json (symmetric key)

```json
{
  "jwt": {
    "enabled": true,
    "issuerSigningKey": "your-secret-key-minimum-32-chars!!",
    "issuer": "https://your-service.example.com",
    "validIssuer": "https://your-service.example.com",
    "validAudience": "your-api",
    "validateAudience": true,
    "validateIssuer": true,
    "validateLifetime": true,
    "expiryMinutes": 60,
    "requireHttpsMetadata": false
  }
}
```

### 7.2 Program.cs — Registration

```csharp
IGenocsBuilder gnxBuilder = builder
    .AddGenocs()
    .AddWebApi()
    .AddJwt();   // reads from "jwt" section

gnxBuilder.Build();
```

### 7.3 Program.cs — Middleware

```csharp
var app = builder.Build();

app.UseAuthentication()      // standard ASP.NET Core
   .UseAuthorization()
   .UseAccessTokenValidator();  // optional real-time revocation check

app.UseEndpoints(endpoints => endpoints
    .Get("/public", ctx => ctx.Response.WriteAsync("ok"))
    .Get("/secure", ctx => ctx.Response.WriteAsync("secret"), auth: true));
```

### 7.4 Token issuance

```csharp
public class AuthEndpoint(IJwtHandler jwt)
{
    public JsonWebToken Login(string userId, string role)
        => jwt.CreateToken(userId, roles: [role]);
}
```

### 7.5 Token revocation

```csharp
public class LogoutEndpoint(IAccessTokenService tokens)
{
    public void Logout() => tokens.DeactivateCurrent();
}
```

### 7.6 X.509 certificate (appsettings.json)

```json
{
  "jwt": {
    "enabled": true,
    "certificate": {
      "location": "/certs/signing.pfx",
      "password": "secret"
    }
  }
}
```

### 7.7 OpenID Connect (Firebase, Entra, etc.)

```csharp
builder.AddOpenIdJwt();   // reads "jwt.issuer" + "jwt.metadataAddress"
```

---

## 8. Behavior Notes

- **`TokenValidationParameters.ClockSkew = TimeSpan.Zero`** — the clock skew allowance is intentionally
  removed. Tokens are considered expired exactly at the `exp` value.
- **Algorithm auto-detection** in `JwtHandler`: if `options.Algorithm` is empty, uses `HS256` for symmetric
  keys and `RS256` for X.509 keys.
- **`JsonWebToken.RefreshToken`** is always set to `string.Empty` by `JwtHandler.CreateToken` — refresh
  tokens are not implemented in this package; callers must implement refresh separately.
- **`InMemoryAccessTokenService.GetKey`** stores revoked tokens under the key
  `"blacklisted-tokens:{token}"` in `IMemoryCache`. The entry TTL matches the JWT expiry (`jwtOptions.Expiry`
  or `ExpiryMinutes`).
- **`AllowAnonymousEndpoints`** matching is exact-string path comparison — no wildcard support.
- **`JwtOptions.Challenge`** (default `"Bearer"`) sets `DefaultAuthenticateScheme`, `DefaultChallengeScheme`,
  and `DefaultScheme`. Changing this affects all scheme-based authorization attributes in the application.
- **`optionsFactory` parameter in `AddJwt`** is invoked after all defaults are set, allowing late overrides
  to `JwtBearerOptions` (e.g. custom `OnTokenValidated` events).

---

## 9. Source-Accurate Capability Map

| Capability | Source Location |
|---|---|
| `AddJwt` (symmetric / cert) | [`Extensions.cs`](../src/Genocs.Auth/Extensions.cs) |
| `AddOpenIdJwt` | [`Extensions.cs`](../src/Genocs.Auth/Extensions.cs) |
| `AddPrivateKeyJwt` | [`Extensions.cs`](../src/Genocs.Auth/Extensions.cs) |
| `UseAccessTokenValidator` | [`Extensions.cs`](../src/Genocs.Auth/Extensions.cs) |
| `DisabledAuthenticationPolicyEvaluator` | [`DisabledAuthenticationPolicyEvaluator.cs`](../src/Genocs.Auth/DisabledAuthenticationPolicyEvaluator.cs) |
| `IJwtHandler` interface | [`IJwtHandler.cs`](../src/Genocs.Auth/IJwtHandler.cs) |
| `JwtHandler` implementation | [`Handlers/JwtHandler.cs`](../src/Genocs.Auth/Handlers/JwtHandler.cs) |
| `JsonWebToken` output model | [`JsonWebToken.cs`](../src/Genocs.Auth/JsonWebToken.cs) |
| `JsonWebTokenPayload` | [`JsonWebTokenPayload.cs`](../src/Genocs.Auth/JsonWebTokenPayload.cs) |
| `IAccessTokenService` interface | [`IAccessTokenService.cs`](../src/Genocs.Auth/IAccessTokenService.cs) |
| `InMemoryAccessTokenService` | [`Services/InMemoryAccessTokenService.cs`](../src/Genocs.Auth/Services/InMemoryAccessTokenService.cs) |
| `AccessTokenValidatorMiddleware` | [`AccessTokenValidatorMiddleware.cs`](../src/Genocs.Auth/AccessTokenValidatorMiddleware.cs) |
| `JwtOptions` config model | [`Configurations/JwtOptions.cs`](../src/Genocs.Auth/Configurations/JwtOptions.cs) |
| `[Auth]` attribute | [`Attributes/AuthAttribute.cs`](../src/Genocs.Auth/Attributes/AuthAttribute.cs) |
| `[ApiKeyOrJwtAuthorize]` attribute | [`Attributes/ApiKeyOrJwtAuthorizeAttribute.cs`](../src/Genocs.Auth/Attributes/ApiKeyOrJwtAuthorizeAttribute.cs) |
| `JwtAuthAttribute` | [`JwtAuthAttribute.cs`](../src/Genocs.Auth/JwtAuthAttribute.cs) |
| `JwtOrApiKeyAuthenticationMiddleware` | [`JwtOrApiKeyAuthenticationMiddleware.cs`](../src/Genocs.Auth/JwtOrApiKeyAuthenticationMiddleware.cs) |

---

## 10. Dependencies

| Dependency | Role |
|---|---|
| `Genocs.Security` | `SecurityKeyBuilder.CreateRsaSecurityKey` (used in `AddPrivateKeyJwt`) |
| `Microsoft.AspNetCore.Authorization` | `IPolicyEvaluator`, `[Authorize]`, policy framework |
| `Microsoft.AspNetCore.Authentication.JwtBearer` (transitive) | Bearer scheme registration |
| `Microsoft.IdentityModel.Tokens` (transitive) | `TokenValidationParameters`, `SigningCredentials` |
| `System.IdentityModel.Tokens.Jwt` (transitive) | `JwtSecurityTokenHandler`, `JwtSecurityToken` |
| `Microsoft.Extensions.Caching.Memory` (transitive) | `IMemoryCache` used in `InMemoryAccessTokenService` |

---

## 11. Related Docs

- NuGet package readme: [src/Genocs.Auth/README_NUGET.md](src/Genocs.Auth/README_NUGET.md)
- Repository guide: [README.md](README.md)
- Package documentation: [docs/Genocs.Auth-Agent-Documentation.md](docs/Genocs.Auth-Agent-Documentation.md)
- Related: [Genocs.Core-Agent-Documentation.md](docs/Genocs.Core-Agent-Documentation.md) — `IGenocsBuilder` that `AddJwt` extends
- Related: [Genocs.WebApi-Agent-Documentation.md](docs/Genocs.WebApi-Agent-Documentation.md) — endpoint DSL that integrates with `auth: true` parameter
