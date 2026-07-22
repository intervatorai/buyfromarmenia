using BFA.Modules.Compliance.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Compliance;

public record GetTradeRestrictionsQuery() : IRequest<IReadOnlyList<TradeRestrictionListItemDto>>;

public record TradeRestrictionListItemDto(
    Guid Id,
    string DestinationCountryCode,
    Guid? CategoryId,
    string Reason,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed class GetTradeRestrictionsQueryHandler
    : IRequestHandler<GetTradeRestrictionsQuery, IReadOnlyList<TradeRestrictionListItemDto>>
{
    private readonly ITradeRestrictionRepository _tradeRestrictionRepository;

    public GetTradeRestrictionsQueryHandler(ITradeRestrictionRepository tradeRestrictionRepository)
    {
        _tradeRestrictionRepository = tradeRestrictionRepository;
    }

    public async Task<IReadOnlyList<TradeRestrictionListItemDto>> Handle(
        GetTradeRestrictionsQuery request,
        CancellationToken cancellationToken)
    {
        var restrictions = await _tradeRestrictionRepository.GetAllAsync(cancellationToken);

        return restrictions
            .Select(restriction => new TradeRestrictionListItemDto(
                restriction.Id,
                restriction.DestinationCountryCode,
                restriction.CategoryId,
                restriction.Reason,
                restriction.IsActive,
                restriction.CreatedAtUtc))
            .ToList();
    }
}
