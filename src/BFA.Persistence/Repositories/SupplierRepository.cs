using BFA.Modules.Suppliers.Domain.Aggregates;
using BFA.Modules.Suppliers.Domain.Enums;
using BFA.Modules.Suppliers.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly BfaDbContext _dbContext;

    public SupplierRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .AsNoTracking()
            .FirstOrDefaultAsync(supplier => supplier.Id == id, cancellationToken);
    }

    public async Task<Supplier?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .FirstOrDefaultAsync(supplier => supplier.Id == id, cancellationToken);
    }

    public async Task<Supplier?> GetByContactEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await QueryWithDetails()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                supplier => supplier.Contact.Email == normalizedEmail,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .AsNoTracking()
            .OrderByDescending(supplier => supplier.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Supplier>> GetByStatusAsync(
        SupplierStatus status,
        CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .AsNoTracking()
            .Where(supplier => supplier.Status == status)
            .OrderByDescending(supplier => supplier.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        await _dbContext.Suppliers.AddAsync(supplier, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        supplier.ClearDomainEvents();
    }

    public async Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        supplier.ClearDomainEvents();
    }

    private IQueryable<Supplier> QueryWithDetails()
    {
        return _dbContext.Suppliers
            .Include("_members")
            .Include("_documents")
            .Include("_bankAccounts");
    }
}
