# Genocs.Http Implementation Backlog

## Purpose

This backlog translates observed architectural, correctness, and maintainability concerns in Genocs.Http into issue-sized implementation work.

The backlog follows the same execution-oriented model used by Genocs.Telemetry and is ordered by delivery risk, not namespace.

## Current Status

Observed baseline (April 2026, source review plus follow-up implementation):

- Genocs.Http targets net10.0, net9.0, and net8.0.
- `AddHttpClient(...)` registers a single typed `IHttpClient` backed by `GenocsHttpClient`.
- The package provides a default `System.Text.Json` serializer through `SystemTextJsonHttpClientSerializer`.
- Retry behavior is implemented in `GenocsHttpClient` with HTTP-aware, transport-focused Polly conditions (**HTTP-005 done**): retries are limited to transient send failures, non-success HTTP status codes are not retried by default, and POST/PUT/PATCH retries require explicit opt-in.
- String-based `*ResultAsync<T>(...)` flows use a dedicated non-throwing send path (`SendAllowingNonSuccessStatusAsync` / shared `SendWithRetryAsync`); non-success responses are returned in `HttpResult<T>` without throwing solely for HTTP error status codes (**HTTP-001 done**).
- `HttpRequestMessage` overloads retry the same request instance, which is unsafe after a request has already been sent.
- Public `HttpClientOptions` fields `enabled`, `type`, and `services` exist in the configuration contract, but runtime registration in this package does not consume them.
- Correlation fallback factories return null through non-nullable `string` contracts.
- `src/tests/Genocs.Http.UnitTests` exists with regression tests for string `*ResultAsync`, request URI / `BaseAddress`, retry boundaries, cancellation semantics, correlation header propagation, masking behavior, and DI registration composition; remaining HTTP-013 work is to keep this suite complete as new fixes land and ensure explicit CI/solution execution coverage.
- String request URIs use `System.Uri` rules only: no implicit `http://` prefix; relative paths combine with `HttpClient.BaseAddress` (**HTTP-002 done**).

**Improvements delivered with HTTP-001 (April 2026)**

- **Runtime**: Refactored string-based sends into `SendWithRetryAsync(..., throwOnNonSuccessStatus)` so retries apply to transport failures, not to HTTP error status codes on the result path. New `SendAllowingNonSuccessStatusAsync` (protected virtual) feeds `SendResultAsync<T>(string, ...)`; string-based exception-oriented `SendAsync` unchanged for `GetAsync` / `PostAsync` / typed `*Async<T>` helpers.
- **API docs**: Extended `IHttpClient` and `HttpResult<T>` XML documentation; clarified `SendAsync<T>(HttpRequestMessage)` vs `SendResultAsync<T>(HttpRequestMessage)`.
- **Consumer docs**: [README_NUGET.md](../src/Genocs.Http/README_NUGET.md) “Response handling” section; [Genocs.Http-Agent-Documentation.md](Genocs.Http-Agent-Documentation.md) alignment for result vs exception-oriented helpers.
- **Tests**: `Genocs.Http.UnitTests` / `StringResultAsyncTests` (e.g. GET 404, POST 400) validating non-throwing `HttpResult<T>` semantics.
- **Validation**: `dotnet build` / `dotnet test` on `Genocs.Http` and `Genocs.Http.UnitTests` succeed for these changes.

**Improvements delivered with HTTP-002 (April 2026)**

- **Runtime**: `ParseRequestUri` (`Uri.TryCreate`, trim) replaces the `http`-prefix heuristic; `GetResponseAsync` takes `Uri` and forwards to `HttpClient` overloads. Retries unchanged; parsing runs once per send attempt.
- **API / registration docs**: `IHttpClient` URI semantics; `Extensions.AddHttpClient` XML documents configuring `BaseAddress` via `IHttpClientBuilder`.
- **Consumer docs**: README “Request URIs and `BaseAddress`”; agent Recipe 2 + migration note for removed implicit prefix.
- **Tests**: `RequestUriTests` (relative + `BaseAddress`, absolute ignoring base, invalid URI `ArgumentException`, whitespace).

Next recommended items:

- Complete remaining **M1** item: **HTTP-003** (configuration contract alignment).
- Continue **M4**: **HTTP-013** to **HTTP-016** (test execution guarantees, documentation alignment, and quality gates).

## Planning Assumptions

- Fix correctness and public-contract mismatches before changing resilience breadth or feature surface.
- Keep Genocs.Http focused on outbound request execution, registration, serialization, and request-level enrichment.
- Avoid hidden behavior that overrides standard `HttpClientFactory` semantics such as `BaseAddress`, handler composition, and cancellation.
- Prefer additive integration with ASP.NET Core and `HttpClientFactory` pipelines over global replacement behavior.
- Pair every behavioral correction with focused tests and migration notes because several fixes will be observable by consumers.

## Functional Overlap Issues To Track

1. Resilience overlap:
- Genocs.Http applies its own Polly retries inside the client implementation.
- Consumers may also attach retry, timeout, or circuit-breaker handlers through `IHttpClientBuilder`.
- Without a clear ownership model, requests can be retried multiple times unintentionally.

2. HttpClientFactory pipeline overlap:
- Genocs.Http replaces `IHttpMessageHandlerBuilderFilter` when request masking is enabled.
- ASP.NET Core and other packages may also register builder filters for logging, metrics, or diagnostics.
- Replacing instead of composing can suppress expected framework or package behavior.

3. Configuration overlap ambiguity:
- `httpClient.enabled`, `httpClient.type`, and `httpClient.services` imply runtime service-discovery or registration behavior.
- This package does not currently enforce or consume those settings itself.
- Consumers can reasonably assume features exist that are actually no-op in Genocs.Http.

4. Correlation ownership overlap:
- Genocs.Http can add correlation headers through factories.
- Logging and telemetry packages may also enrich outbound requests or log scopes with correlation data.
- Without request-scoped ownership guidance, correlation can become stale, duplicated, or misleading.

## Milestones

