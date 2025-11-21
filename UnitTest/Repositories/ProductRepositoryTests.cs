using AppRepository.Context;
using AppRepository.Products;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;

namespace UnitTest.Repositories;

public class ProductRepositoryTests
{
    private readonly DbContextOptions<AppDbContextcs> _options;

    public ProductRepositoryTests()
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
    public async Task GetTopPriceProductAsync_ReturnsTopProducts()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProductRepository(context);

        var products = new List<Product>
        {
            new Product 
            { 
                Id = 1, 
                ProductName = "Expensive", 
                Price = 1000, 
                Stock = 10, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            },
            new Product 
            { 
                Id = 2, 
                ProductName = "Cheap", 
                Price = 100, 
                Stock = 20, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            },
            new Product 
            { 
                Id = 3, 
                ProductName = "Medium", 
                Price = 500, 
                Stock = 15, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetTopPriceProductAsync(2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Price.Should().Be(1000);
        result.Last().Price.Should().Be(500);
    }

    [Fact]
    public async Task GetTopPriceProductAsync_WithZeroCount_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProductRepository(context);

        var products = new List<Product>
        {
            new Product 
            { 
                Id = 1, 
                ProductName = "Product", 
                Price = 1000, 
                Stock = 10, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetTopPriceProductAsync(0);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProductRepository(context);

        var product = new Product 
        { 
            Id = 1, 
            ProductName = "Test Product", 
            Price = 500, 
            Stock = 10, 
            CategoryId = 1,
            Created = DateTime.UtcNow 
        };

        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.ProductName.Should().Be("Test Product");
        result.Price.Should().Be(500);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsProductSuccessfully()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProductRepository(context);

        var product = new Product 
        { 
            ProductName = "New Product", 
            Price = 300, 
            Stock = 5, 
            CategoryId = 1,
            Created = DateTime.UtcNow 
        };

        // Act
        await repository.AddAsync(product);
        await context.SaveChangesAsync();

        // Assert
        var savedProduct = await context.Products.FirstOrDefaultAsync(p => p.ProductName == "New Product");
        savedProduct.Should().NotBeNull();
        savedProduct!.Price.Should().Be(300);
        savedProduct.Stock.Should().Be(5);
    }

    [Fact]
    public async Task Update_UpdatesProductSuccessfully()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProductRepository(context);

        var product = new Product 
        { 
            ProductName = "Original", 
            Price = 100, 
            Stock = 10, 
            CategoryId = 1,
            Created = DateTime.UtcNow 
        };

        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        // Act
        product.ProductName = "Updated";
        product.Price = 200;
        repository.Update(product);
        await context.SaveChangesAsync();

        // Assert
        var updatedProduct = await context.Products.FindAsync(product.Id);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.ProductName.Should().Be("Updated");
        updatedProduct.Price.Should().Be(200);
    }

    [Fact]
    public async Task Delete_RemovesProductSuccessfully()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProductRepository(context);

        var product = new Product 
        { 
            ProductName = "To Delete", 
            Price = 100, 
            Stock = 10, 
            CategoryId = 1,
            Created = DateTime.UtcNow 
        };

        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
        var productId = product.Id;

        // Act
        repository.Delete(product);
        await context.SaveChangesAsync();

        // Assert
        var deletedProduct = await context.Products.FindAsync(productId);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsAllProducts()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProductRepository(context);

        var products = new List<Product>
        {
            new Product 
            { 
                ProductName = "Product1", 
                Price = 100, 
                Stock = 10, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            },
            new Product 
            { 
                ProductName = "Product2", 
                Price = 200, 
                Stock = 20, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            },
            new Product 
            { 
                ProductName = "Product3", 
                Price = 300, 
                Stock = 30, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Act
        var result = repository.GetAll().ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(p => p.ProductName == "Product1");
        result.Should().Contain(p => p.ProductName == "Product2");
        result.Should().Contain(p => p.ProductName == "Product3");
    }

    [Fact]
    public async Task Where_FiltersProductsCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProductRepository(context);

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
            },
            new Product 
            { 
                ProductName = "Keyboard", 
                Price = 150, 
                Stock = 50, 
                CategoryId = 1,
                Created = DateTime.UtcNow 
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.where(p => p.Price > 100).ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.ProductName == "Laptop");
        result.Should().Contain(p => p.ProductName == "Keyboard");
        result.Should().NotContain(p => p.ProductName == "Mouse");
    }

    [Fact]
    public async Task GetTopPriceProductAsync_WithMoreProductsThanCount_ReturnsCorrectNumber()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProductRepository(context);

        var products = Enumerable.Range(1, 10).Select(i => new Product
        {
            ProductName = $"Product{i}",
            Price = i * 100,
            Stock = 10,
            CategoryId = 1,
            Created = DateTime.UtcNow
        }).ToList();

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetTopPriceProductAsync(3);

        // Assert
        result.Should().HaveCount(3);
        result.First().Price.Should().Be(1000); // Highest price
        result.Last().Price.Should().Be(800);   // Third highest
    }
}
