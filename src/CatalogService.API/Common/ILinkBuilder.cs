using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Common;

public record Link(string Href, string Method);

public interface ILinkBuilder<T>
{
    IDictionary<string, Link> BuildLinks(T entity, IUrlHelper urlHelper);
}
