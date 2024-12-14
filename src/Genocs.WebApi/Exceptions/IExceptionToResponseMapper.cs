namespace Genocs.WebApi.Exceptions;

public interface IExceptionToResponseMapper
{
    ExceptionResponse? Map(Exception exception);
}