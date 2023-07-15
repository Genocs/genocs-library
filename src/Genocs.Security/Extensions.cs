using Genocs.Core.Builders;
using Genocs.Security.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Security;

public static class Extensions
{
    public static IGenocsBuilder AddSecurity(this IGenocsBuilder builder)
    {
        builder.Services
            .AddSingleton<IEncryptor, Encryptor>()
            .AddSingleton<IHasher, Hasher>()
            .AddSingleton<ISigner, Signer>();

        return builder;
    }
}