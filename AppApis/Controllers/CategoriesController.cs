using AppApis.Controllers;
using AppService.Categories;
using AppService.Categories.Create;
using AppService.Categories.Update;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppApi.Controllers
{

    public class CategoriesController(ICategoryService _categoryService) : CustomBaseController
    {

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _categoryService.GetAllListAsync();
            return CreateActionResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            return CreateActionResult(result);
        }

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetCategoryWithProducts(int id)
        {
            var result = await _categoryService.GetCategoryWithProductsAsync(id);
            return CreateActionResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequest categoryRequest)
        {
            var result = await _categoryService.CreateAsync(categoryRequest);
            return CreateActionResult(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryRequest categoryRequest)
        {
            var result = await _categoryService.UpdateAsync(categoryRequest);
            return CreateActionResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return CreateActionResult(result);
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetCategoryByProducts()
        {
            var result = await _categoryService.GetCategoryByProductsAsync();
            return CreateActionResult(result);
        }


    }
}
