namespace Genocs.Common.Builders;

public class ServiceId : IServiceId
{
    public string Id { get; } = $"{Guid.NewGuid():N}";
}