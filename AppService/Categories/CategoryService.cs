using AppRepository.Categories;
using AppRepository.UnitOfWorks;
using AppService.Categories.Create;
using AppService.Categories.Dto;
using AppService.Categories.Update;
using Microsoft.EntityFrameworkCore;

namespace AppService.Categories
{
    public class CategoryService (ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapping) : ICategoryService
    {
        public async Task<ServiceResult<int>> CreateAsync(CreateCategoryRequest categoryRequest)
        {
            var anyCategory = await categoryRepository.where(x => x.CategoryName == categoryRequest.CategoryName).AnyAsync();
            if (anyCategory)
            {
                return ServiceResult<int>.Faild("Category already exists", HttpStatusCode.NotFound);
            }

            var newCategory = mapping.Map<Category>(categoryRequest);   

            await categoryRepository.AddAsync(newCategory);
            await unitOfWork.SaveChangesAsync();
            return ServiceResult<int>.Succes(newCategory.Id);  
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateCategoryRequest categoryRequest)
        {
         
           var isCategoryExist = await categoryRepository.where(x => x.CategoryName == categoryRequest.CategoryName && x.Id !=id).AnyAsync();
            if (isCategoryExist)
            {
                return ServiceResult.Faild("Category already exists", HttpStatusCode.NotFound);
            }
            var anyCategory = mapping.Map<Category>(categoryRequest);
            anyCategory.Id = id;
            categoryRepository.Update(anyCategory);
            await unitOfWork.SaveChangesAsync();
            return ServiceResult.Succes(HttpStatusCode.NoContent);
        }

        public async Task<ServiceResult> DeleteAsync(int categoryId)
        {
            var anyCategory = await categoryRepository.GetByIdAsync(categoryId);
            
            categoryRepository.Delete(anyCategory!);
            await unitOfWork.SaveChangesAsync();
            return ServiceResult.Succes(HttpStatusCode.NoContent);
        }   

        public async  Task<ServiceResult<List<CategoryDto>>> GetAllListAsync()
        {
            var categories = await categoryRepository.GetAll().ToListAsync();
            var categoryDto = mapping.Map<List<CategoryDto>>(categories);
            return ServiceResult<List<CategoryDto>>.Succes(categoryDto);
        }

        public async Task<ServiceResult<CategoryDto>> GetByIdAsync(int categoryId)
        {
            var category = await categoryRepository.GetByIdAsync(categoryId);
            if (category is null)
            {
                return ServiceResult<CategoryDto>.Faild("Category not found", HttpStatusCode.NotFound);
            }
            var categoryDto = mapping.Map<CategoryDto>(category);
            return ServiceResult<CategoryDto>.Succes(categoryDto);
        }   

        public async Task<ServiceResult<CategoryDto>> GetCategoryWithProductsAsync(int categoryId)
        {
            var category = await categoryRepository.GetCategoryWithProductsAsync(categoryId);
            if (category is null)
            {
                return ServiceResult<CategoryDto>.Faild("Category not found", HttpStatusCode.NotFound);
            }
            var categoryDto = mapping.Map<CategoryDto>(category);
            return ServiceResult<CategoryDto>.Succes(categoryDto);
        }

        public async Task<ServiceResult<List<CategoryDto>>> GetCategoryByProductsAsync()
        {
            var categories = await categoryRepository.GetCategoryByProductsAsync().ToListAsync();
            var categoryDto = mapping.Map<List<CategoryDto>>(categories);
            return ServiceResult<List<CategoryDto>>.Succes(categoryDto);
        }
   
    }

}

