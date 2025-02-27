using AppRepository.Repository;
using Microsoft.EntityFrameworkCore;

namespace AppRepository.Categories
{
    public class CategoryRepository(Context.AppDbContextcs contextcs) : GenericRepository<Category>(contextcs), ICategoryRepository
    {
        public Task<Category> GetCategoryWithProductsAsync(int categoryId)
        {
            return contextcs.Categorys.Include(x => x.Products).FirstOrDefaultAsync(x => x.CategoryId == categoryId);
        }
        public IQueryable<Category> GetCategoryByProductsAsync()
        {
            return contextcs.Categorys.Include(x => x.Products);
        }
    }
}
