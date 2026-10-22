namespace Genocs.Common.CQRS.Events;

/// <summary>
/// Provides a structured code convention for rejected events.
/// </summary>
public static class RejectionCode
{
    /// <summary>
    /// Creates a structured rejection code using the convention: category.subject.reason.
    /// </summary>
    /// <param name="subject">Business operation or subject that failed.</param>
    /// <param name="reason">Reason category for the rejection.</param>
    /// <param name="category">Top-level classification category.</param>
    /// <returns>A normalized structured code.</returns>
    public static string Create(string subject, string reason = "failed", string category = "rejection")
    {
        return $"{NormalizeSegment(category)}.{NormalizeSegment(subject)}.{NormalizeSegment(reason)}";
    }

    /// <summary>
    /// Attempts to parse a structured rejection code.
    /// </summary>
    /// <param name="code">Code to parse.</param>
    /// <param name="parts">Parsed code parts when valid.</param>
    /// <returns>True when parsing succeeded; otherwise false.</returns>
    public static bool TryParse(string code, out RejectionCodeParts parts)
    {
        parts = default;
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        string[] segments = code.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length != 3)
        {
            return false;
        }

        if (segments.Any(string.IsNullOrWhiteSpace))
        {
            return false;
        }

        parts = new RejectionCodeParts(segments[0], segments[1], segments[2]);
        return true;
    }

    private static string NormalizeSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
        {
            return "unknown";
        }

        char[] buffer = segment.Trim().ToLowerInvariant().ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            if (!char.IsLetterOrDigit(buffer[i]))
            {
                buffer[i] = '_';
            }
        }

        string normalized = new(buffer);
        while (normalized.Contains("__", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("__", "_", StringComparison.Ordinal);
        }

        return normalized.Trim('_');
    }
}

/// <summary>
/// Parsed representation of a structured rejection code.
/// </summary>
/// <param name="Category">Top-level category segment.</param>
/// <param name="Subject">Operation or subject segment.</param>
/// <param name="Reason">Reason segment.</param>
public readonly record struct RejectionCodeParts(string Category, string Subject, string Reason);
