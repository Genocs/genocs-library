using Azure.Core;

namespace Genocs.Secrets.AzureKeyVault.Configurations;

/// <summary>
/// The Azure Kay Vault setting definition.
/// </summary>
public class AzureKeyVaultOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "azureKeyVault";

    /// <summary>
    /// It defines whether the section is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The name of the Azure Key Vault.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The managed identity id.
    /// </summary>
    public string? ManagedIdentityId { get; set; }
    public object? AzureADCertThumbprint { get; internal set; }
    public string? AzureADApplicationId { get; internal set; }
    public string? AzureADDirectoryId { get; internal set; }
}