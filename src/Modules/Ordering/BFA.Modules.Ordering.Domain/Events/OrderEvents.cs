using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Ordering.Domain.Events;

public sealed record CustomerOrderPlacedDomainEvent(
    Guid OrderId,
    string OrderNumber,
    Guid CartId) : DomainEvent;
