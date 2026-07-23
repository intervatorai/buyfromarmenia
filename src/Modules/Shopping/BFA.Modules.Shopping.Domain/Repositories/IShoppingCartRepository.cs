using BFA.Modules.Shopping.Domain.Aggregates;

namespace BFA.Modules.Shopping.Domain.Repositories;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ShoppingCart?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(ShoppingCart cart, CancellationToken cancellationToken = default);
    Task UpdateAsync(ShoppingCart cart, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes cart lines that reference a deleted catalog variant across all carts.
    /// </summary>
    Task<int> RemoveItemsByProductVariantIdAsync(
        Guid productVariantId,
        CancellationToken cancellationToken = default);
}
