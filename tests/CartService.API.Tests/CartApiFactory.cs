using CartService.Application.Abstractions;
using CartService.Application.Entities;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using GrpcCartService = CartService.Grpc.Contracts.CartService;

namespace CartService.API.Tests;

public class CartApiFactory : WebApplicationFactory<Program>
{
    private GrpcChannel? channel;
    internal readonly Mock<ICartRepository> MockRepository = new();

    public CartApiFactory()
    {
        Dictionary<Guid, Cart> cartStorage = [];
        MockRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid cartId, CancellationToken _) =>
            {
                cartStorage.TryGetValue(cartId, out var cart);
                return cart;
            });

        MockRepository.Setup(r => r.SaveAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
            .Callback<Cart, CancellationToken>((cart, _) => cartStorage[cart.Id] = cart)
            .Returns(Task.CompletedTask);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "IntegrationTests";
        builder.UseEnvironment(environment);
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var projectDir = Directory.GetCurrentDirectory();
            config.AddJsonFile(Path.Combine(projectDir, "appsettings.json"), optional: false)
                  .AddJsonFile(Path.Combine(projectDir, $"appsettings.{environment}.json"), optional: true)
                  .AddEnvironmentVariables();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICartRepository>();
            services.AddScoped<ICartRepository>(_ => MockRepository.Object);

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(TestAuthenticationHandler.TestScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddAuthentication(TestAuthenticationHandler.TestScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.TestScheme, options => { });
        });
    }

    public GrpcCartService.CartServiceClient CreateGrpcClient()
    {
        if (channel != null)
            return new GrpcCartService.CartServiceClient(channel);

        channel = GrpcChannel.ForAddress(Server.BaseAddress, new GrpcChannelOptions
        {
            HttpHandler = Server.CreateHandler()
        });

        return new GrpcCartService.CartServiceClient(channel);
    }

    public override async ValueTask DisposeAsync()
    {
        if (channel != null)
        {
            await channel.ShutdownAsync();
            channel.Dispose();
        }
        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
