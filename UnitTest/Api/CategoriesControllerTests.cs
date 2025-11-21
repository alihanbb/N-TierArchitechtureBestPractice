using System.Net;
using AppApi.Controllers;
using AppService;
using AppService.Categories;
using AppService.Categories.Create;
using AppService.Categories.Dto;
using AppService.Categories.Update;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTest.Api
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new CategoriesController(_mockCategoryService.Object);
        }

        #region GetCategories Tests

        [Fact]
        public async Task GetCategories_ReturnsOkResult_WithListOfCategories()
        {
            // Arrange
            var categories = new List<CategoryDto>
            {
                new CategoryDto(1, "Electronics"),
                new CategoryDto(2, "Books")
            };
            var serviceResult = ServiceResult<List<CategoryDto>>.Succes(categories);
            _mockCategoryService.Setup(s => s.GetAllListAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetCategories();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<List<CategoryDto>>>(objectResult.Value);
            Assert.Equal(2, returnedResult.Data.Count);
            _mockCategoryService.Verify(s => s.GetAllListAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCategories_ReturnsOkResult_WithEmptyList()
        {
            // Arrange
            var serviceResult = ServiceResult<List<CategoryDto>>.Succes(new List<CategoryDto>());
            _mockCategoryService.Setup(s => s.GetAllListAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetCategories();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<List<CategoryDto>>>(objectResult.Value);
            Assert.Empty(returnedResult.Data);
        }

        #endregion

        #region GetCategory Tests

        [Fact]
        public async Task GetCategory_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var categoryId = 1;
            var category = new CategoryDto(categoryId, "Electronics");
            var serviceResult = ServiceResult<CategoryDto>.Succes(category);
            _mockCategoryService.Setup(s => s.GetByIdAsync(categoryId))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetCategory(categoryId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<CategoryDto>>(objectResult.Value);
            Assert.Equal(categoryId, returnedResult.Data.CategoryId);
            Assert.Equal("Electronics", returnedResult.Data.CategoryName);
        }

        [Fact]
        public async Task GetCategory_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var categoryId = 999;
            var serviceResult = ServiceResult<CategoryDto>.Faild("Category not found", HttpStatusCode.NotFound);
            _mockCategoryService.Setup(s => s.GetByIdAsync(categoryId))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetCategory(categoryId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<CategoryDto>>(objectResult.Value);
            Assert.False(returnedResult.IsSuccess);
        }

        #endregion

        #region GetCategoryWithProducts Tests

        [Fact]
        public async Task GetCategoryWithProducts_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var categoryId = 1;
            var category = new CategoryDto(categoryId, "Electronics");
            var serviceResult = ServiceResult<CategoryDto>.Succes(category);
            _mockCategoryService.Setup(s => s.GetCategoryWithProductsAsync(categoryId))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetCategoryWithProducts(categoryId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<CategoryDto>>(objectResult.Value);
            Assert.Equal(categoryId, returnedResult.Data.CategoryId);
        }

        [Fact]
        public async Task GetCategoryWithProducts_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var categoryId = 999;
            var serviceResult = ServiceResult<CategoryDto>.Faild("Category not found", HttpStatusCode.NotFound);
            _mockCategoryService.Setup(s => s.GetCategoryWithProductsAsync(categoryId))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetCategoryWithProducts(categoryId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }

        #endregion

        #region CreateCategory Tests

        [Fact]
        public async Task CreateCategory_WithValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var request = new CreateCategoryRequest("New Category");
            var serviceResult = ServiceResult<int>.Succes(1);
            _mockCategoryService.Setup(s => s.CreateAsync(request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.CreateCategory(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<int>>(objectResult.Value);
            Assert.True(returnedResult.IsSuccess);
            Assert.Equal(1, returnedResult.Data);
        }

        [Fact]
        public async Task CreateCategory_WithExistingName_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateCategoryRequest("Existing Category");
            var serviceResult = ServiceResult<int>.Faild("Category already exists", HttpStatusCode.NotFound);
            _mockCategoryService.Setup(s => s.CreateAsync(request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.CreateCategory(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<int>>(objectResult.Value);
            Assert.False(returnedResult.IsSuccess);
        }

        #endregion

        #region UpdateCategory Tests

        [Fact]
        public async Task UpdateCategory_WithValidRequest_ReturnsNoContent()
        {
            // Arrange
            var request = new UpdateCategoryRequest(1, "Updated Category");
            var serviceResult = ServiceResult.Succes(HttpStatusCode.NoContent);
            _mockCategoryService.Setup(s => s.UpdateAsync(1,request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.UpdateCategory(1,request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NoContent, objectResult.StatusCode);
            _mockCategoryService.Verify(s => s.UpdateAsync(1,request), Times.Once);
        }

        [Fact]
        public async Task UpdateCategory_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var request = new UpdateCategoryRequest(999, "Updated Category");
            var serviceResult = ServiceResult.Faild("Category not found", HttpStatusCode.NotFound);
            _mockCategoryService.Setup(s => s.UpdateAsync(999,request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.UpdateCategory(999,request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCategory_WithExistingName_ReturnsNotFound()
        {
            // Arrange
            var request = new UpdateCategoryRequest(1, "Existing Category");
            var serviceResult = ServiceResult.Faild("Category already exists", HttpStatusCode.NotFound);
            _mockCategoryService.Setup(s => s.UpdateAsync(1,request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.UpdateCategory(1,request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }

        #endregion

        #region DeleteCategory Tests

        [Fact]
        public async Task DeleteCategory_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var categoryId = 1;
            var serviceResult = ServiceResult.Succes(HttpStatusCode.NoContent);
            _mockCategoryService.Setup(s => s.DeleteAsync(categoryId))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.DeleteCategory(categoryId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NoContent, objectResult.StatusCode);
            _mockCategoryService.Verify(s => s.DeleteAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task DeleteCategory_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var categoryId = 999;
            var serviceResult = ServiceResult.Faild("Category not found", HttpStatusCode.NotFound);
            _mockCategoryService.Setup(s => s.DeleteAsync(categoryId))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.DeleteCategory(categoryId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }

        #endregion

        #region GetCategoryByProducts Tests

        [Fact]
        public async Task GetCategoryByProducts_ReturnsOkResult_WithListOfCategories()
        {
            // Arrange
            var categories = new List<CategoryDto>
            {
                new CategoryDto(1, "Electronics"),
                new CategoryDto(2, "Books")
            };
            var serviceResult = ServiceResult<List<CategoryDto>>.Succes(categories);
            _mockCategoryService.Setup(s => s.GetCategoryByProductsAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetCategoryByProducts();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<List<CategoryDto>>>(objectResult.Value);
            Assert.Equal(2, returnedResult.Data.Count);
            _mockCategoryService.Verify(s => s.GetCategoryByProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCategoryByProducts_ReturnsOkResult_WithEmptyList()
        {
            // Arrange
            var serviceResult = ServiceResult<List<CategoryDto>>.Succes(new List<CategoryDto>());
            _mockCategoryService.Setup(s => s.GetCategoryByProductsAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetCategoryByProducts();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<List<CategoryDto>>>(objectResult.Value);
            Assert.Empty(returnedResult.Data);
        }

        #endregion
    }
}
