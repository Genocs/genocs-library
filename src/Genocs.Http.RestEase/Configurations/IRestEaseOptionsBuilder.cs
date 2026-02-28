namespace Genocs.Http.RestEase.Configurations;

public interface IRestEaseOptionsBuilder
{
    IRestEaseOptionsBuilder WithLoadBalancer(string loadBalancer);
    IRestEaseOptionsBuilder WithService(Func<IRestEaseServiceBuilder, IRestEaseServiceBuilder> buildService);
    RestEaseOptions Build();
}