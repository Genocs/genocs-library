using System;
using Genocs.Common.Dependency;

namespace Genocs.Common.Interfaces;

/// <summary>
/// [Obsolete] Use <see cref="IScopedDependency"/> from Genocs.Common.Dependency instead.
/// </summary>
[Obsolete("Use IScopedDependency from Genocs.Common.Dependency instead.")]
public interface IScopedService : IScopedDependency;
