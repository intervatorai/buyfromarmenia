using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Compliance.Domain.Services;
using BFA.Modules.Fulfillment.Domain.Aggregates;
using BFA.Modules.Fulfillment.Domain.Repositories;
using BFA.Modules.Inventory.Domain.Repositories;
using BFA.Modules.Identity.Domain.Repositories;
using BFA.Modules.Ordering.Domain.Aggregates;
using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Payments.Domain.Aggregates;
using BFA.Modules.Payments.Domain.Repositories;
using BFA.Modules.Shopping.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Commands.Orders;

public record PlaceOrderCommand(
    Guid CartId,
    string CustomerEmail,
    string CustomerFullName,
    Guid DeliveryAddressId,
    Guid CustomerUserId) : IRequest<PlaceOrderOutcome>;

public abstract record PlaceOrderOutcome;

public sealed record PlaceOrderSuccess(Guid OrderId, string OrderNumber) : PlaceOrderOutcome;

public sealed record PlaceOrderComplianceBlocked(string Message) : PlaceOrderOutcome;

public sealed record PlaceOrderFailed : PlaceOrderOutcome;

public sealed class PlaceOrderCommandHandler
    : IRequestHandler<PlaceOrderCommand, PlaceOrderOutcome>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockItemRepository _stockItemRepository;
    private readonly ICustomerOrderRepository _customerOrderRepository;
    private readonly ISupplierOrderRepository _supplierOrderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOutboxStore _outboxStore;
    private readonly IExportComplianceValidator _exportComplianceValidator;
    private readonly ICustomerDeliveryAddressRepository _deliveryAddressRepository;

    public PlaceOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IShoppingCartRepository cartRepository,
        IProductRepository productRepository,
        IStockItemRepository stockItemRepository,
        ICustomerOrderRepository customerOrderRepository,
        ISupplierOrderRepository supplierOrderRepository,
        IPaymentRepository paymentRepository,
        IOutboxStore outboxStore,
        IExportComplianceValidator exportComplianceValidator,
        ICustomerDeliveryAddressRepository deliveryAddressRepository)
    {
        _unitOfWork = unitOfWork;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _stockItemRepository = stockItemRepository;
        _customerOrderRepository = customerOrderRepository;
        _supplierOrderRepository = supplierOrderRepository;
        _paymentRepository = paymentRepository;
        _outboxStore = outboxStore;
        _exportComplianceValidator = exportComplianceValidator;
        _deliveryAddressRepository = deliveryAddressRepository;
    }

    public async Task<PlaceOrderOutcome> Handle(
        PlaceOrderCommand request,
        CancellationToken cancellationToken)
    {
        var deliveryAddress = await _deliveryAddressRepository.GetByIdForUserAsync(
            request.DeliveryAddressId,
            request.CustomerUserId,
            cancellationToken);
        if (deliveryAddress is null)
        {
            return new PlaceOrderFailed();
        }

        var shippingAddress = deliveryAddress.ToAddress();

        var cart = await _cartRepository.GetByIdForUpdateAsync(
            request.CartId,
            cancellationToken);
        if (cart is null || cart.Items.Count == 0)
        {
            return new PlaceOrderFailed();
        }

        var itemDrafts = new List<CustomerOrderItemDraft>();
        var productIds = new List<Guid>();
        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(
                cartItem.ProductId,
                cancellationToken);
            var variant = product?.Variants.FirstOrDefault(v =>
                v.Id == cartItem.ProductVariantId);
            if (variant is null)
            {
                return new PlaceOrderFailed();
            }

            productIds.Add(cartItem.ProductId);

            var stock = await _stockItemRepository.GetByVariantIdAsync(
                cartItem.ProductVariantId,
                cancellationToken);
            if (stock is null || stock.Available < cartItem.Quantity)
            {
                return new PlaceOrderFailed();
            }

            itemDrafts.Add(new CustomerOrderItemDraft(
                cartItem.ProductId,
                cartItem.ProductVariantId,
                cartItem.SupplierId,
                cartItem.ProductName,
                variant.SupplierSku,
                cartItem.ImageUrl,
                cartItem.UnitPrice,
                cartItem.Currency,
                cartItem.Quantity));
        }

        var compliance = await _exportComplianceValidator.ValidateAsync(
            shippingAddress.CountryCode,
            productIds,
            cancellationToken);

        if (!compliance.IsAllowed)
        {
            var message = compliance.Violations.FirstOrDefault()?.Reason
                ?? "Export to the selected destination is restricted.";

            return new PlaceOrderComplianceBlocked(message);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var orderNumber = GenerateOrderNumber();
            var order = new CustomerOrder(
                request.CartId,
                orderNumber,
                request.CustomerEmail,
                request.CustomerFullName,
                shippingAddress,
                itemDrafts,
                request.CustomerUserId);

            foreach (var item in order.Items)
            {
                var stock = await _stockItemRepository.GetByVariantIdForUpdateAsync(
                    item.ProductVariantId,
                    cancellationToken);
                if (stock is null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return new PlaceOrderFailed();
                }

                var reservation = stock.Reserve(
                    order.Id,
                    item.Quantity,
                    DateTime.UtcNow.AddHours(1));
                stock.ConfirmReservation(reservation.Id);
                await _stockItemRepository.UpdateAsync(stock, cancellationToken);
            }

            var payment = new Payment(order.Id, order.Subtotal, order.Currency);
            payment.Capture();
            order.MarkPaymentPaid();
            order.MarkConfirmed();

            await _customerOrderRepository.AddAsync(order, cancellationToken);
            await _paymentRepository.AddAsync(payment, cancellationToken);

            var supplierGroups = order.Items.GroupBy(item => item.SupplierId);
            foreach (var group in supplierGroups)
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

            cart.Clear();
            await _cartRepository.UpdateAsync(cart, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _outboxStore.EnqueueAsync(
                IntegrationEventTypes.OrderPlaced,
                $"{{\"orderId\":\"{order.Id}\"}}",
                cancellationToken);

            return new PlaceOrderSuccess(order.Id, order.OrderNumber);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static string GenerateOrderNumber()
    {
        return $"BFA-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
    }
}
