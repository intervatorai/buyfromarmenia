using BFA.BuildingBlocks.Domain;
using BFA.Modules.Identity.Domain.Enums;

namespace BFA.Modules.Identity.Domain.Aggregates;

public sealed class AdminUser : AggregateRoot
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public AdminRole Role { get; private set; } = AdminRole.Admin;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private AdminUser()
    {
    }

    public static AdminUser Create(
        string email,
        string passwordHash,
        string fullName,
        AdminRole role)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Admin email is required.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        return new AdminUser
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void RecordLogin()
    {
        if (!IsActive)
        {
            throw new DomainException("Inactive admin cannot sign in.");
        }

        LastLoginAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateProfile(string fullName, AdminRole role)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("Full name is required.");
        }

        FullName = fullName.Trim();
        Role = role;
    }
}
