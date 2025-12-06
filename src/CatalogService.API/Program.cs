using Asp.Versioning;
using CatalogService.API.Categories;
using CatalogService.API.Categories.Contracts;
using CatalogService.API.Common;
using CatalogService.API.Configuration;
using CatalogService.API.Middleware;
using CatalogService.API.Products;
using CatalogService.API.Products.Contracts;
using CatalogService.Application;
using CatalogService.Infrastructure;
using Eventing.Infrastructure;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseKestrelHttpsConfiguration();

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

builder.Services.AddValidation();
builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var corsOptions = builder.Configuration.GetSection("Cors").Get<CorsOptions>() ?? new();
        _ = corsOptions.AllowedOrigins.Contains("*")
           ? policy.AllowAnyOrigin()
           : policy.WithOrigins(corsOptions.AllowedOrigins);

        _ = corsOptions.AllowedMethods.Contains("*")
            ? policy.AllowAnyMethod()
            : policy.WithMethods(corsOptions.AllowedMethods);

        _ = corsOptions.AllowedHeaders.Contains("*")
            ? policy.AllowAnyHeader()
            : policy.WithHeaders(corsOptions.AllowedHeaders);
    });
});

builder.Services.AddSingleton<ErrorHandlingMiddleware>();

builder.Services.Configure<CatalogDatabaseOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<EventingOptions>(builder.Configuration.GetSection("Eventing"));
builder.Services.Configure<CatalogPublisherOptions>(builder.Configuration.GetSection("Eventing:CatalogService"));

builder.Services.AddCatalogServiceInfrastructure();
builder.Services.AddCatalogServiceApplication();
builder.Services.AddEventing();
builder.Services.AddHostedService<EventProcessor>();

builder.Services.AddMapper();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new() { Title = "Catalog Service API", Version = "v1" });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });
}

builder.Services.AddSingleton<ILinkBuilder<CategoryResponse>, CategoryLinkBuilder>();
builder.Services.AddSingleton<ILinkBuilder<ProductResponse>, ProductLinkBuilder>();
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
        options.SwaggerEndpoint("/openapi/v1.json", "Catalog Service API v1");
    });
}

app.Run();