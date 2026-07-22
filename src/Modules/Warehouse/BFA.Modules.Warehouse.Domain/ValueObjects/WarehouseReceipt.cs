using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Warehouse.Domain.ValueObjects;

public sealed class WarehouseReceipt : ValueObject
{
    public string ScanReference { get; private set; } = string.Empty;
    public decimal WeightKg { get; private set; }
    public string? InspectionNotes { get; private set; }
    public string? PhotoUrl { get; private set; }
    public string ReceivedBy { get; private set; } = string.Empty;
    public DateTime ReceivedAtUtc { get; private set; }

    private WarehouseReceipt()
    {
    }

    public WarehouseReceipt(
        string scanReference,
        decimal weightKg,
        string? inspectionNotes,
        string? photoUrl,
        string receivedBy)
    {
        if (string.IsNullOrWhiteSpace(scanReference))
        {
            throw new DomainException("Scan reference is required.");
        }

        if (weightKg <= 0)
        {
            throw new DomainException("Weight must be positive.");
        }

        if (string.IsNullOrWhiteSpace(receivedBy))
        {
            throw new DomainException("Received by is required.");
        }

        ScanReference = scanReference.Trim();
        WeightKg = weightKg;
        InspectionNotes = string.IsNullOrWhiteSpace(inspectionNotes)
            ? null
            : inspectionNotes.Trim();
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
        ReceivedBy = receivedBy.Trim();
        ReceivedAtUtc = DateTime.UtcNow;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ScanReference;
        yield return WeightKg;
        yield return InspectionNotes;
        yield return PhotoUrl;
        yield return ReceivedBy;
        yield return ReceivedAtUtc;
    }
}
