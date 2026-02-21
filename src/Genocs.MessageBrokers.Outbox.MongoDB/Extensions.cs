using Genocs.MessageBrokers.Outbox.Messages;
using Genocs.MessageBrokers.Outbox.MongoDB.Internals;
using Genocs.Persistence.MongoDB.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Genocs.MessageBrokers.Outbox.MongoDB;

public static class Extensions
{
    /// <summary>
    /// Add the MongoDB outbox to the service collection.
    /// </summary>
    /// <param name="configurator">The IMessageOutboxConfigurator configurator.</param>
    /// <returns>The IMessageOutboxConfigurator configurator. You can use it for chain commands.</returns>
    public static IMessageOutboxConfigurator AddMongo(this IMessageOutboxConfigurator configurator)
    {
        var builder = configurator.Builder;
        var options = configurator.Options;

        string inboxCollection = string.IsNullOrWhiteSpace(options.InboxCollection)
            ? "inbox"
            : options.InboxCollection;

        string outboxCollection = string.IsNullOrWhiteSpace(options.OutboxCollection)
            ? "outbox"
            : options.OutboxCollection;

        builder.AddMongoRepository<InboxMessage, string>(inboxCollection);
        builder.AddMongoRepository<OutboxMessage, string>(outboxCollection);

        builder.AddInitializer<MongoOutboxInitializer>();
        builder.Services.AddTransient<IMessageOutbox, MongoMessageOutbox>();
        builder.Services.AddTransient<IMessageOutboxAccessor, MongoMessageOutbox>();
        builder.Services.AddTransient<MongoOutboxInitializer>();

        BsonClassMap.RegisterClassMap<OutboxMessage>(m =>
        {
            m.AutoMap();
            m.UnmapMember(p => p.Message);
            m.UnmapMember(p => p.MessageContext);
        });

        return configurator;
    }
}