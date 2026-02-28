namespace Genocs.WebApi.Exceptions;

/// <summary>
/// Interface for mapping exceptions to Http responses.
/// </summary>
public interface IExceptionToResponseMapper
{
    /// <summary>
    /// Maps an exception to an Http response.
    /// </summary>
    /// <param name="exception">The original Exception.</param>
    /// <returns>The Exception response.</returns>
    ExceptionResponse? Map(Exception exception);
}