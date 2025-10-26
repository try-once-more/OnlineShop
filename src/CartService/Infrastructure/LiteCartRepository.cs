using CartService.Application.Abstractions;
using CartService.Application.Entities;
using LiteDB;

namespace CartService.Infrastructure;

public class LiteCartRepository(ILiteDatabase database) : ICartRepository
{
    private const string Collection = "carts";
    private readonly ILiteDatabase database = database;

    public Task<Cart?> GetAsync(Guid cartId)
    {
        var col = database.GetCollection<Cart>(Collection);
        var cart = col.FindById(cartId);
        return Task.FromResult(cart);
    }

    public Task SaveAsync(Cart cart)
    {
        var col = database.GetCollection<Cart>(Collection);
        col.Upsert(cart.Id, cart);
        return Task.FromResult(cart);
    }
}

