using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Genocs.APIGateway.Framework;

public class UserMiddleware : IMiddleware
{
    private static readonly ISet<string> ValidMethods = new HashSet<string>
    {
        "POST", "PUT", "PATCH"
    };

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var request = context.Request;
        if (!ValidMethods.Contains(request.Method))
        {
            await next(context);
            return;
        }

        if (!request.Headers.ContainsKey("authorization"))
        {
            await next(context);
            return;
        }

        string? path = context.Request.Path.Value;
        if (path is not null && (path.Contains("sign-in") || path.Contains("sign-up")))
        {
            await next(context);
            return;
        }

        var authenticateResult = await context.AuthenticateAsync();
        if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
        {
            // Set the response code to 401.
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        string content;
        context.User = authenticateResult.Principal;
        using (var reader = new StreamReader(request.Body))
        {
            content = await reader.ReadToEndAsync();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            await next(context);
            return;
        }

        var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
        if (payload is null)
        {
            await next(context);
            return;
        }

        payload["userId"] = Guid.Parse(context.User.Identity.Name);
        string json = JsonSerializer.Serialize(payload);
        await using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        context.Request.Body = memoryStream;
        context.Request.ContentLength = json.Length;
        await next(context);
    }
}