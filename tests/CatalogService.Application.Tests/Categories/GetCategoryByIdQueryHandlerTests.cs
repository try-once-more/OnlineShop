using CatalogService.Application.Categories;
using CatalogService.Domain.Entities;
using Moq;

namespace CatalogService.Application.Tests.Categories;

public class GetCategoryByIdQueryHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_WhenCategoryExists_ShouldReturnCategory()
    {
        var categoryId = 1;
        var expectedCategory = new Category
        {
            Id = categoryId,
            Name = "Electronics",
            ImageUrl = new Uri("https://example.com/electronics.jpg")
        };

        MockCategoryRepository
            .Setup(r => r.GetAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCategory);

        var query = new GetCategoryByIdQuery { Id = categoryId };

        var result = await Sender.Send(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedCategory.Id, result.Id);
        Assert.Equal(expectedCategory.Name, result.Name);
        Assert.Equal(expectedCategory.ImageUrl, result.ImageUrl);

        MockCategoryRepository.Verify(r => r.GetAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
