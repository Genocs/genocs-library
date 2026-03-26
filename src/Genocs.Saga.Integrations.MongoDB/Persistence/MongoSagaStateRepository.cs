using MongoDB.Driver;

namespace Genocs.Saga.Integrations.MongoDB.Persistence;

internal sealed class MongoSagaStateRepository : ISagaStateRepository
{
    private const string CollectionName = "SagaData";
    private readonly IMongoCollection<MongoSagaState> _collection;

    public MongoSagaStateRepository(IMongoDatabase database)
        => _collection = database.GetCollection<MongoSagaState>(CollectionName);

    public async Task<ISagaState?> ReadAsync(SagaId id, Type type)
         => await _collection
                 .Find(sld => sld.MongoId == id.Id && sld.SagaType == type.FullName)
                 .FirstOrDefaultAsync();

    public async Task WriteAsync(ISagaState sagaState)
    {
        if (sagaState.Id is null)
        {
            throw new SagaException("Saga state id must be provided.");
        }

        if (sagaState.Type is null)
        {
            throw new SagaException("Saga state type must be provided.");
        }

        MongoSagaState persistedState = new()
        {
            MongoId = sagaState.Id.Value.Id,
            SagaType = sagaState.Type.FullName,
            State = sagaState.State,
            Data = sagaState.Data,
            Version = sagaState.Version + 1
        };

        if (sagaState.Version == 0)
        {
            try
            {
                await _collection.InsertOneAsync(persistedState);
                sagaState.UpdateVersion(persistedState.Version);
                return;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new SagaConcurrencyException($"A saga state for '{sagaState.Type.FullName}' with id '{sagaState.Id.Value.Id}' already exists.", ex);
            }
        }

        ReplaceOneResult result = await _collection.ReplaceOneAsync(
            sld => sld.MongoId == sagaState.Id.Value.Id
                && sld.SagaType == sagaState.Type.FullName
                && sld.Version == sagaState.Version,
            persistedState);

        if (result.ModifiedCount == 0)
        {
            throw new SagaConcurrencyException($"Stale saga state write detected for '{sagaState.Type.FullName}' with id '{sagaState.Id.Value.Id}' at version {sagaState.Version}.");
        }

        sagaState.UpdateVersion(persistedState.Version);
    }
}
