using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Compliance.Domain.Services;
using BFA.Modules.Fulfillment.Domain.Aggregates;
using BFA.Modules.Fulfillment.Domain.Repositories;
using BFA.Modules.Inventory.Domain.Repositories;
using BFA.Modules.Identity.Domain.Repositories;
using BFA.Modules.Ordering.Domain.Aggregates;
using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Payments.Domain.Aggregates;
using BFA.Modules.Payments.Domain.Enums;
using BFA.Modules.Payments.Domain.Repositories;
using BFA.Modules.Shopping.Domain.Repositories;
using BFA.Public.Application.Services.Payments;
using BFA.Public.Application.Services.Shipping;
using BFA.Public.Application.Services.Shopping;
using MediatR;

namespace BFA.Public.Application.Commands.Orders;

public record PlaceOrderCommand(
    Guid CartId,
    string CustomerEmail,
    string CustomerFullName,
    Guid DeliveryAddressId,
    Guid CustomerUserId) : IRequest<PlaceOrderOutcome>;

public abstract record PlaceOrderOutcome;

public sealed record PlaceOrderSuccess(
    Guid OrderId,
    string OrderNumber,
    string? CheckoutUrl = null) : PlaceOrderOutcome;

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
    private readonly IShippingQuoteService _shippingQuoteService;
    private readonly IStripeCheckoutService _stripeCheckoutService;

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
        ICustomerDeliveryAddressRepository deliveryAddressRepository,
        IShippingQuoteService shippingQuoteService,
        IStripeCheckoutService stripeCheckoutService)
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
        _shippingQuoteService = shippingQuoteService;
        _stripeCheckoutService = stripeCheckoutService;
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

        var removed = await CartCatalogSanitizer.RemoveUnavailableItemsAsync(
            cart,
            _productRepository,
            cancellationToken);
        if (removed > 0)
        {
            await _cartRepository.UpdateAsync(cart, cancellationToken);
        }

        if (cart.Items.Count == 0)
        {
            return new PlaceOrderFailed();
        }

        var itemDrafts = new List<CustomerOrderItemDraft>();
        var productIds = new List<Guid>();
        var productsById = new Dictionary<Guid, Product>();
        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(
                cartItem.ProductId,
                cancellationToken);
            var variant = product?.Variants.FirstOrDefault(v =>
                v.Id == cartItem.ProductVariantId);
            if (product is null || variant is null)
            {
                return new PlaceOrderFailed();
            }

            productsById[product.Id] = product;
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

        Modules.Shipping.Domain.Services.ShippingQuoteResult shippingQuote;
        try
        {
            var estimatedWeight = CartShippingWeightEstimator.EstimateWeightKg(cart, productsById);
            shippingQuote = await _shippingQuoteService.QuoteAsync(
                shippingAddress.CountryCode,
                estimatedWeight,
                cancellationToken);
        }
        catch (DomainException)
        {
            return new PlaceOrderFailed();
        }

        var useStripe = _stripeCheckoutService.IsEnabled;
        CustomerOrder order;
        Payment payment;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var orderNumber = GenerateOrderNumber();
            order = new CustomerOrder(
                request.CartId,
                orderNumber,
                request.CustomerEmail,
                request.CustomerFullName,
                shippingAddress,
                itemDrafts,
                shippingQuote.EstimatedWeightKg,
                shippingQuote.ShippingFee,
                shippingQuote.ErrorMarginPercent,
                shippingQuote.ShippingFee,
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

                stock.Reserve(
                    order.Id,
                    item.Quantity,
                    DateTime.UtcNow.AddHours(1));

                if (!useStripe)
                {
                    var reservation = stock.FindActiveReservationByReference(order.Id)
                        ?? throw new DomainException("Stock reservation was not created.");
                    stock.ConfirmReservation(reservation.Id);
                }

                await _stockItemRepository.UpdateAsync(stock, cancellationToken);
            }

            payment = new Payment(
                order.Id,
                order.Total,
                order.Currency,
                useStripe ? PaymentProvider.Stripe : PaymentProvider.Stub);

            if (!useStripe)
            {
                payment.Capture();
                order.MarkPaymentPaid();
                order.MarkConfirmed();

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

            await _customerOrderRepository.AddAsync(order, cancellationToken);
            await _paymentRepository.AddAsync(payment, cancellationToken);

            cart.Clear();
            await _cartRepository.UpdateAsync(cart, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        if (!useStripe)
        {
            await _outboxStore.EnqueueAsync(
                IntegrationEventTypes.OrderPlaced,
                $"{{\"orderId\":\"{order.Id}\"}}",
                cancellationToken);
            return new PlaceOrderSuccess(order.Id, order.OrderNumber);
        }

        var lineItems = order.Items
            .Select(item => new StripeCheckoutLineItem(
                item.ProductName,
                item.UnitPrice,
                item.Quantity))
            .ToList();
        if (order.ShippingFee > 0)
        {
            lineItems.Add(new StripeCheckoutLineItem("Shipping", order.ShippingFee, 1));
        }

        var session = await _stripeCheckoutService.CreateCheckoutSessionAsync(
            new StripeCheckoutSessionRequest(
                order.Id,
                payment.Id,
                order.OrderNumber,
                order.CustomerEmail,
                order.Total,
                order.Currency,
                lineItems),
            cancellationToken);

        var trackedPayment = await _paymentRepository.GetByCustomerOrderIdForUpdateAsync(
            order.Id,
            cancellationToken)
            ?? payment;
        trackedPayment.AttachExternalReference(session.SessionId);
        await _paymentRepository.UpdateAsync(trackedPayment, cancellationToken);

        return new PlaceOrderSuccess(order.Id, order.OrderNumber, session.CheckoutUrl);
    }

    private static string GenerateOrderNumber()
    {
        return $"BFA-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
    }
}
