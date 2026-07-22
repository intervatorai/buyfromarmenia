using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Auth;
using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Commands.Auth;

public record RegisterCustomerCommand(
    string Email,
    string Password,
    string FullName,
    string? Phone) : IRequest<CustomerAuthResult?>;

public record LoginCustomerCommand(string Email, string Password) : IRequest<CustomerAuthResult?>;

public record CustomerAuthResult(
    string AccessToken,
    DateTime ExpiresAt,
    Guid UserId,
    string Email,
    string FullName,
    string? Phone);

public sealed class RegisterCustomerCommandHandler
    : IRequestHandler<RegisterCustomerCommand, CustomerAuthResult?>
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerProfileRepository _profileRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCustomerCommandHandler(
        IUserRepository userRepository,
        ICustomerProfileRepository profileRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<CustomerAuthResult?> Handle(
        RegisterCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            return null;
        }

        var user = User.RegisterCustomer(
            request.Email,
            _passwordHasher.Hash(request.Password));
        var profile = CustomerProfile.Create(user.Id, request.FullName, request.Phone);

        await _userRepository.AddAsync(user, cancellationToken);
        await _profileRepository.AddAsync(profile, cancellationToken);

        var token = _jwtTokenService.GenerateCustomerToken(user, profile);

        return new CustomerAuthResult(
            token.AccessToken,
            token.ExpiresAt,
            user.Id,
            user.Email,
            profile.FullName,
            profile.Phone);
    }
}

public sealed class LoginCustomerCommandHandler
    : IRequestHandler<LoginCustomerCommand, CustomerAuthResult?>
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerProfileRepository _profileRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCustomerCommandHandler(
        IUserRepository userRepository,
        ICustomerProfileRepository profileRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<CustomerAuthResult?> Handle(
        LoginCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var profile = await _profileRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        var token = _jwtTokenService.GenerateCustomerToken(user, profile);

        return new CustomerAuthResult(
            token.AccessToken,
            token.ExpiresAt,
            user.Id,
            user.Email,
            profile.FullName,
            profile.Phone);
    }
}
