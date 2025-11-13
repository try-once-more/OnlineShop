namespace CartService.API.Tests.Endpoints;

public class CartResponse
{
    public Guid Id { get; set; }
    public IReadOnlyCollection<CartItemResponse> Items { get; set; }
}

public class CartItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public ImageInfoResponse? Image { get; set; }
}

public class ImageInfoResponse
{
    public string Url { get; set; } = default!;
    public string AltText { get; set; } = default!;
}
