using BFA.Modules.Warehouse.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Warehouse;

public record AddPackageToConsolidationCommand(
    Guid ConsolidationId,
    decimal WeightKg,
    string? Notes) : IRequest<bool>;

public sealed class AddPackageToConsolidationCommandHandler
    : IRequestHandler<AddPackageToConsolidationCommand, bool>
{
    private readonly IConsolidationRepository _consolidationRepository;

    public AddPackageToConsolidationCommandHandler(
        IConsolidationRepository consolidationRepository)
    {
        _consolidationRepository = consolidationRepository;
    }

    public async Task<bool> Handle(
        AddPackageToConsolidationCommand request,
        CancellationToken cancellationToken)
    {
        var consolidation = await _consolidationRepository.GetByIdForUpdateAsync(
            request.ConsolidationId,
            cancellationToken);
        if (consolidation is null)
        {
            return false;
        }

        consolidation.AddPackage(request.WeightKg, request.Notes);
        await _consolidationRepository.UpdateAsync(consolidation, cancellationToken);
        return true;
    }
}
