using AppService.Categories.Create;
using AppService.Categories.Dto;
using AppService.Categories.Update;

namespace AppService.Categories
{
    public interface ICategoryService
    {
        Task<ServiceResult<int>> CreateAsync(CreateCategoryRequest categoryRequest);
        Task<ServiceResult> UpdateAsync(UpdateCategoryRequest categoryRequest);
        Task<ServiceResult> DeleteAsync(int categoryId);
        Task<ServiceResult<List<CategoryDto>>> GetAllListAsync();
        Task<ServiceResult<CategoryDto>> GetByIdAsync(int categoryId);
        Task<ServiceResult<CategoryDto>> GetCategoryWithProductsAsync(int categoryId);
        Task<ServiceResult<List<CategoryDto>>> GetCategoryByProductsAsync();


    }
}
