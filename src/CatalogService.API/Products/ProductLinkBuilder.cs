using CatalogService.API.Common;
using CatalogService.API.Products.V1;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Products;

internal class ProductLinkBuilder : ILinkBuilder<ProductDto>
{
    public IDictionary<string, Link> BuildLinks(ProductDto entity, IUrlHelper urlHelper)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(urlHelper);

        return new Dictionary<string, Link>
        {
            ["self"] = new(urlHelper.Link(nameof(ProductsController.GetProductById), new { id = entity.Id }), HttpMethods.Get),
            ["update"] = new(urlHelper.Link(nameof(ProductsController.UpdateProduct), new { id = entity.Id }), HttpMethods.Patch),
            ["delete"] = new(urlHelper.Link(nameof(ProductsController.DeleteProduct), new { id = entity.Id }), HttpMethods.Delete)
        };
    }
}