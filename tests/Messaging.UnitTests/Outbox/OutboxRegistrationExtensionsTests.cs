using Genocs.Core.Builders;
using Genocs.Messaging.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Messaging.UnitTests.Outbox;

public class OutboxRegistrationExtensionsTests
{
    [Fact]
    public void AddMessageOutbox_Throws_WhenProviderConfiguratorIsMissing()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());

        var exception = Assert.Throws<InvalidOperationException>(() => builder.AddMessageOutbox());

        Assert.Contains("No outbox provider was configured", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddMessageOutbox_RegistersOutbox_WhenProviderConfiguratorIsProvided()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());

        builder.AddMessageOutbox(outbox => outbox.AddInMemory());

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMessageOutbox));
    }
}