using Genocs.HTTP.RestEase.Configurations;

namespace Genocs.HTTP.RestEase;

public interface IRestEaseServiceBuilder
{
    IRestEaseServiceBuilder WithName(string name);
    IRestEaseServiceBuilder WithScheme(string scheme);
    IRestEaseServiceBuilder WithHost(string host);
    IRestEaseServiceBuilder WithPort(int port);
    RestEaseOptions.Service Build();
}