namespace Genocs.Messaging;

public interface IMessagePropertiesAccessor
{
    IMessageProperties? MessageProperties { get; set; }
}