using BFA.BuildingBlocks.Domain;
using BFA.Modules.Fulfillment.Domain.Aggregates;
using BFA.Modules.Warehouse.Domain.Enums;
using BFA.Modules.Warehouse.Domain.Events;
using BFA.Modules.Warehouse.Domain.ValueObjects;

namespace BFA.Modules.Warehouse.Domain.Aggregates;

public sealed class InboundShipment : AggregateRoot
{
    public Guid SupplierOrderId { get; private set; }
    public Guid CustomerOrderId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string ReferenceNumber { get; private set; } = string.Empty;
    public InboundShipmentStatus Status { get; private set; }
    public int ItemsCount { get; private set; }
    public Guid? ConsolidationId { get; private set; }
    public WarehouseReceipt? Receipt { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private InboundShipment()
    {
    }

    public static InboundShipment CreateFromSupplierOrder(SupplierOrder supplierOrder)
    {
        if (supplierOrder.Id == Guid.Empty)
        {
            throw new DomainException("Supplier order is required.");
        }

        var shipment = new InboundShipment
        {
            Id = Guid.NewGuid(),
            SupplierOrderId = supplierOrder.Id,
            CustomerOrderId = supplierOrder.CustomerOrderId,
            SupplierId = supplierOrder.SupplierId,
            ReferenceNumber = GenerateReferenceNumber(),
            Status = InboundShipmentStatus.Pending,
            ItemsCount = supplierOrder.Items.Count,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        shipment.RaiseDomainEvent(new InboundShipmentCreatedDomainEvent(
            shipment.Id,
            shipment.SupplierOrderId,
            shipment.SupplierId));

        return shipment;
    }

    public void MarkArrived()
    {
        if (Status != InboundShipmentStatus.Pending)
        {
            throw new DomainException("Only pending inbound shipments can be marked as arrived.");
        }

        Status = InboundShipmentStatus.Arrived;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Receive(
        string scanReference,
        decimal weightKg,
        string? inspectionNotes,
        string? photoUrl,
        string receivedBy)
    {
        if (Status is not (InboundShipmentStatus.Pending or InboundShipmentStatus.Arrived))
        {
            throw new DomainException("Inbound shipment cannot be received.");
        }

        Receipt = new WarehouseReceipt(
            scanReference,
            weightKg,
            inspectionNotes,
            photoUrl,
            receivedBy);
        Status = InboundShipmentStatus.Received;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new InboundShipmentReceivedDomainEvent(
            Id,
            SupplierOrderId,
            Receipt.ScanReference));
    }

    public void AssignToConsolidation(Guid consolidationId)
    {
        if (Status != InboundShipmentStatus.Received)
        {
            throw new DomainException("Only received inbound shipments can be consolidated.");
        }

        if (ConsolidationId.HasValue)
        {
            throw new DomainException("Inbound shipment is already assigned to a consolidation.");
        }

        if (consolidationId == Guid.Empty)
        {
            throw new DomainException("Consolidation id is required.");
        }

        ConsolidationId = consolidationId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string GenerateReferenceNumber()
    {
        return $"WH-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
    }
}