| Milestone | Goal | Tasks |
|---|---|---|
| M1 | Contract correctness and public API alignment | HTTP-001 to HTTP-004 |
| M2 | Retry safety, cancellation correctness, and response lifecycle hardening | HTTP-005 to HTTP-008 |
| M3 | DI composition, logging integration, and extensibility hardening | HTTP-009 to HTTP-012 |
| M4 | Tests, documentation, migration guidance, and quality gates | HTTP-013 to HTTP-016 |

## Execution Order

1. Complete M1 before changing resilience behavior so request/result semantics are stable.
2. Complete M2 before recommending Genocs.Http for production write paths or custom `HttpRequestMessage` flows.
3. Complete M3 before combining this package broadly with logging, telemetry, or downstream client composition packages.
4. Use M4 to lock in regression coverage, adoption guidance, and package quality gates.

---

## M1: Contract Correctness and Public API Alignment

### HTTP-001 Make string-based `*ResultAsync<T>(...)` preserve non-success responses

**Status**: Done (implemented April 2026)

**Priority**: P0

**Problem**

Previously, the string-based `GetResultAsync<T>`, `PostResultAsync<T>`, `PutResultAsync<T>`, `PatchResultAsync<T>`, and `DeleteResultAsync<T>` flows delegated to a send path that threw on non-success status codes. Non-success responses therefore raised exceptions instead of returning `HttpResult<T>` with the raw response.

**Scope**

- split response acquisition from exception-oriented success enforcement
- make all string-based `*ResultAsync<T>(...)` methods preserve non-success `HttpResponseMessage` values
- remove dead-code status checks that are unreachable today
- align public documentation with the actual result contract

**Likely touch points**

