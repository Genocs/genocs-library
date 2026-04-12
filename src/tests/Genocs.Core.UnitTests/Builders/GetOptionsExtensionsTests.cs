using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Core.UnitTests.Builders;

public class GetOptionsExtensionsTests
{
    [Fact]
    public void GetOptions_UsesExplicitConfiguration_WhenProvided()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration("Catalog");

        IGenocsBuilder builder = services.AddGenocs(configuration);

        AppOptions options = builder.GetOptions<AppOptions>(AppOptions.Position);

        Assert.Equal("Catalog", options.Name);
        Assert.Same(configuration, builder.Configuration);
    }

    [Fact]
    public void AddGenocs_UsesPreRegisteredConfiguration_WhenExplicitConfigurationMissing()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration("Identity");
        services.AddSingleton(configuration);

        IGenocsBuilder builder = services.AddGenocs();

        AppOptions options = builder.GetOptions<AppOptions>(AppOptions.Position);

        Assert.Equal("Identity", options.Name);
        Assert.Same(configuration, builder.Configuration);
    }

    [Fact]
    public void AddGenocs_ProvidesDeterministicEmptyConfiguration_WhenNoConfigurationIsRegistered()
    {
        var services = new ServiceCollection();

        IGenocsBuilder builder = services.AddGenocs();

        using ServiceProvider provider = services.BuildServiceProvider();
        IConfiguration resolvedConfiguration = provider.GetRequiredService<IConfiguration>();
        AppOptions options = builder.GetOptions<AppOptions>(AppOptions.Position);

        Assert.NotNull(builder.Configuration);
        Assert.Same(builder.Configuration, resolvedConfiguration);
        Assert.NotNull(options);
    }

    private static IConfiguration CreateConfiguration(string serviceName)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [$"{AppOptions.Position}:Name"] = serviceName,
                [$"{AppOptions.Position}:DisplayBanner"] = "false"
            })
            .Build();
    }
}
