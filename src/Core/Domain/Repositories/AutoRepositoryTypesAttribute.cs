namespace Genocs.Core.Domain.Repositories;

/// <summary>
/// Used to define auto-repository types for entities.
/// This can be used for DbContext types.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoRepositoryTypesAttribute(
                                    Type repositoryInterface,
                                    Type repositoryInterfaceWithPrimaryKey,
                                    Type repositoryImplementation,
                                    Type repositoryImplementationWithPrimaryKey) : Attribute
{
    public Type RepositoryInterface { get; } = repositoryInterface;

    public Type RepositoryInterfaceWithPrimaryKey { get; } = repositoryInterfaceWithPrimaryKey;

    public Type RepositoryImplementation { get; } = repositoryImplementation;

    public Type RepositoryImplementationWithPrimaryKey { get; } = repositoryImplementationWithPrimaryKey;

    public bool WithDefaultRepositoryInterfaces { get; set; }
}