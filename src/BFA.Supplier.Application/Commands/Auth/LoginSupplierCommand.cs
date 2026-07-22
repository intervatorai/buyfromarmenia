using BFA.Modules.Identity.Domain.Auth;
using BFA.Modules.Identity.Domain.Enums;
using BFA.Modules.Identity.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Auth;

public record LoginSupplierCommand(string Email, string Password) : IRequest<SupplierAuthResult?>;

public record SupplierAuthResult(
    string AccessToken,
    DateTime ExpiresAt,
    Guid UserId,
    Guid SupplierId,
    string Email,
    string FullName,
    string TradingName,
    string Role);

public sealed class LoginSupplierCommandHandler
    : IRequestHandler<LoginSupplierCommand, SupplierAuthResult?>
{
    private readonly ISupplierMemberRepository _supplierMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginSupplierCommandHandler(
        ISupplierMemberRepository supplierMemberRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _supplierMemberRepository = supplierMemberRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<SupplierAuthResult?> Handle(
        LoginSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var member = await _supplierMemberRepository.GetActiveByEmailAsync(
            request.Email,
            cancellationToken);

        if (member?.UserId is null)
        {
            return null;
        }

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null
            || user.Type != UserType.Supplier
            || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        var token = _jwtTokenService.GenerateSupplierToken(
            user,
            member.SupplierId,
            member.TradingName,
            member.Role.ToString());

        return new SupplierAuthResult(
            token.AccessToken,
            token.ExpiresAt,
            user.Id,
            member.SupplierId,
            user.Email,
            member.FullName,
            member.TradingName,
            member.Role.ToString());
    }
}
