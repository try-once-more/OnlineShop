namespace CartService.Application.Entities;

public record class ImageInfo(Uri Url, string AltText = "");

public enum CartItemStatus
{
    Discontinued = -1,
    OutOfStock = 0,
    Available = 1
}

public class CartItem : BaseEntity<int>
{
    public required string Name
    {
        get;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new CartItemNameInvalidException();

            field = value;
        }
    }

    public required decimal Price
    {
        get;
        set
        {
            if (value <= 0)
                throw new CartItemPriceInvalidException();

            field = value;
        }
    }

    public int Quantity
    {
        get;
        set
        {
            if (value <= 0)
                throw new CartItemQuantityInvalidException();

            field = value;
        }
    } = 1;

    public ImageInfo? Image { get; set; }

    public CartItemStatus Status { get; set; } = CartItemStatus.Available;
}