using Genocs.Common.Types;
using Genocs.MessageBrokers.Outbox.Configurations;
using Genocs.MessageBrokers.Outbox.Messages;
using MongoDB.Driver;

namespace Genocs.MessageBrokers.Outbox.MongoDB.Internals;

internal sealed class MongoOutboxInitializer : IInitializer
{
    private readonly IMongoDatabase _database;
    private readonly OutboxOptions _options;

    public MongoOutboxInitializer(IMongoDatabase database, OutboxOptions options)
    {
        _database = database;
        _options = options;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        if (_options.Expiry <= 0)
        {
            return;
        }

        string inboxCollection = string.IsNullOrWhiteSpace(_options.InboxCollection)
            ? "inbox"
            : _options.InboxCollection;

        var inboxBuilder = Builders<InboxMessage>.IndexKeys;
        await _database.GetCollection<InboxMessage>(inboxCollection)
            .Indexes.CreateOneAsync(
                new CreateIndexModel<InboxMessage>(
                                                    inboxBuilder.Ascending(i => i.ProcessedAt),
                                                    new CreateIndexOptions
                                                    {
                                                        ExpireAfter = TimeSpan.FromSeconds(_options.Expiry)
                                                    }),
                cancellationToken: cancellationToken);

        string outboxCollection = string.IsNullOrWhiteSpace(_options.OutboxCollection)
            ? "outbox"
            : _options.OutboxCollection;

        var outboxBuilder = Builders<OutboxMessage>.IndexKeys;
        await _database.GetCollection<OutboxMessage>(outboxCollection)
            .Indexes.CreateOneAsync(
                new CreateIndexModel<OutboxMessage>(
                                                    outboxBuilder.Ascending(i => i.ProcessedAt),
                                                    new CreateIndexOptions
                                                    {
                                                        ExpireAfter = TimeSpan.FromSeconds(_options.Expiry)
                                                    }),
                cancellationToken: cancellationToken);
    }
}