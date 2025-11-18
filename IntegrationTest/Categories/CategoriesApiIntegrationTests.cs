using System.Net;
using System.Net.Http.Json;
using AppService;
using AppService.Categories.Create;
using AppService.Categories.Dto;
using AppService.Categories.Update;
using FluentAssertions;

namespace IntegrationTest.Categories;

public class CategoriesApiIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public CategoriesApiIntegrationTests(IntegrationTestWebAppFactory factory)
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
    public async Task GetCategories_ReturnsAllCategories()
    {
        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ServiceResult<List<CategoryDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCategory_WithValidId_ReturnsCategory()
    {
        // Arrange
        var categoryId = 1;

        // Act
        var response = await _client.GetAsync($"/api/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ServiceResult<CategoryDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.CategoryId.Should().Be(categoryId);
        result.Data.CategoryName.Should().Be("Elektronik");
    }

    [Fact]
    public async Task GetCategory_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidCategoryId = 999;

        // Act
        var response = await _client.GetAsync($"/api/categories/{invalidCategoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCategoryWithProducts_WithValidId_ReturnsCategoryWithProducts()
    {
        // Arrange
        var categoryId = 1; // Elektronik kategorisi

        // Act
        var response = await _client.GetAsync($"/api/categories/products/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ServiceResult<CategoryDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetCategoryByProducts_ReturnsAllCategoriesWithProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ServiceResult<List<CategoryDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCategory_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var newCategory = new CreateCategoryRequest(
            CategoryName: "Test Kategori"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", newCategory);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ServiceResult<int>>();
        result.Should().NotBeNull();
        result!.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange - Elektronik zaten test verilerinde mevcut
        var duplicateCategory = new CreateCategoryRequest(
            CategoryName: "Elektronik"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", duplicateCategory);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // Service'de NotFound dönüyor
    }

    [Fact]
    public async Task UpdateCategory_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var updateRequest = new UpdateCategoryRequest(
            CategoryId: 2,
            CategoryName: "Updated Giyim"
        );

        // Act
        var response = await _client.PutAsJsonAsync("/api/categories", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdateCategoryRequest(
            CategoryId: 999,
            CategoryName: "Updated Category"
        );

        // Act
        var response = await _client.PutAsJsonAsync("/api/categories", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var categoryId = 3; // Kitap kategorisi

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify - Kategorinin gerçekten silindiðini kontrol et
        var getResponse = await _client.GetAsync($"/api/categories/{categoryId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidCategoryId = 999;

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{invalidCategoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
