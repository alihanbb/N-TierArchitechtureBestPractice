using AppRepository.Context;
using AppRepository.Repository;

namespace AppRepository.Products
{
    public class ProductRepository(AppDbContextcs contextcs) : GenericRepository<Product, int>(contextcs), IProductRepository
    {
        public Task<IEnumerable<object>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Product>> GetTopPriceProductAsync(int count)
        {
           return  Contextcs.Products.OrderByDescending(x => x.Price).Take(count).ToListAsync();
        }
    }
}
