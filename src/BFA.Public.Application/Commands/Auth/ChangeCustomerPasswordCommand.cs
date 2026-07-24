using BFA.Modules.Identity.Domain.Auth;
using BFA.Modules.Identity.Domain.Enums;
using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Commands.Auth;

public record ChangeCustomerPasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : IRequest<ChangePasswordResult>;

public record ChangePasswordResult(bool Success, string? Error = null);

public sealed class ChangeCustomerPasswordCommandHandler
    : IRequestHandler<ChangeCustomerPasswordCommand, ChangePasswordResult>
{
    private const int MinPasswordLength = 6;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangeCustomerPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ChangePasswordResult> Handle(
        ChangeCustomerPasswordCommand request,
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
        if (user is null || user.Type != UserType.Customer || user.Status != UserStatus.Active)
        {
            return new ChangePasswordResult(false, "Customer account not found.");
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
