using System.Net;
using Genocs.WebApi.Exceptions;

namespace Genocs.Products.WebApi;

public class ExceptionToResponseMapper : IExceptionToResponseMapper
{
    public ExceptionResponse Map(Exception exception)
        => new(new { code = "error", message = exception.Message }, HttpStatusCode.BadRequest);
}