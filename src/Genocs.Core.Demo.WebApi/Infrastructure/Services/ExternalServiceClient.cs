using Genocs.Core.Demo.WebApi.Options;
using Genocs.HTTP;
using Genocs.HTTP.Options;
using Genocs.Secrets.Vault;
using Genocs.Secrets.Vault.Options;
using Genocs.Security;
using Genocs.WebApi.Security;
using Newtonsoft.Json;

namespace Genocs.Core.Demo.WebApi.Infrastructure.Services;

/// <summary>
/// The External WebApi client implementation.
/// </summary>
public class ExternalServiceClient : IExternalServiceClient
{
    private readonly IHttpClient _client;
    private readonly string _url;
    private readonly IHasher _hasher;
    private readonly ExternalServiceSettings _externalServiceSettings;

    /// <summary>
    /// The standard constructor.
    /// </summary>
    /// <param name="client">The http client.</param>
    /// <param name="hasher">The Hash service.</param>
    /// <param name="httpClientSettings">The http client settings.</param>
    /// <param name="certificatesService">The certification service.</param>
    /// <param name="vaultSettings">The vault settings.</param>
    /// <param name="securitySettings">The security settings.</param>
    /// <param name="externalServiceSettings">The security settings.</param>
    public ExternalServiceClient(
                                IHttpClient client,
                                IHasher hasher,
                                HttpClientSettings httpClientSettings,
                                ICertificatesService certificatesService,
                                VaultSettings vaultSettings,
                                SecuritySettings securitySettings,
                                ExternalServiceSettings externalServiceSettings)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        _externalServiceSettings = externalServiceSettings ?? throw new ArgumentNullException(nameof(externalServiceSettings));

        if (httpClientSettings is null)
        {
            throw new ArgumentNullException(nameof(httpClientSettings));
        }

        if (vaultSettings is null)
        {
            throw new ArgumentNullException(nameof(vaultSettings));
        }

        if (securitySettings is null)
        {
            throw new ArgumentNullException(nameof(securitySettings));
        }

        string? url = httpClientSettings?.Services?["ca_issuer"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new Exception("products http client settings cannot be null");
        }

        _url = url;

        if (!vaultSettings.Enabled || vaultSettings.Pki?.Enabled != true ||
            securitySettings.Certificate?.Enabled != true)
        {
            return;
        }

        var certificate = certificatesService?.Get(vaultSettings.Pki.RoleName);
        if (certificate is null)
        {
            return;
        }

        string header = securitySettings.Certificate.GetHeaderName();
        string certificateData = certificate.GetRawCertDataString();
        _client.SetHeaders(h => h.Add(header, certificateData));
    }

    private void SetHeaders(string request)
    {
        string hash = _hasher.Hash(request, _externalServiceSettings.Private);
        string headerData = $"Credential={_externalServiceSettings.Public}, Signature={hash}";
        _client.SetHeaders(h => h.TryAddWithoutValidation("Authorization", headerData));
    }

    /// <summary>
    /// Send a request for gift card issuing.
    /// </summary>
    /// <param name="request">The issuing Request.</param>
    /// <returns>The issuing Response.</returns>
    public async Task<IssuingResponse> IssueAsync(IssuingRequest request)
    {
        string serializedRequest = JsonConvert.SerializeObject(request);
        SetHeaders(serializedRequest);
        using (var content = new StringContent(serializedRequest, System.Text.Encoding.UTF8, "application/json"))
        {
            return await _client.PostAsync<IssuingResponse>($"{_url}/redemptions/gift-cards/{_externalServiceSettings.Caller}/direct-issue", content);
        }
    }

    /// <summary>
    /// Get the product based on the Caller.
    /// </summary>
    /// <param name="callerId">The CallerId.</param>
    /// <returns>The Product Response.</returns>
    public async Task<string> RedeemAsync(string callerId)
    {
        // SetHeaders(callerId);
        return await _client.PostAsync<string>($"{_url}/redemptions/gift-cards/custom/redeem", new { });
    }
}