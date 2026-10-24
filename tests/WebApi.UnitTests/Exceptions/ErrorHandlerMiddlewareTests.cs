using Genocs.WebApi.Exceptions;
using Genocs.Core.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace Genocs.WebApi.UnitTests.Exceptions;

public class ErrorHandlerMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_UsesMapperStatusCodeAndSerializesResponse_WhenMapperReturnsResponse()
    {
        using ServiceProvider provider = CreateProvider<MappedExceptionToResponseMapper>();
        IMiddleware middleware = ResolveErrorHandlerMiddleware(provider);
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(httpContext, _ => throw new InvalidOperationException("mapped"));

        Assert.Equal((int)HttpStatusCode.Conflict, httpContext.Response.StatusCode);
        Assert.Equal("application/json", httpContext.Response.ContentType);
        Assert.NotEmpty(ReadBody(httpContext.Response.Body));
    }

    [Fact]
    public async Task InvokeAsync_UsesInternalServerErrorFallback_WhenMapperReturnsNull()
    {
        using ServiceProvider provider = CreateProvider<NullExceptionToResponseMapper>();
        IMiddleware middleware = ResolveErrorHandlerMiddleware(provider);
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(httpContext, _ => throw new InvalidOperationException("unmapped"));

        Assert.Equal((int)HttpStatusCode.InternalServerError, httpContext.Response.StatusCode);
        Assert.Null(httpContext.Response.ContentType);
        Assert.Equal(string.Empty, ReadBody(httpContext.Response.Body));
    }

    private static ServiceProvider CreateProvider<TMapper>()
        where TMapper : class, IExceptionToResponseMapper
    {
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        IGenocsBuilder builder = services
            .AddGenocs(configuration)
            .AddWebApi()
            .AddErrorHandler<TMapper>();

        _ = builder;

        return services.BuildServiceProvider();
    }

    private static IMiddleware ResolveErrorHandlerMiddleware(ServiceProvider provider)
    {
        Type middlewareType = typeof(Genocs.WebApi.Extensions).Assembly
            .GetType("Genocs.WebApi.Exceptions.ErrorHandlerMiddleware", throwOnError: true)!;

        return (IMiddleware)provider.GetRequiredService(middlewareType);
    }

    private static string ReadBody(Stream body)
    {
        body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(body);
        return reader.ReadToEnd();
    }

    private sealed class MappedExceptionToResponseMapper : IExceptionToResponseMapper
    {
        public ExceptionResponse? Map(Exception exception)
            => new(new { message = exception.Message }, HttpStatusCode.Conflict);
    }

    private sealed class NullExceptionToResponseMapper : IExceptionToResponseMapper
    {
        public ExceptionResponse? Map(Exception exception) => null;
    }
}
