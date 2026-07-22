using BFA.BuildingBlocks.Application;
using Microsoft.Extensions.Logging;

namespace BFA.Infrastructure.Notifications;

public sealed class LoggingOrderNotificationSender : IOrderNotificationSender
{
    private readonly ILogger<LoggingOrderNotificationSender> _logger;

    public LoggingOrderNotificationSender(ILogger<LoggingOrderNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task SendOrderConfirmationAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Notification stub: order confirmation queued for customer order {CustomerOrderId}.",
            customerOrderId);

        return Task.CompletedTask;
    }
}
