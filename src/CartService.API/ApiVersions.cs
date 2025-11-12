namespace CartService.API;

public class ApiVersions
{
    public const string V1 = "v1";
    public const string V2 = "v2";

    public static readonly IReadOnlyList<string> All = [V1, V2];
}
