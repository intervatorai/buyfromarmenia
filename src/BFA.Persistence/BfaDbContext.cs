using BFA.BuildingBlocks.Domain;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Fulfillment.Domain.Aggregates;
using BFA.Modules.Identity.Domain.Aggregates;
using BFA.Modules.Inventory.Domain.Aggregates;
using BFA.Modules.Ordering.Domain.Aggregates;
using BFA.Modules.Payments.Domain.Aggregates;
using BFA.Modules.Shopping.Domain.Aggregates;
using BFA.Modules.Suppliers.Domain.Aggregates;
using BFA.Modules.Warehouse.Domain.Aggregates;
using BFA.Modules.Shipping.Domain.Aggregates;
using BFA.Modules.Settlements.Domain.Aggregates;
using BFA.Modules.Returns.Domain.Aggregates;
using BFA.Modules.Compliance.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace BFA.Persistence;

public class BfaDbContext : DbContext
{
    public BfaDbContext(DbContextOptions<BfaDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductMedia> ProductMedia => Set<ProductMedia>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<ProductDocument> ProductDocuments => Set<ProductDocument>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<User> Users => Set<User>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<CustomerDeliveryAddress> CustomerDeliveryAddresses => Set<CustomerDeliveryAddress>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierMember> SupplierMembers => Set<SupplierMember>();
    public DbSet<SupplierDocument> SupplierDocuments => Set<SupplierDocument>();
    public DbSet<SupplierBankAccount> SupplierBankAccounts => Set<SupplierBankAccount>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<ShoppingCartItem> ShoppingCartItems => Set<ShoppingCartItem>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();
    public DbSet<CustomerOrderItem> CustomerOrderItems => Set<CustomerOrderItem>();
    public DbSet<SupplierOrder> SupplierOrders => Set<SupplierOrder>();
    public DbSet<SupplierOrderItem> SupplierOrderItems => Set<SupplierOrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InboundShipment> InboundShipments => Set<InboundShipment>();
    public DbSet<Consolidation> Consolidations => Set<Consolidation>();
    public DbSet<ConsolidationItem> ConsolidationItems => Set<ConsolidationItem>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShippingCountry> ShippingCountries => Set<ShippingCountry>();
    public DbSet<ShippingRateBracket> ShippingRateBrackets => Set<ShippingRateBracket>();
    public DbSet<ShippingPricingSettings> ShippingPricingSettings => Set<ShippingPricingSettings>();
    public DbSet<SupplierSettlement> SupplierSettlements => Set<SupplierSettlement>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<ReturnRequest> ReturnRequests => Set<ReturnRequest>();
    public DbSet<TradeRestriction> TradeRestrictions => Set<TradeRestriction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BfaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