- [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- [src/Genocs.Http/IHttpClient.cs](src/Genocs.Http/IHttpClient.cs)
- [src/Genocs.Http/HttpResult.cs](src/Genocs.Http/HttpResult.cs)
- [src/Genocs.Http/README_NUGET.md](src/Genocs.Http/README_NUGET.md)
- [docs/Genocs.Http-Agent-Documentation.md](Genocs.Http-Agent-Documentation.md)

**Acceptance criteria**

- all string-based `*ResultAsync<T>(...)` methods return `HttpResult<T>` on non-success responses without throwing
- exception-oriented and result-oriented APIs have distinct, documented behavior
- no dead success-check branches remain in the result path

**Dependencies**

- none

**Implementation notes**

- introduce an internal non-throwing response method for verb-based calls
- keep exception-oriented APIs explicit instead of making all public methods silently swallow failures
- ensure retry decisions are based on the send outcome, not on post-processing of result-wrapper methods

**Delivered**

- `GenocsHttpClient`: private `SendWithRetryAsync`; protected `SendAllowingNonSuccessStatusAsync` (non-success status preserved); string `SendResultAsync<T>` uses the non-throwing path; string `SendAsync` still throws on non-success for exception-oriented APIs.
- Documentation and tests as listed under **Current Status → Improvements delivered with HTTP-001**.

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

### HTTP-002 Restore standard URI handling and `BaseAddress` compatibility

**Status**: Done (implemented April 2026)

**Priority**: P0

**Problem**

Previously, `GenocsHttpClient` prepended `http://` to any URI string that did not start with `http`, which bypassed normal `HttpClient` relative-URI and `BaseAddress` composition and could mis-route requests.

**Scope**

- remove forced `http://` prefixing
- support standard absolute and relative URI behavior
- fail predictably for invalid request URIs
- document how consumers should pass service-host and path values

**Likely touch points**

- [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- [src/Genocs.Http/IHttpClient.cs](src/Genocs.Http/IHttpClient.cs)
- [src/Genocs.Http/Extensions.cs](src/Genocs.Http/Extensions.cs)
- [src/Genocs.Http/README_NUGET.md](src/Genocs.Http/README_NUGET.md)
- [docs/Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md)

**Acceptance criteria**

- relative URIs resolve through `HttpClient.BaseAddress` when configured
- absolute URIs are preserved without mutation
- invalid URI input fails with a clear exception rather than implicit rewriting

**Dependencies**

- none

**Implementation notes**

- use `Uri` parsing rules instead of string prefix heuristics
- document whether service-name-based addressing should be expressed through `BaseAddress`, DNS, or companion discovery packages
- treat URI construction as transport configuration, not as a hidden convenience behavior

**Delivered**

- `ParseRequestUri` + `GetResponseAsync(Uri, ...)`; string-based sends parse once per operation (before Polly retry).
- Documentation and tests as listed under **Current Status → Improvements delivered with HTTP-002**.

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

### HTTP-003 Enforce or remove unused `httpClient` configuration fields

**Status**: Not started (assessed April 2026)

**Priority**: P1

**Problem**

`HttpClientOptions` exposes `enabled`, `type`, and `services`, but Genocs.Http runtime code does not enforce or consume those values. This creates a misleading public configuration contract.

**Scope**

- decide whether `httpClient.enabled` is authoritative for registration
- either implement or remove package-level semantics for `type` and `services`
- separate companion-package configuration from Genocs.Http-only configuration if needed
- update documentation to reflect the real runtime contract

**Likely touch points**

- [src/Genocs.Http/Configurations/HttpClientOptions.cs](src/Genocs.Http/Configurations/HttpClientOptions.cs)
- [src/Genocs.Http/Extensions.cs](src/Genocs.Http/Extensions.cs)
- [src/Genocs.Http/README_NUGET.md](src/Genocs.Http/README_NUGET.md)
- [docs/Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md)

**Acceptance criteria**

- every public `httpClient` setting has explicit, testable runtime behavior or is removed from this package contract
- documentation no longer implies discovery or load-balancing behavior that Genocs.Http does not implement

**Dependencies**

- none

**Implementation notes**

- prefer shrinking the contract over keeping no-op flags
- if `type` and `services` belong to higher-level packages, move that guidance out of Genocs.Http package docs
- if `enabled` remains, it should be authoritative and deterministic

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

### HTTP-004 Normalize nullability and correlation factory contracts

**Status**: Completed (implemented April 2026)

**Priority**: P1

**Problem**

`ICorrelationContextFactory.Create()` and `ICorrelationIdFactory.Create()` return non-nullable `string`, while empty fallback implementations return `null`. Several option properties also use non-nullable `string` without safe defaults.

**Scope**

- decide whether correlation factory results are nullable or guaranteed non-null
- align interface signatures, fallback implementations, and header-addition logic
- remove `default!` suppressions where they currently hide public contract mismatches
- tighten option defaults or nullability declarations

**Likely touch points**

- [src/Genocs.Http/ICorrelationContextFactory.cs](src/Genocs.Http/ICorrelationContextFactory.cs)
- [src/Genocs.Http/ICorrelationIdFactory.cs](src/Genocs.Http/ICorrelationIdFactory.cs)
- [src/Genocs.Http/EmptyCorrelationContextFactory.cs](src/Genocs.Http/EmptyCorrelationContextFactory.cs)
- [src/Genocs.Http/EmptyCorrelationIdFactory.cs](src/Genocs.Http/EmptyCorrelationIdFactory.cs)
- [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- [src/Genocs.Http/Configurations/HttpClientOptions.cs](src/Genocs.Http/Configurations/HttpClientOptions.cs)

**Acceptance criteria**

- public nullability contracts match actual runtime behavior
- fallback correlation implementations do not violate interface guarantees
- header propagation code handles missing correlation values explicitly and safely

**Dependencies**

- none

**Implementation notes**

- prefer `string?` when absence is a supported outcome
- treat option properties as nullable or initialize them with safe defaults
- consider enabling package-level nullable warnings as errors after contract cleanup

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`

**Dependent activities outside Genocs.Http (tracking)**

The HTTP-004 nullable contract update is implemented in `Genocs.Http`. The following downstream activities should be tracked to keep dependent projects aligned with the new `ICorrelationIdFactory`/`ICorrelationContextFactory` semantics (`string?` output allowed):

| Project | Affected files | Follow-up activity | Status |
|---|---|---|---|
| ApiGateway (`src/apps/apigateway`) | `WebApi/Framework/LogContextMiddleware.cs` | Guard log enrichment when correlation ID is null or whitespace (decide: omit property vs generate fallback ID). | Not started |
| ApiGateway (`src/apps/apigateway`) | `WebApi/Framework/MessagingMiddleware.cs` | Ensure message correlation ID handling is explicit when factory output is missing (generate deterministic fallback before publish). | Not started |
| ApiGateway (`src/apps/apigateway`) | `WebApi/Framework/CustomForwarderHttpClientFactory.cs` | Add conditional header propagation for `x-correlation-id` (avoid sending blank header values). | Not started |
| Identities (`src/apps/identities`) | `Application/Services/MessageBroker.cs` | Handle nullable correlation ID before passing to outbox/bus publisher APIs; define fallback behavior. | Not started |
| Identities (`src/apps/identities`) | `Application/Logging/LogContextMiddleware.cs` | Make log context enrichment resilient to nullable correlation IDs. | Not started |
| Identities (`src/apps/identities`) | `Application/Decorators/LoggingCommandHandlerDecorator.cs`, `Application/Decorators/LoggingEventHandlerDecorator.cs` | Apply nullable-safe correlation handling in decorator log scopes. | Not started |
| Identities (`src/apps/identities`) | `Application/CorrelationIdFactory.cs` | Decide and document contract intent: keep non-null guarantee (implementation returns `string`) or align signature/docs to nullable interface explicitly. | Not started |
| Companion HTTP clients | `src/Genocs.ServiceDiscovery.Consul/Http/ConsulHttpClient.cs`, `src/Genocs.LoadBalancing.Fabio/Http/FabioHttpClient.cs` | Add/verify regression tests asserting missing correlation values do not produce outbound headers through inherited `GenocsHttpClient` behavior. | Not started |

**Suggested validation for dependent activities**

- `dotnet build src/apps/apigateway/WebApi/Host.csproj -c Debug --nologo`
- `dotnet build src/apps/identities/Application/Application.csproj -c Debug --nologo`

---

## M2: Retry Safety, Cancellation Correctness, and Response Lifecycle Hardening

### HTTP-005 Replace blanket exception retry with HTTP-aware resilience behavior

**Status**: Completed (implemented April 2026)

**Priority**: P0

**Problem**

The current retry policy handles every `Exception`, including non-transient failures, client-side 4xx responses, and deserialization problems. It also retries non-idempotent operations by default.

**Scope**

- replace `Policy.Handle<Exception>()` with transport- and status-aware retry conditions
- avoid retrying 4xx responses and local serialization/deserialization failures
- make retry behavior method-aware, with conservative defaults for POST, PUT, and PATCH
- document retry ownership when consumers also attach handlers through `IHttpClientBuilder`

**Likely touch points**

- [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- [src/Genocs.Http/Extensions.cs](src/Genocs.Http/Extensions.cs)
- [src/Genocs.Http/Configurations/HttpClientOptions.cs](src/Genocs.Http/Configurations/HttpClientOptions.cs)
- [src/Genocs.Http/README_NUGET.md](src/Genocs.Http/README_NUGET.md)

**Acceptance criteria**

- retries occur only for explicitly supported transient conditions
- default behavior does not retry deterministic caller or payload errors
- write-method retry behavior is explicit and documented

**Dependencies**

- HTTP-001
- HTTP-002

**Implementation notes**

- prefer response-aware resilience primitives or a bounded custom strategy
- avoid throwing generic `Exception` solely to drive retry behavior
- keep retry policy narrow enough that consumer-added resilience handlers do not compound unexpectedly

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

**Delivered**

- **Runtime**: Replaced blanket `Policy.Handle<Exception>()` retries with HTTP-aware transport retry policy in `GenocsHttpClient`.
	- Retries now target transient send failures (`HttpRequestException` retry predicate + `IOException`).
	- Non-success status codes are surfaced to callers without being used to trigger retries.
	- Typed `HttpRequestMessage` flows no longer retry deserialization failures because deserialization is outside the retry boundary.
- **Method-aware behavior**: Added `HttpClientOptions.RetryUnsafeHttpMethods` (default `false`) so `POST`/`PUT`/`PATCH` retries are explicit opt-in.
- **Consumer docs**: Updated `README_NUGET.md` retry semantics and configuration guidance (`httpClient.retries`, `httpClient.retryUnsafeHttpMethods`).
- **Tests**: Added `RetryBehaviorTests` in `Genocs.Http.UnitTests` covering transient retry behavior, unsafe-method default/opt-in behavior, non-success no-retry behavior, and no retry on deserialization errors.

**Dependent activities outside Genocs.Http (tracking)**

HTTP-005 changes retry semantics and defaults. The following downstream activities should be tracked to align service behavior and operational expectations:

| Project | Affected files | Follow-up activity | Status |
|---|---|---|---|
| ApiGateway (`src/apps/apigateway`) | `WebApi/appsettings.json` | Review `httpClient.retries` usage and decide whether write operations require `httpClient.retryUnsafeHttpMethods=true` for this service. | Not started |
| Products (`src/apps/products`) | `WebApi/appsettings.json` | Validate write-call resilience expectations with new default (no POST/PUT/PATCH retries unless opted in). | Not started |
| Orders (`src/apps/orders`) | `WebApi/appsettings.json` | Reassess retry ownership between Genocs.Http and any pipeline-level resilience to avoid compounded retries. | Not started |
| Notifications (`src/apps/notifications`) | `WebApi/appsettings.json` | Confirm write flows tolerate transport failures under conservative retry defaults; opt in explicitly only if idempotency safeguards exist. | Not started |
| Identities (`src/apps/identities`) | `WebApi/appsettings.json`, `Application/Extensions.cs` | Verify Application/WebApi compositions have consistent retry intent and update configuration/docs if write retries must be enabled. | Not started |
| Demo hosts | `src/apps/WebApi/appsettings.json`, `src/apps/ServiceBus.Worker/appsettings.json`, `src/apps/Masstransit.WebApi/appsettings.json`, `src/apps/Masstransit.Worker/appsettings.json` | Update sample configuration/docs to reflect new retry defaults and optional `retryUnsafeHttpMethods`. | Not started |

### HTTP-006 Stop retrying the same `HttpRequestMessage` instance

**Status**: Done (implemented April 2026)

**Priority**: P0

**Problem**

The `SendAsync(HttpRequestMessage, ...)`, `SendAsync<T>(HttpRequestMessage, ...)`, and `SendResultAsync<T>(HttpRequestMessage, ...)` methods place Polly retries around `_client.SendAsync(request, ...)`. Re-sending the same `HttpRequestMessage` after it has already been sent is unsafe and can fail immediately or behave unpredictably with request content.

**Scope**

- remove internal retries from `HttpRequestMessage` overloads or implement safe cloning
- decide whether buffered-content cloning is supported and under what constraints
- document the request-message retry model explicitly

**Likely touch points**

- [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- [src/Genocs.Http/IHttpClient.cs](src/Genocs.Http/IHttpClient.cs)
- [src/Genocs.Http/README_NUGET.md](src/Genocs.Http/README_NUGET.md)
- [docs/Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md)

**Acceptance criteria**

- request-message overloads no longer retry an already-sent request instance
- if retry support remains, it uses safe request reconstruction with documented limits
- behavior is covered by tests for content-bearing requests

**Dependencies**

- HTTP-005

**Implementation notes**

- the safest first step is to remove internal retries from custom-request overloads
- if cloning is introduced later, only support it for fully buffered content and copied headers
- do not hide replay limitations behind best-effort behavior

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

**Delivered**

- **Runtime**: Removed internal Polly replay for `HttpRequestMessage` overloads in `GenocsHttpClient` (`SendAsync(HttpRequestMessage, ...)`, `SendAsync<T>(HttpRequestMessage, ...)`, `SendResultAsync<T>(HttpRequestMessage, ...)`). Each call now sends the provided request instance once.
- **API docs**: Updated `IHttpClient` XML documentation to make the single-send request-message model explicit.
- **Consumer docs**: Updated `README_NUGET.md` and [Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md) with explicit request-message retry semantics.
- **Tests**: Extended `RetryBehaviorTests` with request-message transport failure coverage (including a content-bearing POST request) asserting there is no replay retry for the same message instance.

**Dependent activities outside Genocs.Http (tracking)**

HTTP-006 changes the resilience model for call sites that build and pass `HttpRequestMessage` directly. The following downstream activities should be tracked:

| Project | Affected files | Follow-up activity | Status |
|---|---|---|---|
| ApiGateway (`src/apps/apigateway`) | `WebApi/Application/Services/*` | Audit custom `HttpRequestMessage` call sites and move retry ownership to `HttpClientFactory` handlers where replay-safe behavior is needed. | Not started |
| Products (`src/apps/products`) | `WebApi/Application/Services/*` | Validate write-path idempotency and add explicit resilience handlers for any custom request-message flows. | Not started |
| Orders (`src/apps/orders`) | `WebApi/Application/Services/*` | Reassess outbound message/client wrappers that construct `HttpRequestMessage` to ensure expected retry policy still exists at pipeline level. | Not started |
| Notifications (`src/apps/notifications`) | `WebApi/Application/Services/*` | Confirm notification dispatch call paths using `HttpRequestMessage` are resilient through handler policies, not Genocs.Http internal replay. | Not started |
| Demo hosts | `src/apps/**` outbound integration classes | Update apps guidance to show where retry is configured when using custom `HttpRequestMessage` APIs. | Not started |

### HTTP-007 Preserve cancellation semantics and avoid retrying cancellations

**Status**: Done (implemented April 2026)

**Priority**: P1

**Problem**

Because retries handle all exceptions, user-triggered cancellation and some timeout-related cancellation flows can be retried even though cancellation should terminate work immediately.

**Scope**

- exclude `OperationCanceledException` and cancellation-driven failures from retry handling
- ensure cancellation tokens are passed consistently through send, stream, and deserialize steps
- document cancellation guarantees for callers

**Likely touch points**

- [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- [src/Genocs.Http/IHttpClient.cs](src/Genocs.Http/IHttpClient.cs)
- [src/Genocs.Http/README_NUGET.md](src/Genocs.Http/README_NUGET.md)

**Acceptance criteria**

- cancelled operations stop immediately and are not retried
- typed and raw-response methods behave consistently under cancellation
- documentation states cancellation behavior clearly

**Dependencies**

- HTTP-005

**Implementation notes**

- preserve caller intent over internal retry convenience
- make cancellation handling explicit instead of depending on exception catch-all behavior
- ensure token propagation is consistent in all deserialize paths

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

**Delivered**

- **Runtime**: Updated retry evaluation to treat cancellation-driven `HttpRequestException` failures as non-retryable by checking the exception chain for `OperationCanceledException`.
- **Runtime**: Completed cancellation-token propagation for string typed response materialization (`DeserializeJsonFromStream<T>` now receives the caller token in `SendAsync<T>(string, ...)`).
- **Consumer docs**: Updated `README_NUGET.md` and [Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md) to state cancellation behavior explicitly.
- **Tests**: Extended `RetryBehaviorTests` with coverage for immediate cancellation (no retries), cancellation wrapped in `HttpRequestException` (no retries), and token propagation to serializer paths.

**Dependent activities outside Genocs.Http (tracking)**

HTTP-007 tightens cancellation semantics. The following downstream activities should be tracked:

| Project | Affected files | Follow-up activity | Status |
|---|---|---|---|
| ApiGateway (`src/apps/apigateway`) | `WebApi/Application/Services/*` | Ensure any explicit retry wrappers skip `OperationCanceledException`/`TaskCanceledException` and preserve caller cancellation intent. | Not started |
| Products (`src/apps/products`) | `WebApi/Application/Services/*` | Verify outbound application-service calls pass request tokens end-to-end (controller -> service -> `IHttpClient`) without substituting `CancellationToken.None`. | Not started |
| Orders (`src/apps/orders`) | `WebApi/Application/Services/*`, `Infrastructure/*` | Audit timeout/cancellation handling to avoid wrapping cancellation as generic transient errors that trigger unrelated retries. | Not started |
| Notifications (`src/apps/notifications`) | `WebApi/Application/Services/*` | Confirm long-running notification fan-out paths honor cancellation and terminate promptly when upstream token is canceled. | Not started |
| Demo hosts | `src/apps/**` integration call sites | Update apps recipes to show passing `CancellationToken` through typed calls and avoiding cancellation retries in custom resilience handlers. | Not started |

### HTTP-008 Dispose transient responses and separate send failures from payload failures

**Status**: Done (implemented April 2026)

**Priority**: P1

**Problem**

Typed methods deserialize response bodies without disposing the `HttpResponseMessage` when the response is not returned to the caller. In addition, deserialization failures currently occur inside the retry scope, which can trigger unnecessary outbound replays for payload-shape errors.

**Scope**

- dispose `HttpResponseMessage` instances in flows that do not return them
- move deserialization outside the retry boundary, or otherwise ensure payload errors are not retried
- apply consistent disposal behavior to all generic helper methods

**Likely touch points**

- [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- [src/Genocs.Http/IHttpClientSerializer.cs](src/Genocs.Http/IHttpClientSerializer.cs)
- [src/Genocs.Http/SystemTextJsonHttpClientSerializer.cs](src/Genocs.Http/SystemTextJsonHttpClientSerializer.cs)

**Acceptance criteria**

- typed methods do not leak response resources
- malformed payloads fail once instead of triggering transport retries
- response ownership is clear across raw, typed, and result-wrapped APIs

**Dependencies**

- HTTP-001
- HTTP-005

**Implementation notes**

- separate transport execution from payload materialization
- dispose responses in non-returning helper paths after content has been consumed
- keep ownership explicit when `HttpResult<T>` or raw `HttpResponseMessage` is returned

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

**Delivered**

- **Runtime**: Typed methods that return `T?` now dispose transient `HttpResponseMessage` instances in both success and non-success paths (`SendAsync<T>(string, ...)`, `SendAsync<T>(HttpRequestMessage, ...)`).
- **Runtime**: Payload materialization remains outside the retry boundary for string typed helpers, so deserialization failures fail once without transport replay.
- **Consumer docs**: Updated `README_NUGET.md` and [Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md) with explicit response ownership guidance.
- **Tests**: Extended `RetryBehaviorTests` with coverage for response disposal in typed flows and no-retry behavior for string-URI deserialization failures.

**Dependent activities outside Genocs.Http (tracking)**

HTTP-008 changes response lifecycle and ownership expectations. The following downstream activities should be tracked:

| Project | Affected files | Follow-up activity | Status |
|---|---|---|---|
| ApiGateway (`src/apps/apigateway`) | `WebApi/Application/Services/*` | Audit typed outbound wrappers to ensure they do not retain or double-dispose responses that are now disposed inside `T?` methods. | Not started |
| Products (`src/apps/products`) | `WebApi/Application/Services/*` | Verify no call sites expect to access response metadata after typed `T?` calls; migrate those paths to `HttpResult<T>` or raw-response APIs where needed. | Not started |
| Orders (`src/apps/orders`) | `WebApi/Application/Services/*`, `Infrastructure/*` | Confirm payload materialization errors are treated as single-attempt failures in any external resilience wrappers and not reclassified as retryable transport faults. | Not started |
| Notifications (`src/apps/notifications`) | `WebApi/Application/Services/*` | Validate background notification flows do not leak response resources when using typed helper methods under high-throughput dispatch. | Not started |
| Demo hosts | `src/apps/**` outbound integration examples | Update examples to clarify when to choose typed `T?`, `HttpResult<T>`, or raw `HttpResponseMessage` based on response ownership needs. | Not started |

---

## M3: DI Composition, Logging Integration, and Extensibility Hardening

### HTTP-009 Remove `BuildServiceProvider()` from registration flow

**Status**: Done (implemented April 2026)

**Priority**: P1

**Problem**

`AddHttpClient(...)` builds a temporary service provider during registration to check whether correlation factories exist. This is a DI anti-pattern that can instantiate singleton graphs early and couples behavior to registration order.

**Scope**

- remove temporary provider creation from `AddHttpClient(...)`
- use `TryAddSingleton` or equivalent additive registration for fallback factories
- keep registration deterministic without resolving services during setup

**Likely touch points**

- [src/Genocs.Http/Extensions.cs](src/Genocs.Http/Extensions.cs)
- [src/Genocs.Http/EmptyCorrelationContextFactory.cs](src/Genocs.Http/EmptyCorrelationContextFactory.cs)
- [src/Genocs.Http/EmptyCorrelationIdFactory.cs](src/Genocs.Http/EmptyCorrelationIdFactory.cs)

**Acceptance criteria**

- registration does not create a temporary provider
- fallback correlation factories are added only when no custom implementation is registered
- package registration is compatible with normal ASP.NET Core DI composition

**Dependencies**

- HTTP-004

**Implementation notes**

- prefer additive registration primitives over service resolution during setup
- keep the registration path side-effect free
- do not require consumers to register custom factories before or after a specific package call order

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

**Delivered**

- **Runtime**: Removed temporary provider creation from `Extensions.AddHttpClient(...)`; registration no longer resolves services during setup.
- **Runtime**: Switched fallback factory registration to additive DI primitives (`TryAddSingleton`) for `ICorrelationContextFactory` and `ICorrelationIdFactory`.
- **Tests**: Added `ExtensionsRegistrationTests` covering both behaviors:
	- custom correlation factories are not instantiated during registration
	- empty fallback factories are registered only when custom factories are missing

**Dependent activities outside Genocs.Http (tracking)**

HTTP-009 changes registration composition behavior and reduces side effects at startup. The following downstream activities should be tracked:

| Project | Affected files | Follow-up activity | Status |
|---|---|---|---|
| ApiGateway (`src/apps/apigateway`) | `WebApi/Program.cs`, `WebApi/Extensions/*` | Verify startup order assumptions do not rely on early factory instantiation side effects from HTTP registration. | Not started |
| Products (`src/apps/products`) | `WebApi/Program.cs`, `WebApi/Extensions/*` | Confirm custom correlation factory registrations continue to take precedence and no duplicate fallback behavior is assumed. | Not started |
| Orders (`src/apps/orders`) | `WebApi/Program.cs`, `WebApi/Extensions/*` | Validate composition with additional DI modules to ensure no module depended on AddHttpClient creating an intermediate provider. | Not started |
| Notifications (`src/apps/notifications`) | `WebApi/Program.cs`, `WebApi/Extensions/*` | Re-check initialization diagnostics/logging that may have previously observed early correlation-factory construction. | Not started |
| Demo hosts | `src/apps/**/Program.cs` | Align apps startup guidance with additive registration behavior and no temporary provider creation. | Not started |

### HTTP-010 Stop replacing global `IHttpMessageHandlerBuilderFilter`

**Status**: Done (implemented April 2026)

**Priority**: P0

**Problem**

When request masking is enabled, Genocs.Http uses `Replace(...)` for `IHttpMessageHandlerBuilderFilter`. This can remove framework or package-added filters instead of composing with them.

**Scope**

- register masking behavior additively instead of globally replacing the service
- scope request masking to the Genocs client registration rather than the entire handler-builder pipeline
- preserve compatibility with framework logging, diagnostics, and package-added filters

**Likely touch points**

- [src/Genocs.Http/Extensions.cs](src/Genocs.Http/Extensions.cs)
- [src/Genocs.Http/GenocsLoggingScopeHttpMessageHandler.cs](src/Genocs.Http/GenocsLoggingScopeHttpMessageHandler.cs)
- [src/tests/Genocs.Http.UnitTests/ExtensionsRegistrationTests.cs](src/tests/Genocs.Http.UnitTests/ExtensionsRegistrationTests.cs)

**Acceptance criteria**

- enabling request masking does not remove existing handler-builder filters
- masking applies only to the intended Genocs client pipeline
- filter composition is deterministic and test-covered

**Dependencies**

- none

**Implementation notes**

- prefer `AddHttpMessageHandler(...)` or additive filter registration over `Replace(...)`
- keep masking behavior orthogonal to framework logging ownership
- treat this as a pipeline-composition fix, not only a logging change

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

**Delivered**

- **Runtime**: Removed global `IHttpMessageHandlerBuilderFilter` replacement from `Extensions.AddHttpClient(...)`; request masking no longer replaces framework or package-added filters.
- **Runtime**: Request masking now uses additive, named-client registration via `IHttpClientBuilder.AddHttpMessageHandler(...)`, scoped to the Genocs typed client configured by `clientName`.
- **Code cleanup**: Removed `GenocsHttpLoggingFilter` because masking is now attached directly to the target client pipeline.
- **Tests**: Extended `ExtensionsRegistrationTests` with regression coverage asserting existing global filters are preserved and masking handler injection is scoped to the intended named client only.

### HTTP-011 Make correlation header propagation request-scoped

**Status**: Done (implemented April 2026)

**Priority**: P1

**Problem**

Correlation values are added to `HttpClient.DefaultRequestHeaders` in the typed-client constructor. This creates stale or incorrect headers when the typed client outlives the originating request scope or when callers need per-request correlation.

**Scope**

- move correlation header generation into a per-request delegating handler
- define precedence when the caller has already set correlation headers explicitly
- keep the fallback behavior for missing correlation factories explicit and safe

**Likely touch points**

- [src/Genocs.Http/GenocsHttpClient.cs](src/Genocs.Http/GenocsHttpClient.cs)
- [src/Genocs.Http/Extensions.cs](src/Genocs.Http/Extensions.cs)
- [src/Genocs.Http/ICorrelationContextFactory.cs](src/Genocs.Http/ICorrelationContextFactory.cs)
- [src/Genocs.Http/ICorrelationIdFactory.cs](src/Genocs.Http/ICorrelationIdFactory.cs)
- [src/Genocs.Http/GenocsCorrelationHeadersHttpMessageHandler.cs](src/Genocs.Http/GenocsCorrelationHeadersHttpMessageHandler.cs)
- [src/tests/Genocs.Http.UnitTests/CorrelationHeadersTests.cs](src/tests/Genocs.Http.UnitTests/CorrelationHeadersTests.cs)

**Acceptance criteria**

- correlation values are generated per outbound request
- explicit caller-supplied headers are not overwritten unexpectedly
- correlation behavior is documented for singleton and scoped consumers

**Dependencies**

- HTTP-004
- HTTP-009

**Implementation notes**

- request-scoped enrichment belongs in a handler, not in `DefaultRequestHeaders`
- keep correlation creation cheap and lazy
- clarify whether empty factories mean "omit header" or "emit empty value"

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

**Delivered**

- **Runtime**: Moved correlation header propagation out of `GenocsHttpClient` constructor (`HttpClient.DefaultRequestHeaders`) into a dedicated per-request delegating handler (`GenocsCorrelationHeadersHttpMessageHandler`).
- **Runtime**: Registered correlation propagation additively on the Genocs typed client pipeline via `IHttpClientBuilder.AddHttpMessageHandler(...)` in `Extensions.AddHttpClient(...)`.
- **Header precedence**: The request-scoped handler preserves caller intent by not overwriting correlation headers when they are already present on `HttpRequestMessage`.
- **Missing values**: When configured header names are empty, or factory outputs are null/whitespace, no correlation header is emitted.
- **Tests**: Expanded `CorrelationHeadersTests` with coverage for null/whitespace omission, non-empty propagation, caller-supplied header precedence, and per-request value generation.

### HTTP-012 Harden URL masking behavior and define redaction semantics

**Status**: Done (implemented April 2026)

**Priority**: P2

**Problem**

Request masking currently performs raw string replacement across the full URI and then constructs a new `Uri`. This can over-mask unrelated segments, produce invalid URIs, or hide behavior that is difficult to reason about operationally.

**Scope**

- define exactly what parts of the request URI can be masked
- make masking resilient for query strings, path segments, and repeated values
- ensure masking cannot throw because of malformed replacement output
- document masking limitations clearly

**Likely touch points**

- [src/Genocs.Http/GenocsLoggingScopeHttpMessageHandler.cs](src/Genocs.Http/GenocsLoggingScopeHttpMessageHandler.cs)
- [src/Genocs.Http/Configurations/HttpClientOptions.cs](src/Genocs.Http/Configurations/HttpClientOptions.cs)
- [src/Genocs.Http/README_NUGET.md](src/Genocs.Http/README_NUGET.md)
- [docs/Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md)
- [src/tests/Genocs.Http.UnitTests/RequestMaskingTests.cs](src/tests/Genocs.Http.UnitTests/RequestMaskingTests.cs)

**Acceptance criteria**

- masking behavior is deterministic and cannot invalidate log URI rendering
- docs specify whether masking is path-fragment, query-value, or exact-token based
- tests cover common sensitive query and path examples

**Dependencies**

- HTTP-010

**Implementation notes**

- prefer structured URI handling over whole-string replacement
- avoid promising body redaction or arbitrary log-property redaction from this package
- keep masking simple enough to be predictable in production diagnostics

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

**Delivered**

- **Runtime**: Reworked masking in `GenocsLoggingScopeHttpMessageHandler` to use exact-token replacement over the logged URI string instead of rewriting and reparsing `Uri` instances.
- **Semantics**: Masking applies to logging output only (request URI is not mutated). Matching is `StringComparison.Ordinal`, and all occurrences are replaced across path/query/fragment text.
- **Safety**: Mask replacement output is no longer parsed as `Uri`, so non-URI-safe templates cannot throw during log rendering.
- **Tests**: Added `RequestMaskingTests` coverage for repeated token masking across path/query and non-throw behavior when `maskTemplate` is not URI-safe.
- **Docs**: Updated README and agent guidance with explicit masking boundaries and deterministic replacement semantics.

---

## M4: Tests, Documentation, Migration Guidance, and Quality Gates

### HTTP-013 Keep dedicated Genocs.Http unit tests complete and CI-wired

**Status**: In progress (reevaluated April 2026): [src/tests/Genocs.Http.UnitTests](../src/tests/Genocs.Http.UnitTests) is established and now covers HTTP-001, HTTP-002, HTTP-005, HTTP-006, HTTP-007, HTTP-008, HTTP-009, HTTP-010, HTTP-011, and HTTP-012 behavior. Remaining scope focuses on explicit solution/CI execution guarantees and ongoing coverage maintenance for future fixes.

**Priority**: P0

**Problem**

Previously no dedicated Genocs.Http test project existed; request, retry, cancellation, and logging semantics were largely unguarded. The dedicated project now exists with broad regression coverage, but it is not consistently represented in solution-driven CI execution paths.

**Scope**

- keep and expand the focused unit test project under `src/tests`
- keep separate coverage for string-based and `HttpRequestMessage` overloads
- maintain regression tests for result semantics, URI handling, retry boundaries, cancellation, correlation, and masking
- ensure solution/CI paths execute Genocs.Http unit tests explicitly (for example via solution inclusion and/or targeted test commands in workflows)

**Likely touch points**

- [src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj](src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj)
- [genocs.slnx](genocs.slnx)
- [.github/workflows/build-and-test.yml](.github/workflows/build-and-test.yml)
- [.github/workflows/sonar-analysis.yml](.github/workflows/sonar-analysis.yml)

**Acceptance criteria**

- a dedicated Genocs.Http test project exists and is explicitly executed by normal CI paths
- core contract behaviors stay regression-covered before shipping fixes
- tests distinguish transport failures from payload-materialization failures
- future HTTP backlog fixes add or update focused tests in this project as part of done criteria

**Dependencies**

- HTTP-001
- HTTP-002
- HTTP-005
- HTTP-010
- HTTP-011
- HTTP-012

**Implementation notes**

- use fake handlers to assert retry counts and sent request shape
- include coverage for cancellation and response disposal behavior
- keep request-masking and correlation-header semantics regression-covered
- treat solution/CI inclusion as part of test-project hardening, not optional follow-up

**Validation**

- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

### HTTP-014 Expand package README to reflect actual runtime behavior

**Status**: Done (implemented April 2026)

**Priority**: P1

**Problem**

The package README previously under-documented runtime behavior beyond HTTP-001/HTTP-002, especially around configuration ownership, resilience overlap guidance, and operational examples.

**Scope**

- document the supported `httpClient` configuration contract
- explain raw-response, typed-result, and `HttpResult<T>` semantics (partially addressed for result vs throw and URI/`BaseAddress`)
- add examples for `BaseAddress`, custom serializers, correlation factories, and request masking
- add operational guidance for resilience ownership

**Likely touch points**

- [src/Genocs.Http/README_NUGET.md](src/Genocs.Http/README_NUGET.md)

**Acceptance criteria**

- README describes actual runtime behavior and limitations
- examples are aligned with the implementation and intended consumer usage
- no misleading or incomplete configuration guidance remains

**Dependencies**

- HTTP-001
- HTTP-002
- HTTP-003
- HTTP-005

**Implementation notes**

- the README should become the package’s primary contract document
- prefer explicit limitations over implied support
- include a short validation section with canonical build and test commands

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`

**Delivered**

- **Consumer docs**: Expanded [README_NUGET.md](../src/Genocs.Http/README_NUGET.md) with an explicit `httpClient` configuration contract, including which fields are actively used by Genocs.Http and which remain package-shared shape (`enabled`, `type`, `services`) pending companion/runtime ownership.
- **Consumer docs**: Added dedicated examples for `BaseAddress` setup, custom serializer registration, and correlation + request-masking registration.
- **Consumer docs**: Added explicit resilience-ownership guidance to prevent accidental retry amplification when combining Genocs.Http retries with `IHttpClientBuilder` policies.
- **Consumer docs**: Added canonical validation commands section for maintainers.

### HTTP-015 Align agent documentation and add migration guidance for breaking fixes

**Status**: Done (implemented April 2026)

**Priority**: P2

**Problem**

The agent reference previously required alignment updates after the runtime hardening fixes, especially for result semantics, URI behavior, retry boundaries, ownership scope, and migration guidance.

**Scope**

- update the agent document after runtime fixes are merged
- add migration notes for breaking changes in request/result semantics
- clarify which behaviors belong to Genocs.Http versus companion packages

**Likely touch points**

- [docs/Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md)
- [src/Genocs.Http/README_NUGET.md](src/Genocs.Http/README_NUGET.md)

**Acceptance criteria**

- agent guidance matches shipped package behavior
- migration notes identify consumer-visible changes and upgrade actions
- package ownership boundaries are explicit

**Dependencies**

- HTTP-003
- HTTP-005
- HTTP-010

**Implementation notes**

- keep migration notes short and concrete
- call out changes to retry, cancellation, and relative URI handling explicitly
- avoid documenting speculative future integrations as present behavior

**Validation**

- documentation review against the merged runtime behavior

**Delivered**

- **Agent docs**: Updated [Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md) to align with shipped runtime behavior, including correction of typed string `*Async<T>` non-success semantics (exception-oriented path) and updated decision guidance.
- **Migration guidance**: Added a dedicated migration checklist in [Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md) covering consumer-visible changes for result handling, URI rules, retry boundaries, request-message replay model, cancellation/ownership, and correlation/masking semantics.
- **Ownership boundaries**: Added an explicit package ownership boundaries section in [Genocs.Http-Agent-Documentation.md](docs/Genocs.Http-Agent-Documentation.md) clarifying what Genocs.Http owns versus companion/external responsibilities.
- **Consumer docs alignment**: Added a matching migration-notes section in [README_NUGET.md](../src/Genocs.Http/README_NUGET.md) with a link back to the agent reference for source-blind upgrade guidance.

### HTTP-016 Add package-level analyzer and nullability quality gates

**Status**: Done (implemented April 2026)

**Priority**: P2

**Problem**

The repository-wide baseline leaves warnings-as-errors disabled. Without a package-level gate, future nullable and analyzer regressions in Genocs.Http can be reintroduced silently.

**Scope**

- add package-level warning enforcement after nullability cleanup
- make critical nullable warnings fail the build for Genocs.Http
- document the canonical validation commands for maintainers

**Likely touch points**

- [src/Genocs.Http/Genocs.Http.csproj](src/Genocs.Http/Genocs.Http.csproj)
- [Directory.Build.props](Directory.Build.props)
- [src/Genocs.Http/README_NUGET.md](src/Genocs.Http/README_NUGET.md)

**Acceptance criteria**

- Genocs.Http has an explicit package-level quality baseline
- nullable and API-contract regressions are caught during normal build validation
- maintainers have a documented validation path

**Dependencies**

- HTTP-004
- HTTP-013

**Implementation notes**

- enable the gate only after the current contract issues are fixed
- prefer targeted warning enforcement if a full warnings-as-errors switch is too disruptive initially
- keep the package standard aligned with the public-surface risk of outbound transport code

**Validation**

- `dotnet build src/Genocs.Http/Genocs.Http.csproj -c Debug --nologo`
- `dotnet test src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj -c Debug --nologo`

**Delivered**

- **Runtime quality gate**: Added package-scoped quality-gate configuration in [src/Genocs.Http/Genocs.Http.csproj](src/Genocs.Http/Genocs.Http.csproj):
	- `WarningsAsErrors` includes `nullable` to fail the build on nullable contract regressions.
	- `CodeAnalysisTreatWarningsAsErrors=true` to fail on .NET analyzer warnings for Genocs.Http.
	- explicit analyzer baseline (`EnableNETAnalyzers=true`, `AnalysisLevel=latest`) in package scope.
- **Maintainer docs**: Added a dedicated "Maintainer quality gates" section in [README_NUGET.md](../src/Genocs.Http/README_NUGET.md) documenting the package-scoped gate and canonical validation commands.

**Reevaluation summary (April 2026)**

- The dedicated project is already present and contains focused suites (`StringResultAsyncTests`, `RequestUriTests`, `RetryBehaviorTests`, `ExtensionsRegistrationTests`, `CorrelationHeadersTests`, `RequestMaskingTests`).
- The main remaining risk tracked under HTTP-013 is execution drift: `genocs.slnx` currently does not include `src/tests/Genocs.Http.UnitTests/Genocs.Http.UnitTests.csproj`, while some workflows run solution-based test commands.
- HTTP-013 should therefore track CI/solution wiring consistency plus ongoing regression coverage maintenance, not project creation from scratch.