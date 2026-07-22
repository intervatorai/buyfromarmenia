using BFA.BuildingBlocks.Application;
using BFA.Modules.Compliance.Domain.Aggregates;
using BFA.Modules.Compliance.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Compliance;

public record CreateTradeRestrictionCommand(
    string DestinationCountryCode,
    string Reason,
    Guid? CategoryId = null) : IRequest<Guid>;

public sealed class CreateTradeRestrictionCommandHandler
    : IRequestHandler<CreateTradeRestrictionCommand, Guid>
{
    private readonly ITradeRestrictionRepository _tradeRestrictionRepository;
    private readonly IAuditLogger _auditLogger;

    public CreateTradeRestrictionCommandHandler(
        ITradeRestrictionRepository tradeRestrictionRepository,
        IAuditLogger auditLogger)
    {
        _tradeRestrictionRepository = tradeRestrictionRepository;
        _auditLogger = auditLogger;
    }

    public async Task<Guid> Handle(
        CreateTradeRestrictionCommand request,
        CancellationToken cancellationToken)
    {
        var restriction = TradeRestriction.Create(
            request.DestinationCountryCode,
            request.Reason,
            request.CategoryId);

        await _tradeRestrictionRepository.AddAsync(restriction, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "TradeRestrictionCreated",
            "TradeRestriction",
            restriction.Id,
            cancellationToken: cancellationToken);

        return restriction.Id;
    }
}

public record DeactivateTradeRestrictionCommand(Guid RestrictionId) : IRequest<bool>;

public sealed class DeactivateTradeRestrictionCommandHandler
    : IRequestHandler<DeactivateTradeRestrictionCommand, bool>
{
    private readonly ITradeRestrictionRepository _tradeRestrictionRepository;
    private readonly IAuditLogger _auditLogger;

    public DeactivateTradeRestrictionCommandHandler(
        ITradeRestrictionRepository tradeRestrictionRepository,
        IAuditLogger auditLogger)
    {
        _tradeRestrictionRepository = tradeRestrictionRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        DeactivateTradeRestrictionCommand request,
        CancellationToken cancellationToken)
    {
        var restriction = await _tradeRestrictionRepository.GetByIdForUpdateAsync(
            request.RestrictionId,
            cancellationToken);

        if (restriction is null || !restriction.IsActive)
        {
            return false;
        }

        restriction.Deactivate();
        await _tradeRestrictionRepository.UpdateAsync(restriction, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "TradeRestrictionDeactivated",
            "TradeRestriction",
            restriction.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}
