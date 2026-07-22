using BFA.Modules.Ordering.Domain.Aggregates;

namespace BFA.Modules.Ordering.Domain.Repositories;

public interface ICustomerOrderRepository
{
    Task<CustomerOrder?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CustomerOrder?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CustomerOrder?> GetByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerOrder>> GetByCartIdAsync(
        Guid cartId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerOrder>> GetByCustomerUserIdAsync(
        Guid customerUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerOrder>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(CustomerOrder order, CancellationToken cancellationToken = default);
    Task UpdateAsync(CustomerOrder order, CancellationToken cancellationToken = default);
}
