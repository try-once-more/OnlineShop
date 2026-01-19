using CartService.Grpc.Contracts;

namespace CartService.API.Tests.Grpc;

public class DecimalValueTests
{
    [Theory]
    [InlineData(12345, 678_900_000, 12345.6789)]
    [InlineData(0, 0, 0)]
    public void ImplicitConversionToDecimal_ReturnsExpectedValue(long units, int nanos, decimal expected)
    {
        DecimalValue grpcDecimal = new DecimalValue(units, nanos);
        decimal result = grpcDecimal;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ImplicitConversionToDecimal_Null_ReturnsZero()
    {
        DecimalValue? grpcDecimal = null;
        decimal result = grpcDecimal;
        Assert.Equal(0m, result);
    }

    [Theory]
    [InlineData(99.99, 99, 990_000_000)]
    [InlineData(100, 100, 0)]
    public void ImplicitConversionFromDecimal_SplitsCorrectly(decimal value, long expectedUnits, int expectedNanos)
    {
        DecimalValue result = value;
        Assert.Equal(expectedUnits, result.Units);
        Assert.Equal(expectedNanos, result.Nanos);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1.5)]
    [InlineData(999.999)]
    [InlineData(123456.789)]
    public void RoundTripConversion_PreservesValue(decimal original)
    {
        DecimalValue grpcValue = original;
        decimal converted = grpcValue;
        Assert.Equal(original, converted);
    }
}
