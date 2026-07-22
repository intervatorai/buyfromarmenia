using BFA.BuildingBlocks.Domain;
using BFA.Modules.Shipping.Domain.Enums;
using BFA.Modules.Shipping.Domain.Events;
using BFA.Modules.Shipping.Domain.ValueObjects;

namespace BFA.Modules.Shipping.Domain.Aggregates;

public sealed class Shipment : AggregateRoot
{
    public Guid ConsolidationId { get; private set; }
    public Guid CustomerOrderId { get; private set; }
    public string ReferenceNumber { get; private set; } = string.Empty;
    public ShippingCarrier Carrier { get; private set; }
    public string TrackingNumber { get; private set; } = string.Empty;
    public ShipmentStatus Status { get; private set; }
    public CustomsDeclaration Customs { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Shipment()
    {
    }

    public static Shipment CreateFromConsolidation(
        Guid consolidationId,
        Guid customerOrderId,
        ShippingCarrier carrier,
        decimal declaredValue,
        string currency,
        string customsDescription)
    {
        if (consolidationId == Guid.Empty || customerOrderId == Guid.Empty)
        {
            throw new DomainException("Consolidation and customer order are required.");
        }

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            ConsolidationId = consolidationId,
            CustomerOrderId = customerOrderId,
            ReferenceNumber = GenerateReferenceNumber(),
            Carrier = carrier,
            TrackingNumber = GenerateTrackingNumber(carrier),
            Status = ShipmentStatus.Created,
            Customs = new CustomsDeclaration(customsDescription, declaredValue, currency),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        shipment.RaiseDomainEvent(new ShipmentCreatedDomainEvent(
            shipment.Id,
            shipment.CustomerOrderId,
            shipment.TrackingNumber));

        return shipment;
    }

    public void AdvanceStatus()
    {
        Status = Status switch
        {
            ShipmentStatus.Created => ShipmentStatus.PickedUp,
            ShipmentStatus.PickedUp => ShipmentStatus.InTransit,
            ShipmentStatus.InTransit => ShipmentStatus.OutForDelivery,
            ShipmentStatus.OutForDelivery => ShipmentStatus.Delivered,
            _ => throw new DomainException("Shipment is already delivered.")
        };

        UpdatedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new ShipmentStatusUpdatedDomainEvent(Id, CustomerOrderId, Status));
    }

    private static string GenerateReferenceNumber()
    {
        return $"SHIP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
    }

    private static string GenerateTrackingNumber(ShippingCarrier carrier)
    {
        var prefix = carrier switch
        {
            ShippingCarrier.Dhl => "DHL",
            ShippingCarrier.FedEx => "FDX",
            _ => "BFA"
        };

        return $"{prefix}{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000000, 9999999)}";
    }
}
