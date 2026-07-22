using BFA.Modules.Payments.Domain.Aggregates;

namespace BFA.Modules.Payments.Domain.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}
