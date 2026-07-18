using Genocs.Persistence.EFCore.Configurations;
using Genocs.Persistence.EFCore.MultiTenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Genocs.Persistence.EFCore.UnitTests.Multitenancy;

/// <summary>
/// Registration contract tests for EFCORE-004:
/// keep configuration-store and EFCore-store paths separate,
/// and ensure ITenantService is registered on the EFCore-store path.
/// </summary>
public class AddFinbuckleMultiTenancyRegistrationTests
{
    [Fact]
    public void AddFinbuckleMultiTenancy_WithConfigurationStore_DoesNotRegisterTenantService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection([]).Build();

        services.AddFinbuckleMultiTenancy<GNXTenantInfo>(configuration);

        services.Any(d => d.ServiceType == typeof(ITenantService)).ShouldBeFalse();
    }

    [Fact]
    public void AddFinbuckleMultiTenancyWithEfCoreStore_RegistersTenantServiceAndTenantDbContext()
    {
        var services = new ServiceCollection();

        services.AddOptions<DatabaseOptions>()
            .Configure(options =>
            {
                options.DBProvider = "sqlite";
                options.ConnectionString = "Data Source=:memory:";
            });

        services.AddFinbuckleMultiTenancyWithEfCoreStore();

        services.Any(d => d.ServiceType == typeof(ITenantService)).ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(ITenantDatabaseInitializer)).ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(TenantDbContext)).ShouldBeTrue();
    }
}
