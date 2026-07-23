using BFA.Modules.Catalog.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace BFA.Hangfire.Application.Jobs;

public class SeedDefaultCategoriesJob
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<SeedDefaultCategoriesJob> _logger;

    public SeedDefaultCategoriesJob(
        ICategoryRepository categoryRepository,
        ILogger<SeedDefaultCategoriesJob> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var added = 0;
        var prefixesUpdated = 0;
        var sortOrder = 0;

        foreach (var (name, slug, description, skuPrefix) in DefaultCatalogCategories.All)
        {
            var existing = await _categoryRepository.GetBySlugAsync(slug, "en", cancellationToken);
            if (existing is not null)
            {
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
            added++;
        }

        if (added > 0 || prefixesUpdated > 0)
        {
            _logger.LogInformation(
                "Seeded {Count} default categories; updated {PrefixCount} SKU prefixes.",
                added,
                prefixesUpdated);
        }
    }
}
