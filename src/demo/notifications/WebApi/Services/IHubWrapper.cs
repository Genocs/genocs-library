namespace Genocs.Notifications.WebApi.Services;

public interface IHubWrapper
{
    Task PublishToUserAsync(DefaultIdType userId, string message, object data);
    Task PublishToAllAsync(string message, object data);
}