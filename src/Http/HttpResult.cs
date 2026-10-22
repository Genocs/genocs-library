namespace Genocs.Http;

/// <summary>
/// Combines a deserialized value (when the status is successful and deserialization ran) with the raw <see cref="HttpResponseMessage"/>.
/// For non-success responses, <see cref="Result"/> is typically the default value and <see cref="Response"/> carries the status and body.
/// </summary>
public class HttpResult<T>(T? result, HttpResponseMessage response)
{
    public T? Result { get; } = result;
    public HttpResponseMessage Response { get; } = response;
    public bool HasResult => Result is not null;
}