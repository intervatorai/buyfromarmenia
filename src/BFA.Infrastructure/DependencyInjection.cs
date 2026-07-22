using Microsoft.Extensions.DependencyInjection;
using BFA.BuildingBlocks.Application;
using BFA.Infrastructure.Ai;
using BFA.Infrastructure.Fulfillment;
using BFA.Infrastructure.Media;
using BFA.Infrastructure.Notifications;
using BFA.Infrastructure.Outbox;
using BFA.Infrastructure.Search;

namespace BFA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient(nameof(ProductSearchKeywordGenerator));
        services.AddHttpClient(nameof(OpenAiProductCopyGenerator));
        services.AddScoped<IProductSearchKeywordGenerator, ProductSearchKeywordGenerator>();
        services.AddScoped<IProductCopyGenerator, OpenAiProductCopyGenerator>();
        services.AddSingleton<IMediaUrlResolver, MediaUrlResolver>();
        services.AddSingleton<IBlobStorage, R2BlobStorage>();
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();
        services.AddScoped<ISupplierOrderWarehouseTransferService, SupplierOrderWarehouseTransferService>();
        services.AddScoped<IOrderFulfillmentOrchestrator, OrderFulfillmentOrchestrator>();
        services.AddScoped<IOrderNotificationSender, LoggingOrderNotificationSender>();
        return services;
    }
}
