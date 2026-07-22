using BFA.Modules.Warehouse.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Warehouse;

public record ReceiveInboundShipmentCommand(
    Guid InboundShipmentId,
    string ScanReference,
    decimal WeightKg,
    string? InspectionNotes,
    string? PhotoUrl,
    string ReceivedBy) : IRequest<bool>;

public sealed class ReceiveInboundShipmentCommandHandler
    : IRequestHandler<ReceiveInboundShipmentCommand, bool>
{
    private readonly IInboundShipmentRepository _inboundShipmentRepository;

    public ReceiveInboundShipmentCommandHandler(
        IInboundShipmentRepository inboundShipmentRepository)
    {
        _inboundShipmentRepository = inboundShipmentRepository;
    }

    public async Task<bool> Handle(
        ReceiveInboundShipmentCommand request,
        CancellationToken cancellationToken)
    {
        var shipment = await _inboundShipmentRepository.GetByIdForUpdateAsync(
            request.InboundShipmentId,
            cancellationToken);
        if (shipment is null)
        {
            return false;
        }

        shipment.Receive(
            request.ScanReference,
            request.WeightKg,
            request.InspectionNotes,
            request.PhotoUrl,
            request.ReceivedBy);

        await _inboundShipmentRepository.UpdateAsync(shipment, cancellationToken);
        return true;
    }
}
