using BFA.BuildingBlocks.Application;
using BFA.Modules.Settlements.Domain.Aggregates;
using BFA.Modules.Settlements.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Finance;

public record MarkSettlementEligibleCommand(Guid SettlementId) : IRequest<bool>;

public sealed class MarkSettlementEligibleCommandHandler
    : IRequestHandler<MarkSettlementEligibleCommand, bool>
{
    private readonly ISupplierSettlementRepository _settlementRepository;
    private readonly IAuditLogger _auditLogger;

    public MarkSettlementEligibleCommandHandler(
        ISupplierSettlementRepository settlementRepository,
        IAuditLogger auditLogger)
    {
        _settlementRepository = settlementRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        MarkSettlementEligibleCommand request,
        CancellationToken cancellationToken)
    {
        var settlement = await _settlementRepository.GetByIdForUpdateAsync(
            request.SettlementId,
            cancellationToken);
        if (settlement is null)
        {
            return false;
        }

        settlement.MarkEligible();
        await _settlementRepository.UpdateAsync(settlement, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "SettlementMarkedEligible",
            "SupplierSettlement",
            settlement.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record CreatePayoutFromSettlementCommand(Guid SettlementId) : IRequest<Guid?>;

public sealed class CreatePayoutFromSettlementCommandHandler
    : IRequestHandler<CreatePayoutFromSettlementCommand, Guid?>
{
    private readonly ISupplierSettlementRepository _settlementRepository;
    private readonly IPayoutRepository _payoutRepository;
    private readonly IAuditLogger _auditLogger;

    public CreatePayoutFromSettlementCommandHandler(
        ISupplierSettlementRepository settlementRepository,
        IPayoutRepository payoutRepository,
        IAuditLogger auditLogger)
    {
        _settlementRepository = settlementRepository;
        _payoutRepository = payoutRepository;
        _auditLogger = auditLogger;
    }

    public async Task<Guid?> Handle(
        CreatePayoutFromSettlementCommand request,
        CancellationToken cancellationToken)
    {
        var settlement = await _settlementRepository.GetByIdForUpdateAsync(
            request.SettlementId,
            cancellationToken);
        if (settlement is null)
        {
            return null;
        }

        settlement.MarkPaid();
        await _settlementRepository.UpdateAsync(settlement, cancellationToken);

        var payout = new Payout(
            settlement.SupplierId,
            settlement.NetAmount,
            settlement.Currency,
            DateTime.UtcNow);

        await _payoutRepository.AddAsync(payout, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "PayoutCreatedFromSettlement",
            "Payout",
            payout.Id,
            cancellationToken: cancellationToken);

        return payout.Id;
    }
}

public record CompletePayoutCommand(Guid PayoutId) : IRequest<bool>;

public sealed class CompletePayoutCommandHandler : IRequestHandler<CompletePayoutCommand, bool>
{
    private readonly IPayoutRepository _payoutRepository;
    private readonly IAuditLogger _auditLogger;

    public CompletePayoutCommandHandler(
        IPayoutRepository payoutRepository,
        IAuditLogger auditLogger)
    {
        _payoutRepository = payoutRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(CompletePayoutCommand request, CancellationToken cancellationToken)
    {
        var payout = await _payoutRepository.GetByIdForUpdateAsync(request.PayoutId, cancellationToken);
        if (payout is null)
        {
            return false;
        }

        payout.MarkCompleted();
        await _payoutRepository.UpdateAsync(payout, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "PayoutCompleted",
            "Payout",
            payout.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}
