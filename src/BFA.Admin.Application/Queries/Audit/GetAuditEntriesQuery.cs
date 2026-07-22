using BFA.BuildingBlocks.Application;
using MediatR;

namespace BFA.Admin.Application.Queries.Audit;

public record GetAuditEntriesQuery(int Take = 100) : IRequest<IReadOnlyList<AuditEntryDto>>;

public sealed class GetAuditEntriesQueryHandler
    : IRequestHandler<GetAuditEntriesQuery, IReadOnlyList<AuditEntryDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditEntriesQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public Task<IReadOnlyList<AuditEntryDto>> Handle(
        GetAuditEntriesQuery request,
        CancellationToken cancellationToken)
    {
        return _auditLogRepository.GetRecentAsync(request.Take, cancellationToken);
    }
}
