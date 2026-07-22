using BFA.BuildingBlocks.Domain;
using BFA.Modules.Fulfillment.Domain.Enums;
using BFA.Modules.Fulfillment.Domain.Events;

namespace BFA.Modules.Fulfillment.Domain.Aggregates;

public sealed class SupplierOrder : AggregateRoot
{
    private readonly List<SupplierOrderItem> _items = [];

    public Guid CustomerOrderId { get; private set; }
    public Guid SupplierId { get; private set; }
    public SupplierOrderStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public string Currency { get; private set; } = "USD";
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<SupplierOrderItem> Items => _items.AsReadOnly();

    private SupplierOrder()
    {
    }

    public SupplierOrder(
        Guid customerOrderId,
        Guid supplierId,
        string currency,
        IReadOnlyList<SupplierOrderItemDraft> items)
    {
        if (customerOrderId == Guid.Empty || supplierId == Guid.Empty)
        {
            throw new DomainException("Customer order and supplier are required.");
        }

        if (items.Count == 0)
        {
            throw new DomainException("Supplier order must contain items.");
        }

        Id = Guid.NewGuid();
        CustomerOrderId = customerOrderId;
        SupplierId = supplierId;
        Status = SupplierOrderStatus.New;
        Currency = currency.Trim().ToUpperInvariant();
        Subtotal = items.Sum(item => item.UnitPrice * item.Quantity);
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;

        foreach (var item in items)
        {
            _items.Add(new SupplierOrderItem(
                Id,
                item.ProductId,
                item.ProductVariantId,
                item.ProductName,
                item.SupplierSku,
                item.UnitPrice,
                item.Currency,
                item.Quantity));
        }

        RaiseDomainEvent(new SupplierOrderCreatedDomainEvent(
            Id,
            CustomerOrderId,
            SupplierId));
    }

    public void Confirm()
    {
        if (Status != SupplierOrderStatus.New)
        {
            throw new DomainException("Only new supplier orders can be confirmed.");
        }

        Status = SupplierOrderStatus.Confirmed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkPreparing()
    {
        if (Status is not (SupplierOrderStatus.New or SupplierOrderStatus.Confirmed))
        {
            throw new DomainException("Supplier order cannot be marked as preparing.");
        }

        Status = SupplierOrderStatus.Preparing;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkReadyForPickup()
    {
        if (Status is not (SupplierOrderStatus.Confirmed or SupplierOrderStatus.Preparing))
        {
            throw new DomainException("Supplier order cannot be marked ready for pickup.");
        }

        Status = SupplierOrderStatus.ReadyForPickup;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkTransferredToWarehouse()
    {
        if (Status != SupplierOrderStatus.ReadyForPickup)
        {
            throw new DomainException("Only ready-for-pickup orders can be transferred to warehouse.");
        }

        Status = SupplierOrderStatus.TransferredToWarehouse;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

public sealed record SupplierOrderItemDraft(
    Guid ProductId,
    Guid ProductVariantId,
    string ProductName,
    string SupplierSku,
    decimal UnitPrice,
    string Currency,
    int Quantity);
