using CatalogService.API.Categories.Contracts;
using CatalogService.API.Products.Contracts;
using CatalogService.Application.Categories;
using CatalogService.Application.Products;
using CatalogService.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace CatalogService.API.Configuration;

internal static class ServiceRegistration
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddMapper()
        {
            TypeAdapterConfig<Category, CategoryResponse>.NewConfig()
                .MapToConstructor(true)
                .Map(dest => dest.ImageUrl, src => src.ImageUrl != null ? src.ImageUrl.ToString() : null);
            TypeAdapterConfig<AddCategoryRequest, AddCategoryCommand>.NewConfig();

            TypeAdapterConfig<Product, ProductResponse>.NewConfig()
                .MapToConstructor(true)
                .Map(dest => dest.ImageUrl, src => src.ImageUrl != null ? src.ImageUrl.ToString() : null);

            TypeAdapterConfig<AddProductRequest, AddProductCommand>.NewConfig();

            services.AddSingleton<IMapper>(new Mapper(TypeAdapterConfig.GlobalSettings));

            return services;
        }
    }
}