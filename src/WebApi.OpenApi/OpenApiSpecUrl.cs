namespace Genocs.WebApi.OpenApi;

/// <summary>
/// Builds the Swagger UI and ReDoc spec URL from route prefix, document name, and document path mode.
/// </summary>
internal static class OpenApiSpecUrl
{
    public const string Absolute = "absolute";
    public const string Relative = "relative";

    public static string Build(string? routePrefix, string? name, string? documentPath)
    {
        string mode = string.IsNullOrWhiteSpace(documentPath) ? Absolute : documentPath;
        EnsureValid(mode);

        string documentName = name ?? string.Empty;
        if (mode == Relative)
        {
            return $"{documentName}/swagger.json";
        }

        string prefix = string.IsNullOrWhiteSpace(routePrefix) ? string.Empty : routePrefix;
        return $"/{prefix}/{documentName}/swagger.json".Replace("//", "/", StringComparison.Ordinal);
    }

    public static void EnsureValid(string? documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath) || documentPath is Absolute or Relative)
        {
            return;
        }

        throw new ArgumentException(
            "openapi.documentPath must be 'absolute' or 'relative'.",
            nameof(documentPath));
    }
}
