using Genocs.Core.Builders;
using Genocs.Identities.Application;
using Genocs.Identities.Application.Commands;
using Genocs.Identities.Application.DTO;
using Genocs.Identities.Application.Queries;
using Genocs.Identities.Application.Services;
using Genocs.Logging;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
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

builder.Host
        .UseLogging();
//        .UseVault();


var services = builder.Services;

services.AddGenocs(builder.Configuration)
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
