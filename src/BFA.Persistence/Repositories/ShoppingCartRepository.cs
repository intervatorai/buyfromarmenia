using BFA.Modules.Shopping.Domain.Aggregates;
using BFA.Modules.Shopping.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly BfaDbContext _dbContext;

    public ShoppingCartRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ShoppingCart?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return QueryWithDetails()
            .AsNoTracking()
            .FirstOrDefaultAsync(cart => cart.Id == id, cancellationToken);
    }

    public Task<ShoppingCart?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return QueryWithDetails()
            .FirstOrDefaultAsync(cart => cart.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        ShoppingCart cart,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ShoppingCarts.AddAsync(cart, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        cart.ClearDomainEvents();
    }

    public async Task UpdateAsync(
        ShoppingCart cart,
        CancellationToken cancellationToken = default)
    {
        // Client-assigned Guids on new cart lines can be tracked as Modified after a
        // multi-collection Include; force INSERT when the row is not in the database.
        var persistedItemIds = await _dbContext.Set<ShoppingCartItem>()
            .AsNoTracking()
            .Where(item => item.ShoppingCartId == cart.Id)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var persistedWishlistIds = await _dbContext.Set<WishlistItem>()
            .AsNoTracking()
            .Where(item => item.ShoppingCartId == cart.Id)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

        var currentItemIds = cart.Items.Select(item => item.Id).ToHashSet();
        foreach (var persistedId in persistedItemIds.Where(id => !currentItemIds.Contains(id)))
        {
            var tracked = _dbContext.ShoppingCartItems.Local
                              .FirstOrDefault(item => item.Id == persistedId)
                          ?? await _dbContext.ShoppingCartItems.FirstOrDefaultAsync(
                              item => item.Id == persistedId,
                              cancellationToken);
            if (tracked is not null)
            {
                _dbContext.ShoppingCartItems.Remove(tracked);
            }
        }

        EnsureInserted(cart.Items, persistedItemIds);
        EnsureInserted(cart.WishlistItems, persistedWishlistIds);

        await _dbContext.SaveChangesAsync(cancellationToken);
        cart.ClearDomainEvents();
    }

    public async Task<int> RemoveItemsByProductVariantIdAsync(
        Guid productVariantId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShoppingCartItems
            .Where(item => item.ProductVariantId == productVariantId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private void EnsureInserted<TEntity>(
        IEnumerable<TEntity> entities,
        IReadOnlyCollection<Guid> persistedIds)
        where TEntity : class
    {
        foreach (var entity in entities)
        {
            var entry = _dbContext.Entry(entity);
            var id = (Guid)(entry.Property("Id").CurrentValue ?? Guid.Empty);
            if (persistedIds.Contains(id) || entry.State == EntityState.Added)
            {
                continue;
            }

            if (entry.State is EntityState.Modified or EntityState.Unchanged)
            {
                entry.State = EntityState.Detached;
            }

            if (entry.State == EntityState.Detached)
            {
                _dbContext.Set<TEntity>().Add(entity);
            }
        }
    }

    private IQueryable<ShoppingCart> QueryWithDetails()
    {
        return _dbContext.ShoppingCarts
            .AsSplitQuery()
            .Include("_items")
            .Include("_wishlistItems");
    }
}
