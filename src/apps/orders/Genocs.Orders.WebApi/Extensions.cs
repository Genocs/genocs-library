using Genocs.Core.Builders;
using Genocs.Orders.WebApi.Services;
using Genocs.WebApi.Security;

namespace Genocs.Orders.WebApi;

public static class Extensions
{
    public static IGenocsBuilder AddServices(this IGenocsBuilder builder)
    {
        builder.AddCertificateAuthentication();
        builder.Services.AddSingleton<IProductServiceClient, ProductServiceClient>();
        return builder;
    }
}