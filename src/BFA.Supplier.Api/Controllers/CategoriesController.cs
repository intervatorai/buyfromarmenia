using BFA.Supplier.Application.Commands.Categories;
using BFA.Supplier.Application.Queries.Categories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BFA.Supplier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var categoryId = await _mediator.Send(
            new CreateCategoryCommand(
                request.Name,
                request.Slug,
                request.Description,
                request.ParentCategoryId,
                request.SortOrder,
                request.LanguageCode),
            cancellationToken);

        return CreatedAtAction(nameof(GetCategories), new { id = categoryId }, new { id = categoryId });
    }
}

public record CreateCategoryRequest(
    string Name,
    string Slug,
    string? Description = null,
    Guid? ParentCategoryId = null,
    int SortOrder = 0,
    string LanguageCode = "en");
