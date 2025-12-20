using System.Text.Json;
using CatalogService.API.Products.Contracts;
using CatalogService.Application.Common;
using CatalogService.Application.Products;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CatalogService.API.Products;

internal class UpdateProductModelBinder : IModelBinder
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

            var request = new UpdateProductCommand
            {
                Id = id,
                Name = GetOptionalProperty<string>(elements, nameof(UpdateProductRequest.Name), e => e.GetString()),
                Description = GetOptionalProperty<string>(elements, nameof(UpdateProductRequest.Description), e => e.GetString()),
                ImageUrl = GetOptionalProperty<Uri?>(elements, nameof(UpdateProductRequest.ImageUrl), e =>
                {
                    var str = e.GetString();
                    if (!Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out var uri))
                        throw new JsonException($"Invalid URI: '{str}'.");
                    return uri;
                }),
                CategoryId = TryGetProperty(elements, nameof(UpdateProductRequest.CategoryId), out var categoryIdProp)
                    && categoryIdProp.Value.ValueKind == JsonValueKind.Number
                    ? categoryIdProp.Value.GetInt32()
                    : null,
                Price = TryGetProperty(elements, nameof(UpdateProductRequest.Price), out var priceProp)
                    && priceProp.Value.ValueKind == JsonValueKind.Number
                    ? priceProp.Value.GetDecimal()
                    : null,
                Amount = TryGetProperty(elements, nameof(UpdateProductRequest.Amount), out var amountProp)
                    && amountProp.Value.ValueKind == JsonValueKind.Number
                    ? amountProp.Value.GetInt32()
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
