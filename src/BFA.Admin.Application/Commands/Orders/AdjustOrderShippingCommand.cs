using BFA.BuildingBlocks.Application;
using BFA.BuildingBlocks.Domain;
using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Services;
using MediatR;

namespace BFA.Admin.Application.Commands.Orders;

public record AdjustOrderShippingCommand(
    Guid OrderId,
    decimal? ActualWeightKg,
    decimal? ManualShippingFee,
    string? Reason) : IRequest<AdjustOrderShippingResult?>;

public record AdjustOrderShippingResult(
    Guid OrderId,
    decimal ShippingFeeQuoted,
    decimal ShippingFee,
    decimal Subtotal,
    decimal Total,
    decimal? ActualWeightKg,
    string? Reason);

public sealed class AdjustOrderShippingCommandHandler
    : IRequestHandler<AdjustOrderShippingCommand, AdjustOrderShippingResult?>
{
    private readonly ICustomerOrderRepository _orderRepository;
    private readonly IShippingRateBracketRepository _bracketRepository;
    private readonly IShippingPricingSettingsRepository _settingsRepository;
    private readonly IAuditLogger _auditLogger;

    public AdjustOrderShippingCommandHandler(
        ICustomerOrderRepository orderRepository,
        IShippingRateBracketRepository bracketRepository,
        IShippingPricingSettingsRepository settingsRepository,
        IAuditLogger auditLogger)
    {
        _orderRepository = orderRepository;
        _bracketRepository = bracketRepository;
        _settingsRepository = settingsRepository;
        _auditLogger = auditLogger;
    }

    public async Task<AdjustOrderShippingResult?> Handle(
        AdjustOrderShippingCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdForUpdateAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        decimal newFee;
        if (request.ManualShippingFee.HasValue)
        {
            newFee = request.ManualShippingFee.Value;
        }
        else if (request.ActualWeightKg.HasValue)
        {
            var weight = request.ActualWeightKg.Value;
            if (weight <= 0)
            {
                throw new DomainException("Actual weight must be positive.");
            }

            var bracket = await _bracketRepository.GetActiveForWeightAsync(
                order.ShippingAddress.CountryCode,
                weight,
                cancellationToken)
                ?? throw new DomainException(
                    $"No shipping rate for {order.ShippingAddress.CountryCode} at {weight:0.###} kg.");

            var settings = await _settingsRepository.GetOrCreateAsync(cancellationToken);
            var quote = ShippingQuoteCalculator.Calculate(
                order.ShippingAddress.CountryCode,
                weight,
                bracket.Price,
                settings.ErrorMarginPercent,
                bracket.Currency,
                bracket.Id);
            newFee = quote.ShippingFee;
        }
        else
        {
            throw new DomainException("Provide either actualWeightKg or manualShippingFee.");
        }

        order.AdjustShippingFee(newFee, request.Reason);
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "OrderShippingAdjusted",
            "CustomerOrder",
            order.Id,
            $"{{\"shippingFee\":{order.ShippingFee},\"quoted\":{order.ShippingFeeQuoted}}}",
            cancellationToken);

        return new AdjustOrderShippingResult(
            order.Id,
            order.ShippingFeeQuoted,
            order.ShippingFee,
            order.Subtotal,
            order.Total,
            request.ActualWeightKg,
            order.ShippingAdjustmentReason);
    }
}
