using System.Net;

namespace Genocs.WebApi.Exceptions;

/// <summary>
/// Represents an exception response.
/// </summary>
/// <param name="response">The response.</param>
/// <param name="statusCode">The http statu code.</param>
public class ExceptionResponse(object response, HttpStatusCode statusCode)
{
    /// <summary>
    /// Gets the response.
    /// </summary>
    public object Response { get; } = response;

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public HttpStatusCode StatusCode { get; } = statusCode;
}