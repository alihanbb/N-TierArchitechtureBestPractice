using System.Linq.Expressions;
using System.Net;
using AppRepository.Categories;
using AppRepository.UnitOfWorks;
using AppService.Categories;
using AppService.Categories.Create;
using AppService.Categories.Dto;
using AppService.Categories.Update;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace UnitTest.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _categoryService = new CategoryService(
            _mockCategoryRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object
        );
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsSuccessWithCategoryId()
    {
        // Arrange
        var request = new CreateCategoryRequest("New Category");
        var category = new Category 
        { 
            Id = 5, 
            CategoryName = "New Category",
            Created = DateTime.UtcNow
        };

        _mockCategoryRepository.Setup(r => r.where(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(new List<Category>().AsQueryable());
        _mockMapper.Setup(m => m.Map<Category>(request))
            .Returns(category);
        _mockCategoryRepository.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Returns(ValueTask.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _categoryService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(5);
        _mockCategoryRepository.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsFailure()
    {
        // Arrange
        var request = new CreateCategoryRequest("Existing Category");
        var existingCategories = new List<Category>
        {
            new Category { Id = 1, CategoryName = "Existing Category", Created = DateTime.UtcNow }
        };

        _mockCategoryRepository.Setup(r => r.where(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(existingCategories.AsQueryable());

        // Act
        var result = await _categoryService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.ErrorMessage.Should().ContainSingle()
            .Which.Should().Be("Category already exists");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsFailure()
    {
        // Arrange
        var request = new CreateCategoryRequest("");

        _mockCategoryRepository.Setup(r => r.where(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(new List<Category>().AsQueryable());

        // Act
        var result = await _categoryService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        _mockCategoryRepository.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var request = new UpdateCategoryRequest(1, "Updated Category");
        var existingCategory = new Category 
        { 
            Id = 1, 
            CategoryName = "Old Category",
            Created = DateTime.UtcNow
        };
        var updatedCategory = new Category 
        { 
            Id = 1, 
            CategoryName = "Updated Category",
            Created = DateTime.UtcNow
        };

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(request.CategoryId))
            .ReturnsAsync(existingCategory);
        _mockCategoryRepository.Setup(r => r.where(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(new List<Category>().AsQueryable());
        _mockMapper.Setup(m => m.Map(request, existingCategory))
            .Returns(updatedCategory);
        _mockCategoryRepository.Setup(r => r.Update(It.IsAny<Category>()));
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _categoryService.UpdateAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _mockCategoryRepository.Verify(r => r.Update(It.IsAny<Category>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateCategoryRequest(999, "Updated Category");

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(request.CategoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.UpdateAsync(999, request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.ErrorMessage.Should().ContainSingle()
            .Which.Should().Be("Category not found");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ReturnsFailure()
    {
        // Arrange
        var request = new UpdateCategoryRequest(1, "Existing Category");
        var existingCategory = new Category 
        { 
            Id = 1, 
            CategoryName = "Old Category",
            Created = DateTime.UtcNow
        };
        var duplicateCategories = new List<Category>
        {
            new Category { Id = 2, CategoryName = "Existing Category", Created = DateTime.UtcNow }
        };

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(request.CategoryId))
            .ReturnsAsync(existingCategory);
        _mockCategoryRepository.Setup(r => r.where(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(duplicateCategories.AsQueryable());

        // Act
        var result = await _categoryService.UpdateAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.ErrorMessage.Should().ContainSingle()
            .Which.Should().Be("Category already exists");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var categoryId = 1;
        var category = new Category 
        { 
            Id = categoryId, 
            CategoryName = "Category",
            Created = DateTime.UtcNow
        };

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(category);
        _mockCategoryRepository.Setup(r => r.Delete(It.IsAny<Category>()));
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _categoryService.DeleteAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _mockCategoryRepository.Verify(r => r.Delete(category), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var categoryId = 999;

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.DeleteAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.ErrorMessage.Should().ContainSingle()
            .Which.Should().Be("Category not found");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region GetAllListAsync Tests

    [Fact]
    public async Task GetAllListAsync_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = 1, CategoryName = "Category1", Created = DateTime.UtcNow },
            new Category { Id = 2, CategoryName = "Category2", Created = DateTime.UtcNow }
        };
        var categoryDtos = new List<CategoryDto>
        {
            new CategoryDto(1, "Category1"),
            new CategoryDto(2, "Category2")
        };

        _mockCategoryRepository.Setup(r => r.GetAll())
            .Returns(categories.AsQueryable());
        _mockMapper.Setup(m => m.Map<List<CategoryDto>>(It.IsAny<List<Category>>()))
            .Returns(categoryDtos);

        // Act
        var result = await _categoryService.GetAllListAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data!.First().CategoryName.Should().Be("Category1");
    }

    [Fact]
    public async Task GetAllListAsync_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var categories = new List<Category>();
        var categoryDtos = new List<CategoryDto>();

        _mockCategoryRepository.Setup(r => r.GetAll())
            .Returns(categories.AsQueryable());
        _mockMapper.Setup(m => m.Map<List<CategoryDto>>(It.IsAny<List<Category>>()))
            .Returns(categoryDtos);

        // Act
        var result = await _categoryService.GetAllListAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsCategory()
    {
        // Arrange
        var categoryId = 1;
        var category = new Category 
        { 
            Id = categoryId, 
            CategoryName = "Category",
            Created = DateTime.UtcNow
        };
        var categoryDto = new CategoryDto(categoryId, "Category");

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(category);
        _mockMapper.Setup(m => m.Map<CategoryDto>(category))
            .Returns(categoryDto);

        // Act
        var result = await _categoryService.GetByIdAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CategoryId.Should().Be(categoryId);
        result.Data.CategoryName.Should().Be("Category");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var categoryId = 999;

        _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.GetByIdAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.ErrorMessage.Should().ContainSingle()
            .Which.Should().Be("Category not found");
    }

    #endregion

    #region GetCategoryWithProductsAsync Tests

    [Fact]
    public async Task GetCategoryWithProductsAsync_WithValidId_ReturnsCategory()
    {
        // Arrange
        var categoryId = 1;
        var category = new Category 
        { 
            Id = categoryId, 
            CategoryName = "Electronics",
            Created = DateTime.UtcNow,
            Products = new List<AppRepository.Products.Product>
            {
                new AppRepository.Products.Product 
                { 
                    Id = 1, 
                    ProductName = "Laptop", 
                    Price = 1000, 
                    Stock = 10,
                    CategoryId = categoryId,
                    Created = DateTime.UtcNow
                }
            }
        };
        var categoryDto = new CategoryDto(categoryId, "Electronics");

        _mockCategoryRepository.Setup(r => r.GetCategoryWithProductsAsync(categoryId))
            .ReturnsAsync(category);
        _mockMapper.Setup(m => m.Map<CategoryDto>(category))
            .Returns(categoryDto);

        // Act
        var result = await _categoryService.GetCategoryWithProductsAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task GetCategoryWithProductsAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var categoryId = 999;

        _mockCategoryRepository.Setup(r => r.GetCategoryWithProductsAsync(categoryId))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.GetCategoryWithProductsAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.ErrorMessage.Should().ContainSingle()
            .Which.Should().Be("Category not found");
    }

    #endregion

    #region GetCategoryByProductsAsync Tests

    [Fact]
    public async Task GetCategoryByProductsAsync_ReturnsCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category 
            { 
                Id = 1, 
                CategoryName = "Electronics",
                Created = DateTime.UtcNow,
                Products = new List<AppRepository.Products.Product>()
            },
            new Category 
            { 
                Id = 2, 
                CategoryName = "Books",
                Created = DateTime.UtcNow,
                Products = new List<AppRepository.Products.Product>()
            }
        };
        var categoryDtos = new List<CategoryDto>
        {
            new CategoryDto(1, "Electronics"),
            new CategoryDto(2, "Books")
        };

        _mockCategoryRepository.Setup(r => r.GetCategoryByProductsAsync())
            .Returns(categories.AsQueryable());
        _mockMapper.Setup(m => m.Map<List<CategoryDto>>(It.IsAny<List<Category>>()))
            .Returns(categoryDtos);

        // Act
        var result = await _categoryService.GetCategoryByProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCategoryByProductsAsync_WithNoCategories_ReturnsEmptyList()
    {
        // Arrange
        var categories = new List<Category>();
        var categoryDtos = new List<CategoryDto>();

        _mockCategoryRepository.Setup(r => r.GetCategoryByProductsAsync())
            .Returns(categories.AsQueryable());
        _mockMapper.Setup(m => m.Map<List<CategoryDto>>(It.IsAny<List<Category>>()))
            .Returns(categoryDtos);

        // Act
        var result = await _categoryService.GetCategoryByProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    #endregion
}
