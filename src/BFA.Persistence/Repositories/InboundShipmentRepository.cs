using BFA.Modules.Warehouse.Domain.Aggregates;
using BFA.Modules.Warehouse.Domain.Enums;
using BFA.Modules.Warehouse.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class InboundShipmentRepository : IInboundShipmentRepository
{
    private readonly BfaDbContext _dbContext;

    public InboundShipmentRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<InboundShipment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.InboundShipments
            .AsNoTracking()
            .FirstOrDefaultAsync(shipment => shipment.Id == id, cancellationToken);
    }

    public Task<InboundShipment?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.InboundShipments
            .FirstOrDefaultAsync(shipment => shipment.Id == id, cancellationToken);
    }

    public Task<InboundShipment?> GetBySupplierOrderIdAsync(
        Guid supplierOrderId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.InboundShipments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                shipment => shipment.SupplierOrderId == supplierOrderId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<InboundShipment>> GetAllAsync(
        InboundShipmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.InboundShipments.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(shipment => shipment.Status == status.Value);
        }

        return await query
            .OrderByDescending(shipment => shipment.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InboundShipment>> GetEligibleForConsolidationAsync(
        Guid? customerOrderId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.InboundShipments
            .AsNoTracking()
            .Where(shipment =>
                shipment.Status == InboundShipmentStatus.Received
                && shipment.ConsolidationId == null);

        if (customerOrderId.HasValue)
        {
            query = query.Where(shipment => shipment.CustomerOrderId == customerOrderId.Value);
        }

        return await query
            .OrderBy(shipment => shipment.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        InboundShipment shipment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.InboundShipments.AddAsync(shipment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        shipment.ClearDomainEvents();
    }

    public async Task UpdateAsync(
        InboundShipment shipment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        shipment.ClearDomainEvents();
    }
}
