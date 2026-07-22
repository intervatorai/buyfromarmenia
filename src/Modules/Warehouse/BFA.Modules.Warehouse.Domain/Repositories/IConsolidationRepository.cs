using BFA.Modules.Warehouse.Domain.Aggregates;
using BFA.Modules.Warehouse.Domain.Enums;

namespace BFA.Modules.Warehouse.Domain.Repositories;

public interface IConsolidationRepository
{
    Task<Consolidation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Consolidation?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Consolidation?> GetByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Consolidation>> GetAllAsync(
        ConsolidationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Consolidation consolidation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Consolidation consolidation, CancellationToken cancellationToken = default);
}
