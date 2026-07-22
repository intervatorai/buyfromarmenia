using BFA.BuildingBlocks.Application;
using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Auth;
using BFA.Modules.Identity.Domain.Enums;
using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Users;

public record CreateAdminUserCommand(
    string Email,
    string Password,
    string FullName,
    string Role) : IRequest<Guid?>;

public sealed class CreateAdminUserCommandHandler : IRequestHandler<CreateAdminUserCommand, Guid?>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogger _auditLogger;

    public CreateAdminUserCommandHandler(
        IAdminUserRepository adminUserRepository,
        IPasswordHasher passwordHasher,
        IAuditLogger auditLogger)
    {
        _adminUserRepository = adminUserRepository;
        _passwordHasher = passwordHasher;
        _auditLogger = auditLogger;
    }

    public async Task<Guid?> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AdminRole>(request.Role, true, out var role))
        {
            return null;
        }

        var existing = await _adminUserRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return null;
        }

        var user = AdminUser.Create(
            request.Email,
            _passwordHasher.Hash(request.Password),
            request.FullName,
            role);

        await _adminUserRepository.AddAsync(user, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "AdminUserCreated",
            "AdminUser",
            user.Id,
            cancellationToken: cancellationToken);

        return user.Id;
    }
}

public record UpdateAdminUserCommand(Guid UserId, string FullName, string Role) : IRequest<bool>;

public sealed class UpdateAdminUserCommandHandler : IRequestHandler<UpdateAdminUserCommand, bool>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IAuditLogger _auditLogger;

    public UpdateAdminUserCommandHandler(
        IAdminUserRepository adminUserRepository,
        IAuditLogger auditLogger)
    {
        _adminUserRepository = adminUserRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(UpdateAdminUserCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AdminRole>(request.Role, true, out var role))
        {
            return false;
        }

        var user = await _adminUserRepository.GetByIdForUpdateAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.UpdateProfile(request.FullName, role);
        await _adminUserRepository.UpdateAsync(user, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "AdminUserUpdated",
            "AdminUser",
            user.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record SuspendAdminUserCommand(Guid UserId) : IRequest<bool>;

public sealed class SuspendAdminUserCommandHandler : IRequestHandler<SuspendAdminUserCommand, bool>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IAuditLogger _auditLogger;

    public SuspendAdminUserCommandHandler(
        IAdminUserRepository adminUserRepository,
        IAuditLogger auditLogger)
    {
        _adminUserRepository = adminUserRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(SuspendAdminUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _adminUserRepository.GetByIdForUpdateAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.Suspend();
        await _adminUserRepository.UpdateAsync(user, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "AdminUserSuspended",
            "AdminUser",
            user.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record ActivateAdminUserCommand(Guid UserId) : IRequest<bool>;

public sealed class ActivateAdminUserCommandHandler : IRequestHandler<ActivateAdminUserCommand, bool>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IAuditLogger _auditLogger;

    public ActivateAdminUserCommandHandler(
        IAdminUserRepository adminUserRepository,
        IAuditLogger auditLogger)
    {
        _adminUserRepository = adminUserRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(ActivateAdminUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _adminUserRepository.GetByIdForUpdateAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.Activate();
        await _adminUserRepository.UpdateAsync(user, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "AdminUserActivated",
            "AdminUser",
            user.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}
