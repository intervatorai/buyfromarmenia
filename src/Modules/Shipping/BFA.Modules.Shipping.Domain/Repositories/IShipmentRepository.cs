using BFA.Modules.Shipping.Domain.Aggregates;

namespace BFA.Modules.Shipping.Domain.Repositories;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Shipment?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Shipment?> GetByCustomerOrderIdAsync(
        Guid customerOrderId,
        CancellationToken cancellationToken = default);
    Task<Shipment?> GetByConsolidationIdAsync(
        Guid consolidationId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shipment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default);
}
