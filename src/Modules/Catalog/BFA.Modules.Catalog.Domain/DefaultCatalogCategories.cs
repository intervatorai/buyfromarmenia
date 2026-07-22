namespace BFA.Modules.Catalog.Domain;

public static class DefaultCatalogCategories
{
    public static IReadOnlyList<(string Name, string Slug, string Description)> All { get; } =
    [
        ("Food & Grocery", "food-grocery", "Armenian food products"),
        ("Handicrafts", "handicrafts", "Traditional handmade goods"),
        ("Textiles", "textiles", "Carpets, clothing and fabrics"),
        ("Wine & Spirits", "wine-spirits", "Armenian wine and brandy"),
        ("Beauty & Wellness", "beauty-wellness", "Natural cosmetics and wellness"),
        ("Jewelry", "jewelry", "Armenian jewelry and silverware"),
        ("Home & Decor", "home-decor", "Home decoration and gifts"),
        ("Souvenirs", "souvenirs", "Travel souvenirs and keepsakes"),
    ];
}
