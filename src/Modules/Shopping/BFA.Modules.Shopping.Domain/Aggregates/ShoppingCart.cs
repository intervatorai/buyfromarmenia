using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Shopping.Domain.Aggregates;

public sealed class ShoppingCart : AggregateRoot
{
    private readonly List<ShoppingCartItem> _items = [];
    private readonly List<WishlistItem> _wishlistItems = [];

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<ShoppingCartItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<WishlistItem> WishlistItems => _wishlistItems.AsReadOnly();

    private ShoppingCart()
    {
    }

    public ShoppingCart(Guid id) : base(id)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Shopping cart id is required.");
        }

        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void AddItem(
        Guid productId,
        Guid productVariantId,
        Guid supplierId,
        string productName,
        string? imageUrl,
        decimal unitPrice,
        string currency,
        int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new DomainException("Product name is required.");
        }

        if (unitPrice < 0)
        {
            throw new DomainException("Unit price cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        var existingItem = _items.FirstOrDefault(item =>
            item.ProductVariantId == productVariantId);

        if (existingItem is not null)
        {
            existingItem.ChangeQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            _items.Add(new ShoppingCartItem(
                Id,
                productId,
                productVariantId,
                supplierId,
                productName,
                imageUrl,
                unitPrice,
                currency,
                quantity));
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangeQuantity(Guid itemId, int quantity)
    {
        var item = GetItem(itemId);
        item.ChangeQuantity(quantity);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RemoveItem(Guid itemId)
    {
        _items.Remove(GetItem(itemId));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddToWishlist(Guid productId)
    {
        if (_wishlistItems.All(item => item.ProductId != productId))
        {
            _wishlistItems.Add(new WishlistItem(Id, productId));
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public void RemoveFromWishlist(Guid productId)
    {
        var item = _wishlistItems.FirstOrDefault(item => item.ProductId == productId);
        if (item is not null)
        {
            _wishlistItems.Remove(item);
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private ShoppingCartItem GetItem(Guid itemId)
    {
        return _items.FirstOrDefault(item => item.Id == itemId)
               ?? throw new DomainException("Shopping cart item was not found.");
    }
}
