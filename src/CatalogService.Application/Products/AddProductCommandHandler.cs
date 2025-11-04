using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
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
    [Required, MaxLength(50)]
    public required string Name { get; init; }

    /// <summary>
    /// Optional product description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional URL for the product image.
    /// </summary>
    [Url]
    public Uri? ImageUrl { get; init; }

    /// <summary>
    /// ID of the category this product belongs to.
    /// </summary>
    [Required, Range(1, int.MaxValue)]
    public required int CategoryId { get; init; }

    /// <summary>
    /// Product price.
    /// </summary>
    [Required, Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public required decimal Price { get; init; }

    /// <summary>
    /// Product amount.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Amount { get; init; }
}

internal class AddProductCommandHandler(IUnitOfWork unitOfWork, ILogger<AddProductCommandHandler>? logger = default)
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

        await unitOfWork.Products.AddAsync(product, cancellationToken);

        logger?.LogInformation("Successfully added product with ID: {ProductId}, Name: {ProductName}",
            product.Id, product.Name);

        return product;
    }
}
