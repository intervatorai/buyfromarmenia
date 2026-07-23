using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Events;

namespace BFA.Modules.Catalog.Domain.Aggregates;

public sealed class Category : AggregateRoot
{
    public Guid? ParentCategoryId { get; private set; }
    public CategoryStatus Status { get; private set; } = CategoryStatus.Active;
    public int SortOrder { get; private set; }
    public string SkuPrefix { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<CategoryTranslation> _translations = [];
    public IReadOnlyCollection<CategoryTranslation> Translations => _translations.AsReadOnly();

    private Category()
    {
    }

    public static Category Create(
        string name,
        string slug,
        string languageCode = "en",
        Guid? parentCategoryId = null,
        int sortOrder = 0,
        string? description = null,
        string? skuPrefix = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Category slug is required.");
        }

        var language = LanguageCode.From(languageCode);
        var category = new Category
        {
            Id = Guid.NewGuid(),
            ParentCategoryId = parentCategoryId,
            SortOrder = sortOrder,
            Status = CategoryStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(skuPrefix))
        {
            category.SetSkuPrefix(skuPrefix);
        }

        category._translations.Add(new CategoryTranslation(
            category.Id,
            language,
            name.Trim(),
            slug.Trim().ToLowerInvariant(),
            description?.Trim() ?? string.Empty));

        category.RaiseDomainEvent(new CategoryCreatedDomainEvent(category.Id, parentCategoryId));
        return category;
    }

    public void SetSkuPrefix(string skuPrefix)
    {
        var normalized = NormalizeSkuPrefix(skuPrefix);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainException("SKU prefix is required (2–4 letters).");
        }

        SkuPrefix = normalized;
        UpdatedAt = DateTime.UtcNow;
    }

    public static string NormalizeSkuPrefix(string? skuPrefix)
    {
        if (string.IsNullOrWhiteSpace(skuPrefix))
        {
            return string.Empty;
        }

        var letters = new string(skuPrefix.Trim().ToUpperInvariant().Where(char.IsLetter).ToArray());
        if (letters.Length is < 2 or > 4)
        {
            throw new DomainException("SKU prefix must be 2–4 letters.");
        }

        return letters;
    }

    public CategoryTranslation GetTranslation(LanguageCode language)
    {
        var translation = _translations.FirstOrDefault(t => t.Language == language);
        if (translation is null)
        {
            throw new DomainException($"Translation for language '{language}' was not found.");
        }

        return translation;
    }

    public void AddTranslation(string name, string slug, string languageCode, string? description = null)
    {
        var language = LanguageCode.From(languageCode);

        if (_translations.Any(t => t.Language == language))
        {
            throw new DomainException($"Translation for language '{language}' already exists.");
        }

        _translations.Add(new CategoryTranslation(
            Id,
            language,
            name.Trim(),
            slug.Trim().ToLowerInvariant(),
            description?.Trim() ?? string.Empty));

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTranslation(
        string languageCode,
        string name,
        string slug,
        string? description = null)
    {
        var language = LanguageCode.From(languageCode);
        var translation = GetTranslation(language);
        translation.Update(name, slug, description);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveTo(Guid? parentCategoryId, int sortOrder)
    {
        ParentCategoryId = parentCategoryId;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Hide()
    {
        Status = CategoryStatus.Hidden;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = CategoryStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = CategoryStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void LoadTranslations(IEnumerable<CategoryTranslation> translations)
    {
        _translations.Clear();
        _translations.AddRange(translations);
    }
}
