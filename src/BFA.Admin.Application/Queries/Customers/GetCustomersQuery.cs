using BFA.Modules.Identity.Domain.Enums;
using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Customers;

public record GetCustomersQuery(string? Status = null)
    : IRequest<IReadOnlyList<CustomerListItemDto>>;

public record GetCustomerQuery(Guid CustomerId) : IRequest<CustomerDetailDto?>;

public record CustomerListItemDto(
    Guid Id,
    string Email,
    string FullName,
    string? Phone,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc);

public record CustomerDetailDto(
    Guid Id,
    string Email,
    string FullName,
    string? Phone,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc,
    DateTime ProfileUpdatedAtUtc);

public sealed class GetCustomersQueryHandler
    : IRequestHandler<GetCustomersQuery, IReadOnlyList<CustomerListItemDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerProfileRepository _profileRepository;

    public GetCustomersQueryHandler(
        IUserRepository userRepository,
        ICustomerProfileRepository profileRepository)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
    }

    public async Task<IReadOnlyList<CustomerListItemDto>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        UserStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<UserStatus>(request.Status, true, out var parsed))
        {
            status = parsed;
        }

        var users = await _userRepository.GetCustomersAsync(status, cancellationToken);
        var profiles = await _profileRepository.GetByUserIdsAsync(
            users.Select(user => user.Id).ToList(),
            cancellationToken);
        var profileByUserId = profiles.ToDictionary(profile => profile.UserId);

        return users
            .Select(user =>
            {
                profileByUserId.TryGetValue(user.Id, out var profile);
                return new CustomerListItemDto(
                    user.Id,
                    user.Email,
                    profile?.FullName ?? string.Empty,
                    profile?.Phone,
                    user.Status.ToString(),
                    user.CreatedAtUtc,
                    user.LastLoginAtUtc);
            })
            .ToList();
    }
}

public sealed class GetCustomerQueryHandler
    : IRequestHandler<GetCustomerQuery, CustomerDetailDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerProfileRepository _profileRepository;

    public GetCustomerQueryHandler(
        IUserRepository userRepository,
        ICustomerProfileRepository profileRepository)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
    }

    public async Task<CustomerDetailDto?> Handle(
        GetCustomerQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (user is null || user.Type != UserType.Customer)
        {
            return null;
        }

        var profile = await _profileRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        return new CustomerDetailDto(
            user.Id,
            user.Email,
            profile.FullName,
            profile.Phone,
            user.Status.ToString(),
            user.CreatedAtUtc,
            user.LastLoginAtUtc,
            profile.UpdatedAtUtc);
    }
}
