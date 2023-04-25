using Genocs.HTTP.RestEase.Options;

namespace Genocs.HTTP.RestEase.Builders;

internal sealed class RestEaseSettingsBuilder : IRestEaseSettingsBuilder
{
    private readonly RestEaseSettings _options = new();
    private readonly List<RestEaseSettings.Service> _services = new();

    public IRestEaseSettingsBuilder WithLoadBalancer(string loadBalancer)
    {
        _options.LoadBalancer = loadBalancer;
        return this;
    }

    public IRestEaseSettingsBuilder WithService(Func<IRestEaseServiceBuilder, IRestEaseServiceBuilder> buildService)
    {
        var service = buildService(new RestEaseServiceBuilder()).Build();
        _services.Add(service);
        return this;
    }

    public RestEaseSettings Build()
    {
        _options.Services = _services;
        return _options;
    }

    private class RestEaseServiceBuilder : IRestEaseServiceBuilder
    {
        private readonly RestEaseSettings.Service _service = new();

        public IRestEaseServiceBuilder WithName(string name)
        {
            _service.Name = name;
            return this;
        }

        public IRestEaseServiceBuilder WithScheme(string scheme)
        {
            _service.Scheme = scheme;
            return this;
        }

        public IRestEaseServiceBuilder WithHost(string host)
        {
            _service.Host = host;
            return this;
        }

        public IRestEaseServiceBuilder WithPort(int port)
        {
            _service.Port = port;
            return this;
        }

        public RestEaseSettings.Service Build() => _service;
    }
}