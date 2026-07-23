using BFA.Hangfire.Application;
using BFA.Hangfire.Application.Jobs;
using BFA.Infrastructure;
using BFA.Infrastructure.Auth;
using BFA.Persistence;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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

app.UseHangfireDashboard("/hangfire");

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<ApplyDatabaseMigrationsJob>(
        JobIds.ApplyDatabaseMigrations,
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Minutely);

    recurringJobManager.AddOrUpdate<EnsureDefaultSuperAdminJob>(
        JobIds.EnsureDefaultSuperAdmin,
        job => job.ExecuteAsync(CancellationToken.None),
        "*/5 * * * *");

    recurringJobManager.AddOrUpdate<ProcessOutboxMessagesJob>(
        JobIds.ProcessOutboxMessages,
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Minutely);

    var migrationJobId = BackgroundJob.Enqueue<ApplyDatabaseMigrationsJob>(
        job => job.ExecuteAsync(CancellationToken.None));

    BackgroundJob.ContinueJobWith<EnsureDefaultSuperAdminJob>(
        migrationJobId,
        job => job.ExecuteAsync(CancellationToken.None));

    BackgroundJob.ContinueJobWith<SeedDefaultCategoriesJob>(
        migrationJobId,
        job => job.ExecuteAsync(CancellationToken.None));

    BackgroundJob.ContinueJobWith<SeedDefaultTradeRestrictionsJob>(
        migrationJobId,
        job => job.ExecuteAsync(CancellationToken.None));
}

app.MapGet("/", () => Results.Redirect("/hangfire"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
