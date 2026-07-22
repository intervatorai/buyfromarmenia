using BFA.BuildingBlocks.Domain;
using BFA.Modules.Suppliers.Domain.Enums;

namespace BFA.Modules.Suppliers.Domain.Aggregates;

public sealed class SupplierMember : Entity
{
    public Guid SupplierId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public SupplierMemberRole Role { get; private set; } = SupplierMemberRole.Manager;
    public bool IsActive { get; private set; } = true;
    public DateTime InvitedAt { get; private set; }
    public DateTime? JoinedAt { get; private set; }

    private SupplierMember()
    {
    }

    internal SupplierMember(
        Guid supplierId,
        string email,
        string fullName,
        SupplierMemberRole role,
        Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Member email is required.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("Member full name is required.");
        }

        Id = Guid.NewGuid();
        SupplierId = supplierId;
        Email = email.Trim().ToLowerInvariant();
        FullName = fullName.Trim();
        Role = role;
        UserId = userId;
        InvitedAt = DateTime.UtcNow;
        JoinedAt = userId.HasValue ? DateTime.UtcNow : null;
    }

    internal void AssignUser(Guid userId)
    {
        UserId = userId;
        JoinedAt = DateTime.UtcNow;
    }

    internal void ChangeRole(SupplierMemberRole role)
    {
        Role = role;
    }

    internal void Deactivate()
    {
        IsActive = false;
    }

    internal void Activate()
    {
        IsActive = true;
    }
}
