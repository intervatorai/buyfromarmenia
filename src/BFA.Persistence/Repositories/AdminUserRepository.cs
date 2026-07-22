using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Enums;
using BFA.Modules.Identity.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly BfaDbContext _dbContext;

    public AdminUserRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AdminUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<AdminUser?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.AdminUsers
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _dbContext.AdminUsers
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAndRoleAsync(
        string email,
        AdminRole role,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _dbContext.AdminUsers
            .AsNoTracking()
            .AnyAsync(
                user => user.Email == normalizedEmail && user.Role == role,
                cancellationToken);
    }

    public async Task AddAsync(AdminUser adminUser, CancellationToken cancellationToken = default)
    {
        await _dbContext.AdminUsers.AddAsync(adminUser, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        adminUser.ClearDomainEvents();
    }

    public async Task UpdateAsync(AdminUser adminUser, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        adminUser.ClearDomainEvents();
    }

    public async Task<IReadOnlyList<AdminUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.AdminUsers
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .ToListAsync(cancellationToken);
    }
}
