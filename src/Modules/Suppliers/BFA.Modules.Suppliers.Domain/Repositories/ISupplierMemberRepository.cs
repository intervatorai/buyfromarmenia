using BFA.Modules.Suppliers.Domain.Enums;

namespace BFA.Modules.Suppliers.Domain.Repositories;

public record SupplierMemberAccount(
    Guid MemberId,
    Guid SupplierId,
    Guid? UserId,
    string Email,
    string FullName,
    string TradingName,
    SupplierMemberRole Role);

public interface ISupplierMemberRepository
{
    Task<SupplierMemberAccount?> GetActiveByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<SupplierMemberAccount?> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
