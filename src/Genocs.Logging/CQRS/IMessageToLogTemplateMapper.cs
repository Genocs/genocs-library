namespace Genocs.Logging.Cqrs;

/// <summary>
/// Interface used to log messages using a template.
/// </summary>
public interface IMessageToLogTemplateMapper
{
    /// <summary>
    /// Map the message using the template.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="message">The message instance.</param>
    /// <returns>The LogTemplate.</returns>
    HandlerLogTemplate? Map<TMessage>(TMessage message)
        where TMessage : class;
}