using BFA.Modules.Ordering.Domain.Aggregates;
using BFA.Modules.Ordering.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class CustomerOrderRepository : ICustomerOrderRepository
{
    private readonly BfaDbContext _dbContext;

    public CustomerOrderRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CustomerOrder?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return QueryWithItems()
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public Task<CustomerOrder?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return QueryWithItems()
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public Task<CustomerOrder?> GetByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        return QueryWithItems()
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerOrder>> GetByCartIdAsync(
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        return await QueryWithItems()
            .AsNoTracking()
            .Where(order => order.CartId == cartId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerOrder>> GetByCustomerUserIdAsync(
        Guid customerUserId,
        CancellationToken cancellationToken = default)
    {
        return await QueryWithItems()
            .AsNoTracking()
            .Where(order => order.CustomerUserId == customerUserId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerOrder>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await QueryWithItems()
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        CustomerOrder order,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CustomerOrders.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        order.ClearDomainEvents();
    }

    public async Task UpdateAsync(
        CustomerOrder order,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        order.ClearDomainEvents();
    }

    private IQueryable<CustomerOrder> QueryWithItems()
    {
        return _dbContext.CustomerOrders.Include("_items");
    }
}
