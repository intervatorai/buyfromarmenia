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
        await _dbContext.Database.ExecuteSqlRawAsync(
            "CREATE SCHEMA IF NOT EXISTS identity;",
            cancellationToken);
        await _dbContext.Database.ExecuteSqlRawAsync(
            "CREATE SCHEMA IF NOT EXISTS suppliers;",
            cancellationToken);
        await _dbContext.Database.ExecuteSqlRawAsync(
            "CREATE SCHEMA IF NOT EXISTS catalog;",
            cancellationToken);
        await _dbContext.Database.ExecuteSqlRawAsync(
            "CREATE SCHEMA IF NOT EXISTS shipping;",
            cancellationToken);
    }
}
