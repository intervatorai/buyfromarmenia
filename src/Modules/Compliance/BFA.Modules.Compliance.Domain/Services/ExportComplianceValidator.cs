using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Compliance.Domain.Repositories;

namespace BFA.Modules.Compliance.Domain.Services;

public sealed class ExportComplianceValidator : IExportComplianceValidator
{
    private readonly ITradeRestrictionRepository _tradeRestrictionRepository;
    private readonly IProductRepository _productRepository;

    public ExportComplianceValidator(
        ITradeRestrictionRepository tradeRestrictionRepository,
        IProductRepository productRepository)
    {
        _tradeRestrictionRepository = tradeRestrictionRepository;
        _productRepository = productRepository;
    }

    public async Task<ExportComplianceResult> ValidateAsync(
        string destinationCountryCode,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        var countryCode = destinationCountryCode.Trim().ToUpperInvariant();
        var restrictions = await _tradeRestrictionRepository.GetActiveForCountryAsync(
            countryCode,
            cancellationToken);

        if (restrictions.Count == 0)
        {
            return ExportComplianceResult.Allowed();
        }

        var violations = new List<ExportComplianceViolation>();

        var countryWide = restrictions.Where(restriction => restriction.CategoryId is null).ToList();
        foreach (var restriction in countryWide)
        {
            violations.Add(new ExportComplianceViolation(
                restriction.DestinationCountryCode,
                restriction.Reason,
                null));
        }

        if (violations.Count > 0)
        {
            return ExportComplianceResult.Blocked(violations);
        }

        var categoryRestrictions = restrictions
            .Where(restriction => restriction.CategoryId.HasValue)
            .ToList();

        if (categoryRestrictions.Count == 0)
        {
            return ExportComplianceResult.Allowed();
        }

        foreach (var productId in productIds.Distinct())
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product?.CategoryId is null)
            {
                continue;
            }

            var match = categoryRestrictions.FirstOrDefault(
                restriction => restriction.CategoryId == product.CategoryId);

            if (match is not null)
            {
                violations.Add(new ExportComplianceViolation(
                    match.DestinationCountryCode,
                    match.Reason,
                    product.CategoryId));
            }
        }

        return violations.Count == 0
            ? ExportComplianceResult.Allowed()
            : ExportComplianceResult.Blocked(violations);
    }
}
