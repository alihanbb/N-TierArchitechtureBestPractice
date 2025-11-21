using AppRepository.Context;
using AppRepository.Repository;

namespace AppRepository.Categories
{
    public class CategoryRepository(AppDbContextcs context) : GenericRepository<Category, int>(context), ICategoryRepository
    {
        public Task<Category> GetCategoryWithProductsAsync(int categoryId)
        {
            return context.Categorys.Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == categoryId);
        }
        public IQueryable<Category> GetCategoryByProductsAsync()
        {
            return context.Categorys.Include(x => x.Products);
        }
    }
}
