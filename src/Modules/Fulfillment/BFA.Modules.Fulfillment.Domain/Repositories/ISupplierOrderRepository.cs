using BFA.Modules.Fulfillment.Domain.Aggregates;

namespace BFA.Modules.Fulfillment.Domain.Repositories;

public interface ISupplierOrderRepository
{
    Task<SupplierOrder?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SupplierOrder?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SupplierOrder>> GetBySupplierIdAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SupplierOrder>> GetByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SupplierOrder>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(SupplierOrder order, CancellationToken cancellationToken = default);
    Task UpdateAsync(SupplierOrder order, CancellationToken cancellationToken = default);
}
