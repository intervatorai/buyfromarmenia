using BFA.Modules.Warehouse.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Warehouse;

public record MarkInboundShipmentArrivedCommand(Guid InboundShipmentId) : IRequest<bool>;

public sealed class MarkInboundShipmentArrivedCommandHandler
    : IRequestHandler<MarkInboundShipmentArrivedCommand, bool>
{
    private readonly IInboundShipmentRepository _inboundShipmentRepository;

    public MarkInboundShipmentArrivedCommandHandler(
        IInboundShipmentRepository inboundShipmentRepository)
    {
        _inboundShipmentRepository = inboundShipmentRepository;
    }

    public async Task<bool> Handle(
        MarkInboundShipmentArrivedCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await _inboundShipmentRepository.GetByIdForUpdateAsync(
            request.InboundShipmentId,
            cancellationToken);
        if (shipment is null)
        {
            return false;
        }

        shipment.MarkArrived();
        await _inboundShipmentRepository.UpdateAsync(shipment, cancellationToken);
        return true;
    }
}
