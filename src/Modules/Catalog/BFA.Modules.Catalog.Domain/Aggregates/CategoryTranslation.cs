using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Catalog.Domain.Aggregates;

public sealed class CategoryTranslation : Entity
{
    public Guid CategoryId { get; private set; }
    public LanguageCode Language { get; private set; } = LanguageCode.English;
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private CategoryTranslation()
    {
    }

    internal CategoryTranslation(
        Guid categoryId,
        LanguageCode language,
        string name,
        string slug,
        string description)
    {
        Id = Guid.NewGuid();
        CategoryId = categoryId;
        Language = language;
        Name = name;
        Slug = slug;
        Description = description;
    }

    internal void Update(string name, string slug, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Category slug is required.");
        }

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = description?.Trim() ?? string.Empty;
    }
}
