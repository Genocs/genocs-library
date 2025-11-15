namespace Genocs.Library.Demo.WebApi.Infrastructure.Services;

/// <summary>
/// The External WebApi client definition.
/// </summary>
public interface IExternalServiceClient
{
    /// <summary>
    /// Send a request for gift card issuing.
    /// </summary>
    /// <param name="request">The issuing Request.</param>
    /// <returns>The issuing Response containing the gift card details.</returns>
    Task<IssuingResponse> IssueAsync(IssuingRequest request);

    /// <summary>
    /// Get the product based on the productId.
    /// </summary>
    /// <param name="request">The redemption request.</param>
    /// <returns>The redemption Response.</returns>
    Task<string> RedeemAsync(RedemptionRequest request);
}
