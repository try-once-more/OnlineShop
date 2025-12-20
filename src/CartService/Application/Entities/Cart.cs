namespace CartService.Application.Entities;

public class Cart : BaseEntity<Guid>
{
    private IList<CartItem> items = [];
    public IReadOnlyList<CartItem> Items
    {
        get => items.AsReadOnly();
        init => items = [.. value];
    }

    public void AddItem(CartItem itemToAdd)
    {
        ArgumentNullException.ThrowIfNull(itemToAdd);

        var existingItem = items.FirstOrDefault(item => item.Equals(itemToAdd));
        if (existingItem is null)
        {
            items.Add(itemToAdd);
        }
        else
        {
            existingItem.Quantity += itemToAdd.Quantity;
        }
    }

    public bool RemoveItem(int itemId)
    {
        var existingItem = items.FirstOrDefault(item => item.Id == itemId);
        if (existingItem is null)
            return false;

        items.Remove(existingItem);
        return true;
    }

    public bool Clear()
    {
        if (items.Count == 0)
            return false;

        items.Clear();
        return true;
    }
}
