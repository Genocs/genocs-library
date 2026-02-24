using Genocs.Http;
using Genocs.Http.Configurations;
using Genocs.Library.Demo.WebApi.Configurations;
using Newtonsoft.Json;

namespace Genocs.Library.Demo.WebApi.Infrastructure.Services;

/// <summary>
/// The Verification WebApi client implementation.
/// </summary>
public class VerificationServiceClient : IVerificationServiceClient
{
    private readonly IHttpClient _client;
    private readonly string _url;
    private readonly VerificationServiceOptions _externalServiceSettings;

    /// <summary>
    /// The standard constructor.
    /// </summary>
    /// <param name="client">The http client.</param>
    /// <param name="httpClientSettings">The http client settings.</param>
    /// <param name="externalServiceSettings">The security settings.</param>
    public VerificationServiceClient(
                                IHttpClient client,
                                HttpClientOptions httpClientSettings,
                                VerificationServiceOptions externalServiceSettings)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _externalServiceSettings = externalServiceSettings ?? throw new ArgumentNullException(nameof(externalServiceSettings));

        ArgumentNullException.ThrowIfNull(httpClientSettings);

        string? url = httpClientSettings?.Services?["user_verifier"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new Exception("user_verifier http client settings cannot be null");
        }

        _url = url;
    }

    private void SetHeaders()
    {
        _client.SetHeaders(h => h.TryAddWithoutValidation("Authorization", _externalServiceSettings.ApiKey));
    }

    /// <summary>
    /// The method to verify the user.
    /// </summary>
    /// <param name="request">The verification api request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response.</returns>
    public async Task<VerificationApiResponse?> VerifyAsync(VerificationApiRequest request, CancellationToken cancellationToken = default)
    {
        SetHeaders();
        string serializedRequest = JsonConvert.SerializeObject(request);
        using var content = new StringContent(serializedRequest, System.Text.Encoding.UTF8, "application/json");
        return await _client.PostAsync<VerificationApiResponse>($"{_url}/clients", request);
    }
}