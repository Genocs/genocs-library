using System.Diagnostics;
using Genocs.Messaging.Outbox.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Genocs.Messaging.Outbox.Processors;

internal sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBusPublisher _publisher;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _interval;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly OutboxType _type = OutboxType.Sequential;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, IBusPublisher publisher, OutboxOptions options, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!_options.Enabled)
        {
            _logger.LogInformation("Outbox is disabled.");
            return;
        }

        if (_options.IntervalMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                _options.IntervalMilliseconds,
                "Outbox interval must be greater than 0.");
        }

        _interval = TimeSpan.FromMilliseconds(_options.IntervalMilliseconds);

        if (!string.IsNullOrWhiteSpace(_options.Type))
        {
            if (!Enum.TryParse<OutboxType>(_options.Type, true, out var outboxType))
            {
                throw new ArgumentException(
                    $"Invalid outbox type: '{_options.Type}', valid types: '{OutboxType.Sequential}', '{OutboxType.Parallel}'.",
                    nameof(options));
            }

            _type = outboxType;
        }

        _logger.LogInformation(
            "Outbox is enabled, type: '{Type}', message processing every {Interval} ms.",
            _type,
            _options.IntervalMilliseconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await ProcessMessagesAsync(stoppingToken);

        using var timer = new PeriodicTimer(_interval);

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessMessagesAsync(stoppingToken);
        }
    }

    private async Task ProcessMessagesAsync(CancellationToken stoppingToken)
    {
        if (!await _lock.WaitAsync(0, stoppingToken))
        {
            _logger.LogTrace("Skipping outbox execution because the previous cycle is still running.");
            return;
        }

        try
        {
            await SendOutboxMessagesAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SendOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        string jobId = Guid.NewGuid().ToString("N");
        _logger.LogTrace("Started processing outbox messages... [job id: '{JobId}']", jobId);

        var stopwatch = Stopwatch.StartNew();

        using var scope = _scopeFactory.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IMessageOutboxAccessor>();
        var messages = await outbox.GetUnsentAsync();

        _logger.LogTrace("Found {Count} unsent messages in outbox [job ID: '{JobId}'].", messages.Count, jobId);

        if (messages.Count == 0)
        {
            _logger.LogTrace("No messages to be processed in outbox [job ID: '{JobId}'].", jobId);
            return;
        }

        foreach (var message in messages.OrderBy(m => m.SentAt))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (message.Message is null)
            {
                _logger.LogWarning(
                    "Skipping outbox message with ID: '{MessageId}' because deserialized payload is null.",
                    message.Id);

                if (_type == OutboxType.Sequential)
                {
                    await outbox.ProcessAsync(message);
                }

                continue;
            }

            IDictionary<string, object>? headers = null;
            if (message.Headers.Count > 0)
            {
                headers = message.Headers
                    .Where(static header => header.Value is not null)
                    .ToDictionary(static header => header.Key, static header => (object)header.Value!);
            }

            await _publisher.PublishAsync(
                message.Message,
                message.Id,
                message.CorrelationId,
                message.SpanContext,
                message.MessageContext,
                headers,
                cancellationToken);

            if (_type == OutboxType.Sequential)
            {
                await outbox.ProcessAsync(message);
            }
        }

        if (_type == OutboxType.Parallel)
        {
            await outbox.ProcessAsync(messages);
        }

        stopwatch.Stop();

        _logger.LogTrace(
            "Processed {Count} outbox messages in {ElapsedMilliseconds} ms [job ID: '{JobId}'].",
            messages.Count,
            stopwatch.ElapsedMilliseconds,
            jobId);
    }

    private enum OutboxType
    {
        Sequential,
        Parallel
    }
}