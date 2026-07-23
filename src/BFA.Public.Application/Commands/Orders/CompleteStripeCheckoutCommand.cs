using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Fulfillment.Domain.Aggregates;
using BFA.Modules.Fulfillment.Domain.Repositories;
using BFA.Modules.Inventory.Domain.Repositories;
using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Payments.Domain.Enums;
using BFA.Modules.Payments.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Commands.Orders;

public record CompleteStripeCheckoutCommand(
    string EventId,
    string EventType,
    string? SessionId,
    Guid? OrderId,
    Guid? PaymentId,
    string? PaymentIntentId) : IRequest<bool>;

public sealed class CompleteStripeCheckoutCommandHandler
    : IRequestHandler<CompleteStripeCheckoutCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerOrderRepository _customerOrderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISupplierOrderRepository _supplierOrderRepository;
    private readonly IStockItemRepository _stockItemRepository;
    private readonly IOutboxStore _outboxStore;

    public CompleteStripeCheckoutCommandHandler(
        IUnitOfWork unitOfWork,
        ICustomerOrderRepository customerOrderRepository,
        IPaymentRepository paymentRepository,
        ISupplierOrderRepository supplierOrderRepository,
        IStockItemRepository stockItemRepository,
        IOutboxStore outboxStore)
    {
        _unitOfWork = unitOfWork;
        _customerOrderRepository = customerOrderRepository;
        _paymentRepository = paymentRepository;
        _supplierOrderRepository = supplierOrderRepository;
        _stockItemRepository = stockItemRepository;
        _outboxStore = outboxStore;
    }

    public async Task<bool> Handle(
        CompleteStripeCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(request.EventType, "checkout.session.completed", StringComparison.Ordinal))
        {
            return false;
        }

        var payment = !string.IsNullOrWhiteSpace(request.SessionId)
            ? await _paymentRepository.GetByExternalReferenceForUpdateAsync(
                request.SessionId,
                cancellationToken)
            : null;

        if (payment is null && request.OrderId.HasValue)
        {
            payment = await _paymentRepository.GetByCustomerOrderIdForUpdateAsync(
                request.OrderId.Value,
                cancellationToken);
        }

        if (payment is null)
        {
            return false;
        }

        if (payment.Status == PaymentRecordStatus.Captured)
        {
            return true;
        }

        if (payment.Provider != PaymentProvider.Stripe)
        {
            return false;
        }

        var order = await _customerOrderRepository.GetByIdForUpdateAsync(
            payment.CustomerOrderId,
            cancellationToken);
        if (order is null)
        {
            return false;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var captureRef = request.PaymentIntentId
                ?? request.SessionId
                ?? payment.ExternalReference
                ?? $"stripe-{request.EventId}";

            payment.Capture(captureRef);
            order.MarkPaymentPaid();
            order.MarkConfirmed();

            foreach (var item in order.Items)
            {
                var stock = await _stockItemRepository.GetByVariantIdForUpdateAsync(
                    item.ProductVariantId,
                    cancellationToken);
                if (stock is null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw new DomainException(
                        $"Stock was not found for variant {item.ProductVariantId}.");
                }

                var reservation = stock.FindActiveReservationByReference(order.Id);
                if (reservation is not null)
                {
                    stock.ConfirmReservation(reservation.Id);
                    await _stockItemRepository.UpdateAsync(stock, cancellationToken);
                }
            }

            var existingSupplierOrders = await _supplierOrderRepository.GetByCustomerOrderIdAsync(
                order.Id,
                cancellationToken);
            if (existingSupplierOrders.Count == 0)
            {
                foreach (var group in order.Items.GroupBy(item => item.SupplierId))
                {
                    var supplierOrder = new SupplierOrder(
                        order.Id,
                        group.Key,
                        order.Currency,
                        group.Select(item => new SupplierOrderItemDraft(
                            item.ProductId,
                            item.ProductVariantId,
                            item.ProductName,
                            item.SupplierSku,
                            item.UnitPrice,
                            item.Currency,
                            item.Quantity)).ToList());

                    await _supplierOrderRepository.AddAsync(supplierOrder, cancellationToken);
                }
            }

            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _customerOrderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _outboxStore.EnqueueAsync(
                IntegrationEventTypes.OrderPlaced,
                $"{{\"orderId\":\"{order.Id}\"}}",
                cancellationToken);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
