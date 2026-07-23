using BFA.Hangfire.Application;
using BFA.Hangfire.Application.Jobs;
using BFA.Infrastructure;
using BFA.Infrastructure.Auth;
using BFA.Persistence;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = PostgresConnectionString.Normalize(
    builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddOpenApi();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure();
builder.Services.AddAuthServices();
builder.Services.AddHangfireApplication();

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Apply EF migrations before Hangfire starts processing jobs (avoids outbox/table races).
using (var scope = app.Services.CreateScope())
{
    var migrations = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();
    await migrations.ApplyMigrationsAsync();
}

app.UseHangfireDashboard("/hangfire");

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<ApplyDatabaseMigrationsJob>(
        JobIds.ApplyDatabaseMigrations,
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Hourly);

    recurringJobManager.AddOrUpdate<EnsureDefaultSuperAdminJob>(
        JobIds.EnsureDefaultSuperAdmin,
        job => job.ExecuteAsync(CancellationToken.None),
        "*/5 * * * *");

    recurringJobManager.AddOrUpdate<ProcessOutboxMessagesJob>(
        JobIds.ProcessOutboxMessages,
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Minutely);

    BackgroundJob.Enqueue<EnsureDefaultSuperAdminJob>(
        job => job.ExecuteAsync(CancellationToken.None));
    BackgroundJob.Enqueue<SeedDefaultCategoriesJob>(
        job => job.ExecuteAsync(CancellationToken.None));
    BackgroundJob.Enqueue<SeedDefaultTradeRestrictionsJob>(
        job => job.ExecuteAsync(CancellationToken.None));
}

app.MapGet("/", () => Results.Redirect("/hangfire"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
