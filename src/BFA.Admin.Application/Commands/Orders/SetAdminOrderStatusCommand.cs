using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Ordering.Domain.Enums;
using BFA.Modules.Ordering.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Orders;

public record SetAdminOrderStatusCommand(
    Guid OrderId,
    OrderStatus? OrderStatus,
    PaymentStatus? PaymentStatus) : IRequest<SetAdminOrderStatusResult?>;

public record SetAdminOrderStatusResult(
    Guid OrderId,
    string Status,
    string PaymentStatus,
    string FulfillmentStatus);

public sealed class SetAdminOrderStatusCommandHandler
    : IRequestHandler<SetAdminOrderStatusCommand, SetAdminOrderStatusResult?>
{
    private readonly ICustomerOrderRepository _orderRepository;
    private readonly IAuditLogger _auditLogger;

    public SetAdminOrderStatusCommandHandler(
        ICustomerOrderRepository orderRepository,
        IAuditLogger auditLogger)
    {
        _orderRepository = orderRepository;
        _auditLogger = auditLogger;
    }

    public async Task<SetAdminOrderStatusResult?> Handle(
        SetAdminOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.OrderStatus.HasValue && !request.PaymentStatus.HasValue)
        {
            throw new DomainException("Provide orderStatus and/or paymentStatus.");
        }

        var order = await _orderRepository.GetByIdForUpdateAsync(
            request.OrderId,
            cancellationToken);
        if (order is null)
        {
            return null;
        }

        var previousStatus = order.Status.ToString();
        var previousPayment = order.PaymentStatus.ToString();

        if (request.PaymentStatus.HasValue)
        {
            order.ChangePaymentStatusAsAdmin(request.PaymentStatus.Value);
        }

        if (request.OrderStatus.HasValue)
        {
            order.ChangeStatusAsAdmin(request.OrderStatus.Value);
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "OrderStatusUpdated",
            "CustomerOrder",
            order.Id,
            $"{{\"fromStatus\":\"{previousStatus}\",\"toStatus\":\"{order.Status}\",\"fromPayment\":\"{previousPayment}\",\"toPayment\":\"{order.PaymentStatus}\"}}",
            cancellationToken);

        return new SetAdminOrderStatusResult(
            order.Id,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.FulfillmentStatus.ToString());
    }
}
