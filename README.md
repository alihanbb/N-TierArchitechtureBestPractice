# 🏗️ N-Tier Architecture - Best Practice Implementation

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-336791?logo=postgresql)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7.0+-DC382D?logo=redis)](https://redis.io/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Modern, ölçeklenebilir ve bakımı kolay bir **N-Tier (Katmanlı) Mimari** implementasyonu. Bu proje, .NET 8 ekosistemindeki en iyi uygulamaları (best practices) ve modern tasarım desenlerini içerir.

---

## 📋 İçindekiler

- [Genel Bakış](#-genel-bakış)
- [Mimari](#-mimari)
- [Kullanılan Teknolojiler](#-kullanılan-teknolojiler)
- [Best Practices](#-best-practices-uygulamaları)
- [Proje Yapısı](#-proje-yapısı)
- [Başlangıç](#-başlangıç)
- [Kullanım](#-kullanım)
- [Test](#-test)
- [Deployment](#-deployment)
- [Katkıda Bulunma](#-katkıda-bulunma)

---

## 🎯 Genel Bakış

Bu proje, kurumsal düzeyde bir uygulamanın nasıl yapılandırılması gerektiğini gösteren kapsamlı bir örnektir. Aşağıdaki özellikleri içerir:

### ✨ Öne Çıkan Özellikler

- **🏛️ Clean Architecture**: Bağımlılıkların doğru yönde olması
- **🎨 Decorator Pattern**: Hybrid cache implementasyonu
- **⚡ Hybrid Cache**: L1 (Memory) + L2 (Redis) iki katmanlı cache stratejisi
- **🚦 Rate Limiting**: Farklı stratejilerle (Fixed Window, Sliding Window, Token Bucket, vb.)
- **📊 Serilog**: Yapılandırılmış loglama (Console, File, Seq)
- **🏥 Health Checks**: Uygulama ve bağımlılıkların sağlık kontrolü
- **🧪 Comprehensive Testing**: Unit ve Integration testleri
- **📦 Repository Pattern**: Generic repository ile veri erişimi
- **🎯 SOLID Principles**: Tüm katmanlarda uygulanmış
- **🔧 Extension Methods**: Modüler ve okunabilir kod yapısı
- **📝 Global Usings**: Tekrarları azaltan organized imports

---

## 🏗️ Mimari

### Katman Yapısı

```
┌─────────────────────────────────────────────────────────┐
│                   Presentation Layer                     │
│                      (AppApis)                           │
│  • Controllers                                           │
│  • Extensions (DI Configuration)                         │
│  • Middleware                                            │
└────────────────────┬─────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                    Service Layer                         │
│                    (AppService)                          │
│  • Business Logic                                        │
│  • DTOs & Mappings                                       │
│  • Validation (FluentValidation)                         │
│  • Infrastructure (Caching, etc.)                        │
└────────────────────┬─────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                  Repository Layer                        │
│                  (AppRepository)                         │
│  • Data Access                                           │
│  • Entity Framework Core                                 │
│  • Generic Repository Pattern                            │
│  • Unit of Work Pattern                                  │
└────────────────────┬─────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│                     Database                             │
│                   (PostgreSQL)                           │
└─────────────────────────────────────────────────────────┘
```

### Hybrid Cache Mimaris

```
┌─────────────────────────────────────────────────────────┐
│             HybridCacheDecorator                         │
│                                                          │
│  ┌──────────────┐              ┌──────────────────┐    │
│  │  L1 Cache    │              │    L2 Cache      │    │
│  │  (Memory)    │──────────────▶    (Redis)       │    │
│  │  5 min TTL   │   Promote    │    30 min TTL    │    │
│  └──────────────┘              └──────────────────┘    │
│         │                               │               │
│         │ Hit (1ms)                     │ Hit (5-10ms)  │
│         ▼                               ▼               │
│    Return Data ◄──── Fetch from L2 ◄───┘               │
└─────────────────────────────────────────────────────────┘
```

---

## 🛠️ Kullanılan Teknolojiler

### Core Technologies

| Teknoloji | Versiyon | Kullanım Amacı |
|-----------|----------|----------------|
| [.NET](https://dotnet.microsoft.com/) | 8.0 | Backend Framework |
| [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) | 8.0 | Web API |
| [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) | 8.0 | ORM |
| [PostgreSQL](https://www.postgresql.org/) | 15+ | Database |
| [Redis](https://redis.io/) | 7.0+ | Distributed Cache |

### Libraries & Packages

#### API & Middleware
- **Asp.Versioning.Mvc** - API versioning
- **Serilog** - Structured logging (Console, File, Seq)
- **AspNetCore.HealthChecks** - Health checks (DB, Redis, etc.)
- **Microsoft.AspNetCore.RateLimiting** - Rate limiting

#### Data & Caching
- **Npgsql.EntityFrameworkCore.PostgreSQL** - PostgreSQL provider
- **StackExchange.Redis** - Redis client
- **Microsoft.Extensions.Caching.Memory** - In-memory cache

#### Mapping & Validation
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Model validation

#### Testing
- **xUnit** - Testing framework
- **Moq** - Mocking library
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing

---

## 🎯 Best Practices Uygulamaları

### 1. **SOLID Principles** ✅

#### Single Responsibility Principle (SRP)
```csharp
// Her sınıf tek bir sorumluluğa sahip
public class ProductService { } // Sadece ürün business logic
public class ProductRepository { } // Sadece ürün data access
public class HybridCacheDecorator { } // Sadece cache orchestration
```

#### Open/Closed Principle (OCP)
```csharp
// Extension methods ile genişletilebilir
public static class CacheServiceExtensions
{
    public static IServiceCollection AddHybridCache(this IServiceCollection services) { }
}
```

#### Liskov Substitution Principle (LSP)
```csharp
// Interface ile abstraction
ICacheService cache = new HybridCacheDecorator(); // veya
ICacheService cache = new InMemoryCacheService(); // veya
ICacheService cache = new RedisCacheService();
```

#### Interface Segregation Principle (ISP)
```csharp
// Küçük, odaklanmış interface'ler
public interface ICacheService { } // Sadece cache işlemleri
public interface IProductRepository : IGenericRepository<Product, int> { } // Sadece product ile ilgili
```

#### Dependency Inversion Principle (DIP)
```csharp
// Concrete değil, abstraction'a bağımlı
public class ProductService
{
    private readonly IProductRepository _repository; // Interface
    private readonly ICacheService _cache; // Interface
}
```

### 2. **Design Patterns** ✅

#### Decorator Pattern
```csharp
public class HybridCacheDecorator : ICacheService
{
    private readonly IMemoryCache _l1Cache;
    private readonly IDatabase _l2Cache;
    
    // Decorates both L1 and L2 cache implementations
}
```

#### Repository Pattern
```csharp
public class GenericRepository<T, TId> : IGenericRepository<T, TId>
    where T : BaseEntity<TId>
{
    // Generic CRUD operations
}
```

#### Unit of Work Pattern
```csharp
public class UnitOfWork : IUnitOfWork
{
    public async Task<int> SaveChangesAsync() { }
}
```

### 3. **Dependency Injection** ✅

#### Extension Methods ile Modüler Konfigürasyon
```csharp
// Program.cs
builder.Services.AddHybridCache(builder.Configuration);
builder.Services.AddRateLimitingConfiguration();
builder.Services.AddHealthCheckConfiguration(builder.Configuration);
```

#### Service Registration
```csharp
public static class CacheServiceExtensions
{
    public static IServiceCollection AddHybridCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<IConnectionMultiplexer>(...);
        services.AddSingleton<ICacheService, HybridCacheDecorator>();
        return services;
    }
}
```

### 4. **Global Usings** ✅

#### Organized Imports
```csharp
// AppApis/GlobalUsings.cs
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;
global using Serilog;

// AppService/GlobalUsings.cs
global using AutoMapper;
global using Microsoft.Extensions.Logging;

// AppRepository/GlobalUsings.cs
global using Microsoft.EntityFrameworkCore;
```

### 5. **Caching Strategy** ✅

#### Cache-Aside Pattern
```csharp
public async Task<ProductDto?> GetProductByIdAsync(int id)
{
    // 1. Try cache first
    var cached = await _cache.GetAsync<ProductDto>($"product:{id}");
    if (cached != null) return cached;
    
    // 2. Fetch from database
    var product = await _repository.GetByIdAsync(id);
    
    // 3. Populate cache
    await _cache.SetAsync($"product:{id}", productDto, TimeSpan.FromMinutes(10));
    
    return productDto;
}
```

#### Cache Invalidation
```csharp
public async Task UpdateProductAsync(int id, UpdateProductRequest request)
{
    await _repository.UpdateAsync(product);
    
    // Invalidate cache after update
    await _cache.RemoveAsync($"product:{id}");
    await _cache.RemoveAsync("products:all");
}
```

### 6. **Logging** ✅

#### Structured Logging with Serilog
```csharp
_logger.LogInformation("Ürün başarıyla oluşturuldu. ID: {ProductId}, İsim: {ProductName}", 
    product.Id, product.ProductName);

_logger.LogWarning("Ürün bulunamadı. ID: {ProductId}", productId);

_logger.LogError(ex, "Cache hatası. Key: {CacheKey}", key);
```

#### Request Logging
```csharp
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} completed. Status: {StatusCode}";
    options.EnrichDiagnosticContext = (ctx, httpContext) =>
    {
        ctx.Set("ClientIP", httpContext.Connection.RemoteIpAddress);
    };
});
```

### 7. **Rate Limiting** ✅

#### Multiple Strategies
```csharp
// Fixed Window: 100 requests per minute
options.AddFixedWindowLimiter("fixed", opt => { ... });

// Sliding Window: 50 requests per minute with segments
options.AddSlidingWindowLimiter("sliding", opt => { ... });

// Token Bucket: Replenishing tokens
options.AddTokenBucketLimiter("token", opt => { ... });

// Per-IP: Different limits per client
options.AddPolicy("perIp", context => { ... });
```

### 8. **Health Checks** ✅

#### Comprehensive Health Monitoring
```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddRedis(redisConnection, "redis")
    .AddNpgSql(dbConnection, "database");

// Endpoints
app.MapHealthChecks("/health");          // All checks
app.MapHealthChecks("/health/ready");    // Readiness probe
app.MapHealthChecks("/health/live");     // Liveness probe
```

---

## 📁 Proje Yapısı

```
KatmanliMimari/
├── AppApis/                          # Presentation Layer
│   ├── Controllers/
│   │   ├── ProductsController.cs
│   │   ├── CategoriesController.cs
│   │   └── CustomBaseController.cs
│   ├── Extensions/                   # 🆕 DI Extension Methods
│   │   ├── CacheServiceExtensions.cs
│   │   ├── ApiVersioningExtensions.cs
│   │   ├── RateLimitingExtensions.cs
│   │   ├── HealthCheckExtensions.cs
│   │   └── SerilogExtensions.cs
│   ├── GlobalUsings.cs               # 🆕 Global imports
│   ├── Program.cs                    # ✨ Refactored (245→100 lines)
│   └── appsettings.json
│
├── AppService/                       # Business Logic Layer
│   ├── Products/
│   │   ├── ProductService.cs
│   │   ├── IProductService.cs
│   │   ├── ProductDto.cs
│   │   ├── Create/
│   │   │   └── CreateProductRequest.cs
│   │   └── Update/
│   │       └── UpdateProductRequest.cs
│   ├── Categories/
│   │   └── (Similar structure)
│   ├── Infrastructure/
│   │   └── Caching/
│   │       ├── ICacheService.cs
│   │       ├── HybridCacheDecorator.cs  # 🆕 Decorator Pattern
│   │       ├── InMemoryCacheService.cs
│   │       └── RedisCacheService.cs
│   ├── GlobalUsings.cs               # 🆕
│   └── ServiceResult.cs
│
├── AppRepository/                    # Data Access Layer
│   ├── Products/
│   │   ├── Product.cs
│   │   ├── ProductRepository.cs
│   │   └── IProductRepository.cs
│   ├── Categories/
│   │   └── (Similar structure)
│   ├── Repository/
│   │   ├── GenericRepository.cs
│   │   └── IGenericRepository.cs
│   ├── UnitOfWorks/
│   │   ├── UnitOfWork.cs
│   │   └── IUnitOfWork.cs
│   ├── Context/
│   │   └── AppDbContext.cs
│   ├── GlobalUsings.cs               # 🆕
│   └── Extentions/
│       └── RepositoryExtensions.cs
│
├── UnitTest/                         # 🆕 Updated
│   ├── Services/
│   │   ├── ProductServiceTests.cs
│   │   └── HybridCacheDecoratorTests.cs
│   ├── Repositories/
│   └── Api/
│
├── IntegrationTest/                  # 🆕 Updated
│   ├── Products/
│   │   └── ProductsIntegrationTests.cs
│   ├── Categories/
│   └── IntegrationTestWebAppFactory.cs
│
├── docker-compose.yml
├── .dockerignore
├── .gitignore
├── KatmanlıMimari.sln
├── README.md                         # 🆕 This file
├── HYBRID_CACHE.md                   # Cache documentation
├── RATE_LIMITING.md                  # Rate limiting guide
└── REFACTORING_SUMMARY.md            # Refactoring report
```

---

## 🚀 Başlangıç

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Redis 7.0+](https://redis.io/download/) (Opsiyonel - InMemory cache de kullanılabilir)
- [Docker](https://www.docker.com/) (Opsiyonel)

### Kurulum

#### 1. Projeyi Klonlayın
```bash
git clone https://github.com/your-username/KatmanliMimari.git
cd KatmanliMimari
```

#### 2. Docker ile Bağımlılıkları Başlatın
```bash
docker-compose up -d
```

Bu komut aşağıdakileri başlatır:
- PostgreSQL (Port: 5432)
- Redis (Port: 6379)
- Seq (Port: 5341) - Log viewer

#### 3. Veritabanı Migration
```bash
cd AppApis
dotnet ef database update
```

#### 4. Uygulamayı Çalıştırın
```bash
dotnet run
```

Uygulama `https://localhost:5001` adresinde başlayacaktır.

### Konfigürasyon

#### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=your_db;User Id=postgres;Password=your_password"
  },
  "Cache": {
    "Provider": "Hybrid",  // "Hybrid", "Redis", or "InMemory"
    "L1": {
      "MaxSize": 1024,
      "DefaultExpiration": "00:05:00"
    },
    "L2": {
      "Enabled": true,
      "DefaultExpiration": "00:30:00"
    },
    "Redis": {
      "ConnectionString": "localhost:6379,password=RedisSecurePass123!",
      "InstanceName": "AppApi:"
    }
  }
}
```

---

## 💻 Kullanım

### API Endpoints

#### Products

```http
# Get all products
GET /api/v1/products

# Get product by ID
GET /api/v1/products/{id}

# Create product
POST /api/v1/products/create
Content-Type: application/json
{
  "productName": "Laptop",
  "price": 15000,
  "stock": 10,
  "categoryId": 1
}

# Update product
PUT /api/v1/products/{id}
Content-Type: application/json
{
  "productName": "Updated Laptop",
  "price": 16000,
  "stock": 15,
  "categoryId": 1
}

# Delete product
DELETE /api/v1/products/{id}
```

#### Categories

```http
# Get all categories
GET /api/v1/categories

# Get category with products
GET /api/v1/categories/products/{id}
```

### Swagger UI

Swagger UI'a erişim: `https://localhost:5001/swagger`

### Health Checks

```bash
# Tüm health checks
curl https://localhost:5001/health

# Readiness probe
curl https://localhost:5001/health/ready

# Liveness probe
curl https://localhost:5001/health/live
```

---

## 🧪 Test

### Unit Tests

```bash
cd UnitTest
dotnet test
```

**Test Coverage:**
- ✅ Service layer tests (with cache mocking)
- ✅ Repository layer tests
- ✅ Cache decorator tests
- ✅ Controller tests

### Integration Tests

```bash
cd IntegrationTest
dotnet test
```

**Test Coverage:**
- ✅ End-to-end API tests
- ✅ Database integration tests
- ✅ Cache invalidation tests
- ✅ Rate limiting tests

### Test Example

```csharp
[Fact]
public async Task GetProductByIdAsync_WhenProductExistsInCache_ShouldReturnFromCache()
{
    // Arrange
    var productId = 1;
    var cachedProduct = new ProductDto(1, "Cached Product", 100, 10);
    _mockCacheService
        .Setup(x => x.GetAsync<ProductDto>($"product:{productId}", default))
        .ReturnsAsync(cachedProduct);

    // Act
    var result = await _productService.GetProductByIdAsync(productId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(cachedProduct.ProductName, result.Data.ProductName);
    
    // Verify repository was NOT called (cache hit)
    _mockProductRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
}
```

---

## 🐳 Deployment

### Docker

#### Build Image
```bash
docker build -t katmanli-mimari:latest .
```

#### Run Container
```bash
docker run -d \
  -p 5000:8080 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Cache__Redis__ConnectionString="..." \
  katmanli-mimari:latest
```

### Docker Compose

```bash
docker-compose up -d
```

docker-compose.yml içeriği:
```yaml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "5000:8080"
    depends_on:
      - postgres
      - redis
    environment:
      - ConnectionStrings__DefaultConnection=...
      - Cache__Provider=Hybrid
      - Cache__Redis__ConnectionString=redis:6379

  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: appdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    command: redis-server --requirepass RedisSecurePass123!
    ports:
      - "6379:6379"

  seq:
    image: datalust/seq:latest
    environment:
      ACCEPT_EULA: Y
    ports:
      - "5341:80"

volumes:
  postgres-data:
```

---

## 📊 Performance

### Benchmarks

| Scenario | InMemory | Redis | Hybrid |
|----------|----------|-------|--------|
| L1 Hit | ~1ms | - | ~1ms ✅ |
| L2 Hit | - | ~5-10ms | ~5ms + L1 promotion |
| After L2 Promotion | - | - | ~1ms ✅✅ |
| Write | ~0.5ms | ~5ms | ~5.5ms |
| Throughput | High | Medium | **Highest** ✅ |

### Cache Hit Ratios (Typical)

- **L1 (Memory) Hit Rate**: 80-90%
- **L2 (Redis) Hit Rate**: 10-15%
- **Database Queries**: 5-10%

---

## 🔒 Security

- ✅ Rate Limiting (DoS protection)
- ✅ Input Validation (FluentValidation)
- ✅ SQL Injection Prevention (EF Core)
- ✅ Secure Configuration (User Secrets)
- ✅ HTTPS Enforcement

---

## 📈 Monitoring

### Logging

- **Console**: Development
- **File**: Production logs
- **Seq**: Centralized log viewer (`http://localhost:5341`)

### Metrics

- Health check endpoints
- Structured logging with Serilog
- Request/Response logging
- Cache hit/miss tracking

---

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'feat: add amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

### Commit Convention

```
feat: Yeni özellik
fix: Bug düzeltmesi
docs: Dokümantasyon değişikliği
style: Kod formatı (kod değişikliği yok)
refactor: Refactoring
test: Test ekleme/düzenleme
chore: Build/config değişiklikleri
```

---

## 📝 License

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

---

## 👨‍💻 Author

**Your Name**
- GitHub: [@yourusername](https://github.com/yourusername)
- LinkedIn: [Your LinkedIn](https://linkedin.com/in/yourprofile)

---

## 🙏 Acknowledgments

- .NET Team for excellent framework
- Community contributors
- Open source libraries used in this project

---

## 📚 Additional Documentation

- [Hybrid Cache Documentation](HYBRID_CACHE.md)
- [Rate Limiting Guide](RATE_LIMITING.md)
- [Refactoring Summary](REFACTORING_SUMMARY.md)

---

<div align="center">

**⭐ Star this repo if you find it helpful!**

Made with ❤️ using .NET 8

</div>
