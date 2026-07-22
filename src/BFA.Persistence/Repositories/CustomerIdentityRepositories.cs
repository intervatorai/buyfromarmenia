using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly BfaDbContext _dbContext;

    public UserRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == normalized, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        user.ClearDomainEvents();
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        user.ClearDomainEvents();
    }
}

public sealed class CustomerProfileRepository : ICustomerProfileRepository
{
    private readonly BfaDbContext _dbContext;

    public CustomerProfileRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CustomerProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public Task<CustomerProfile?> GetByUserIdForUpdateAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(CustomerProfile profile, CancellationToken cancellationToken = default)
    {
        await _dbContext.CustomerProfiles.AddAsync(profile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        profile.ClearDomainEvents();
    }

    public async Task UpdateAsync(CustomerProfile profile, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        profile.ClearDomainEvents();
    }
}

public sealed class CustomerDeliveryAddressRepository : ICustomerDeliveryAddressRepository
{
    private readonly BfaDbContext _dbContext;

    public CustomerDeliveryAddressRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CustomerDeliveryAddress>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CustomerDeliveryAddresses
            .AsNoTracking()
            .Where(address => address.UserId == userId)
            .OrderByDescending(address => address.IsDefault)
            .ThenByDescending(address => address.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<CustomerDeliveryAddress?> GetByIdForUserAsync(
        Guid addressId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerDeliveryAddresses
            .FirstOrDefaultAsync(
                address => address.Id == addressId && address.UserId == userId,
                cancellationToken);
    }

    public Task<CustomerDeliveryAddress?> GetDefaultForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomerDeliveryAddresses
            .FirstOrDefaultAsync(
                address => address.UserId == userId && address.IsDefault,
                cancellationToken);
    }

    public async Task AddAsync(
        CustomerDeliveryAddress address,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CustomerDeliveryAddresses.AddAsync(address, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        address.ClearDomainEvents();
    }

    public async Task UpdateAsync(
        CustomerDeliveryAddress address,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        address.ClearDomainEvents();
    }

    public async Task DeleteAsync(
        CustomerDeliveryAddress address,
        CancellationToken cancellationToken = default)
    {
        _dbContext.CustomerDeliveryAddresses.Remove(address);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
