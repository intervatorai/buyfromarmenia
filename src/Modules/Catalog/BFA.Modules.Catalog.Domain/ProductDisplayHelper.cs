using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;

namespace BFA.Modules.Catalog.Domain;

public static class ProductDisplayHelper
{
    public static (string Name, string Description, string ShortDescription) GetDisplayText(
        Product product,
        string? languageCode = null)
    {
        ProductTranslation? translation = null;

        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            try
            {
                var requested = LanguageCode.From(languageCode);
                translation = product.Translations.FirstOrDefault(t => t.Language == requested);
            }
            catch (DomainException)
            {
                // Fall through to default language.
            }
        }

        translation ??= product.Translations.FirstOrDefault(t =>
                t.Language.Value == product.DefaultLanguage)
            ?? product.Translations.FirstOrDefault();

        if (translation is null)
        {
            return ("Unnamed product", string.Empty, string.Empty);
        }

        return (translation.Name, translation.Description, translation.ShortDescription);
    }
}
