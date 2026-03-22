# Genocs.Auth — Agent Reference Documentation

## Consumer Mode for Agents

- Assume package is installed from NuGet
- Do not rely on repository source code access
- Prefer stable public APIs and extension methods documented here
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

`Genocs.Auth` provides JWT authentication registration, token handling, and optional access-token revocation validation.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Auth` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | JWT authentication and token lifecycle services |
| Core entry points | `AddJwt()`, `AddOpenIdJwt()`, `AddPrivateKeyJwt()`, `UseAccessTokenValidator()` |

## Install

```bash
dotnet add package Genocs.Auth
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Auth;
using Genocs.Core.Builders;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder gnxBuilder = builder
    .AddGenocs()
    .AddJwt();

gnxBuilder.Build();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseAccessTokenValidator();
app.Run();
```

## Configuration

Use the `jwt` section.

```json
{
    "jwt": {
        "enabled": true,
        "algorithm": "HS256",
        "issuer": "https://identity.example.com",
        "issuerSigningKey": "your-secret-key",
        "authority": "https://identity.example.com",
        "audience": "orders-api",
        "challenge": "Bearer",
        "metadataAddress": "/.well-known/openid-configuration",
        "saveToken": true,
        "saveSigninToken": false,
        "requireAudience": true,
        "requireHttpsMetadata": true,
        "requireExpirationTime": true,
        "requireSignedTokens": true,
        "expiryMinutes": 60,
        "validAudience": "orders-api",
        "validAudiences": ["orders-api", "admin-api"],
        "validIssuer": "https://identity.example.com",
        "validIssuers": ["https://identity.example.com"],
        "validateActor": false,
        "validateAudience": true,
        "validateIssuer": true,
        "validateLifetime": true,
        "validateTokenReplay": false,
        "validateIssuerSigningKey": true,
        "refreshOnIssuerKeyNotFound": true,
        "includeErrorDetails": true,
        "authenticationType": "Bearer",
        "nameClaimType": "name",
        "roleClaimType": "Role",
        "allowAnonymousEndpoints": ["/health", "/metrics"],
        "certificate": {
            "location": "certs/signing.pfx",
            "rawData": null,
            "password": "change-me"
        }
    }
}
```

| Setting | Type | Description |
|---|---|---|
| `enabled` | `bool` | Enables or disables JWT authentication. When `false`, auth can be bypassed depending on the registration path. |
| `algorithm` | `string` | Token signing algorithm. Defaults to `HS256`. |
| `issuer` | `string` | Logical token issuer used when issuing or validating tokens. |
| `issuerSigningKey` | `string` | Symmetric key or RSA key material used for signing and validation. |
| `authority` | `string` | Upstream authority used by OpenID-based validation flows. |
| `audience` | `string` | Primary intended audience for issued tokens. |
| `challenge` | `string` | Authentication challenge scheme. Defaults to `Bearer`. |
| `metadataAddress` | `string` | OpenID configuration endpoint path or absolute URL. |
| `saveToken` | `bool` | Persists the validated token in auth properties. |
| `saveSigninToken` | `bool` | Persists the sign-in token when available. |
| `requireAudience` | `bool` | Requires an audience claim during validation. |
| `requireHttpsMetadata` | `bool` | Forces HTTPS metadata retrieval for authority discovery. |
| `requireExpirationTime` | `bool` | Requires an `exp` claim. |
| `requireSignedTokens` | `bool` | Rejects unsigned tokens when enabled. |
| `expiryMinutes` | `int` | Default token lifetime in minutes when `expiry` is not supplied. |
| `expiry` | `TimeSpan` | Explicit token lifetime override. |
| `validAudience` | `string` | Single audience accepted by validators. |
| `validAudiences` | `string[]` | Multiple valid audiences accepted by validators. |
| `validIssuer` | `string` | Single issuer accepted by validators. |
| `validIssuers` | `string[]` | Multiple valid issuers accepted by validators. |
| `validateActor` | `bool` | Enables actor claim validation. |
| `validateAudience` | `bool` | Enables audience validation. |
| `validateIssuer` | `bool` | Enables issuer validation. |
| `validateLifetime` | `bool` | Enables expiration and not-before validation. |
| `validateTokenReplay` | `bool` | Enables replay protection checks when supported. |
| `validateIssuerSigningKey` | `bool` | Forces validation of the issuer signing key. |
| `refreshOnIssuerKeyNotFound` | `bool` | Refreshes metadata when signing keys rotate. |
| `includeErrorDetails` | `bool` | Includes auth failure details in responses and logs. |
| `authenticationType` | `string` | Custom authentication type attached to the identity. |
| `nameClaimType` | `string` | Claim type used as the principal name. |
| `roleClaimType` | `string` | Claim type used for roles. Defaults to `Role`. |
| `allowAnonymousEndpoints` | `string[]` | Exact request paths ignored by the access-token revocation middleware. |
| `certificate.location` | `string` | Path to the signing or validation certificate file. |
| `certificate.rawData` | `string` | Inline certificate payload when loading from configuration. |
| `certificate.password` | `string` | Password for certificate material that requires it. |

Use `issuerSigningKey` for shared-secret or raw RSA-key scenarios. Use `certificate.*` when token signing or validation should rely on X.509 material.

## Decision Matrix For Agents

| If you need to... | Use |
|---|---|
| Enable shared-secret JWT authentication | `AddJwt()` |
| Validate JWTs from OpenID configuration | `AddOpenIdJwt()` |
| Use asymmetric key based JWT validation | `AddPrivateKeyJwt()` |
| Issue and inspect tokens programmatically | `IJwtHandler` |
| Reject deactivated access tokens | `UseAccessTokenValidator()` + `IAccessTokenService` |

## Behavior Notes / Constraints

- `jwt.enabled = false` disables authentication checks and is suitable only for controlled non-production scenarios.
- Default token deactivation state is process-local unless a distributed implementation is provided.
- Token validation behavior depends directly on issuer, audience, signing, and lifetime settings.

## Public Capability Map

- Authentication registration: `AddJwt()`, `AddOpenIdJwt()`, `AddPrivateKeyJwt()`
- Token operations: `IJwtHandler`
- Token deactivation and checks: `IAccessTokenService`, `UseAccessTokenValidator()`
- ASP.NET Core pipeline integration: standard `UseAuthentication()` and `UseAuthorization()`

## Dependencies

- `Genocs.Core`
- `Genocs.Security`
- `Microsoft.AspNetCore.Authentication.JwtBearer`

## Troubleshooting

1. Protected endpoints always return unauthorized responses.
Fix: Confirm `jwt` values (issuer, audience, signing key/certificate) and middleware order in the HTTP pipeline.
2. A revoked token still succeeds on another instance.
Fix: Replace default in-memory token deactivation storage with a distributed implementation.
3. Application fails during JWT startup validation.
Fix: Validate signing material format and ensure issuer/audience settings match token contents.


