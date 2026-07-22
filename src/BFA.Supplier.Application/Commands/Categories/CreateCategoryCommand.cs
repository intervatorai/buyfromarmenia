using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Supplier.Application.Commands.Categories;

public record CreateCategoryCommand(
    string Name,
    string Slug,
    string? Description = null,
    Guid? ParentCategoryId = null,
    int SortOrder = 0,
    string LanguageCode = "en") : IRequest<Guid>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(
            request.Name,
            request.Slug,
            request.LanguageCode,
            request.ParentCategoryId,
            request.SortOrder,
            request.Description);

        await _categoryRepository.AddAsync(category, cancellationToken);
        return category.Id;
    }
}
