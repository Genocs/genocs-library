using Genocs.Core.Builders;
using Genocs.Messaging.AzureServiceBus;
using Genocs.Messaging.AzureServiceBus.Queues.Interfaces;
using Genocs.Messaging.AzureServiceBus.Topics.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Genocs.Messaging.UnitTests.AzureServiceBus;

public class AzureServiceBusRegistrationExtensionsTests
{
    [Fact]
    public void AddAzureServiceBus_RegistersQueueAndTopic_WhenBothAreEnabled()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(
            queueEnabled: true,
            topicEnabled: true,
            queueConnectionString: "Endpoint=sb://queue/;SharedAccessKeyName=Root;SharedAccessKey=key",
            queueName: "orders-queue",
            topicConnectionString: "Endpoint=sb://topic/;SharedAccessKeyName=Root;SharedAccessKey=key",
            topicName: "orders-topic");
        IGenocsBuilder builder = services.AddGenocs(configuration);

        builder.AddAzureServiceBus();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAzureServiceBusQueue));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAzureServiceBusTopic));
        Assert.Equal(2, services.Count(descriptor => descriptor.ServiceType == typeof(IHostedService)));
    }

    [Fact]
    public void AddAzureServiceBus_DoesNotRegisterQueueAndTopic_WhenBothAreDisabled()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(queueEnabled: false, topicEnabled: false);
        IGenocsBuilder builder = services.AddGenocs(configuration);

        builder.AddAzureServiceBus();

        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IAzureServiceBusQueue));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IAzureServiceBusTopic));
        Assert.Equal(0, services.Count(descriptor => descriptor.ServiceType == typeof(IHostedService)));
    }

    [Fact]
    public void AddAzureServiceBus_Throws_WhenQueueIsEnabledWithoutConnectionString()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(
            queueEnabled: true,
            topicEnabled: false,
            queueConnectionString: string.Empty,
            queueName: "orders-queue");
        IGenocsBuilder builder = services.AddGenocs(configuration);

        var exception = Assert.Throws<ArgumentException>(() => builder.AddAzureServiceBus());

        Assert.Equal("connectionString", exception.ParamName, ignoreCase: true);
    }

    [Fact]
    public void AddAzureServiceBus_Throws_WhenTopicIsEnabledWithoutTopicName()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(
            queueEnabled: false,
            topicEnabled: true,
            topicConnectionString: "Endpoint=sb://topic/;SharedAccessKeyName=Root;SharedAccessKey=key",
            topicName: string.Empty);
        IGenocsBuilder builder = services.AddGenocs(configuration);

        var exception = Assert.Throws<ArgumentException>(() => builder.AddAzureServiceBus());

        Assert.Equal("topicName", exception.ParamName, ignoreCase: true);
    }

    private static IConfiguration CreateConfiguration(
        bool queueEnabled,
        bool topicEnabled,
        string? queueConnectionString = null,
        string? queueName = null,
        string? topicConnectionString = null,
        string? topicName = null)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["azureServiceBusQueue:Enabled"] = queueEnabled.ToString(),
                ["azureServiceBusQueue:ConnectionString"] = queueConnectionString,
                ["azureServiceBusQueue:QueueName"] = queueName,
                ["azureServiceBusTopic:Enabled"] = topicEnabled.ToString(),
                ["azureServiceBusTopic:ConnectionString"] = topicConnectionString,
                ["azureServiceBusTopic:TopicName"] = topicName
            })
            .Build();
}