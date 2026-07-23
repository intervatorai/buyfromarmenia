using BFA.Admin.Application.Commands.Categories;
using BFA.Admin.Application.Queries.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Admin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _mediator.Send(
                new CreateCategoryCommand(
                    request.Name,
                    request.Slug,
                    request.Description,
                    request.SortOrder,
                    request.ParentCategoryId,
                    SkuPrefix: request.SkuPrefix),
                cancellationToken);

            return CreatedAtAction(nameof(GetCategories), new { id }, new { id });
        }
        catch (BFA.BuildingBlocks.Domain.DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _mediator.Send(
                new UpdateCategoryCommand(
                    id,
                    request.Name,
                    request.Slug,
                    request.Description,
                    request.SortOrder,
                    request.ParentCategoryId,
                    SkuPrefix: request.SkuPrefix),
                cancellationToken);

            return updated ? NoContent() : NotFound();
        }
        catch (BFA.BuildingBlocks.Domain.DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("seed-defaults")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> SeedDefaults(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SeedDefaultCategoriesCommand(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/hide")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> HideCategory(Guid id, CancellationToken cancellationToken)
    {
        var hidden = await _mediator.Send(new HideCategoryCommand(id), cancellationToken);
        return hidden ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "ModeratorOrAbove")]
    public async Task<IActionResult> ActivateCategory(Guid id, CancellationToken cancellationToken)
    {
        var activated = await _mediator.Send(new ActivateCategoryCommand(id), cancellationToken);
        return activated ? NoContent() : NotFound();
    }
}

public record CreateCategoryRequest(
    string Name,
    string Slug,
    string? Description = null,
    int SortOrder = 0,
    Guid? ParentCategoryId = null,
    string? SkuPrefix = null);

public record UpdateCategoryRequest(
    string Name,
    string Slug,
    string? Description = null,
    int SortOrder = 0,
    Guid? ParentCategoryId = null,
    string? SkuPrefix = null);
