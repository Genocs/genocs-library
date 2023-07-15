using Genocs.Common.Types;


namespace Genocs.Core.Domain.Repositories;

/// <summary>
/// This interface is used to identify a repository so can be used to be registered by convention.
/// Implement generic version instead of this one.
/// </summary>
public interface IRepository<TEntity, in TIdentifiable> where TEntity : IIdentifiable<TIdentifiable>
{

}