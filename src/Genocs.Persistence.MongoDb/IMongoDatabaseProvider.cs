namespace Genocs.Persistence.MongoDb
{
    using MongoDB.Driver;

    /// <summary>
    /// Defines interface to obtain a <see cref="MongoDatabase"/> object.
    /// </summary>
    public interface IMongoDatabaseProvider
    {
        /// <summary>
        /// Gets the <see cref="MongoDatabase"/>.
        /// </summary>
        IMongoDatabase Database { get; }
    }
}
