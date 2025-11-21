using System.Net;
using System.Net.Http.Json;
using AppService;
using AppService.Products;
using AppService.Products.Create;
using AppService.Products.Update;
using FluentAssertions;

namespace IntegrationTest.Products;

public class ProductsApiIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public ProductsApiIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        // Her test öncesi veritabanýný yeniden baþlat
        _factory.InitializeDbForTests();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAll_ReturnsAllProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ServiceResult<List<ProductDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;

        // Act
        var response = await _client.GetAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ServiceResult<ProductDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.ProductId.Should().Be(productId);
        result.Data.ProductName.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidProductId = 999;

        // Act
        var response = await _client.GetAsync($"/api/products/{invalidProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllPageList_ReturnsPagedProducts()
    {
        // Arrange
        int pageIndex = 1;
        int pageSize = 2;

        // Act
        var response = await _client.GetAsync($"/api/products/{pageIndex}/{pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ServiceResult<List<ProductDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Count.Should().BeLessThanOrEqualTo(pageSize);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        // Arrange
        var newProduct = new CreateProductRequest(
            ProductName: "TestPrd",  // 3-10 karakter arasý
            Price: 100,
            Stock: 10,
            CategoryId: 1
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/products/create", newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<ServiceResult<CreateProductResponse>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Id.Should().BeGreaterThan(0);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange - Laptop zaten test verilerinde mevcut
        var duplicateProduct = new CreateProductRequest(
            ProductName: "Laptop",
            Price: 100,
            Stock: 10,
            CategoryId: 1
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/products/create", duplicateProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var productId = 2;
        var updateRequest = new UpdateProductRequest(
            ProductName: "NewMouse",  // 3-10 karakter arasý
            Price: 200,
            Stock: 60
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidProductId = 999;
        var updateRequest = new UpdateProductRequest(
            ProductName: "TestUpdate",  // 3-10 karakter arasý
            Price: 200,
            Stock: 60
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{invalidProductId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStock_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var stockUpdateRequest = new UpdateStockProductRequest(
            ProductId: 1,
            Quantity: 25
        );

        // Act
        var response = await _client.PatchAsync("/api/products/stock", 
            JsonContent.Create(stockUpdateRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var productId = 3;

        // Act
        var response = await _client.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify - Ürünün gerçekten silindiðini kontrol et
        var getResponse = await _client.GetAsync($"/api/products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidProductId = 999;

        // Act
        var response = await _client.DeleteAsync($"/api/products/{invalidProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
