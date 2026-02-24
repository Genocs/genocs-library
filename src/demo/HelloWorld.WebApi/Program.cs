using System.Security.Claims;
using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Library.Demo.HelloWorld.WebApi.Services;
using Genocs.Logging;
using Genocs.Saga;
using Genocs.Telemetry;
using Genocs.WebApi;
using Genocs.WebApi.OpenApi;
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
    .AddSwaggerDocs()
    .AddWebApiSwaggerDocs()
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

//services.AddAuthorizationBuilder()
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
    .UseSwaggerDocs();

app.UseHttpsRedirection();

app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Used to validate the access token
// In RealTime
app.UseAccessTokenValidator();

app.MapControllers();

// Minimal API

// welcome endpoint
app.MapGet(string.Empty, () => "welcome");

// free endpoint
app.MapGet("/free", () => "this is a free endpoint");

// protected endpoint
app.MapGet("/protected", () => "this is a protected endpoint")
    .RequireAuthorization()
    .WithTags("Home");
//.WithOpenApi(op =>
//{
//    op.OperationId = "_home_default";
//    op.Summary = "Get a single todo";
//    op.Description = "Returns a todo by its ID.";
//    op.Tags = new[] { new Microsoft.OpenApi.Models.OpenApiTag { Name = "Home" } };
//    return op;
//});

// with authorization policy
app.MapGet("/onlyreader", () => "ok").RequireAuthorization("Reader");
app.MapGet("/onlyreader2", () => "ok").RequireAuthorization("Reader2");

// Saga - Start Transaction endpoint
app.MapPost("/saga/start", async (ISagaTransactionService sagaService) =>
{
    SagaId sagaId = await sagaService.StartTransactionAsync("Start transaction", "Test");
    return Results.Ok(sagaId);
}).WithTags("Saga");

// Saga - Complete Transaction endpoint
app.MapPost("/saga/complete/{sagaId}", async (ISagaTransactionService sagaService, string sagaId) =>
{
    SagaId completedSagaId = await sagaService.CompleteTransactionAsync(sagaId, "Complete transaction", "Test");
    return Results.Ok(completedSagaId);
}).WithTags("Saga");

await app.RunAsync();

await Log.CloseAndFlushAsync();
