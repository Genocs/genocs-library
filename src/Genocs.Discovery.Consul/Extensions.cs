using Genocs.Core.Builders;
using Genocs.Discovery.Consul.Builders;
using Genocs.Discovery.Consul.Configurations;
using Genocs.Discovery.Consul.Http;
using Genocs.Discovery.Consul.MessageHandlers;
using Genocs.Discovery.Consul.Models;
using Genocs.Discovery.Consul.Services;
using Genocs.HTTP;
using Genocs.HTTP.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Discovery.Consul;

public static class Extensions
{
    private const string DefaultInterval = "5s";
    private const string SectionName = "consul";
    private const string RegistryName = "discovery.consul";

    public static IGenocsBuilder AddConsul(
                                            this IGenocsBuilder builder,
                                            string sectionName = SectionName,
                                            string httpClientSectionName = "httpClient")
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = SectionName;
        }

        var consulOptions = builder.GetOptions<ConsulOptions>(sectionName);
        var httpClientOptions = builder.GetOptions<HttpClientOptions>(httpClientSectionName);
        return builder.AddConsul(consulOptions, httpClientOptions);
    }

    public static IGenocsBuilder AddConsul(
                                            this IGenocsBuilder builder,
                                            Func<IConsulOptionsBuilder, IConsulOptionsBuilder> buildOptions,
                                            HttpClientOptions httpClientOptions)
    {
        var options = buildOptions(new ConsulOptionsBuilder()).Build();
        return builder.AddConsul(options, httpClientOptions);
    }

    public static IGenocsBuilder AddConsul(
                                            this IGenocsBuilder builder,
                                            ConsulOptions options,
                                            HttpClientOptions httpClientOptions)
    {
        builder.Services.AddSingleton(options);
        if (!options.Enabled || !builder.TryRegister(RegistryName))
        {
            return builder;
        }

        if (httpClientOptions.Type?.ToLowerInvariant() == "consul")
        {
            builder.Services.AddTransient<ConsulServiceDiscoveryMessageHandler>();
            builder.Services.AddHttpClient<IConsulHttpClient, ConsulHttpClient>("consul-http")
                .AddHttpMessageHandler<ConsulServiceDiscoveryMessageHandler>();
            builder.RemoveHttpClient();
            builder.Services.AddHttpClient<IHttpClient, ConsulHttpClient>("consul")
                .AddHttpMessageHandler<ConsulServiceDiscoveryMessageHandler>();
        }

        builder.Services.AddTransient<IConsulServicesRegistry, ConsulServicesRegistry>();
        var registration = builder.CreateConsulAgentRegistration(options);
        if (registration is null)
        {
            return builder;
        }

        builder.Services.AddSingleton(registration);

        return builder;
    }

    public static void AddConsulHttpClient(this IGenocsBuilder builder, string clientName, string? serviceName)
        => builder.Services.AddHttpClient<IHttpClient, ConsulHttpClient>(clientName)
            .AddHttpMessageHandler(c => new ConsulServiceDiscoveryMessageHandler(
                c.GetRequiredService<IConsulServicesRegistry>(),
                c.GetRequiredService<ConsulOptions>(), serviceName, true));

    private static ServiceRegistration? CreateConsulAgentRegistration(this IGenocsBuilder builder, ConsulOptions options)
    {
        bool enabled = options.Enabled;
        string? consulEnabled = Environment.GetEnvironmentVariable("CONSUL_ENABLED")?.ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(consulEnabled))
        {
            enabled = consulEnabled is "true" or "1";
        }

        if (!enabled)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(options.Address))
        {
            throw new ArgumentException("Consul address can not be empty.", nameof(options.PingEndpoint));
        }

        builder.Services.AddHttpClient<IConsulService, ConsulService>(c => c.BaseAddress = new Uri(options.Url));

        if (builder.Services.All(x => x.ServiceType != typeof(ConsulHostedService)))
        {
            builder.Services.AddHostedService<ConsulHostedService>();
        }

        string serviceId;
        using (var serviceProvider = builder.Services.BuildServiceProvider())
        {
            serviceId = serviceProvider.GetRequiredService<IServiceId>().Id;
        }

        var registration = new ServiceRegistration
        {
            Name = options.Service,
            Id = $"{options.Service}:{serviceId}",
            Address = options.Address,
            Port = options.Port,
            Tags = options.Tags,
            Meta = options.Meta,
            EnableTagOverride = options.EnableTagOverride,
            Connect = options.Connect?.Enabled == true ? new Connect() : null
        };

        if (!options.PingEnabled)
        {
            return registration;
        }

        string pingEndpoint = string.IsNullOrWhiteSpace(options.PingEndpoint) ? string.Empty :
            options.PingEndpoint.StartsWith("/") ? options.PingEndpoint : $"/{options.PingEndpoint}";
        if (pingEndpoint.EndsWith("/"))
        {
            pingEndpoint = pingEndpoint.Substring(0, pingEndpoint.Length - 1);
        }

        string scheme = options.Address.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)
            ? string.Empty
            : "http://";
        var check = new ServiceCheck
        {
            Interval = ParseTime(options.PingInterval),
            DeregisterCriticalServiceAfter = ParseTime(options.RemoveAfterInterval),
            Http = $"{scheme}{options.Address}{(options.Port > 0 ? $":{options.Port}" : string.Empty)}" +
                   $"{pingEndpoint}"
        };
        registration.Checks = new[] { check };

        return registration;
    }

    private static string ParseTime(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DefaultInterval;
        }

        return int.TryParse(value, out int number) ? $"{number}s" : value;
    }
}