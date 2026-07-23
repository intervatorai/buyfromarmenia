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
        try
        {
            var processedCount = await _outboxProcessor.ProcessPendingAsync(cancellationToken);
            if (processedCount > 0)
            {
                _logger.LogInformation("Processed {Count} outbox messages.", processedCount);
            }
        }
        catch (Exception ex) when (IsMissingRelation(ex))
        {
            _logger.LogWarning(
                ex,
                "Outbox table is not ready yet; skipping this run.");
        }
    }

    private static bool IsMissingRelation(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            var sqlState = current.GetType().GetProperty("SqlState")?.GetValue(current) as string;
            if (sqlState == "42P01")
            {
                return true;
            }

            if (current.Message.Contains("outbox_messages", StringComparison.OrdinalIgnoreCase)
                && current.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
