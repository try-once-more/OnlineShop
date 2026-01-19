namespace CartService.Grpc.Contracts;

/// <summary>
/// Fixed-point decimal representation for Protobuf/gRPC.
/// </summary>
public partial class DecimalValue
{
    private const decimal NanoFactor = 1_000_000_000;

    /// <summary>
    /// Creates a Protobuf-compatible decimal value.
    /// </summary>
    /// <param name="units">Whole part of the decimal value.</param>
    /// <param name="nanos">
    /// Nano units of the decimal value (10^-9).
    /// Must be same sign as units.
    /// </param>
    public DecimalValue(long units, int nanos)
    {
        Units = units;
        Nanos = nanos;
    }

    public static implicit operator decimal(DecimalValue? grpcDecimal)
    {
        if (grpcDecimal is null)
        {
            return 0m;
        }

        return grpcDecimal.Units + grpcDecimal.Nanos / NanoFactor;
    }

    public static implicit operator DecimalValue(decimal value)
    {
        var units = decimal.ToInt64(value);
        var nanos = decimal.ToInt32((value - units) * NanoFactor);
        return new DecimalValue(units, nanos);
    }
}
