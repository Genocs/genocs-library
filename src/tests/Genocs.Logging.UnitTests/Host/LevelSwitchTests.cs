using Serilog.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Genocs.Logging.UnitTests.Host;

/// <summary>
/// Tests for LOGGING-008 (level endpoint contract).
/// </summary>
public class LevelSwitchTests
{
    // LOGGING-008 ----------------------------------------------------------------

    [Theory]
    [InlineData("Verbose", LogEventLevel.Verbose)]
    [InlineData("verbose", LogEventLevel.Verbose)]
    [InlineData("Debug", LogEventLevel.Debug)]
    [InlineData("Information", LogEventLevel.Information)]
    [InlineData("Warning", LogEventLevel.Warning)]
    [InlineData("Error", LogEventLevel.Error)]
    [InlineData("Fatal", LogEventLevel.Fatal)]
    public void GetLogEventLevel_WithValidLevelName_ReturnsMatchingLevel(string input, LogEventLevel expected)
    {
        var result = Extensions.GetLogEventLevel(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Trace")]
    [InlineData("NOTICE")]
    [InlineData("unknown")]
    public void GetLogEventLevel_WithInvalidOrUnknownLevel_ReturnsInformationDefault(string? input)
    {
        var result = Extensions.GetLogEventLevel(input);

        Assert.Equal(LogEventLevel.Information, result);
    }
}
