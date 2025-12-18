namespace CartService.API.CartEndpoints.Contracts;

/// <summary>
/// Represents image information for a cart item.
/// </summary>
/// <param name="Url">The URL of the image.</param>
/// <param name="AltText">Alternative text describing the image.</param>
public record ImageInfoResponse(
    string Url,
    string? AltText = null
);
