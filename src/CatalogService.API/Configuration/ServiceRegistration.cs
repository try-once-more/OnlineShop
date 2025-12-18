using System.Reflection;
using CatalogService.API.Categories.Contracts;
using CatalogService.API.Products.Contracts;
using CatalogService.Application.Categories;
using CatalogService.Application.Products;
using CatalogService.Domain.Entities;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;

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

        internal IServiceCollection AddSwagger(WebApplicationBuilder builder)
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
                    In = ParameterLocation.Header
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
                });

                if (builder.Environment.IsDevelopment())
                {
                    var swaggerOptions = builder.Configuration.GetSection("Swagger").Get<SwaggerOptions>() ?? new();
                    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri(swaggerOptions.AuthorizationUrl),
                                TokenUrl = new Uri(swaggerOptions.TokenUrl),
                                Scopes = swaggerOptions.Scopes.ToDictionary(i => i, i => string.Empty)
                            }
                        }
                    });
                    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("oauth2", document)] = [.. swaggerOptions.Scopes]
                    });
                }
            });

            return services;
        }
    }
}
