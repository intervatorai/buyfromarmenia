using BFA.Modules.Payments.Domain.Aggregates;

namespace BFA.Modules.Payments.Domain.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> GetByCustomerOrderIdsAsync(
        IReadOnlyCollection<Guid> customerOrderIds,
        CancellationToken cancellationToken = default);

    Task<Payment?> GetByCustomerOrderIdForUpdateAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default);

    Task<Payment?> GetByExternalReferenceForUpdateAsync(
        string externalReference,
        CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}
