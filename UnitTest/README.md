# Unit Tests

Bu proje, N-Tier (Katmanlý) mimarideki AppService ve AppRepository katmanlarý için unit testleri içerir.

## ?? Proje Yapýsý

```
UnitTest/
??? Api/                              # Controller testleri
?   ??? ProductsControllerTests.cs    # Products API controller testleri
?   ??? CategoriesControllerTests.cs  # Categories API controller testleri
??? Services/                         # Service layer testleri  
?   ??? ProductServiceTests.cs        # ProductService testleri
?   ??? CategoryServiceTests.cs       # CategoryService testleri
??? Repositories/                     # Repository layer testleri
    ??? ProductRepositoryTests.cs     # ProductRepository testleri
    ??? CategoryRepositoryTests.cs    # CategoryRepository testleri
```

## ?? Kullanýlan Teknolojiler

- **xUnit** - Test framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion kütüphanesi
- **Entity Framework Core InMemory** - Repository testleri için
- **MockQueryable.Moq** - IQueryable mock desteði

## ? Test Kapsamý

### Controller Layer Testleri (34 test)
- ? ProductsController: 20 test
- ? CategoriesController: 14 test

### Service Layer Testleri (Yeni Eklendi)
- ? ProductService testleri:
  - GetTopPriceProductAsync
  - GetProductByIdAsync
  - GettAllListAsync
  - GetAllPageListAsync
  - CreateProductAsync
  - UpdateProductAsync
  - UpdateStockAsync
  - DeleteProductAsync

- ? CategoryService testleri:
  - CreateAsync
  - UpdateAsync
  - DeleteAsync
  - GetAllListAsync
  - GetByIdAsync
  - GetCategoryWithProductsAsync
  - GetCategoryByProductsAsync

### Repository Layer Testleri (Yeni Eklendi)
- ? ProductRepository testleri:
  - GetTopPriceProductAsync
  - GetByIdAsync
  - AddAsync
  - Update
  - Delete
  - GetAll
  - Where (LINQ filtering)

- ? CategoryRepository testleri:
  - GetCategoryWithProductsAsync
  - GetCategoryByProductsAsync
  - GetByIdAsync
  - AddAsync
  - Update
  - Delete
  - GetAll
  - Where (LINQ filtering)

## ?? Testleri Çalýþtýrma

### Tüm testleri çalýþtýr
```bash
dotnet test UnitTest/UnitTest.csproj
```

### Belirli bir test sýnýfýný çalýþtýr
```bash
# Service testleri
dotnet test --filter "FullyQualifiedName~ProductServiceTests"
dotnet test --filter "FullyQualifiedName~CategoryServiceTests"

# Repository testleri
dotnet test --filter "FullyQualifiedName~ProductRepositoryTests"
dotnet test --filter "FullyQualifiedName~CategoryRepositoryTests"

# Controller testleri
dotnet test --filter "FullyQualifiedName~ProductsControllerTests"
dotnet test --filter "FullyQualifiedName~CategoriesControllerTests"
```

### Test sonuçlarýný detaylý göster
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Code coverage ile çalýþtýr
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## ?? Test Yazma Rehberi

### Controller Testleri
Controller testleri, service layer'ý mock'layarak HTTP response'larý test eder:

```csharp
[Fact]
public async Task GetAll_ReturnsOkResult_WithListOfProducts()
{
    // Arrange
    var products = new List<ProductDto> { ... };
    var serviceResult = ServiceResult<List<ProductDto>>.Succes(products);
    _mockProductService.Setup(s => s.GettAllListAsync())
        .ReturnsAsync(serviceResult);

    // Act
    var result = await _controller.GetAll();

    // Assert
    var objectResult = Assert.IsType<ObjectResult>(result);
    Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
}
```

### Service Testleri
Service testleri, repository ve mapping katmanlarýný mock'layarak business logic'i test eder:

```csharp
[Fact]
public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
{
    // Arrange
    var product = new Product { ProductId = 1, ... };
    var productDto = new ProductDto(1, ...);
    
    _mockProductRepository.Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(product);
    _mockMapper.Setup(m => m.Map<ProductDto>(product))
        .Returns(productDto);

    // Act
    var result = await _productService.GetProductByIdAsync(1);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Data.ProductId.Should().Be(1);
}
```

