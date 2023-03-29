using Genocs.Orders.WebApi.DTO;

namespace Genocs.Orders.WebApi.Services;

public interface IPricingServiceClient
{
    Task<PricingDto> GetAsync(Guid orderId);
}