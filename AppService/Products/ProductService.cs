using System.Net;
using AppRepository.Products;
using AppRepository.UnitOfWorks;
using AppService.Products.Create;
using AppService.Products.Update;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppService.Products;

public class ProductService(IProductRepository productRepository,IUnitOfWork unitOfWork, IMapper mapper) : IProductService
{
    public async Task<ServiceResult<List<ProductDto>>> GetTopPriceProductAsync(int count)
    {
        var prodoucts = await productRepository.GetTopPriceProductAsync(count);
        #region ManuelMapping
        // var productDtos = prodoucts.Select(p => new ProductDto(p.ProductId, p.ProductName, p.Price,p.Stock)).ToList();
        #endregion
        var productDtos = mapper.Map<List<ProductDto>>(prodoucts);
        return new ServiceResult<List<ProductDto>>()
        {
            Data = productDtos
        };
    }
    public async Task<ServiceResult<ProductDto?>> GetProductByIdAsync(int productId)
    {
        var product = await productRepository.GetByIdAsync(productId);
        
        if (product is null)
        {
            return ServiceResult<ProductDto>.Faild("Product not found", HttpStatusCode.NotFound);
        }
        #region ManuelMapping
        //var productDtos = new ProductDto(product.ProductId, product.ProductName, product.Price, product.Stock);
        #endregion
        var productDtos = mapper.Map<ProductDto>(product);
        return ServiceResult<ProductDto>.Succes(productDtos)!;
    }
    public async Task<ServiceResult<List<ProductDto>>> GettAllListAsync()
    {
        var products = await productRepository.GetAll().ToListAsync();
        #region ManuelMapping
        // var productDtos = products.Select(p => new ProductDto(p.ProductId, p.ProductName, p.Price, p.Stock)).ToList();
        #endregion
        var productDtos = mapper.Map<List<ProductDto>>(products);
        return ServiceResult<List<ProductDto>>.Succes(productDtos);
    }
    
    public async Task<ServiceResult<List<ProductDto>>> GetAllPageListAsync(int pageIndex, int pageSize)
    {
        //1-10=> ilk 10 kayıt  skip(0).take(10)
        //2-10=> 11-20 kayıt  skip(10).take(10)
        //3-10=> 21-30 kayıt skip(20).take(10)
        var products = await productRepository.GetAll().Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        #region ManuelMapping
        //var productDtos = products.Select(p => new ProductDto(p.ProductId, p.ProductName, p.Price, p.Stock)).ToList();
        #endregion
        var productDtos = mapper.Map<List<ProductDto>>(products);
        return ServiceResult<List<ProductDto>>.Succes(productDtos);
    }

    public async Task<ServiceResult<CreateProductResponse>> CreateProductAsync(CreateProductRequest request)
    {
        var anyProduct = await productRepository.where(p => p.ProductName == request.ProductName).AnyAsync();
        if (anyProduct)
        {
            return ServiceResult<CreateProductResponse>.Faild("ürün ismi veri tabanında bulunmaktadır", HttpStatusCode.BadRequest);
        }
        var product = mapper.Map<Product>(request);
                 //   yada
        //var product = new Product
        //{
        //    ProductName = request.ProductName,
        //    Price = request.Price,
        //    Stock = request.Stock
        //};
        await productRepository.AddAsync(product);
        await unitOfWork.SaveChangesAsync();
        return ServiceResult<CreateProductResponse>.SuccessAsCreated(new CreateProductResponse(product.ProductId),$"api/products/{product.ProductId}");
    }

    public async Task<ServiceResult> UpdateProductAsync(int productId, UpdateProductRequest request)
    {
        // clean code prensipleri
        //Fast fail --> oncelikte olumsuz durumları dön sonra basarılı durumları dön
        //Guard Clauses --> oncelikle olumsuz durumları if ile kontrol ede ede git mümkün olduğunca else yazma
        var product = await productRepository.GetByIdAsync(productId);
        if (product is null)
        {
            return ServiceResult.Faild("Ürün bulunamadı", HttpStatusCode.NotFound);
        }
        var isProductNameExist = await productRepository.where(p => p.ProductName == request.ProductName && p.ProductId!= product.ProductId).AnyAsync();
        if (isProductNameExist)
        {
            return ServiceResult.Faild("ürün ismi veri tabanında bulunmaktadır", HttpStatusCode.BadRequest); // badrequest = hatalı istek biçiminden dolayı geriye dönen error code'dur.
        }
        product = mapper.Map(request, product);
        //product.ProductName = request.ProductName;
        //product.Price = request.Price;
        //product.Stock = request.Stock;
        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync();
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
        return ServiceResult.Succes(HttpStatusCode.NoContent);
    }
    public async Task<ServiceResult> DeleteProductAsync(int productId)
    {
        var product = await productRepository.GetByIdAsync(productId);
        if (product is null)
        {
            return ServiceResult.Faild("Product not found", HttpStatusCode.NotFound);
        }
        productRepository.Delete(product);
        await unitOfWork.SaveChangesAsync();
        return ServiceResult.Succes(HttpStatusCode.NoContent);
    }
}

/*
   Not: service kısımları her zaman önmelidir tüm metodlaarı ve işlmleride bu katmanda yaparız.
Busıness katmanı dışında kullanması lazım. 
Contrellar kısmında ne olursa olsun metod yazma
Controller kısmında sadece request al ve response dön.
 
 */