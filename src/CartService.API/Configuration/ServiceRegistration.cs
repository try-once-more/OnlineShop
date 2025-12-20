using CartService.API.CartEndpoints.Contracts;
using CartService.Application.Entities;
using Mapster;

namespace CartService.API.Configuration;

internal static class ServiceRegistration
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddMapper()
        {
            TypeAdapterConfig<CartItem, CartItemResponse>.NewConfig();
            TypeAdapterConfig<ImageInfo, ImageInfoResponse>.NewConfig();

            TypeAdapterConfig<CreateCartItemRequest, CartItem>.NewConfig()
                .Map(dest => dest.Image, src => src.Image == null ? null : new ImageInfo(src.Image.Url, src.Image.AltText));

            TypeAdapterConfig<CreateImageInfoRequest, ImageInfo>.NewConfig()
                .MapWith(src => new ImageInfo(src.Url, src.AltText));

            return services;
        }
    }
}
