using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Warehouse.Domain.Aggregates;

public sealed class Package : Entity
{
    public Guid ConsolidationId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public decimal WeightKg { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Package()
    {
    }

    internal Package(
        Guid consolidationId,
        string label,
        decimal weightKg,
        string? notes)
    {
        if (consolidationId == Guid.Empty)
        {
            throw new DomainException("Consolidation id is required.");
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new DomainException("Package label is required.");
        }

        if (weightKg <= 0)
        {
            throw new DomainException("Package weight must be positive.");
        }

        Id = Guid.NewGuid();
        ConsolidationId = consolidationId;
        Label = label.Trim();
        WeightKg = weightKg;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        CreatedAtUtc = DateTime.UtcNow;
    }
}
