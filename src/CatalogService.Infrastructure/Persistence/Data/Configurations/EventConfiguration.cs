using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Data.Configurations;

internal class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(o => o.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.EventType)
            .IsRequired();

        builder.Property(o => o.Payload)
            .IsRequired();

        builder.Property(o => o.OccurredAtUtc)
            .IsRequired();

        builder.Property(o => o.Processed)
            .IsRequired();

        builder.Property(o => o.ProcessedAtUtc);

        builder.Property(o => o.Error);

        builder.HasIndex(nameof(Event.Processed));
    }
}
