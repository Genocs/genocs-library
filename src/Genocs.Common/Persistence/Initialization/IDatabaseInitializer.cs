﻿// using Genocs.Microservice.Template.Infrastructure.Multitenancy;

namespace Genocs.Common.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);

    // Task InitializeApplicationDbForTenantAsync(GNXTenantInfo tenant, CancellationToken cancellationToken);
}