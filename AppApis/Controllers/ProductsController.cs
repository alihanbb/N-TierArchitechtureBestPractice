using AppService.Products;
using AppService.Products.Create;
using AppService.Products.Update;
using Microsoft.AspNetCore.Mvc;

namespace AppApis.Controllers
{

    public class ProductsController(IProductService productService) : CustomBaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
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
            return CreateActionResult(await productService.GetProductByIdAsync(productId));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            return CreateActionResult(await productService.CreateProductAsync(request));
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> Update(int productId, UpdateProductRequest request)
        {
            return CreateActionResult(await productService.UpdateProductAsync(productId, request));
        }

        [HttpPatch("stock")]
        public async Task<IActionResult> UpdateStockPatch(UpdateStockProductRequest request)
        {
            return CreateActionResult(await productService.UpdateStockAsync(request));
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> Delete(int productId)
        {
            return CreateActionResult(await productService.DeleteProductAsync(productId));
        }
    }
}
