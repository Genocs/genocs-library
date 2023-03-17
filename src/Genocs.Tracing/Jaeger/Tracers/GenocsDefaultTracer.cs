using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using OpenTracing;
using System.Reflection;

namespace Genocs.Tracing.Jaeger.Tracers;

internal sealed class GenocsDefaultTracer
{
    public static ITracer Create()
        => new Tracer.Builder(Assembly.GetEntryAssembly().FullName)
            .WithReporter(new NoopReporter())
            .WithSampler(new ConstSampler(false))
            .Build();
}