---
name: csharp-backend-developer
description: Build and evolve C# backend microservices with Genocs Library. Use for ASP.NET Core API implementation, CQRS endpoints, MongoDB repositories, messaging integration, authentication, logging, telemetry, and production hardening.
argument-hint: Feature or problem statement, constraints, and target service/project path.
---

# C# Backend Developer

## What This Skill Produces
A Genocs-first delivery workflow that produces:
- A concrete package composition plan for the requested feature
- A step-by-step implementation path across API, application, domain, and infrastructure
- Decision branches for WebApi, CQRS, Auth, OpenApi, Messaging, RabbitMQ, Outbox, Saga, MongoDB, Logging, and Telemetry
- Completion checks that verify runtime safety, compatibility, and observability

## When To Use
Use this skill when prompts include terms like:
- C# backend
- ASP.NET Core API
- Genocs
- CQRS command/query
- WebApi endpoints
- repository pattern
- MongoDB repository
- RabbitMQ messaging
- FluentValidation
- microservice feature
- bug fix in service layer
- refactor backend code

## Inputs To Capture
1. Business goal and user-visible behavior
2. Non-functional constraints: performance, security, reliability, compliance
3. Data shape and source of truth: SQL Server, MongoDB, cache, message broker
4. Contract constraints: request/response schema, backward compatibility, versioning
5. Delivery constraints: timeline, risk tolerance, test depth
6. Genocs package boundary: which package should own each concern

## Genocs Package Selection
1. Web API surface
- Use Genocs.WebApi for endpoint DSL, request binding, response helpers, and error handling middleware.
- Use Genocs.WebApi.CQRS when endpoints should dispatch commands and queries through CQRS dispatchers.

2. API documentation
- Use Genocs.WebApi.OpenApi for Swagger or ReDoc exposure and OpenAPI metadata.

3. Security
- Use Genocs.Auth for JWT paths: AddJwt, AddOpenIdJwt, or AddPrivateKeyJwt depending on token issuer model.
- Use Genocs.WebApi.Security when client certificate authentication is required.

4. Data
- Use Genocs.Persistence.MongoDB for Mongo registration and repositories.
- Choose AddMongoWithRegistration for ObjectId plus IMongoEntity.
- Choose AddMongo plus AddMongoRepository for custom key entities such as Guid.

5. Messaging
- Use Genocs.Messaging as abstraction only.
- Add a transport package such as Genocs.Messaging.RabbitMQ for runtime publish and subscribe behavior.
- Add outbox package only when delivery guarantees require durable message handoff.

6. RabbitMQ transport
- Use Genocs.Messaging.RabbitMQ when RabbitMQ is the concrete broker.
- Register transport with AddRabbitMQAsync and activate subscriber processing with UseRabbitMQ.
- Keep retries, dead-letter routing, QoS, and message persistence in rabbitmq configuration rather than handler code.

7. Outbox and idempotent consume
- Use Genocs.Messaging.Outbox to buffer outgoing messages and deduplicate inbound handling by message id.
- Choose a durable provider such as Genocs.Messaging.Outbox.MongoDB for production-grade persistence.
- Treat outbox as at-least-once delivery support, not exactly-once guarantees.

8. Saga orchestration
- Use Genocs.Saga for long-running business workflows that require stateful orchestration and compensation.
- Implement ISagaStartAction for messages that are allowed to create saga state.
- Use explicit saga correlation via SagaContext with stable saga id and originator.
- Use MongoDB or Redis saga integration package when orchestration state must survive restarts.

9. Observability
- Use Genocs.Logging for Serilog host logging, correlation middleware, and runtime log-level endpoint.
- Use Genocs.Telemetry for OpenTelemetry traces and metrics, exporters, and optional Prometheus endpoint.

## Workflow
1. Define outcome and boundaries
- Convert the request into explicit acceptance criteria.
- Identify in-scope and out-of-scope behavior.
- Record assumptions that may affect implementation choices.

2. Choose architectural landing zone
- Endpoint/API concerns: routing, request mapping, authn/authz, response codes.
- Application concerns: command/query handlers, orchestration, transaction boundary.
- Domain concerns: invariants, entities/value objects, domain events.
- Infrastructure concerns: repository/connector implementation, external integrations.

Decision branch:
- If behavior changes business rules, start at domain/application layers first.
- If behavior is transport-only, confine changes to endpoint and mapping layers.
- If behavior depends on external systems, design retry/timeouts/fallbacks before coding.

