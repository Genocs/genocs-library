using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Open.Serialization.Json;

namespace Genocs.WebApi.Formatters;

/// <summary>
/// JsonOutputFormatter.
/// </summary>
/// <param name="serializer">The json serializer.</param>
internal class JsonOutputFormatter(IJsonSerializer serializer) : IOutputFormatter
{
    private readonly IJsonSerializer _serializer = serializer;

    public bool CanWriteResult(OutputFormatterCanWriteContext context)
        => true;

    public async Task WriteAsync(OutputFormatterWriteContext context)
    {
        if (context.Object is null)
        {
            return;
        }

        context.HttpContext.Response.ContentType = "application/json";
        if (context.Object is string json)
        {
            await context.HttpContext.Response.WriteAsync(json);
            return;
        }

        await _serializer.SerializeAsync(context.HttpContext.Response.Body, context.Object);
    }
}