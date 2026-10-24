using Genocs.Core.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Xunit;

namespace Genocs.Http.UnitTests;

public sealed class ExtensionsRegistrationTests
{
    private const string LoggingScopeHandlerTypeName = "Genocs.Http.GenocsLoggingScopeHttpMessageHandler";

    private sealed class StubHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            ArgumentNullException.ThrowIfNull(next);
            return next;
        }
    }

    private sealed class CountingCorrelationContextFactory : ICorrelationContextFactory
    {
        public static int InstancesCreated;

        public CountingCorrelationContextFactory()
            => InstancesCreated++;

        public string? Create() => "context";
    }

    private sealed class CountingCorrelationIdFactory : ICorrelationIdFactory
    {
        public static int InstancesCreated;

        public CountingCorrelationIdFactory()
            => InstancesCreated++;

        public string? Create() => "id";
    }

    private static IGenocsBuilder CreateBuilder(IServiceCollection services, IConfiguration? configuration = null)
    {
        configuration ??= new ConfigurationBuilder().Build();
        return GenocsBuilder.Create(services, configuration);
    }

    private static IConfiguration CreateRequestMaskingEnabledConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["httpClient:requestMasking:enabled"] = "true",
                ["httpClient:requestMasking:maskTemplate"] = "*****",
                ["httpClient:requestMasking:urlParts:0"] = "token"
            })
            .Build();

    private static bool ContainsHandlerTypeName(HttpMessageHandler handler, string handlerTypeName)
    {
        HttpMessageHandler? current = handler;
        while (current is not null)
        {
            if (string.Equals(current.GetType().FullName, handlerTypeName, StringComparison.Ordinal))
            {
                return true;
            }

            current = (current as DelegatingHandler)?.InnerHandler;
        }

        return false;
    }

    [Fact]
    public void AddHttpClient_does_not_instantiate_custom_correlation_factories_during_registration()
    {
        CountingCorrelationContextFactory.InstancesCreated = 0;
        CountingCorrelationIdFactory.InstancesCreated = 0;

        var services = new ServiceCollection();
        services.AddSingleton<ICorrelationContextFactory, CountingCorrelationContextFactory>();
        services.AddSingleton<ICorrelationIdFactory, CountingCorrelationIdFactory>();

        var builder = CreateBuilder(services);

        builder.AddHttpClient();

        Assert.Equal(0, CountingCorrelationContextFactory.InstancesCreated);
        Assert.Equal(0, CountingCorrelationIdFactory.InstancesCreated);

        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(ICorrelationContextFactory)
                && descriptor.ImplementationType?.FullName == "Genocs.Http.EmptyCorrelationContextFactory");
        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(ICorrelationIdFactory)
                && descriptor.ImplementationType?.FullName == "Genocs.Http.EmptyCorrelationIdFactory");
    }

    [Fact]
    public void AddHttpClient_registers_empty_correlation_factories_when_custom_ones_are_missing()
    {
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        builder.AddHttpClient();

        using var provider = services.BuildServiceProvider();

        Assert.Equal("Genocs.Http.EmptyCorrelationContextFactory", provider.GetRequiredService<ICorrelationContextFactory>().GetType().FullName);
        Assert.Equal("Genocs.Http.EmptyCorrelationIdFactory", provider.GetRequiredService<ICorrelationIdFactory>().GetType().FullName);
    }

    [Fact]
    public void AddHttpClient_with_request_masking_enabled_does_not_replace_existing_global_filters()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHttpMessageHandlerBuilderFilter, StubHttpMessageHandlerBuilderFilter>();

        var builder = CreateBuilder(services, CreateRequestMaskingEnabledConfiguration());

        builder.AddHttpClient();

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IHttpMessageHandlerBuilderFilter)
                && descriptor.ImplementationType == typeof(StubHttpMessageHandlerBuilderFilter));

        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(IHttpMessageHandlerBuilderFilter)
                && string.Equals(descriptor.ImplementationType?.FullName, "Genocs.Http.GenocsHttpLoggingFilter", StringComparison.Ordinal));
    }

    [Fact]
    public void AddHttpClient_with_request_masking_enabled_scopes_masking_handler_to_target_client()
    {
        const string maskedClientName = "masked-client";
        const string otherClientName = "other-client";

        var services = new ServiceCollection();
        services.AddHttpClient(otherClientName);

        var builder = CreateBuilder(services, CreateRequestMaskingEnabledConfiguration());
        builder.AddHttpClient(clientName: maskedClientName);

        using var provider = services.BuildServiceProvider();
        var handlerFactory = provider.GetRequiredService<IHttpMessageHandlerFactory>();

        using var maskedClientHandler = handlerFactory.CreateHandler(maskedClientName);
        using var otherClientHandler = handlerFactory.CreateHandler(otherClientName);

        Assert.True(ContainsHandlerTypeName(maskedClientHandler, LoggingScopeHandlerTypeName));
        Assert.False(ContainsHandlerTypeName(otherClientHandler, LoggingScopeHandlerTypeName));
    }
}
