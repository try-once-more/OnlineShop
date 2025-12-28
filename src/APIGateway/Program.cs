using APIGateway.Aggregators;
using APIGateway.Configuration;
using APIGateway.HttpHandlers;
using APIGateway.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("scp");
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Add("scp", "scope");

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Authentication"));
builder.Services.Configure<AdminRole>(builder.Configuration.GetSection("AdminRole"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
builder.Services.AddSingleton<ErrorHandlingMiddleware>();

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

app.UseSwaggerForOcelotUI();

await app.UseOcelot();
await app.RunAsync();
