using System.Net;

namespace Genocs.WebApi.Exceptions;

public class ExceptionResponse(object response, HttpStatusCode statusCode)
{
    public object Response { get; } = response;
    public HttpStatusCode StatusCode { get; } = statusCode;
}