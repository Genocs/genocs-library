namespace Genocs.Persistence.MongoDb.Repository
{
    using Genocs.Core.Domain.Entities;
    using MongoDB.Bson;

    public interface IMongoDbEntity : IEntity<ObjectId>
    {

    }
}
