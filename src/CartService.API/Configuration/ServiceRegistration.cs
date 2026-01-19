using CartService.API.CartEndpoints.Contracts;
using CartService.Application.Entities;
using CartService.Grpc.Contracts;
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

            ConfigureGrpcMappings();

            return services;
        }
    }

    private static void ConfigureGrpcMappings()
    {
        TypeAdapterConfig<ImageInfo, ImageInfoMessage>.NewConfig()
            .Map(dest => dest.Url, src => src.Url.ToString())
            .Map(dest => dest.AltText, src => src.AltText);

        TypeAdapterConfig<ImageInfoMessage, ImageInfo>.NewConfig()
            .MapWith(src => new ImageInfo(new Uri(src.Url), src.AltText));

        TypeAdapterConfig<decimal, DecimalValue>.NewConfig()
            .MapWith(src => src);
        TypeAdapterConfig<DecimalValue, decimal>.NewConfig()
            .MapWith(src => src);

        TypeAdapterConfig<CartItem, CartItemMessage>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.Status, src => (CartService.Grpc.Contracts.CartItemStatus)src.Status)
            .Map(dest => dest.Image, src => src.Image == null ? null : src.Image.Adapt<ImageInfoMessage>());

        TypeAdapterConfig<CartItemMessage, CartItem>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.Status, src => (Application.Entities.CartItemStatus)src.Status)
            .Map(dest => dest.Image, src => src.Image == null ? null : src.Image.Adapt<ImageInfo>());
    }
}
