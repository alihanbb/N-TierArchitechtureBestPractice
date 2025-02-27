using AppService.Products.Create;
using AppService.Products.Update;

namespace AppService.Products
{
    // generic service mümkünse yazma
    public interface IProductService
    {
        Task<ServiceResult<List<ProductDto>>> GetTopPriceProductAsync(int count);
        Task<ServiceResult<ProductDto?>> GetProductByIdAsync(int productId);
        Task<ServiceResult<List<ProductDto>>> GettAllListAsync();
        Task<ServiceResult<List<ProductDto>>> GetAllPageListAsync(int pageIndex, int pageSize);
        Task<ServiceResult<CreateProductResponse>> CreateProductAsync(CreateProductRequest request);
        Task<ServiceResult> UpdateProductAsync(int productId, UpdateProductRequest request);
        Task<ServiceResult> UpdateStockAsync(UpdateStockProductRequest updateStockProductRequest);
        Task<ServiceResult> DeleteProductAsync(int productId);
        
    }
}
