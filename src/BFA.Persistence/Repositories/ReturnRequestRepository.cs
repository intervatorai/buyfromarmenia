using BFA.Modules.Returns.Domain.Aggregates;
using BFA.Modules.Returns.Domain.Enums;
using BFA.Modules.Returns.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class ReturnRequestRepository : IReturnRequestRepository
{
    private readonly BfaDbContext _dbContext;

    public ReturnRequestRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ReturnRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ReturnRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(request => request.Id == id, cancellationToken);
    }

    public Task<ReturnRequest?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ReturnRequests
            .FirstOrDefaultAsync(request => request.Id == id, cancellationToken);
    }

    public Task<ReturnRequest?> GetOpenByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ReturnRequests
            .AsNoTracking()
            .Where(request =>
                request.CustomerOrderId == customerOrderId
                && request.Status != ReturnRequestStatus.Rejected
                && request.Status != ReturnRequestStatus.Refunded
                && request.Status != ReturnRequestStatus.Cancelled)
            .OrderByDescending(request => request.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReturnRequest>> GetByStatusAsync(
        ReturnRequestStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ReturnRequests.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(request => request.Status == status.Value);
        }

        return await query
            .OrderByDescending(request => request.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReturnRequest>> GetByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ReturnRequests
            .AsNoTracking()
            .Where(request => request.CustomerOrderId == customerOrderId)
            .OrderByDescending(request => request.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ReturnRequest returnRequest, CancellationToken cancellationToken = default)
    {
        await _dbContext.ReturnRequests.AddAsync(returnRequest, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        returnRequest.ClearDomainEvents();
    }

    public async Task UpdateAsync(ReturnRequest returnRequest, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        returnRequest.ClearDomainEvents();
    }
}
