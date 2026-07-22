using BFA.Hangfire.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace BFA.Hangfire.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireApplication(this IServiceCollection services)
    {
        services.AddScoped<ApplyDatabaseMigrationsJob>();
        services.AddScoped<EnsureDefaultSuperAdminJob>();
        services.AddScoped<SeedDefaultCategoriesJob>();
        services.AddScoped<SeedDefaultTradeRestrictionsJob>();
        services.AddScoped<ProcessOutboxMessagesJob>();

        return services;
    }
}
