namespace Genocs.Common.Validations;

/// <summary>
/// Represents one validation failure for a specific request member.
/// </summary>
/// <param name="PropertyName">The logical member name associated with the failure.</param>
/// <param name="ErrorCode">A machine-readable validation code.</param>
/// <param name="ErrorMessage">A human-readable validation message.</param>
public sealed record ValidationError(string PropertyName, string ErrorCode, string ErrorMessage);