# Genocs.Auth Agent Reference

## Agent Operating Mode

- Assume `Genocs.Auth` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, middleware, option types, interfaces, and attributes as the only safe API surface.
- Generate ASP.NET Core authentication wiring only for the provider modes documented here.
- Do not invent package-specific helpers for Auth0, Azure AD, Keycloak, Okta, Firebase, or API keys beyond the public APIs that actually exist.
- If package composition is unclear, ask whether `Genocs.Core` is already installed because all registration entry points extend `IGenocsBuilder`.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Auth` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | JWT authentication registration, token creation, token parsing, and access-token revocation checks |
| Main value | One package that can validate JWTs through shared-secret, X.509, OpenID Connect discovery, or RSA XML key paths |
| Requires | ASP.NET Core authentication pipeline and `Genocs.Core` |

## What This Package Is For

Use `Genocs.Auth` when you need to:

- register bearer-token authentication in an ASP.NET Core host
- validate JWTs signed with a shared secret
- validate or issue JWTs backed by X.509 certificate material
- validate JWTs from an OpenID Connect discovery endpoint
- validate JWTs using RSA XML key material
- create JWTs in application code through `IJwtHandler`
- parse an existing JWT into a `JsonWebTokenPayload`
- deactivate already-issued access tokens through `IAccessTokenService`
- reject revoked tokens through `UseAccessTokenValidator()`

## What This Package Does Not Do By Itself

Do not assume `Genocs.Auth` can:

- provide interactive sign-in flows such as OAuth authorization code or cookie auth
- provide a dedicated provider extension for Auth0, Azure AD, Keycloak, Okta, or Firebase
- persist revoked tokens across multiple nodes unless you replace the default in-memory implementation
- configure OpenAPI security metadata
- provide mutual TLS or inbound client-certificate authentication for web APIs
- configure endpoint authorization rules beyond standard ASP.NET Core auth services
- make API-key auth a first-class builder-based registration mode

## Safe Default Mental Model

Treat `Genocs.Auth` as four things:

1. A Genocs builder extension for JWT bearer authentication
2. A token-tooling package through `IJwtHandler`
3. A token blacklisting abstraction through `IAccessTokenService`
4. A package with multiple validation modes, but one host still owns `UseAuthentication()` and `UseAuthorization()`

If a user asks for client-certificate auth, direct them to `Genocs.WebApi.Security`. If they ask for a named cloud identity provider, map it to the nearest supported JWT mode instead of inventing a provider-specific API.

## Supported Provider Modes

| Provider mode | Registration API | Best fit | Signing material | Includes token tooling |
|---|---|---|---|---|
| Symmetric JWT | `AddJwt()` | first-party services sharing a secret | `jwt.issuerSigningKey` | Yes |
| X.509-backed JWT | `AddJwt()` | certificate-based validation or issuance | `jwt.certificate.location` or `jwt.certificate.rawData` | Yes |
| OpenID Connect discovery | `AddOpenIdJwt()` | external identity providers exposing OIDC metadata | `jwt.issuer` plus `jwt.metadataAddress` | No |
| RSA XML key JWT | `AddPrivateKeyJwt()` | JWT validation with XML RSA key material | `jwt.issuerSigningKey` as `<RSAKeyValue>...</RSAKeyValue>` | No |
| Manual JWT-or-API-key hybrid | manual `JwtOrApiKeyAuthenticationMiddleware` | advanced compatibility scenarios only | bearer token or `x-gnx-apikey` | No |

## Choose The Right Mode

| Situation | Prefer | Why |
|---|---|---|
| One service issues and validates its own tokens with a shared secret | `AddJwt()` with `issuerSigningKey` | simplest built-in path |
| Token validation or issuance should use X.509 material | `AddJwt()` with `certificate.*` | same API, different signing key source |
| Tokens come from Auth0, Azure AD, Keycloak, Okta, Firebase, or another OIDC provider | `AddOpenIdJwt()` | built for discovery-based key resolution |
| The app only has RSA XML key material | `AddPrivateKeyJwt()` | explicit asymmetric validation path |
| An endpoint must accept bearer auth or API key | manual middleware plus attribute usage | helper exists, but it is not a primary provider registration path |

## Fast Start Recipes

### Recipe 1: Symmetric JWT For First-Party Services

Use this when the same trust boundary controls both token issuance and validation.

```csharp
using Genocs.Auth;
using Genocs.Core.Builders;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
        .AddGenocs()
        .AddJwt();

