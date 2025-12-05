using CartService.API.Configuration;
using CartService.API.Endpoints;
using CartService.API.Middlewares;
using CartService.Application;
using CartService.Infrastructure;
using Eventing.Infrastructure;

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
builder.Services.AddOpenApi("v1");
builder.Services.AddOpenApi("v2");
builder.Services.AddValidation();

builder.Services.AddLogging();
builder.Services.AddMetrics();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<ErrorHandlingMiddleware>();

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
app.UseHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/openapi/v1.json", $"Cart API v1");
        options.SwaggerEndpoint($"/openapi/v2.json", $"Cart API v2");
    });
}

app.MapCartEndpointsV1();
app.MapCartEndpointsV2();

app.Run();