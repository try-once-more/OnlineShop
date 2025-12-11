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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

var authenticationOptions = builder.Configuration
    .GetSection("Authentication")
    .Get<AuthenticationOptions>()
    ?? throw new InvalidOperationException("Authentication configuration is missing");

var permissionOptions = builder.Configuration
    .GetSection("Permissions")
    .Get<PermissionOptions>()
    ?? throw new InvalidOperationException("Permissions configuration is missing");

var swaggerOptions = builder.Configuration
        .GetSection("Swagger")
        .Get<SwaggerOptions>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authenticationOptions.Authority;
        options.Audience = authenticationOptions.Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuers = authenticationOptions.ValidIssuers,
            ValidAudiences = authenticationOptions.ValidAudiences,
            ClockSkew = TimeSpan.FromMinutes(authenticationOptions.ClockSkewMinutes)
        };
    });

builder.Services.AddAuthorizationPolicies(permissionOptions, authenticationOptions.RequiredScopes.Select(s => (s.Claim, s.Name)));

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
    builder.Services.AddSwagger(swaggerOptions, authenticationOptions.RequiredScopes);
}

builder.Services.AddSingleton<ILinkBuilder<CategoryResponse>, CategoryLinkBuilder>();
builder.Services.AddSingleton<ILinkBuilder<ProductResponse>, ProductLinkBuilder>();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    //leave swagger unprotected for dev purposes
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Catalog Service API v1");
        if (swaggerOptions != null)
        {
            options.OAuthClientId(swaggerOptions.ClientId);
            options.OAuthUsePkce();
            options.OAuthScopes(authenticationOptions.RequiredScopes.Select(s => s.FullName).ToArray());
        }
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();