using Asp.Versioning;
using CatalogService.API.Common;
using CatalogService.API.Versions;
using CatalogService.Application.Categories;
using CatalogService.Domain.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.API.Categories.V1;

/// <summary>
/// Categories management controller
/// </summary>
[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/categories")]
[Produces("application/json")]
public class CategoriesController(IMediator mediator, IMapper mapper, ILinkBuilder<CategoryDto> linkBuilder, ILogger<CategoriesController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves a list of categories with optional pagination.
    /// </summary>
    /// <param name="pageNumber">Page number to retrieve.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of categories for the specified parameters.</returns>
    /// <response code="200">Successfully retrieved categories.</response>
    [HttpGet(Name = nameof(GetCategories))]
    [ProducesResponseType<IReadOnlyCollection<Category>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<Category>>> GetCategories(
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 1000)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving categories page {PageNumber} with page size {PageSize}", pageNumber, pageSize);
        var request = new GetCategoriesQuery() { PageNumber = pageNumber, PageSize = pageSize };
        var categories = await mediator.Send(request, cancellationToken);

        var result = mapper.Map<CategoryDto[]>(categories);
        logger.LogInformation("Retrieved {Count} categories", result.Length);

        return Ok(result);
    }

    /// <summary>
    /// Gets a specific category by its ID.
    /// </summary>
    /// <param name="id">The ID of the category to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category details</returns>
    /// <response code="200">Returns the category.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Category not found.</response>
    [HttpGet("{id:int}", Name = nameof(GetCategoryById))]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(
        [FromRoute, Required, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving category with ID {CategoryId}", id);
        var query = new GetCategoryByIdQuery { Id = id };
        var category = await mediator.Send(query, cancellationToken);

        if (category is null)
        {
            logger.LogWarning("Category with ID {CategoryId} not found", id);
            return NotFound();
        }

        var result = category.Adapt<CategoryDto>();
        result.Links = linkBuilder.BuildLinks(result, Url);
        logger.LogInformation("Category with ID {CategoryId} retrieved successfully", id);

        return Ok(result);
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="request">The category creation data.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created category.</returns>
    /// <response code="201">Category created successfully.</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost(Name = nameof(CreateCategory))]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        [FromBody, Required] AddCategoryCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating a new category: Name={CategoryName}, ImageUrl={ImageUrl}, ParentCategoryId={ParentCategoryId}",
            request.Name,
            request.ImageUrl,
            request.ParentCategoryId
        );
        var category = await mediator.Send(request, cancellationToken);

        var result = category.Adapt<CategoryDto>();
        result.Links = linkBuilder.BuildLinks(result, Url);
        logger.LogInformation("Category created with ID {CategoryId}", category.Id);

        return CreatedAtRoute(nameof(GetCategoryById), new { id = category.Id }, result);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The ID of the category to update.</param>
    /// <param name="request">The category update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content</returns>
    /// <response code="204">Category updated successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Category not found.</response>
    [HttpPatch("{id:int}", Name = nameof(UpdateCategory))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        [FromRoute, Required, Range(1, int.MaxValue)] int id, // Not used, only for Swagger documentation
        [FromBody, Required, ModelBinder(BinderType = typeof(UpdateCategoryModelBinder))] UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating category with ID {CategoryId}", id);
        await mediator.Send(request, cancellationToken);
        logger.LogInformation("Category with ID {CategoryId} updated successfully", id);

        return NoContent();
    }

    /// <summary>
    /// Deletes a category by its ID.
    /// </summary>
    /// <param name="id">The ID of the category to delete.</param>
    /// <param name="force">If true, deletes the category along with all its child categories and associated products.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content.</returns>
    /// <response code="204">Category deleted successfully.</response>
    /// <response code="400">Cannot delete category.</response>
    [HttpDelete("{id:int}", Name = nameof(DeleteCategory))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCategory(
        [FromRoute, Required, Range(1, int.MaxValue)] int id,
        [FromQuery] bool force,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting category with ID {CategoryId}, force={Force}", id, force);
        var command = new DeleteCategoryCommand { Id = id, ForceDelete = force };
        await mediator.Send(command, cancellationToken);
        logger.LogInformation("Category with ID {CategoryId} deleted successfully", id);

        return NoContent();
    }
}