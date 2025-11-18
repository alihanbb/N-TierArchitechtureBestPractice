using AppApis.Controllers;
using AppService.Categories;
using AppService.Categories.Create;
using AppService.Categories.Update;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AppApi.Controllers
{
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
        [HttpPut]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryRequest categoryRequest)
        {
            return CreateActionResult(await _categoryService.UpdateAsync(categoryRequest));
        }
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
