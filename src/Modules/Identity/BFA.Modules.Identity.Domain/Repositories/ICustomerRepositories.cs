using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Enums;

namespace BFA.Modules.Identity.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetCustomersAsync(
        UserStatus? status = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}

public interface ICustomerProfileRepository
{
    Task<CustomerProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    Task<CustomerProfile?> GetByUserIdForUpdateAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerProfile>> GetByUserIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default);
    Task AddAsync(CustomerProfile profile, CancellationToken cancellationToken = default);
    Task UpdateAsync(CustomerProfile profile, CancellationToken cancellationToken = default);
}

public interface ICustomerDeliveryAddressRepository
{
    Task<IReadOnlyList<CustomerDeliveryAddress>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<CustomerDeliveryAddress?> GetByIdForUserAsync(
        Guid addressId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<CustomerDeliveryAddress?> GetDefaultForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(CustomerDeliveryAddress address, CancellationToken cancellationToken = default);
    Task UpdateAsync(CustomerDeliveryAddress address, CancellationToken cancellationToken = default);
    Task DeleteAsync(CustomerDeliveryAddress address, CancellationToken cancellationToken = default);
}