genocs.Build();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseAccessTokenValidator();
app.Run();
```

Configuration:

```json
{
    "jwt": {
        "enabled": true,
        "issuer": "https://identity.internal",
        "issuerSigningKey": "super-secret-key",
        "audience": "orders-api",
        "validIssuer": "https://identity.internal",
        "validAudience": "orders-api",
        "validateIssuer": true,
        "validateAudience": true,
        "validateLifetime": true,
        "requireExpirationTime": true,
        "requireSignedTokens": true,
        "expiryMinutes": 60
    }
}
```

Effect:

- registers ASP.NET Core JWT bearer authentication
- registers `AddAuthorization()` automatically
- registers `IJwtHandler`
- registers the default in-memory `IAccessTokenService`
- makes `UseAccessTokenValidator()` available for revocation checks

### Recipe 2: X.509 Certificate-Backed JWT

Use this when the signing or validation key should come from certificate material instead of a plain shared secret.

```csharp
using Genocs.Auth;
using Genocs.Core.Builders;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
        .AddGenocs()
        .AddJwt();

genocs.Build();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

Configuration from file path:

```json
{
    "jwt": {
        "enabled": true,
        "issuer": "https://identity.internal",
        "validIssuer": "https://identity.internal",
        "validAudience": "orders-api",
        "validateIssuer": true,
        "validateAudience": true,
        "validateLifetime": true,
        "certificate": {
            "location": "certs/signing.pfx",
            "password": "change-me"
        }
    }
}
```

Configuration from inline base64 certificate bytes:

```json
{
    "jwt": {
        "enabled": true,
        "validIssuer": "https://identity.internal",
        "validAudience": "orders-api",
        "certificate": {
            "rawData": "BASE64_CERTIFICATE_BYTES",
            "password": "change-me"
        }
    }
}
```

Important behavior:

- if the certificate contains a private key, `IJwtHandler` can sign tokens with it
- if the certificate contains only a public key, validation is the safe assumption and token issuance may fail
- if `jwt.algorithm` is empty, the package switches to `RS256` automatically for certificate-backed keys
- if both certificate location and raw data are provided, the later-loaded source wins in practice, so treat that as misconfiguration and choose one source only

### Recipe 3: OpenID Connect Provider Authentication

Use this when an external identity provider publishes `/.well-known/openid-configuration` metadata.

```csharp
using Genocs.Auth;
using Genocs.Core.Builders;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
        .AddGenocs()
        .AddOpenIdJwt();

builder.Services.AddAuthorization();
genocs.Build();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

Configuration:

```json
{
    "jwt": {
        "challenge": "Bearer",
        "issuer": "https://issuer.example.com",
        "metadataAddress": "/.well-known/openid-configuration",
        "audience": "orders-api",
        "includeErrorDetails": true,
        "refreshOnIssuerKeyNotFound": true
    }
}
```

Provider mapping guidance:

- Auth0: use your tenant issuer URL in `jwt.issuer`
- Azure AD or Entra ID: use the tenant authority URL in `jwt.issuer`
- Keycloak: use the realm issuer URL in `jwt.issuer`
- Firebase or another OIDC-compatible provider: use the provider issuer and discovery path it exposes

Important behavior:

- the package concatenates `jwt.issuer` and `jwt.metadataAddress`
- this path validates tokens from discovered keys rather than local signing material
- unlike `AddJwt()`, this method does not register `IJwtHandler`, `IAccessTokenService`, or authorization services

### Recipe 4: RSA XML Key JWT Validation

Use this when the signing key arrives as RSA XML key material rather than a symmetric secret or certificate.

```csharp
using Genocs.Auth;
using Genocs.Core.Builders;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
        .AddGenocs()
        .AddPrivateKeyJwt();

builder.Services.AddAuthorization();
genocs.Build();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

Configuration:

```json
{
    "jwt": {
        "issuerSigningKey": "<RSAKeyValue><Modulus>...</Modulus><Exponent>...</Exponent></RSAKeyValue>",
        "validIssuer": "https://identity.internal",
        "validAudience": "orders-api",
        "validateIssuer": true,
        "validateAudience": true,
        "validateLifetime": true,
        "validateIssuerSigningKey": true
    }
}
```

Important behavior:

- the expected key format is XML RSA key material
- this path is validation-oriented and does not register token tooling
- missing `jwt.issuerSigningKey` throws at startup

### Recipe 5: Manual JWT-Or-API-Key Hybrid

Use this only when an endpoint must accept either bearer token auth or the `x-gnx-apikey` header.

```csharp
using Genocs.Auth;
using Genocs.Auth.Attributes;

var app = builder.Build();
app.UseMiddleware<JwtOrApiKeyAuthenticationMiddleware>();

[ApiKeyOrJwtAuthorize]
public sealed class SystemController : ControllerBase
{
}
```

