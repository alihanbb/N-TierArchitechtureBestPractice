using AppRepository.Products;
using AppRepository.UnitOfWorks;
using AppService.Products.Create;
using AppService.Products.Update;
using AppService.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;

namespace AppService.Products;

public class ProductService(
    IProductRepository productRepository, 
    IUnitOfWork unitOfWork, 
    IMapper mapper, 
    ILogger<ProductService> logger,
    ICacheService cacheService) : IProductService
{
    private const string ProductCacheKeyPrefix = "product:";
    private const string AllProductsCacheKey = "products:all";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);
    public async Task<ServiceResult<List<ProductDto>>> GetTopPriceProductAsync(int count)
    {
        logger.LogInformation("En yüksek fiyatlı {Count} ürün getiriliyor...", count);
        var prodoucts = await productRepository.GetTopPriceProductAsync(count);
        logger.LogInformation("{ProductCount} adet ürün başarıyla getirildi.", prodoucts.Count);
        
        var productDtos = mapper.Map<List<ProductDto>>(prodoucts);
        return new ServiceResult<List<ProductDto>>()
        {
            Data = productDtos
        };
    }
    public async Task<ServiceResult<ProductDto?>> GetProductByIdAsync(int productId)
    {
        logger.LogInformation("ID'si {ProductId} olan ürün getiriliyor...", productId);
        
        // Cache-Aside Pattern: Try to get from cache first
        var cacheKey = $"{ProductCacheKeyPrefix}{productId}";
        var cachedProduct = await cacheService.GetAsync<ProductDto>(cacheKey);
        
        if (cachedProduct != null)
        {
            logger.LogInformation("Ürün cache'ten getirildi: {ProductName}", cachedProduct.ProductName);
            return ServiceResult<ProductDto?>.Succes(cachedProduct)!;
        }
        
        // Cache miss - get from database
        var product = await productRepository.GetByIdAsync(productId);
        
        if (product is null)
        {
            logger.LogWarning("ID'si {ProductId} olan ürün bulunamadı.", productId);
            return ServiceResult<ProductDto>.Faild("Product not found", HttpStatusCode.NotFound);
        }
       
        logger.LogInformation("Ürün veritabanından getirildi ve cache'e ekleniyor: {ProductName}", product.ProductName);
        var productDtos = mapper.Map<ProductDto>(product);
        
        // Populate cache for next requests (Write-through)
        await cacheService.SetAsync(cacheKey, productDtos, CacheExpiration);
        
        return ServiceResult<ProductDto?>.Succes(productDtos)!;
    }
    public async Task<ServiceResult<List<ProductDto>>> GettAllListAsync()
    {
        // Try to get from cache first
        var cachedProducts = await cacheService.GetAsync<List<ProductDto>>(AllProductsCacheKey);
        
        if (cachedProducts != null)
        {
            logger.LogInformation("{ProductCount} adet ürün cache'ten getirildi.", cachedProducts.Count);
            return ServiceResult<List<ProductDto>>.Succes(cachedProducts);
        }
        
        // Cache miss - get from database
        var products = await productRepository.GetAll().ToListAsync();
       
        var productDtos = mapper.Map<List<ProductDto>>(products);
        
        // Populate cache
        await cacheService.SetAsync(AllProductsCacheKey, productDtos, CacheExpiration);
        logger.LogInformation("{ProductCount} adet ürün veritabanından getirildi ve cache'e eklendi.", productDtos.Count);
        
        return ServiceResult<List<ProductDto>>.Succes(productDtos);
    }
    
    public async Task<ServiceResult<List<ProductDto>>> GetAllPageListAsync(int pageIndex, int pageSize)
    {
        //1-10=> ilk 10 kayıt  skip(0).take(10)
        //2-10=> 11-20 kayıt  skip(10).take(10)
        //3-10=> 21-30 kayıt skip(20).take(10)
        var products = await productRepository.GetAll().Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        
        var productDtos = mapper.Map<List<ProductDto>>(products);
        return ServiceResult<List<ProductDto>>.Succes(productDtos);
    }

    public async Task<ServiceResult<CreateProductResponse>> CreateProductAsync(CreateProductRequest request)
    {
        logger.LogInformation("Yeni ürün oluşturuluyor: {ProductName}", request.ProductName);
        var anyProduct = await productRepository.where(p => p.ProductName == request.ProductName).AnyAsync();
        if (anyProduct)
        {
            logger.LogWarning("Ürün oluşturulamadı. '{ProductName}' isimli ürün zaten mevcut.", request.ProductName);
            return ServiceResult<CreateProductResponse>.Faild("ürün ismi veri tabanında bulunmaktadır", HttpStatusCode.BadRequest);
        }
        var product = mapper.Map<Product>(request);
       
        await productRepository.AddAsync(product);
        await unitOfWork.SaveChangesAsync();
        
        // Invalidate cache after creating new product
        await cacheService.RemoveAsync(AllProductsCacheKey);
        logger.LogInformation("Ürün başarıyla oluşturuldu ve cache temizlendi. ID: {ProductId}, İsim: {ProductName}", product.Id, product.ProductName);
        
        return ServiceResult<CreateProductResponse>.SuccessAsCreated(new CreateProductResponse(product.Id),$"api/products/{product.Id}");
    }

    public async Task<ServiceResult> UpdateProductAsync(int productId, UpdateProductRequest request)
    {
        logger.LogInformation("ID'si {ProductId} olan ürün güncelleniyor...", productId);
        // clean code prensipleri
        //Fast fail --> oncelikte olumsuz durumları dön sonra basarılı durumları dön
        //Guard Clauses --> oncelikle olumsuz durumları if ile kontrol ede ede git mümkün olduğunca else yazma
        
        var product = await productRepository.GetByIdAsync(productId);
        if (product is null)
        {
            logger.LogWarning("Ürün güncellenemedi. ID'si {ProductId} olan ürün bulunamadı.", productId);
            return ServiceResult.Faild("Ürün bulunamadı", HttpStatusCode.NotFound);
        }

        var isProductNameExist = await productRepository.where(p => p.ProductName == request.ProductName && p.Id != productId).AnyAsync();
        if (isProductNameExist)
        {
            logger.LogWarning("Ürün güncellenemedi. '{ProductName}' isimli başka bir ürün zaten mevcut.", request.ProductName);
            return ServiceResult.Faild("ürün ismi veri tabanında bulunmaktadır", HttpStatusCode.BadRequest); // badrequest = hatalı istek biçiminden dolayı geriye dönen error code'dur.
        }
        
        mapper.Map(request, product);
        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync();
        
        // Invalidate cache after updating product
        var cacheKey = $"{ProductCacheKeyPrefix}{productId}";
        await cacheService.RemoveAsync(cacheKey);
        await cacheService.RemoveAsync(AllProductsCacheKey);
        logger.LogInformation("Ürün başarıyla güncellendi ve cache temizlendi. ID: {ProductId}, Yeni İsim: {ProductName}", productId, product.ProductName);
        
        return ServiceResult.Succes(HttpStatusCode.NoContent);
    }
    public async Task<ServiceResult> UpdateStockAsync(UpdateStockProductRequest updateStockProductRequest)// 2'den faznla parametre varsa request objesi oluştur
    {
        var product = await productRepository.GetByIdAsync(updateStockProductRequest.ProductId);
        if (product is null)
        {
            return ServiceResult.Faild("Product not found", HttpStatusCode.NotFound);
        }
        product.Stock = updateStockProductRequest.Quantity;
        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync();
        
        // Invalidate cache after stock update
        var cacheKey = $"{ProductCacheKeyPrefix}{updateStockProductRequest.ProductId}";
        await cacheService.RemoveAsync(cacheKey);
        await cacheService.RemoveAsync(AllProductsCacheKey);
        
        return ServiceResult.Succes(HttpStatusCode.NoContent);
    }
    public async Task<ServiceResult> DeleteProductAsync(int productId)
    {
        logger.LogInformation("ID'si {ProductId} olan ürün siliniyor...", productId);
        var product = await productRepository.GetByIdAsync(productId);
        if (product is null)
        {
            logger.LogWarning("Ürün silinemedi. ID'si {ProductId} olan ürün bulunamadı.", productId);
            return ServiceResult.Faild("Product not found", HttpStatusCode.NotFound);
        }
        
        var productName = product.ProductName;
        productRepository.Delete(product);
        await unitOfWork.SaveChangesAsync();
        
        // Invalidate cache after deleting product
        var cacheKey = $"{ProductCacheKeyPrefix}{productId}";
        await cacheService.RemoveAsync(cacheKey);
        await cacheService.RemoveAsync(AllProductsCacheKey);
        logger.LogInformation("Ürün başarıyla silindi ve cache temizlendi. ID: {ProductId}, İsim: {ProductName}", productId, productName);
        
        return ServiceResult.Succes(HttpStatusCode.NoContent);
    }
}
