using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Products;

public record AddProductVariantCommand(
    Guid SupplierId,
    Guid ProductId,
    string? SupplierSku,
    decimal Weight,
    string CountryOfOrigin,
    string? Barcode = null,
    string? Size = null,
    string? Color = null) : IRequest<bool>;

public class AddProductVariantCommandHandler : IRequestHandler<AddProductVariantCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public AddProductVariantCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<bool> Handle(AddProductVariantCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);
        if (product is null || product.SupplierId != request.SupplierId)
        {
            return false;
        }

        var sku = await ProductSkuAssigner.ResolveAsync(
            request.SupplierSku,
            product.CategoryId,
            _categoryRepository,
            _productRepository,
            cancellationToken);

        product.AddVariant(
            sku,
            request.Weight,
            request.CountryOfOrigin,
            request.Barcode,
            request.Size,
            request.Color);

        await _productRepository.UpdateAsync(product, cancellationToken);
        return true;
    }
}
