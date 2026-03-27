using MongoDB.Driver;

namespace Genocs.Saga.Integrations.MongoDB.Persistence;

internal sealed class MongoSagaLog : ISagaLog
{
    private const string CollectionName = "SagaLog";
    private readonly IMongoCollection<MongoSagaLogData> _collection;

    public MongoSagaLog(IMongoDatabase database)
        => _collection = database.GetCollection<MongoSagaLogData>(CollectionName);

    public async Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type type)
       => await _collection
           .Find(sld => sld.SagaId == id.Id && sld.SagaType == type.FullName)
           .ToListAsync();

    public async Task WriteAsync(ISagaLogData message)
       => await _collection.InsertOneAsync(new MongoSagaLogData
       {
           EntryId = message.EntryId,
           SagaId = message.Id,
           SagaType = message.Type.FullName,
           Message = message.Message,
           CreatedAt = message.CreatedAt,
           MessageId = message.MessageId,
           Outcome = message.Outcome
       });

    public async Task UpdateOutcomeAsync(SagaId id, Type type, string entryId, SagaLogEntryOutcome outcome)
    {
        UpdateResult result = await _collection.UpdateOneAsync(
            sld => sld.SagaId == id.Id && sld.SagaType == type.FullName && sld.EntryId == entryId,
            Builders<MongoSagaLogData>.Update.Set(sld => sld.Outcome, outcome));

        if (result.MatchedCount == 0)
        {
            throw new SagaException($"Saga log entry '{entryId}' was not found for '{type.FullName}' and id '{id.Id}'.");
        }
    }
}
