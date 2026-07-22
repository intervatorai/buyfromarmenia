using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Enums;

namespace BFA.Modules.Identity.Domain.Repositories;

public interface IAdminUserRepository
{
    Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdminUser?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAndRoleAsync(
        string email,
        AdminRole role,
        CancellationToken cancellationToken = default);
    Task AddAsync(AdminUser adminUser, CancellationToken cancellationToken = default);
    Task UpdateAsync(AdminUser adminUser, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminUser>> GetAllAsync(CancellationToken cancellationToken = default);
}
