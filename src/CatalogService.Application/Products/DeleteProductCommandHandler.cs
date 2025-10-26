using CatalogService.Application.Abstractions.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Products;

public record DeleteProductCommand([property: Required, Range(1, int.MaxValue)] int Id) : IRequest;

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
