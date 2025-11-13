using Asp.Versioning;
using CatalogService.API.Categories;
using CatalogService.API.Categories.V1;
using CatalogService.API.Common;
using CatalogService.API.Middleware;
using CatalogService.API.Products;
using CatalogService.API.Products.V1;
using CatalogService.API.Versions;
using CatalogService.Application.Categories;
using CatalogService.Application.Products;
using CatalogService.Domain.Entities;
using Mapster;
using MapsterMapper;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = false;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddSingleton<ErrorHandlingMiddleware>();

builder.Services.Configure<CatalogDatabaseSettings>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.AddCatalogServiceInfrastructure();
builder.Services.AddCatalogServiceApplication();

AddMapper(builder.Services);
AddOpenApi(builder.Services);

builder.Services.AddSingleton<ILinkBuilder<CategoryDto>, CategoryLinkBuilder>();
builder.Services.AddSingleton<ILinkBuilder<ProductDto>, ProductLinkBuilder>();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });
    app.UseSwaggerUI(options =>
    {
        foreach (var version in ApiVersions.All)
        {
            options.SwaggerEndpoint($"/openapi/v{version}.json", $"Catalog Service API v{version}");
        }
    });
}

app.Run();


static void AddMapper(IServiceCollection services)
{
    var config = TypeAdapterConfig.GlobalSettings;
    config.NewConfig<Category, CategoryDto>();
    config.NewConfig<Product, ProductDto>();

    services.AddSingleton<IMapper>(new Mapper(config));
}

static void AddOpenApi(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        foreach (var version in ApiVersions.All)
        {
            options.SwaggerDoc($"v{version}", new() { Title = "Catalog Service API", Version = $"v{version}" });
        }

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);

        options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(UpdateCategoryCommand), () => new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>(StringComparer.OrdinalIgnoreCase)
            {
                ["name"] = new OpenApiSchema { Type = "string", Nullable = true, MinLength = 1, MaxLength = 50 },
                ["imageUrl"] = new OpenApiSchema { Type = "string", Format = "uri", Nullable = true, Example = new OpenApiString("https://example.com/image.jpg") },
                ["parentCategoryId"] = new OpenApiSchema { Type = "integer", Format = "int32", Nullable = true, Minimum = 1 }
            },
            Required = new HashSet<string>()
        });

        options.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(UpdateProductCommand), () => new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>(StringComparer.OrdinalIgnoreCase)
            {
                ["name"] = new OpenApiSchema { Type = "string", Nullable = true, MinLength = 1, MaxLength = 50 },
                ["description"] = new OpenApiSchema { Type = "string", Nullable = true },
                ["imageUrl"] = new OpenApiSchema { Type = "string", Format = "uri", Nullable = true, Example = new OpenApiString("https://example.com/image.jpg") },
                ["categoryId"] = new OpenApiSchema { Type = "integer", Format = "int32", Nullable = true, Minimum = 1 },
                ["price"] = new OpenApiSchema { Type = "number", Format = "decimal", Nullable = true, Minimum = 0.01m },
                ["amount"] = new OpenApiSchema { Type = "integer", Format = "int32", Nullable = true, Minimum = 1 }
            },
            Required = new HashSet<string>()
        });

        options.EnableAnnotations();
    });
}

public partial class Program { }