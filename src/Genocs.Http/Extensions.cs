using System.ComponentModel;
using Genocs.Core.Builders;
using Genocs.Http.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Genocs.Http;

/// <summary>
/// The Http client extensions.
/// </summary>
/// <remarks>
/// Use <see cref="IHttpClientBuilder"/> (for example <c>ConfigureHttpClient</c>) to set <see cref="System.Net.Http.HttpClient.BaseAddress"/>
/// when callers pass relative URI strings to <see cref="IHttpClient"/>.
/// </remarks>
public static class Extensions
{
    private const string SectionName = "httpClient";
    private const string RegistryName = "http.client";
    private const string ClientName = "genocs";

    /// <summary>
    /// Registers the typed <see cref="IHttpClient"/>, <see cref="HttpClientOptions"/>, serializers, and optional correlation factories.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="clientName">The named <see cref="HttpClient"/> registration name.</param>
    /// <param name="maskedRequestUrlParts">Optional URL substrings to mask in logs when request masking is configured.</param>
    /// <param name="sectionName">Configuration section name for <see cref="HttpClientOptions"/>.</param>
    /// <param name="httpClientBuilder">Optional configuration for the named typed client; use <c>ConfigureHttpClient</c> to set <see cref="System.Net.Http.HttpClient.BaseAddress"/> when using relative request paths.</param>
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
            throw new ArgumentException("Http client name cannot be empty.", nameof(clientName));
        }

        var options = builder.GetOptions<HttpClientOptions>(sectionName);
        if (maskedRequestUrlParts is not null && options.RequestMasking is not null)
        {
            options.RequestMasking.UrlParts = maskedRequestUrlParts;
        }

        builder.Services.TryAddSingleton<ICorrelationContextFactory, EmptyCorrelationContextFactory>();
        builder.Services.TryAddSingleton<ICorrelationIdFactory, EmptyCorrelationIdFactory>();

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<IHttpClientSerializer, SystemTextJsonHttpClientSerializer>();
        var clientBuilder = builder.Services.AddHttpClient<IHttpClient, GenocsHttpClient>(clientName);
        clientBuilder.AddHttpMessageHandler(serviceProvider =>
            new GenocsCorrelationHeadersHttpMessageHandler(
                options,
                serviceProvider.GetRequiredService<ICorrelationContextFactory>(),
                serviceProvider.GetRequiredService<ICorrelationIdFactory>()));

        httpClientBuilder?.Invoke(clientBuilder);

        if (options.RequestMasking?.Enabled == true)
        {
            clientBuilder.AddHttpMessageHandler(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{clientName}.LogicalHandler");
                return new GenocsLoggingScopeHttpMessageHandler(logger, options);
            });
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