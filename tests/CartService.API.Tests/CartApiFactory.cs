using CartService.Application.Abstractions;
using CartService.Application.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CartService.API.Tests;

public class CartApiFactory : WebApplicationFactory<Program>
{
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
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICartRepository>();
            services.AddScoped<ICartRepository>(_ => MockRepository.Object);
        });
    }
}
