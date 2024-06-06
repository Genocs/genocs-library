using Genocs.Core.Builders;
using Genocs.Security.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Security;

public static class Extensions
{
    /// <summary>
    /// Extension method to add security services to the DI container.
    /// The AddSecurity method adds the following services to the DI container:
    /// The Encryptor service <see cref="IEncryptor"/>.
    /// The Hasher service <see cref="IHasher"/>.
    /// The Signer service  <see cref="ISigner"/>.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <returns>The builder to be used for chain.</returns>
    public static IGenocsBuilder AddSecurity(this IGenocsBuilder builder)
    {
        builder.Services
            .AddSingleton<IEncryptor, Encryptor>()
            .AddSingleton<IHasher, Hasher>()
            .AddSingleton<ISigner, Signer>();

        return builder;
    }
}