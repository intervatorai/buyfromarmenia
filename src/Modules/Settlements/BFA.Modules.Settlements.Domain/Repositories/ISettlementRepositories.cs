using BFA.Modules.Settlements.Domain.Aggregates;

namespace BFA.Modules.Settlements.Domain.Repositories;

public interface ISupplierSettlementRepository
{
    Task<IReadOnlyList<SupplierSettlement>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SupplierSettlement>> GetBySupplierIdAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default);

    Task<SupplierSettlement?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(SupplierSettlement settlement, CancellationToken cancellationToken = default);
    Task UpdateAsync(SupplierSettlement settlement, CancellationToken cancellationToken = default);
}

public interface IPayoutRepository
{
    Task<IReadOnlyList<Payout>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payout>> GetBySupplierIdAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default);

    Task<Payout?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(Payout payout, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payout payout, CancellationToken cancellationToken = default);
}
