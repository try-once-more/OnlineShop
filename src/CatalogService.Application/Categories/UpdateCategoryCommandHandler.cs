using CatalogService.Application.Abstractions.Repository;
using CatalogService.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Categories;

public record UpdateCategoryCommand(
    [property: Required, Range(1, int.MaxValue)] int Id,
    [property: MaxLength(50)] string? Name,
    Uri? ImageUrl,
    [property: Range(1, int.MaxValue)] int? ParentCategoryId) : IRequest;

internal class UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateCategoryCommandHandler>? logger = default)
    : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IUnitOfWork unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        logger?.LogDebug("Updating category with ID: {CategoryId}", request.Id);

        var existing = await unitOfWork.Categories.GetAsync(request.Id, cancellationToken)
            ?? throw new CategoryNotFoundException(request.Id);

        if (!string.IsNullOrWhiteSpace(request.Name))
            existing.Name = request.Name;

        if (request.ImageUrl != null)
            existing.ImageUrl = request.ImageUrl;

        if (request.ParentCategoryId.HasValue)
        {
            existing.ParentCategory = await unitOfWork.Categories.GetAsync(request.ParentCategoryId.Value, cancellationToken)
                ?? throw new CategoryNotFoundException(request.ParentCategoryId.Value);
        }

        await unitOfWork.Categories.UpdateAsync(existing, cancellationToken);

        logger?.LogInformation("Successfully updated category with ID: {CategoryId}", request.Id);
    }
}
