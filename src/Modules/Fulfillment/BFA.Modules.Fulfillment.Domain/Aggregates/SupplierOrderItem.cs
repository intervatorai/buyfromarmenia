using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Fulfillment.Domain.Aggregates;

public sealed class SupplierOrderItem : Entity
{
    public Guid SupplierOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string SupplierSku { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = "USD";
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;

    private SupplierOrderItem()
    {
    }

    internal SupplierOrderItem(
        Guid supplierOrderId,
        Guid productId,
        Guid productVariantId,
        string productName,
        string supplierSku,
        decimal unitPrice,
        string currency,
        int quantity)
    {
        Id = Guid.NewGuid();
        SupplierOrderId = supplierOrderId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        ProductName = productName.Trim();
        SupplierSku = supplierSku.Trim();
        UnitPrice = unitPrice;
        Currency = currency.Trim().ToUpperInvariant();
        Quantity = quantity;
    }
}
