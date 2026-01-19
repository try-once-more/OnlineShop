using Azure.Monitor.OpenTelemetry.AspNetCore;
using CartService.API;
using CartService.API.Configuration;
using CartService.API.Endpoints;
using CartService.API.Grpc;
using CartService.API.Middlewares;
using CartService.Application;
using CartService.Infrastructure;
using Eventing.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseKestrelHttpsConfiguration();


var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddOpenTelemetry()
        .UseAzureMonitor(options => options.ConnectionString = appInsightsConnectionString)
        .ConfigureResource(options =>
        {
            options.AddAttributes(new Dictionary<string, object> { { "service.name", "CartService.API" } });
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
builder.Services.Configure<GrpcOptions>(builder.Configuration.GetSection("Grpc"));

builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
builder.Services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureAuthorizationOptions>();
builder.Services.AddSingleton<ErrorHandlingMiddleware>();
builder.Services.AddSingleton<IdentityTokenLoggingMiddleware>();
builder.Services.AddSingleton<LoggingMiddleware>();

builder.Services.AddCartServiceInfrastructure();
builder.Services.AddCartServiceApplication();
builder.Services.AddEventing();
builder.Services.AddContext();
builder.Services.AddHostedService<EventProcessor>();
builder.Services.AddMapper();

var grpcOptions = builder.Configuration.GetSection("Grpc").Get<GrpcOptions>() ?? new();
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GrpcExceptionInterceptor>();
    options.EnableDetailedErrors = grpcOptions.EnableDetailedErrors;

    if (grpcOptions.MaxReceiveMessageSize.HasValue)
    {
        options.MaxReceiveMessageSize = grpcOptions.MaxReceiveMessageSize.Value;
    }

    if (grpcOptions.MaxSendMessageSize.HasValue)
    {
        options.MaxSendMessageSize = grpcOptions.MaxSendMessageSize.Value;
    }

    if (grpcOptions.ResponseCompression != null)
    {
        options.ResponseCompressionAlgorithm = grpcOptions.ResponseCompression.Algorithm;
        options.ResponseCompressionLevel = grpcOptions.ResponseCompression.Level switch
        {
            "Optimal" => System.IO.Compression.CompressionLevel.Optimal,
            "Fastest" => System.IO.Compression.CompressionLevel.Fastest,
            "NoCompression" => System.IO.Compression.CompressionLevel.NoCompression,
            "SmallestSize" => System.IO.Compression.CompressionLevel.SmallestSize,
            _ => System.IO.Compression.CompressionLevel.Optimal
        };
    }
});

if (grpcOptions.EnableReflection)
{
    builder.Services.AddGrpcReflection();
}

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseSerilogRequestLogging();

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
app.MapGrpcService<CartGrpcService>();
if (grpcOptions.EnableReflection)
{
    app.MapGrpcReflectionService();
}

await app.RunAsync();
