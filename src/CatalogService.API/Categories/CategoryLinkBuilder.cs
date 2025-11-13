using CatalogService.API.Categories.V1;
using CatalogService.API.Common;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Categories;

internal class CategoryLinkBuilder : ILinkBuilder<CategoryDto>
{
    public IDictionary<string, Link> BuildLinks(CategoryDto entity, IUrlHelper urlHelper)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(urlHelper);

        return new Dictionary<string, Link>
        {
            ["self"] = new(urlHelper.Link(nameof(CategoriesController.GetCategoryById), new { id = entity.Id }), HttpMethods.Get),
            ["update"] = new(urlHelper.Link(nameof(CategoriesController.UpdateCategory), new { id = entity.Id }), HttpMethods.Patch),
            ["delete"] = new(urlHelper.Link(nameof(CategoriesController.DeleteCategory), new { id = entity.Id }), HttpMethods.Delete)
        };
    }
}