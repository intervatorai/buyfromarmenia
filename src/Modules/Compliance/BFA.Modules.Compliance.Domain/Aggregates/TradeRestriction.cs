using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Compliance.Domain.Aggregates;

public sealed class TradeRestriction : AggregateRoot
{
    public string DestinationCountryCode { get; private set; } = string.Empty;
    public Guid? CategoryId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; }

    private TradeRestriction()
    {
    }

    public static TradeRestriction Create(
        string destinationCountryCode,
        string reason,
        Guid? categoryId = null)
    {
        if (string.IsNullOrWhiteSpace(destinationCountryCode))
        {
            throw new DomainException("Destination country code is required.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Restriction reason is required.");
        }

        return new TradeRestriction
        {
            Id = Guid.NewGuid(),
            DestinationCountryCode = destinationCountryCode.Trim().ToUpperInvariant(),
            CategoryId = categoryId,
            Reason = reason.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
