using BFA.Modules.Payments.Domain.Aggregates;
using BFA.Modules.Payments.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly BfaDbContext _dbContext;

    public PaymentRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Payment?> GetByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                payment => payment.CustomerOrderId == customerOrderId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByCustomerOrderIdsAsync(
        IReadOnlyCollection<Guid> customerOrderIds,
        CancellationToken cancellationToken = default)
    {
        if (customerOrderIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Payments
            .AsNoTracking()
            .Where(payment => customerOrderIds.Contains(payment.CustomerOrderId))
            .ToListAsync(cancellationToken);
    }

    public Task<Payment?> GetByCustomerOrderIdForUpdateAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Payments
            .FirstOrDefaultAsync(
                payment => payment.CustomerOrderId == customerOrderId,
                cancellationToken);
    }

    public Task<Payment?> GetByExternalReferenceForUpdateAsync(
        string externalReference,
        CancellationToken cancellationToken = default)
    {
        var normalized = externalReference.Trim();
        return _dbContext.Payments
            .FirstOrDefaultAsync(
                payment => payment.ExternalReference == normalized,
                cancellationToken);
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Payments.AddAsync(payment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        payment.ClearDomainEvents();
    }

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        payment.ClearDomainEvents();
    }
}
