using CatalogService.Application.Abstractions.Repository;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence.Data;

namespace CatalogService.Infrastructure.Persistence.Repositories;

internal class EventRepository(CatalogDbContext context)
    : Repository<Event, Guid>(context), IEventRepository;
