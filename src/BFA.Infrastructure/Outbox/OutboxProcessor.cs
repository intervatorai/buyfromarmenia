using BFA.BuildingBlocks.Application;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BFA.Infrastructure.Outbox;

public sealed class OutboxProcessor : IOutboxProcessor
{
    private const int DefaultBatchSize = 50;
    private readonly IOutboxReader _outboxReader;
    private readonly IOrderFulfillmentOrchestrator _orderFulfillmentOrchestrator;
    private readonly IOrderNotificationSender _orderNotificationSender;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IOutboxReader outboxReader,
        IOrderFulfillmentOrchestrator orderFulfillmentOrchestrator,
        IOrderNotificationSender orderNotificationSender,
        ILogger<OutboxProcessor> logger)
    {
        _outboxReader = outboxReader;
        _orderFulfillmentOrchestrator = orderFulfillmentOrchestrator;
        _orderNotificationSender = orderNotificationSender;
        _logger = logger;
    }

    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var messages = await _outboxReader.GetPendingAsync(DefaultBatchSize, cancellationToken);
        var processedCount = 0;

        foreach (var message in messages)
        {
            await HandleMessageAsync(message, cancellationToken);
            await _outboxReader.MarkProcessedAsync(message.Id, cancellationToken);
            processedCount++;
        }

        return processedCount;
    }

    private async Task HandleMessageAsync(PendingOutboxMessage message, CancellationToken cancellationToken)
    {
        switch (message.Type)
        {
            case IntegrationEventTypes.SupplierRegistered:
                _logger.LogInformation(
                    "Outbox handled SupplierRegistered message {MessageId}",
                    message.Id);
                break;
            case IntegrationEventTypes.OrderPlaced:
                if (TryReadGuid(message.Payload, "orderId", out var orderId))
                {
                    await _orderFulfillmentOrchestrator.StartForOrderAsync(orderId, cancellationToken);
                    await _orderNotificationSender.SendOrderConfirmationAsync(orderId, cancellationToken);
                }
                else
                {
                    _logger.LogWarning(
                        "OrderPlaced outbox message {MessageId} has invalid payload.",
                        message.Id);
                }
                break;
            case IntegrationEventTypes.ProductApproved:
                _logger.LogInformation(
                    "Outbox handled ProductApproved message {MessageId}",
                    message.Id);
                break;
            default:
                _logger.LogWarning(
                    "Outbox message type {MessageType} has no handler yet.",
                    message.Type);
                break;
        }
    }

    private static bool TryReadGuid(string payload, string propertyName, out Guid value)
    {
        value = Guid.Empty;

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.TryGetProperty(propertyName, out var property)
                && Guid.TryParse(property.GetString(), out var parsed))
            {
                value = parsed;
                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }
}
