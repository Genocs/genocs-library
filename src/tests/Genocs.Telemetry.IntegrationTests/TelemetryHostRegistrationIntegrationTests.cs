using Genocs.Core.Builders;
using Genocs.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Xunit;

namespace Genocs.Telemetry.IntegrationTests;

[CollectionDefinition(nameof(TelemetryIntegrationCollection), DisableParallelization = true)]
public sealed class TelemetryIntegrationCollection;

[Collection(nameof(TelemetryIntegrationCollection))]
public sealed class TelemetryHostRegistrationIntegrationTests
{
    [Fact]
    public void AddTelemetry_WhenAppServiceIsMissing_DoesNotRegisterOpenTelemetryProviders()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions());
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["telemetry:enabled"] = "true",
            ["telemetry:exporter:enabled"] = "false"
        });

        IGenocsBuilder genocsBuilder = builder.AddGenocs();

        genocsBuilder.AddTelemetry();

        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType == typeof(TracerProvider));
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddTelemetry_WhenTelemetryIsDisabled_DoesNotRegisterOpenTelemetryProviders()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions());
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["app:service"] = "telemetry-integration-tests",
            ["telemetry:enabled"] = "false",
            ["telemetry:exporter:enabled"] = "false"
        });

        IGenocsBuilder genocsBuilder = builder.AddGenocs();

        genocsBuilder.AddTelemetry();

        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType == typeof(TracerProvider));
        Assert.DoesNotContain(builder.Services, descriptor => descriptor.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddTelemetry_WhenUsingServiceCollectionHostMode_RegistersTracingAndMetrics()
    {
        IServiceCollection services = new ServiceCollection();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["app:service"] = "telemetry-integration-tests",
                ["telemetry:enabled"] = "true",
                ["telemetry:exporter:enabled"] = "false",
                ["telemetry:console:enabled"] = "false",
                ["telemetry:azure:enabled"] = "false"
            })
            .Build();

        IGenocsBuilder genocsBuilder = services.AddGenocs(configuration);

        Assert.Null(genocsBuilder.WebApplicationBuilder);

        genocsBuilder.AddTelemetry();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<TracerProvider>());
        Assert.NotNull(serviceProvider.GetService<MeterProvider>());
    }
}
