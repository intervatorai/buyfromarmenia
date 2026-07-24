using BFA.Modules.Identity.Domain.Auth;
using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Auth;

public record ChangeAdminPasswordCommand(
    Guid AdminId,
    string CurrentPassword,
    string NewPassword) : IRequest<ChangePasswordResult>;

public record ChangePasswordResult(bool Success, string? Error = null);

public sealed class ChangeAdminPasswordCommandHandler
    : IRequestHandler<ChangeAdminPasswordCommand, ChangePasswordResult>
{
    private const int MinPasswordLength = 6;

    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangeAdminPasswordCommandHandler(
        IAdminUserRepository adminUserRepository,
        IPasswordHasher passwordHasher)
    {
        _adminUserRepository = adminUserRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ChangePasswordResult> Handle(
        ChangeAdminPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword)
            || request.NewPassword.Length < MinPasswordLength)
        {
            return new ChangePasswordResult(
                false,
                $"New password must be at least {MinPasswordLength} characters.");
        }

        var admin = await _adminUserRepository.GetByIdForUpdateAsync(
            request.AdminId,
            cancellationToken);

        if (admin is null || !admin.IsActive)
        {
            return new ChangePasswordResult(false, "Admin account not found.");
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, admin.PasswordHash))
        {
            return new ChangePasswordResult(false, "Current password is incorrect.");
        }

        if (_passwordHasher.Verify(request.NewPassword, admin.PasswordHash))
        {
            return new ChangePasswordResult(
                false,
                "New password must be different from the current password.");
        }

        admin.SetPasswordHash(_passwordHasher.Hash(request.NewPassword));
        await _adminUserRepository.UpdateAsync(admin, cancellationToken);

        return new ChangePasswordResult(true);
    }
}