3. Compose Genocs host pipeline first
- Start from AddGenocs, then add only required Genocs modules.
- For HTTP endpoints, add Genocs.WebApi.
- For CQRS endpoint bridging, add Genocs.WebApi.CQRS and ensure dispatcher registration exists.
- Call genocs.Build before app build completion where required by selected modules.
- Use app.UseGenocs when startup initializers such as Mongo seeding are expected.

Decision branch:
- If only REST endpoint helpers are needed, keep Genocs.WebApi without CQRS adapter.
- If command and query dispatch over HTTP is needed, use Genocs.WebApi.CQRS endpoint mapping.
- If runtime contracts endpoint is needed, add public contracts middleware from CQRS package.

4. Design contracts before implementation
- Define request/response DTOs and error contract.
- Preserve backward compatibility unless explicitly approved.
- Validate nullability and optional fields for safe evolution.

5. Implement with explicit quality guards
- Use dependency injection with clear abstractions.
- Add FluentValidation rules for all externally supplied inputs.
- Enforce idempotency for commands where duplicate delivery is possible.
- Add structured logging at decision points and failure boundaries.

Decision branch:
- If persistence is SQL-centric, optimize query shape and transaction scope.
- If persistence is Mongo-centric, use Genocs.Persistence.MongoDB registration path that matches entity key strategy.
- If cross-service communication is required, use Genocs.Messaging abstractions with a confirmed transport provider.
- If RabbitMQ is selected, enforce rabbitmq section completeness before coding handlers.
- If durable publish is required, use outbox-backed send flow instead of direct bus publish in transactional paths.
- If business workflow spans multiple asynchronous steps with compensation, model it as a saga instead of ad-hoc orchestration.

6. Harden operational behavior
- Add cancellation token propagation for I/O paths.
- Define retry policy only for transient failures; avoid retry storms.
- Ensure failures return actionable error details without leaking sensitive internals.
- Add Genocs.Telemetry configuration for traces and metrics.
- Add Genocs.Logging configuration for structured logs and operator-level diagnostics.

Decision branch:
- If Prometheus scraping is required, enable telemetry.prometheus and map endpoint with UsePrometheus and MapPrometheus.
- If runtime log tuning is required, expose logging level endpoint via MapLogLevelHandler.
- If security uses JWT, ensure authentication and authorization middleware order is explicit and validated.
- If RabbitMQ retry and dead-letter behavior is required, set retries, retryInterval, deadLetter, and requeueFailedMessages in config and verify expected settlement outcomes.
- If saga compensation is enabled, define retry compensation procedure and verify state transitions for Rejected, Compensating, and Compensated paths.

7. Validate with tests
- Unit tests: business rules, validators, mapper logic.
- Integration tests: repository behavior, API contract, infrastructure wiring.
- Regression tests: reproduce and lock fixed bugs.

Decision branch:
- If change is high risk, increase integration coverage and add negative-path tests.
- If change is low risk/internal refactor, keep test scope focused but verify public behavior is unchanged.

8. Final completion checks
- Build passes with analyzers/linting clean for touched code.
- Tests pass for touched behavior and critical regressions.
- API and message contracts are documented if changed.
- Observability and operational runbook notes are updated for new failure modes.
- Package boundaries are respected and no undocumented Genocs APIs are introduced.
- RabbitMQ, Outbox, and Saga configuration values are explicitly validated for the target environment.

## Definition Of Done
A task is complete only when:
1. Acceptance criteria are demonstrated by tests or reproducible verification steps.
2. Error handling paths are implemented and validated.
3. Logging and telemetry allow operators to diagnose failures.
4. Breaking changes are either avoided or explicitly called out with migration notes.
5. Chosen Genocs modules match actual runtime behavior and companion package requirements.
6. Messaging flow documents whether direct publish, outbox publish, or saga orchestration is the source of truth.

