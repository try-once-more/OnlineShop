using CatalogService.API.Categories.Contracts;
using CatalogService.API.Products.Contracts;
using CatalogService.Application.Categories;
using CatalogService.Application.Products;
using CatalogService.Domain.Entities;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using System.Reflection;

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

        internal IServiceCollection AddSwagger(SwaggerOptions swaggerOptions, RequiredScopeOptions[] requiredScopes)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "Catalog Service API", Version = "v1" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter an existing JWT token"
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
                });

                if (swaggerOptions != null)
                {
                    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri(swaggerOptions.AuthorizationUrl),
                                TokenUrl = new Uri(swaggerOptions.TokenUrl),
                                Scopes = requiredScopes.ToDictionary(i => i.FullName, i => i.Description)
                            }
                        }
                    });
                    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("oauth2", document)] = [.. requiredScopes.Select(s => s.FullName),]
                    });
                }
            });

            return services;
        }
    }
}