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
