using CatalogService.Domain.Entities;
using CatalogService.Domain.Exceptions;

namespace CatalogService.Domain.Tests.Entities;

public class CategoryTests
{
    internal static Category GetValidCategory(Category? parentCategory = null) => new()
    {
        Id = Random.Shared.Next(),
        Name = $"{Guid.NewGuid():N}",
        ImageUrl = new Uri($"https://example.com/{Guid.NewGuid():N}"),
        ParentCategory = parentCategory
    };

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Name_SetEmptyOrWhitespace_ShouldThrowCategoryValidationException(string invalidName)
    {
        var category = GetValidCategory();
        Assert.Throws<CategoryValidationException>(() => category.Name = invalidName);
    }

    [Fact]
    public void Name_SetTooLong_ShouldThrowCategoryValidationException()
    {
        var category = GetValidCategory();
        var longName = new string('A', 51);
        Assert.Throws<CategoryValidationException>(() => category.Name = longName);
    }

    [Fact]
    public void Name_SetMaxLength_ShouldSucceed()
    {
        var category = GetValidCategory();
        var maxLengthName = new string('A', 50);

        category.Name = maxLengthName;

        Assert.Equal(maxLengthName, category.Name);
    }

    [Fact]
    public void ParentCategory_SetValidParent_ShouldAssignParent()
    {
        var parentCategory = GetValidCategory();
        var childCategory = GetValidCategory();

        childCategory.ParentCategory = parentCategory;

        Assert.NotNull(childCategory.ParentCategory);
        Assert.Equal(parentCategory, childCategory.ParentCategory);
    }

    [Fact]
    public void ParentCategory_SetSelf_ShouldThrowCategoryValidationException()
    {
        var category = GetValidCategory();
        Assert.Throws<CategoryValidationException>(() => category.ParentCategory = category);
    }

    [Fact]
    public void ParentCategory_SetCircularReference_ShouldThrowCategoryValidationException()
    {
        var grandParent = GetValidCategory();
        var parent = GetValidCategory(grandParent);
        var child = GetValidCategory(parent);

        Assert.Throws<CategoryValidationException>(() => grandParent.ParentCategory = child);
    }

    [Fact]
    public void ParentCategory_SetNull_ShouldClearParent()
    {
        var parentCategory = GetValidCategory();
        var childCategory = GetValidCategory(parentCategory);
        childCategory.ParentCategory = null;

        Assert.Null(childCategory.ParentCategory);
    }

    [Fact]
    public void Constructor_WithoutImageUrl_ShouldCreateCategoryWithNullImage()
    {
        var category = new Category
        {
            Id = 1,
            Name = "Electronics"
        };

        Assert.Equal("Electronics", category.Name);
        Assert.Null(category.ImageUrl);
    }
}
