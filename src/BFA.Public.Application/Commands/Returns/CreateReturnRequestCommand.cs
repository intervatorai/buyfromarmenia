using BFA.Modules.Ordering.Domain.Repositories;
using BFA.Modules.Returns.Domain.Aggregates;
using BFA.Modules.Returns.Domain.Repositories;
using MediatR;

namespace BFA.Public.Application.Commands.Returns;

public record CreateReturnRequestCommand(
    Guid CustomerOrderId,
    string CustomerEmail,
    string Reason,
    Guid? CustomerUserId = null) : IRequest<CreateReturnRequestResult?>;

public record CreateReturnRequestResult(Guid ReturnRequestId, string Status);

public sealed class CreateReturnRequestCommandHandler
    : IRequestHandler<CreateReturnRequestCommand, CreateReturnRequestResult?>
{
    private readonly ICustomerOrderRepository _customerOrderRepository;
    private readonly IReturnRequestRepository _returnRequestRepository;

    public CreateReturnRequestCommandHandler(
        ICustomerOrderRepository customerOrderRepository,
        IReturnRequestRepository returnRequestRepository)
    {
        _customerOrderRepository = customerOrderRepository;
        _returnRequestRepository = returnRequestRepository;
    }

    public async Task<CreateReturnRequestResult?> Handle(
        CreateReturnRequestCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _customerOrderRepository.GetByIdAsync(
            request.CustomerOrderId,
            cancellationToken);

        if (order is null)
        {
            return null;
        }

        if (!order.CustomerEmail.Equals(
                request.CustomerEmail.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var existing = await _returnRequestRepository.GetOpenByCustomerOrderIdAsync(
            request.CustomerOrderId,
            cancellationToken);

        if (existing is not null)
        {
            return null;
        }

        var returnRequest = ReturnRequest.Create(
            request.CustomerOrderId,
            request.CustomerEmail,
            request.Reason,
            request.CustomerUserId);

        await _returnRequestRepository.AddAsync(returnRequest, cancellationToken);

        return new CreateReturnRequestResult(
            returnRequest.Id,
            returnRequest.Status.ToString());
    }
}
