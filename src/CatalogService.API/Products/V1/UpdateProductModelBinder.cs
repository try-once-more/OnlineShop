using CatalogService.Application.Common;
using CatalogService.Application.Products;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace CatalogService.API.Categories.V1;

internal class UpdateProductModelBinder : IModelBinder
{
    private static readonly JsonDocumentOptions JsonOptions = new() { AllowTrailingCommas = true };

    public async Task BindModelAsync(ModelBindingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var idValue = context.ValueProvider.GetValue("id");
        if (idValue == ValueProviderResult.None || !int.TryParse(idValue.FirstValue, out var id) || id <= 0)
        {
            context.ModelState.AddModelError(nameof(id), "Invalid id.");
            context.Result = ModelBindingResult.Failed();
            return;
        }

        try
        {
            using var doc = await JsonDocument.ParseAsync(
                context.HttpContext.Request.Body,
                JsonOptions,
                context.HttpContext.RequestAborted);

            var root = doc.RootElement;

            var command = new UpdateProductCommand
            {
                Id = id,
                Name = GetOptionalProperty<Optional<string>>(root, nameof(UpdateProductCommand.Name), e => e.GetString()),
                Description = GetOptionalProperty<Optional<string?>>(root, nameof(UpdateProductCommand.Description), e => e.GetString()),
                ImageUrl = GetOptionalProperty<Optional<Uri?>>(root, nameof(UpdateProductCommand.ImageUrl), e =>
                {
                    var str = e.GetString();
                    if (!Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out var uri))
                        throw new JsonException($"Invalid URI: '{str}'.");
                    return uri;
                }),
                CategoryId = GetOptionalProperty<int?>(root, nameof(UpdateProductCommand.CategoryId), e => e.GetInt32()),
                Price = GetOptionalProperty<decimal?>(root, nameof(UpdateProductCommand.Price), e => e.GetDecimal()),
                Amount = GetOptionalProperty<int?>(root, nameof(UpdateProductCommand.Amount), e => e.GetInt32())
            };

            context.Result = ModelBindingResult.Success(command);
        }
        catch (JsonException ex)
        {
            context.ModelState.AddModelError(string.Empty, $"Invalid JSON: {ex.Message}");
            context.Result = ModelBindingResult.Failed();
        }
    }

    private static T GetOptionalProperty<T>(
        JsonElement element,
        string name,
        Func<JsonElement, T> converter)
    {
        foreach (var prop in element.EnumerateObject())
        {
            if (!string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                return converter(prop.Value);
            }
            catch (Exception ex)
            {
                throw new JsonException(
                    $"Failed to convert property '{name}' to {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        return default!;
    }
}