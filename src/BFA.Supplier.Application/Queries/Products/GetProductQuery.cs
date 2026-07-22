using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Queries.Products;

public record GetProductQuery(Guid SupplierId, Guid ProductId) : IRequest<ProductDetailDto?>;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDetailDto?>
{
    private readonly IProductRepository _productRepository;
    private readonly IMediaUrlResolver _mediaUrlResolver;

    public GetProductQueryHandler(
        IProductRepository productRepository,
        IMediaUrlResolver mediaUrlResolver)
    {
        _productRepository = productRepository;
        _mediaUrlResolver = mediaUrlResolver;
    }

    public async Task<ProductDetailDto?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || product.SupplierId != request.SupplierId)
        {
            return null;
        }

        return ProductMapper.ToDetail(product, _mediaUrlResolver);
    }
}
