using CartService.Application.Entities;

namespace CartService.Tests;

internal static class TestHelper
{
    internal static Cart CreateCartWithRandomItems(
        int itemCount,
        decimal minPrice = 0.01m,
        decimal maxPrice = 1000m,
        int minQuantity = 1,
        int maxQuantity = int.MaxValue) =>
        CreateCartWithRandomItems(Guid.NewGuid(), itemCount, minPrice, maxPrice, minQuantity, maxQuantity);

    internal static Cart CreateCartWithRandomItems(Guid cartId,
        int itemCount,
        decimal minPrice = 0.01m,
        decimal maxPrice = 1000m,
        int minQuantity = 1,
        int maxQuantity = int.MaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minPrice, maxPrice);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minQuantity, maxQuantity);

        var cart = new Cart { Id = cartId };
        for (int i = 1; i <= itemCount; i++)
        {
            ImageInfo? image = Random.Shared.NextDouble() < 0.5
                ? null
                : new ImageInfo(
                    Url: new Uri($"https://example.com/image{i}.png"),
                    AltText: $"Image {i}"
                );

            var item = new CartItem
            {
                Id = i,
                Name = $"Item{i}",
                Price = NextPrice(minPrice, maxPrice),
                Quantity = NextQuantity(minQuantity, maxQuantity),
                Image = image
            };

            cart.AddItem(item);
        }

        return cart;
    }

    internal static decimal NextPrice(decimal min, decimal max) =>
        Math.Round(min + (decimal)Random.Shared.NextDouble() * (max - min), 2);

    internal static int NextQuantity(int min, int max) =>
        Random.Shared.Next(min, max);
}
