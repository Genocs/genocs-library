using System.Security.Claims;
using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.GnxOpenTelemetry;
using Genocs.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Genocs.WebApi;
using Genocs.WebApi.Swagger;
using Genocs.WebApi.Swagger.Docs;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseLogging();

builder.AddGenocs()
    .AddOpenTelemetry()
    .AddJwt("simmetric_jwt")
    .AddWebApi()
    .AddSwaggerDocs()
    .AddWebApiSwaggerDocs()
    .Build();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Override the default authorization policy
builder.Services.AddAuthorizationBuilder()
                .AddPolicy("Reader", builder => builder.RequireAssertion(context => context.User.HasClaim(ClaimTypes.Role, "user")))
                .AddPolicy("Reader2", builder => builder.RequireClaim(ClaimTypes.Role, "user"))
                .AddPolicy("Reader3", builder => builder.RequireRole(["user"]))
                .AddPolicy("Reader4", builder => builder.AddRequirements(new AssertionRequirement(context => context.User.IsInRole("user"))))
                ;

//builder.Services.AddAuthorizationBuilder()
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

// Minimal API with authorization
app.MapGet("/", () => "ok").RequireAuthorization();

// Minimal API with authorization policy
app.MapGet("/onlyreader", () => "ok").RequireAuthorization("Reader");
app.MapGet("/onlyreader2", () => "ok").RequireAuthorization("Reader2");

app.Run();
