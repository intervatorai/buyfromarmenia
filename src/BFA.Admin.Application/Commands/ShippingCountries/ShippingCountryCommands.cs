using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Shipping.Domain.Aggregates;
using BFA.Modules.Shipping.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.ShippingCountries;

public record ShippingCountryDto(
    Guid Id,
    string IsoCode,
    string NameEn,
    string NameHy,
    bool IsEnabled,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record CreateShippingCountryCommand(
    string IsoCode,
    string NameEn,
    string NameHy,
    int SortOrder = 0,
    bool IsEnabled = true) : IRequest<ShippingCountryDto>;

public record UpdateShippingCountryCommand(
    Guid Id,
    string NameEn,
    string NameHy,
    int SortOrder) : IRequest<ShippingCountryDto?>;

public record SetShippingCountryEnabledCommand(Guid Id, bool IsEnabled) : IRequest<bool>;

public static class ShippingCountryMapper
{
    public static ShippingCountryDto ToDto(ShippingCountry country) =>
        new(
            country.Id,
            country.IsoCode,
            country.NameEn,
            country.NameHy,
            country.IsEnabled,
            country.SortOrder,
            country.CreatedAtUtc,
            country.UpdatedAtUtc);
}

public sealed class CreateShippingCountryCommandHandler
    : IRequestHandler<CreateShippingCountryCommand, ShippingCountryDto>
{
    private readonly IShippingCountryRepository _countryRepository;
    private readonly IAuditLogger _auditLogger;

    public CreateShippingCountryCommandHandler(
        IShippingCountryRepository countryRepository,
        IAuditLogger auditLogger)
    {
        _countryRepository = countryRepository;
        _auditLogger = auditLogger;
    }

    public async Task<ShippingCountryDto> Handle(
        CreateShippingCountryCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _countryRepository.GetByIsoCodeAsync(request.IsoCode, cancellationToken);
        if (existing is not null)
        {
            throw new DomainException($"Shipping country '{request.IsoCode}' already exists.");
        }

        var country = ShippingCountry.Create(
            request.IsoCode,
            request.NameEn,
            request.NameHy,
            request.SortOrder,
            request.IsEnabled);

        await _countryRepository.AddAsync(country, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ShippingCountryCreated",
            "ShippingCountry",
            country.Id,
            cancellationToken: cancellationToken);

        return ShippingCountryMapper.ToDto(country);
    }
}

public sealed class UpdateShippingCountryCommandHandler
    : IRequestHandler<UpdateShippingCountryCommand, ShippingCountryDto?>
{
    private readonly IShippingCountryRepository _countryRepository;
    private readonly IAuditLogger _auditLogger;

    public UpdateShippingCountryCommandHandler(
        IShippingCountryRepository countryRepository,
        IAuditLogger auditLogger)
    {
        _countryRepository = countryRepository;
        _auditLogger = auditLogger;
    }

    public async Task<ShippingCountryDto?> Handle(
        UpdateShippingCountryCommand request,
        CancellationToken cancellationToken)
    {
        var country = await _countryRepository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (country is null)
        {
            return null;
        }

        country.Update(request.NameEn, request.NameHy, request.SortOrder);
        await _countryRepository.UpdateAsync(country, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ShippingCountryUpdated",
            "ShippingCountry",
            country.Id,
            cancellationToken: cancellationToken);

        return ShippingCountryMapper.ToDto(country);
    }
}

public sealed class SetShippingCountryEnabledCommandHandler
    : IRequestHandler<SetShippingCountryEnabledCommand, bool>
{
    private readonly IShippingCountryRepository _countryRepository;
    private readonly IAuditLogger _auditLogger;

    public SetShippingCountryEnabledCommandHandler(
        IShippingCountryRepository countryRepository,
        IAuditLogger auditLogger)
    {
        _countryRepository = countryRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        SetShippingCountryEnabledCommand request,
        CancellationToken cancellationToken)
    {
        var country = await _countryRepository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (country is null)
        {
            return false;
        }

        if (request.IsEnabled)
        {
            country.Enable();
        }
        else
        {
            country.Disable();
        }

        await _countryRepository.UpdateAsync(country, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            request.IsEnabled ? "ShippingCountryEnabled" : "ShippingCountryDisabled",
            "ShippingCountry",
            country.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}
