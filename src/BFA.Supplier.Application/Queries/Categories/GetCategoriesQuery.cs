using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Supplier.Application.Queries.Products;
using MediatR;

namespace BFA.Supplier.Application.Queries.Categories;

public record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        return categories
            .Select(category =>
            {
                var translation = category.Translations.FirstOrDefault()
                    ?? throw new InvalidOperationException("Category must have at least one translation.");

                return new CategoryDto(
                    category.Id,
                    translation.Name,
                    translation.Slug,
                    translation.Description,
                    category.ParentCategoryId,
                    category.SortOrder);
            })
            .ToList();
    }
}
