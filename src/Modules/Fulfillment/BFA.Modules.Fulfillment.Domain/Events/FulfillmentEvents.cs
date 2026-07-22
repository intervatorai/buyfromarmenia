using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Fulfillment.Domain.Events;

public sealed record SupplierOrderCreatedDomainEvent(
    Guid SupplierOrderId,
    Guid CustomerOrderId,
    Guid SupplierId) : DomainEvent;
