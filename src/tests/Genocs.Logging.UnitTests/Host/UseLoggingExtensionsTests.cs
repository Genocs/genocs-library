using Genocs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Genocs.Logging.UnitTests.Host;

public class UseLoggingExtensionsTests
{
    [Fact]
    public void UseLogging_WhenSeqEnabledWithoutUrl_BuildsHostWithoutThrowing()
    {
        using IHost host = BuildHost(new Dictionary<string, string>
        {
            ["logger:enabled"] = "true",
            ["logger:seq:enabled"] = "true"
        });

        Assert.NotNull(host.Services.GetService<ILoggingService>());
    }

    [Fact]
    public void UseLogging_WhenLokiEnabledWithoutUrl_BuildsHostWithoutThrowing()
    {
        using IHost host = BuildHost(new Dictionary<string, string>
        {
            ["logger:enabled"] = "true",
            ["logger:loki:enabled"] = "true"
        });

        Assert.NotNull(host.Services.GetService<ILoggingService>());
    }

    [Fact]
    public void UseLogging_WhenLoggerDisabled_IgnoresInvalidSinkSettingsAndBuildsHost()
    {
        using IHost host = BuildHost(new Dictionary<string, string>
        {
            ["logger:enabled"] = "false",
            ["logger:seq:enabled"] = "true",
            ["logger:loki:enabled"] = "true",
            ["logger:loki:url"] = " ",
            ["logger:seq:url"] = ""
        });

        Assert.NotNull(host.Services.GetService<ILoggingService>());
    }

    private static IHost BuildHost(Dictionary<string, string> configuration)
    {
        var builder = new HostBuilder()
            .ConfigureAppConfiguration((_, cfg) => cfg.AddInMemoryCollection(configuration));

        builder.UseLogging();

        return builder.Build();
    }
}
