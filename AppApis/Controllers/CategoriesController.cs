using AppApis.Controllers;
using AppRepository.Categories;
using AppRepository.Products;
using AppService.Categories;
using AppService.Categories.Create;
using AppService.Categories.Update;
using AppService.Filters;
using Microsoft.AspNetCore.RateLimiting;

namespace AppApi.Controllers
{
    [EnableRateLimiting("perIp")]
    public class CategoriesController(ICategoryService _categoryService) : CustomBaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            return CreateActionResult(await _categoryService.GetAllListAsync());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            return CreateActionResult(await _categoryService.GetByIdAsync(id));
        }

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetCategoryWithProducts(int id)
        {
            return CreateActionResult(await _categoryService.GetCategoryWithProductsAsync(id));
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequest categoryRequest)
        {
            return CreateActionResult(await _categoryService.CreateAsync(categoryRequest));
        }
        [ServiceFilter(typeof(NotFoundFilter<Category, int>))]
        [HttpPut]
        public async Task<IActionResult> UpdateCategory(int id,UpdateCategoryRequest categoryRequest)
        {
            return CreateActionResult(await _categoryService.UpdateAsync(id, categoryRequest));
        }
        [ServiceFilter(typeof(NotFoundFilter<Category, int>))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            return CreateActionResult(await _categoryService.DeleteAsync(id));
        }
        [HttpGet("products")]
        public async Task<IActionResult> GetCategoryByProducts()
        {
            return CreateActionResult(await _categoryService.GetCategoryByProductsAsync());
        }
    }
}
