using Genocs.Core.Builders;
using Genocs.WebApi.Security;

namespace Genocs.Products.WebApi;

public static class Extensions
{
    public static IGenocsBuilder AddServices(this IGenocsBuilder builder)
    {
        builder.AddCertificateAuthentication();

        // Add here services like API client
        //       builder.Services.AddSingleton<IPricingServiceClient, PricingServiceClient>();
        return builder;
    }
}