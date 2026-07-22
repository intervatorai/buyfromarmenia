using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Queries.Products;

public record GetProductsQuery(Guid SupplierId) : IRequest<IReadOnlyList<SupplierProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<SupplierProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMediaUrlResolver _mediaUrlResolver;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        IMediaUrlResolver mediaUrlResolver)
    {
        _productRepository = productRepository;
        _mediaUrlResolver = mediaUrlResolver;
    }

    public async Task<IReadOnlyList<SupplierProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetBySupplierIdAsync(request.SupplierId, cancellationToken);
        return products.Select(product => ProductMapper.ToListItem(product, _mediaUrlResolver)).ToList();
    }
}
