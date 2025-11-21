using AppRepository.Products;
using AppService.Filters;
using AppService.Products;
using AppService.Products.Create;
using AppService.Products.Update;
using Microsoft.AspNetCore.RateLimiting;

namespace AppApis.Controllers
{
    [EnableRateLimiting("fixed")]
    public class ProductsController(IProductService productService, ILogger<ProductsController> logger) : CustomBaseController
    {
        [HttpGet]
        [EnableRateLimiting("sliding")]
        public async Task<IActionResult> GetAll()
        {
            logger.LogInformation("Tüm ürünler listeleniyor...");
            return CreateActionResult(await productService.GettAllListAsync());
        }
        [HttpGet("{pageIndex:int}/{pageSize:int}")]
        public async Task<IActionResult> GetAllPageList(int pageIndex, int pageSize)
        {
            return CreateActionResult(await productService.GetAllPageListAsync(pageIndex, pageSize));
        }
        [HttpGet("{productId:int}")]
        public async Task<IActionResult> GetById(int productId)
        {
            logger.LogInformation("Ürün detayı getiriliyor. ID: {ProductId}", productId);
            return CreateActionResult(await productService.GetProductByIdAsync(productId));
        }

        [HttpPost("create")]
        [EnableRateLimiting("token")]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            logger.LogInformation("Yeni ürün oluşturma isteği alındı. Ürün Adı: {ProductName}", request.ProductName);
            return CreateActionResult(await productService.CreateProductAsync(request));
        }
        [ServiceFilter(typeof(NotFoundFilter<Product, int>))]
        [HttpPut("{productId}")]
        public async Task<IActionResult> Update(int productId, UpdateProductRequest request)
        {
            logger.LogInformation("Ürün güncelleme isteği alındı. ID: {ProductId}", productId);
            return CreateActionResult(await productService.UpdateProductAsync(productId, request));
        }

        [HttpPatch("stock")]
        [EnableRateLimiting("concurrency")]
        public async Task<IActionResult> UpdateStockPatch(UpdateStockProductRequest request)
        {
            return CreateActionResult(await productService.UpdateStockAsync(request));
        }
        [ServiceFilter(typeof(NotFoundFilter<Product,int>))]
        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> Delete(int productId)
        {
            logger.LogInformation("Ürün silme isteği alındı. ID: {ProductId}", productId);
            return CreateActionResult(await productService.DeleteProductAsync(productId));
        }
    }
}
