namespace Genocs.Core.Demo.WebApi.Infrastructure.Services;

/// <summary>
/// The External WebApi client definition.
/// This API client is used to verify the user.
/// </summary>
public interface IVerificationServiceClient
{
    /// <summary>
    /// Send a request To verify the user.
    /// </summary>
    /// <param name="request">The request with the user data.</param>
    /// <returns>The user verification response.</returns>
    Task<VerificationApiResponse> VerifyAsync(VerificationApiRequest request);
}
