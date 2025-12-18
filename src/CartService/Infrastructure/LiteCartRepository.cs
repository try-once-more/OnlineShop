using CartService.Application.Abstractions;
using CartService.Application.Entities;
using LiteDB;

namespace CartService.Infrastructure;

public class LiteCartRepository(ILiteDatabase database) : ICartRepository
{
    private const string Collection = "carts";
    private readonly ILiteDatabase database = database;

    public Task<Cart?> GetAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var col = database.GetCollection<Cart>(Collection);
        var cart = col.FindById(cartId);
        return Task.FromResult(cart);
    }

    public Task<IReadOnlyCollection<Cart>> GetCartsByItemIdAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var col = database.GetCollection<Cart>(Collection);
        IReadOnlyCollection<Cart> carts = [.. col.Find(Query.EQ("$.Items[*]._id ANY", itemId))];
        return Task.FromResult(carts);
    }

    public Task SaveAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cart);
        var col = database.GetCollection<Cart>(Collection);
        col.Upsert(cart.Id, cart);
        return Task.CompletedTask;
    }

    public Task SaveAsync(IEnumerable<Cart> carts, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(carts);
        database.BeginTrans();
        var col = database.GetCollection<Cart>(Collection);
        foreach (var cart in carts)
            col.Upsert(cart.Id, cart);

        database.Commit();
        return Task.CompletedTask;
    }
}

