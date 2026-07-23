using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Categories;

public record GetAdminCategoriesQuery() : IRequest<IReadOnlyList<AdminCategoryDto>>;

public record AdminCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string Status,
    int SortOrder,
    Guid? ParentCategoryId,
    string SkuPrefix);

public sealed class GetAdminCategoriesQueryHandler
    : IRequestHandler<GetAdminCategoriesQuery, IReadOnlyList<AdminCategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAdminCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<AdminCategoryDto>> Handle(
        GetAdminCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        return categories
            .Select(category =>
            {
                var translation = category.Translations.FirstOrDefault();
                return new AdminCategoryDto(
                    category.Id,
                    translation?.Name ?? "Category",
                    translation?.Slug ?? category.Id.ToString("N")[..8],
                    translation?.Description ?? string.Empty,
                    category.Status.ToString(),
                    category.SortOrder,
                    category.ParentCategoryId,
                    category.SkuPrefix);
            })
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToList();
    }
}
