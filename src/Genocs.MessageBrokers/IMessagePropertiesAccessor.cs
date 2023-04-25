namespace Genocs.MessageBrokers;

public interface IMessagePropertiesAccessor
{
    IMessageProperties MessageProperties { get; set; }
}