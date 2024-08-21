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

    /// <summary>
    /// The client id. To be used with Certificate authentication.
    /// </summary>
    public object? AzureADCertThumbprint { get; internal set; }

    /// <summary>
    /// The client id. To be used with Certificate authentication.
    /// </summary>
    public string? AzureADApplicationId { get; internal set; }

    /// <summary>
    /// The Azure EntraID tenant Id. To be used with Certificate authentication.
    /// </summary>
    public string? AzureADDirectoryId { get; internal set; }
}