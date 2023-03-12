namespace Genocs.Core.Types
{
    using System.Threading.Tasks;

    /// <summary>
    /// Initializer interface definition
    /// </summary>
    public interface IInitializer
    {
        /// <summary>
        /// Standard initializer
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync();
    }
}