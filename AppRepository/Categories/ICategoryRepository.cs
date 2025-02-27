using AppRepository.Repository;

namespace AppRepository.Categories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
         Task<Category> GetCategoryWithProductsAsync(int categoryId);
         IQueryable<Category> GetCategoryByProductsAsync();
    }
}