Configuration:

```json
{
    "Authorization": {
        "Enabled": true,
        "ApiKeys": ["prod-key-1", "prod-key-2"],
        "DevApiKey": "local-dev-key"
    }
}
```

Important behavior:

- this is a public helper, not a first-class builder extension
- it allows either a JWT or an API key, but rejects requests that send both at once with `409`
- it reads `x-gnx-apikey` directly and sets claims manually when the key is valid
- it is safest to treat this as an advanced compatibility mode, not the default architecture

## Pipeline Requirements

### Standard JWT Pipeline

For `AddJwt()`, `AddOpenIdJwt()`, and `AddPrivateKeyJwt()`, the safe default host order is:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

### Revocation Check Middleware

Add this only when the host uses `AddJwt()` and actually wants token blacklisting:

```csharp
app.UseAccessTokenValidator();
```

Safe order:

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseAccessTokenValidator();
```

### Anonymous Path Bypass

`UseAccessTokenValidator()` skips exact paths from `jwt.allowAnonymousEndpoints`.

Example:

```json
{
    "jwt": {
        "allowAnonymousEndpoints": ["/healthz", "/swagger"]
    }
}
```

Treat the match as exact path matching, not prefix or pattern matching.

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddJwt()` | Register JWT bearer auth with local validation settings | Registers auth, authorization, `IJwtHandler`, `IAccessTokenService`, and revocation middleware services | Assuming it always uses a symmetric key even when a certificate is configured |
| `AddOpenIdJwt()` | Validate JWTs from an OIDC provider | Uses discovery metadata from `issuer + metadataAddress` | Assuming it registers token issuance or `AddAuthorization()` |
| `AddPrivateKeyJwt()` | Validate JWTs with RSA XML key material | Requires `jwt.issuerSigningKey` in XML RSA format | Assuming PEM or JWK formats work here |
| `UseAccessTokenValidator()` | Reject blacklisted tokens after auth | Checks `IAccessTokenService` unless the request path is configured anonymous | Assuming revocation is distributed across instances by default |
| `IJwtHandler` | Create and inspect JWTs in application code | Available only through `AddJwt()` | Assuming it exists for every provider mode |
| `IAccessTokenService` | Blacklist and test token activity | Default implementation is in-memory only | Assuming revocation survives app restart or scale-out |
| `JwtAuthAttribute` | Apply bearer authorization at attribute level | Uses the `Bearer` authentication scheme | Assuming it creates the scheme registration |
| `AuthAttribute` | Apply a chosen auth scheme and optional policy | Thin wrapper over `AuthorizeAttribute` | Assuming it configures policies |
| `ApiKeyOrJwtAuthorizeAttribute` | Enforce manual JWT-or-API-key access | Depends on middleware having populated `HttpContext.User` | Assuming it validates the API key itself |
| `JwtOrApiKeyAuthenticationMiddleware` | Support manual bearer-or-api-key auth | Reads `x-gnx-apikey` or `Authorization` header directly | Treating it as a polished package registration feature |

## Public Capability Map

### Registration And Pipeline

- `AddJwt()`
- `AddOpenIdJwt()`
- `AddPrivateKeyJwt()`
- `UseAccessTokenValidator()`

### Token Tooling

- `IJwtHandler`
- `JsonWebToken`
- `JsonWebTokenPayload`
- `JwtOptions`

### Revocation

- `IAccessTokenService`

### Attributes And Advanced Helpers

- `JwtAuthAttribute`
- `AuthAttribute`
- `ApiKeyOrJwtAuthorizeAttribute`
- `JwtOrApiKeyAuthenticationMiddleware`

## Configuration Ownership

`Genocs.Auth` owns the `jwt` section.

Key fields the package actively uses:

