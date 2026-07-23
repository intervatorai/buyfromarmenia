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
    public CustomerTrackingStage TrackingStage { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal EstimatedWeightKg { get; private set; }
    public decimal ShippingFeeQuoted { get; private set; }
    public decimal ShippingMarginPercent { get; private set; }
    public decimal ShippingFee { get; private set; }
    public decimal Total => Subtotal + ShippingFee;
    public string? ShippingAdjustmentReason { get; private set; }
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
        decimal estimatedWeightKg,
        decimal shippingFeeQuoted,
        decimal shippingMarginPercent,
        decimal shippingFee,
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

        if (estimatedWeightKg <= 0)
        {
            throw new DomainException("Estimated shipping weight must be positive.");
        }

        if (shippingFeeQuoted < 0 || shippingFee < 0)
        {
            throw new DomainException("Shipping fee cannot be negative.");
        }

        if (shippingMarginPercent < 0)
        {
            throw new DomainException("Shipping margin percent cannot be negative.");
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
        TrackingStage = CustomerTrackingStage.OrderPlaced;
        Currency = currencies[0];
        Subtotal = items.Sum(item => item.UnitPrice * item.Quantity);
        EstimatedWeightKg = estimatedWeightKg;
        ShippingFeeQuoted = shippingFeeQuoted;
        ShippingMarginPercent = shippingMarginPercent;
        ShippingFee = shippingFee;
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

    public void AdjustShippingFee(decimal newShippingFee, string? reason)
    {
        if (newShippingFee < 0)
        {
            throw new DomainException("Shipping fee cannot be negative.");
        }

        ShippingFee = newShippingFee;
        ShippingAdjustmentReason = string.IsNullOrWhiteSpace(reason)
            ? null
            : reason.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
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
        if (TrackingStage < CustomerTrackingStage.Confirmed)
        {
            TrackingStage = CustomerTrackingStage.Confirmed;
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        if (Status == OrderStatus.Completed)
        {
            return;
        }

        if (Status is OrderStatus.Cancelled)
        {
            throw new DomainException("Cancelled orders cannot be completed.");
        }

        if (Status is not (OrderStatus.Confirmed or OrderStatus.Placed))
        {
            throw new DomainException("Only confirmed (or placed) orders can be completed.");
        }

        if (Status == OrderStatus.Placed && PaymentStatus != PaymentStatus.Paid)
        {
            throw new DomainException("Order must be paid before completion.");
        }

        Status = OrderStatus.Completed;
        FulfillmentStatus = FulfillmentStatus.Completed;
        TrackingStage = CustomerTrackingStage.Delivered;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Completed or OrderStatus.Cancelled)
        {
            throw new DomainException("Completed or cancelled orders cannot be cancelled.");
        }

        Status = OrderStatus.Cancelled;
        FulfillmentStatus = FulfillmentStatus.Cancelled;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Admin override for order lifecycle. Keeps payment/fulfillment consistent.
    /// </summary>
    public void ChangeStatusAsAdmin(OrderStatus newStatus)
    {
        if (Status == newStatus)
        {
            return;
        }

        switch (newStatus)
        {
            case OrderStatus.Confirmed:
                if (Status != OrderStatus.Placed)
                {
                    throw new DomainException("Only placed orders can be confirmed.");
                }

                if (PaymentStatus == PaymentStatus.Pending)
                {
                    PaymentStatus = PaymentStatus.Paid;
                }

                if (PaymentStatus is PaymentStatus.Failed)
                {
                    throw new DomainException("Cannot confirm an order with failed payment.");
                }

                Status = OrderStatus.Confirmed;
                FulfillmentStatus = FulfillmentStatus.InProgress;
                if (TrackingStage < CustomerTrackingStage.Confirmed)
                {
                    TrackingStage = CustomerTrackingStage.Confirmed;
                }

                break;

            case OrderStatus.Completed:
                if (Status is OrderStatus.Cancelled)
                {
                    throw new DomainException("Cancelled orders cannot be completed.");
                }

                if (PaymentStatus == PaymentStatus.Pending)
                {
                    PaymentStatus = PaymentStatus.Paid;
                }

                if (PaymentStatus is PaymentStatus.Failed)
                {
                    throw new DomainException("Cannot complete an order with failed payment.");
                }

                Status = OrderStatus.Completed;
                FulfillmentStatus = FulfillmentStatus.Completed;
                TrackingStage = CustomerTrackingStage.Delivered;
                break;

            case OrderStatus.Cancelled:
                Cancel();
                return;

            case OrderStatus.Placed:
                throw new DomainException("Cannot move an order back to Placed.");

            default:
                throw new DomainException($"Unsupported order status '{newStatus}'.");
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangePaymentStatusAsAdmin(PaymentStatus newStatus)
    {
        if (PaymentStatus == newStatus)
        {
            return;
        }

        switch (newStatus)
        {
            case PaymentStatus.Paid:
                if (PaymentStatus is not (PaymentStatus.Pending or PaymentStatus.Failed))
                {
                    throw new DomainException("Only pending or failed payments can be marked as paid.");
                }

                PaymentStatus = PaymentStatus.Paid;
                break;

            case PaymentStatus.Failed:
                if (PaymentStatus != PaymentStatus.Pending)
                {
                    throw new DomainException("Only pending payments can be marked as failed.");
                }

                PaymentStatus = PaymentStatus.Failed;
                break;

            case PaymentStatus.Refunded:
                if (PaymentStatus != PaymentStatus.Paid)
                {
                    throw new DomainException("Only paid payments can be refunded.");
                }

                PaymentStatus = PaymentStatus.Refunded;
                break;

            case PaymentStatus.Pending:
                throw new DomainException("Cannot move payment back to Pending.");

            default:
                throw new DomainException($"Unsupported payment status '{newStatus}'.");
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Admin override for the customer-facing tracking timeline (8 stages).
    /// </summary>
    public void SetTrackingStageAsAdmin(CustomerTrackingStage stage)
    {
        TrackingStage = stage;
        UpdatedAtUtc = DateTime.UtcNow;

        if (stage >= CustomerTrackingStage.Confirmed
            && Status == OrderStatus.Placed
            && PaymentStatus != PaymentStatus.Failed)
        {
            if (PaymentStatus == PaymentStatus.Pending)
            {
                PaymentStatus = PaymentStatus.Paid;
            }

            Status = OrderStatus.Confirmed;
            FulfillmentStatus = FulfillmentStatus.InProgress;
        }

        if (stage == CustomerTrackingStage.Delivered
            && Status != OrderStatus.Cancelled)
        {
            if (PaymentStatus == PaymentStatus.Pending)
            {
                PaymentStatus = PaymentStatus.Paid;
            }

            Status = OrderStatus.Completed;
            FulfillmentStatus = FulfillmentStatus.Completed;
        }
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
