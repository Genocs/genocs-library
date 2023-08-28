using Genocs.Core.Builders;
using Genocs.Discovery.Consul;
using Genocs.Discovery.Consul.Models;
using Genocs.Discovery.Consul.Options;
using Genocs.HTTP;
using Genocs.HTTP.Options;
using Genocs.LoadBalancing.Fabio.Builders;
using Genocs.LoadBalancing.Fabio.Http;
using Genocs.LoadBalancing.Fabio.MessageHandlers;
using Genocs.LoadBalancing.Fabio.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.LoadBalancing.Fabio;

public static class Extensions
{
    private const string RegistryName = "loadBalancing.fabio";

    public static IGenocsBuilder AddFabio(this IGenocsBuilder builder, string sectionName = FabioSettings.Position,
        string consulSectionName = "consul", string httpClientSectionName = "httpClient")
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = FabioSettings.Position;
        }

        var fabioOptions = builder.GetOptions<FabioSettings>(sectionName);
        var consulOptions = builder.GetOptions<ConsulSettings>(consulSectionName);
        var httpClientOptions = builder.GetOptions<HttpClientSettings>(httpClientSectionName);
        return builder.AddFabio(fabioOptions, httpClientOptions,
            b => b.AddConsul(consulOptions, httpClientOptions));
    }

    public static IGenocsBuilder AddFabio(this IGenocsBuilder builder,
        Func<IFabioOptionsBuilder, IFabioOptionsBuilder> buildOptions,
        Func<IConsulOptionsBuilder, IConsulOptionsBuilder> buildConsulOptions,
        HttpClientSettings httpClientOptions)
    {
        var fabioOptions = buildOptions(new FabioOptionsBuilder()).Build();
        return builder.AddFabio(fabioOptions, httpClientOptions,
            b => b.AddConsul(buildConsulOptions, httpClientOptions));
    }

    public static IGenocsBuilder AddFabio(this IGenocsBuilder builder, FabioSettings fabioOptions,
        ConsulSettings consulOptions, HttpClientSettings httpClientOptions)
        => builder.AddFabio(fabioOptions, httpClientOptions, b => b.AddConsul(consulOptions, httpClientOptions));

    private static IGenocsBuilder AddFabio(this IGenocsBuilder builder, FabioSettings fabioOptions,
        HttpClientSettings httpClientOptions, Action<IGenocsBuilder> registerConsul)
    {
        registerConsul(builder);
        builder.Services.AddSingleton(fabioOptions);

        if (!fabioOptions.Enabled || !builder.TryRegister(RegistryName))
        {
            return builder;
        }

        if (httpClientOptions.Type?.ToLowerInvariant() == "fabio")
        {
            builder.Services.AddTransient<FabioMessageHandler>();
            builder.Services.AddHttpClient<IFabioHttpClient, FabioHttpClient>("fabio-http")
                .AddHttpMessageHandler<FabioMessageHandler>();


            builder.RemoveHttpClient();
            builder.Services.AddHttpClient<IHttpClient, FabioHttpClient>("fabio")
                .AddHttpMessageHandler<FabioMessageHandler>();
        }

        using var serviceProvider = builder.Services.BuildServiceProvider();
        var registration = serviceProvider.GetRequiredService<ServiceRegistration>();
        var tags = GetFabioTags(registration.Name, fabioOptions.Service);
        if (registration.Tags is null)
        {
            registration.Tags = tags;
        }
        else
        {
            registration.Tags.AddRange(tags);
        }

        builder.Services.UpdateConsulRegistration(registration);

        return builder;
    }

    public static void AddFabioHttpClient(this IGenocsBuilder builder, string clientName, string serviceName)
        => builder.Services.AddHttpClient<IHttpClient, FabioHttpClient>(clientName)
            .AddHttpMessageHandler(c => new FabioMessageHandler(c.GetRequiredService<FabioSettings>(), serviceName));

    private static void UpdateConsulRegistration(this IServiceCollection services,
        ServiceRegistration registration)
    {
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(ServiceRegistration));
        services.Remove(serviceDescriptor);
        services.AddSingleton(registration);
    }

    private static List<string> GetFabioTags(string consulService, string fabioService)
    {
        var service = (string.IsNullOrWhiteSpace(fabioService) ? consulService : fabioService)
            .ToLowerInvariant();

        return new List<string> { $"urlprefix-/{service} strip=/{service}" };
    }
}