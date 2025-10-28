namespace CartService.Application;

public abstract class CartServiceException(string message) : Exception(message);

public sealed class EntityIdInvalidException()
    : CartServiceException("Id is required.");

public sealed class CartItemNameInvalidException()
    : CartServiceException("Name is required.");

public sealed class CartItemPriceInvalidException()
    : CartServiceException("Price must be positive.");

public sealed class CartItemQuantityInvalidException()
    : CartServiceException($"Quantity must be positive and cannot exceed {int.MaxValue}.");

public sealed class CartNotFoundException(Guid cartId)
    : CartServiceException($"Cart with Id '{cartId}' was not found.");

public sealed class CartItemNotAddedException(int itemId, Guid cartId)
    : CartServiceException($"Cart item with Id '{itemId}' was not added to the cart with Id '{cartId}'");
