using Genocs.Core.Builders;
using Genocs.Messaging.AzureServiceBus.Configurations;
using Genocs.Messaging.AzureServiceBus.Queues;
using Genocs.Messaging.AzureServiceBus.Queues.Interfaces;
using Genocs.Messaging.AzureServiceBus.Topics;
using Genocs.Messaging.AzureServiceBus.Topics.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Genocs.Messaging.AzureServiceBus;

/// <summary>
/// Azure Service Bus registration extensions.
/// </summary>
public static class Extensions
{
    private const string RegistryName = "messageBrokers.azureServiceBus";

    /// <summary>
    /// Registers Azure Service Bus queue/topic services and binds options from configuration.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="queueSectionName">Configuration section for queue options.</param>
    /// <param name="topicSectionName">Configuration section for topic options.</param>
    /// <returns>The Genocs builder for chaining.</returns>
    /// <exception cref="ArgumentException">Raised when enabled configuration is invalid.</exception>
    public static IGenocsBuilder AddAzureServiceBus(
        this IGenocsBuilder builder,
        string queueSectionName = AzureServiceBusQueueOptions.Position,
        string topicSectionName = AzureServiceBusTopicOptions.Position)
    {
        if (string.IsNullOrWhiteSpace(queueSectionName))
        {
            queueSectionName = AzureServiceBusQueueOptions.Position;
        }

        if (string.IsNullOrWhiteSpace(topicSectionName))
        {
            topicSectionName = AzureServiceBusTopicOptions.Position;
        }

        var queueOptions = builder.GetOptions<AzureServiceBusQueueOptions>(queueSectionName);
        var topicOptions = builder.GetOptions<AzureServiceBusTopicOptions>(topicSectionName);

        builder.Services.AddSingleton(queueOptions);
        builder.Services.AddSingleton(topicOptions);

        if (!builder.TryRegister(RegistryName))
        {
            return builder;
        }

        ValidateOptions(queueOptions, topicOptions);

        if (queueOptions.Enabled)
        {
            builder.Services.AddSingleton<AzureServiceBusQueue>();
            builder.Services.AddSingleton<IAzureServiceBusQueue>(sp => sp.GetRequiredService<AzureServiceBusQueue>());
            builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<AzureServiceBusQueue>());
        }

        if (topicOptions.Enabled)
        {
            builder.Services.AddSingleton<AzureServiceBusTopic>();
            builder.Services.AddSingleton<IAzureServiceBusTopic>(sp => sp.GetRequiredService<AzureServiceBusTopic>());
            builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<AzureServiceBusTopic>());
        }

        return builder;
    }

    private static void ValidateOptions(AzureServiceBusQueueOptions queueOptions, AzureServiceBusTopicOptions topicOptions)
    {
        if (queueOptions.Enabled)
        {
            if (string.IsNullOrWhiteSpace(queueOptions.ConnectionString))
            {
                throw new ArgumentException("Azure Service Bus queue connection string is not specified.", nameof(queueOptions.ConnectionString));
            }

            if (string.IsNullOrWhiteSpace(queueOptions.QueueName))
            {
                throw new ArgumentException("Azure Service Bus queue name is not specified.", nameof(queueOptions.QueueName));
            }
        }

        if (topicOptions.Enabled)
        {
            if (string.IsNullOrWhiteSpace(topicOptions.ConnectionString))
            {
                throw new ArgumentException("Azure Service Bus topic connection string is not specified.", nameof(topicOptions.ConnectionString));
            }

            if (string.IsNullOrWhiteSpace(topicOptions.TopicName))
            {
                throw new ArgumentException("Azure Service Bus topic name is not specified.", nameof(topicOptions.TopicName));
            }
        }
    }
}