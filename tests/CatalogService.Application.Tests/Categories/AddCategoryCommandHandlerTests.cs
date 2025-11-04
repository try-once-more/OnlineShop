using CatalogService.Application.Categories;
using CatalogService.Domain.Entities;
using Moq;

namespace CatalogService.Application.Tests.Categories;

public class AddCategoryCommandHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_WhenValidCommandWithoutParent_ShouldCreateCategory()
    {
        var command = new AddCategoryCommand
        {
            Name = "Electronics",
            ImageUrl = new Uri("https://example.com/electronics.jpg"),
            ParentCategoryId = null
        };

        Category? capturedCategory = null;

        MockCategoryRepository
            .Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Callback<Category, CancellationToken>((cat, _) => capturedCategory = cat)
            .Returns(Task.CompletedTask);

        var result = await Sender.Send(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.ImageUrl, result.ImageUrl);
        Assert.Null(result.ParentCategory);

        MockCategoryRepository.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(capturedCategory);
        Assert.Equal(command.Name, capturedCategory.Name);
    }

    [Fact]
    public async Task Handle_WhenValidCommandWithParent_ShouldCreateCategoryWithParent()
    {
        var parentCategory = new Category { Id = 1, Name = "Electronics" };
        var command = new AddCategoryCommand
        {
            Name = "Laptops",
            ImageUrl = new Uri("https://example.com/laptops.jpg"),
            ParentCategoryId = parentCategory.Id
        };

        MockCategoryRepository
            .Setup(r => r.GetAsync(parentCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentCategory);

        Category? capturedCategory = null;
        MockCategoryRepository
            .Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Callback<Category, CancellationToken>((cat, _) => capturedCategory = cat)
            .Returns(Task.CompletedTask);

        var result = await Sender.Send(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.ImageUrl, result.ImageUrl);
        Assert.NotNull(result.ParentCategory);
        Assert.Equal(parentCategory.Id, result.ParentCategory.Id);
        Assert.Equal(parentCategory.Name, result.ParentCategory.Name);

        MockCategoryRepository.Verify(r => r.GetAsync(parentCategory.Id, It.IsAny<CancellationToken>()), Times.Once);
        MockCategoryRepository.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(capturedCategory);
        Assert.Equal(command.Name, capturedCategory.Name);
        Assert.Equal(parentCategory.Id, capturedCategory.ParentCategory?.Id);
    }
}
