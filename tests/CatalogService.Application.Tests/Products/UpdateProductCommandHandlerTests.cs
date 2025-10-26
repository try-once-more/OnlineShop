using CatalogService.Application.Products;
using CatalogService.Domain.Entities;
using Moq;

namespace CatalogService.Application.Tests.Products;

public class UpdateProductCommandHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_WhenUpdatingAllFields_ShouldUpdateProduct()
    {
        var oldCategory = new Category { Id = 1, Name = "Electronics" };
        var newCategory = new Category { Id = 2, Name = "Computers" };

        var existingProduct = new Product
        {
            Id = 1,
            Name = "Old Laptop",
            Description = "Old description",
            ImageUrl = new Uri("https://example.com/old.jpg"),
            Category = oldCategory,
            Price = 500m,
            Amount = 5
        };

        var command = new UpdateProductCommand(
            Id: 1,
            Name: "New Gaming Laptop",
            Description: "High-end gaming laptop",
            ImageUrl: new Uri("https://example.com/new.jpg"),
            CategoryId: 2,
            Price: 1999.99m,
            Amount: 15);

        MockProductRepository
            .Setup(r => r.GetAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        MockCategoryRepository
            .Setup(r => r.GetAsync(newCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        Product? updatedProduct = null;
        MockProductRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((prod, _) => updatedProduct = prod)
            .Returns(Task.CompletedTask);

        await Sender.Send(command, CancellationToken.None);

        Assert.NotNull(updatedProduct);
        Assert.Equal(command.Name, updatedProduct.Name);
        Assert.Equal(command.Description, updatedProduct.Description);
        Assert.Equal(command.ImageUrl, updatedProduct.ImageUrl);
        Assert.Equal(command.Price, updatedProduct.Price);
        Assert.Equal(command.Amount, updatedProduct.Amount);
        Assert.Equal(newCategory.Id, updatedProduct.Category.Id);

        MockProductRepository.Verify(r => r.GetAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        MockCategoryRepository.Verify(r => r.GetAsync(newCategory.Id, It.IsAny<CancellationToken>()), Times.Once);
        MockProductRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUpdatingPartialFields_ShouldUpdateOnlySpecifiedFields()
    {
        var category = new Category { Id = 1, Name = "Electronics" };

        var existingProduct = new Product
        {
            Id = 1,
            Name = "Laptop",
            Description = "Original description",
            ImageUrl = new Uri("https://example.com/laptop.jpg"),
            Category = category,
            Price = 999.99m,
            Amount = 10
        };

        var command = new UpdateProductCommand(
            Id: 1,
            Name: "Updated Laptop",
            Description: null,
            ImageUrl: null,
            CategoryId: null,
            Price: 1299.99m,
            Amount: null);

        MockProductRepository
            .Setup(r => r.GetAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        Product? updatedProduct = null;
        MockProductRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((prod, _) => updatedProduct = prod)
            .Returns(Task.CompletedTask);

        await Sender.Send(command, CancellationToken.None);

        Assert.NotNull(updatedProduct);
        Assert.Equal(command.Name, updatedProduct.Name);
        Assert.Equal(existingProduct.Description, updatedProduct.Description);
        Assert.Equal(existingProduct.ImageUrl, updatedProduct.ImageUrl);
        Assert.Equal(command.Price, updatedProduct.Price);
        Assert.Equal(existingProduct.Amount, updatedProduct.Amount);
        Assert.Equal(category.Id, updatedProduct.Category.Id);

        MockProductRepository.Verify(r => r.GetAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        MockProductRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
