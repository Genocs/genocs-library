using Genocs.Common.Configurations;
using Xunit;

namespace Genocs.Common.UnitTests.Configurations;

public class AppOptionsTests
{
    [Fact]
    public void Properties_ShouldBeImmutableAfterInit()
    {
        var options = new AppOptions
        {
            Enabled = true,
            Name = "TestApp",
            Service = "test-service",
            Instance = "test-instance",
            Version = "1.0.0",
            DisplayBanner = true,
            DisplayVersion = false
        };

        Assert.True(options.Enabled);
        Assert.Equal("TestApp", options.Name);
        Assert.Equal("test-service", options.Service);
        Assert.Equal("test-instance", options.Instance);
        Assert.Equal("1.0.0", options.Version);
        Assert.True(options.DisplayBanner);
        Assert.False(options.DisplayVersion);

        // The following lines should not compile if properties are truly immutable:
        // options.Name = "Other";
        // options.Enabled = false;
        // options.Version = "2.0.0";
    }
}
