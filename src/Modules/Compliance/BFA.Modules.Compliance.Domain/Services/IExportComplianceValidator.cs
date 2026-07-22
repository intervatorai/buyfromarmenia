namespace BFA.Modules.Compliance.Domain.Services;

public sealed record ExportComplianceViolation(
    string DestinationCountryCode,
    string Reason,
    Guid? CategoryId);

public sealed record ExportComplianceResult(bool IsAllowed, IReadOnlyList<ExportComplianceViolation> Violations)
{
    public static ExportComplianceResult Allowed() => new(true, []);

    public static ExportComplianceResult Blocked(IReadOnlyList<ExportComplianceViolation> violations)
        => new(false, violations);
}

public interface IExportComplianceValidator
{
    Task<ExportComplianceResult> ValidateAsync(
        string destinationCountryCode,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);
}
