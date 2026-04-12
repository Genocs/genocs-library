using Genocs.Core.Builders;
using Genocs.Telemetry;
using Genocs.Telemetry.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Xunit;

namespace Genocs.Telemetry.UnitTests;

public class OpenTelemetryExtensionsSqlClientTests
{
    [Fact]
    public void AddTelemetry_WhenBuilderHasNoWebApplicationBuilder_RegistersTracingAndMetricsWithoutThrowing()
    {
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateTelemetryConfiguration();

        IGenocsBuilder builder = services.AddGenocs(configuration);

        Assert.Null(builder.WebApplicationBuilder);

        IGenocsBuilder updatedBuilder = builder.AddTelemetry();

        Assert.Same(builder, updatedBuilder);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TracerProvider));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddTelemetry_WhenUsingServiceCollectionBuilder_DoesNotRegisterOpenTelemetryLogProvider()
    {
        IServiceCollection services = new ServiceCollection();
        IConfiguration configuration = CreateTelemetryConfiguration();

        IGenocsBuilder builder = services.AddGenocs(configuration);

        builder.AddTelemetry();

        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ImplementationType?.FullName?.Contains("OpenTelemetryLoggerProvider", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void NormalizeOtlpBatchSettings_WhenValuesAreWithinBounds_UsesConfiguredValues()
    {
        var options = new OtlpExportOptions
        {
            MaxQueueSize = 4096,
            ScheduledDelayMilliseconds = 2000,
            ExporterTimeoutMilliseconds = 45000,
            MaxExportBatchSize = 256
        };

        (int maxQueueSize, int scheduledDelayMilliseconds, int exporterTimeoutMilliseconds, int maxExportBatchSize) =
            OpenTelemetryExtensions.NormalizeOtlpBatchSettings(options);

        Assert.Equal(4096, maxQueueSize);
        Assert.Equal(2000, scheduledDelayMilliseconds);
        Assert.Equal(45000, exporterTimeoutMilliseconds);
        Assert.Equal(256, maxExportBatchSize);
    }

    [Fact]
    public void NormalizeOtlpBatchSettings_WhenValuesAreOutOfRange_UsesFallbackDefaults()
    {
        var options = new OtlpExportOptions
        {
            MaxQueueSize = -1,
            ScheduledDelayMilliseconds = 0,
            ExporterTimeoutMilliseconds = 500000,
            MaxExportBatchSize = 0
        };

        (int maxQueueSize, int scheduledDelayMilliseconds, int exporterTimeoutMilliseconds, int maxExportBatchSize) =
            OpenTelemetryExtensions.NormalizeOtlpBatchSettings(options);

        Assert.Equal(2048, maxQueueSize);
        Assert.Equal(5000, scheduledDelayMilliseconds);
        Assert.Equal(30000, exporterTimeoutMilliseconds);
        Assert.Equal(512, maxExportBatchSize);
    }

    [Fact]
    public void NormalizeOtlpBatchSettings_WhenBatchSizeExceedsQueueSize_UsesSafeFallback()
    {
        var options = new OtlpExportOptions
        {
            MaxQueueSize = 600,
            MaxExportBatchSize = 900
        };

        (int maxQueueSize, _, _, int maxExportBatchSize) = OpenTelemetryExtensions.NormalizeOtlpBatchSettings(options);

        Assert.Equal(600, maxQueueSize);
        Assert.Equal(512, maxExportBatchSize);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("localhost:4317")]
    [InlineData("ftp://localhost:4317")]
    [InlineData("not-a-uri")]
    public void TryParseOtlpEndpoint_WhenEndpointIsInvalid_ReturnsFalse(string? endpoint)
    {
        bool isValid = OpenTelemetryExtensions.TryParseOtlpEndpoint(endpoint, out Uri? endpointUri);

        Assert.False(isValid);
        Assert.Null(endpointUri);
    }

    [Theory]
    [InlineData("http://localhost:4317")]
    [InlineData("https://otel.example.com:4318/v1/traces")]
    public void TryParseOtlpEndpoint_WhenEndpointIsValid_ReturnsTrue(string endpoint)
    {
        bool isValid = OpenTelemetryExtensions.TryParseOtlpEndpoint(endpoint, out Uri? endpointUri);

        Assert.True(isValid);
        Assert.NotNull(endpointUri);
        Assert.Equal(endpoint, endpointUri!.AbsoluteUri.TrimEnd('/'));
    }

    [Fact]
    public void IsSqlClientTracingEnabled_WhenSqlClientSectionMissing_ReturnsTrue()
    {
        var options = new TelemetryOptions
        {
            SqlClient = null
        };

        bool enabled = OpenTelemetryExtensions.IsSqlClientTracingEnabled(options);

        Assert.True(enabled);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IsSqlClientTracingEnabled_UsesSqlClientEnabledFlag(bool configuredValue, bool expected)
    {
        var options = new TelemetryOptions
        {
            SqlClient = new SqlClientOptions
            {
                Enabled = configuredValue
            }
        };

        bool enabled = OpenTelemetryExtensions.IsSqlClientTracingEnabled(options);

        Assert.Equal(expected, enabled);
    }

    [Fact]
    public void ShouldScrubSqlStatementText_WhenSqlTracingDisabled_ReturnsFalse()
    {
        var options = new TelemetryOptions
        {
            SqlClient = new SqlClientOptions
            {
                Enabled = false,
                EnableStatementText = false
            }
        };

        bool scrubEnabled = OpenTelemetryExtensions.ShouldScrubSqlStatementText(options);

        Assert.False(scrubEnabled);
    }

    [Fact]
    public void ShouldScrubSqlStatementText_WhenSqlTracingEnabledAndStatementTextDisabled_ReturnsTrue()
    {
        var options = new TelemetryOptions
        {
            SqlClient = new SqlClientOptions
            {
                Enabled = true,
                EnableStatementText = false
            }
        };

        bool scrubEnabled = OpenTelemetryExtensions.ShouldScrubSqlStatementText(options);

        Assert.True(scrubEnabled);
    }

    [Fact]
    public void ShouldScrubSqlStatementText_WhenStatementTextEnabled_ReturnsFalse()
    {
        var options = new TelemetryOptions
        {
            SqlClient = new SqlClientOptions
            {
                Enabled = true,
                EnableStatementText = true
            }
        };

        bool scrubEnabled = OpenTelemetryExtensions.ShouldScrubSqlStatementText(options);

        Assert.False(scrubEnabled);
    }

    private static IConfiguration CreateTelemetryConfiguration()
    {
        Dictionary<string, string?> settings = new()
        {
            ["app:service"] = "telemetry-test-service",
            ["telemetry:enabled"] = "true",
            ["telemetry:exporter:enabled"] = "false",
            ["telemetry:console:enabled"] = "false",
            ["telemetry:azure:enabled"] = "false"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
}
