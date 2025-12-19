using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using CatalogService.API.Common;
using CatalogService.API.Configuration;
using CatalogService.API.Products.Contracts;
using CatalogService.Application.Products;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;

namespace CatalogService.API.Products;

/// <summary>
/// Products management controller
/// </summary>
[ApiController]
[ApiVersion(1)]
[Authorize(Policy = nameof(Permissions.Read))]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class ProductsController(IMediator mediator, IMapper mapper, ILinkBuilder<ProductResponse> linkBuilder, ILogger<ProductsController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves a list of products with optional filtering by category and pagination.
    /// </summary>
    /// <param name="categoryId">Optional category filter. Only products belonging to the specified category will be returned.</param>
    /// <param name="pageNumber">Page number to retrieve.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of products for the specified parameters.</returns>
    /// <response code="200">Successfully retrieved products.</response>
    [HttpGet(Name = nameof(GetProducts))]
    [ProducesResponseType<IReadOnlyCollection<ProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> GetProducts(
        [FromQuery, Range(1, int.MaxValue)] int? categoryId,
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 1000)] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving products page {PageNumber} with page size {PageSize}", pageNumber, pageSize);
        var request = categoryId.HasValue
            ? new GetProductsByCategoryIdQuery { CategoryId = categoryId.Value, PageNumber = pageNumber, PageSize = pageSize }
            : new GetProductsQuery() { PageNumber = pageNumber, PageSize = pageSize };

        var products = await mediator.Send(request, cancellationToken);
        var result = mapper.Map<ProductResponse[]>(products);
        logger.LogInformation("Retrieved {Count} products", result.Length);

        return Ok(result);
    }

    /// <summary>
    /// Get a specific product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product details</returns>
    /// <response code="200">Returns the product.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Product not found.</response>
    [HttpGet("{id:int}", Name = nameof(GetProductById))]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProductById(
        [FromRoute, Required, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving product with ID {ProductId}", id);
        var query = new GetProductByIdQuery { Id = id };
        var product = await mediator.Send(query, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product with ID {ProductId} not found", id);
            return NotFound();
        }

        var result = product.Adapt<ProductResponse>();
        result.Links = linkBuilder.BuildLinks(result, Url);

        logger.LogInformation("Product with ID {ProductId} retrieved successfully", id);
        return Ok(result);
    }

    /// <summary>
    /// Create a new product.
    /// </summary>
    /// <param name="request">The product creation data.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product.</returns>
    /// <response code="201">Product created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost(Name = nameof(CreateProduct))]
    [Authorize(Policy = nameof(Permissions.Create))]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        [FromBody, Required] AddProductRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        logger.LogInformation(
            "Creating a new product: Name={ProductName}, ImageUrl={ImageUrl}, CategoryId={CategoryId}, Price={ProductPrice}, Amount={ProductAmount}",
            request.Name,
            request.ImageUrl,
            request.CategoryId,
            request.Price,
            request.Amount
        );
        var command = request.Adapt<AddProductCommand>();
        var product = await mediator.Send(command, cancellationToken);

        var result = product.Adapt<ProductResponse>();
        result.Links = linkBuilder.BuildLinks(result, Url);
        logger.LogInformation("Product created with ID {ProductId}", product.Id);

        return CreatedAtRoute(nameof(GetProductById), new { id = product.Id }, result);
    }

    /// <summary>
    /// Update an existing product.
    /// </summary>
    /// <param name="id">The ID of the product to update.</param>
    /// <param name="request">The product update data.</param>
    /// <param name="command"></param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content</returns>
    /// <response code="204">Product updated successfull.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Product not found.</response>
    [HttpPatch("{id:int}", Name = nameof(UpdateProduct))]
    [Authorize(Policy = nameof(Permissions.Update))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(

        [FromRoute, Required, Range(1, int.MaxValue)] int id, // Not used, only for Swagger documentation
        [BindNever, FromBody, Required] UpdateProductRequest request, // Not used, only for Swagger documentation
        [SwaggerIgnore, ModelBinder(BinderType = typeof(UpdateProductModelBinder))] UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating product with ID {ProductId}", id);
        await mediator.Send(command, cancellationToken);
        logger.LogInformation("Product with ID {ProductId} updated successfully", id);

        return NoContent();
    }

    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Product deleted successfully.</response>
    /// <response code="400">Cannot delete product.</response>
    [HttpDelete("{id:int}", Name = nameof(DeleteProduct))]
    [Authorize(Policy = nameof(Permissions.Delete))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProduct(
        [FromRoute, Required, Range(1, int.MaxValue)] int id,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting product with ID {ProductId}", id);
        var command = new DeleteProductCommand { Id = id };
        await mediator.Send(command, cancellationToken);
        logger.LogInformation("Product with ID {ProductId} deleted successfully", id);

        return NoContent();
    }
}
