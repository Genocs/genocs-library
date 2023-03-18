using Convey;
using Convey.WebApi;
using Convey.WebApi.CQRS;
using Genocs.Core.Demo.Users.Application;
using Genocs.Core.Demo.Users.Application.Commands;
using Genocs.Core.Demo.Users.Application.DTO;
using Genocs.Core.Demo.Users.Application.Queries;
using Genocs.Core.Demo.Users.Application.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

//builder.Host
//        .UseLogging()
//        .UseVault();

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.WriteTo.Console();

    string? applicationInsightsConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");

    if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
    {
        lc.WriteTo.ApplicationInsights(new TelemetryConfiguration
        {
            ConnectionString = applicationInsightsConnectionString
        }, TelemetryConverter.Traces);
    }
});

var services = builder.Services;

services.AddConvey()
        .AddWebApi()
        .AddCore()
        .Build();

var app = builder.Build();
app.UseCore();
app.UseDispatcherEndpoints(endpoints => endpoints
                            .Get("", ctx => ctx.GetAppName())
                            .Post<SignIn>("sign-in", afterDispatch: (cmd, ctx) =>
                            {
                                var auth = ctx.RequestServices.GetRequiredService<ITokenStorage>().Get(cmd.Id);
                                return ctx.Response.WriteJsonAsync(auth);
                            })
                            .Post<CreateUser>("sign-up")
                            .Post<RevokeAccessToken>("access-tokens/revoke")
                            .Post<UseRefreshToken>("refresh-tokens/use", afterDispatch: (cmd, ctx) =>
                            {
                                var auth = ctx.RequestServices.GetRequiredService<ITokenStorage>().Get(cmd.Id);
                                return ctx.Response.WriteJsonAsync(auth);
                            })
                            .Post<RevokeRefreshToken>("refresh-tokens/revoke")
                            .Get<GetUser, UserDetailsDto>("users/{userId:guid}")
                            .Get<BrowseUsers, PagedDto<UserDto>>("users")
                            .Put<LockUser>("users/{userId:guid}/lock")
                            .Put<UnlockUser>("users/{userId:guid}/unlock"));

app.Run();

Log.CloseAndFlush();

