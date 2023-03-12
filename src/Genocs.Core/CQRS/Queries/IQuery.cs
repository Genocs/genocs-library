namespace Genocs.Core.CQRS.Queries
{
    /// <summary>
    /// Generic interface for query. 
    /// </summary>
    public interface IQuery
    {
    }

    /// <summary>
    /// Generic interface for type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQuery<T> : IQuery
    {
    }
}