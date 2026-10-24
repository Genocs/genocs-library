using System.Diagnostics;
using System.Text;
using Genocs.Logging.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Genocs.Logging.UnitTests.Host;

public class CorrelationContextLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithLargeBaggage_EnforcesEntryLimit()
    {
        var logger = new TestLogger<CorrelationContextLoggingMiddleware>();
        var options = new LoggerOptions
        {
            HttpPayload = new HttpPayloadOptions
            {
                Enabled = false,
                CaptureRequestBody = false,
                CaptureResponseBody = false
            }
        };

        var middleware = new CorrelationContextLoggingMiddleware(logger, options);
        var context = new DefaultHttpContext();

        using var activity = new Activity("request");
        activity.Start();

        for (int index = 0; index < 40; index++)
        {
            activity.AddBaggage($"bg-{index:00}", $"value-{index:00}");
        }

        await middleware.InvokeAsync(context, _ =>
        {
            logger.LogInformation("Inside pipeline");
            return Task.CompletedTask;
        });

        var record = Assert.Single(logger.Records);
        int baggageCount = record.ScopeValues.Keys.Count(key => key.StartsWith("bg-", StringComparison.Ordinal));
        Assert.Equal(32, baggageCount);
    }

    [Fact]
    public async Task InvokeAsync_WithLongBaggageKeyAndValue_TruncatesBoth()
    {
        var logger = new TestLogger<CorrelationContextLoggingMiddleware>();
        var options = new LoggerOptions
        {
            HttpPayload = new HttpPayloadOptions
            {
                Enabled = false,
                CaptureRequestBody = false,
                CaptureResponseBody = false
            }
        };

        var middleware = new CorrelationContextLoggingMiddleware(logger, options);
        var context = new DefaultHttpContext();

        string longKey = new('k', 90);
        string longValue = new('v', 300);

        using var activity = new Activity("request");
        activity.Start();
        activity.AddBaggage(longKey, longValue);

        await middleware.InvokeAsync(context, _ =>
        {
            logger.LogInformation("Inside pipeline");
            return Task.CompletedTask;
        });

        var record = Assert.Single(logger.Records);

        string expectedKey = new('k', 64);
        Assert.True(record.ScopeValues.TryGetValue(expectedKey, out object? value));

        string? capturedValue = value as string;
        Assert.NotNull(capturedValue);
        Assert.Equal(256, capturedValue.Length);
    }

    [Fact]
    public async Task InvokeAsync_WithContentTypeParameters_StillCapturesMatchingPayload()
    {
        var logger = new TestLogger<CorrelationContextLoggingMiddleware>();
        var options = new LoggerOptions
        {
            HttpPayload = new HttpPayloadOptions
            {
                Enabled = true,
                CaptureRequestBody = true,
                CaptureResponseBody = false,
                AllowedContentTypes = ["application/json"],
                MaxBodyLength = 4096
            }
        };

        var middleware = new CorrelationContextLoggingMiddleware(logger, options);

        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json; charset=utf-8";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"name\":\"demo\"}"));

        await middleware.InvokeAsync(context, _ =>
        {
            logger.LogInformation("Inside pipeline");
            return Task.CompletedTask;
        });

        var record = Assert.Single(logger.Records);
        Assert.Equal("{\"name\":\"demo\"}", record.ScopeValues["HttpRequestBody"] as string);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidMaxBodyLength_NormalizesToSafeDefault()
    {
        var logger = new TestLogger<CorrelationContextLoggingMiddleware>();
        var options = new LoggerOptions
        {
            HttpPayload = new HttpPayloadOptions
            {
                Enabled = true,
                CaptureRequestBody = true,
                CaptureResponseBody = false,
                AllowedContentTypes = ["application/json"],
                MaxBodyLength = -10
            }
        };

        var middleware = new CorrelationContextLoggingMiddleware(logger, options);

        string payload = new('x', 5000);
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));

        await middleware.InvokeAsync(context, _ =>
        {
            logger.LogInformation("Inside pipeline");
            return Task.CompletedTask;
        });

        var record = Assert.Single(logger.Records);
        string? captured = record.ScopeValues["HttpRequestBody"] as string;
        Assert.NotNull(captured);
        Assert.Equal(4096, captured.Length);
    }

    [Fact]
    public async Task InvokeAsync_WithResponseCaptureEnabled_DoesNotExposeResponseBodyInInFlightScope()
    {
        var logger = new TestLogger<CorrelationContextLoggingMiddleware>();
        var options = new LoggerOptions
        {
            HttpPayload = new HttpPayloadOptions
            {
                Enabled = true,
                CaptureRequestBody = false,
                CaptureResponseBody = true,
                AllowedContentTypes = ["application/json"],
                MaxBodyLength = 4096
            }
        };

        var middleware = new CorrelationContextLoggingMiddleware(logger, options);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Response.ContentType = "application/json";

        using var activity = new Activity("request");
        activity.Start();

        await middleware.InvokeAsync(context, async httpContext =>
        {
            logger.LogInformation("Inside pipeline");
            await httpContext.Response.WriteAsync("{\"ok\":true}");
        });

        var record = Assert.Single(logger.Records);
        Assert.False(record.ScopeValues.ContainsKey("HttpResponseBody"));
        Assert.Equal("{\"ok\":true}", activity.GetTagItem("http.response.body") as string);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Equal("{\"ok\":true}", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_WithRequestCaptureEnabled_ExposesRequestBodyInPipelineScope()
    {
        var logger = new TestLogger<CorrelationContextLoggingMiddleware>();
        var options = new LoggerOptions
        {
            HttpPayload = new HttpPayloadOptions
            {
                Enabled = true,
                CaptureRequestBody = true,
                CaptureResponseBody = false,
                AllowedContentTypes = ["application/json"],
                MaxBodyLength = 4096
            }
        };

        var middleware = new CorrelationContextLoggingMiddleware(logger, options);

        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"name\":\"demo\"}"));

        using var activity = new Activity("request");
        activity.Start();

        await middleware.InvokeAsync(context, _ =>
        {
            logger.LogInformation("Inside pipeline");
            return Task.CompletedTask;
        });

        var record = Assert.Single(logger.Records);
        Assert.Equal("{\"name\":\"demo\"}", record.ScopeValues["HttpRequestBody"] as string);
        Assert.Equal("{\"name\":\"demo\"}", activity.GetTagItem("http.request.body") as string);
    }

    [Fact]
    public async Task InvokeAsync_WhenPayloadCaptureDisabled_DoesNotReadRequestBodyOrReplaceResponseStream()
    {
        var logger = new TestLogger<CorrelationContextLoggingMiddleware>();
        var options = new LoggerOptions
        {
            HttpPayload = new HttpPayloadOptions
            {
                Enabled = false,
                CaptureRequestBody = true,
                CaptureResponseBody = true,
                AllowedContentTypes = ["application/json"],
                MaxBodyLength = 4096
            }
        };

        var middleware = new CorrelationContextLoggingMiddleware(logger, options);

        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Body = new ThrowOnReadStream();

        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        await middleware.InvokeAsync(context, httpContext =>
        {
            Assert.Same(responseStream, httpContext.Response.Body);
            logger.LogInformation("Inside pipeline");
            return httpContext.Response.WriteAsync("ok");
        });

        Assert.Empty(logger.Records.Single().ScopeValues.Where(pair => pair.Key is "HttpRequestBody" or "HttpResponseBody"));
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseCaptureDisabled_DoesNotReplaceResponseStream()
    {
        var logger = new TestLogger<CorrelationContextLoggingMiddleware>();
        var options = new LoggerOptions
        {
            HttpPayload = new HttpPayloadOptions
            {
                Enabled = true,
                CaptureRequestBody = false,
                CaptureResponseBody = false,
                AllowedContentTypes = ["application/json"],
                MaxBodyLength = 4096
            }
        };

        var middleware = new CorrelationContextLoggingMiddleware(logger, options);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        await middleware.InvokeAsync(context, httpContext =>
        {
            Assert.Same(responseStream, httpContext.Response.Body);
            return httpContext.Response.WriteAsync("ok");
        });
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        private readonly LoggerExternalScopeProvider _scopeProvider = new();

        public List<LogRecord> Records { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
            => _scopeProvider.Push(state);

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var scopeValues = new Dictionary<string, object?>();
            _scopeProvider.ForEachScope((scope, values) =>
            {
                if (scope is null)
                {
                    return;
                }

                AddScopeValues(values, scope);
            }, scopeValues);

            Records.Add(new LogRecord(logLevel, formatter(state, exception), scopeValues));
        }

        private static void AddScopeValues(IDictionary<string, object?> target, object scope)
        {
            if (scope is IEnumerable<KeyValuePair<string, object?>> nullablePairs)
            {
                foreach (var pair in nullablePairs)
                {
                    target[pair.Key] = pair.Value;
                }

                return;
            }

            if (scope is IEnumerable<KeyValuePair<string, object>> pairs)
            {
                foreach (var pair in pairs)
                {
                    target[pair.Key] = pair.Value;
                }
            }
        }
    }

    private sealed record LogRecord(LogLevel Level, string Message, IReadOnlyDictionary<string, object?> ScopeValues);

    private sealed class ThrowOnReadStream : MemoryStream
    {
        public override int Read(byte[] buffer, int offset, int count)
            => throw new InvalidOperationException("Request body should not be read when payload capture is disabled.");

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Request body should not be read when payload capture is disabled.");
    }
}
