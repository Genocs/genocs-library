using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Core.Demo.WebApi.Infrastructure.Extensions;
using Genocs.Core.Demo.WebApi.Options;
using Genocs.Logging;
using Genocs.Persistence.MongoDb.Extensions;
using Genocs.Secrets.AzureKeyVault;
using Genocs.Tracing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using System.Reflection;
using System.Text.Json.Serialization;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
        .UseLogging()
        .UseAzureKeyVault();

var services = builder.Services;

services
    .AddGenocs(builder.Configuration)
    .AddPrivateKeyJwt()
    .AddOpenTelemetry()
    .AddMongoFast()
    .RegisterMongoRepositories(Assembly.GetExecutingAssembly())
    .AddApplicationServices()
    .Build();

services.AddCors();
services.AddControllers().AddJsonOptions(x =>
{
    // serialize Enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

services.AddHealthChecks();

services.Configure<SecretSettings>(builder.Configuration.GetSection(SecretSettings.Position));

var settings = new SecretSettings();
builder.Configuration.GetSection(SecretSettings.Position).Bind(settings);
services.AddSingleton(settings);

services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(2);
    options.Predicate = check => check.Tags.Contains("ready");
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// Add Masstransit bus configuration
services.AddCustomMassTransit(builder.Configuration);

services.AddOptions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// global cors policy
app.UseCors(x => x
    .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/hc");

app.Run();

Log.CloseAndFlush();
