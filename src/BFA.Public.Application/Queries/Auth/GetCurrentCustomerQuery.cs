using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Queries.Auth;

public record GetCurrentCustomerQuery(Guid UserId) : IRequest<CurrentCustomerDto?>;

public record CurrentCustomerDto(
    Guid UserId,
    string Email,
    string FullName,
    string? Phone);

public sealed class GetCurrentCustomerQueryHandler
    : IRequestHandler<GetCurrentCustomerQuery, CurrentCustomerDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerProfileRepository _profileRepository;

    public GetCurrentCustomerQueryHandler(
        IUserRepository userRepository,
        ICustomerProfileRepository profileRepository)
    {
        _userRepository = userRepository;
        _profileRepository = profileRepository;
    }

    public async Task<CurrentCustomerDto?> Handle(
        GetCurrentCustomerQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var profile = await _profileRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        return new CurrentCustomerDto(
            user.Id,
            user.Email,
            profile.FullName,
            profile.Phone);
    }
}
