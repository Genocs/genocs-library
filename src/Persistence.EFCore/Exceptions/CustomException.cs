using System.Net;

namespace Genocs.Persistence.EFCore.Exceptions;

public class CustomException : Exception
{
    public CustomException(string message, List<string>? errors = default, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : this(message, errors, statusCode, null)
    {
    }

    public CustomException(string message, List<string>? errors, HttpStatusCode statusCode, Exception? innerException)
        : base(message, innerException)
    {
        ErrorMessages = errors;
        StatusCode = statusCode;
    }

    public List<string>? ErrorMessages { get; }

    public HttpStatusCode StatusCode { get; }
}