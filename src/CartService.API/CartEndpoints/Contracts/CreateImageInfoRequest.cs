using System.ComponentModel.DataAnnotations;

namespace CartService.API.CartEndpoints.Contracts;

/// <summary>
/// Request model for creating image information.
/// </summary>
/// <param name="Url">The URL of the image.</param>
/// <param name="AltText">Alternative text describing the image.</param>
public record CreateImageInfoRequest(
    [param: Required] Uri Url,
    [param: MinLength(0)] string AltText = ""
);