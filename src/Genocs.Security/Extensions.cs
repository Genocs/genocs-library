using Genocs.Core.Builders;
using Genocs.Core.Security.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.Security;

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