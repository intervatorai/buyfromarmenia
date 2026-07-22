using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Queries.Categories;

public record GetCategoriesQuery : IRequest<IReadOnlyList<PublicCategoryDto>>;

public record PublicCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    Guid? ParentCategoryId);

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<PublicCategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<PublicCategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        return categories
            .Where(category => category.Status == CategoryStatus.Active)
            .Select(category =>
            {
                var translation = category.Translations.FirstOrDefault()
                    ?? throw new InvalidOperationException("Category must have a translation.");

                return new PublicCategoryDto(
                    category.Id,
                    translation.Name,
                    translation.Slug,
                    translation.Description,
                    category.ParentCategoryId);
            })
            .OrderBy(category => category.Name)
            .ToList();
    }
}
