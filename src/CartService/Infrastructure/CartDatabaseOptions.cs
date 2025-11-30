namespace CartService.Infrastructure;

public record CartDatabaseOptions
{
    public required string CartDatabase { get; set; }
}