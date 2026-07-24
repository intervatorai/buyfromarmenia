using BFA.Admin.Application.Commands.Suppliers;
using BFA.Admin.Application.Queries.Suppliers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuppliers(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var suppliers = await _mediator.Send(new GetSuppliersQuery(status), cancellationToken);
        return Ok(suppliers);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSupplier(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _mediator.Send(new GetSupplierQuery(id), cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> CreateSupplier(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new CreateSupplierCommand(
                request.LegalName,
                request.TradingName,
                request.ContactPerson,
                request.Email,
                request.Phone,
                request.TaxNumber,
                request.RegistrationNumber,
                request.ActivateImmediately),
            cancellationToken);

        return CreatedAtAction(nameof(GetSupplier), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> UpdateSupplier(
        Guid id,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new UpdateSupplierCommand(
                id,
                request.LegalName,
                request.TradingName,
                request.ContactPerson,
                request.Email,
                request.Phone,
                request.TaxNumber,
                request.RegistrationNumber),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var approved = await _mediator.Send(new ApproveSupplierCommand(id), cancellationToken);
        return approved ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var rejected = await _mediator.Send(
            new RejectSupplierCommand(id, request.Reason),
            cancellationToken);

        return rejected ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/request-changes")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> RequestChanges(
        Guid id,
        [FromBody] RejectSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new RequestSupplierChangesCommand(id, request.Reason),
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> Suspend(
        Guid id,
        [FromBody] RejectSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var suspended = await _mediator.Send(
            new SuspendSupplierCommand(id, request.Reason),
            cancellationToken);
        return suspended ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "AdminOrAbove")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var activated = await _mediator.Send(new ActivateSupplierCommand(id), cancellationToken);
        return activated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/bank-accounts")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> AddBankAccount(
        Guid id,
        [FromBody] AddSupplierBankAccountRequest request,
        CancellationToken cancellationToken)
    {
        var added = await _mediator.Send(
            new AddSupplierBankAccountCommand(
                id,
                request.BankName,
                request.AccountHolder,
                request.Iban,
                request.Currency,
                request.Swift,
                request.IsPrimary),
            cancellationToken);

        return added ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/bank-accounts/{bankAccountId:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> UpdateBankAccount(
        Guid id,
        Guid bankAccountId,
        [FromBody] AddSupplierBankAccountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateSupplierBankAccountCommand(
                id,
                bankAccountId,
                request.BankName,
                request.AccountHolder,
                request.Iban,
                request.Currency,
                request.Swift,
                request.IsPrimary),
            cancellationToken);

        return result.Success
            ? NoContent()
            : result.Error == "Supplier not found."
                ? NotFound()
                : BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id:guid}/bank-accounts/{bankAccountId:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> RemoveBankAccount(
        Guid id,
        Guid bankAccountId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RemoveSupplierBankAccountCommand(id, bankAccountId),
            cancellationToken);

        return result.Success
            ? NoContent()
            : result.Error == "Supplier not found."
                ? NotFound()
                : BadRequest(new { message = result.Error });
    }

    [HttpPost("{id:guid}/documents")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> AddDocument(
        Guid id,
        [FromBody] SupplierDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AddSupplierDocumentCommand(
                id,
                request.DocumentType,
                request.FileName,
                request.FileUrl),
            cancellationToken);

        return result.Success
            ? NoContent()
            : result.Error == "Supplier not found."
                ? NotFound()
                : BadRequest(new { message = result.Error });
    }

    [HttpPut("{id:guid}/documents/{documentId:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> UpdateDocument(
        Guid id,
        Guid documentId,
        [FromBody] SupplierDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateSupplierDocumentCommand(
                id,
                documentId,
                request.DocumentType,
                request.FileName,
                request.FileUrl),
            cancellationToken);

        return result.Success
            ? NoContent()
            : result.Error == "Supplier not found."
                ? NotFound()
                : BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> RemoveDocument(
        Guid id,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RemoveSupplierDocumentCommand(id, documentId),
            cancellationToken);

        return result.Success
            ? NoContent()
            : result.Error == "Supplier not found."
                ? NotFound()
                : BadRequest(new { message = result.Error });
    }

    [HttpPost("{id:guid}/set-password")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> SetPassword(
        Guid id,
        [FromBody] SetSupplierPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SetSupplierPasswordCommand(id, request.NewPassword),
            cancellationToken);

        return result.Success
            ? Ok(new { message = "Password updated." })
            : BadRequest(new { message = result.Error });
    }
}

public record RejectSupplierRequest(string Reason);

public record CreateSupplierRequest(
    string LegalName,
    string TradingName,
    string ContactPerson,
    string Email,
    string Phone,
    string? TaxNumber = null,
    string? RegistrationNumber = null,
    bool ActivateImmediately = false);

public record UpdateSupplierRequest(
    string LegalName,
    string TradingName,
    string ContactPerson,
    string Email,
    string Phone,
    string? TaxNumber = null,
    string? RegistrationNumber = null);

public record AddSupplierBankAccountRequest(
    string BankName,
    string AccountHolder,
    string Iban,
    string Currency,
    string? Swift = null,
    bool IsPrimary = true);

public record SupplierDocumentRequest(
    string DocumentType,
    string FileName,
    string FileUrl);

public record SetSupplierPasswordRequest(string NewPassword);
