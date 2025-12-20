using System.Text.Json;
using CatalogService.API.Categories.Contracts;
using CatalogService.Application.Categories;
using CatalogService.Application.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CatalogService.API.Categories;

internal class UpdateCategoryModelBinder : IModelBinder
{
    private static readonly JsonDocumentOptions JsonOptions = new() { AllowTrailingCommas = true };

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var idValue = bindingContext.ValueProvider.GetValue("id");
        if (idValue == ValueProviderResult.None || !int.TryParse(idValue.FirstValue, out var id) || id <= 0)
        {
            bindingContext.ModelState.AddModelError(nameof(id), "Invalid id.");
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        try
        {
            using var doc = await JsonDocument.ParseAsync(
                bindingContext.HttpContext.Request.Body,
                JsonOptions,
                bindingContext.HttpContext.RequestAborted);

            var root = doc.RootElement;
            var elements = root.EnumerateObject();



            var request = new UpdateCategoryCommand
            {
                Id = id,
                Name = GetOptionalProperty<string>(elements, nameof(UpdateCategoryRequest.Name), e => e.GetString()),
                ImageUrl = GetOptionalProperty<Uri?>(elements, nameof(UpdateCategoryRequest.ImageUrl), e =>
                {
                    var str = e.GetString();
                    if (!Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out var uri))
                        throw new JsonException($"Invalid URI: '{str}'.");
                    return uri;
                }),
                ParentCategoryId = TryGetProperty(elements, nameof(UpdateCategoryRequest.ParentCategoryId), out var prop)
                    && prop.Value.ValueKind == JsonValueKind.Number
                    ? prop.Value.GetInt32()
                    : null
            };

            bindingContext.Result = ModelBindingResult.Success(request);
        }
        catch (JsonException ex)
        {
            bindingContext.ModelState.AddModelError(string.Empty, $"Invalid JSON: {ex.Message}");
            bindingContext.Result = ModelBindingResult.Failed();
        }
    }

    private static Optional<T> GetOptionalProperty<T>(
        IEnumerable<JsonProperty> elements,
        string name,
        Func<JsonElement, T> convertFn)
    {
        if (!TryGetProperty(elements, name, out var property))
            return new Optional<T>();

        if (property.Value.ValueKind == JsonValueKind.Null)
            return new Optional<T>(default);

        try
        {
            return new Optional<T>(convertFn(property.Value));
        }
        catch (Exception ex)
        {
            throw new JsonException(
                $"Failed to convert property '{name}' to {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    private static bool TryGetProperty(
        IEnumerable<JsonProperty> elements,
        string name,
        out JsonProperty property)
    {
        property = elements.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        return !property.Equals(default(JsonProperty));
    }
}
