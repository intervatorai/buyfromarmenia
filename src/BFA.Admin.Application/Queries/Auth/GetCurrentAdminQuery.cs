using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Auth;

public record GetCurrentAdminQuery(Guid AdminId) : IRequest<CurrentAdminDto?>;

public record CurrentAdminDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    DateTime? LastLoginAt);

public class GetCurrentAdminQueryHandler : IRequestHandler<GetCurrentAdminQuery, CurrentAdminDto?>
{
    private readonly IAdminUserRepository _adminUserRepository;

    public GetCurrentAdminQueryHandler(IAdminUserRepository adminUserRepository)
    {
        _adminUserRepository = adminUserRepository;
    }

    public async Task<CurrentAdminDto?> Handle(GetCurrentAdminQuery request, CancellationToken cancellationToken)
    {
        var adminUser = await _adminUserRepository.GetByIdAsync(request.AdminId, cancellationToken);

        if (adminUser is null || !adminUser.IsActive)
        {
            return null;
        }

        return new CurrentAdminDto(
            adminUser.Id,
            adminUser.Email,
            adminUser.FullName,
            adminUser.Role.ToString(),
            adminUser.LastLoginAt);
    }
}
