using BFA.BuildingBlocks.Domain;
using BFA.Modules.Warehouse.Domain.Enums;
using BFA.Modules.Warehouse.Domain.Events;

namespace BFA.Modules.Warehouse.Domain.Aggregates;

public sealed class Consolidation : AggregateRoot
{
    private readonly List<ConsolidationItem> _items = [];
    private readonly List<Package> _packages = [];

    public Guid CustomerOrderId { get; private set; }
    public string ReferenceNumber { get; private set; } = string.Empty;
    public ConsolidationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<ConsolidationItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<Package> Packages => _packages.AsReadOnly();
    public IReadOnlyCollection<Guid> InboundShipmentIds =>
        _items.Select(item => item.InboundShipmentId).ToList().AsReadOnly();

    private Consolidation()
    {
    }

    public static Consolidation Create(
        Guid customerOrderId,
        IReadOnlyList<Guid> inboundShipmentIds)
    {
        if (customerOrderId == Guid.Empty)
        {
            throw new DomainException("Customer order id is required.");
        }

        if (inboundShipmentIds.Count == 0)
        {
            throw new DomainException("At least one inbound shipment is required.");
        }

        var consolidation = new Consolidation
        {
            Id = Guid.NewGuid(),
            CustomerOrderId = customerOrderId,
            ReferenceNumber = GenerateReferenceNumber(),
            Status = ConsolidationStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        consolidation._items.AddRange(
            inboundShipmentIds.Distinct().Select(id => new ConsolidationItem(consolidation.Id, id)));

        consolidation.RaiseDomainEvent(new ConsolidationCreatedDomainEvent(
            consolidation.Id,
            consolidation.CustomerOrderId));

        return consolidation;
    }

    public Package AddPackage(decimal weightKg, string? notes = null)
    {
        if (Status != ConsolidationStatus.Draft)
        {
            throw new DomainException("Packages can only be added to draft consolidations.");
        }

        var label = $"PKG-{_packages.Count + 1}";
        var package = new Package(Id, label, weightKg, notes);
        _packages.Add(package);
        Status = ConsolidationStatus.Packed;
        UpdatedAtUtc = DateTime.UtcNow;
        return package;
    }

    public void Seal()
    {
        if (Status == ConsolidationStatus.Sealed)
        {
            throw new DomainException("Consolidation is already sealed.");
        }

        if (_packages.Count == 0)
        {
            throw new DomainException("At least one package is required before sealing.");
        }

        Status = ConsolidationStatus.Sealed;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new ConsolidationSealedDomainEvent(Id, CustomerOrderId));
    }

    private static string GenerateReferenceNumber()
    {
        return $"CONS-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
    }
}
