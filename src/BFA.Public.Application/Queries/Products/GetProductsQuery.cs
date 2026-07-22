using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Queries.Products;

public record GetProductsQuery(
    Guid? CategoryId = null,
    string? CategorySlug = null,
    string? Search = null,
    string? Tag = null,
    bool FeaturedOnly = false,
    int? Take = null,
    string? Language = null) : IRequest<IReadOnlyList<PublicProductDto>>;

public record PublicProductDto(
    Guid Id,
    string Slug,
    string Name,
    string ShortDescription,
    decimal Price,
    string Currency,
    string? PrimaryImageUrl,
    Guid? CategoryId,
    string Tag);

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<PublicProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMediaUrlResolver _mediaUrlResolver;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IMediaUrlResolver mediaUrlResolver)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mediaUrlResolver = mediaUrlResolver;
    }

    public async Task<IReadOnlyList<PublicProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        Guid? categoryId = request.CategoryId;
        if (!categoryId.HasValue && !string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            var slug = request.CategorySlug.Trim();
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            var matched = categories.FirstOrDefault(category =>
                category.Translations.Any(translation =>
                    string.Equals(translation.Slug, slug, StringComparison.OrdinalIgnoreCase)));

            categoryId = matched?.Id;
            if (!categoryId.HasValue)
            {
                return [];
            }
        }

        ProductTag? tagFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Tag)
            && Enum.TryParse<ProductTag>(request.Tag, ignoreCase: true, out var parsedTag)
            && parsedTag != ProductTag.None)
        {
            tagFilter = parsedTag;
        }

        var products = await _productRepository.SearchPublishedAsync(
            new ProductSearchCriteria(
                categoryId,
                request.Search,
                tagFilter,
                request.FeaturedOnly && tagFilter is null,
                request.Take),
            cancellationToken);

        return products.Select(product => MapListItem(product, request.Language, _mediaUrlResolver)).ToList();
    }

    internal static PublicProductDto MapListItem(
        BFA.Modules.Catalog.Domain.Aggregates.Product product,
        string? language,
        IMediaUrlResolver mediaUrlResolver)
    {
        var (name, _, shortDescription) = ProductDisplayHelper.GetDisplayText(product, language);
        var primaryImage = product.Media.FirstOrDefault(m => m.IsPrimary)
            ?? product.Media.OrderBy(m => m.SortOrder).FirstOrDefault();

        return new PublicProductDto(
            product.Id,
            product.Slug,
            name,
            shortDescription,
            product.BasePrice.Amount,
            product.BasePrice.Currency,
            primaryImage is null ? null : mediaUrlResolver.Resolve(primaryImage.MediaAsset.StorageKey),
            product.CategoryId,
            product.Tag.ToString());
    }
}
