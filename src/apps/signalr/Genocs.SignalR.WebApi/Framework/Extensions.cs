using Genocs.Core.Builders;
using Genocs.SignalR.WebApi.Configurations;

namespace Genocs.SignalR.WebApi.Framework;

public static class Extensions
{
    public static string ToUserGroup(this Guid userId)
        => $"users:{userId}";

    public static IGenocsBuilder AddSignalR(this IGenocsBuilder builder)
    {
        var options = builder.Configuration.GetOptions<SignalROptions>("signalr");

        if (options is not null)
        {
            builder.Services.AddSingleton(options);
        }

        return builder;
    }

    public static IGenocsBuilder UseSignalR(this IGenocsBuilder builder)
    {
        var options = builder.GetOptions<SignalROptions>("signalr");

        if (options is not null)
        {
            builder.Services.AddSingleton(options);
        }

        return builder;
    }
}