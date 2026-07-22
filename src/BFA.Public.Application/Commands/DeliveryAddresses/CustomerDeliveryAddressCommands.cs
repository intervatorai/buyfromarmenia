using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Commands.DeliveryAddresses;

public record CustomerDeliveryAddressDto(
    Guid Id,
    string Label,
    string CountryCode,
    string City,
    string Line1,
    string? Line2,
    string? PostalCode,
    string? Region,
    bool IsDefault);

public record CreateCustomerDeliveryAddressCommand(
    Guid UserId,
    string CountryCode,
    string City,
    string Line1,
    string? Line2,
    string? PostalCode,
    string? Region,
    string? Label,
    bool IsDefault) : IRequest<CustomerDeliveryAddressDto?>;

public record UpdateCustomerDeliveryAddressCommand(
    Guid UserId,
    Guid AddressId,
    string CountryCode,
    string City,
    string Line1,
    string? Line2,
    string? PostalCode,
    string? Region,
    string? Label) : IRequest<CustomerDeliveryAddressDto?>;

public record SetDefaultCustomerDeliveryAddressCommand(Guid UserId, Guid AddressId)
    : IRequest<bool>;

public record DeleteCustomerDeliveryAddressCommand(Guid UserId, Guid AddressId)
    : IRequest<bool>;

public record GetCustomerDeliveryAddressesQuery(Guid UserId)
    : IRequest<IReadOnlyList<CustomerDeliveryAddressDto>>;

internal static class DeliveryAddressMapper
{
    public static CustomerDeliveryAddressDto ToDto(CustomerDeliveryAddress address) =>
        new(
            address.Id,
            address.Label,
            address.CountryCode,
            address.City,
            address.Line1,
            address.Line2,
            address.PostalCode,
            address.Region,
            address.IsDefault);
}

public sealed class GetCustomerDeliveryAddressesQueryHandler
    : IRequestHandler<GetCustomerDeliveryAddressesQuery, IReadOnlyList<CustomerDeliveryAddressDto>>
{
    private readonly ICustomerDeliveryAddressRepository _addressRepository;

    public GetCustomerDeliveryAddressesQueryHandler(
        ICustomerDeliveryAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<IReadOnlyList<CustomerDeliveryAddressDto>> Handle(
        GetCustomerDeliveryAddressesQuery request,
        CancellationToken cancellationToken)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return addresses.Select(DeliveryAddressMapper.ToDto).ToList();
    }
}

public sealed class CreateCustomerDeliveryAddressCommandHandler
    : IRequestHandler<CreateCustomerDeliveryAddressCommand, CustomerDeliveryAddressDto?>
{
    private readonly ICustomerDeliveryAddressRepository _addressRepository;
    private readonly ICustomerProfileRepository _profileRepository;

    public CreateCustomerDeliveryAddressCommandHandler(
        ICustomerDeliveryAddressRepository addressRepository,
        ICustomerProfileRepository profileRepository)
    {
        _addressRepository = addressRepository;
        _profileRepository = profileRepository;
    }

    public async Task<CustomerDeliveryAddressDto?> Handle(
        CreateCustomerDeliveryAddressCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var existing = await _addressRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        var makeDefault = request.IsDefault || existing.Count == 0;

        if (makeDefault)
        {
            foreach (var item in existing.Where(address => address.IsDefault))
            {
                var tracked = await _addressRepository.GetByIdForUserAsync(
                    item.Id,
                    request.UserId,
                    cancellationToken);
                if (tracked is null)
                {
                    continue;
                }

                tracked.ClearDefault();
                await _addressRepository.UpdateAsync(tracked, cancellationToken);
            }
        }

        var address = CustomerDeliveryAddress.Create(
            request.UserId,
            request.CountryCode,
            request.City,
            request.Line1,
            request.Line2,
            request.PostalCode,
            request.Region,
            request.Label,
            makeDefault);

        await _addressRepository.AddAsync(address, cancellationToken);
        return DeliveryAddressMapper.ToDto(address);
    }
}

public sealed class UpdateCustomerDeliveryAddressCommandHandler
    : IRequestHandler<UpdateCustomerDeliveryAddressCommand, CustomerDeliveryAddressDto?>
{
    private readonly ICustomerDeliveryAddressRepository _addressRepository;

    public UpdateCustomerDeliveryAddressCommandHandler(
        ICustomerDeliveryAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<CustomerDeliveryAddressDto?> Handle(
        UpdateCustomerDeliveryAddressCommand request,
        CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdForUserAsync(
            request.AddressId,
            request.UserId,
            cancellationToken);
        if (address is null)
        {
            return null;
        }

        address.Update(
            request.CountryCode,
            request.City,
            request.Line1,
            request.Line2,
            request.PostalCode,
            request.Region,
            request.Label);
        await _addressRepository.UpdateAsync(address, cancellationToken);
        return DeliveryAddressMapper.ToDto(address);
    }
}

public sealed class SetDefaultCustomerDeliveryAddressCommandHandler
    : IRequestHandler<SetDefaultCustomerDeliveryAddressCommand, bool>
{
    private readonly ICustomerDeliveryAddressRepository _addressRepository;

    public SetDefaultCustomerDeliveryAddressCommandHandler(
        ICustomerDeliveryAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<bool> Handle(
        SetDefaultCustomerDeliveryAddressCommand request,
        CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdForUserAsync(
            request.AddressId,
            request.UserId,
            cancellationToken);
        if (address is null)
        {
            return false;
        }

        var existing = await _addressRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        foreach (var item in existing.Where(item => item.IsDefault && item.Id != address.Id))
        {
            var tracked = await _addressRepository.GetByIdForUserAsync(
                item.Id,
                request.UserId,
                cancellationToken);
            if (tracked is null)
            {
                continue;
            }

            tracked.ClearDefault();
            await _addressRepository.UpdateAsync(tracked, cancellationToken);
        }

        address.MarkAsDefault();
        await _addressRepository.UpdateAsync(address, cancellationToken);
        return true;
    }
}

public sealed class DeleteCustomerDeliveryAddressCommandHandler
    : IRequestHandler<DeleteCustomerDeliveryAddressCommand, bool>
{
    private readonly ICustomerDeliveryAddressRepository _addressRepository;

    public DeleteCustomerDeliveryAddressCommandHandler(
        ICustomerDeliveryAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<bool> Handle(
        DeleteCustomerDeliveryAddressCommand request,
        CancellationToken cancellationToken)
    {
        var address = await _addressRepository.GetByIdForUserAsync(
            request.AddressId,
            request.UserId,
            cancellationToken);
        if (address is null)
        {
            return false;
        }

        var wasDefault = address.IsDefault;
        await _addressRepository.DeleteAsync(address, cancellationToken);

        if (!wasDefault)
        {
            return true;
        }

        var remaining = await _addressRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        var next = remaining.FirstOrDefault();
        if (next is null)
        {
            return true;
        }

        var tracked = await _addressRepository.GetByIdForUserAsync(
            next.Id,
            request.UserId,
            cancellationToken);
        if (tracked is null)
        {
            return true;
        }

        tracked.MarkAsDefault();
        await _addressRepository.UpdateAsync(tracked, cancellationToken);
        return true;
    }
}
