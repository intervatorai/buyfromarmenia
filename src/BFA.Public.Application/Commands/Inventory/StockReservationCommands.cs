using BFA.Modules.Inventory.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Commands.Inventory;

public record ReserveStockCommand(
    Guid ProductVariantId,
    Guid ReferenceId,
    int Quantity,
    DateTime ExpiresAtUtc) : IRequest<Guid?>;

public sealed class ReserveStockCommandHandler
    : IRequestHandler<ReserveStockCommand, Guid?>
{
    private readonly IStockItemRepository _stockItemRepository;

    public ReserveStockCommandHandler(IStockItemRepository stockItemRepository)
    {
        _stockItemRepository = stockItemRepository;
    }

    public async Task<Guid?> Handle(
        ReserveStockCommand request,
        CancellationToken cancellationToken)
    {
        var stockItem = await _stockItemRepository.GetByVariantIdForUpdateAsync(
            request.ProductVariantId,
            cancellationToken);
        if (stockItem is null || stockItem.Available < request.Quantity)
        {
            return null;
        }

        var reservation = stockItem.Reserve(
            request.ReferenceId,
            request.Quantity,
            request.ExpiresAtUtc);
        await _stockItemRepository.UpdateAsync(stockItem, cancellationToken);
        return reservation.Id;
    }
}

public record ReleaseStockReservationCommand(
    Guid StockItemId,
    Guid ReservationId) : IRequest<bool>;

public sealed class ReleaseStockReservationCommandHandler
    : IRequestHandler<ReleaseStockReservationCommand, bool>
{
    private readonly IStockItemRepository _stockItemRepository;

    public ReleaseStockReservationCommandHandler(
        IStockItemRepository stockItemRepository)
    {
        _stockItemRepository = stockItemRepository;
    }

    public async Task<bool> Handle(
        ReleaseStockReservationCommand request,
        CancellationToken cancellationToken)
    {
        var stockItem = await _stockItemRepository.GetByIdForUpdateAsync(
            request.StockItemId,
            cancellationToken);
        if (stockItem is null)
        {
            return false;
        }

        stockItem.ReleaseReservation(request.ReservationId);
        await _stockItemRepository.UpdateAsync(stockItem, cancellationToken);
        return true;
    }
}

public record ConfirmStockReservationCommand(
    Guid StockItemId,
    Guid ReservationId) : IRequest<bool>;

public sealed class ConfirmStockReservationCommandHandler
    : IRequestHandler<ConfirmStockReservationCommand, bool>
{
    private readonly IStockItemRepository _stockItemRepository;

    public ConfirmStockReservationCommandHandler(
        IStockItemRepository stockItemRepository)
    {
        _stockItemRepository = stockItemRepository;
    }

    public async Task<bool> Handle(
        ConfirmStockReservationCommand request,
        CancellationToken cancellationToken)
    {
        var stockItem = await _stockItemRepository.GetByIdForUpdateAsync(
            request.StockItemId,
            cancellationToken);
        if (stockItem is null)
        {
            return false;
        }

        stockItem.ConfirmReservation(request.ReservationId);
        await _stockItemRepository.UpdateAsync(stockItem, cancellationToken);
        return true;
    }
}
