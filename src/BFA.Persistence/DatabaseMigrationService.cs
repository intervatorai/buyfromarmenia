using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BFA.Persistence;

public class DatabaseMigrationService
{
    private readonly BfaDbContext _dbContext;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(
        BfaDbContext dbContext,
        ILogger<DatabaseMigrationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSchemasAsync(cancellationToken);
        await _dbContext.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Database migrations are up to date.");
    }

    private async Task EnsureSchemasAsync(CancellationToken cancellationToken)
    {
        string[] schemas =
        [
            "identity",
            "suppliers",
            "catalog",
            "shipping",
            "infrastructure",
            "inventory",
            "shopping",
            "ordering",
            "payments",
            "fulfillment",
            "warehouse",
            "settlements",
            "returns",
            "compliance"
        ];

        foreach (var schema in schemas)
        {
            // Schema names are fixed literals from the allow-list above.
#pragma warning disable EF1002
            await _dbContext.Database.ExecuteSqlRawAsync(
                $"CREATE SCHEMA IF NOT EXISTS {schema};",
                cancellationToken);
#pragma warning restore EF1002
        }
    }
}
