namespace Genocs.Core.Domain.Repositories;

/// <summary>
/// Represents a unit of work that can be used to save changes to the database.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves the changes made in the unit of work to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> Save();
}