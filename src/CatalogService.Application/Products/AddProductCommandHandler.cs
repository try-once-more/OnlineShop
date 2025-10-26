using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Products;

public record AddProductCommand(
    [property: Required, MaxLength(50)] string Name,
    string? Description,
    Uri? ImageUrl,
    [property: Range(1, int.MaxValue)] int CategoryId,
    [property: Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal Price,
    [property: Range(0, int.MaxValue)] int Amount
) : IRequest<Product>;

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
