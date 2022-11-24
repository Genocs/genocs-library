namespace Genocs.Persistence.MongoDb.Repositories
{
    using Genocs.Core.Domain.Entities;
    using MongoDB.Bson;

    /// <summary>
    /// General purpose Entity used by default in MongoDB
    /// </summary>
    public interface IMongoDbEntity : IEntity<ObjectId>
    {

    }
}
