using Genocs.Core.Builders;
using Genocs.Core.Demo.WebApi.Infrastructure.Services;
using Genocs.Core.Demo.WebApi.Options;
using Genocs.WebApi.Security;

namespace Genocs.Core.Demo.WebApi.Infrastructure.Extensions;

public static class BuilderExtensions
{
    public static IGenocsBuilder AddServices(this IGenocsBuilder builder)
    {
        builder.AddCertificateAuthentication();

        builder.Services.AddTransient<IExternalServiceClient, ExternalServiceClient>();

        var settings = new ExternalServiceSettings();
        builder.Configuration.GetSection(ExternalServiceSettings.Position).Bind(settings);

        builder.Services.AddSingleton(settings);

        // builder.Services.Configure<ExternalServiceSettings>(builder.Configuration.GetSection(ExternalServiceSettings.Position));

        return builder;
    }

}
