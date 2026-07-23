using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Shipping.Domain.Aggregates;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.ShippingRates;

public record ShippingRateBracketDto(
    Guid Id,
    string CountryIsoCode,
    decimal WeightFromKg,
    decimal WeightToKg,
    decimal Price,
    string Currency,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record ShippingPricingSettingsDto(decimal ErrorMarginPercent, DateTime UpdatedAtUtc);

public static class ShippingRateMapper
{
    public static ShippingRateBracketDto ToDto(ShippingRateBracket bracket) =>
        new(
            bracket.Id,
            bracket.CountryIsoCode,
            bracket.WeightFromKg,
            bracket.WeightToKg,
            bracket.Price,
            bracket.Currency,
            bracket.IsActive,
            bracket.CreatedAtUtc,
            bracket.UpdatedAtUtc);

    public static ShippingPricingSettingsDto ToDto(ShippingPricingSettings settings) =>
        new(settings.ErrorMarginPercent, settings.UpdatedAtUtc);
}

public record CreateShippingRateBracketCommand(
    string CountryIsoCode,
    decimal WeightFromKg,
    decimal WeightToKg,
    decimal Price,
    string Currency = "USD",
    bool IsActive = true) : IRequest<ShippingRateBracketDto>;

public record UpdateShippingRateBracketCommand(
    Guid Id,
    decimal WeightFromKg,
    decimal WeightToKg,
    decimal Price,
    string Currency,
    bool IsActive) : IRequest<ShippingRateBracketDto?>;

public record DeleteShippingRateBracketCommand(Guid Id) : IRequest<bool>;

public record UpdateShippingPricingSettingsCommand(decimal ErrorMarginPercent)
    : IRequest<ShippingPricingSettingsDto>;

public sealed class CreateShippingRateBracketCommandHandler
    : IRequestHandler<CreateShippingRateBracketCommand, ShippingRateBracketDto>
{
    private readonly IShippingRateBracketRepository _bracketRepository;
    private readonly IShippingCountryRepository _countryRepository;
    private readonly IAuditLogger _auditLogger;

    public CreateShippingRateBracketCommandHandler(
        IShippingRateBracketRepository bracketRepository,
        IShippingCountryRepository countryRepository,
        IAuditLogger auditLogger)
    {
        _bracketRepository = bracketRepository;
        _countryRepository = countryRepository;
        _auditLogger = auditLogger;
    }

    public async Task<ShippingRateBracketDto> Handle(
        CreateShippingRateBracketCommand request,
        CancellationToken cancellationToken)
    {
        var country = await _countryRepository.GetByIsoCodeAsync(request.CountryIsoCode, cancellationToken)
            ?? throw new DomainException($"Shipping country '{request.CountryIsoCode}' was not found.");

        await EnsureNoOverlapAsync(
            country.IsoCode,
            request.WeightFromKg,
            request.WeightToKg,
            excludeId: null,
            cancellationToken);

        var bracket = ShippingRateBracket.Create(
            country.IsoCode,
            request.WeightFromKg,
            request.WeightToKg,
            request.Price,
            request.Currency,
            request.IsActive);

        await _bracketRepository.AddAsync(bracket, cancellationToken);
        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ShippingRateBracketCreated",
            nameof(ShippingRateBracket),
            bracket.Id,
            $"{{\"country\":\"{bracket.CountryIsoCode}\",\"from\":{bracket.WeightFromKg},\"to\":{bracket.WeightToKg}}}",
            cancellationToken);

        return ShippingRateMapper.ToDto(bracket);
    }

    private async Task EnsureNoOverlapAsync(
        string countryIso,
        decimal fromKg,
        decimal toKg,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var existing = await _bracketRepository.GetByCountryAsync(countryIso, cancellationToken);
        var overlap = existing.FirstOrDefault(bracket =>
            (!excludeId.HasValue || bracket.Id != excludeId.Value)
            && bracket.Overlaps(fromKg, toKg));

        if (overlap is not null)
        {
            throw new DomainException(
                $"Weight range overlaps existing bracket {overlap.WeightFromKg}-{overlap.WeightToKg} kg.");
        }
    }
}

