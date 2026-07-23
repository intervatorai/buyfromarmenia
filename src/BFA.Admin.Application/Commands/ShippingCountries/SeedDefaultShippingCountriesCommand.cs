using BFA.BuildingBlocks.Application;
using BFA.Modules.Shipping.Domain.Aggregates;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.ShippingCountries;

public record SeedDefaultShippingCountriesResult(int Added, int Skipped);

public record SeedDefaultShippingCountriesCommand()
    : IRequest<SeedDefaultShippingCountriesResult>;

/// <summary>
/// Curated destination list for BuyFromArmenia. Existing ISO codes are skipped.
/// New countries are created disabled (except Armenia) so admins opt-in to shipping.
/// </summary>
public static class DefaultShippingCountriesCatalog
{
    public static IReadOnlyList<(string Iso, string NameEn, string NameHy, int SortOrder, bool Enabled)> Items { get; } =
    [
        ("AM", "Armenia", "Հայաստան", 0, true),
        ("RU", "Russia", "Ռուսաստան", 10, false),
        ("US", "United States", "ԱՄՆ", 20, false),
        ("CA", "Canada", "Կանադա", 30, false),
        ("GB", "United Kingdom", "Միացյալ Թագավորություն", 40, false),
        ("DE", "Germany", "Գերմանիա", 50, false),
        ("FR", "France", "Ֆրանսիա", 60, false),
        ("IT", "Italy", "Իտալիա", 70, false),
        ("ES", "Spain", "Իսպանիա", 80, false),
        ("NL", "Netherlands", "Նիդերլանդներ", 90, false),
        ("BE", "Belgium", "Բելգիա", 100, false),
        ("CH", "Switzerland", "Շվեյցարիա", 110, false),
        ("AT", "Austria", "Ավստրիա", 120, false),
        ("PL", "Poland", "Լեհաստան", 130, false),
        ("SE", "Sweden", "Շվեդիա", 140, false),
        ("NO", "Norway", "Նորվեգիա", 150, false),
        ("DK", "Denmark", "Դանիա", 160, false),
        ("FI", "Finland", "Ֆինլանդիա", 170, false),
        ("GE", "Georgia", "Վրաստան", 180, false),
        ("AE", "United Arab Emirates", "Արաբական Միացյալ Էմիրություններ", 190, false),
        ("AU", "Australia", "Ավստրալիա", 200, false),
        ("NZ", "New Zealand", "Նոր Զելանդիա", 210, false),
        ("TR", "Turkey", "Թուրքիա", 220, false),
        ("IL", "Israel", "Իսրայել", 230, false),
        ("IR", "Iran", "Իրան", 240, false),
        ("IN", "India", "Հնդկաստան", 250, false),
        ("CN", "China", "Չինաստան", 260, false),
        ("JP", "Japan", "Ճապոնիա", 270, false),
        ("KR", "South Korea", "Հարավային Կորեա", 280, false),
        ("BR", "Brazil", "Բրազիլիա", 290, false),
        ("MX", "Mexico", "Մեքսիկա", 300, false),
        ("UA", "Ukraine", "Ուկրաինա", 310, false),
        ("KZ", "Kazakhstan", "Ղազախստան", 320, false),
        ("BY", "Belarus", "Բելառուս", 330, false),
    ];
}

public sealed class SeedDefaultShippingCountriesCommandHandler
    : IRequestHandler<SeedDefaultShippingCountriesCommand, SeedDefaultShippingCountriesResult>
{
    private readonly IShippingCountryRepository _countryRepository;
    private readonly IAuditLogger _auditLogger;

    public SeedDefaultShippingCountriesCommandHandler(
        IShippingCountryRepository countryRepository,
        IAuditLogger auditLogger)
    {
        _countryRepository = countryRepository;
        _auditLogger = auditLogger;
    }

    public async Task<SeedDefaultShippingCountriesResult> Handle(
        SeedDefaultShippingCountriesCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _countryRepository.GetAllAsync(cancellationToken);
        var existingCodes = existing
            .Select(country => country.IsoCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toAdd = new List<ShippingCountry>();
        var skipped = 0;

        foreach (var item in DefaultShippingCountriesCatalog.Items)
        {
            if (existingCodes.Contains(item.Iso))
            {
                skipped++;
                continue;
            }

            toAdd.Add(ShippingCountry.Create(
                item.Iso,
                item.NameEn,
                item.NameHy,
                item.SortOrder,
                item.Enabled));
            existingCodes.Add(item.Iso);
        }

        if (toAdd.Count > 0)
        {
            await _countryRepository.AddRangeAsync(toAdd, cancellationToken);

            await _auditLogger.WriteAsync(
                "Admin",
                null,
                "ShippingCountriesSeeded",
                "ShippingCountry",
                null,
                $"Added={toAdd.Count};Skipped={skipped}",
                cancellationToken);
        }

        return new SeedDefaultShippingCountriesResult(toAdd.Count, skipped);
    }
}
