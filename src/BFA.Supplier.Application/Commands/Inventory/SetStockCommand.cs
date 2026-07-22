using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Inventory.Domain.Aggregates;
using BFA.Modules.Inventory.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Inventory;

public record SetStockCommand(
    Guid SupplierId,
    Guid ProductId,
    Guid ProductVariantId,
    int OnHand,
    int LowStockThreshold) : IRequest<bool>;

public sealed class SetStockCommandHandler
    : IRequestHandler<SetStockCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IStockItemRepository _stockItemRepository;

    public SetStockCommandHandler(
        IProductRepository productRepository,
        IStockItemRepository stockItemRepository)
    {
        _productRepository = productRepository;
        _stockItemRepository = stockItemRepository;
    }

    public async Task<bool> Handle(
        SetStockCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);

        if (product is null
            || product.SupplierId != request.SupplierId
            || product.Variants.All(variant => variant.Id != request.ProductVariantId))
        {
            return false;
        }

        var stockItem = await _stockItemRepository.GetByVariantIdForUpdateAsync(
            request.ProductVariantId,
            cancellationToken);

        if (stockItem is null)
        {
            stockItem = new StockItem(
                request.SupplierId,
                request.ProductId,
                request.ProductVariantId,
                request.OnHand,
                request.LowStockThreshold);
            await _stockItemRepository.AddAsync(stockItem, cancellationToken);
        }
        else
        {
            if (stockItem.SupplierId != request.SupplierId)
            {
                return false;
            }

            stockItem.SetOnHand(request.OnHand, request.LowStockThreshold);
            await _stockItemRepository.UpdateAsync(stockItem, cancellationToken);
        }

        return true;
    }
}
