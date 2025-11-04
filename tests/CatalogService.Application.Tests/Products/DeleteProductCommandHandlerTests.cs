using CatalogService.Application.Products;
using Moq;

namespace CatalogService.Application.Tests.Products;

public class DeleteProductCommandHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_WhenValidProductId_ShouldDeleteProduct()
    {
        var productId = 1;
        var command = new DeleteProductCommand { Id = productId };

        int deletedProductId = 0;
        MockProductRepository
            .Setup(r => r.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .Callback<int, CancellationToken>((id, _) => deletedProductId = id)
            .Returns(Task.CompletedTask);

        await Sender.Send(command, CancellationToken.None);

        Assert.Equal(productId, deletedProductId);
        MockProductRepository.Verify(r => r.DeleteAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(999)]
    public async Task Handle_WhenDifferentProductIds_ShouldDeleteCorrectProduct(int productId)
    {
        var command = new DeleteProductCommand { Id = productId };
        int deletedProductId = 0;

        MockProductRepository
            .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<int, CancellationToken>((id, _) => deletedProductId = id)
            .Returns(Task.CompletedTask);

        await Sender.Send(command, CancellationToken.None);

        Assert.Equal(productId, deletedProductId);
        MockProductRepository.Verify(r => r.DeleteAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
