using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Products;

public record UpdateProductCommand(
    [property: Required, Range(1, int.MaxValue)] int Id,
    [property: MaxLength(50)] string? Name,
    string? Description,
    Uri? ImageUrl,
    [property: Range(1, int.MaxValue)] int? CategoryId,
    [property: Range(typeof(decimal), "0.01", "79228162514264337593543950335")] decimal? Price,
    [property: Range(0, int.MaxValue)] int? Amount
) : IRequest;

internal class UpdateProductCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateProductCommandHandler>? logger = default)
    : IRequestHandler<UpdateProductCommand>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Updating product with ID: {ProductId}", request.Id);

        var existing = await unitOfWork.Products.GetAsync(request.Id, cancellationToken)
            ?? throw new ProductNotFoundException(request.Id);

        if (!string.IsNullOrWhiteSpace(request.Name))
            existing.Name = request.Name;

        if (request.Description is not null)
            existing.Description = request.Description;

        if (request.ImageUrl is not null)
            existing.ImageUrl = request.ImageUrl;

        if (request.Price.HasValue)
            existing.Price = request.Price.Value;

        if (request.Amount.HasValue)
            existing.Amount = request.Amount.Value;

        if (request.CategoryId.HasValue)
        {
            existing.Category = await unitOfWork.Categories.GetAsync(request.CategoryId.Value, cancellationToken)
                ?? throw new CategoryNotFoundException(request.CategoryId.Value);
        }

        await unitOfWork.Products.UpdateAsync(existing, cancellationToken);

        logger?.LogInformation("Successfully updated product with ID: {ProductId}", request.Id);
    }
}
