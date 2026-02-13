namespace Genocs.Common.Persistence.Initialization;

/// <summary>
/// This interface is used to define a custom seeder for the database.
/// It can be implemented by any class that wants to seed the database with custom data.
/// </summary>
public interface ICustomSeeder
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}