using Azure.Identity;
using Genocs.Core.Builders;
using Genocs.Secrets.AzureKeyVault.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Genocs.Secrets.AzureKeyVault;

/// <summary>
/// The Extensions helper class.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// UseVault.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="sectionName">The section name.</param>
    /// <returns>The Host builder.</returns>
    public static IHostBuilder UseAzureKeyVault(
                                        this IHostBuilder builder,
                                        string sectionName = AzureKeyVaultSettings.Position)
        => builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                // TODO Test
                if (string.IsNullOrWhiteSpace(sectionName))
                {
                    sectionName = AzureKeyVaultSettings.Position;
                }

                var settings = ctx.Configuration.GetOptions<AzureKeyVaultSettings>(sectionName);
                if (!settings.Enabled)
                {
                    return;
                }

                cfg.AddAzureKeyVault(
                                        new Uri($"https://{settings.Name}.vault.azure.net/"),
                                        new DefaultAzureCredential(new DefaultAzureCredentialOptions
                                        {
                                            ManagedIdentityClientId = settings.ManagedIdentityId
                                        }));
            });

    /// <summary>
    /// UseVault.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="sectionName">The section name.</param>
    /// <returns>The Web Host builder.</returns>
    public static IWebHostBuilder UseAzureKeyVault(
                                           this IWebHostBuilder builder,
                                           string sectionName = AzureKeyVaultSettings.Position)
        => builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                var settings = new AzureKeyVaultSettings();
                ctx.Configuration.GetSection(sectionName).Bind(settings);
                if (!settings.Enabled)
                {
                    return;
                }

                cfg.AddAzureKeyVault(
                                    new Uri($"https://{settings.Name}.vault.azure.net/"),
                                    new DefaultAzureCredential(new DefaultAzureCredentialOptions
                                    {
                                        ManagedIdentityClientId = settings.ManagedIdentityId
                                    }));
            });

    public static WebApplicationBuilder UseAzureKeyVault(this WebApplicationBuilder builder)
    {

        var settings = new AzureKeyVaultSettings();
        builder.Configuration.GetSection(AzureKeyVaultSettings.Position).Bind(settings);
        if (!settings.Enabled)
        {
            return builder;
        }

        builder.Configuration.AddAzureKeyVault(
                                                new Uri($"https://{settings.Name}.vault.azure.net/"),
                                                new DefaultAzureCredential(new DefaultAzureCredentialOptions
                                                {
                                                    ManagedIdentityClientId = settings.ManagedIdentityId
                                                }));

        return builder;
    }
}