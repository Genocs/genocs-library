using Genocs.LoadBalancing.Fabio.Options;

namespace Genocs.LoadBalancing.Fabio;

public interface IFabioOptionsBuilder
{
    IFabioOptionsBuilder Enable(bool enabled);
    IFabioOptionsBuilder WithUrl(string url);
    IFabioOptionsBuilder WithService(string service);
    FabioSettings Build();
}