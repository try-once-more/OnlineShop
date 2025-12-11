using CartService.API.Configuration;
using CartService.API.Endpoints;
using CartService.API.Middlewares;
using CartService.Application;
using CartService.Infrastructure;
using Eventing.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseKestrelHttpsConfiguration();

builder.Services.Configure<AuthenticationOptions>(builder.Configuration.GetSection("Authentication"));
var authenticationOptions = builder.Configuration.GetSection("Authentication").Get<AuthenticationOptions>()
    ?? throw new InvalidOperationException("Authentication configuration is missing");

var permissionOptions = builder.Configuration.GetSection("Permissions").Get<PermissionOptions>()
    ?? throw new InvalidOperationException("Permissions configuration is missing");

builder.Services.Configure<SwaggerOptions>(builder.Configuration.GetSection("Swagger"));
var swaggerOptions = builder.Configuration.GetSection("Swagger").Get<SwaggerOptions>();

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<SecurityDocumentTransformer>();
    options.AddOperationTransformer<SecurityOperationTransformer>();
});
builder.Services.AddOpenApi("v2", options =>
{
    options.AddDocumentTransformer<SecurityDocumentTransformer>();
    options.AddOperationTransformer<SecurityOperationTransformer>();
});
builder.Services.AddValidation();

builder.Services.AddLogging();
builder.Services.AddMetrics();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<ErrorHandlingMiddleware>();
builder.Services.AddSingleton<IdentityTokenLoggingMiddleware>();

builder.Services.Configure<CartDatabaseOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<EventingOptions>(builder.Configuration.GetSection("Eventing"));
builder.Services.Configure<CartSubscriberOptions>(builder.Configuration.GetSection("Eventing:CartService"));
builder.Services.AddCartServiceInfrastructure();
builder.Services.AddCartServiceApplication();
builder.Services.AddEventing();
builder.Services.AddHostedService<EventProcessor>();
builder.Services.AddMapper();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    //leave swagger unprotected for dev purposes
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/openapi/v1.json", $"Cart API v1");
        options.SwaggerEndpoint($"/openapi/v2.json", $"Cart API v2");
        options.OAuthClientId(swaggerOptions?.ClientId);
        options.OAuthUsePkce();
        options.OAuthScopes([.. authenticationOptions.RequiredScopes.Select(s => s.FullName)]);
    });
}

app.UseAuthentication();
app.UseMiddleware<IdentityTokenLoggingMiddleware>();
app.UseAuthorization();

app.UseHealthChecks("/health");
app.MapCartEndpointsV1();
app.MapCartEndpointsV2();

app.Run();