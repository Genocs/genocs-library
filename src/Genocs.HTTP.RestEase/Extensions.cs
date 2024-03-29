using Genocs.Core.Builders;
using Genocs.Discovery.Consul;
using Genocs.Discovery.Consul.Options;
using Genocs.HTTP.Options;
using Genocs.HTTP.RestEase.Builders;
using Genocs.HTTP.RestEase.Options;
using Genocs.HTTP.RestEase.Serializers;
using Genocs.LoadBalancing.Fabio;
using Genocs.LoadBalancing.Fabio.Options;
using Microsoft.Extensions.DependencyInjection;
using RestEase;

namespace Genocs.HTTP.RestEase;

public static class Extensions
{
    private const string SectionName = "restEase";
    private const string RegistryName = "http.restEase";

    public static IGenocsBuilder AddServiceClient<T>(this IGenocsBuilder builder, string serviceName,
        string sectionName = SectionName, string consulSectionName = "consul", string fabioSectionName = "fabio",
        string httpClientSectionName = "httpClient")
        where T : class
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = SectionName;
        }

        var restEaseOptions = builder.GetOptions<RestEaseSettings>(sectionName);
        return builder.AddServiceClient<T>(serviceName, restEaseOptions,
            b => b.AddFabio(fabioSectionName, consulSectionName, httpClientSectionName));
    }

    public static IGenocsBuilder AddServiceClient<T>(this IGenocsBuilder builder, string serviceName,
        Func<IRestEaseSettingsBuilder, IRestEaseSettingsBuilder> buildOptions,
        Func<IConsulOptionsBuilder, IConsulOptionsBuilder> buildConsulOptions,
        Func<IFabioOptionsBuilder, IFabioOptionsBuilder> buildFabioOptions,
        HttpClientSettings httpClientOptions)
        where T : class
    {
        var options = buildOptions(new RestEaseSettingsBuilder()).Build();
        return builder.AddServiceClient<T>(serviceName, options,
            b => b.AddFabio(buildFabioOptions, buildConsulOptions, httpClientOptions));
    }

    public static IGenocsBuilder AddServiceClient<T>(this IGenocsBuilder builder, string serviceName,
        RestEaseSettings options, ConsulSettings consulOptions, FabioSettings fabioOptions,
        HttpClientSettings httpClientOptions)
        where T : class
        => builder.AddServiceClient<T>(serviceName, options,
            b => b.AddFabio(fabioOptions, consulOptions, httpClientOptions));

    private static IGenocsBuilder AddServiceClient<T>(
                                                        this IGenocsBuilder builder,
                                                        string serviceName,
                                                        RestEaseSettings options,
                                                        Action<IGenocsBuilder> registerFabio)
        where T : class
    {
        if (!builder.TryRegister(RegistryName))
        {
            return builder;
        }

        string clientName = typeof(T).ToString();

        switch (options.LoadBalancer?.ToLowerInvariant())
        {
            case "consul":
                builder.AddConsulHttpClient(clientName, serviceName);
                break;
            case "fabio":
                builder.AddFabioHttpClient(clientName, serviceName);
                break;
            default:
                ConfigureDefaultClient(builder.Services, clientName, serviceName, options);
                break;
        }

        ConfigureForwarder<T>(builder.Services, clientName);

        registerFabio(builder);

        return builder;
    }

    private static void ConfigureDefaultClient(
                                                IServiceCollection services,
                                                string clientName,
                                                string serviceName,
                                                RestEaseSettings options)
    {
        services.AddHttpClient(clientName, client =>
        {
            var service = options.Services.SingleOrDefault(s => s.Name.Equals(serviceName, StringComparison.InvariantCultureIgnoreCase));

            if (service is null)
            {
                throw new RestEaseServiceNotFoundException($"RestEase service: '{serviceName}' was not found.", serviceName);
            }

            client.BaseAddress = new UriBuilder
            {
                Scheme = service.Scheme,
                Host = service.Host,
                Port = service.Port
            }.Uri;
        });
    }

    private static void ConfigureForwarder<T>(IServiceCollection services, string clientName)
        where T : class
    {
        services.AddTransient(c => new RestClient(c.GetRequiredService<IHttpClientFactory>().CreateClient(clientName))
        {
            RequestQueryParamSerializer = new QueryParamSerializer()
        }.For<T>());
    }
}