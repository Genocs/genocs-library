using Genocs.Core.Builders;
using Genocs.Telemetry;
using Genocs.Telemetry.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Xunit;

namespace Genocs.Telemetry.UnitTests;

public class OpenTelemetryExtensionsSqlClientTests
{
    [Fact]
    public void HasEnabledTracingExportPath_WhenAllTracingExportersDisabled_ReturnsFalse()
    {
        var options = new TelemetryOptions
        {
            Exporter = new OtlpExportOptions
            {
                Enabled = false,
                EnableTracing = false
            },
            Console = new ConsoleOptions
            {
                Enabled = false,
                EnableTracing = false
            },
            Azure = new AzureOptions
            {
                Enabled = false,
                EnableTracing = false,
                ConnectionString = null
            }
        };

        bool hasExportPath = OpenTelemetryExtensions.HasEnabledTracingExportPath(options);

        Assert.False(hasExportPath);
    }

    [Fact]
    public void HasEnabledTracingExportPath_WhenConsoleTracingEnabled_ReturnsTrue()
    {
        var options = new TelemetryOptions
        {
            Console = new ConsoleOptions
            {
                Enabled = true,
                EnableTracing = true
            }
        };

        bool hasExportPath = OpenTelemetryExtensions.HasEnabledTracingExportPath(options);

        Assert.True(hasExportPath);
    }

    [Fact]
    public void HasEnabledTracingExportPath_WhenAzureTracingEnabledWithConnectionString_ReturnsTrue()
    {
        var options = new TelemetryOptions
        {
            Azure = new AzureOptions
            {
                Enabled = true,
                EnableTracing = true,
                ConnectionString = "InstrumentationKey=test;IngestionEndpoint=https://example"
            }
        };

        bool hasExportPath = OpenTelemetryExtensions.HasEnabledTracingExportPath(options);

        Assert.True(hasExportPath);
    }

    [Fact]
    public void HasEnabledTracingExportPath_WhenOtlpTracingEnabledWithValidEndpoint_ReturnsTrue()
    {
        var options = new TelemetryOptions
        {
            Exporter = new OtlpExportOptions
            {
                Enabled = true,
                EnableTracing = true,
                OtlpEndpoint = "http://localhost:4317"
            }
        };

        bool hasExportPath = OpenTelemetryExtensions.HasEnabledTracingExportPath(options);

        Assert.True(hasExportPath);
    }

    [Fact]
    public void HasEnabledTracingExportPath_WhenOtlpTracingEnabledWithInvalidEndpoint_ReturnsFalse()
    {
        var options = new TelemetryOptions
        {
            Exporter = new OtlpExportOptions
            {
                Enabled = true,
                EnableTracing = true,
                OtlpEndpoint = "not-a-valid-endpoint"
            }
        };

        bool hasExportPath = OpenTelemetryExtensions.HasEnabledTracingExportPath(options);

        Assert.False(hasExportPath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeExceptionTagValue_WhenValueIsEmpty_ReturnsNull(string? value)
    {
        string? normalized = OpenTelemetryExtensions.NormalizeExceptionTagValue(value, 32);

        Assert.Null(normalized);
    }

    [Fact]
    public void NormalizeExceptionTagValue_WhenValueContainsControlCharacters_SanitizesValue()
    {
        const string input = "failure\u0001detected\nfor\torder";

        string? normalized = OpenTelemetryExtensions.NormalizeExceptionTagValue(input, 128);

        Assert.Equal("failure detected for order", normalized);
    }

    [Fact]
    public void NormalizeExceptionTagValue_WhenValueExceedsMaxLength_TruncatesWithSuffix()
    {
        string input = new('x', 40);

        string? normalized = OpenTelemetryExtensions.NormalizeExceptionTagValue(input, 20);

        Assert.NotNull(normalized);
        Assert.Equal(20, normalized!.Length);
        Assert.EndsWith("...(truncated)", normalized);
    }

    [Fact]
    public void NormalizeExceptionTagValue_WhenMaxLengthIsTooSmall_UsesSuffixSlice()
    {
        const string input = "any-error-message";

        string? normalized = OpenTelemetryExtensions.NormalizeExceptionTagValue(input, 5);

        Assert.Equal("...(t", normalized);
    }

    [Fact]
    public void ResolveRouteTag_WhenRouteTemplateIsAvailable_UsesRouteTemplate()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/orders/123";

        var endpoint = new RouteEndpoint(
            _ => Task.CompletedTask,
            RoutePatternFactory.Parse("orders/{orderId}"),
            0,
            EndpointMetadataCollection.Empty,
            "orders-route");

        context.SetEndpoint(endpoint);

        var options = new TelemetryOptions();

        string route = OpenTelemetryExtensions.ResolveRouteTag(context, options);

        Assert.Equal("orders/{orderId}", route);
    }

    [Fact]
    public void ResolveRouteTag_WhenRouteTemplateIsMissing_UsesBoundedDefaultFallback()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/orders/123";

        var options = new TelemetryOptions
        {
            EnableRoutePathFallback = false
        };

        string route = OpenTelemetryExtensions.ResolveRouteTag(context, options);

        Assert.Equal("/_unmatched", route);
    }

    [Fact]
    public void ResolveRouteTag_WhenPathFallbackAndNormalizationEnabled_NormalizesPath()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/orders/123/items/550e8400-e29b-41d4-a716-446655440000";

        var options = new TelemetryOptions
        {
            EnableRoutePathFallback = true,
            NormalizeRoutePathFallback = true
        };

        string route = OpenTelemetryExtensions.ResolveRouteTag(context, options);

        Assert.Equal("/orders/{id}/items/{guid}", route);
    }

    [Fact]
    public void ResolveRouteTag_WhenPathFallbackEnabledWithoutNormalization_UsesRawPath()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/orders/123/items/550e8400-e29b-41d4-a716-446655440000";

        var options = new TelemetryOptions
        {
            EnableRoutePathFallback = true,
            NormalizeRoutePathFallback = false
        };

        string route = OpenTelemetryExtensions.ResolveRouteTag(context, options);

        Assert.Equal("/orders/123/items/550e8400-e29b-41d4-a716-446655440000", route);
    }

    [Fact]
    public void GetTracingActivitySources_WhenUsingDefaults_ReturnsBoundedGenocsSources()
    {
        var options = new TelemetryOptions();

        IReadOnlyCollection<string> sources = OpenTelemetryExtensions.GetTracingActivitySources(options);

        Assert.Contains("Genocs.Saga", sources);
        Assert.Contains("Genocs.Messaging.RabbitMQ", sources);
        Assert.Contains("Genocs.Messaging.AzureServiceBus", sources);
        Assert.DoesNotContain("*", sources);
    }

    [Fact]
    public void GetTracingActivitySources_WhenWildcardEnabled_AddsWildcardSource()
    {
        var options = new TelemetryOptions
        {
            EnableWildcardActivitySources = true
        };

        IReadOnlyCollection<string> sources = OpenTelemetryExtensions.GetTracingActivitySources(options);

        Assert.Contains("*", sources);
    }

    [Fact]
    public void GetTracingActivitySources_WhenCustomSourcesConfigured_MergesAndNormalizes()
    {
        var options = new TelemetryOptions
        {
            ActivitySources =
            [
                "Custom.App",
                " custom.app ",
                "",
                "  ",
                "Other.Source"
            ]
        };

        IReadOnlyCollection<string> sources = OpenTelemetryExtensions.GetTracingActivitySources(options);

        Assert.Contains("Custom.App", sources);
        Assert.Contains("Other.Source", sources);
        Assert.Equal(5, sources.Count);
    }

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
