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

        internal IServiceCollection AddAuthorizationPolicies(
            PermissionOptions permissionOptions,
            IEnumerable<(string Claim, string Name)> requiredScopes)
        {
            List<(string PolicyName, string Role)> policies =
            [
                (nameof(PermissionOptions.ReadRole), permissionOptions.ReadRole),
                (nameof(PermissionOptions.CreateRole), permissionOptions.CreateRole),
                (nameof(PermissionOptions.UpdateRole), permissionOptions.UpdateRole),
                (nameof(PermissionOptions.DeleteRole), permissionOptions.DeleteRole)
            ];

            var authBuilder = services.AddAuthorizationBuilder();
            foreach (var (policyName, role) in policies)
            {
                authBuilder.AddPolicy(policyName, p =>
                {
                    foreach (var (claim, value) in requiredScopes)
                        p.RequireClaim(claim, value);
                    p.RequireRole(role);
                });
            }

            authBuilder.AddDefaultPolicy("DefaultPolicy", p =>
            {
                p.RequireAuthenticatedUser();
                foreach (var (claim, value) in requiredScopes)
                    p.RequireClaim(claim, value);
            })
                .AddFallbackPolicy("FallbackPolicy", p =>
                {
                    p.RequireAuthenticatedUser();
                    foreach (var (claim, value) in requiredScopes)
                        p.RequireClaim(claim, value);
                });

            return services;
        }
    }
}