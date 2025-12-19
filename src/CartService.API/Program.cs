using CartService.API;
using CartService.API.Configuration;
using CartService.API.Endpoints;
using CartService.API.Middlewares;
using CartService.Application;
using CartService.Infrastructure;
using Eventing.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseKestrelHttpsConfiguration();

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerDocumentTransformer>();
    options.AddOperationTransformer<BearerOperationTransformer>();

    if (builder.Environment.IsDevelopment())
    {
        options.AddDocumentTransformer<SwaggerDocumentTransformer>();
        options.AddOperationTransformer<SwaggerOperationTransformer>();
    }
});
builder.Services.AddOpenApi("v2", options =>
{
    options.AddDocumentTransformer<BearerDocumentTransformer>();
    options.AddOperationTransformer<BearerOperationTransformer>();

    if (builder.Environment.IsDevelopment())
    {
        options.AddDocumentTransformer<SwaggerDocumentTransformer>();
        options.AddOperationTransformer<SwaggerOperationTransformer>();
    }
});
builder.Services.AddValidation();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddLogging();
builder.Services.AddMetrics();
builder.Services.AddHealthChecks();

builder.Services.Configure<CartDatabaseOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<EventingOptions>(builder.Configuration.GetSection("Eventing"));
builder.Services.Configure<CartSubscriberOptions>(builder.Configuration.GetSection("Eventing:CartService"));
builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Authentication"));
builder.Services.Configure<SwaggerOptions>(builder.Configuration.GetSection("Swagger"));

builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
builder.Services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureAuthorizationOptions>();
builder.Services.AddSingleton<ErrorHandlingMiddleware>();
builder.Services.AddSingleton<IdentityTokenLoggingMiddleware>();

builder.Services.AddCartServiceInfrastructure();
builder.Services.AddCartServiceApplication();
builder.Services.AddEventing();
builder.Services.AddHostedService<EventProcessor>();
builder.Services.AddMapper();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<IdentityTokenLoggingMiddleware>();
app.UseAuthorization();

var openApi = app.MapOpenApi();
if (app.Environment.IsDevelopment())
{
    //swagger unprotected just for dev purposes
    openApi.AllowAnonymous();
    app.MapGet("/swagger/{**any}", () => Results.Redirect("/swagger/index.html"))
        .ExcludeFromDescription()
        .AllowAnonymous();

    app.UseSwaggerUI(options =>
    {
        var swaggerOptions = app.Services.GetService<IOptions<SwaggerOptions>>()?.Value ?? new();
        options.SwaggerEndpoint($"/openapi/v1.json", $"Cart API v1");
        options.SwaggerEndpoint($"/openapi/v2.json", $"Cart API v2");
        options.OAuthClientId(swaggerOptions.ClientId);
        options.OAuthUsePkce();
        options.OAuthScopes(swaggerOptions.Scopes);
    });
}

app.UseHealthChecks("/health");
app.MapCartEndpointsV1();
app.MapCartEndpointsV2();

await app.RunAsync();
