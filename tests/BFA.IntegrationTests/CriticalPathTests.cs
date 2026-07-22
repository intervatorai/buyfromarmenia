using System.Net;
using System.Net.Http.Json;
using BFA.BuildingBlocks.Application;
using BFA.Modules.Fulfillment.Domain.Repositories;
using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Warehouse.Domain.Repositories;
using BFA.Public.Application.Commands.Orders;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BFA.IntegrationTests;

[Collection("Integration")]
public sealed class CriticalPathTests
{
    private readonly IntegrationTestFixture _fixture;

    public CriticalPathTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CustomerAuth_RegisterAndLogin_WorksOverHttp()
    {
        if (!_fixture.IsDatabaseAvailable)
        {
            return;
        }

        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var email = $"buyer-{Guid.NewGuid():N}@test.bfa";
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Password123!",
            fullName = "Test Buyer",
            phone = "+10000000000",
        });

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "Password123!",
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(loginPayload?.AccessToken));
    }

    [Fact]
    public async Task PlaceOrder_CreatesCustomerOrderAndSupplierOrders()
    {
        if (!_fixture.IsDatabaseAvailable)
        {
            return;
        }

        var seed = await MarketplaceTestSeeder.SeedAsync(_fixture.Services);

        using var scope = _fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new PlaceOrderCommand(
            seed.CartId,
            "buyer@test.bfa",
            "Test Buyer",
            "AM",
            "Yerevan",
            "Test Street 1",
            Line2: null,
            PostalCode: "0001",
            Region: null));

        Assert.IsType<PlaceOrderSuccess>(result);

        var success = (PlaceOrderSuccess)result;
        var customerOrderRepository = scope.ServiceProvider.GetRequiredService<ICustomerOrderRepository>();
        var supplierOrderRepository = scope.ServiceProvider.GetRequiredService<ISupplierOrderRepository>();

        var order = await customerOrderRepository.GetByIdAsync(success.OrderId);
        Assert.NotNull(order);
        Assert.Equal("Paid", order!.PaymentStatus.ToString());

        var supplierOrders = await supplierOrderRepository.GetByCustomerOrderIdAsync(success.OrderId);
        Assert.Single(supplierOrders);
        Assert.Equal(seed.SupplierId, supplierOrders[0].SupplierId);
        Assert.Equal(2, supplierOrders[0].Items.Sum(item => item.Quantity));
    }

    [Fact]
    public async Task OrderFulfillmentOrchestrator_CreatesInboundShipments()
    {
        if (!_fixture.IsDatabaseAvailable)
        {
            return;
        }

        var seed = await MarketplaceTestSeeder.SeedAsync(_fixture.Services);

        using var scope = _fixture.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new PlaceOrderCommand(
            seed.CartId,
            "buyer-fulfillment@test.bfa",
            "Fulfillment Buyer",
            "AM",
            "Yerevan",
            "Fulfillment Street 1",
            Line2: null,
            PostalCode: "0002",
            Region: null));

        Assert.IsType<PlaceOrderSuccess>(result);
        var success = (PlaceOrderSuccess)result;

        var orchestrator = scope.ServiceProvider.GetRequiredService<IOrderFulfillmentOrchestrator>();
        await orchestrator.StartForOrderAsync(success.OrderId);

        var supplierOrderRepository = scope.ServiceProvider.GetRequiredService<ISupplierOrderRepository>();
        var inboundShipmentRepository = scope.ServiceProvider.GetRequiredService<IInboundShipmentRepository>();

        var supplierOrders = await supplierOrderRepository.GetByCustomerOrderIdAsync(success.OrderId);
        Assert.Single(supplierOrders);
        Assert.Equal("TransferredToWarehouse", supplierOrders[0].Status.ToString());

        var inbound = await inboundShipmentRepository.GetBySupplierOrderIdAsync(supplierOrders[0].Id);
        Assert.NotNull(inbound);
        Assert.Equal(success.OrderId, inbound!.CustomerOrderId);
        Assert.Equal("Pending", inbound.Status.ToString());
    }

    private sealed record AuthResponse(string AccessToken);
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
{
}
