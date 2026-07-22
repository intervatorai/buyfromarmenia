using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Shopping.Domain.Aggregates;

public sealed class WishlistItem : Entity
{
    public Guid ShoppingCartId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime AddedAtUtc { get; private set; }

    private WishlistItem()
    {
    }

    internal WishlistItem(Guid shoppingCartId, Guid productId)
    {
        Id = Guid.NewGuid();
        ShoppingCartId = shoppingCartId;
        ProductId = productId;
        AddedAtUtc = DateTime.UtcNow;
    }
}
