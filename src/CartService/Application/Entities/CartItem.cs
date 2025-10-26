namespace CartService.Application.Entities;

public record class ImageInfo(Uri Url, string AltText = "");

public class CartItem : BaseEntity<int>
{
    public required string Name
    {
        get;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new CartItemNameInvalidException();

            field = value;
        }
    }

    public required decimal Price
    {
        get;
        init
        {
            if (value <= 0)
                throw new CartItemPriceInvalidException();

            field = value;
        }
    }

    private int quantity;
    public required int Quantity
    {
        get => quantity;
        init
        {
            if (value <= 0)
                throw new CartItemQuantityInvalidException();

            quantity = value;
        }
    }

    public ImageInfo? Image { get; set; }

    public void IncreaseQuantity(int value)
    {
        if (value <= 0 || value > int.MaxValue - Quantity)
            throw new CartItemQuantityInvalidException();

        quantity += value;
    }

    public void DecreaseQuantity(int value)
    {
        if (value <= 0 || value > Quantity)
            throw new CartItemQuantityInvalidException();

        quantity -= value;
    }
}