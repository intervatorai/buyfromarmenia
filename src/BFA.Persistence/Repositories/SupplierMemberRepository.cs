using BFA.Modules.Suppliers.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class SupplierMemberRepository : ISupplierMemberRepository
{
    private readonly BfaDbContext _dbContext;

    public SupplierMemberRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SupplierMemberAccount?> GetActiveByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await (
            from member in _dbContext.SupplierMembers.AsNoTracking()
            join supplier in _dbContext.Suppliers.AsNoTracking()
                on member.SupplierId equals supplier.Id
            where member.IsActive && member.Email == normalizedEmail
            select new SupplierMemberAccount(
                member.Id,
                member.SupplierId,
                member.UserId,
                member.Email,
                member.FullName,
                supplier.TradingName,
                member.Role)
        ).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SupplierMemberAccount?> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from member in _dbContext.SupplierMembers.AsNoTracking()
            join supplier in _dbContext.Suppliers.AsNoTracking()
                on member.SupplierId equals supplier.Id
            where member.IsActive && member.UserId == userId
            select new SupplierMemberAccount(
                member.Id,
                member.SupplierId,
                member.UserId,
                member.Email,
                member.FullName,
                supplier.TradingName,
                member.Role)
        ).FirstOrDefaultAsync(cancellationToken);
    }
}
