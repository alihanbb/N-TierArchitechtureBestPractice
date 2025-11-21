using System.Net;
using System.Net.Http.Json;
using AppService.Products;
using AppService.Products.Create;
using AppService.Products.Update;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTest.Products;

public class ProductsIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public ProductsIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProduct_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = 1;

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{productId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"data\":", content);
    }

    [Fact]
    public async Task GetProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
       var nonExistentId = 99999;

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreated_WhenProductIsValid()
    {
        // Arrange
        var request = new CreateProductRequest(
            ProductName: $"Test Product {Guid.NewGuid()}",
            Price: 100,
            Stock: 10,
            CategoryId: 1
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products/create", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnNoContent_WhenProductIsUpdated()
    {
        // Arrange - First create a product
        var createRequest = new CreateProductRequest(
            ProductName: $"Product to Update {Guid.NewGuid()}",
            Price: 100,
            Stock: 10,
            CategoryId: 1
        );

        var createResponse = await _client.PostAsJsonAsync("/api/v1/products/create", request);
        createResponse.EnsureSuccessStatusCode();

        var createResult = await createResponse.Content.ReadFromJsonAsync<ServiceResult<CreateProductResponse>>();
        var productId = createResult?.Data?.Id ?? 0;

        var updateRequest = new UpdateProductRequest(
            ProductName: "Updated Product Name",
            Price: 150,
            Stock: 20,
            CategoryId: 1
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/products/{productId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnNoContent_WhenProductIsDeleted()
    {
        // Arrange - First create a product
        var createRequest = new CreateProductRequest(
            ProductName: $"Product to Delete {Guid.NewGuid()}",
            Price: 100,
            Stock: 10,
            CategoryId: 1
        );

        var createResponse = await _client.PostAsJsonAsync("/api/v1/products/create", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var createResult = await createResponse.Content.ReadFromJsonAsync<ServiceResult<CreateProductResponse>>();
        var productId = createResult?.Data?.Id ?? 0;

        // Act
        var response = await _client.DeleteAsync($"/api/v1/products/{productId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify product is deleted
        var getResponse = await _client.GetAsync($"/api/v1/products/{productId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"data\":", content);
    }

    [Fact]
    public async Task RateLimiting_ShouldReturnTooManyRequests_WhenLimitExceeded()
    {
        // Arrange - Make multiple requests to trigger rate limiting
        var tasks = new List<Task<HttpResponse>>();
        
        for (int i = 0; i < 150; i++) // Exceeds the fixed window limit of 100
        {
            tasks.Add(_client.GetAsync("/api/v1/products"));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        var tooManyRequestsResponses = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        Assert.True(tooManyRequestsResponses > 0, "Expected some requests to be rate limited");
    }

    [Fact]
    public async Task CacheInvalidation_ProductUpdate_ShouldInvalidateCache()
    {
        // Arrange - Create a product
        var createRequest = new CreateProductRequest(
            ProductName: $"Cache Test Product {Guid.NewGuid()}",
            Price: 100,
            Stock: 10,
            CategoryId: 1
        );

        var createResponse = await _client.PostAsJsonAsync("/api/v1/products/create", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ServiceResult<CreateProductResponse>>();
        var productId = createResult?.Data?.Id ?? 0;

        // Act 1: Get product (cache it)
        var firstGet = await _client.GetAsync($"/api/v1/products/{productId}");
        firstGet.EnsureSuccessStatusCode();

        // Act 2: Update product (should invalidate cache)
        var updateRequest = new UpdateProductRequest(
            ProductName: "Updated for Cache Test",
            Price: 200,
            Stock: 30,
            CategoryId: 1
        );
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/products/{productId}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        // Act 3: Get product again (should fetch from DB with new data)
        var secondGet = await _client.GetAsync($"/api/v1/products/{productId}");
        var result = await secondGet.Content.ReadFromJsonAsync<ServiceResult<ProductDto>>();

        // Assert
        Assert.NotNull(result?.Data);
        Assert.Equal("Updated for Cache Test", result.Data.ProductName);
        Assert.Equal(200, result.Data.Price);
    }
}

// Helper record for deserialization
public record CreateProductResponse(int Id);
public record ServiceResult<T>(T? Data, bool IsSuccess, int StatusCode);
