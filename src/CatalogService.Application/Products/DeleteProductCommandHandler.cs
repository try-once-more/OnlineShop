using CatalogService.Application.Abstractions.Repository;
using CatalogService.Events.Products;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

internal class DeleteProductCommandHandler(IUnitOfWork unitOfWork, IOptions<CatalogPublisherOptions> options, ILogger<DeleteProductCommandHandler>? logger = default)
    : IRequestHandler<DeleteProductCommand>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Deleting product with ID: {ProductId}", request.Id);

        if (options.Value.IsEnabled)
        {
            await DeleteProductAndQueueEvent(request, cancellationToken);
        }
        else
        {
            await unitOfWork.Products.DeleteAsync(request.Id, cancellationToken);
        }

        logger?.LogInformation("Successfully deleted product with ID: {ProductId}", request.Id);
    }

    private async Task DeleteProductAndQueueEvent(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var @event = new ProductDeletedEvent
        {
            Id = request.Id
        }.ToEventEntity();

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        await unitOfWork.Products.DeleteAsync(request.Id, cancellationToken);
        await unitOfWork.Events.AddAsync(@event, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger?.LogInformation("Successfully added event with ID: {EventId}, EventType: {EventType}",
            @event.Id, @event.EventType);
    }
}
