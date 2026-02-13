using Genocs.Core.Collections.Extensions;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Genocs.Core.Extensions;

/// <summary>
/// Extension methods for String class.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Adds a char to end of given string if it does not ends with the char.
    /// </summary>
    public static string? EnsureEndsWith(this string? str, char c)
    {
        return EnsureEndsWith(str, c, StringComparison.Ordinal);
    }

    /// <summary>
    /// Adds a char to end of given string if it does not ends with the char.
    /// </summary>
    public static string? EnsureEndsWith(this string? str, char c, StringComparison comparisonType)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.EndsWith(c.ToString(), comparisonType))
        {
            return str;
        }

        return str + c;
    }

    /// <summary>
    /// Adds a char to end of given string if it does not ends with the char.
    /// </summary>
    public static string? EnsureEndsWith(this string? str, char c, bool ignoreCase, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.EndsWith(c.ToString(culture), ignoreCase, culture))
        {
            return str;
        }

        return str + c;
    }

    /// <summary>
    /// Adds a char to beginning of given string if it does not starts with the char.
    /// </summary>
    public static string? EnsureStartsWith(this string? str, char c)
    {
        return EnsureStartsWith(str, c, StringComparison.Ordinal);
    }

    /// <summary>
    /// Adds a char to beginning of given string if it does not starts with the char.
    /// </summary>
    public static string? EnsureStartsWith(this string? str, char c, StringComparison comparisonType)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.StartsWith(c.ToString(), comparisonType))
        {
            return str;
        }

        return c + str;
    }

    /// <summary>
    /// Adds a char to beginning of given string if it does not starts with the char.
    /// </summary>
    public static string? EnsureStartsWith(this string? str, char c, bool ignoreCase, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.StartsWith(c.ToString(culture), ignoreCase, culture))
        {
            return str;
        }

        return c + str;
    }

    /// <summary>
    /// Gets a substring of a string from beginning of the string.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length.</exception>
    public static string? Left(this string? str, int len)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.Length < len)
        {
            throw new ArgumentException("len argument can not be bigger than given string's length!");
        }

        return str[..len];
    }

    /// <summary>
    /// Converts line endings in the string to <see cref="Environment.NewLine"/>.
    /// </summary>
    public static string? NormalizeLineEndings(this string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        return str.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
    }

    /// <summary>
    /// Gets index of nth occurrence of a char in a string.
    /// </summary>
    /// <param name="str">source string to be searched.</param>
    /// <param name="c">Char to search in <see cref="string"/>.</param>
    /// <param name="n">Count of the occurrence.</param>
    public static int NthIndexOf(this string str, char c, int n)
    {
        if (string.IsNullOrEmpty(str)) throw new ArgumentException("String can not be null or empty!", nameof(str));

        int count = 0;
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] != c)
            {
                continue;
            }

            if ((++count) == n)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Removes first occurrence of the given postfixes from end of the given string.
    /// Ordering is important. If one of the postFixes is matched, others will not be tested.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="postFixes">one or more postfix.</param>
    /// <returns>Modified string or the same string if it has not any of given postfixes.</returns>
    public static string? RemovePostFix(this string? str, params string[] postFixes)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (postFixes.IsNullOrEmpty())
        {
            return str;
        }

        foreach (string postFix in postFixes)
        {
            if (str.EndsWith(postFix))
            {
                return str.Left(str.Length - postFix.Length);
            }
        }

        return str;
    }

    /// <summary>
    /// Removes first occurrence of the given prefixes from beginning of the given string.
    /// Ordering is important. If one of the preFixes is matched, others will not be tested.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="preFixes">one or more prefix.</param>
    /// <returns>Modified string or the same string if it has not any of given prefixes.</returns>
    public static string? RemovePreFix(this string? str, params string[] preFixes)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (preFixes.IsNullOrEmpty())
        {
            return str;
        }

        foreach (string preFix in preFixes)
        {
            if (str.StartsWith(preFix))
            {
                return str.Right(str.Length - preFix.Length);
            }
        }

        return str;
    }

    /// <summary>
    /// Gets a substring of a string from end of the string.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length.</exception>
    public static string? Right(this string? str, int len)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.Length < len)
        {
            throw new ArgumentException("len argument can not be bigger than given string's length!");
        }

        return str.Substring(str.Length - len, len);
    }

    /// <summary>
    /// Uses string.Split method to split given string by given separator.
    /// </summary>
    public static string[] Split(this string str, string separator)
    {
        return str.Split(new[] { separator }, StringSplitOptions.None);
    }

    /// <summary>
    /// Uses string.Split method to split given string by given separator.
    /// </summary>
    public static string[] Split(this string str, string separator, StringSplitOptions options)
    {
        return str.Split(new[] { separator }, options);
    }

    /// <summary>
    /// Uses string.Split method to split given string by <see cref="Environment.NewLine"/>.
    /// </summary>
    public static string[] SplitToLines(this string str)
    {
        return str.Split(Environment.NewLine);
    }

    /// <summary>
    /// Uses string.Split method to split given string by <see cref="Environment.NewLine"/>.
    /// </summary>
    public static string[] SplitToLines(this string str, StringSplitOptions options)
    {
        return str.Split(Environment.NewLine, options);
    }

    /// <summary>
    /// Converts PascalCase string to camelCase string.
    /// </summary>
    /// <param name="str">String to convert.</param>
    /// <param name="invariantCulture">Invariant culture.</param>
    /// <returns>camelCase of the string.</returns>
    public static string? ToCamelCase(this string? str, bool invariantCulture = true)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.Length == 1)
        {
            return invariantCulture ? str.ToLowerInvariant() : str.ToLower();
        }

        return (invariantCulture ? char.ToLowerInvariant(str[0]) : char.ToLower(str[0])) + str.Substring(1);
    }

    /// <summary>
    /// Converts PascalCase string to camelCase string in specified culture.
    /// </summary>
    /// <param name="str">String to convert.</param>
    /// <param name="culture">An object that supplies culture-specific casing rules.</param>
    /// <returns>camelCase of the string.</returns>
    public static string? ToCamelCase(this string? str, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.Length == 1)
        {
            return str.ToLower(culture);
        }

        return char.ToLower(str[0], culture) + str[1..];
    }

    /// <summary>
    /// Converts given PascalCase/camelCase string to sentence (by splitting words by space).
    /// Example: "ThisIsSampleSentence" is converted to "This is a sample sentence".
    /// </summary>
    /// <param name="str">String to convert.</param>
    /// <param name="invariantCulture">Invariant culture.</param>
    public static string? ToSentenceCase(this string? str, bool invariantCulture = false)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        return MyRegex().Replace(str, m => m.Value[0] + " " + (invariantCulture ? char.ToLowerInvariant(m.Value[1]) : char.ToLower(m.Value[1])));
    }

    /// <summary>
    /// Converts given PascalCase/camelCase string to sentence (by splitting words by space).
    /// Example: "ThisIsSampleSentence" is converted to "This is a sample sentence".
    /// </summary>
    /// <param name="str">String to convert.</param>
    /// <param name="culture">An object that supplies culture-specific casing rules.</param>
    public static string? ToSentenceCase(this string? str, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        return MyRegex().Replace(str, m => m.Value[0] + " " + char.ToLower(m.Value[1], culture));
    }

    /// <summary>
    /// Converts string to enum value.
    /// </summary>
    /// <typeparam name="T">Type of enum.</typeparam>
    /// <param name="str">String value to convert.</param>
    /// <returns>Returns enum object.</returns>
    public static T ToEnum<T>(this string? str)
        where T : struct
    {
        if (string.IsNullOrEmpty(str)) throw new ArgumentException("String can not be null or empty!", nameof(str));

        return (T)Enum.Parse(typeof(T), str);
    }

    /// <summary>
    /// Converts string to enum value.
    /// </summary>
    /// <typeparam name="T">Type of enum.</typeparam>
    /// <param name="str">String value to convert.</param>
    /// <param name="ignoreCase">Ignore case.</param>
    /// <returns>Returns enum object.</returns>
    public static T ToEnum<T>(this string? str, bool ignoreCase)
        where T : struct
    {
        if (string.IsNullOrEmpty(str)) throw new ArgumentException("String can not be null or empty!", nameof(str));

        return (T)Enum.Parse(typeof(T), str, ignoreCase);
    }

    public static string ToMd5(this string str)
    {
        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(str);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        var sb = new StringBuilder();
        foreach (byte hashByte in hashBytes)
        {
            sb.Append(hashByte.ToString("X2"));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts camelCase string to PascalCase string.
    /// </summary>
    /// <param name="str">String to convert.</param>
    /// <param name="invariantCulture">Invariant culture.</param>
    /// <returns>PascalCase of the string.</returns>
    public static string? ToPascalCase(this string? str, bool invariantCulture = true)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.Length == 1)
        {
            return invariantCulture ? str.ToUpperInvariant() : str.ToUpper();
        }

        return (invariantCulture ? char.ToUpperInvariant(str[0]) : char.ToUpper(str[0])) + str.Substring(1);
    }

    /// <summary>
    /// Converts camelCase string to PascalCase string in specified culture.
    /// </summary>
    /// <param name="str">String to convert.</param>
    /// <param name="culture">An object that supplies culture-specific casing rules.</param>
    /// <returns>PascalCase of the string.</returns>
    public static string? ToPascalCase(this string? str, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.Length == 1)
        {
            return str.ToUpper(culture);
        }

        return char.ToUpper(str[0], culture) + str.Substring(1);
    }

    /// <summary>
    /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
    /// </summary>
    /// <returns>Truncated string if it is too long, otherwise the entire string.</returns>
    public static string? Truncate(this string? str, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str.Length <= maxLength)
        {
            return str;
        }

        return str.Left(maxLength);
    }

    /// <summary>
    /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
    /// It adds a "..." postfix to end of the string if it's truncated.
    /// Returning string can not be longer than maxLength.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null.</exception>
    public static string? TruncateWithPostfix(this string? str, int maxLength)
    {
        return TruncateWithPostfix(str, maxLength, "...");
    }

    /// <summary>
    /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
    /// It adds given <paramref name="postfix"/> to end of the string if it's truncated.
    /// Returning string can not be longer than maxLength.
    /// </summary>
    public static string? TruncateWithPostfix(this string? str, int maxLength, string postfix)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (maxLength == 0)
        {
            return string.Empty;
        }

        if (str.Length <= maxLength)
        {
            return str;
        }

        if (maxLength <= postfix.Length)
        {
            return postfix.Left(maxLength);
        }

        return str.Left(maxLength - postfix.Length) + postfix;
    }

    /// <summary>
    /// Helper method. It's used to convert a string to snake case.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <returns>The output result.</returns>
    public static string? Underscore(this string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()))
            .ToLowerInvariant();
    }

    [GeneratedRegex("[a-z][A-Z]")]
    private static partial Regex MyRegex();
}