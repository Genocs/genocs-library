using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Open.Serialization.Json;
using System.Net;

namespace Genocs.WebApi.Exceptions;

/// <summary>
/// Middleware for handling exceptions in the ASP.NET Core pipeline.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ErrorHandlerMiddleware"/> class.
/// </remarks>
/// <param name="exceptionToResponseMapper">The exception to Response Mapper.</param>
/// <param name="jsonSerializer">The Json initializer.</param>
/// <param name="logger">The logger.</param>
internal sealed class ErrorHandlerMiddleware(
                                IExceptionToResponseMapper exceptionToResponseMapper,
                                IJsonSerializer jsonSerializer,
                                ILogger<ErrorHandlerMiddleware> logger) : IMiddleware
{
    private readonly IExceptionToResponseMapper _exceptionToResponseMapper = exceptionToResponseMapper;
    private readonly IJsonSerializer _jsonSerializer = jsonSerializer;
    private readonly ILogger<ErrorHandlerMiddleware> _logger = logger;

    /// <summary>
    /// Invokes the middleware to handle exceptions in the ASP.NET Core pipeline.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="next">the next step into for the pipeline.</param>
    /// <returns>the Task.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);
            await HandleErrorAsync(context, exception);
        }
    }

    /// <summary>
    /// Handles the error by mapping the exception to an HTTP response and writing it to the response body.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="next">the next step into for the pipeline.</param>
    /// <returns>the Task.</returns>
    private async Task HandleErrorAsync(HttpContext context, Exception exception)
    {
        var exceptionResponse = _exceptionToResponseMapper.Map(exception);
        context.Response.StatusCode = (int)(exceptionResponse?.StatusCode ?? HttpStatusCode.BadRequest);
        object? response = exceptionResponse?.Response;
        if (response is null)
        {
            await context.Response.WriteAsync(string.Empty);
            return;
        }

        context.Response.ContentType = "application/json";
        await _jsonSerializer.SerializeAsync(context.Response.Body, exceptionResponse!.Response);
    }
}