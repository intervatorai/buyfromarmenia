using BFA.Supplier.Application.Commands.Suppliers;
using BFA.Supplier.Application.Queries.Suppliers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Supplier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterSupplierRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(
                new RegisterSupplierCommand(
                    request.LegalName,
                    request.TradingName,
                    request.ContactPerson,
                    request.Email,
                    request.Phone,
                    request.Password,
                    request.TaxNumber,
                    request.RegistrationNumber,
                    request.LegalCountryCode,
                    request.LegalCity,
                    request.LegalLine1,
                    request.LegalLine2,
                    request.LegalPostalCode,
                    request.LegalRegion),
                cancellationToken);

            return CreatedAtAction(
                nameof(GetSupplier),
                new { id = result.SupplierId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSupplier(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _mediator.Send(new GetSupplierQuery(id), cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProfile(
        Guid id,
        [FromBody] UpdateSupplierProfileRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(
            new UpdateSupplierProfileCommand(
                id,
                request.LegalName,
                request.TradingName,
                request.ContactPerson,
                request.Email,
                request.Phone,
                request.TaxNumber,
                request.RegistrationNumber,
                request.LegalCountryCode,
                request.LegalCity,
                request.LegalLine1,
                request.LegalLine2,
                request.LegalPostalCode,
                request.LegalRegion,
                request.WarehouseCountryCode,
                request.WarehouseCity,
                request.WarehouseLine1,
                request.WarehouseLine2,
                request.WarehousePostalCode,
                request.WarehouseRegion,
                request.PreparationDays),
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> SubmitApplication(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var submitted = await _mediator.Send(
                new SubmitSupplierApplicationCommand(id),
                cancellationToken);

            return submitted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/bank-accounts")]
    public async Task<IActionResult> AddBankAccount(
        Guid id,
        [FromBody] AddBankAccountRequest request,
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
    public async Task<IActionResult> UpdateBankAccount(
        Guid id,
        Guid bankAccountId,
        [FromBody] AddBankAccountRequest request,
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
    public async Task<IActionResult> AddDocument(
        Guid id,
        [FromBody] AddDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var added = await _mediator.Send(
                new AddSupplierDocumentCommand(id, request.DocumentType, request.FileName, request.FileUrl),
                cancellationToken);

            return added ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}/documents/{documentId:guid}")]
    public async Task<IActionResult> UpdateDocument(
        Guid id,
        Guid documentId,
        [FromBody] AddDocumentRequest request,
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
}

public record RegisterSupplierRequest(
    string LegalName,
    string TradingName,
    string ContactPerson,
    string Email,
    string Phone,
    string Password,
    string? TaxNumber,
    string? RegistrationNumber,
    string? LegalCountryCode,
    string? LegalCity,
    string? LegalLine1,
    string? LegalLine2,
    string? LegalPostalCode,
    string? LegalRegion);

public record UpdateSupplierProfileRequest(
    string LegalName,
    string TradingName,
    string ContactPerson,
    string Email,
    string Phone,
    string? TaxNumber,
    string? RegistrationNumber,
    string? LegalCountryCode,
    string? LegalCity,
    string? LegalLine1,
    string? LegalLine2,
    string? LegalPostalCode,
    string? LegalRegion,
    string? WarehouseCountryCode,
    string? WarehouseCity,
    string? WarehouseLine1,
    string? WarehouseLine2,
    string? WarehousePostalCode,
    string? WarehouseRegion,
    int PreparationDays = 2);

public record AddBankAccountRequest(
    string BankName,
    string AccountHolder,
    string Iban,
    string Currency,
    string? Swift,
    bool IsPrimary = true);

public record AddDocumentRequest(string DocumentType, string FileName, string FileUrl);
