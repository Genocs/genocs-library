using Genocs.Telemetry;
using Genocs.Telemetry.Configurations;
using Xunit;

namespace Genocs.Telemetry.UnitTests;

public class OpenTelemetryExtensionsSqlClientTests
{
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
}
