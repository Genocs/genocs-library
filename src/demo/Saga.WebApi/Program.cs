using Genocs.Core.Builders;
using Genocs.Library.Demo.Saga.WebApi.Extensions;
using Genocs.Logging;
using Genocs.Saga;
using Serilog;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseLogging();

builder
    .AddGenocs()
    .Build();

var services = builder.Services;

services.AddSaga();

var app = builder.Build();
app.ConfigureSaga();

await app.RunAsync();

await Log.CloseAndFlushAsync();