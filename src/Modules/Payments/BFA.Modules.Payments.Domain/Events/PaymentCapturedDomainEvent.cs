using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Payments.Domain.Events;

public sealed record PaymentCapturedDomainEvent(
    Guid PaymentId,
    Guid CustomerOrderId,
    decimal Amount,
    string Currency) : DomainEvent;
