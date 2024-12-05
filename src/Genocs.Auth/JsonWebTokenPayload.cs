namespace Genocs.Auth;

/// <summary>
/// The JsonWebToken payload.
/// </summary>
public class JsonWebTokenPayload
{
    /// <summary>
    /// The subject.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// The Identity Role.
    /// </summary>
    public IEnumerable<string>? Roles { get; set; }

    /// <summary>
    /// The expiration ticks.
    /// </summary>
    public long Expires { get; set; }

    /// <summary>
    /// List of claims.
    /// </summary>
    public IDictionary<string, IEnumerable<string>>? Claims { get; set; }
}