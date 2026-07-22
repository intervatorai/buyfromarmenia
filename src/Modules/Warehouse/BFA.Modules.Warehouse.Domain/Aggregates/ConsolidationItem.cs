using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Warehouse.Domain.Aggregates;

public sealed class ConsolidationItem : Entity
{
    public Guid ConsolidationId { get; private set; }
    public Guid InboundShipmentId { get; private set; }

    private ConsolidationItem()
    {
    }

    internal ConsolidationItem(Guid consolidationId, Guid inboundShipmentId)
    {
        if (consolidationId == Guid.Empty || inboundShipmentId == Guid.Empty)
        {
            throw new DomainException("Consolidation and inbound shipment are required.");
        }

        Id = Guid.NewGuid();
        ConsolidationId = consolidationId;
        InboundShipmentId = inboundShipmentId;
    }
}
