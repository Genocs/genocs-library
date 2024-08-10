using Azure.Identity;
using Genocs.Core.Builders;
using Genocs.Secrets.AzureKeyVault.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Genocs.Secrets.AzureKeyVault;

/// <summary>
/// The Extensions helper class.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// This method is used to add the Azure Key Vault to the Host builder.
    /// You can use the Azure Key Vault to store and manage application secrets.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="sectionName">The section name.</param>
    /// <returns>The Host builder.</returns>
    public static IHostBuilder UseAzureKeyVault(
                                        this IHostBuilder builder,
                                        string sectionName = AzureKeyVaultOptions.Position)
        => builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                // TODO Test
                if (string.IsNullOrWhiteSpace(sectionName))
                {
                    sectionName = AzureKeyVaultOptions.Position;
                }

                var settings = ctx.Configuration.GetOptions<AzureKeyVaultOptions>(sectionName);
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
                                           string sectionName = AzureKeyVaultOptions.Position)
        => builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                if (string.IsNullOrWhiteSpace(sectionName))
                {
                    sectionName = AzureKeyVaultOptions.Position;
                }

                AzureKeyVaultOptions settings = ctx.Configuration.GetOptions<AzureKeyVaultOptions>(sectionName);
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
        AzureKeyVaultOptions settings = builder.Configuration.GetOptions<AzureKeyVaultOptions>(AzureKeyVaultOptions.Position);
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