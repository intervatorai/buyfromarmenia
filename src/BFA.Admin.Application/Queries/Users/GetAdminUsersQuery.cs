using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Users;

public record GetAdminUsersQuery() : IRequest<IReadOnlyList<AdminUserListItemDto>>;

public record AdminUserListItemDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public sealed class GetAdminUsersQueryHandler
    : IRequestHandler<GetAdminUsersQuery, IReadOnlyList<AdminUserListItemDto>>
{
    private readonly IAdminUserRepository _adminUserRepository;

    public GetAdminUsersQueryHandler(IAdminUserRepository adminUserRepository)
    {
        _adminUserRepository = adminUserRepository;
    }

    public async Task<IReadOnlyList<AdminUserListItemDto>> Handle(
        GetAdminUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _adminUserRepository.GetAllAsync(cancellationToken);

        return users
            .Select(user => new AdminUserListItemDto(
                user.Id,
                user.Email,
                user.FullName,
                user.Role.ToString(),
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt))
            .ToList();
    }
}
