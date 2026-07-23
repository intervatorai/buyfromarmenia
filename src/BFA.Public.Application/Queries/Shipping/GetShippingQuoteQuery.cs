using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Identity.Domain.Repositories;
using BFA.Modules.Shopping.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Services;
using BFA.Public.Application.Services.Shipping;
using BFA.Public.Application.Services.Shopping;
using MediatR;

namespace BFA.Public.Application.Queries.Shipping;

public record GetShippingQuoteQuery(
    Guid CartId,
    Guid DeliveryAddressId,
    Guid CustomerUserId) : IRequest<ShippingQuoteResult>;

public sealed class GetShippingQuoteQueryHandler
    : IRequestHandler<GetShippingQuoteQuery, ShippingQuoteResult>
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerDeliveryAddressRepository _deliveryAddressRepository;
    private readonly IShippingQuoteService _shippingQuoteService;

    public GetShippingQuoteQueryHandler(
        IShoppingCartRepository cartRepository,
        IProductRepository productRepository,
        ICustomerDeliveryAddressRepository deliveryAddressRepository,
        IShippingQuoteService shippingQuoteService)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _deliveryAddressRepository = deliveryAddressRepository;
        _shippingQuoteService = shippingQuoteService;
    }

    public async Task<ShippingQuoteResult> Handle(
        GetShippingQuoteQuery request,
        CancellationToken cancellationToken)
    {
        var address = await _deliveryAddressRepository.GetByIdForUserAsync(
            request.DeliveryAddressId,
            request.CustomerUserId,
            cancellationToken)
            ?? throw new DomainException("Delivery address was not found.");

        var cart = await _cartRepository.GetByIdForUpdateAsync(request.CartId, cancellationToken);
        if (cart is null)
        {
            throw new DomainException("Cart is empty.");
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
            throw new DomainException(
                removed > 0
                    ? "Some items are no longer available and were removed from your cart. Cart is now empty."
                    : "Cart is empty.");
        }

        var products = new Dictionary<Guid, Product>();
        foreach (var productId in cart.Items.Select(item => item.ProductId).Distinct())
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
                ?? throw new DomainException($"Product '{productId}' was not found.");
            products[productId] = product;
        }

        var weightKg = CartShippingWeightEstimator.EstimateWeightKg(cart, products);
        return await _shippingQuoteService.QuoteAsync(
            address.CountryCode,
            weightKg,
            cancellationToken);
    }
}
