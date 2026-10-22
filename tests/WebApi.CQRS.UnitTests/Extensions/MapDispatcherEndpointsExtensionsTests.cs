using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Queries;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.WebApi.CQRS.UnitTests.Extensions;

public class MapDispatcherEndpointsExtensionsTests
{
    [Fact]
    public void MapDispatcherEndpoints_MapsRoutes_AndUpdatesEndpointDefinitions()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<WebApiEndpointDefinitions>();

        using var app = builder.Build();

        app.MapDispatcherEndpoints(endpoints => endpoints
            .Get<TestQuery, TestResult>("orders/{id}")
            .Post<TestCommand>("orders"));

        var endpointDefinitions = app.Services.GetRequiredService<WebApiEndpointDefinitions>();
        Assert.Equal(2, endpointDefinitions.Count);
        Assert.Contains(endpointDefinitions, d => d.Method == HttpMethods.Get && d.Path == "orders/{id}");
        Assert.Contains(endpointDefinitions, d => d.Method == HttpMethods.Post && d.Path == "orders");
    }

    private sealed class TestCommand : ICommand
    {
    }

    private sealed class TestQuery : IQuery<TestResult>
    {
    }

    private sealed class TestResult
    {
    }
}