## Genocs References
- [WebApi](../../../docs/Genocs.WebApi-Agent-Documentation.md)
- [WebApi CQRS](../../../docs/Genocs.WebApi.CQRS-Agent-Documentation.md)
- [WebApi OpenApi](../../../docs/Genocs.WebApi.OpenApi-Agent-Documentation.md)
- [WebApi Auth](../../../docs/Genocs.WebApi.Auth-Agent-Documentation.md)
- [Persistence MongoDB](../../../docs/Genocs.Persistence.MongoDB-Agent-Documentation.md)
- [Messaging](../../../docs/Genocs.Messaging-Agent-Documentation.md)
- [Messaging RabbitMQ](../../../docs/Genocs.Messaging.RabbitMQ-Agent-Documentation.md)
- [Messaging Outbox](../../../docs/Genocs.Messaging.Outbox-Agent-Documentation.md)
- [Saga](../../../docs/Genocs.Saga-Agent-Documentation.md)
- [Logging](../../../docs/Genocs.Logging-Agent-Documentation.md)
- [Telemetry](../../../docs/Genocs.Telemetry-Agent-Documentation.md)

## Implementation Templates
Use these templates in strict order when building a messaging and orchestration baseline.

### Step 1. RabbitMQ Transport Baseline
Program.cs starter:

	using Genocs.Core.Builders;
	using Genocs.Messaging.RabbitMQ;

	var builder = WebApplication.CreateBuilder(args);
	IGenocsBuilder genocs = builder.AddGenocs();

	await genocs.AddRabbitMQAsync();
	genocs.Build();

	var app = builder.Build();
	app.UseRabbitMQ();
	app.Run();

Minimum rabbitmq config checklist:
- hostNames is set and non-empty
- retries, retryInterval, and deadLetter policy are explicit
- messagesPersisted and qos.prefetchCount are tuned for workload
- logger.logMessagePayload is disabled by default in production

### Step 2. Outbox With Mongo Provider
Program.cs starter:

	using Genocs.Core.Builders;
	using Genocs.Messaging.Outbox;
	using Genocs.Messaging.Outbox.MongoDB;
	using Genocs.Persistence.MongoDB.Extensions;

	var builder = WebApplication.CreateBuilder(args);

	IGenocsBuilder genocs = builder
		.AddGenocs()
		.AddMongo()
		.AddMessageOutbox(outbox => outbox.AddMongo());

	genocs.Build();

	var app = builder.Build();
	app.Run();

Outbox config checklist:
- outbox.enabled is true when background publishing is required
- intervalMilliseconds is greater than zero
- type is set to sequential unless delayed batch-marking is explicitly required
- message flow documents when to use direct publish versus outbox send

### Step 3. Saga Persistence With Mongo Or Redis
Mongo path starter:

	using Genocs.Core.Builders;
	using Genocs.Persistence.MongoDB.Extensions;
	using Genocs.Saga;
	using Genocs.Saga.Integrations.MongoDB;

	var builder = WebApplication.CreateBuilder(args);

	IGenocsBuilder genocs = builder
		.AddGenocs()
		.AddMongo();

	builder.Services.AddSaga(saga =>
	{
		saga.UseMongoPersistence();
	});

	genocs.Build();

	var app = builder.Build();
	app.Run();

Redis path starter:

	using Genocs.Saga;
	using Genocs.Saga.Integrations.Redis;
	using Genocs.Saga.Integrations.Redis.Configurations;

	var builder = WebApplication.CreateBuilder(args);

	var sagaRedisOptions = builder.Configuration
		.GetSection(SagaRedisOptions.Position)
		.Get<SagaRedisOptions>()
		?? throw new InvalidOperationException("Missing sagaRedis configuration.");

	builder.Services.AddSaga(saga =>
	{
		saga.UseRedisPersistence(sagaRedisOptions);
	});

	var app = builder.Build();
	app.Run();

Saga checklist:
- implement ISagaStartAction for messages that can create saga state
- always pass stable saga id and originator in SagaContext
- define compensation and retry strategy for failed compensation paths
- validate that persisted saga contracts remain backward-compatible

## Prompt Starters
- Implement a new C# backend feature for order cancellation with idempotency and audit logging.
- Implement a Genocs WebApi plus CQRS endpoint flow for order placement with command and query handlers.
- Add Genocs.Auth JWT validation and secure selected endpoints with policy-based authorization.
- Add Genocs.Persistence.MongoDB repository wiring for Guid-key entities and integration tests.
- Add Genocs.Telemetry and Genocs.Logging operational baseline for this service.
- Configure Genocs.Messaging.RabbitMQ with dead-letter and retry policy for order events.
- Implement outbox-backed event publishing with Genocs.Messaging.Outbox.MongoDB.
- Build a Genocs.Saga orchestration with compensation for a multi-step checkout workflow.
