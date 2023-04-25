using Genocs.HTTP.RestEase.Options;

namespace Genocs.HTTP.RestEase;

public interface IRestEaseSettingsBuilder
{
    IRestEaseSettingsBuilder WithLoadBalancer(string loadBalancer);
    IRestEaseSettingsBuilder WithService(Func<IRestEaseServiceBuilder, IRestEaseServiceBuilder> buildService);
    RestEaseSettings Build();
}