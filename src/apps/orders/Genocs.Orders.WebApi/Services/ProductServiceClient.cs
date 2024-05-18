using Genocs.HTTP;
using Genocs.HTTP.Options;
using Genocs.Orders.WebApi.DTO;
using Genocs.Secrets.Vault;
using Genocs.Secrets.Vault.Options;
using Genocs.WebApi.Security;

namespace Genocs.Orders.WebApi.Services;

/// <summary>
/// The Product WebApi client implementation.
/// </summary>
public class ProductServiceClient : IProductServiceClient
{
    private readonly IHttpClient _client;
    private readonly string _url;

    /// <summary>
    /// The standard constructor.
    /// </summary>
    /// <param name="client">The http client.</param>
    /// <param name="certificatesService">The certification service.</param>
    /// <param name="httpClientOptions"></param>
    /// <param name="vaultOptions"></param>
    /// <param name="securitySettings"></param>
    public ProductServiceClient(
                                IHttpClient client,
                                ICertificatesService certificatesService,
                                HttpClientSettings httpClientOptions,
                                VaultSettings vaultOptions,
                                SecuritySettings securitySettings)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));


        if (httpClientOptions is null)
        {
            throw new ArgumentNullException(nameof(httpClientOptions));
        }

        if (vaultOptions is null)
        {
            throw new ArgumentNullException(nameof(vaultOptions));
        }

        if (securitySettings is null)
        {
            throw new ArgumentNullException(nameof(securitySettings));
        }

        string? url = httpClientOptions?.Services?["products"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new Exception("products http client option cannot be null");
        }

        _url = url;

        if (!vaultOptions.Enabled || vaultOptions.Pki?.Enabled != true ||
            securitySettings.Certificate?.Enabled != true)
        {
            return;
        }

        var certificate = certificatesService?.Get(vaultOptions.Pki.RoleName);
        if (certificate is null)
        {
            return;
        }

        string header = securitySettings.Certificate.GetHeaderName();
        string certificateData = certificate.GetRawCertDataString();
        _client.SetHeaders(h => h.Add(header, certificateData));
    }

    /// <summary>
    /// Get the product result based on the productId.
    /// </summary>
    /// <param name="productId">The ProductId.</param>
    /// <returns>The Product Response.</returns>
    public Task<ProductDto> GetAsync(Guid productId)
        => _client.GetAsync<ProductDto>($"{_url}/products/{productId}");
}