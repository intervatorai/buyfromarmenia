using BFA.BuildingBlocks.Application;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Fulfillment.Domain.Repositories;
using BFA.Modules.Identity.Domain.Repositories;
using BFA.Modules.Inventory.Domain.Repositories;
using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Payments.Domain.Repositories;
using BFA.Modules.Shopping.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.Repositories;
using BFA.Modules.Warehouse.Domain.Repositories;
using BFA.Modules.Shipping.Domain.Repositories;
using BFA.Modules.Settlements.Domain.Repositories;
using BFA.Modules.Returns.Domain.Repositories;
using BFA.Modules.Compliance.Domain.Repositories;
using BFA.Modules.Compliance.Domain.Services;
using BFA.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BFA.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<BfaDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "public")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();
        services.AddScoped<ICustomerDeliveryAddressRepository, CustomerDeliveryAddressRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<ISupplierMemberRepository, SupplierMemberRepository>();
        services.AddScoped<IStockItemRepository, StockItemRepository>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<ICustomerOrderRepository, CustomerOrderRepository>();
        services.AddScoped<ISupplierOrderRepository, SupplierOrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IInboundShipmentRepository, InboundShipmentRepository>();
        services.AddScoped<IConsolidationRepository, ConsolidationRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<ISupplierSettlementRepository, SupplierSettlementRepository>();
        services.AddScoped<IPayoutRepository, PayoutRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IOutboxReader, OutboxStore>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IReturnRequestRepository, ReturnRequestRepository>();
        services.AddScoped<ITradeRestrictionRepository, TradeRestrictionRepository>();
        services.AddScoped<IExportComplianceValidator, ExportComplianceValidator>();

        services.AddScoped<DatabaseMigrationService>();

        return services;
    }
}
