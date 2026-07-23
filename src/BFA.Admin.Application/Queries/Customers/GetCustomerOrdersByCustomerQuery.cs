using BFA.Modules.Identity.Domain.Enums;
using BFA.Modules.Identity.Domain.Repositories;
using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Payments.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Queries.Customers;

public record GetCustomerOrdersByCustomerQuery(Guid CustomerId)
    : IRequest<IReadOnlyList<CustomerOrderSummaryDto>?>;

public record CustomerOrderSummaryDto(
    Guid Id,
    string OrderNumber,
    string Status,
    string PaymentStatus,
    string FulfillmentStatus,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Total,
    string Currency,
    string? PaymentProvider,
    decimal? PaymentAmount,
    string? PaymentRecordStatus,
    DateTime? PaymentCapturedAtUtc,
    CustomerOrderAddressDto ShippingAddress,
    DateTime CreatedAtUtc);

public record CustomerOrderAddressDto(
    string CountryCode,
    string City,
    string Line1,
    string? Line2,
    string? PostalCode,
    string? Region);

public sealed class GetCustomerOrdersByCustomerQueryHandler
    : IRequestHandler<GetCustomerOrdersByCustomerQuery, IReadOnlyList<CustomerOrderSummaryDto>?>
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerOrderRepository _customerOrderRepository;
    private readonly IPaymentRepository _paymentRepository;

    public GetCustomerOrdersByCustomerQueryHandler(
        IUserRepository userRepository,
        ICustomerOrderRepository customerOrderRepository,
        IPaymentRepository paymentRepository)
    {
        _userRepository = userRepository;
        _customerOrderRepository = customerOrderRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<IReadOnlyList<CustomerOrderSummaryDto>?> Handle(
        GetCustomerOrdersByCustomerQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (user is null || user.Type != UserType.Customer)
        {
            return null;
        }

        var orders = await _customerOrderRepository.GetByCustomerUserIdAsync(
            request.CustomerId,
            cancellationToken);
        var payments = await _paymentRepository.GetByCustomerOrderIdsAsync(
            orders.Select(order => order.Id).ToList(),
            cancellationToken);
        var paymentByOrderId = payments.ToDictionary(payment => payment.CustomerOrderId);

        return orders
            .OrderByDescending(order => order.CreatedAtUtc)
            .Select(order =>
            {
                paymentByOrderId.TryGetValue(order.Id, out var payment);
                return new CustomerOrderSummaryDto(
                    order.Id,
                    order.OrderNumber,
                    order.Status.ToString(),
                    order.PaymentStatus.ToString(),
                    order.FulfillmentStatus.ToString(),
                    order.Subtotal,
                    order.ShippingFee,
                    order.Total,
                    order.Currency,
                    payment?.Provider.ToString(),
                    payment?.Amount,
                    payment?.Status.ToString(),
                    payment?.CapturedAtUtc,
                    new CustomerOrderAddressDto(
                        order.ShippingAddress.CountryCode,
                        order.ShippingAddress.City,
                        order.ShippingAddress.Line1,
                        order.ShippingAddress.Line2,
                        order.ShippingAddress.PostalCode,
                        order.ShippingAddress.Region),
                    order.CreatedAtUtc);
            })
            .ToList();
    }
}
