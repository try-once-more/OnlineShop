using CatalogService.Application.Abstractions.Repository;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace CatalogService.Application.Tests;

public abstract class HandlerTestBase
{
    protected readonly Mock<IUnitOfWork> MockUnitOfWork = new();
    protected readonly Mock<ICategoryRepository> MockCategoryRepository = new();
    protected readonly Mock<IProductRepository> MockProductRepository = new();
    protected readonly Mock<IOptions<CatalogPublisherOptions>> MockCatalogPublisherOptions = new();
    protected readonly ISender Sender;

    protected HandlerTestBase()
    {
        MockUnitOfWork.Setup(u => u.Categories).Returns(MockCategoryRepository.Object);
        MockUnitOfWork.Setup(u => u.Products).Returns(MockProductRepository.Object);
        MockCatalogPublisherOptions.Setup(o => o.Value).Returns(new CatalogPublisherOptions { TopicName = string.Empty });

        var services = new ServiceCollection();
        services.AddCatalogServiceApplication();
        services.AddScoped(_ => MockUnitOfWork.Object);
        services.AddSingleton(_ => MockCatalogPublisherOptions.Object);

        Sender = services.BuildServiceProvider().GetRequiredService<ISender>();
    }
}
