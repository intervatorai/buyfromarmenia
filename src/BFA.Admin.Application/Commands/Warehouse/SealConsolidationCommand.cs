using BFA.Modules.Warehouse.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Warehouse;

public record SealConsolidationCommand(Guid ConsolidationId) : IRequest<bool>;

public sealed class SealConsolidationCommandHandler
    : IRequestHandler<SealConsolidationCommand, bool>
{
    private readonly IConsolidationRepository _consolidationRepository;

    public SealConsolidationCommandHandler(IConsolidationRepository consolidationRepository)
    {
        _consolidationRepository = consolidationRepository;
    }

    public async Task<bool> Handle(
        SealConsolidationCommand request,
        CancellationToken cancellationToken)
    {
        var consolidation = await _consolidationRepository.GetByIdForUpdateAsync(
            request.ConsolidationId,
            cancellationToken);
        if (consolidation is null)
        {
            return false;
        }

        consolidation.Seal();
        await _consolidationRepository.UpdateAsync(consolidation, cancellationToken);
        return true;
    }
}
