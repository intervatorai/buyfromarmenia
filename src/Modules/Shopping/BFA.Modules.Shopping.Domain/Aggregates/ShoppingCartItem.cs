using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Shopping.Domain.Aggregates;

public sealed class ShoppingCartItem : Entity
{
    public Guid ShoppingCartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = "USD";
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;

    private ShoppingCartItem()
    {
    }

    internal ShoppingCartItem(
        Guid shoppingCartId,
        Guid productId,
        Guid productVariantId,
        Guid supplierId,
        string productName,
        string? imageUrl,
        decimal unitPrice,
        string currency,
        int quantity)
    {
        Id = Guid.NewGuid();
        ShoppingCartId = shoppingCartId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        SupplierId = supplierId;
        ProductName = productName.Trim();
        ImageUrl = imageUrl?.Trim();
        UnitPrice = unitPrice;
        Currency = currency.Trim().ToUpperInvariant();
        ChangeQuantity(quantity);
    }

    internal void ChangeQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Cart item quantity must be positive.");
        }

        Quantity = quantity;
    }
}
