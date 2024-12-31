using Genocs.Core.Extensions;
using Genocs.WebApi.Exceptions;
using System.Collections.Concurrent;
using System.Net;

namespace Genocs.SignalR.WebApi.Exceptions;

public class ExceptionToResponseMapper : IExceptionToResponseMapper
{
    private static readonly ConcurrentDictionary<Type, string> Codes = new ConcurrentDictionary<Type, string>();

    public ExceptionResponse Map(Exception exception)
        => exception switch
        {
            // DomainException ex => new ExceptionResponse(new { code = GetCode(ex), reason = ex.Message },
            //    HttpStatusCode.BadRequest),
            AppException ex => new ExceptionResponse(new { code = GetCode(ex), reason = ex.Message }, HttpStatusCode.BadRequest),
            _ => new ExceptionResponse(new { code = "error", reason = "There was an error." }, HttpStatusCode.BadRequest)
        };

    private static string? GetCode(Exception exception)
    {
        var type = exception.GetType();
        if (Codes.TryGetValue(type, out string? code))
        {
            return code;
        }

        string? exceptionCode = exception.GetType().Name.Underscore()?.Replace("_exception", string.Empty);
        if (!string.IsNullOrWhiteSpace(exceptionCode))
        {
            Codes.TryAdd(type, exceptionCode);
        }

        return exceptionCode;
    }
}
