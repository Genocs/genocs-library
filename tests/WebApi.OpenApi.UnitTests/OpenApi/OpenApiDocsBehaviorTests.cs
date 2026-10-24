using Genocs.Core.Builders;
using Genocs.WebApi;
using Genocs.WebApi.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;

namespace Genocs.WebApi.OpenApi.UnitTests.OpenApi;

public class OpenApiDocsBehaviorTests
{
    [Fact]
    public void UseOpenApiDocs_DoesNotThrow_WhenOpenApiWasNotRegistered()
    {
        var builder = WebApplication.CreateBuilder();

        using var app = builder.Build();

        Exception? exception = Record.Exception(() => app.UseOpenApiDocs());

        Assert.Null(exception);
    }

    [Fact]
    public void UseOpenApiDocs_DoesNotThrow_WhenOpenApiIsDisabled()
    {
        using var app = CreateApp(enabled: false);

        Exception? exception = Record.Exception(() => app.UseOpenApiDocs());

        Assert.Null(exception);
        Assert.Null(app.Services.GetService<ISwaggerProvider>());
    }

    [Fact]
    public void UseOpenApiDocs_DoesNotThrow_WhenOpenApiIsEnabled()
    {
        using var app = CreateApp(enabled: true);

        Exception? exception = Record.Exception(() => app.UseOpenApiDocs());

        Assert.Null(exception);
        Assert.NotNull(app.Services.GetService<ISwaggerProvider>());
    }

    [Fact]
    public void DocumentFilter_UnsupportedMethods_AreIgnoredWithoutThrowing()
    {
        using var app = CreateApp(enabled: true);

        OpenApiDocument document = GenerateDocument(app, definitions =>
        {
            definitions.Add(new WebApiEndpointDefinition
            {
                Method = "PATCH",
                Path = "orders/patch",
                Parameters = [],
                Responses = []
            });
        });

        Assert.True(document.Paths.ContainsKey("/orders/patch"));
        IOpenApiPathItem pathItem = document.Paths["/orders/patch"];
        if (pathItem.Operations is not null)
        {
            Assert.Empty(pathItem.Operations);
        }
    }

    [Fact]
    public void QueryContracts_AreEmittedAsQueryParameters_WithoutRequestBody()
    {
        using var app = CreateApp(enabled: true);

        OpenApiDocument document = GenerateDocument(app, definitions =>
        {
            definitions.Add(new WebApiEndpointDefinition
            {
                Method = HttpMethods.Get,
                Path = "orders",
                Parameters =
                [
                    new WebApiEndpointParameter
                    {
                        In = "query",
                        Name = "request",
                        Type = typeof(GetOrdersQuery),
                        Example = new GetOrdersQuery()
                    }
                ],
                Responses =
                [
                    new WebApiEndpointResponse
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Type = typeof(GetOrdersResult),
                        Example = new GetOrdersResult()
                    }
                ]
            });
        });

        IOpenApiPathItem pathItem = document.Paths["/orders"];
        Assert.NotNull(pathItem.Operations);
        OpenApiOperation operation = pathItem.Operations[HttpMethod.Get];

        Assert.Null(operation.RequestBody);
        Assert.NotNull(operation.Parameters);
        Assert.NotEmpty(operation.Parameters);
        Assert.Contains(operation.Parameters, parameter =>
            parameter is OpenApiParameter openApiParameter
            && openApiParameter.In == ParameterLocation.Query
            && openApiParameter.Name == "request");
    }

    [Fact]
    public void SchemaGeneration_UsesModelSchemas_ForRequestAndResponseContracts()
    {
        using var app = CreateApp(enabled: true);

        OpenApiDocument document = GenerateDocument(app, definitions =>
        {
            definitions.Add(new WebApiEndpointDefinition
            {
                Method = HttpMethods.Post,
                Path = "orders",
                Parameters =
                [
                    new WebApiEndpointParameter
                    {
                        In = "body",
                        Name = "request",
                        Type = typeof(CreateOrderRequest),
                        Example = new CreateOrderRequest()
                    }
                ],
                Responses =
                [
                    new WebApiEndpointResponse
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Type = typeof(CreateOrderResponse),
                        Example = new CreateOrderResponse()
                    }
                ]
            });
        });

        IOpenApiPathItem pathItem = document.Paths["/orders"];
        Assert.NotNull(pathItem.Operations);
        OpenApiOperation operation = pathItem.Operations[HttpMethod.Post];

        Assert.NotNull(operation.RequestBody);
        Assert.NotNull(operation.Responses);
        Assert.True(operation.Responses.ContainsKey(StatusCodes.Status200OK.ToString()));
        Assert.NotNull(document.Components);
        Assert.NotNull(document.Components.Schemas);
        Assert.True(document.Components.Schemas.ContainsKey(nameof(CreateOrderRequest)));
        Assert.True(document.Components.Schemas.ContainsKey(nameof(CreateOrderResponse)));
    }

    [Fact]
    public void IncludeSecurity_RegistersBearerScheme_AndGlobalRequirement()
    {
        using var app = CreateApp(enabled: true, includeSecurity: true);

        OpenApiDocument document = GenerateDocument(app);

        Assert.NotNull(document.Components);
        Assert.NotNull(document.Components.SecuritySchemes);
        Assert.True(document.Components.SecuritySchemes.ContainsKey("Bearer"));

        IOpenApiSecurityScheme scheme = document.Components.SecuritySchemes["Bearer"];
        Assert.Equal(SecuritySchemeType.Http, scheme.Type);
        Assert.Equal("bearer", scheme.Scheme);
        Assert.Equal("JWT", scheme.BearerFormat);

        Assert.NotNull(document.Security);
        Assert.NotEmpty(document.Security);
    }

    private static WebApplication CreateApp(bool enabled, bool includeSecurity = false)
    {
        var builder = WebApplication.CreateBuilder();

        _ = builder
            .AddGenocs()
            .AddWebApi()
            .AddOpenApiDocs(options => options
                .Enable(enabled)
                .ReDocEnable(false)
                .WithName("v1")
                .WithTitle("OpenApi Test API")
                .WithVersion("1.0.0")
                .WithDescription("OpenApi package tests")
                .WithRoutePrefix("swagger")
                .IncludeSecurity(includeSecurity));

        return builder.Build();
    }

    private static OpenApiDocument GenerateDocument(WebApplication app, Action<WebApiEndpointDefinitions>? configureDefinitions = null)
    {
        WebApiEndpointDefinitions definitions = app.Services.GetRequiredService<WebApiEndpointDefinitions>();
        configureDefinitions?.Invoke(definitions);

        ISwaggerProvider swaggerProvider = app.Services.GetRequiredService<ISwaggerProvider>();
        return swaggerProvider.GetSwagger("v1");
    }

    private sealed class GetOrdersQuery
    {
        public Guid CustomerId { get; init; }
        public int Page { get; init; }
    }

    private sealed class GetOrdersResult
    {
        public int Count { get; init; }
    }

    private sealed class CreateOrderRequest
    {
        public Guid CustomerId { get; init; }
        public string ProductCode { get; init; } = string.Empty;
    }

    private sealed class CreateOrderResponse
    {
        public Guid OrderId { get; init; }
        public string Status { get; init; } = string.Empty;
    }
}
