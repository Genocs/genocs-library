using Genocs.HTTP;
using Genocs.HTTP.Options;
using Genocs.Orders.WebApi.DTO;
using Genocs.Secrets.Vault;
using Genocs.Secrets.Vault.Options;
using Genocs.WebApi.Security;

namespace Genocs.Orders.WebApi.Services;


/// <summary>
/// The Product WebApi client implementation
/// </summary>
public class ProductServiceClient : IProductServiceClient
{
    private readonly IHttpClient _client;
    private readonly string _url;

    /// <summary>
    /// The standard constructor
    /// </summary>
    /// <param name="client"></param>
    /// <param name="certificatesService"></param>
    /// <param name="httpClientOptions"></param>
    /// <param name="vaultOptions"></param>
    /// <param name="securityOptions"></param>
    public ProductServiceClient(IHttpClient client,
                                ICertificatesService certificatesService,
                                HttpClientSettings httpClientOptions,
                                VaultSettings vaultOptions,
                                SecurityOptions securityOptions)
    {
        _client = client;
        _url = httpClientOptions.Services["products"];
        if (!vaultOptions.Enabled || vaultOptions.Pki?.Enabled != true ||
            securityOptions.Certificate?.Enabled != true)
        {
            return;
        }

        var certificate = certificatesService.Get(vaultOptions.Pki.RoleName);
        if (certificate is null)
        {
            return;
        }

        var header = securityOptions.Certificate.GetHeaderName();
        var certificateData = certificate.GetRawCertDataString();
        _client.SetHeaders(h => h.Add(header, certificateData));
    }

    /// <summary>
    /// Get the product result based on the productId
    /// </summary>
    /// <param name="productId">The ProductId</param>
    /// <returns>The Product Response</returns>
    public Task<ProductDto> GetAsync(Guid productId)
        => _client.GetAsync<ProductDto>($"{_url}/products/{productId}");
}