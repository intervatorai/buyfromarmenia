using BFA.Modules.Compliance.Domain.Aggregates;
using BFA.Modules.Compliance.Domain.Repositories;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace BFA.Hangfire.Application.Jobs;

public class SeedDefaultTradeRestrictionsJob
{
    private readonly ITradeRestrictionRepository _tradeRestrictionRepository;
    private readonly ILogger<SeedDefaultTradeRestrictionsJob> _logger;

    public SeedDefaultTradeRestrictionsJob(
        ITradeRestrictionRepository tradeRestrictionRepository,
        ILogger<SeedDefaultTradeRestrictionsJob> logger)
    {
        _tradeRestrictionRepository = tradeRestrictionRepository;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _tradeRestrictionRepository.GetAllAsync(cancellationToken);
        if (existing.Count > 0)
        {
            return;
        }

        var restrictions = new[]
        {
            TradeRestriction.Create(
                "KP",
                "Exports to this destination are restricted by platform policy."),
            TradeRestriction.Create(
                "IR",
                "Exports to this destination are restricted by platform policy."),
        };

        foreach (var restriction in restrictions)
        {
            await _tradeRestrictionRepository.AddAsync(restriction, cancellationToken);
        }

        _logger.LogInformation("Seeded {Count} default trade restrictions.", restrictions.Length);
    }
}
