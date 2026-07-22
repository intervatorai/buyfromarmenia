using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Identity.Domain.Aggregates;

public sealed class CustomerProfile : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private CustomerProfile()
    {
    }

    public static CustomerProfile Create(Guid userId, string fullName, string? phone = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("Full name is required.");
        }

        var now = DateTime.UtcNow;

        return new CustomerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = fullName.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void Update(string fullName, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("Full name is required.");
        }

        FullName = fullName.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
