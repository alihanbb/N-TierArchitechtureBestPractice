using AppRepository.Categories;
using AppRepository.Context;
using AppRepository.Products;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace UnitTest.Repositories;

public class CategoryRepositoryTests
{
    private readonly DbContextOptions<AppDbContextcs> _options;

    public CategoryRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContextcs>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    }

    private AppDbContextcs CreateContext()
    {
        var context = new AppDbContextcs(_options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetCategoryWithProductsAsync_ReturnsCategoryWithProducts()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        var category = new Category 
        { 
            CategoryId = 1, 
            CategoryName = "Electronics",
            Created = DateTime.UtcNow 
        };

        var products = new List<Product>
        {
            new Product 
            { 
                ProductName = "Laptop", 
                Price = 1000, 
                Stock = 10, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            },
            new Product 
            { 
                ProductName = "Mouse", 
                Price = 50, 
                Stock = 100, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            }
        };

        await context.Categorys.AddAsync(category);
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetCategoryWithProductsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.CategoryId.Should().Be(1);
        result.CategoryName.Should().Be("Electronics");
        result.Products.Should().NotBeNull();
        result.Products!.Should().HaveCount(2);
        result.Products.Should().Contain(p => p.ProductName == "Laptop");
        result.Products.Should().Contain(p => p.ProductName == "Mouse");
    }

    [Fact]
    public async Task GetCategoryWithProductsAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        // Act
        var result = await repository.GetCategoryWithProductsAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCategoryWithProductsAsync_WithNoProducts_ReturnsCategoryWithEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        var category = new Category 
        { 
            CategoryId = 1, 
            CategoryName = "Empty Category",
            Created = DateTime.UtcNow 
        };

        await context.Categorys.AddAsync(category);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetCategoryWithProductsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.CategoryId.Should().Be(1);
        result.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategoryByProductsAsync_ReturnsAllCategoriesWithProducts()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        var categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Electronics", Created = DateTime.UtcNow },
            new Category { CategoryId = 2, CategoryName = "Books", Created = DateTime.UtcNow },
            new Category { CategoryId = 3, CategoryName = "Empty", Created = DateTime.UtcNow }
        };

        var products = new List<Product>
        {
            new Product 
            { 
                ProductName = "Laptop", 
                Price = 1000, 
                Stock = 10, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            },
            new Product 
            { 
                ProductName = "Novel", 
                Price = 20, 
                Stock = 100, 
                CategoryId = 2,
                Created = DateTime.UtcNow 
            }
        };

        await context.Categorys.AddRangeAsync(categories);
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetCategoryByProductsAsync().ToListAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        var electronicsCategory = result.First(c => c.CategoryName == "Electronics");
        electronicsCategory.Products.Should().HaveCount(1);
        electronicsCategory.Products!.First().ProductName.Should().Be("Laptop");

        var booksCategory = result.First(c => c.CategoryName == "Books");
        booksCategory.Products.Should().HaveCount(1);
        booksCategory.Products!.First().ProductName.Should().Be("Novel");

        var emptyCategory = result.First(c => c.CategoryName == "Empty");
        emptyCategory.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsCategory()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        var category = new Category 
        { 
            CategoryId = 1, 
            CategoryName = "Test Category",
            Created = DateTime.UtcNow 
        };

        await context.Categorys.AddAsync(category);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryId.Should().Be(1);
        result.CategoryName.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsCategorySuccessfully()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        var category = new Category 
        { 
            CategoryName = "New Category",
            Created = DateTime.UtcNow 
        };

        // Act
        await repository.AddAsync(category);
        await context.SaveChangesAsync();

        // Assert
        var savedCategory = await context.Categorys.FirstOrDefaultAsync(c => c.CategoryName == "New Category");
        savedCategory.Should().NotBeNull();
        savedCategory!.CategoryId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Update_UpdatesCategorySuccessfully()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        var category = new Category 
        { 
            CategoryName = "Original",
            Created = DateTime.UtcNow 
        };

        await context.Categorys.AddAsync(category);
        await context.SaveChangesAsync();

        // Act
        category.CategoryName = "Updated";
        repository.Update(category);
        await context.SaveChangesAsync();

        // Assert
        var updatedCategory = await context.Categorys.FindAsync(category.CategoryId);
        updatedCategory.Should().NotBeNull();
        updatedCategory!.CategoryName.Should().Be("Updated");
    }

    [Fact]
    public async Task Delete_RemovesCategorySuccessfully()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        var category = new Category 
        { 
            CategoryName = "To Delete",
            Created = DateTime.UtcNow 
        };

        await context.Categorys.AddAsync(category);
        await context.SaveChangesAsync();
        var categoryId = category.CategoryId;

        // Act
        repository.Delete(category);
        await context.SaveChangesAsync();

        // Assert
        var deletedCategory = await context.Categorys.FindAsync(categoryId);
        deletedCategory.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsAllCategories()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        var categories = new List<Category>
        {
            new Category { CategoryName = "Category1", Created = DateTime.UtcNow },
            new Category { CategoryName = "Category2", Created = DateTime.UtcNow },
            new Category { CategoryName = "Category3", Created = DateTime.UtcNow }
        };

        await context.Categorys.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // Act
        var result = repository.GetAll().ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.CategoryName == "Category1");
        result.Should().Contain(c => c.CategoryName == "Category2");
        result.Should().Contain(c => c.CategoryName == "Category3");
    }

    [Fact]
    public async Task Where_FiltersCategoriesCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        var categories = new List<Category>
        {
            new Category { CategoryName = "Electronics", Created = DateTime.UtcNow },
            new Category { CategoryName = "Books", Created = DateTime.UtcNow },
            new Category { CategoryName = "Clothing", Created = DateTime.UtcNow }
        };

        await context.Categorys.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.where(c => c.CategoryName.Contains("o")).ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.CategoryName == "Books");
        result.Should().Contain(c => c.CategoryName == "Clothing");
        result.Should().NotContain(c => c.CategoryName == "Electronics");
    }

    [Fact]
    public async Task GetCategoryByProductsAsync_OnlyReturnsDistinctCategories()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new CategoryRepository(context);

        var category = new Category 
        { 
            CategoryId = 1, 
            CategoryName = "Electronics",
            Created = DateTime.UtcNow 
        };

        var products = new List<Product>
        {
            new Product 
            { 
                ProductName = "Laptop", 
                Price = 1000, 
                Stock = 10, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            },
            new Product 
            { 
                ProductName = "Phone", 
                Price = 500, 
                Stock = 20, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            },
            new Product 
            { 
                ProductName = "Tablet", 
                Price = 300, 
                Stock = 15, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            }
        };

        await context.Categorys.AddAsync(category);
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetCategoryByProductsAsync().ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().CategoryName.Should().Be("Electronics");
        result.First().Products.Should().HaveCount(3);
    }
}
