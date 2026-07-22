using BFA.Modules.Warehouse.Domain.Aggregates;
using BFA.Modules.Warehouse.Domain.Enums;

namespace BFA.Modules.Warehouse.Domain.Repositories;

public interface IInboundShipmentRepository
{
    Task<InboundShipment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<InboundShipment?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<InboundShipment?> GetBySupplierOrderIdAsync(
        Guid supplierOrderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InboundShipment>> GetAllAsync(
        InboundShipmentStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InboundShipment>> GetEligibleForConsolidationAsync(
        Guid? customerOrderId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(InboundShipment shipment, CancellationToken cancellationToken = default);
    Task UpdateAsync(InboundShipment shipment, CancellationToken cancellationToken = default);
}
