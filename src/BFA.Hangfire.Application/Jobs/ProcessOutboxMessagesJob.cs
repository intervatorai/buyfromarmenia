using BFA.BuildingBlocks.Application;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace BFA.Hangfire.Application.Jobs;

public class ProcessOutboxMessagesJob
{
    private readonly IOutboxProcessor _outboxProcessor;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(
        IOutboxProcessor outboxProcessor,
        ILogger<ProcessOutboxMessagesJob> logger)
    {
        _outboxProcessor = outboxProcessor;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var processedCount = await _outboxProcessor.ProcessPendingAsync(cancellationToken);
        if (processedCount > 0)
        {
            _logger.LogInformation("Processed {Count} outbox messages.", processedCount);
        }
    }
}
