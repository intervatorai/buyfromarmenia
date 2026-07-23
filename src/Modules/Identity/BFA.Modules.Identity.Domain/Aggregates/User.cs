using BFA.BuildingBlocks.Domain;
using BFA.Modules.Identity.Domain.Enums;

namespace BFA.Modules.Identity.Domain.Aggregates;

public sealed class User : AggregateRoot
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserType Type { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }

    private User()
    {
    }

    public static User RegisterCustomer(string email, string passwordHash)
    {
        return Register(email, passwordHash, UserType.Customer);
    }

    public static User RegisterSupplier(string email, string passwordHash)
    {
        return Register(email, passwordHash, UserType.Supplier);
    }

    private static User Register(string email, string passwordHash, UserType type)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Type = type,
            Status = UserStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void RecordLogin()
    {
        if (Status != UserStatus.Active)
        {
            throw new DomainException("Suspended user cannot sign in.");
        }

        LastLoginAtUtc = DateTime.UtcNow;
    }

    public void Suspend()
    {
        if (Status == UserStatus.Suspended)
        {
            return;
        }

        Status = UserStatus.Suspended;
    }

    public void Activate()
    {
        if (Status == UserStatus.Active)
        {
            return;
        }

        Status = UserStatus.Active;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        PasswordHash = passwordHash;
    }
}
