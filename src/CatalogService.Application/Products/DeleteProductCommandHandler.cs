using CatalogService.Application.Abstractions.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Products;

/// <summary>
/// Represents a request to delete an existing product by its ID.
/// </summary>
public record DeleteProductCommand : IRequest
{
    /// <summary>
    /// ID of the product to delete.
    /// </summary>
    [Required(ErrorMessage = "ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ID must be positive.")]
    public required int Id { get; init; }
}

internal class DeleteProductCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteProductCommandHandler>? logger = default)
    : IRequestHandler<DeleteProductCommand>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Deleting product with ID: {ProductId}", request.Id);

        await unitOfWork.Products.DeleteAsync(request.Id, cancellationToken);

        logger?.LogInformation("Successfully deleted product with ID: {ProductId}", request.Id);
    }
}
