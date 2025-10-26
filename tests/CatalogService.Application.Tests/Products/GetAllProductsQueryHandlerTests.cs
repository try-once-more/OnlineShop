using CatalogService.Application.Products;
using CatalogService.Domain.Entities;
using Moq;

namespace CatalogService.Application.Tests.Products;

public class GetAllProductsQueryHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_WhenProductsExist_ShouldReturnAllProducts()
    {
        var category = new Category { Id = 1, Name = "Electronics" };
        var expectedProducts = new List<Product>
        {
            new()
            {
                Id = 1,
                Name = "Laptop",
                Description = "High-performance laptop",
                ImageUrl = new Uri("https://example.com/laptop.jpg"),
                Category = category,
                Price = 999.99m,
                Amount = 10
            },
            new()
            {
                Id = 2,
                Name = "Smartphone",
                Description = "Latest smartphone",
                ImageUrl = new Uri("https://example.com/phone.jpg"),
                Category = category,
                Price = 699.99m,
                Amount = 25
            },
            new()
            {
                Id = 3,
                Name = "Tablet",
                Category = category,
                Price = 449.99m,
                Amount = 15
            }
        };

        MockProductRepository
            .Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);

        var query = new GetAllProductsQuery();

        var result = await Sender.Send(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedProducts.Count, result.Count);
        Assert.Collection(result,
            product =>
            {
                Assert.Equal(expectedProducts[0].Id, product.Id);
                Assert.Equal(expectedProducts[0].Name, product.Name);
                Assert.Equal(expectedProducts[0].Price, product.Price);
                Assert.Equal(expectedProducts[0].Amount, product.Amount);
            },
            product =>
            {
                Assert.Equal(expectedProducts[1].Id, product.Id);
                Assert.Equal(expectedProducts[1].Name, product.Name);
                Assert.Equal(expectedProducts[1].Price, product.Price);
                Assert.Equal(expectedProducts[1].Amount, product.Amount);
            },
            product =>
            {
                Assert.Equal(expectedProducts[2].Id, product.Id);
                Assert.Equal(expectedProducts[2].Name, product.Name);
                Assert.Equal(expectedProducts[2].Price, product.Price);
                Assert.Equal(expectedProducts[2].Amount, product.Amount);
            });

        MockProductRepository.Verify(r => r.ListAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoProducts_ShouldReturnEmptyCollection()
    {
        var query = new GetAllProductsQuery();

        MockProductRepository
            .Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        var result = await Sender.Send(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);

        MockProductRepository.Verify(r => r.ListAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
