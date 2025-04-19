// using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Core.Demo.WebApi.Configurations;
using Genocs.Core.Demo.WebApi.Infrastructure.Extensions;
using Genocs.Logging;
using Genocs.Persistence.MongoDb.Extensions;
using Genocs.Secrets.AzureKeyVault;
using Genocs.Tracing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Genocs.Auth;
using System.Reflection;
using System.Text.Json.Serialization;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
        .UseAzureKeyVault()
        .UseLogging();

builder
    .AddGenocs()
    .AddJwt()
    .AddOpenTelemetry()
    .AddMongo()
    .AddMongoFast()
    .RegisterMongoRepositories(Assembly.GetExecutingAssembly())
    .AddApplicationServices()
    .Build();

var services = builder.Services;

services.AddCors();
services.AddControllers().AddJsonOptions(x =>
{
    // serialize Enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

services.Configure<SecretOptions>(builder.Configuration.GetSection(SecretOptions.Position));

SecretOptions settings = builder.Configuration.GetOptions<SecretOptions>(SecretOptions.Position);
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

app.UseGenocs();

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

// Use it only if you need to authenticate with Firebase
// app.UseFirebaseAuthentication();

app.MapDefaultEndpoints();

app.MapControllers();

app.Run();

Log.CloseAndFlush();
