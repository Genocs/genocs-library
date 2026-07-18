namespace Genocs.Persistence.EFCore.Providers;

internal static class DbProviderResolver
{
    /// <summary>
    /// Resolves the provider matching the given provider key (case-insensitive),
    /// throwing when no registered provider supports it.
    /// </summary>
    public static IEFCoreDbProvider Resolve(this IEnumerable<IEFCoreDbProvider> providers, string dbProvider)
        => providers.TryResolve(dbProvider)
            ?? throw new InvalidOperationException($"DB Provider {dbProvider} is not supported.");

    /// <summary>
    /// Resolves the provider matching the given provider key (case-insensitive), or null when none matches.
    /// </summary>
    public static IEFCoreDbProvider? TryResolve(this IEnumerable<IEFCoreDbProvider> providers, string? dbProvider)
        => providers.FirstOrDefault(p => string.Equals(p.ProviderKey, dbProvider, StringComparison.OrdinalIgnoreCase));
}
