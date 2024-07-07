namespace Genocs.Secrets.AzureKeyVault.Configurations;

/// <summary>
/// The vault Setting definition.
/// </summary>
public class AzureKeyVaultSettings
{
    public const string Position = "AzureKeyVault";

    /// <summary>
    /// The flag to enable or disable the Azure Key Vault.
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

}