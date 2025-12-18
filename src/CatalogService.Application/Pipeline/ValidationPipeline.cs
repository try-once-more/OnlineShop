using System.ComponentModel.DataAnnotations;
using MediatR;

namespace CatalogService.Application.Pipeline;

internal class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(request, context, results, validateAllProperties: true))
        {
            var messages = results.Select(r => r.ErrorMessage).Where(m => m != null);
            throw new ValidationException(string.Join("; ", messages));
        }

        return await next();
    }
}
