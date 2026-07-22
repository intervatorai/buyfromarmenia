using BFA.Modules.Returns.Domain.Enums;
using BFA.Modules.Returns.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Returns;

public record GetReturnRequestsQuery(string? Status = null) : IRequest<IReadOnlyList<ReturnRequestListItemDto>>;

public record ReturnRequestListItemDto(
    Guid Id,
    Guid CustomerOrderId,
    string CustomerEmail,
    string Reason,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? ResolvedAtUtc);

public sealed class GetReturnRequestsQueryHandler
    : IRequestHandler<GetReturnRequestsQuery, IReadOnlyList<ReturnRequestListItemDto>>
{
    private readonly IReturnRequestRepository _returnRequestRepository;

    public GetReturnRequestsQueryHandler(IReturnRequestRepository returnRequestRepository)
    {
        _returnRequestRepository = returnRequestRepository;
    }

    public async Task<IReadOnlyList<ReturnRequestListItemDto>> Handle(
        GetReturnRequestsQuery request,
        CancellationToken cancellationToken)
    {
        ReturnRequestStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<ReturnRequestStatus>(request.Status, true, out var parsed))
        {
            status = parsed;
        }

        var returns = await _returnRequestRepository.GetByStatusAsync(status, cancellationToken);

        return returns
            .Select(item => new ReturnRequestListItemDto(
                item.Id,
                item.CustomerOrderId,
                item.CustomerEmail,
                item.Reason,
                item.Status.ToString(),
                item.CreatedAtUtc,
                item.ResolvedAtUtc))
            .ToList();
    }
}
