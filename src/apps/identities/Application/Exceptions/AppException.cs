namespace Genocs.Identities.Application.Exceptions;

/// <summary>
/// Represents the base exception for all application-related errors in the application.
/// This exception should be used to indicate issues that arise from application logic or rules violations.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
public abstract class AppException(string message) : Exception(message);