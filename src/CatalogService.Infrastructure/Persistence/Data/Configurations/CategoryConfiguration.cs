using Microsoft.EntityFrameworkCore;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Persistence.Data.Configurations;

internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.ImageUrl)
            .HasConversion(x => x != null ? x.ToString() : null, x => x != null ? new Uri(x, UriKind.RelativeOrAbsolute) : null)
            .HasMaxLength(500);

        builder.HasOne(c => c.ParentCategory)
            .WithMany()
            .HasForeignKey("ParentCategoryId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex("ParentCategoryId");
    }
}
