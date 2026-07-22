using BFA.Modules.Identity.Domain.Auth;
using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Auth;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult?>;

public record LoginResult(
    string AccessToken,
    DateTime ExpiresAt,
    Guid AdminId,
    string Email,
    string FullName,
    string Role);

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult?>
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IAdminUserRepository adminUserRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _adminUserRepository = adminUserRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResult?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var adminUser = await _adminUserRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (adminUser is null || !adminUser.IsActive)
        {
            return null;
        }

        if (!_passwordHasher.Verify(request.Password, adminUser.PasswordHash))
        {
            return null;
        }

        adminUser.RecordLogin();
        await _adminUserRepository.UpdateAsync(adminUser, cancellationToken);

        var token = _jwtTokenService.GenerateToken(adminUser);

        return new LoginResult(
            token.AccessToken,
            token.ExpiresAt,
            adminUser.Id,
            adminUser.Email,
            adminUser.FullName,
            adminUser.Role.ToString());
    }
}
