using System.Security.Claims;
using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Library.Demo.Services;
using Genocs.Library.Demo.WebApi.Features;
using Genocs.Library.Demo.WebApi.Services;
using Genocs.Logging;
using Genocs.Saga;
using Genocs.Telemetry;
using Genocs.WebApi;
using Genocs.WebApi.OpenApi.Docs;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Serilog;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseLogging();

builder
    .AddGenocs()
    .AddJwt("simmetric_jwt")
    .AddTelemetry()
    .AddWebApi()
    .AddOpenApiDocs()
    .Build();

// Add services to the container.
var services = builder.Services;

services
    .AddSaga()
    .AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    })
    .AddControllers();

// Registrazione del servizio Saga
services.AddScoped<ISagaTransactionService, SagaTransactionService>();

// Override the default authorization policy
services.AddAuthorizationBuilder()
                .AddPolicy("Reader", builder => builder.RequireAssertion(context => context.User.HasClaim(ClaimTypes.Role, "user")))
                .AddPolicy("Reader2", builder => builder.RequireClaim(ClaimTypes.Role, "user"))
                .AddPolicy("Reader3", builder => builder.RequireRole(["user"]))
                .AddPolicy("Reader4", builder => builder.AddRequirements(new AssertionRequirement(context => context.User.IsInRole("user"))));

// services.AddAuthorizationBuilder()
//                    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
//                    .RequireAuthenticatedUser()
//                    .Build())
//                        .AddPolicy(Policies.UserOnly, policy => policy.RequireAssertion(context
//                            => context.User.HasClaim(ClaimTypes.Role, Roles.User)))
//                        .AddPolicy(Policies.AdminOnly, policy => policy.RequireAssertion(context
//                            => context.User.HasClaim(ClaimTypes.Role, Roles.Admin)))
//                        .AddPolicy(Policies.UserOrAdmin, policy => policy.RequireAssertion(context
//                            => context.User.HasClaim(ClaimTypes.Role, Roles.User)
//                                || context.User.HasClaim(ClaimTypes.Role, Roles.Admin)));

var app = builder.Build();

app.UseGenocs()
    .UseOpenApiDocs();

app.UseHttpsRedirection();

app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Used to validate the access token
// In RealTime
app.UseAccessTokenValidator();

app.MapControllers();
app.MapFeatures();

await app.RunAsync();

await Log.CloseAndFlushAsync();
