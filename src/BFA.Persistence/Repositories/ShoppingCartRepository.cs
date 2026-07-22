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
        await _dbContext.SaveChangesAsync(cancellationToken);
        cart.ClearDomainEvents();
    }

    private IQueryable<ShoppingCart> QueryWithDetails()
    {
        return _dbContext.ShoppingCarts
            .Include("_items")
            .Include("_wishlistItems");
    }
}
