using System.Net;
using AppApis.Controllers;
using AppService;
using AppService.Products;
using AppService.Products.Create;
using AppService.Products.Update;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTest.Api
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _controller = new ProductsController(_mockProductService.Object);
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfProducts()
        {
            // Arrange
            var products = new List<ProductDto>
            {
                new ProductDto(1, "Product 1", 100, 10),
                new ProductDto(2, "Product 2", 200, 20)
            };
            var serviceResult = ServiceResult<List<ProductDto>>.Succes(products);
            _mockProductService.Setup(s => s.GettAllListAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<List<ProductDto>>>(objectResult.Value);
            Assert.Equal(2, returnedResult.Data.Count);
            _mockProductService.Verify(s => s.GettAllListAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithEmptyList()
        {
            // Arrange
            var serviceResult = ServiceResult<List<ProductDto>>.Succes(new List<ProductDto>());
            _mockProductService.Setup(s => s.GettAllListAsync())
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<List<ProductDto>>>(objectResult.Value);
            Assert.Empty(returnedResult.Data);
        }

        #endregion

        #region GetAllPageList Tests

        [Fact]
        public async Task GetAllPageList_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            int pageIndex = 1;
            int pageSize = 10;
            var products = new List<ProductDto>
            {
                new ProductDto(1, "Product 1", 100, 10),
                new ProductDto(2, "Product 2", 200, 20)
            };
            var serviceResult = ServiceResult<List<ProductDto>>.Succes(products);
            _mockProductService.Setup(s => s.GetAllPageListAsync(pageIndex, pageSize))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetAllPageList(pageIndex, pageSize);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<List<ProductDto>>>(objectResult.Value);
            Assert.Equal(2, returnedResult.Data.Count);
            _mockProductService.Verify(s => s.GetAllPageListAsync(pageIndex, pageSize), Times.Once);
        }

        [Fact]
        public async Task GetAllPageList_WithDifferentPageSize_ReturnsOkResult()
        {
            // Arrange
            int pageIndex = 2;
            int pageSize = 5;
            var products = new List<ProductDto>
            {
                new ProductDto(6, "Product 6", 600, 60)
            };
            var serviceResult = ServiceResult<List<ProductDto>>.Succes(products);
            _mockProductService.Setup(s => s.GetAllPageListAsync(pageIndex, pageSize))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetAllPageList(pageIndex, pageSize);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            _mockProductService.Verify(s => s.GetAllPageListAsync(pageIndex, pageSize), Times.Once);
        }

        [Fact]
        public async Task GetAllPageList_WithEmptyPage_ReturnsOkResultWithEmptyList()
        {
            // Arrange
            int pageIndex = 100;
            int pageSize = 10;
            var serviceResult = ServiceResult<List<ProductDto>>.Succes(new List<ProductDto>());
            _mockProductService.Setup(s => s.GetAllPageListAsync(pageIndex, pageSize))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetAllPageList(pageIndex, pageSize);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<List<ProductDto>>>(objectResult.Value);
            Assert.Empty(returnedResult.Data);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var productId = 1;
            var product = new ProductDto(productId, "Product 1", 100, 10);
            var serviceResult = ServiceResult<ProductDto>.Succes(product);
            _mockProductService.Setup(s => s.GetProductByIdAsync(productId))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetById(productId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<ProductDto>>(objectResult.Value);
            Assert.Equal(productId, returnedResult.Data.ProductId);
            Assert.Equal("Product 1", returnedResult.Data.ProductName);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var productId = 999;
            var serviceResult = ServiceResult<ProductDto>.Faild("Product not found", HttpStatusCode.NotFound);
            _mockProductService.Setup(s => s.GetProductByIdAsync(productId))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetById(productId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<ProductDto>>(objectResult.Value);
            Assert.False(returnedResult.IsSuccess);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_WithValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var request = new CreateProductRequest("New Product", 150.00m, 50, 1);
            var response = new CreateProductResponse(1);
            var serviceResult = ServiceResult<CreateProductResponse>.SuccessAsCreated(response, "api/products/1");
            _mockProductService.Setup(s => s.CreateProductAsync(request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal("api/products/1", createdResult.Location);
            var returnedResult = Assert.IsType<ServiceResult<CreateProductResponse>>(createdResult.Value);
            Assert.True(returnedResult.IsSuccess);
            Assert.Equal(1, returnedResult.Data.ProductId);
        }

        [Fact]
        public async Task Create_WithExistingProductName_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateProductRequest("Existing Product", 150.00m, 50, 1);
            var serviceResult = ServiceResult<CreateProductResponse>.Faild("ürün ismi veri tabanýnda bulunmaktadýr", HttpStatusCode.BadRequest);
            _mockProductService.Setup(s => s.CreateProductAsync(request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
            var returnedResult = Assert.IsType<ServiceResult<CreateProductResponse>>(objectResult.Value);
            Assert.False(returnedResult.IsSuccess);
        }

        [Fact]
        public async Task Create_WithValidComplexRequest_ReturnsCreatedResult()
        {
            // Arrange
            var request = new CreateProductRequest("Laptop", 2500.00m, 25, 1);
            var response = new CreateProductResponse(5);
            var serviceResult = ServiceResult<CreateProductResponse>.SuccessAsCreated(response, "api/products/5");
            _mockProductService.Setup(s => s.CreateProductAsync(request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal("api/products/5", createdResult.Location);
            _mockProductService.Verify(s => s.CreateProductAsync(request), Times.Once);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidRequest_ReturnsNoContent()
        {
            // Arrange
            var productId = 1;
            var request = new UpdateProductRequest("Updated Product", 200.00m, 30);
            var serviceResult = ServiceResult.Succes(HttpStatusCode.NoContent);
            _mockProductService.Setup(s => s.UpdateProductAsync(productId, request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Update(productId, request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NoContent, objectResult.StatusCode);
            _mockProductService.Verify(s => s.UpdateProductAsync(productId, request), Times.Once);
        }

        [Fact]
        public async Task Update_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var productId = 999;
            var request = new UpdateProductRequest("Updated Product", 200.00m, 30);
            var serviceResult = ServiceResult.Faild("Ürün bulunamadý", HttpStatusCode.NotFound);
            _mockProductService.Setup(s => s.UpdateProductAsync(productId, request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Update(productId, request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task Update_WithExistingProductName_ReturnsBadRequest()
        {
            // Arrange
            var productId = 1;
            var request = new UpdateProductRequest("Existing Product", 200.00m, 30);
            var serviceResult = ServiceResult.Faild("ürün ismi veri tabanýnda bulunmaktadýr", HttpStatusCode.BadRequest);
            _mockProductService.Setup(s => s.UpdateProductAsync(productId, request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Update(productId, request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
        }

        #endregion

        #region UpdateStockPatch Tests

        [Fact]
        public async Task UpdateStockPatch_WithValidRequest_ReturnsNoContent()
        {
            // Arrange
            var request = new UpdateStockProductRequest(1, 100);
            var serviceResult = ServiceResult.Succes(HttpStatusCode.NoContent);
            _mockProductService.Setup(s => s.UpdateStockAsync(request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.UpdateStockPatch(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NoContent, objectResult.StatusCode);
            _mockProductService.Verify(s => s.UpdateStockAsync(request), Times.Once);
        }

        [Fact]
        public async Task UpdateStockPatch_WithInvalidProductId_ReturnsNotFound()
        {
            // Arrange
            var request = new UpdateStockProductRequest(999, 100);
            var serviceResult = ServiceResult.Faild("Product not found", HttpStatusCode.NotFound);
            _mockProductService.Setup(s => s.UpdateStockAsync(request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.UpdateStockPatch(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdateStockPatch_WithZeroQuantity_ReturnsNoContent()
        {
            // Arrange
            var request = new UpdateStockProductRequest(1, 0);
            var serviceResult = ServiceResult.Succes(HttpStatusCode.NoContent);
            _mockProductService.Setup(s => s.UpdateStockAsync(request))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.UpdateStockPatch(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NoContent, objectResult.StatusCode);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var productId = 1;
            var serviceResult = ServiceResult.Succes(HttpStatusCode.NoContent);
            _mockProductService.Setup(s => s.DeleteProductAsync(productId))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Delete(productId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NoContent, objectResult.StatusCode);
            _mockProductService.Verify(s => s.DeleteProductAsync(productId), Times.Once);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var productId = 999;
            var serviceResult = ServiceResult.Faild("Product not found", HttpStatusCode.NotFound);
            _mockProductService.Setup(s => s.DeleteProductAsync(productId))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Delete(productId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task Delete_MultipleDeletesWithSameId_CallsServiceMultipleTimes()
        {
            // Arrange
            var productId = 1;
            var serviceResult = ServiceResult.Succes(HttpStatusCode.NoContent);
            _mockProductService.Setup(s => s.DeleteProductAsync(productId))
                .ReturnsAsync(serviceResult);

            // Act
            await _controller.Delete(productId);
            await _controller.Delete(productId);

            // Assert
            _mockProductService.Verify(s => s.DeleteProductAsync(productId), Times.Exactly(2));
        }

        #endregion
    }
}
