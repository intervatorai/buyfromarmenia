using BFA.Modules.Identity.Domain.Auth;
using BFA.Modules.Identity.Domain.Enums;
using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Auth;

public record ChangeSupplierPasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : IRequest<ChangePasswordResult>;

public record ChangePasswordResult(bool Success, string? Error = null);

public sealed class ChangeSupplierPasswordCommandHandler
    : IRequestHandler<ChangeSupplierPasswordCommand, ChangePasswordResult>
{
    private const int MinPasswordLength = 6;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangeSupplierPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ChangePasswordResult> Handle(
        ChangeSupplierPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword)
            || request.NewPassword.Length < MinPasswordLength)
        {
            return new ChangePasswordResult(
                false,
                $"New password must be at least {MinPasswordLength} characters.");
        }

        var user = await _userRepository.GetByIdForUpdateAsync(request.UserId, cancellationToken);
        if (user is null || user.Type != UserType.Supplier || user.Status != UserStatus.Active)
        {
            return new ChangePasswordResult(false, "Supplier account not found.");
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return new ChangePasswordResult(false, "Current password is incorrect.");
        }

        if (_passwordHasher.Verify(request.NewPassword, user.PasswordHash))
        {
            return new ChangePasswordResult(
                false,
                "New password must be different from the current password.");
        }

        user.SetPasswordHash(_passwordHasher.Hash(request.NewPassword));
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new ChangePasswordResult(true);
    }
}
