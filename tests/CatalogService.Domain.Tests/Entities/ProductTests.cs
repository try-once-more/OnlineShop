using CatalogService.Domain.Entities;
using CatalogService.Domain.Exceptions;

namespace CatalogService.Domain.Tests.Entities;

public class ProductTests
{
    private static Category ValidCategory() => CategoryTests.GetValidCategory();

    [Fact]
    public void Create_WithValidData_ShouldInitializeProperties()
    {
        var category = ValidCategory();
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Description = "High-performance laptop",
            ImageUrl = new Uri("https://example.com/laptop.jpg"),
            Category = category,
            Price = 999.99m,
            Amount = 10
        };

        Assert.Equal("Laptop", product.Name);
        Assert.Equal("High-performance laptop", product.Description);
        Assert.Equal(category, product.Category);
        Assert.Equal(999.99m, product.Price);
        Assert.Equal(10, product.Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Name_SetInvalid_ShouldThrowProductValidationException(string? invalidName)
    {
        var product = new Product
        {
            Id = 1,
            Name = "Valid",
            Category = ValidCategory(),
            Price = 100m,
            Amount = 10
        };

        Assert.Throws<ProductValidationException>(() => product.Name = invalidName);
    }

    [Fact]
    public void Category_SetNull_ShouldThrowProductValidationException()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Category = ValidCategory(),
            Price = 100m,
            Amount = 10
        };

        Assert.Throws<ProductValidationException>(() => product.Category = null);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Price_SetNonPositive_ShouldThrowProductValidationException(decimal invalidPrice)
    {
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Category = ValidCategory(),
            Price = 100m,
            Amount = 10
        };

        Assert.Throws<ProductValidationException>(() => product.Price = invalidPrice);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void Amount_SetValidValue_ShouldAssignValue(int validAmount)
    {
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Category = ValidCategory(),
            Price = 100m,
            Amount = 10
        };

        product.Amount = validAmount;

        Assert.Equal(validAmount, product.Amount);
    }

    [Fact]
    public void Category_SetValid_ShouldChangeCategory()
    {
        var category1 = ValidCategory();
        var category2 = new Category { Id = 2, Name = "Home Appliances" };
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Category = category1,
            Price = 999.99m,
            Amount = 10
        };

        product.Category = category2;

        Assert.Equal(category2, product.Category);
    }
}

