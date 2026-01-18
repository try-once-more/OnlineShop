using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using CatalogService.API;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

[assembly: HotChocolate.Module("Types")]

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseKestrelHttpsConfiguration();


var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddOpenTelemetry()
        .UseAzureMonitor(options => options.ConnectionString = appInsightsConnectionString)
        .ConfigureResource(options =>
        {
            options.AddAttributes(new Dictionary<string, object> { { "service.name", "CatalogService.API" } });
        })
        .WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
            tracing.AddSource("Azure.*");
        })
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation());
}

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

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

ValidationServiceCollectionExtensions.AddValidation(builder.Services);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();

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
builder.Services.AddTransient<LoggingMiddleware>();
builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
builder.Services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureAuthorizationOptions>();

builder.Services.Configure<CatalogDatabaseOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<EventingOptions>(builder.Configuration.GetSection("Eventing"));
builder.Services.Configure<CatalogPublisherOptions>(builder.Configuration.GetSection("Eventing:CatalogService"));
builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Authentication"));
builder.Services.Configure<SwaggerOptions>(builder.Configuration.GetSection("Swagger"));
builder.Services.Configure<GraphQLOptions>(builder.Configuration.GetSection("GraphQL"));

builder.Services.AddCatalogServiceInfrastructure();
builder.Services.AddCatalogServiceApplication();
builder.Services.AddEventing();
builder.Services.AddContext();
builder.Services.AddHostedService<EventProcessor>();

builder.Services.AddMapper();

builder.Services.AddSwagger(builder);

var graphqlOptions = builder.Configuration.GetSection("GraphQL").Get<GraphQLOptions>() ?? new();

builder.Services.AddGraphQLServer()
    .AddAuthorization()
    .AddMutationConventions()
    .AddQueryContext()
    .AddDbContextCursorPagingProvider()
    .AddTypes()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .ModifyPagingOptions(opt =>
    {
        opt.DefaultPageSize = graphqlOptions.DefaultPageSize;
        opt.MaxPageSize = graphqlOptions.MaxPageSize;
        opt.IncludeTotalCount = graphqlOptions.IncludeTotalCount;
    })
    .ModifyRequestOptions(opt =>
    {
        opt.ExecutionTimeout = TimeSpan.FromSeconds(graphqlOptions.ExecutionTimeoutSeconds);
        opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    })
    .AddMaxExecutionDepthRule(graphqlOptions.MaxAllowedExecutionDepth, skipIntrospectionFields: true);

builder.Services.AddSingleton<ILinkBuilder<CategoryResponse>, CategoryLinkBuilder>();
builder.Services.AddSingleton<ILinkBuilder<ProductResponse>, ProductLinkBuilder>();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    //swagger unprotected just for dev purposes
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });
    app.UseSwaggerUI(options =>
    {
        var swaggerOptions = app.Services.GetService<IOptions<SwaggerOptions>>()?.Value ?? new();
        options.SwaggerEndpoint("/openapi/v1.json", "Catalog Service API v1");
        if (swaggerOptions != null)
        {
            options.OAuthClientId(swaggerOptions.ClientId);
            options.OAuthUsePkce();
            options.OAuthScopes(swaggerOptions.Scopes);
        }
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL("/graphql");

await app.RunAsync();
