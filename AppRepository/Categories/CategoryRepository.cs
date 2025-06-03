using AppRepository.Context;
using AppRepository.Repository;
using Microsoft.EntityFrameworkCore;

namespace AppRepository.Categories
{
    public class CategoryRepository(AppDbContextcs context) : GenericRepository<Category>(context), ICategoryRepository
    {
        public Task<Category> GetCategoryWithProductsAsync(int categoryId)
        {
            return context.Categorys.Include(x => x.Products).FirstOrDefaultAsync(x => x.CategoryId == categoryId);
        }
        public IQueryable<Category> GetCategoryByProductsAsync()
        {
            return context.Categorys.Include(x => x.Products);
        }
    }
}
