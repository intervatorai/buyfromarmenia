using BFA.BuildingBlocks.Domain;
using BFA.Modules.Shipping.Domain.Enums;

namespace BFA.Modules.Shipping.Domain.Events;

public sealed record ShipmentCreatedDomainEvent(
    Guid ShipmentId,
    Guid CustomerOrderId,
    string TrackingNumber) : DomainEvent;

public sealed record ShipmentStatusUpdatedDomainEvent(
    Guid ShipmentId,
    Guid CustomerOrderId,
    ShipmentStatus Status) : DomainEvent;
