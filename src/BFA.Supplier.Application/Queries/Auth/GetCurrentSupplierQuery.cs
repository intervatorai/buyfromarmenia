using BFA.Modules.Identity.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Queries.Auth;

public record GetCurrentSupplierQuery(Guid UserId) : IRequest<CurrentSupplierDto?>;

public record CurrentSupplierDto(
    Guid UserId,
    Guid SupplierId,
    string Email,
    string FullName,
    string TradingName,
    string Role);

public sealed class GetCurrentSupplierQueryHandler
    : IRequestHandler<GetCurrentSupplierQuery, CurrentSupplierDto?>
{
    private readonly ISupplierMemberRepository _supplierMemberRepository;
    private readonly IUserRepository _userRepository;

    public GetCurrentSupplierQueryHandler(
        ISupplierMemberRepository supplierMemberRepository,
        IUserRepository userRepository)
    {
        _supplierMemberRepository = supplierMemberRepository;
        _userRepository = userRepository;
    }

    public async Task<CurrentSupplierDto?> Handle(
        GetCurrentSupplierQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var member = await _supplierMemberRepository.GetActiveByUserIdAsync(
            request.UserId,
            cancellationToken);

        if (member?.UserId is null)
        {
            return null;
        }

        return new CurrentSupplierDto(
            user.Id,
            member.SupplierId,
            user.Email,
            member.FullName,
            member.TradingName,
            member.Role.ToString());
    }
}
