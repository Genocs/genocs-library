// using Genocs.Microservice.Template.Infrastructure.Multitenancy;

namespace Genocs.Common.Persistence.Initialization;

/// <summary>
/// The IDatabaseInitializer interface is responsible for initializing the databases.
/// It can be implemented by any class that wants to initialize the databases.
/// </summary>
public interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken = default);

    // Task InitializeApplicationDbForTenantAsync(GNXTenantInfo tenant, CancellationToken cancellationToken = default);
}