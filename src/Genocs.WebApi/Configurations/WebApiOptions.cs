namespace Genocs.WebApi.Configurations;

/// <summary>
/// The WebApiOptions definition.
/// </summary>
public class WebApiOptions
{
    /// <summary>
    /// It defines whether the request body should be bound from the route or from body.
    /// </summary>
    public bool BindRequestFromRoute { get; set; }
}