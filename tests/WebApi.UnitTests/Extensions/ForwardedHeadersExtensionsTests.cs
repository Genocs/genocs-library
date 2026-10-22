using Genocs.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Net;
using Xunit;

namespace Genocs.WebApi.UnitTests.Extensions;

public class ForwardedHeadersExtensionsTests
{
    [Fact]
    public async Task UseAllForwardedHeaders_DefaultBehavior_DoesNotTrustUnlistedForwarder()
    {
        using var server = CreateServer(resetKnownNetworksAndProxies: false);
        using var client = server.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Forwarded-Proto", "https");

        using HttpResponseMessage response = await client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("http", body);
    }

    [Fact]
    public async Task UseAllForwardedHeaders_WhenOptedIn_TrustsForwardedHeaders()
    {
        using var server = CreateServer(resetKnownNetworksAndProxies: true);
        using var client = server.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Forwarded-Proto", "https");

        using HttpResponseMessage response = await client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("https", body);
    }

    private static TestServer CreateServer(bool resetKnownNetworksAndProxies)
    {
        var webHostBuilder = new WebHostBuilder()
            .Configure(app =>
            {
                app.Use(async (context, next) =>
                {
                    context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.10");
                    await next();
                });

                app.UseAllForwardedHeaders(resetKnownNetworksAndProxies);

                app.Run(async context =>
                {
                    await context.Response.WriteAsync(context.Request.Scheme);
                });
            });

        return new TestServer(webHostBuilder);
    }
}
