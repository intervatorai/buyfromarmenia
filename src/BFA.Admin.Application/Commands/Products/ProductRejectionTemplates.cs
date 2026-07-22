namespace BFA.Admin.Application.Commands.Products;

public static class ProductRejectionTemplates
{
    public const string PoorImageQuality = "Poor image quality";
    public const string IncorrectCategory = "Incorrect category";
    public const string MissingCertificate = "Missing certificate";
    public const string IncompleteDescription = "Incomplete description";
    public const string RestrictedForInternationalShipment = "Restricted for international shipment";
    public const string IncorrectProductWeight = "Incorrect product weight";

    public static IReadOnlyList<string> All { get; } =
    [
        PoorImageQuality,
        IncorrectCategory,
        MissingCertificate,
        IncompleteDescription,
        RestrictedForInternationalShipment,
        IncorrectProductWeight
    ];
}
