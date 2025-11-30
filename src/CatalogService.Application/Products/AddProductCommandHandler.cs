using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Entities;
using CatalogService.Events.Products;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Products;

/// <summary>
/// Represents a request to create a new product.
/// </summary>
public record AddProductCommand : IRequest<Product>
{
    /// <summary>
    /// Product name.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [Length(1, 50, ErrorMessage = "Name cannot exceed 50 characters.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
    public required string Name { get; init; }

    /// <summary>
    /// Optional product description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional URL for the product image.
    /// </summary>
    public Uri? ImageUrl { get; init; }

    /// <summary>
    /// ID of the category this product belongs to.
    /// </summary>
    [Required(ErrorMessage = "Category ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be positive.")]
    public required int CategoryId { get; init; }

    /// <summary>
    /// Product price.
    /// </summary>
    [Required(ErrorMessage = "Price is required.")]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Price must be positive.")]
    public required decimal Price { get; init; }

    /// <summary>
    /// Product amount.
    /// </summary>
    [Required(ErrorMessage = "Amount is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be positive.")]
    public required int Amount { get; init; }
}

internal class AddProductCommandHandler(IUnitOfWork unitOfWork, IOptions<CatalogPublisherOptions> options, ILogger<AddProductCommandHandler>? logger = default)
    : IRequestHandler<AddProductCommand, Product>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task<Product> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Adding product with name: {ProductName}, CategoryId: {CategoryId}, Price: {Price}",
            request.Name, request.CategoryId, request.Price);

        var category = await unitOfWork.Categories.GetAsync(request.CategoryId, cancellationToken)
            ?? throw new CategoryNotFoundException(request.CategoryId);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            Category = category,
            Price = request.Price,
            Amount = request.Amount
        };

        if (options.Value.IsEnabled)
        {
            await AddProductAndQueueEvent(product, cancellationToken);
        }
        else
        {
            await unitOfWork.Products.AddAsync(product, cancellationToken);
        }

        logger?.LogInformation("Successfully added product with ID: {ProductId}, Name: {ProductName}",
            product.Id, product.Name);

        return product;
    }

    private async Task AddProductAndQueueEvent(Product product, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        await unitOfWork.Products.AddAsync(product, cancellationToken);

        var entity = new ProductCreatedEvent
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Amount = product.Amount,
            CategoryId = product.Category.Id,
            Description = product.Description,
            ImageUrl = product.ImageUrl
        }.ToEventEntity();

        await unitOfWork.Events.AddAsync(entity, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger?.LogInformation("Successfully added event with ID: {EventId}, EventType: {EventType}",
            entity.Id, entity.EventType);
    }
}
