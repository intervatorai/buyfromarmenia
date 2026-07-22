using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Catalog.Domain.ValueObjects;
using MediatR;

namespace BFA.Supplier.Application.Commands.Products;

public record SetProductShippingProfileCommand(
    Guid SupplierId,
    Guid ProductId,
    decimal NetWeight,
    decimal GrossWeight,
    decimal PackageLength,
    decimal PackageWidth,
    decimal PackageHeight,
    string PackageDimensionUnit = "cm",
    bool IsFragile = false,
    bool IsPerishable = false) : IRequest<bool>;

public class SetProductShippingProfileCommandHandler
    : IRequestHandler<SetProductShippingProfileCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public SetProductShippingProfileCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(SetProductShippingProfileCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken);
        if (product is null || product.SupplierId != request.SupplierId)
        {
            return false;
        }

        product.SetShippingProfile(new ShippingProfile(
            request.NetWeight,
            request.GrossWeight,
            request.PackageLength,
            request.PackageWidth,
            request.PackageHeight,
            request.PackageDimensionUnit,
            request.IsFragile,
            request.IsPerishable));

        await _productRepository.UpdateAsync(product, cancellationToken);
        return true;
    }
}
