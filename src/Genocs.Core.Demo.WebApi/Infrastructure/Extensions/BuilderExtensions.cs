﻿using Genocs.Core.Builders;
using Genocs.Core.Demo.WebApi.Infrastructure.Services;
using Genocs.Core.Demo.WebApi.Options;
using Genocs.HTTP;
using Genocs.Security;
using Genocs.WebApi.Security;

namespace Genocs.Core.Demo.WebApi.Infrastructure.Extensions;

public static class BuilderExtensions
{
    /// <summary>
    /// Extension method to add application services to the DI container.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <returns>The Genocs builder to be used for chain.</returns>
    public static IGenocsBuilder AddApplicationServices(this IGenocsBuilder builder)
    {
        // Add the Security services
        builder.AddSecurity();

        // Add the Certification Authentication
        builder.AddCertificateAuthentication();

        // Add the Genocs Http client
        builder.AddHttpClient();

        // Add the External Service settings
        var settings = new ExternalServiceSettings();
        builder.Configuration.GetSection(ExternalServiceSettings.Position).Bind(settings);
        builder.Services.AddSingleton(settings);

        // Add the External Service http Client
        builder.Services.AddTransient<IExternalServiceClient, ExternalServiceClient>();

        // builder.Services.Configure<ExternalServiceSettings>(builder.Configuration.GetSection(ExternalServiceSettings.Position));

        return builder;
    }
}
