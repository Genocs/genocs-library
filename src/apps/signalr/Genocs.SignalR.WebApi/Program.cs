using Genocs.Secrets.Vault;
using Genocs.Logging;
using Genocs.Core.Builders;
using Genocs.Tracing.Jaeger;
using Genocs.Tracing.Jaeger.RabbitMQ;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Genocs.WebApi.Security;
using Genocs.WebApi.Swagger;
using Genocs.WebApi.Swagger.Docs;
using Serilog;
using Serilog.Events;

using Genocs.SignalR.WebApi.Framework;
using Genocs.SignalR.WebApi.Hubs;
using Microsoft.OpenApi.Models;


//using DShop.Common.Logging;
//using DShop.Common.Vault;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Host
        .UseLogging()
        .UseVault();

var services = builder.Services;

services.AddGenocs();
services.AddMvc();
services.AddSignalR();


var app = builder.Build();

app.UseGenocs();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Cors policy
app.UseCors("AllowAll");


app.UseEndpoints(endpoints =>
{
    //endpoints.MapRazorPages();
    endpoints.MapControllers();
    // Configure SignalR
    endpoints.MapHub<GenocsHub>("/notificationHub");

});


app.Run();

Log.CloseAndFlush();





//public class Program
//{
//    public static void Main(string[] args)
//    {
//        CreateWebHostBuilder(args).Build().Run();
//    }

//    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
//        WebHost.CreateDefaultBuilder(args)
//            .UseStartup<Startup>()
//            .UseLogging()
//            .UseVault()
//            .UseLockbox()
//            .UseAppMetrics();
//}