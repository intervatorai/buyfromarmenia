using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Warehouse.Domain.Events;

public sealed record InboundShipmentCreatedDomainEvent(
    Guid InboundShipmentId,
    Guid SupplierOrderId,
    Guid SupplierId) : DomainEvent;

public sealed record InboundShipmentReceivedDomainEvent(
    Guid InboundShipmentId,
    Guid SupplierOrderId,
    string ScanReference) : DomainEvent;

public sealed record ConsolidationCreatedDomainEvent(
    Guid ConsolidationId,
    Guid CustomerOrderId) : DomainEvent;

public sealed record ConsolidationSealedDomainEvent(
    Guid ConsolidationId,
    Guid CustomerOrderId) : DomainEvent;
