using BFA.Modules.Suppliers.Domain.Aggregates;
using BFA.Modules.Suppliers.Domain.Enums;

namespace BFA.Modules.Suppliers.Domain.Repositories;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Supplier?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Supplier?> GetByContactEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Supplier>> GetByStatusAsync(
        SupplierStatus status,
        CancellationToken cancellationToken = default);
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
    Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);
}
