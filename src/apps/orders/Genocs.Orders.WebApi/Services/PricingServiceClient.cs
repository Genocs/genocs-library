using Genocs.HTTP;
using Genocs.HTTP.Options;
using Genocs.Orders.WebApi.DTO;
using Genocs.Secrets.Vault;
using Genocs.WebApi.Security;

namespace Genocs.Orders.WebApi.Services;


/// <summary>
/// The Pricing WebApi client implementation
/// </summary>
public class PricingServiceClient : IPricingServiceClient
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
    public PricingServiceClient(IHttpClient client,
                                ICertificatesService certificatesService,
                                HttpClientSettings httpClientOptions,
                                VaultOptions vaultOptions,
                                SecurityOptions securityOptions)
    {
        _client = client;
        _url = httpClientOptions.Services["pricing"];
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
    /// Get the pricing result based on the orderId
    /// </summary>
    /// <param name="orderId">The OrderId</param>
    /// <returns>The Pricing Response</returns>
    public Task<PricingDto> GetAsync(Guid orderId)
        => _client.GetAsync<PricingDto>($"{_url}/orders/{orderId}/pricing");
}