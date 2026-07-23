using BFA.BuildingBlocks.Domain;
using BFA.Infrastructure;
using BFA.Infrastructure.Auth;
using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Repositories;
using BFA.Modules.Fulfillment.Domain.Repositories;
using BFA.Modules.Inventory.Domain.Aggregates;
using BFA.Modules.Inventory.Domain.Repositories;
using BFA.Modules.Shopping.Domain.Aggregates;
using BFA.Modules.Shopping.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.Aggregates;
using BFA.Modules.Suppliers.Domain.Repositories;
using BFA.Modules.Suppliers.Domain.ValueObjects;
using BFA.Persistence;
using BFA.Public.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BFA.IntegrationTests;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    public IServiceProvider Services { get; private set; } = null!;
    public bool IsDatabaseAvailable { get; private set; }

    public async Task InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    Environment.GetEnvironmentVariable("BFA_TEST_CONNECTION")
                    ?? "Host=localhost;Port=5432;Database=bfa;Username=postgres;Password=postgres",
                ["Jwt:Secret"] = "integration-test-jwt-secret-32-characters",
                ["Jwt:Issuer"] = "BFA.Public.Api",
                ["Jwt:Audience"] = "BFA.Public.UI",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddPersistence(configuration);
        services.AddPublicApplication(configuration);
        services.AddInfrastructure();
        services.AddAuthServices();

        Services = services.BuildServiceProvider();

        try
        {
            using var scope = Services.CreateScope();
            var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();
            await migrationService.ApplyMigrationsAsync();
            IsDatabaseAvailable = true;
        }
        catch
        {
            IsDatabaseAvailable = false;
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public static class MarketplaceTestSeeder
{
    public static async Task<PlaceOrderSeedData> SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var supplierRepository = scope.ServiceProvider.GetRequiredService<ISupplierRepository>();
        var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var stockRepository = scope.ServiceProvider.GetRequiredService<IStockItemRepository>();
        var cartRepository = scope.ServiceProvider.GetRequiredService<IShoppingCartRepository>();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var contact = new ContactInformation("Test Owner", $"owner-{suffix}@test.bfa", "+37400000000");
        var supplier = Supplier.Register(
            $"Test Legal {suffix}",
            $"Test Shop {suffix}",
            contact);

        supplier.SubmitApplication();
        supplier.MarkUnderReview();
        supplier.Approve();
        await supplierRepository.AddAsync(supplier);

        var product = Product.Create(
            supplier.Id,
            new Money(19.99m, "USD"),
            "Test Honey",
            "Organic Armenian honey for integration tests.");

        var variant = product.AddVariant(
            $"SKU-{suffix}",
            0.5m,
            "AM");

        product.SubmitForReview();
        product.Approve();
        product.Publish();
        await productRepository.AddAsync(product);

        var stock = new StockItem(supplier.Id, product.Id, variant.Id, onHand: 25);
        await stockRepository.AddAsync(stock);

        var cartId = Guid.NewGuid();
        var cart = new ShoppingCart(cartId);
        cart.AddItem(
            product.Id,
            variant.Id,
            supplier.Id,
            "Test Honey",
            null,
            19.99m,
            "USD",
            2);
        await cartRepository.AddAsync(cart);

        return new PlaceOrderSeedData(cartId, supplier.Id, product.Id, variant.Id);
    }
}

public sealed record PlaceOrderSeedData(
    Guid CartId,
    Guid SupplierId,
    Guid ProductId,
    Guid ProductVariantId);
