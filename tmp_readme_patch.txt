*** Begin Patch
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Auth\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Auth\README_NUGET.md
+# Genocs.Auth
+
+Authentication and authorization helpers for ASP.NET Core applications. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Auth`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddJwt`
+- `AddOpenIdJwt`
+- `AddPrivateKeyJwt`
+- `UseAccessTokenValidator`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Common\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Common\README_NUGET.md
+# Genocs.Common
+
+Shared primitives and utilities used across Genocs packages. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Common`
+
+## Getting Started
+
+Reference this package from your project and integrate its exposed abstractions and types in your application flow.
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Core\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Core\README_NUGET.md
+# Genocs.Core
+
+Core building blocks and abstractions for Genocs-based applications. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Core`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddCommandHandlers`
+- `AddDispatchers`
+- `AddEventHandlers`
+- `AddGenocs`
+- `AddHandlers`
+- `AddInMemoryCommandDispatcher`
+- `AddInMemoryEventDispatcher`
+- `AddInMemoryQueryDispatcher`
+- `AddQueryHandlers`
+- `MapDefaultEndpoints`
+- `UseGenocs`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Discovery.Consul\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Discovery.Consul\README_NUGET.md
+# Genocs.Discovery.Consul
+
+Consul-based service discovery integration for Genocs applications. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Discovery.Consul`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddConsul`
+- `AddConsulHttpClient`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Http.RestEase\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Http.RestEase\README_NUGET.md
+# Genocs.Http.RestEase
+
+RestEase integration for typed Http clients in Genocs applications. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Http.RestEase`
+
+## Getting Started
+
+Reference this package from your project and integrate its exposed abstractions and types in your application flow.
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Http\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Http\README_NUGET.md
+# Genocs.Http
+
+Http client abstractions and helpers for Genocs applications. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Http`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddHttpClient`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.LoadBalancing.Fabio\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.LoadBalancing.Fabio\README_NUGET.md
+# Genocs.LoadBalancing.Fabio
+
+Fabio load-balancing integration for service-to-service calls. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.LoadBalancing.Fabio`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddFabio`
+- `AddFabioHttpClient`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Logging\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Logging\README_NUGET.md
+# Genocs.Logging
+
+Logging abstractions and extensions for Genocs applications. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Logging`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddCommandHandlersLogging`
+- `AddCorrelationContextLogging`
+- `AddEventHandlersLogging`
+- `MapLogLevelHandler`
+- `UseLogging`
+- `UserCorrelationContextLogging`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Messaging.Outbox.MongoDB\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Messaging.Outbox.MongoDB\README_NUGET.md
+# Genocs.Messaging.Outbox.MongoDB
+
+MongoDB-backed implementation of the messaging outbox pattern. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Messaging.Outbox.MongoDB`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddMongo`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Messaging.Outbox\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Messaging.Outbox\README_NUGET.md
+# Genocs.Messaging.Outbox
+
+Outbox pattern abstractions for reliable message delivery. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Messaging.Outbox`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddInMemory`
+- `AddMessageOutbox`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Messaging.RabbitMQ\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Messaging.RabbitMQ\README_NUGET.md
+# Genocs.Messaging.RabbitMQ
+
+RabbitMQ integration for Genocs messaging abstractions. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Messaging.RabbitMQ`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddRabbitMQAsync`
+- `UseRabbitMQ`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Messaging\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Messaging\README_NUGET.md
+# Genocs.Messaging
+
+Abstractions for message broker publishing and consuming. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Messaging`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddServiceBusCommandDispatcher`
+- `AddServiceBusEventDispatcher`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Metrics\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Metrics\README_NUGET.md
+# Genocs.Metrics
+
+Metrics abstractions and instrumentation contracts for Genocs applications. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Metrics`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddPrometheus`
+- `UsePrometheus`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Persistence.EFCore\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Persistence.EFCore\README_NUGET.md
+# Genocs.Persistence.EFCore
+
+EF Core repository and persistence integration for Genocs applications. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Persistence.EFCore`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddEFCorePersistence`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Persistence.MongoDB\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Persistence.MongoDB\README_NUGET.md
+# Genocs.Persistence.MongoDB
+
+MongoDB repository and persistence integration for Genocs applications. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Persistence.MongoDB`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddMongo`
+- `AddMongoWithRegistration`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Persistence.Redis\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Persistence.Redis\README_NUGET.md
+# Genocs.Persistence.Redis
+
+Redis-backed persistence helpers and repository integration. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Persistence.Redis`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddRedis`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.QueryBuilder\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.QueryBuilder\README_NUGET.md
+# Genocs.QueryBuilder
+
+Fluent query builder abstractions independent from persistence providers. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.QueryBuilder`
+
+## Getting Started
+
+Reference this package from your project and integrate its exposed abstractions and types in your application flow.
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Saga.Integrations.MongoDB\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Saga.Integrations.MongoDB\README_NUGET.md
+# Genocs.Saga.Integrations.MongoDB
+
+MongoDB storage integration for Genocs saga state management. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Saga.Integrations.MongoDB`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `UseMongoPersistence`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Saga.Integrations.Redis\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Saga.Integrations.Redis\README_NUGET.md
+# Genocs.Saga.Integrations.Redis
+
+Redis storage integration for Genocs saga state management. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Saga.Integrations.Redis`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `UseRedisPersistence`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Saga\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Saga\README_NUGET.md
+# Genocs.Saga
+
+Saga pattern abstractions for distributed workflow orchestration. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Saga`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddSaga`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Secrets.AzureKeyVault\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Secrets.AzureKeyVault\README_NUGET.md
+# Genocs.Secrets.AzureKeyVault
+
+Azure Key Vault integration for retrieving and managing application secrets. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Secrets.AzureKeyVault`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `UseAzureKeyVault`
+- `UseAzureKeyVaultWithCertificate`
+- `UseAzureKeyVaultWithCertificates`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Secrets.HashicorpKeyVault\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Secrets.HashicorpKeyVault\README_NUGET.md
+# Genocs.Secrets.HashicorpKeyVault
+
+HashiCorp Vault integration for retrieving and managing application secrets. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Secrets.HashicorpKeyVault`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `UseVault`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Security\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Security\README_NUGET.md
+# Genocs.Security
+
+Security helpers for hashing, encryption, and related operations. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Security`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddSecurity`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Messaging.AzureServiceBus\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Messaging.AzureServiceBus\README_NUGET.md
+# Genocs.Messaging.AzureServiceBus
+
+Azure Service Bus integration for messaging and transport workflows. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Messaging.AzureServiceBus`
+
+## Getting Started
+
+Reference this package from your project and integrate its exposed abstractions and types in your application flow.
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Telemetry\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Telemetry\README_NUGET.md
+# Genocs.Telemetry
+
+OpenTelemetry integration helpers for traces, metrics, and logs. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Telemetry`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddTelemetry`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.Tracing.Jaeger.RabbitMQ\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.Tracing.Jaeger.RabbitMQ\README_NUGET.md
+# Genocs.Tracing.Jaeger.RabbitMQ
+
+Jaeger tracing integration for RabbitMQ-based messaging pipelines. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.Tracing.Jaeger.RabbitMQ`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddJaegerRabbitMqPlugin`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.WebApi.CQRS\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.WebApi.CQRS\README_NUGET.md
+# Genocs.WebApi.CQRS
+
+CQRS extensions for Genocs ASP.NET Core Web APIs. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.WebApi.CQRS`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddInMemoryDispatcher`
+- `UseDispatcherEndpoints`
+- `UsePublicContracts`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.WebApi.Security\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.WebApi.Security\README_NUGET.md
+# Genocs.WebApi.Security
+
+Security extensions for authentication and authorization in Genocs Web APIs. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.WebApi.Security`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddCertificateAuthentication`
+- `UseCertificateAuthentication`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.WebApi.OpenApi\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.WebApi.OpenApi\README_NUGET.md
+# Genocs.WebApi.OpenApi
+
+Swagger/OpenAPI integration for documenting Genocs Web APIs. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.WebApi.OpenApi`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddSwaggerDocs`
+- `AddWebApiSwaggerDocs`
+- `UseSwaggerDocs`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** Delete File: D:/dev/genocs/genocs-library\src\Genocs.WebApi\README_NUGET.md
*** Add File: D:/dev/genocs/genocs-library\src\Genocs.WebApi\README_NUGET.md
+# Genocs.WebApi
+
+ASP.NET Core Web API extensions and conventions for Genocs applications. Target frameworks: `net10.0;net9.0;net8.0`.
+
+## Installation
+
+- `dotnet add package Genocs.WebApi`
+
+## Getting Started
+
+Register this package in your composition root using the extension methods listed below, then bind required options from configuration.
+
+## Main Entry Points
+
+- `AddWebApi`
+- `UseAllForwardedHeaders`
+- `UseEndpoints`
+- `UseErrorHandler`
+
+## Support
+
+- Repository: https://github.com/genocs/genocs-library
+- Documentation: https://github.com/genocs/genocs-library/tree/main/docs
+
+## Release Notes
+
+- CHANGELOG: https://github.com/genocs/genocs-library/blob/main/CHANGELOG.md
+- Releases: https://github.com/genocs/genocs-library/releases
*** End Patch
