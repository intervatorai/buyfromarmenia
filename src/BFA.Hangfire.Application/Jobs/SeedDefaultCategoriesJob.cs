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
        var sortOrder = 0;

        foreach (var (name, slug, description) in DefaultCatalogCategories.All)
        {
            var existing = await _categoryRepository.GetBySlugAsync(slug, "en", cancellationToken);
            if (existing is not null)
            {
                continue;
            }

            var category = Category.Create(name, slug, "en", sortOrder: sortOrder++, description: description);
            await _categoryRepository.AddAsync(category, cancellationToken);
            added++;
        }

        if (added > 0)
        {
            _logger.LogInformation("Seeded {Count} default categories.", added);
        }
    }
}
