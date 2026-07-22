using BFA.BuildingBlocks.Domain;
using BFA.Modules.Ordering.Domain.Enums;
using BFA.Modules.Ordering.Domain.Events;

namespace BFA.Modules.Ordering.Domain.Aggregates;

public sealed class CustomerOrder : AggregateRoot
{
    private readonly List<CustomerOrderItem> _items = [];

    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CartId { get; private set; }
    public Guid? CustomerUserId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public string CustomerFullName { get; private set; } = string.Empty;
    public Address ShippingAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public FulfillmentStatus FulfillmentStatus { get; private set; }
    public decimal Subtotal { get; private set; }
    public string Currency { get; private set; } = "USD";
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<CustomerOrderItem> Items => _items.AsReadOnly();

    private CustomerOrder()
    {
    }

    public CustomerOrder(
        Guid cartId,
        string orderNumber,
        string customerEmail,
        string customerFullName,
        Address shippingAddress,
        IReadOnlyList<CustomerOrderItemDraft> items,
        Guid? customerUserId = null)
    {
        if (cartId == Guid.Empty)
        {
            throw new DomainException("Cart id is required.");
        }

        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("Order number is required.");
        }

        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new DomainException("Customer email is required.");
        }

        if (string.IsNullOrWhiteSpace(customerFullName))
        {
            throw new DomainException("Customer full name is required.");
        }

        if (items.Count == 0)
        {
            throw new DomainException("Order must contain at least one item.");
        }

        var currencies = items.Select(item => item.Currency).Distinct().ToList();
        if (currencies.Count != 1)
        {
            throw new DomainException("All order items must use the same currency.");
        }

        Id = Guid.NewGuid();
        CartId = cartId;
        CustomerUserId = customerUserId;
        OrderNumber = orderNumber.Trim();
        CustomerEmail = customerEmail.Trim();
        CustomerFullName = customerFullName.Trim();
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Placed;
        PaymentStatus = PaymentStatus.Pending;
        FulfillmentStatus = FulfillmentStatus.Pending;
        Currency = currencies[0];
        Subtotal = items.Sum(item => item.UnitPrice * item.Quantity);
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;

        foreach (var item in items)
        {
            _items.Add(new CustomerOrderItem(
                Id,
                item.ProductId,
                item.ProductVariantId,
                item.SupplierId,
                item.ProductName,
                item.SupplierSku,
                item.ImageUrl,
                item.UnitPrice,
                item.Currency,
                item.Quantity));
        }

        RaiseDomainEvent(new CustomerOrderPlacedDomainEvent(Id, OrderNumber, CartId));
    }

    public void MarkPaymentPaid()
    {
        if (PaymentStatus != PaymentStatus.Pending)
        {
            throw new DomainException("Only pending payments can be marked as paid.");
        }

        PaymentStatus = PaymentStatus.Paid;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkConfirmed()
    {
        if (Status != OrderStatus.Placed)
        {
            throw new DomainException("Only placed orders can be confirmed.");
        }

        if (PaymentStatus != PaymentStatus.Paid)
        {
            throw new DomainException("Order must be paid before confirmation.");
        }

        Status = OrderStatus.Confirmed;
        FulfillmentStatus = FulfillmentStatus.InProgress;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        Status = OrderStatus.Completed;
        FulfillmentStatus = FulfillmentStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

public sealed record CustomerOrderItemDraft(
    Guid ProductId,
    Guid ProductVariantId,
    Guid SupplierId,
    string ProductName,
    string SupplierSku,
    string? ImageUrl,
    decimal UnitPrice,
    string Currency,
    int Quantity);
