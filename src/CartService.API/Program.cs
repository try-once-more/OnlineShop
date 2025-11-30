using CartService.API;
using CartService.API.Endpoints;
using CartService.API.Middlewares;
using CartService.Application.Entities;
using Mapster;
using MapsterMapper;
using System.Reflection;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        string key = httpContext.User.Identity?.Name
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? string.Empty;

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    foreach (var version in ApiVersions.All)
    {
        options.SwaggerDoc(version, new() { Title = "Cart API", Version = version });
    }

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddLogging();
builder.Services.AddMetrics();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<ErrorHandlingMiddleware>();

builder.Services.Configure<CartDatabaseSettings>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<EventingOptions>(builder.Configuration.GetSection("Eventing"));
builder.Services.Configure<CartItemEventOptions>(builder.Configuration.GetSection("Eventing:CartService"));
builder.Services.AddCartServiceInfrastructure();
builder.Services.AddCartServiceApplication();
builder.Services.AddEventing();
builder.Services.AddHostedService<EventProcessor>();
AddMapper(builder.Services);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(o => o.RouteTemplate = "openapi/{documentName}.json");
    app.UseSwaggerUI(options =>
    {
        foreach (var version in ApiVersions.All)
        {
            options.SwaggerEndpoint($"/openapi/{version}.json", $"Cart API {version}");
        }
    });
}

app.MapCartEndpointsV1();
app.MapCartEndpointsV2();

app.Run();

static void AddMapper(IServiceCollection services)
{
    var config = TypeAdapterConfig.GlobalSettings;
    config.NewConfig<CartItem, CartItemResponse>();
    config.NewConfig<ImageInfo, ImageInfoResponse>();

    services.AddSingleton<IMapper>(new Mapper(config));
}


public partial class Program { }