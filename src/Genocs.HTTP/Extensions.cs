using Genocs.Core.Builders;
using Genocs.HTTP.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using System.ComponentModel;

namespace Genocs.HTTP;

/// <summary>
/// The HTTP client extensions.
/// </summary>
public static class Extensions
{
    private const string SectionName = "httpClient";
    private const string RegistryName = "http.client";
    private const string ClientName = "genocs";

    public static IGenocsBuilder AddHttpClient(
                                                this IGenocsBuilder builder,
                                                string clientName = ClientName,
                                                IEnumerable<string>? maskedRequestUrlParts = null,
                                                string sectionName = SectionName,
                                                Action<IHttpClientBuilder>? httpClientBuilder = null)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = SectionName;
        }

        if (!builder.TryRegister(RegistryName))
        {
            return builder;
        }

        if (string.IsNullOrWhiteSpace(clientName))
        {
            throw new ArgumentException("HTTP client name cannot be empty.", nameof(clientName));
        }

        var options = builder.GetOptions<HttpClientOptions>(sectionName);
        if (maskedRequestUrlParts is not null && options.RequestMasking is not null)
        {
            options.RequestMasking.UrlParts = maskedRequestUrlParts;
        }

        bool registerCorrelationContextFactory;
        bool registerCorrelationIdFactory;
        using (var scope = builder.Services.BuildServiceProvider().CreateScope())
        {
            registerCorrelationContextFactory = scope.ServiceProvider.GetService<ICorrelationContextFactory>() is null;
            registerCorrelationIdFactory = scope.ServiceProvider.GetService<ICorrelationIdFactory>() is null;
        }

        if (registerCorrelationContextFactory)
        {
            builder.Services.AddSingleton<ICorrelationContextFactory, EmptyCorrelationContextFactory>();
        }

        if (registerCorrelationIdFactory)
        {
            builder.Services.AddSingleton<ICorrelationIdFactory, EmptyCorrelationIdFactory>();
        }

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<IHttpClientSerializer, SystemTextJsonHttpClientSerializer>();
        var clientBuilder = builder.Services.AddHttpClient<IHttpClient, GenocsHttpClient>(clientName);
        httpClientBuilder?.Invoke(clientBuilder);

        if (options.RequestMasking?.Enabled == true)
        {
            builder.Services.Replace(ServiceDescriptor
                .Singleton<IHttpMessageHandlerBuilderFilter, GenocsHttpLoggingFilter>());
        }

        return builder;
    }

    [Description("This is a hack related to HttpClient issue: https://github.com/aspnet/AspNetCore/issues/13346")]
    public static void RemoveHttpClient(this IGenocsBuilder builder)
    {
        var registryType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
            .SingleOrDefault(t => t.Name == "HttpClientMappingRegistry");

        object? registry = builder.Services.SingleOrDefault(s => s.ServiceType == registryType)?.ImplementationInstance;
        var registrations = registry?.GetType().GetProperty("TypedClientRegistrations");
        var clientRegistrations = registrations?.GetValue(registry) as IDictionary<Type, string>;
        clientRegistrations?.Remove(typeof(IHttpClient));
    }
}