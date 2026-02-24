namespace Genocs.Logging.Cqrs;

public sealed class HandlerLogTemplate
{
    public string? Before { get; set; }
    public string? After { get; set; }
    public IReadOnlyDictionary<Type, string>? OnError { get; set; }

    public string? GetExceptionTemplate(Exception ex)
    {
        var exceptionType = ex.GetType();

        if (OnError is null)
        {
            return null;
        }

        return OnError.TryGetValue(exceptionType, out string? template) ? template : null;
    }
}