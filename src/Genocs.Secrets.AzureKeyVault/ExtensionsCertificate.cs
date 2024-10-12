using Azure.Identity;
using Genocs.Core.Builders;
using Genocs.Secrets.AzureKeyVault.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography.X509Certificates;

namespace Genocs.Secrets.AzureKeyVault;

/// <summary>
/// The ExtensionsCertificate helper class.
/// </summary>
public static class ExtensionsCertificate
{
    /// <summary>
    /// This method is used to add the Azure Key Vault to the Host builder.
    /// You can use the Azure Key Vault to store and manage application secrets.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="sectionName">The section name.</param>
    /// <returns>The Host builder.</returns>
    public static IHostBuilder UseAzureKeyVaultWithCertificate(
                                        this IHostBuilder builder,
                                        string sectionName = AzureKeyVaultOptions.Position)
        => builder.ConfigureAppConfiguration((ctx, cfg) =>
            {
                // TODO: Test
                if (string.IsNullOrWhiteSpace(sectionName))
                {
                    sectionName = AzureKeyVaultOptions.Position;
                }

                var settings = ctx.Configuration.GetOptions<AzureKeyVaultOptions>(sectionName);
                if (!settings.Enabled)
                {
                    return;
                }

                // TODO: Test
                // To use the Azure Key Vault with Certificate authentication, you need to have the certificate installed in the Current User store.
                using (var x509Store = new X509Store(StoreLocation.CurrentUser))
                {
                    x509Store.Open(OpenFlags.ReadOnly);

                    var x509Certificate = x509Store.Certificates
                        .Find(
                                X509FindType.FindByThumbprint,
                                settings.AzureADCertThumbprint!,
                                validOnly: false)
                        .OfType<X509Certificate2>()
                        .Single();

                    cfg.AddAzureKeyVault(
                                            new Uri($"https://{settings.Name}.vault.azure.net/"),
                                            new ClientCertificateCredential(
                                                                            settings.AzureADDirectoryId,
                                                                            settings.AzureADApplicationId,
                                                                            x509Certificate));

                    x509Store?.Close();
                }
            });

    /// <summary>
    /// UseVault.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="sectionName">The section name.</param>
    /// <returns>The Web Host builder.</returns>
    public static IWebHostBuilder UseAzureKeyVaultWithCertificates(
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

                using (var x509Store = new X509Store(StoreLocation.CurrentUser))
                {
                    x509Store.Open(OpenFlags.ReadOnly);

                    var x509Certificate = x509Store.Certificates
                        .Find(
                                X509FindType.FindByThumbprint,
                                settings.AzureADCertThumbprint!,
                                validOnly: false)
                        .OfType<X509Certificate2>()
                        .Single();

                    cfg.AddAzureKeyVault(
                                            new Uri($"https://{settings.Name}.vault.azure.net/"),
                                            new ClientCertificateCredential(
                                                                            settings.AzureADDirectoryId,
                                                                            settings.AzureADApplicationId,
                                                                            x509Certificate));

                    x509Store?.Close();
                }
            });

    public static WebApplicationBuilder UseAzureKeyVaultWithCertificates(this WebApplicationBuilder builder)
    {

        AzureKeyVaultOptions settings = builder.Configuration.GetOptions<AzureKeyVaultOptions>(AzureKeyVaultOptions.Position);
        if (!settings.Enabled)
        {
            return builder;
        }

        using (var x509Store = new X509Store(StoreLocation.CurrentUser))
        {
            x509Store.Open(OpenFlags.ReadOnly);

            var x509Certificate = x509Store.Certificates
                .Find(
                        X509FindType.FindByThumbprint,
                        settings.AzureADCertThumbprint!,
                        validOnly: false)
                .OfType<X509Certificate2>()
                .Single();

            builder.Configuration.AddAzureKeyVault(
                                                    new Uri($"https://{settings.Name}.vault.azure.net/"),
                                                    new ClientCertificateCredential(
                                                                                    settings.AzureADDirectoryId,
                                                                                    settings.AzureADApplicationId,
                                                                                    x509Certificate));

            x509Store?.Close();
        }

        return builder;
    }
}
