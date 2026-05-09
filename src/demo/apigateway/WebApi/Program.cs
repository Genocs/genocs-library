using Genocs.APIGateway.WebApi.Configurations;
using Genocs.APIGateway.WebApi.Framework;
using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Logging;
using Genocs.Messaging.RabbitMQ;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.Secrets.HashicorpKeyVault;
using Genocs.Security;
using Genocs.Telemetry;
using Genocs.WebApi;
using Serilog;
using Yarp.ReverseProxy.Forwarder;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
       .UseLogging()
       .UseVault();

builder.Services.AddScoped<LogContextMiddleware>();
builder.Services.AddScoped<UserMiddleware>();
builder.Services.AddScoped<MessagingMiddleware>();
builder.Services.AddSingleton<CorrelationIdFactory>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ICorrelationContextBuilder, CorrelationContextBuilder>();
builder.Services.AddSingleton<RouteMatcher>();
builder.Services.Configure<MessagingOptions>(builder.Configuration.GetSection(MessagingOptions.Position));
builder.Services.AddSingleton<IForwarderHttpClientFactory, CustomForwarderHttpClientFactory>();

IGenocsBuilder gnxBuilder = builder.Services
                                  .AddGenocs(builder.Configuration)
                                  .AddTelemetry()
                                  .AddMongoWithRegistration()
                                  .AddJwt(sectionName: "azureAdB2C");

await gnxBuilder.AddRabbitMQAsync();

gnxBuilder.AddSecurity()
          .AddWebApi();

builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(cors =>
{
    cors.AddPolicy("cors", x =>
    {
        x.AllowAnyOrigin()
         .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS", "HEAD")
         .WithHeaders("Content-Type", "Authorization", "x-correlation-id");
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();
gnxBuilder.Build(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<LogContextMiddleware>();
app.UseCors("cors");
app.UseGenocs();
app.UsePrometheus();
app.UseAccessTokenValidator();
app.UseAuthentication();
app.UseRabbitMQ();
app.UseMiddleware<UserMiddleware>();
app.UseMiddleware<MessagingMiddleware>();
app.UseRouting();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapPrometheus();
app.MapReverseProxy();

await app.RunAsync();

Log.CloseAndFlush();
