using Genocs.Core.Builders;
using Genocs.GnxOpenTelemetry;
using Genocs.Logging;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseLogging();

builder.AddGenocs()
    .AddOpenTelemetry()
    .Build();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
