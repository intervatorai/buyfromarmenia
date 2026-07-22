namespace BFA.BuildingBlocks.Application;

public static class IntegrationEventTypes
{
    public const string SupplierRegistered = "SupplierRegistered";
    public const string OrderPlaced = "OrderPlaced";
    public const string ProductApproved = "ProductApproved";
}

public interface IOrderFulfillmentOrchestrator
{
    Task StartForOrderAsync(Guid customerOrderId, CancellationToken cancellationToken = default);
}

public interface IOrderNotificationSender
{
    Task SendOrderConfirmationAsync(Guid customerOrderId, CancellationToken cancellationToken = default);
}
