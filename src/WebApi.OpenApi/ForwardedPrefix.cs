namespace Genocs.WebApi.OpenApi;

/// <summary>
/// Accepts a single path prefix from <c>X-Forwarded-Prefix</c>.
/// </summary>
internal static class ForwardedPrefix
{
    public const string HeaderName = "X-Forwarded-Prefix";

    public static bool TryRead(string? value, out string prefix)
    {
        prefix = string.Empty;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (!value.StartsWith('/'))
        {
            return false;
        }

        if (value.Contains("//", StringComparison.Ordinal)
            || value.Contains('?')
            || value.Contains('#')
            || value.Contains(':'))
        {
            return false;
        }

        if (value.Length > 1 && value.EndsWith('/'))
        {
            return false;
        }

        prefix = value;
        return true;
    }
}
