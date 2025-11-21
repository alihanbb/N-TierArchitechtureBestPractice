
using AppRepository.Repository;

namespace AppRepository.Products
{
    public interface IProductRepository : IGenericRepository<Product, int>
    {
        Task<IEnumerable<object>> GetAllAsync();
        public Task<List<Product>>  GetTopPriceProductAsync(int count);
    }
}
