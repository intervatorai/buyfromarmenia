using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly BfaDbContext _dbContext;

    public CategoryRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await QueryWithTranslations()
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
    }

    public Task<Category?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return QueryWithTranslations()
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
    }

    public async Task<Category?> GetBySlugAsync(
        string slug,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var language = LanguageCode.From(languageCode);

        var categoryId = await _dbContext.CategoryTranslations
            .AsNoTracking()
            .Where(translation =>
                translation.Slug == normalizedSlug && translation.Language == language)
            .Select(translation => (Guid?)translation.CategoryId)
            .FirstOrDefaultAsync(cancellationToken);

        if (categoryId is null)
        {
            return null;
        }

        return await GetByIdAsync(categoryId.Value, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await QueryWithTranslations()
            .AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetByParentIdAsync(
        Guid? parentCategoryId,
        CancellationToken cancellationToken = default)
    {
        return await QueryWithTranslations()
            .AsNoTracking()
            .Where(category => category.ParentCategoryId == parentCategoryId)
            .OrderBy(category => category.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _dbContext.Categories.AddAsync(category, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        category.ClearDomainEvents();
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        category.ClearDomainEvents();
    }

    private IQueryable<Category> QueryWithTranslations()
    {
        return _dbContext.Categories.Include("_translations");
    }
}
