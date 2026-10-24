using Genocs.Common.Dependency;

namespace Genocs.Common.Interfaces;

/// <summary>
/// [Obsolete] Use <see cref="ITransientDependency"/> from Genocs.Common.Dependency instead.
/// </summary>
[Obsolete("Use ITransientDependency from Genocs.Common.Dependency instead.")]
public interface ITransientService : ITransientDependency;
