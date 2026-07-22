using BFA.BuildingBlocks.Application;
using BFA.Modules.Returns.Domain.Repositories;
using MediatR;

namespace BFA.Admin.Application.Commands.Returns;

public record ApproveReturnRequestCommand(Guid ReturnRequestId, string? AdminNotes = null)
    : IRequest<bool>;

public sealed class ApproveReturnRequestCommandHandler
    : IRequestHandler<ApproveReturnRequestCommand, bool>
{
    private readonly IReturnRequestRepository _returnRequestRepository;
    private readonly IAuditLogger _auditLogger;

    public ApproveReturnRequestCommandHandler(
        IReturnRequestRepository returnRequestRepository,
        IAuditLogger auditLogger)
    {
        _returnRequestRepository = returnRequestRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        ApproveReturnRequestCommand request,
        CancellationToken cancellationToken)
    {
        var returnRequest = await _returnRequestRepository.GetByIdForUpdateAsync(
            request.ReturnRequestId,
            cancellationToken);

        if (returnRequest is null)
        {
            return false;
        }

        returnRequest.Approve(request.AdminNotes);
        await _returnRequestRepository.UpdateAsync(returnRequest, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ReturnApproved",
            "ReturnRequest",
            returnRequest.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record RejectReturnRequestCommand(Guid ReturnRequestId, string Reason) : IRequest<bool>;

public sealed class RejectReturnRequestCommandHandler
    : IRequestHandler<RejectReturnRequestCommand, bool>
{
    private readonly IReturnRequestRepository _returnRequestRepository;
    private readonly IAuditLogger _auditLogger;

    public RejectReturnRequestCommandHandler(
        IReturnRequestRepository returnRequestRepository,
        IAuditLogger auditLogger)
    {
        _returnRequestRepository = returnRequestRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        RejectReturnRequestCommand request,
        CancellationToken cancellationToken)
    {
        var returnRequest = await _returnRequestRepository.GetByIdForUpdateAsync(
            request.ReturnRequestId,
            cancellationToken);

        if (returnRequest is null)
        {
            return false;
        }

        returnRequest.Reject(request.Reason);
        await _returnRequestRepository.UpdateAsync(returnRequest, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ReturnRejected",
            "ReturnRequest",
            returnRequest.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record MarkReturnReceivedCommand(Guid ReturnRequestId, string? AdminNotes = null) : IRequest<bool>;

public sealed class MarkReturnReceivedCommandHandler
    : IRequestHandler<MarkReturnReceivedCommand, bool>
{
    private readonly IReturnRequestRepository _returnRequestRepository;
    private readonly IAuditLogger _auditLogger;

    public MarkReturnReceivedCommandHandler(
        IReturnRequestRepository returnRequestRepository,
        IAuditLogger auditLogger)
    {
        _returnRequestRepository = returnRequestRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        MarkReturnReceivedCommand request,
        CancellationToken cancellationToken)
    {
        var returnRequest = await _returnRequestRepository.GetByIdForUpdateAsync(
            request.ReturnRequestId,
            cancellationToken);

        if (returnRequest is null)
        {
            return false;
        }

        returnRequest.MarkReceived(request.AdminNotes);
        await _returnRequestRepository.UpdateAsync(returnRequest, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ReturnReceived",
            "ReturnRequest",
            returnRequest.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}

public record RefundReturnRequestCommand(Guid ReturnRequestId) : IRequest<bool>;

public sealed class RefundReturnRequestCommandHandler
    : IRequestHandler<RefundReturnRequestCommand, bool>
{
    private readonly IReturnRequestRepository _returnRequestRepository;
    private readonly IAuditLogger _auditLogger;

    public RefundReturnRequestCommandHandler(
        IReturnRequestRepository returnRequestRepository,
        IAuditLogger auditLogger)
    {
        _returnRequestRepository = returnRequestRepository;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(
        RefundReturnRequestCommand request,
        CancellationToken cancellationToken)
    {
        var returnRequest = await _returnRequestRepository.GetByIdForUpdateAsync(
            request.ReturnRequestId,
            cancellationToken);

        if (returnRequest is null)
        {
            return false;
        }

        returnRequest.MarkRefunded();
        await _returnRequestRepository.UpdateAsync(returnRequest, cancellationToken);

        await _auditLogger.WriteAsync(
            "Admin",
            null,
            "ReturnRefunded",
            "ReturnRequest",
            returnRequest.Id,
            cancellationToken: cancellationToken);

        return true;
    }
}
