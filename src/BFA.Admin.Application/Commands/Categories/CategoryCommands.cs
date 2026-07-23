using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Categories;

public record CreateCategoryCommand(
    string Name,
    string Slug,
    string? Description = null,
    int SortOrder = 0,
    Guid? ParentCategoryId = null,
    string LanguageCode = "en",
    string? SkuPrefix = null) : IRequest<Guid>;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditLogger _auditLogger;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IAuditLogger auditLogger)
    {
        _categoryRepository = categoryRepository;
        _auditLogger = auditLogger;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(
            request.Name,
            request.Slug,
            request.LanguageCode,
            request.ParentCategoryId,
            request.SortOrder,
            request.Description,
            request.SkuPrefix);

        await _categoryRepository.AddAsync(category, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "CategoryCreated",
            "Category",
            category.Id,
            cancellationToken: cancellationToken);

        return category.Id;
    }
}

public record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string Slug,
    string? Description = null,
    int SortOrder = 0,
    Guid? ParentCategoryId = null,
    string LanguageCode = "en",
    string? SkuPrefix = null) : IRequest<bool>;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditLogger _auditLogger;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IAuditLogger auditLogger)
    {
        _categoryRepository = categoryRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdForUpdateAsync(
            request.CategoryId,
            cancellationToken);
        if (category is null)
        {
            return false;
        }

        category.UpdateTranslation(
            request.LanguageCode,
            request.Name,
            request.Slug,
            request.Description);
        category.MoveTo(request.ParentCategoryId, request.SortOrder);

        // Always apply when the client sends a prefix (edit form includes the field).
        if (request.SkuPrefix is not null)
        {
            if (string.IsNullOrWhiteSpace(request.SkuPrefix))
            {
                throw new DomainException("SKU prefix is required (2–4 letters).");
            }

            category.SetSkuPrefix(request.SkuPrefix);
        }

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "CategoryUpdated",
            "Category",
            category.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record HideCategoryCommand(Guid CategoryId) : IRequest<bool>;

public sealed class HideCategoryCommandHandler : IRequestHandler<HideCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditLogger _auditLogger;

    public HideCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IAuditLogger auditLogger)
    {
        _categoryRepository = categoryRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(HideCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdForUpdateAsync(
            request.CategoryId,
            cancellationToken);
        if (category is null)
        {
            return false;
        }

        category.Hide();
        await _categoryRepository.UpdateAsync(category, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "CategoryHidden",
            "Category",
            category.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record ActivateCategoryCommand(Guid CategoryId) : IRequest<bool>;

public sealed class ActivateCategoryCommandHandler : IRequestHandler<ActivateCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditLogger _auditLogger;

    public ActivateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IAuditLogger auditLogger)
    {
        _categoryRepository = categoryRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(ActivateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdForUpdateAsync(
            request.CategoryId,
            cancellationToken);
        if (category is null)
        {
            return false;
        }

        category.Activate();
        await _categoryRepository.UpdateAsync(category, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "CategoryActivated",
            "Category",
            category.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record SeedDefaultCategoriesCommand() : IRequest<SeedDefaultCategoriesResult>;

public record SeedDefaultCategoriesResult(
    int Added,
    int Skipped,
    int PrefixesUpdated,
    IReadOnlyList<string> AddedSlugs);

public sealed class SeedDefaultCategoriesCommandHandler
    : IRequestHandler<SeedDefaultCategoriesCommand, SeedDefaultCategoriesResult>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditLogger _auditLogger;

    public SeedDefaultCategoriesCommandHandler(
        ICategoryRepository categoryRepository,
        IAuditLogger auditLogger)
    {
        _categoryRepository = categoryRepository;
        _auditLogger = auditLogger;
    }

    public async Task<SeedDefaultCategoriesResult> Handle(
        SeedDefaultCategoriesCommand request,
        CancellationToken cancellationToken)
    {
        var added = 0;
        var skipped = 0;
        var prefixesUpdated = 0;
        var addedSlugs = new List<string>();
        var existingCategories = await _categoryRepository.GetAllAsync(cancellationToken);
        var sortOrder = existingCategories.Count > 0
            ? existingCategories.Max(category => category.SortOrder) + 1
            : 0;

        foreach (var (name, slug, description, skuPrefix) in DefaultCatalogCategories.All)
        {
            var existing = await _categoryRepository.GetBySlugAsync(slug, "en", cancellationToken);
            if (existing is not null)
            {
                skipped++;
                if (string.IsNullOrWhiteSpace(existing.SkuPrefix))
                {
                    var tracked = await _categoryRepository.GetByIdForUpdateAsync(
                        existing.Id,
                        cancellationToken);
                    if (tracked is not null && string.IsNullOrWhiteSpace(tracked.SkuPrefix))
                    {
                        tracked.SetSkuPrefix(skuPrefix);
                        await _categoryRepository.UpdateAsync(tracked, cancellationToken);
                        prefixesUpdated++;
                    }
                }

                continue;
            }

            var category = Category.Create(
                name,
                slug,
                "en",
                sortOrder: sortOrder++,
                description: description,
                skuPrefix: skuPrefix);

            await _categoryRepository.AddAsync(category, cancellationToken);
            addedSlugs.Add(slug);
            added++;
        }

        if (added > 0 || prefixesUpdated > 0)
        {
            await _auditLogger.WriteAsync(
                "Admin",
                null,
                "DefaultCategoriesSeeded",
                "Category",
                null,
                detailsJson: System.Text.Json.JsonSerializer.Serialize(
                    new { added, skipped, prefixesUpdated, addedSlugs }),
                cancellationToken: cancellationToken);
        }

        return new SeedDefaultCategoriesResult(added, skipped, prefixesUpdated, addedSlugs);
    }
}
