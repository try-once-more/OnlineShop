using CatalogService.Application.Categories;
using CatalogService.Application.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace CatalogService.API.Categories.V1;

internal class UpdateCategoryModelBinder : IModelBinder
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

            var command = new UpdateCategoryCommand
            {
                Id = id,
                Name = GetOptionalProperty<Optional<string>>(root, nameof(UpdateCategoryCommand.Name), JsonValueKind.String, e => e.GetString()),
                ImageUrl = GetOptionalProperty<Optional<Uri>>(root, nameof(UpdateCategoryCommand.ImageUrl), JsonValueKind.String, e =>
                {
                    var str = e.GetString();
                    if (!Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out var uri))
                        throw new JsonException($"Invalid URI: '{str}'.");
                    return uri;
                }),

                ParentCategoryId = GetOptionalProperty<int?>(root, nameof(UpdateCategoryCommand.ParentCategoryId), JsonValueKind.Number, e => e.GetInt32())
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
        JsonValueKind expectedKind,
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

        return default;
    }
}