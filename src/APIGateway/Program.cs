using APIGateway.Aggregators;
using APIGateway.Configuration;
using APIGateway.HttpHandlers;
using APIGateway.Middlewares;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddOpenTelemetry()
        .UseAzureMonitor(options => options.ConnectionString = appInsightsConnectionString)
        .ConfigureResource(options =>
        {
            options.AddAttributes(new Dictionary<string, object> { { "service.name", "APIGateway" } });
        })
        .WithTracing(tracing => tracing.AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation())
        .WithMetrics(metrics => metrics
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation());
}

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("scp");
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Add("scp", "scope");

builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Authentication"));
builder.Services.Configure<AdminRole>(builder.Configuration.GetSection("AdminRole"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
builder.Services.AddSingleton<ErrorHandlingMiddleware>();
builder.Services.AddTransient<LoggingMiddleware>();

builder.Services.AddOcelot(builder.Configuration)
    .AddCacheManager(x => x.WithDictionaryHandle())
    .AddDelegatingHandler<RequireAdminHandler>()
    .AddTransientDefinedAggregator<RouteKeysAggregator>();

builder.Services.AddSwaggerForOcelot(builder.Configuration, o =>
{
    // Does not work currently as Swagger cannot retrieve OpenAPI documentation for aggregate routes
    o.GenerateDocsForAggregates = true;
});

var app = builder.Build();
app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwaggerForOcelotUI();

await app.UseOcelot();
await app.RunAsync();
