using BFA.BuildingBlocks.Application;
using BFA.Modules.Warehouse.Domain.Aggregates;
using BFA.Modules.Warehouse.Domain.Enums;
using BFA.Modules.Warehouse.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Warehouse;

public record CreateConsolidationCommand(
    Guid CustomerOrderId,
    IReadOnlyList<Guid> InboundShipmentIds) : IRequest<CreateConsolidationResult?>;

public record CreateConsolidationResult(Guid ConsolidationId, string ReferenceNumber);

public sealed class CreateConsolidationCommandHandler
    : IRequestHandler<CreateConsolidationCommand, CreateConsolidationResult?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInboundShipmentRepository _inboundShipmentRepository;
    private readonly IConsolidationRepository _consolidationRepository;

    public CreateConsolidationCommandHandler(
        IUnitOfWork unitOfWork,
        IInboundShipmentRepository inboundShipmentRepository,
        IConsolidationRepository consolidationRepository)
    {
        _unitOfWork = unitOfWork;
        _inboundShipmentRepository = inboundShipmentRepository;
        _consolidationRepository = consolidationRepository;
    }

    public async Task<CreateConsolidationResult?> Handle(
        CreateConsolidationCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _consolidationRepository.GetByCustomerOrderIdAsync(
            request.CustomerOrderId,
            cancellationToken);
        if (existing is not null)
        {
            return null;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var shipments = new List<InboundShipment>();
            foreach (var shipmentId in request.InboundShipmentIds.Distinct())
            {
                var shipment = await _inboundShipmentRepository.GetByIdForUpdateAsync(
                    shipmentId,
                    cancellationToken);
                if (shipment is null
                    || shipment.CustomerOrderId != request.CustomerOrderId
                    || shipment.Status != InboundShipmentStatus.Received
                    || shipment.ConsolidationId.HasValue)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return null;
                }

                shipments.Add(shipment);
            }

            var consolidation = Consolidation.Create(
                request.CustomerOrderId,
                shipments.Select(shipment => shipment.Id).ToList());

            foreach (var shipment in shipments)
            {
                shipment.AssignToConsolidation(consolidation.Id);
                await _inboundShipmentRepository.UpdateAsync(shipment, cancellationToken);
            }

            await _consolidationRepository.AddAsync(consolidation, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new CreateConsolidationResult(
                consolidation.Id,
                consolidation.ReferenceNumber);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
