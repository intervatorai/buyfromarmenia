namespace BFA.Hangfire.Application;

public static class JobIds
{
    public const string ApplyDatabaseMigrations = "apply-database-migrations";
    public const string EnsureDefaultSuperAdmin = "ensure-default-super-admin";
    public const string SeedDefaultCategories = "seed-default-categories";
    public const string SeedDefaultTradeRestrictions = "seed-default-trade-restrictions";
    public const string ProcessOutboxMessages = "process-outbox-messages";
}
