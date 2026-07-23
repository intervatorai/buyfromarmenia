using BFA.BuildingBlocks.Application;
using BFA.Modules.Identity.Domain.Auth;
using BFA.Modules.Identity.Domain.Enums;
using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Customers;

public record ImpersonateCustomerCommand(Guid CustomerId, Guid AdminUserId)
    : IRequest<ImpersonateCustomerResult?>;

public record ImpersonateCustomerResult(
    string AccessToken,
    DateTime ExpiresAt,
    Guid UserId,
    string Email,
    string FullName,
    string? Phone);

public sealed class ImpersonateCustomerCommandHandler
    : IRequestHandler<ImpersonateCustomerCommand, ImpersonateCustomerResult?>
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerProfileRepository _profileRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditLogger _auditLogger;

    public ImpersonateCustomerCommandHandler(
        IUserRepository userRepository,
        ICustomerProfileRepository profileRepository,
        IJwtTokenService jwtTokenService,
        IAuditLogger auditLogger)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _jwtTokenService = jwtTokenService;
        _auditLogger = auditLogger;
    }

    public async Task<ImpersonateCustomerResult?> Handle(
        ImpersonateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (user is null || user.Type != UserType.Customer)
        {
            return null;
        }

        if (user.Status != UserStatus.Active)
        {
            return null;
        }

        var profile = await _profileRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var token = _jwtTokenService.GenerateCustomerToken(
            user,
            profile,
            impersonatedByAdminId: request.AdminUserId,
            expirationHoursOverride: 2);

        await _auditLogger.WriteAsync(
            "Admin",
            request.AdminUserId,
            "CustomerImpersonated",
            "Customer",
            user.Id,
            cancellationToken: cancellationToken);

        return new ImpersonateCustomerResult(
            token.AccessToken,
            token.ExpiresAt,
            user.Id,
            user.Email,
            profile.FullName,
            profile.Phone);
    }
}