- `enabled`
- `allowAnonymousEndpoints`
- `certificate.location`
- `certificate.rawData`
- `certificate.password`
- `algorithm`
- `issuer`
- `issuerSigningKey`
- `authority`
- `audience`
- `challenge`
- `metadataAddress`
- `saveToken`
- `saveSigninToken`
- `requireAudience`
- `requireHttpsMetadata`
- `requireExpirationTime`
- `requireSignedTokens`
- `expiryMinutes`
- `expiry`
- `validAudience`
- `validAudiences`
- `validIssuer`
- `validIssuers`
- `validateActor`
- `validateAudience`
- `validateIssuer`
- `validateLifetime`
- `validateTokenReplay`
- `validateIssuerSigningKey`
- `refreshOnIssuerKeyNotFound`
- `includeErrorDetails`
- `authenticationType`
- `nameClaimType`
- `roleClaimType`

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume every named identity provider has its own extension method. Most external providers map to `AddOpenIdJwt()`.
2. Do not assume `AddOpenIdJwt()` or `AddPrivateKeyJwt()` register `IJwtHandler`, `IAccessTokenService`, or authorization services.
3. Do not assume revoked-token checks work across multiple app instances with the default implementation.
4. Do not assume a certificate without a private key can issue tokens successfully.
5. Do not assume PEM, DER, or JWK key formats are supported by `AddPrivateKeyJwt()`.
6. Do not assume `allowAnonymousEndpoints` performs wildcard, prefix, or regex matching.
7. Do not assume the manual JWT-or-API-key helper is a first-class provider mode.
8. Do not assume `jwt.enabled = false` means the app is secure by default. It bypasses authentication through a disabled policy evaluator path.
9. Do not assume client-certificate auth belongs to this package. That path lives in `Genocs.WebApi.Security`.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Auth`, answer these questions:

1. Does the app need to issue tokens, only validate them, or both?
2. Is the signing source a shared secret, certificate, OIDC discovery endpoint, or RSA XML key?
3. Is `Genocs.Core` already installed so `IGenocsBuilder` is available?
4. Does the host already call `UseAuthentication()` and `UseAuthorization()`?
5. Is token revocation required across multiple app instances?
6. Are provider-specific claims mapped correctly for the app's authorization rules?
7. Is the app actually asking for client-certificate auth, which belongs to another package?
8. Is the API-key hybrid helper really necessary, or would standard bearer auth be simpler and safer?

If any answer is unknown, prefer the simplest provider mode that matches the available key material and avoid claiming runtime capabilities that require extra infrastructure.

## Common Tasks And Safe Responses

### Task: "Set up JWT auth for an internal service"

Safe response:

- use `AddJwt()`
- configure `issuerSigningKey`, issuer, and audience values
- add `UseAuthentication()` and `UseAuthorization()`

### Task: "Integrate Auth0, Azure AD, Keycloak, Okta, or Firebase"

Safe response:

- use `AddOpenIdJwt()`
- configure `issuer`, `metadataAddress`, and `audience`
- mention that provider-specific claim mapping may still need host-level authorization work

### Task: "Use a certificate for JWT signing or validation"

Safe response:

- use `AddJwt()` with `jwt.certificate.*`
- mention that token issuance requires a private key

### Task: "Use RSA asymmetric validation"

Safe response:

- use `AddPrivateKeyJwt()`
- ensure the key is in XML RSA format

### Task: "Blacklist tokens immediately"

Safe response:

- use `UseAccessTokenValidator()`
- mention that the built-in implementation is process-local in memory

### Task: "Accept either API key or JWT"

Safe response:

- mention the manual middleware and attribute path
- state clearly that this is an advanced helper, not a builder-registered provider mode

## Failure Modes And Troubleshooting

1. Authenticated requests always return `401`.
Fix: Re-check issuer, audience, and signing material, then verify `UseAuthentication()` runs before authorization and endpoint execution.

2. OIDC authentication cannot discover signing keys.
Fix: Verify that `jwt.issuer + jwt.metadataAddress` resolves to the correct OpenID configuration URL.

3. `IJwtHandler` is missing from DI.
Fix: Use `AddJwt()`. The OIDC and RSA XML registration paths do not register token tooling.

4. Revoked tokens still work on another instance.
Fix: Replace the default in-memory `IAccessTokenService` with a distributed implementation.

5. Startup fails because the issuer signing key is missing.
Fix: For `AddPrivateKeyJwt()`, ensure `jwt.issuerSigningKey` exists and is valid RSA XML key material.

6. Token issuance fails in a certificate-backed setup.
Fix: Confirm the certificate includes a private key. Public-key-only certificates are validation-oriented.

7. Anonymous endpoint bypass does not work.
Fix: Re-check the exact request path value because `allowAnonymousEndpoints` uses exact matching.

8. API-key hybrid auth behaves inconsistently.
Fix: Verify manual middleware registration, `Authorization` config keys, and endpoint use of `ApiKeyOrJwtAuthorizeAttribute`.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.WebApi`
- `Genocs.WebApi.Security`
- `Genocs.WebApi.OpenApi`
- `Genocs.WebApi.CQRS`
- `Genocs.Logging`
- `Genocs.Telemetry`

## One-Line Recommendation For Agents

If you only know that `Genocs.Auth` is installed, choose the JWT registration path that matches the available key material, assume only `AddJwt()` includes token tooling and revocation services, and ask before extending the solution into client-certificate auth or distributed revocation.


