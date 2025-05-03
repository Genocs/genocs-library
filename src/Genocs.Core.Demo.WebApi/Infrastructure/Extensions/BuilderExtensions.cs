﻿using Genocs.Core.Builders;
using Genocs.Core.Demo.WebApi.Configurations;
using Genocs.Core.Demo.WebApi.Infrastructure.Services;
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
    /// <returns>The Genocs builder. You can use it for chain commands.</returns>
    public static IGenocsBuilder AddApplicationServices(this IGenocsBuilder builder)
    {
        // Add the Security services
        builder.AddSecurity();

        // Add the Certification Authentication
        builder.AddCertificateAuthentication();

        // Add the Genocs Http client
        builder.AddHttpClient();

        // Add the External Service settings
        // Option 1: In this way ExternalServiceSettings is available for dependency injection.
        // var settings = new ExternalServiceSettings();
        // builder.Configuration.GetSection(ExternalServiceSettings.Position).Bind(settings);
        // builder.Services.AddSingleton(settings);

        // Option 2: In this way ExternalServiceSettings is available for dependency injection. By using IOptions<ExternalServiceSettings> you can access the settings.
        builder.Services.AddOptions<ExternalServiceOptions>()
                .BindConfiguration(ExternalServiceOptions.Position)
                .ValidateDataAnnotations()
                .ValidateOnStart();

        // builder.Services.AddSingleton<IConfigureOptions<WebApiOptions>, ConfigureWebApiSettings>();
        ExternalServiceOptions settings = builder.Configuration.GetOptions<ExternalServiceOptions>(ExternalServiceOptions.Position);
        builder.Services.AddSingleton(settings);

        // Add the External Service http Client
        builder.Services.AddTransient<IExternalServiceClient, ExternalServiceClient>();

        // builder.Services.Configure<ExternalServiceSettings>(builder.Configuration.GetSection(ExternalServiceSettings.Position));

        return builder;
    }
}
