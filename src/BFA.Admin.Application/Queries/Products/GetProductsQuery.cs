using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Products;

public record GetProductsQuery(string? Status = null) : IRequest<IReadOnlyList<AdminProductDto>>;

public record AdminProductDto(
    Guid Id,
    Guid SupplierId,
    string Name,
    string ShortDescription,
    string Description,
    decimal Price,
    string Currency,
    string Status,
    string Tag,
    Guid? CategoryId,
    int VariantsCount,
    string? PrimaryImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<AdminProductDto>>
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

    public async Task<IReadOnlyList<AdminProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = string.IsNullOrWhiteSpace(request.Status)
            ? await _productRepository.GetAllAsync(cancellationToken)
            : await _productRepository.GetByStatusAsync(
                Enum.Parse<ProductStatus>(request.Status, true),
                cancellationToken);

        return products
            .Select(product =>
            {
                var (name, description, shortDescription) = ProductDisplayHelper.GetDisplayText(product);
                var primaryImage = product.Media.FirstOrDefault(m => m.IsPrimary)
                    ?? product.Media.OrderBy(m => m.SortOrder).FirstOrDefault();

                return new AdminProductDto(
                    product.Id,
                    product.SupplierId,
                    name,
                    shortDescription,
                    description,
                    product.BasePrice.Amount,
                    product.BasePrice.Currency,
                    product.Status.ToString(),
                    product.Tag.ToString(),
                    product.CategoryId,
                    product.Variants.Count,
                    primaryImage is null
                        ? null
                        : _mediaUrlResolver.Resolve(primaryImage.MediaAsset.StorageKey),
                    product.CreatedAt,
                    product.UpdatedAt);
            })
            .ToList();
    }
}
