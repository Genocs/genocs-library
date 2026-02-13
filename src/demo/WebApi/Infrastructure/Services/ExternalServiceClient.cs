using Genocs.HTTP;
using Genocs.HTTP.Configurations;
using Genocs.Library.Demo.WebApi.Configurations;
using Genocs.Security;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Genocs.Library.Demo.WebApi.Infrastructure.Services;

/// <summary>
/// The External WebApi client implementation.
/// </summary>
public class ExternalServiceClient : IExternalServiceClient
{
    private readonly IHttpClient _client;
    private readonly string _url;
    private readonly IHasher _hasher;
    private readonly ExternalServiceOptions _externalServiceOptions;

    /// <summary>
    /// The standard constructor.
    /// </summary>
    /// <param name="client">The http client.</param>
    /// <param name="hasher">The Hash service.</param>
    /// <param name="httpClientSettings">The http client settings.</param>
    /// <param name="options">The security settings.</param>
    public ExternalServiceClient(
                                IHttpClient client,
                                IHasher hasher,
                                HttpClientOptions httpClientSettings,
                                IOptions<ExternalServiceOptions> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        _externalServiceOptions = options.Value ?? throw new ArgumentNullException(nameof(options));

        ArgumentNullException.ThrowIfNull(httpClientSettings);

        string? url = httpClientSettings?.Services?["ca_issuer"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new Exception("products http client settings cannot be null");
        }

        _url = url;
    }

    private void SetHeaders(string request)
    {
        string hash = _hasher.Hash(request, _externalServiceOptions.Private);
        string headerData = $"Credential={_externalServiceOptions.Public}, Signature={hash}";
        _client.SetHeaders(h => h.TryAddWithoutValidation("Authorization", headerData));
    }

    /// <summary>
    /// Send a request for gift card issuing.
    /// </summary>
    /// <param name="request">The issuing Request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The issuing Response containing the gift card details.</returns>
    public async Task<IssuingResponse?> IssueAsync(IssuingRequest request, CancellationToken cancellationToken = default)
    {
        string serializedRequest = JsonConvert.SerializeObject(request);
        SetHeaders(serializedRequest);
        using var content = new StringContent(serializedRequest, System.Text.Encoding.UTF8, "application/json");
        return await _client.PostAsync<IssuingResponse>($"{_url}/redemptions/gift-cards/{_externalServiceOptions.Caller}/direct-issue", content);
    }

    /// <summary>
    /// Get the product based on the productId.
    /// </summary>
    /// <param name="request">The redemption request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The redemption Response.</returns>
    public async Task<string?> RedeemAsync(RedemptionRequest request, CancellationToken cancellationToken = default)
    {
        // SetHeaders(callerId);
        return await _client.PostAsync<string>($"{_url}/redemptions/gift-cards/custom/redeem", request);
    }
}