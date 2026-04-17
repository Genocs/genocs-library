using Genocs.Core.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Http.UnitTests;

public sealed class ExtensionsRegistrationTests
{
    private sealed class CountingCorrelationContextFactory : ICorrelationContextFactory
    {
        public static int InstancesCreated;

        public CountingCorrelationContextFactory()
            => InstancesCreated++;

        public string? Create() => "context";
    }

    private sealed class CountingCorrelationIdFactory : ICorrelationIdFactory
    {
        public static int InstancesCreated;

        public CountingCorrelationIdFactory()
            => InstancesCreated++;

        public string? Create() => "id";
    }

    private static IGenocsBuilder CreateBuilder(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder().Build();
        return GenocsBuilder.Create(services, configuration);
    }

    [Fact]
    public void AddHttpClient_does_not_instantiate_custom_correlation_factories_during_registration()
    {
        CountingCorrelationContextFactory.InstancesCreated = 0;
        CountingCorrelationIdFactory.InstancesCreated = 0;

        var services = new ServiceCollection();
        services.AddSingleton<ICorrelationContextFactory, CountingCorrelationContextFactory>();
        services.AddSingleton<ICorrelationIdFactory, CountingCorrelationIdFactory>();

        var builder = CreateBuilder(services);

        builder.AddHttpClient();

        Assert.Equal(0, CountingCorrelationContextFactory.InstancesCreated);
        Assert.Equal(0, CountingCorrelationIdFactory.InstancesCreated);

        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(ICorrelationContextFactory)
                && descriptor.ImplementationType?.FullName == "Genocs.Http.EmptyCorrelationContextFactory");
        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(ICorrelationIdFactory)
                && descriptor.ImplementationType?.FullName == "Genocs.Http.EmptyCorrelationIdFactory");
    }

    [Fact]
    public void AddHttpClient_registers_empty_correlation_factories_when_custom_ones_are_missing()
    {
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        builder.AddHttpClient();

        using var provider = services.BuildServiceProvider();

        Assert.Equal("Genocs.Http.EmptyCorrelationContextFactory", provider.GetRequiredService<ICorrelationContextFactory>().GetType().FullName);
        Assert.Equal("Genocs.Http.EmptyCorrelationIdFactory", provider.GetRequiredService<ICorrelationIdFactory>().GetType().FullName);
    }
}
