using Genocs.HTTP.RestEase.Options;

namespace Genocs.HTTP.RestEase;

public interface IRestEaseServiceBuilder
{
    IRestEaseServiceBuilder WithName(string name);
    IRestEaseServiceBuilder WithScheme(string scheme);
    IRestEaseServiceBuilder WithHost(string host);
    IRestEaseServiceBuilder WithPort(int port);
    RestEaseSettings.Service Build();
}