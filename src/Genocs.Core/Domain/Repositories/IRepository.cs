namespace Genocs.Core.Domain.Repositories
{
    using Genocs.Core.Dependency;

    /// <summary>
    /// This interface must be implemented by all repositories to identify them by convention.
    /// Implement generic version instead of this one.
    /// </summary>
    public interface IRepository : ITransientDependency
    {
        
    }
}