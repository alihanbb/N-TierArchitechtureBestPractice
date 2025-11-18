# Integration Tests

Bu proje, katmanlý mimaride yazýlmýþ API'ler için integration testlerini içermektedir.

## Yapý

Integration testler, gerçek HTTP istekleri yaparak API endpoint'lerini test eder. InMemory veritabaný kullanýlarak gerçek veritabaný baðlantýsýna gerek kalmadan testler çalýþtýrýlabilir.

## Kullanýlan Teknolojiler

- **xUnit**: Test framework'ü
- **FluentAssertions**: Daha okunabilir assertion'lar için
- **Microsoft.AspNetCore.Mvc.Testing**: WebApplicationFactory ile API testleri için
- **Microsoft.EntityFrameworkCore.InMemory**: Test için InMemory veritabaný

## Test Yapýsý

### IntegrationTestWebAppFactory.cs
Base factory sýnýfý. API'yi test ortamýnda ayaða kaldýrýr ve:
- SQL Server yerine InMemory veritabaný kullanýr
- Test verileriyle veritabanýný seed eder
- Her test için temiz bir ortam saðlar

### Test Sýnýflarý

#### ProductsApiIntegrationTests
Products API endpoint'lerini test eder:
- ? `GetAll_ReturnsAllProducts` - Tüm ürünleri getirme
- ? `GetById_WithValidId_ReturnsProduct` - ID ile ürün getirme
- ? `GetById_WithInvalidId_ReturnsNotFound` - Geçersiz ID ile 404 kontrolü
- ? `GetAllPageList_ReturnsPagedProducts` - Sayfalama testi
- ? `Create_WithValidData_ReturnsCreated` - Yeni ürün oluþturma
- ? `Create_WithDuplicateName_ReturnsBadRequest` - Duplicate kontrol
- ? `Update_WithValidData_ReturnsNoContent` - Ürün güncelleme
- ? `Update_WithInvalidId_ReturnsNotFound` - Geçersiz ID ile güncelleme
- ? `UpdateStock_WithValidData_ReturnsNoContent` - Stok güncelleme
- ? `Delete_WithValidId_ReturnsNoContent` - Ürün silme
- ? `Delete_WithInvalidId_ReturnsNotFound` - Geçersiz ID ile silme

#### CategoriesApiIntegrationTests
Categories API endpoint'lerini test eder:
- ? `GetCategories_ReturnsAllCategories` - Tüm kategorileri getirme
- ? `GetCategory_WithValidId_ReturnsCategory` - ID ile kategori getirme
- ? `GetCategory_WithInvalidId_ReturnsNotFound` - Geçersiz ID ile 404 kontrolü
- ? `GetCategoryWithProducts_WithValidId_ReturnsCategoryWithProducts` - Ürünleriyle kategori getirme
- ? `GetCategoryByProducts_ReturnsAllCategoriesWithProducts` - Tüm kategorileri ürünleriyle getirme
- ? `CreateCategory_WithValidData_ReturnsSuccess` - Yeni kategori oluþturma
- ? `CreateCategory_WithDuplicateName_ReturnsBadRequest` - Duplicate kontrol
- ? `UpdateCategory_WithValidData_ReturnsNoContent` - Kategori güncelleme
- ? `UpdateCategory_WithInvalidId_ReturnsNotFound` - Geçersiz ID ile güncelleme
- ? `DeleteCategory_WithValidId_ReturnsNoContent` - Kategori silme
- ? `DeleteCategory_WithInvalidId_ReturnsNotFound` - Geçersiz ID ile silme

## Testleri Çalýþtýrma

### Visual Studio'dan
1. Test Explorer'ý açýn (Test > Test Explorer)
2. "Run All Tests" butonuna týklayýn

### Komut satýrýndan
```bash
cd IntegrationTest
dotnet test
```

### Sadece belirli bir test sýnýfýný çalýþtýrma
```bash
dotnet test --filter "FullyQualifiedName~ProductsApiIntegrationTests"
```

## Test Verileri

IntegrationTestWebAppFactory içinde seed edilen test verileri:

### Kategoriler
- CategoryId: 1, CategoryName: "Elektronik"
- CategoryId: 2, CategoryName: "Giyim"
- CategoryId: 3, CategoryName: "Kitap"

### Ürünler
- ProductId: 1, ProductName: "Laptop", Price: 15000, Stock: 10, CategoryId: 1
- ProductId: 2, ProductName: "Mouse", Price: 150, Stock: 50, CategoryId: 1
- ProductId: 3, ProductName: "T-Shirt", Price: 200, Stock: 100, CategoryId: 2

## Önemli Notlar

1. **InMemory Veritabaný**: Testler InMemory veritabaný kullanýr, bu nedenle gerçek veritabanýna ihtiyaç yoktur.

2. **Test Isolation**: Her test sýnýfý için yeni bir WebApplicationFactory instance'ý oluþturulur, bu sayede testler birbirinden izole çalýþýr.

3. **Seed Data**: Her factory oluþturulduðunda test verileri otomatik olarak seed edilir.

4. **HTTP Client**: `_client` field'ý gerçek HTTP istekleri yapar ve API'yi tam olarak test eder.

5. **FluentAssertions**: Daha okunabilir test assertion'larý için kullanýlýr:
   ```csharp
   response.StatusCode.Should().Be(HttpStatusCode.OK);
   result.Data.Should().NotBeNull();
   ```

## Best Practices

1. ? Her test metodu tek bir senaryoyu test eder (Single Responsibility)
2. ? Test metodlarý açýklayýcý isimlendirmeye sahiptir: `Method_Scenario_ExpectedResult`
3. ? AAA pattern kullanýlýr: Arrange, Act, Assert
4. ? Hem pozitif hem negatif senaryolar test edilir
5. ? HTTP status code'larý kontrol edilir
6. ? Response body içeriði doðrulanýr

## Gelecek Ýyileþtirmeler

- [ ] Authentication/Authorization testleri eklenebilir
- [ ] Performance testleri eklenebilir
- [ ] Daha fazla edge case test edilebilir
- [ ] Test data builder pattern kullanýlabilir
- [ ] Custom assertions oluþturulabilir
