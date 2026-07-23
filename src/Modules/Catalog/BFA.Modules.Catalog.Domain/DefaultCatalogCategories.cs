namespace BFA.Modules.Catalog.Domain;

public static class DefaultCatalogCategories
{
    public static IReadOnlyList<(string Name, string Slug, string Description, string SkuPrefix)> All { get; } =
    [
        ("Food & Grocery", "food-grocery", "Armenian food products", "FG"),
        ("Handicrafts", "handicrafts", "Traditional handmade goods", "HC"),
        ("Textiles", "textiles", "Carpets, clothing and fabrics", "TX"),
        ("Wine & Spirits", "wine-spirits", "Armenian wine and brandy", "WS"),
        ("Beauty & Wellness", "beauty-wellness", "Natural cosmetics and wellness", "BW"),
        ("Jewelry", "jewelry", "Armenian jewelry and silverware", "JW"),
        ("Home & Decor", "home-decor", "Home decoration and gifts", "HD"),
        ("Souvenirs", "souvenirs", "Travel souvenirs and keepsakes", "SV"),
    ];
}
