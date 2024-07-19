using Genocs.Core.Demo.WebApi.Configurations;
using Genocs.HTTP;
using Genocs.HTTP.Configurations;
using Newtonsoft.Json;

namespace Genocs.Core.Demo.WebApi.Infrastructure.Services;

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

        if (httpClientSettings is null)
        {
            throw new ArgumentNullException(nameof(httpClientSettings));
        }

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
    /// <returns>The response.</returns>
    public async Task<VerificationApiResponse> VerifyAsync(VerificationApiRequest request)
    {
        SetHeaders();
        string serializedRequest = JsonConvert.SerializeObject(request);
        using (var content = new StringContent(serializedRequest, System.Text.Encoding.UTF8, "application/json"))
        {
            return await _client.PostAsync<VerificationApiResponse>($"{_url}/clients", request);
        }
    }
}