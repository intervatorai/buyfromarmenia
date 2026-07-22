using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Ordering.Domain.Aggregates;

public sealed class CustomerOrderItem : Entity
{
    public Guid CustomerOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string SupplierSku { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = "USD";
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;

    private CustomerOrderItem()
    {
    }

    internal CustomerOrderItem(
        Guid customerOrderId,
        Guid productId,
        Guid productVariantId,
        Guid supplierId,
        string productName,
        string supplierSku,
        string? imageUrl,
        decimal unitPrice,
        string currency,
        int quantity)
    {
        Id = Guid.NewGuid();
        CustomerOrderId = customerOrderId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        SupplierId = supplierId;
        ProductName = productName.Trim();
        SupplierSku = supplierSku.Trim();
        ImageUrl = imageUrl?.Trim();
        UnitPrice = unitPrice;
        Currency = currency.Trim().ToUpperInvariant();
        Quantity = quantity;
    }
}
