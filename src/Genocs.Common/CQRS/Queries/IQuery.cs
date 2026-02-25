namespace Genocs.Common.CQRS.Queries;

/// <summary>
/// Generic interface for query.
/// </summary>
public interface IQuery;

/// <summary>
/// Generic interface for type.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public interface IQuery<T> : IQuery;
