using Genocs.Orders.WebApi.DTO;

namespace Genocs.Orders.WebApi.Services;

/// <summary>
/// The Pricing WebApi client definition
/// </summary>
public interface IPricingServiceClient
{
    /// <summary>
    /// Get the pricing result based on the orderId
    /// </summary>
    /// <param name="orderId">The OrderId</param>
    /// <returns>The Pricing Response</returns>
    Task<PricingDto> GetAsync(Guid orderId);
}