public sealed class UpdateShippingRateBracketCommandHandler
    : IRequestHandler<UpdateShippingRateBracketCommand, ShippingRateBracketDto?>
{
    private readonly IShippingRateBracketRepository _bracketRepository;
    private readonly IAuditLogger _auditLogger;

    public UpdateShippingRateBracketCommandHandler(
        IShippingRateBracketRepository bracketRepository,
        IAuditLogger auditLogger)
    {
        _bracketRepository = bracketRepository;
        _auditLogger = auditLogger;
    }

    public async Task<ShippingRateBracketDto?> Handle(
        UpdateShippingRateBracketCommand request,
        CancellationToken cancellationToken)
    {
        var bracket = await _bracketRepository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (bracket is null)
        {
            return null;
        }

        var existing = await _bracketRepository.GetByCountryAsync(bracket.CountryIsoCode, cancellationToken);
        var overlap = existing.FirstOrDefault(item =>
            item.Id != bracket.Id && item.Overlaps(request.WeightFromKg, request.WeightToKg));
        if (overlap is not null)
        {
            throw new DomainException(
                $"Weight range overlaps existing bracket {overlap.WeightFromKg}-{overlap.WeightToKg} kg.");
        }

        bracket.Update(
            request.WeightFromKg,
            request.WeightToKg,
            request.Price,
            request.Currency,
            request.IsActive);

        await _bracketRepository.UpdateAsync(bracket, cancellationToken);
        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ShippingRateBracketUpdated",
            nameof(ShippingRateBracket),
            bracket.Id,
            $"{{\"country\":\"{bracket.CountryIsoCode}\",\"from\":{bracket.WeightFromKg},\"to\":{bracket.WeightToKg}}}",
            cancellationToken);

        return ShippingRateMapper.ToDto(bracket);
    }
}

public sealed class DeleteShippingRateBracketCommandHandler
    : IRequestHandler<DeleteShippingRateBracketCommand, bool>
{
    private readonly IShippingRateBracketRepository _bracketRepository;
    private readonly IAuditLogger _auditLogger;

    public DeleteShippingRateBracketCommandHandler(
        IShippingRateBracketRepository bracketRepository,
        IAuditLogger auditLogger)
    {
        _bracketRepository = bracketRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        DeleteShippingRateBracketCommand request,
        CancellationToken cancellationToken)
    {
        var bracket = await _bracketRepository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (bracket is null)
        {
            return false;
        }

        await _bracketRepository.DeleteAsync(bracket, cancellationToken);
        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ShippingRateBracketDeleted",
            nameof(ShippingRateBracket),
            request.Id,
            cancellationToken: cancellationToken);
        return true;
    }
}

public sealed class UpdateShippingPricingSettingsCommandHandler
    : IRequestHandler<UpdateShippingPricingSettingsCommand, ShippingPricingSettingsDto>
{
    private readonly IShippingPricingSettingsRepository _settingsRepository;
    private readonly IAuditLogger _auditLogger;

    public UpdateShippingPricingSettingsCommandHandler(
        IShippingPricingSettingsRepository settingsRepository,
        IAuditLogger auditLogger)
    {
        _settingsRepository = settingsRepository;
        _auditLogger = auditLogger;
    }

    public async Task<ShippingPricingSettingsDto> Handle(
        UpdateShippingPricingSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetOrCreateAsync(cancellationToken);
        settings.SetErrorMarginPercent(request.ErrorMarginPercent);
        await _settingsRepository.UpdateAsync(settings, cancellationToken);
        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ShippingPricingSettingsUpdated",
            nameof(ShippingPricingSettings),
            settings.Id,
            $"{{\"marginPercent\":{settings.ErrorMarginPercent}}}",
            cancellationToken);
        return ShippingRateMapper.ToDto(settings);
    }
}
