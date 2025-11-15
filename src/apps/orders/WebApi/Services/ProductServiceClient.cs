using Genocs.HTTP;
using Genocs.HTTP.Configurations;
using Genocs.Orders.WebApi.DTO;
using Genocs.Secrets.Vault;
using Genocs.Secrets.Vault.Configurations;
using Genocs.WebApi.Security.Configurations;

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
    /// <param name="httpClientOptions">The client Options.</param>
    /// <param name="certificatesService">The certification service.</param>
    /// <param name="vaultOptions">The Key Vault Options.</param>
    /// <param name="securityOptions">The Security Options like Certificates.</param>
    public ProductServiceClient(
                                IHttpClient client,
                                HttpClientOptions httpClientOptions,
                                ICertificatesService certificatesService,
                                VaultOptions vaultOptions,
                                SecurityOptions securityOptions)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));

        ArgumentNullException.ThrowIfNull(httpClientOptions);

        ArgumentNullException.ThrowIfNull(vaultOptions);

        ArgumentNullException.ThrowIfNull(securityOptions);

        string? url = httpClientOptions?.Services?["products"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new Exception("products http client option cannot be null");
        }

        _url = url;

        if (!vaultOptions.Enabled || vaultOptions.Pki?.Enabled != true ||
            securityOptions.Certificate?.Enabled != true)
        {
            return;
        }

        var certificate = certificatesService?.Get(vaultOptions.Pki.RoleName);
        if (certificate is null)
        {
            return;
        }

        string header = securityOptions.Certificate.GetHeaderName();
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