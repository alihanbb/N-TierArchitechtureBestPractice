using System.Linq.Expressions;
using System.Net;
using AppRepository.Products;
using AppRepository.UnitOfWorks;
using AppService;
using AppService.Products;
using AppService.Products.Create;
using AppService.Products.Update;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace UnitTest.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _productService = new ProductService(
            _mockProductRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object
        );
    }

    #region GetTopPriceProductAsync Tests

    [Fact]
    public async Task GetTopPriceProductAsync_ReturnsTopProducts()
    {
        // Arrange
        var count = 3;
        var products = new List<Product>
        {
            new Product { Id = 1, ProductName = "Laptop", Price = 15000, Stock = 10, CategoryId = 1 },
            new Product { Id = 2, ProductName = "Phone", Price = 8000, Stock = 20, CategoryId = 1 },
            new Product { Id = 3, ProductName = "Tablet", Price = 5000, Stock = 15, CategoryId = 1 }
        };
        var productDtos = new List<ProductDto>
        {
            new ProductDto(1, "Laptop", 15000, 10),
            new ProductDto(2, "Phone", 8000, 20),
            new ProductDto(3, "Tablet", 5000, 15)
        };

        _mockProductRepository.Setup(r => r.GetTopPriceProductAsync(count))
            .ReturnsAsync(products);
        _mockMapper.Setup(m => m.Map<List<ProductDto>>(products))
            .Returns(productDtos);

        // Act
        var result = await _productService.GetTopPriceProductAsync(count);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(3);
        result.Data!.First().Price.Should().Be(15000);
        _mockProductRepository.Verify(r => r.GetTopPriceProductAsync(count), Times.Once);
    }

    [Fact]
    public async Task GetTopPriceProductAsync_WithZeroCount_ReturnsEmptyList()
    {
        // Arrange
        var count = 0;
        var products = new List<Product>();
        var productDtos = new List<ProductDto>();

        _mockProductRepository.Setup(r => r.GetTopPriceProductAsync(count))
            .ReturnsAsync(products);
        _mockMapper.Setup(m => m.Map<List<ProductDto>>(products))
            .Returns(productDtos);

        // Act
        var result = await _productService.GetTopPriceProductAsync(count);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    #endregion

    #region GetProductByIdAsync Tests

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product 
        { 
            Id = productId, 
            ProductName = "Laptop", 
            Price = 15000, 
            Stock = 10, 
            CategoryId = 1 
        };
        var productDto = new ProductDto(productId, "Laptop", 15000, 10);

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockMapper.Setup(m => m.Map<ProductDto>(product))
            .Returns(productDto);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ProductName.Should().Be("Laptop");
        result.Data.Price.Should().Be(15000);
        result.Data.Stock.Should().Be(10);
        _mockProductRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999;
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.ErrorMessage.Should().Contain("Product not found");
    }

    #endregion

    #region GettAllListAsync Tests

    [Fact]
    public async Task GettAllListAsync_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, ProductName = "Product1", Price = 100, Stock = 10, CategoryId = 1 },
            new Product { Id = 2, ProductName = "Product2", Price = 200, Stock = 20, CategoryId = 1 }
        };
        var productDtos = new List<ProductDto>
        {
            new ProductDto(1, "Product1", 100, 10),
            new ProductDto(2, "Product2", 200, 20)
        };

        _mockProductRepository.Setup(r => r.GetAll())
            .Returns(products.AsQueryable());
        _mockMapper.Setup(m => m.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
            .Returns(productDtos);

        // Act
        var result = await _productService.GettAllListAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    #endregion

    #region GetAllPageListAsync Tests

    [Fact]
    public async Task GetAllPageListAsync_ReturnsPagedProducts()
    {
        // Arrange
        var pageIndex = 1;
        var pageSize = 2;
        var products = new List<Product>
        {
            new Product { Id = 1, ProductName = "Product1", Price = 100, Stock = 10, CategoryId = 1 },
            new Product { Id = 2, ProductName = "Product2", Price = 200, Stock = 20, CategoryId = 1 },
            new Product { Id = 3, ProductName = "Product3", Price = 300, Stock = 30, CategoryId = 1 }
        };
        var productDtos = new List<ProductDto>
        {
            new ProductDto(1, "Product1", 100, 10),
            new ProductDto(2, "Product2", 200, 20)
        };

        _mockProductRepository.Setup(r => r.GetAll())
            .Returns(products.AsQueryable());
        _mockMapper.Setup(m => m.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
            .Returns(productDtos);

        // Act
        var result = await _productService.GetAllPageListAsync(pageIndex, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllPageListAsync_SecondPage_ReturnsCorrectProducts()
    {
        // Arrange
        var pageIndex = 2;
        var pageSize = 2;
        var products = new List<Product>
        {
            new Product { Id = 1, ProductName = "Product1", Price = 100, Stock = 10, CategoryId = 1 },
            new Product { Id = 2, ProductName = "Product2", Price = 200, Stock = 20, CategoryId = 1 },
            new Product { Id = 3, ProductName = "Product3", Price = 300, Stock = 30, CategoryId = 1 }
        };
        var productDtos = new List<ProductDto>
        {
            new ProductDto(3, "Product3", 300, 30)
        };

        _mockProductRepository.Setup(r => r.GetAll())
            .Returns(products.AsQueryable());
        _mockMapper.Setup(m => m.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
            .Returns(productDtos);

        // Act
        var result = await _productService.GetAllPageListAsync(pageIndex, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    #endregion

    #region CreateProductAsync Tests

    [Fact]
    public async Task CreateProductAsync_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateProductRequest("NewProduct", 500, 50, 1);
        var product = new Product 
        { 
            Id = 5, 
            ProductName = "NewProduct", 
            Price = 500, 
            Stock = 50, 
            CategoryId = 1 
        };

        _mockProductRepository.Setup(r => r.where(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(new List<Product>().AsQueryable());
        _mockMapper.Setup(m => m.Map<Product>(request))
            .Returns(product);
        _mockProductRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Returns(ValueTask.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _productService.CreateProductAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Data.Should().NotBeNull();
        result.Data!.ProductId.Should().Be(5);
        result.Url.Should().Be("api/products/5");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateProductRequest("ExistingProduct", 500, 50, 1);
        var existingProducts = new List<Product>
        {
            new Product { Id = 1, ProductName = "ExistingProduct", Price = 500, Stock = 50, CategoryId = 1 }
        };

        _mockProductRepository.Setup(r => r.where(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(existingProducts.AsQueryable());

        // Act
        var result = await _productService.CreateProductAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.ErrorMessage.Should().Contain("ürün ismi veri tabanýnda bulunmaktadýr");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region UpdateProductAsync Tests

    [Fact]
    public async Task UpdateProductAsync_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var productId = 1;
        var request = new UpdateProductRequest("UpdatedProduct", 600, 60);
        var existingProduct = new Product 
        { 
            Id = productId, 
            ProductName = "OldProduct", 
            Price = 500, 
            Stock = 50, 
            CategoryId = 1 
        };
        var updatedProduct = new Product 
        { 
            Id = productId, 
            ProductName = "UpdatedProduct", 
            Price = 600, 
            Stock = 60, 
            CategoryId = 1 
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        _mockProductRepository.Setup(r => r.where(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(new List<Product>().AsQueryable());
        _mockMapper.Setup(m => m.Map(request, existingProduct))
            .Returns(updatedProduct);
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _productService.UpdateProductAsync(productId, request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999;
        var request = new UpdateProductRequest("UpdatedProduct", 600, 60);

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.UpdateProductAsync(productId, request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.ErrorMessage.Should().Contain("Ürün bulunamadý");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateProductAsync_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var productId = 1;
        var request = new UpdateProductRequest("ExistingProduct", 600, 60);
        var existingProduct = new Product 
        { 
            Id = productId, 
            ProductName = "OldProduct", 
            Price = 500, 
            Stock = 50, 
            CategoryId = 1 
        };
        var duplicateProducts = new List<Product>
        {
            new Product { Id = 2, ProductName = "ExistingProduct", Price = 700, Stock = 70, CategoryId = 1 }
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        _mockProductRepository.Setup(r => r.where(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(duplicateProducts.AsQueryable());

        // Act
        var result = await _productService.UpdateProductAsync(productId, request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.ErrorMessage.Should().Contain("ürün ismi veri tabanýnda bulunmaktadýr");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region UpdateStockAsync Tests

    [Fact]
    public async Task UpdateStockAsync_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var request = new UpdateStockProductRequest(1, 100);
        var product = new Product 
        { 
            Id = 1, 
            ProductName = "Product", 
            Price = 500, 
            Stock = 50, 
            CategoryId = 1 
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(request.ProductId))
            .ReturnsAsync(product);
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _productService.UpdateStockAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        product.Stock.Should().Be(100);
        _mockProductRepository.Verify(r => r.Update(product), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStockAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateStockProductRequest(999, 100);

        _mockProductRepository.Setup(r => r.GetByIdAsync(request.ProductId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.UpdateStockAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.ErrorMessage.Should().Contain("Product not found");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region DeleteProductAsync Tests

    [Fact]
    public async Task DeleteProductAsync_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var productId = 1;
        var product = new Product 
        { 
            Id = productId, 
            ProductName = "Product", 
            Price = 500, 
            Stock = 50, 
            CategoryId = 1 
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockProductRepository.Setup(r => r.Delete(It.IsAny<Product>()));
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _mockProductRepository.Verify(r => r.Delete(product), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999;

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.ErrorMessage.Should().Contain("Product not found");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion
}
