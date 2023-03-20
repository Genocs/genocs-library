namespace Genocs.WebApi;

public class WebApiEndpointDefinitions : List<WebApiEndpointDefinition>
{
}

public class WebApiEndpointDefinition
{
    public string Method { get; set; } = default!;
    public string Path { get; set; } = default!;
    public IEnumerable<WebApiEndpointParameter> Parameters { get; set; } = new List<WebApiEndpointParameter>();
    public IEnumerable<WebApiEndpointResponse> Responses { get; set; } = new List<WebApiEndpointResponse>();
}

public class WebApiEndpointParameter
{
    public string? In { get; set; }
    public Type? Type { get; set; }
    public string? Name { get; set; }
    public object? Example { get; set; }
}

public class WebApiEndpointResponse
{
    public Type? Type { get; set; }
    public int StatusCode { get; set; }
    public object? Example { get; set; }
}