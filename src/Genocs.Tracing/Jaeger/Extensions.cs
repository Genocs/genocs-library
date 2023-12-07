using Genocs.Core.Builders;
using Genocs.Tracing.Jaeger.Options;
using Genocs.Tracing.Jaeger.Tracers;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;

namespace Genocs.Tracing.Jaeger;

/// <summary>
/// The Open Tracing.
/// </summary>
public static class Extensions
{
    private static int _initialized;
    private const string RegistryName = "tracing.jaeger";

    /// <summary>
    /// Add Jaeger Tracer.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IGenocsBuilder AddJaeger(this IGenocsBuilder builder, string sectionName = JaegerSettings.Position)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return builder;
        }

        var options = builder.GetOptions<JaegerSettings>(sectionName);

        builder.Services.AddSingleton(options);

        if (!options.Enabled)
        {
            var defaultTracer = GenocsDefaultTracer.Create();
            builder.Services.AddSingleton(defaultTracer);
            return builder;
        }

        if (!builder.TryRegister(RegistryName))
        {
            return builder;
        }

        builder.Services.AddSingleton<ITracer>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            int maxPacketSize = options.MaxPacketSize <= 0 ? 64967 : options.MaxPacketSize;
            string? senderType = string.IsNullOrWhiteSpace(options.Sender) ? "udp" : options.Sender?.ToLowerInvariant();
            ISender sender = senderType switch
            {
                "http" => BuildHttpSender(options.HttpSender),
                "udp" => new UdpSender(options.UdpHost, options.UdpPort, maxPacketSize),
                _ => throw new Exception($"Invalid Jaeger sender type: '{senderType}'.")
            };

            var reporter = new RemoteReporter.Builder()
                .WithSender(sender)
                .WithLoggerFactory(loggerFactory)
                .Build();

            var sampler = GetSampler(options);

            var tracer = new Tracer.Builder(options.ServiceName)
                .WithLoggerFactory(loggerFactory)
                .WithReporter(reporter)
                .WithSampler(sampler)
                .Build();

            GlobalTracer.Register(tracer);

            return tracer;
        });

        return builder;
    }

    private static HttpSender BuildHttpSender(JaegerSettings.HttpSenderSettings? options)
    {
        if (options is null)
        {
            throw new Exception("Missing Jaeger HTTP sender options.");
        }

        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            throw new Exception("Missing Jaeger HTTP sender endpoint.");
        }

        var builder = new HttpSender.Builder(options.Endpoint);
        if (options.MaxPacketSize > 0)
        {
            builder = builder.WithMaxPacketSize(options.MaxPacketSize);
        }

        if (!string.IsNullOrWhiteSpace(options.AuthToken))
        {
            builder = builder.WithAuth(options.AuthToken);
        }

        if (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password))
        {
            builder = builder.WithAuth(options.Username, options.Password);
        }

        if (!string.IsNullOrWhiteSpace(options.UserAgent))
        {
            builder = builder.WithUserAgent(options.Username);
        }

        return builder.Build();
    }

    public static IApplicationBuilder UseJaeger(this IApplicationBuilder app)
    {
        // Could be extended with some additional middleware
        using var scope = app.ApplicationServices.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<JaegerSettings>();

        return app;
    }

    private static ISampler GetSampler(JaegerSettings options)
    {
        return options.Sampler switch
        {
            "const" => new ConstSampler(true),
            "rate" => new RateLimitingSampler(options.MaxTracesPerSecond),
            "probabilistic" => new ProbabilisticSampler(options.SamplingRate),
            _ => new ConstSampler(true),
        };
    }
}