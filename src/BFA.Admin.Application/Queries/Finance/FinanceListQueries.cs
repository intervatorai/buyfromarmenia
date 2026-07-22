using BFA.Modules.Settlements.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Finance;

public record GetSettlementsQuery() : IRequest<IReadOnlyList<SettlementListItemDto>>;

public record SettlementListItemDto(
    Guid Id,
    Guid SupplierId,
    Guid SupplierOrderId,
    decimal GrossAmount,
    decimal PlatformFee,
    decimal NetAmount,
    string Currency,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? EligibleAtUtc);

public sealed class GetSettlementsQueryHandler
    : IRequestHandler<GetSettlementsQuery, IReadOnlyList<SettlementListItemDto>>
{
    private readonly ISupplierSettlementRepository _settlementRepository;

    public GetSettlementsQueryHandler(ISupplierSettlementRepository settlementRepository)
    {
        _settlementRepository = settlementRepository;
    }

    public async Task<IReadOnlyList<SettlementListItemDto>> Handle(
        GetSettlementsQuery request,
        CancellationToken cancellationToken)
    {
        var settlements = await _settlementRepository.GetAllAsync(cancellationToken);

        return settlements.Select(settlement => new SettlementListItemDto(
            settlement.Id,
            settlement.SupplierId,
            settlement.SupplierOrderId,
            settlement.GrossAmount,
            settlement.PlatformFee,
            settlement.NetAmount,
            settlement.Currency,
            settlement.Status.ToString(),
            settlement.CreatedAtUtc,
            settlement.EligibleAtUtc)).ToList();
    }
}

public record GetPayoutsQuery() : IRequest<IReadOnlyList<PayoutListItemDto>>;

public record PayoutListItemDto(
    Guid Id,
    Guid SupplierId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime ScheduledForUtc,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);

public sealed class GetPayoutsQueryHandler
    : IRequestHandler<GetPayoutsQuery, IReadOnlyList<PayoutListItemDto>>
{
    private readonly IPayoutRepository _payoutRepository;

    public GetPayoutsQueryHandler(IPayoutRepository payoutRepository)
    {
        _payoutRepository = payoutRepository;
    }

    public async Task<IReadOnlyList<PayoutListItemDto>> Handle(
        GetPayoutsQuery request,
        CancellationToken cancellationToken)
    {
        var payouts = await _payoutRepository.GetAllAsync(cancellationToken);

        return payouts.Select(payout => new PayoutListItemDto(
            payout.Id,
            payout.SupplierId,
            payout.Amount,
            payout.Currency,
            payout.Status.ToString(),
            payout.ScheduledForUtc,
            payout.CreatedAtUtc,
            payout.CompletedAtUtc)).ToList();
    }
}
