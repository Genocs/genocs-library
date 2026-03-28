namespace Genocs.Identities.Application.Domain.Exceptions;

/// <summary>
/// Represents the base exception for all domain-related errors in the application.
/// This exception should be used to indicate issues that arise from business logic or domain rules violations.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
public abstract class DomainException(string message) : Exception(message);