### Repository Testleri
Repository testleri, InMemory database kullanarak gerçek database operasyonlarýný test eder:

```csharp
[Fact]
public async Task AddAsync_AddsProductSuccessfully()
{
    // Arrange
    using var context = CreateContext();
    var repository = new ProductRepository(context);
    var product = new Product { ProductName = "Test", ... };

    // Act
    await repository.AddAsync(product);
    await context.SaveChangesAsync();

    // Assert
    var savedProduct = await context.Products
        .FirstOrDefaultAsync(p => p.ProductName == "Test");
    savedProduct.Should().NotBeNull();
}
```

## ?? Test Prensipleri

### AAA Pattern
Tüm testler **Arrange-Act-Assert** pattern'ini takip eder:
- **Arrange**: Test verilerini hazýrla
- **Act**: Test edilecek metodu çaðýr
- **Assert**: Sonuçlarý doðrula

### Test Ýsimlendirme
Test metod isimleri açýklayýcý ve tutarlýdýr:
```
MethodName_Scenario_ExpectedResult
```

Örnek:
- `GetProductByIdAsync_WithValidId_ReturnsProduct`
- `CreateAsync_WithDuplicateName_ReturnsFailure`
- `Delete_WithInvalidId_ReturnsNotFound`

### Mocking Strategy
- Controller testlerinde **Service layer mock'lanýr**
- Service testlerinde **Repository ve Mapper mock'lanýr**
- Repository testlerinde **InMemory Database kullanýlýr** (mock yok)

### Test Isolation
- Her test baðýmsýzdýr
- Testler birbirini etkilemez
- Repository testlerinde her test için yeni DbContext oluþturulur

## ?? Test Coverage

```
Controllers:  ???????????????????????????? 100%
Services:     ????????????????????????????  ~80%
Repositories: ????????????????????????????  ~75%
```

## ?? Troubleshooting

### Test baþarýsýz oluyor
```bash
# Detaylý log ile çalýþtýr
dotnet test --logger "console;verbosity=detailed"

# Sadece baþarýsýz testleri çalýþtýr
dotnet test --filter "TestCategory!=Integration"
```

### Mock hatalarý
```csharp
// Moq setup'ý doðru yapýldýðýndan emin olun
_mockRepository.Setup(x => x.Method(It.IsAny<Type>()))
    .ReturnsAsync(expectedResult);

// Verify kullanýmý
_mockRepository.Verify(x => x.Method(It.IsAny<Type>()), Times.Once);
```

### InMemory Database hatalarý
```csharp
// Her test için unique database kullanýn
var options = new DbContextOptionsBuilder<AppDbContextcs>()
    .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
    .Options;
```

## ?? Best Practices

1. ? **Her metod için en az 2 test yaz**: Positive ve negative senaryolar
2. ? **Edge case'leri test et**: Null, empty, boundary deðerler
3. ? **Mock verify kullan**: Metodlarýn çaðrýldýðýný doðrula
4. ? **Açýklayýcý assertion mesajlarý**: Hata durumunda anlaþýlýr olsun
5. ? **Test data builder kullan**: Tekrar eden test verilerini merkezi olarak yönet
6. ? **Async/await doðru kullan**: Async metodlarý düzgün test et
7. ? **Clean up yap**: using statements ile resource'larý düzgün temizle

## ?? CI/CD Integration

```yaml
# GitHub Actions örneði
- name: Run Unit Tests
  run: dotnet test UnitTest/UnitTest.csproj --no-build --verbosity normal

- name: Generate Coverage Report
  run: dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## ?? Ek Kaynaklar

- [xUnit Documentation](https://xunit.net/)
- [Moq Quick Start](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

## ?? Katkýda Bulunma

Yeni test eklerken:
1. Mevcut test pattern'lerini takip edin
2. AAA pattern kullanýn
3. Açýklayýcý test isimleri kullanýn
4. Hem positive hem negative senaryolar ekleyin
5. Mock'larý düzgün verify edin

---

**Not:** Bu testler sürekli geliþtirilmektedir. Yeni feature'lar eklendikçe testler de güncellenmektedir.
