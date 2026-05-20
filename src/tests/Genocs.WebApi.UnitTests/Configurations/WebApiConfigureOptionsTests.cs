using Genocs.WebApi.Configurations;
using Microsoft.Extensions.Options;
using Xunit;

namespace Genocs.WebApi.UnitTests.Configurations;

public class WebApiConfigureOptionsTests
{
    [Fact]
    public void Configure_DoesNotThrow_AndPreservesExplicitValues()
    {
        var options = new WebApiOptions
        {
            BindRequestFromRoute = true
        };

        var sut = new WebApiConfigureOptions();

        sut.Configure(options);

        Assert.True(options.BindRequestFromRoute);
    }

    [Fact]
    public void Configure_DoesNotMutateDefaultValues()
    {
        var options = new WebApiOptions();
        var sut = new WebApiConfigureOptions();

        sut.Configure(options);

        Assert.False(options.BindRequestFromRoute);
    }

    [Fact]
    public void Configure_ThrowsArgumentNullException_WhenOptionsAreNull()
    {
        var sut = new WebApiConfigureOptions();

        Assert.Throws<ArgumentNullException>(() => sut.Configure((WebApiOptions)null!));
    }

    [Fact]
    public void ConfigureNamed_DelegatesToUnnamedConfigure()
    {
        var options = new WebApiOptions
        {
            BindRequestFromRoute = true
        };
        var sut = new WebApiConfigureOptions();

        sut.Configure("webApi", options);

        Assert.True(options.BindRequestFromRoute);
    }

    [Fact]
    public void Constructor_WithIOptions_IsBackwardCompatible()
    {
        IOptions<WebApiOptions> sourceOptions = Options.Create(new WebApiOptions
        {
            BindRequestFromRoute = true
        });

        var sut = new WebApiConfigureOptions(sourceOptions);
        var targetOptions = new WebApiOptions();

        sut.Configure(targetOptions);

        Assert.False(targetOptions.BindRequestFromRoute);
    }
}
