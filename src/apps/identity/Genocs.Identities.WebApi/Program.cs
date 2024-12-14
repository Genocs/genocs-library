using System.Security.Claims;
using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Discovery.Consul;
using Genocs.Identities.Application;
using Genocs.Identities.Application.Commands;
using Genocs.Identities.Application.Domain.Constants;
using Genocs.Identities.Application.DTO;
using Genocs.Identities.Application.Queries;
using Genocs.Identities.Application.Services;
using Genocs.LoadBalancing.Fabio;
using Genocs.Logging;
using Genocs.Secrets.Vault;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Serilog;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
        .UseLogging();

IGenocsBuilder gnxBuilder = await builder
                                    .AddGenocs()
                                    .AddJwt()
                                    .AddWebApi()
                                    .AddCoreAsync();

gnxBuilder.Build();

//// Override the default authorization policy
//builder.Services.AddAuthorizationBuilder()
//                .AddPolicy("Reader", builder => builder.RequireAssertion(context => context.User.HasClaim(ClaimTypes.Role, "user")))
//                .AddPolicy("Reader2", builder => builder.RequireClaim(ClaimTypes.Role, "user"))
//                .AddPolicy("Reader3", builder => builder.RequireRole(["user"]))
//                .AddPolicy("Reader4", builder => builder.AddRequirements(new AssertionRequirement(context => context.User.IsInRole("user"))))
//                ;

var app = builder.Build();

// app.MapDefaultEndpoints();

app.UseCore();

app.UseDispatcherEndpoints(endpoints => endpoints
                            .Post<SignIn>("sign-in", afterDispatch: (cmd, ctx) =>
                            {
                                var auth = ctx.RequestServices.GetRequiredService<ITokenStorage>().Get(cmd.Id);
                                return ctx.Response.WriteJsonAsync(auth);
                            })
                            .Post<CreateUser>("sign-up", afterDispatch: (cmd, ctx) =>
                            {
                                ctx.Response.Headers.Append("user-id", cmd.UserId.ToString());
                                return Task.CompletedTask;
                            })
                            .Post<CreateAdmin>("onboarding", auth: true, policies: [Policies.AdminOnly], afterDispatch: (cmd, ctx) =>
                            {
                                ctx.Response.Headers.Append("user-id", cmd.UserId.ToString());
                                return Task.CompletedTask;
                            })
                            .Post<RevokeAccessToken>("access-tokens/revoke", auth: true, policies: [Policies.AdminOnly])
                            .Post<UseRefreshToken>("refresh-tokens/use", afterDispatch: (cmd, ctx) =>
                            {
                                var auth = ctx.RequestServices.GetRequiredService<ITokenStorage>().Get(cmd.Id);
                                return ctx.Response.WriteJsonAsync(auth);
                            })
                            .Post<RevokeRefreshToken>("refresh-tokens/revoke", auth: true, policies: [Policies.AdminOnly])
                            .Get<GetUser, UserDetailsDto>("users/{userId:guid}", auth: true)
                            .Get<BrowseUsers, PagedDto<UserDto>>("users", auth: true)
                            .Put<LockUser>("users/{userId:guid}/lock", auth: true, policies: [Policies.AdminOnly])
                            .Put<UnlockUser>("users/{userId:guid}/unlock", auth: true, policies: [Policies.AdminOnly]));

app.Run();

Log.CloseAndFlush();