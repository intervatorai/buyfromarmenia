using BFA.Persistence;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace BFA.Hangfire.Application.Jobs;

public class ApplyDatabaseMigrationsJob
{
    private readonly DatabaseMigrationService _databaseMigrationService;
    private readonly ILogger<ApplyDatabaseMigrationsJob> _logger;

    public ApplyDatabaseMigrationsJob(
        DatabaseMigrationService databaseMigrationService,
        ILogger<ApplyDatabaseMigrationsJob> logger)
    {
        _databaseMigrationService = databaseMigrationService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Applying database migrations.");
        await _databaseMigrationService.ApplyMigrationsAsync(cancellationToken);
    }
}
