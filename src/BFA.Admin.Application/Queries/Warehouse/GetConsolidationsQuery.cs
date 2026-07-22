using BFA.Modules.Warehouse.Domain.Enums;
using BFA.Modules.Warehouse.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Warehouse;

public record GetConsolidationsQuery(string? Status = null)
    : IRequest<IReadOnlyList<ConsolidationListItemDto>>;

public record ConsolidationListItemDto(
    Guid Id,
    string ReferenceNumber,
    Guid CustomerOrderId,
    string Status,
    int InboundShipmentsCount,
    int PackagesCount,
    decimal TotalWeightKg,
    DateTime CreatedAtUtc);

public sealed class GetConsolidationsQueryHandler
    : IRequestHandler<GetConsolidationsQuery, IReadOnlyList<ConsolidationListItemDto>>
{
    private readonly IConsolidationRepository _consolidationRepository;

    public GetConsolidationsQueryHandler(IConsolidationRepository consolidationRepository)
    {
        _consolidationRepository = consolidationRepository;
    }

    public async Task<IReadOnlyList<ConsolidationListItemDto>> Handle(
        GetConsolidationsQuery request,
        CancellationToken cancellationToken)
    {
        ConsolidationStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<ConsolidationStatus>(request.Status, out var parsedStatus))
        {
            status = parsedStatus;
        }

        var consolidations = await _consolidationRepository.GetAllAsync(status, cancellationToken);

        return consolidations.Select(consolidation => new ConsolidationListItemDto(
            consolidation.Id,
            consolidation.ReferenceNumber,
            consolidation.CustomerOrderId,
            consolidation.Status.ToString(),
            consolidation.InboundShipmentIds.Count,
            consolidation.Packages.Count,
            consolidation.Packages.Sum(package => package.WeightKg),
            consolidation.CreatedAtUtc)).ToList();
    }
}
