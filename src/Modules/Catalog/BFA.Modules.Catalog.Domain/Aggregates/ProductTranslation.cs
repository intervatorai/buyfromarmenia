using BFA.BuildingBlocks.Domain;

namespace BFA.Modules.Catalog.Domain.Aggregates;

public sealed class ProductTranslation : Entity
{
    public Guid ProductId { get; private set; }
    public LanguageCode Language { get; private set; } = LanguageCode.English;
    public string Name { get; private set; } = string.Empty;
    public string ShortDescription { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Ingredients { get; private set; } = string.Empty;
    public string UsageInstructions { get; private set; } = string.Empty;
    public string SeoTitle { get; private set; } = string.Empty;
    public string SeoDescription { get; private set; } = string.Empty;

    private ProductTranslation()
    {
    }

    internal ProductTranslation(
        Guid productId,
        LanguageCode language,
        string name,
        string description,
        string shortDescription = "",
        string ingredients = "",
        string usageInstructions = "",
        string seoTitle = "",
        string seoDescription = "")
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Language = language;
        Name = name;
        Description = description;
        ShortDescription = shortDescription;
        Ingredients = ingredients;
        UsageInstructions = usageInstructions;
        SeoTitle = seoTitle;
        SeoDescription = seoDescription;
    }

    internal void Update(
        string name,
        string description,
        string shortDescription,
        string ingredients,
        string usageInstructions,
        string seoTitle,
        string seoDescription)
    {
        Name = name;
        Description = description;
        ShortDescription = shortDescription;
        Ingredients = ingredients;
        UsageInstructions = usageInstructions;
        SeoTitle = seoTitle;
        SeoDescription = seoDescription;
    }
}
