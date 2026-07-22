using BFA.Modules.Shipping.Domain.Aggregates;
using BFA.Modules.Shipping.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class ShipmentRepository : IShipmentRepository
{
    private readonly BfaDbContext _dbContext;

    public ShipmentRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Shipments.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<Shipment?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Shipments.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<Shipment?> GetByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Shipments
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CustomerOrderId == customerOrderId, cancellationToken);
    }

    public Task<Shipment?> GetByConsolidationIdAsync(
        Guid consolidationId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Shipments
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ConsolidationId == consolidationId, cancellationToken);
    }

    public async Task<IReadOnlyList<Shipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Shipments
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Shipments.AddAsync(shipment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        shipment.ClearDomainEvents();
    }

    public async Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        shipment.ClearDomainEvents();
    }
}
